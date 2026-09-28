using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ERP.Api.OpenApi
{
    /// <summary>
    /// Documenta bien las propiedades calculadas (solo lectura) de las respuestas, como <c>RoleDescription</c>
    /// o <c>PageSizeOptions</c>. El serializador siempre las envía, pero el generador solo marca como obligatorios
    /// los parámetros del constructor y las describe como anulables: el frontend las recibiría como opcionales.
    /// Aquí se marcan como obligatorias y, si el tipo en C# no es anulable, se quita el <c>null</c>.
    /// </summary>
    internal sealed class ComputedPropertiesTransformer : IOpenApiSchemaTransformer
    {
        public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
        {
            if (context.JsonPropertyInfo is not null || context.JsonTypeInfo.Kind != JsonTypeInfoKind.Object || schema.Properties is null)
                return Task.CompletedTask;

            foreach (var property in context.JsonTypeInfo.Properties)
            {
                if (property.Set is not null || property.Get is null || !schema.Properties.TryGetValue(property.Name, out var propertySchema))
                    continue;

                schema.Required ??= new HashSet<string>();
                schema.Required.Add(property.Name);

                if (!property.IsGetNullable && propertySchema is OpenApiSchema { Type: { } type } inline)
                    inline.Type = type & ~JsonSchemaType.Null;
            }

            return Task.CompletedTask;
        }
    }
}
