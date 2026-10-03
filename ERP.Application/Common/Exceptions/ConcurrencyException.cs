using ERP.Domain.Catalogs;
using ERP.Domain.Companies;
using ERP.Domain.Partners;
using ERP.Domain.Products;
using ERP.Domain.Purchases;
using ERP.Domain.SavedViews;
using ERP.Domain.Users;

namespace ERP.Application.Common.Exceptions
{
    /// <summary>Otra persona cambió los mismos datos al mismo tiempo. La API responde 409 con el mensaje.</summary>
    public sealed class ConcurrencyException : Exception
    {
        public ConcurrencyException(Exception innerException)
            : base("Los datos fueron modificados por otro usuario. Vuelve a intentarlo.", innerException) { }

        public ConcurrencyException(string message, Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        /// El aviso cuando el formulario envía una versión que ya no es la actual (decisión 21), el mismo en todas las
        /// pantallas. Después del 409 la pantalla vuelve a pedir los datos (decisión 32).
        /// </summary>
        /// <param name="what">Qué se editaba: "este producto", "esta empresa".</param>
        public static string EditedWhileOpenMessage(string what) =>
            $"Otra persona hizo cambios en {what} mientras tenías abierto el formulario. Cierra el formulario y vuelve a abrirlo para ver los datos actuales.";

        /// <summary>
        /// Los casos de uso revisan que un código o documento no se repita, pero si dos personas guardan el mismo a la
        /// vez, lo frena el índice único de la base. El mensaje dice qué se repitió.
        /// </summary>
        /// <param name="entityType">La entidad de la tabla que rechazó el dato, o null si no se sabe.</param>
        public static ConcurrencyException Duplicate(Type? entityType, Exception innerException) =>
            new(DuplicateMessage(entityType), innerException);

        private static string DuplicateMessage(Type? entityType)
        {
            const string Retry = "Revisa los datos y vuelve a intentarlo.";

            if (entityType == typeof(Product))
                return $"Otra persona acaba de guardar un producto con el mismo código interno. {Retry}";
            if (entityType == typeof(ProductSupplierCode))
                return $"Otra persona acaba de enlazar ese código del proveedor a un producto. {Retry}";
            if (entityType == typeof(BusinessPartner))
                return $"Otra persona acaba de registrar un cliente o proveedor con el mismo documento. {Retry}";
            if (entityType == typeof(Company))
                return $"Otra persona acaba de registrar una empresa con el mismo RUC. {Retry}";
            if (entityType == typeof(User))
                return $"Otra persona acaba de registrar un usuario con el mismo correo. {Retry}";
            if (entityType == typeof(Purchase))
                return "Este comprobante se acaba de registrar. Búscalo en la lista de compras.";
            if (entityType == typeof(ExchangeRate))
                return "El tipo de cambio de esa fecha se acaba de guardar. Vuelve a pedirlo.";
            if (entityType == typeof(SavedView))
                return "Ya tienes una vista con ese nombre en esta pantalla.";

            return $"Otra persona acaba de guardar un registro igual. {Retry}";
        }
    }
}
