using ERP.Domain.Common;
using ERP.Domain.Users;

namespace ERP.Domain.Tests.Users
{
    /// <summary>La rotación del refresh token y la detección de un token copiado (decisión 24).</summary>
    public class RefreshTokenTests
    {
        private static readonly DateTime Now = new(2026, 10, 3, 15, 0, 0, DateTimeKind.Utc);
        private static readonly string HashA = new('a', RefreshToken.HashLength);
        private static readonly string HashB = new('b', RefreshToken.HashLength);

        [Fact]
        public void Renovar_anula_el_token_y_entrega_el_siguiente_de_la_misma_sesion()
        {
            var first = RefreshToken.Start(Guid.CreateVersion7(), HashA, Now);

            var next = first.Rotate(HashB, Now.AddMinutes(10));

            Assert.True(first.IsRevoked);
            Assert.Equal(first.FamilyId, next.FamilyId);
            Assert.Equal(first.SessionExpiresAt, next.SessionExpiresAt);
        }

        [Fact]
        public void Un_token_reemplazado_dentro_del_margen_no_es_un_robo_y_despues_si()
        {
            var first = RefreshToken.Start(Guid.CreateVersion7(), HashA, Now);
            first.Rotate(HashB, Now);

            Assert.False(first.IsReuse(Now.AddSeconds(10)));
            Assert.True(first.IsReuse(Now.AddMinutes(1)));
        }

        [Fact]
        public void La_sesion_vence_tras_8_horas_sin_usar()
        {
            var token = RefreshToken.Start(Guid.CreateVersion7(), HashA, Now);

            Assert.Null(token.RefreshError(Now.AddHours(7)));
            Assert.Equal(RefreshToken.ExpiredError, token.RefreshError(Now.AddHours(8)));
        }

        [Fact]
        public void La_huella_debe_ser_un_SHA256_en_hexadecimal()
        {
            Assert.Throws<DomainException>(() => RefreshToken.Start(Guid.CreateVersion7(), "no-es-un-hash", Now));
        }
    }
}
