using ERP.Domain.Partners.Enums;

namespace ERP.Domain.Tests.Partners
{
    /// <summary>El número de documento: RUC con su dígito verificador (módulo 11) y DNI de 8 dígitos.</summary>
    public class DocumentNumberTests
    {
        [Fact]
        public void Un_RUC_con_su_digito_verificador_correcto_es_valido()
        {
            Assert.Null(IdentityDocumentType.Ruc.DocumentNumberError("20100047218"));
        }

        [Fact]
        public void Un_RUC_con_un_digito_cambiado_se_rechaza()
        {
            Assert.NotNull(IdentityDocumentType.Ruc.DocumentNumberError("20100047219"));
        }

        [Theory]
        [InlineData("2010004721")]
        [InlineData("201000472180")]
        [InlineData("2010004721A")]
        public void Un_RUC_que_no_tiene_11_digitos_se_rechaza(string ruc) =>
            Assert.NotNull(IdentityDocumentType.Ruc.DocumentNumberError(ruc));

        [Fact]
        public void El_DNI_tiene_8_digitos()
        {
            Assert.Null(IdentityDocumentType.Dni.DocumentNumberError("10665965"));
            Assert.NotNull(IdentityDocumentType.Dni.DocumentNumberError("1066596"));
        }
    }
}
