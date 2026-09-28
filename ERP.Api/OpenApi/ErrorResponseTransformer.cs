using ERP.Api.Common;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ERP.Api.OpenApi
{
    /// <summary>
    /// Documenta en cada endpoint que cualquier respuesta no exitosa tiene la forma <see cref="ErrorResponse"/>.
    /// </summary>
    internal sealed class ErrorResponseTransformer : IOpenApiOperationTransformer
    {
        private const string SchemaName = nameof(ErrorResponse);

        public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            var document = context.Document!;
            var schema = await context.GetOrCreateSchemaAsync(typeof(ErrorResponse), null, cancellationToken);
            document.AddComponent(SchemaName, schema);

            operation.Responses ??= [];
            operation.Responses["default"] = new OpenApiResponse
            {
                Description = "Error. El cuerpo trae la lista de mensajes para el usuario.",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchemaReference(SchemaName, document) }
                }
            };
        }
    }
}
