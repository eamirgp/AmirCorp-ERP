using ERP.Domain.Common;
using ERP.Domain.Users;
using ERP.Domain.Users.Enums;

namespace ERP.Domain.Tests.Users
{
    /// <summary>Quién puede gestionar a quién, el correo y el cierre de sesiones al desactivar o cambiar la contraseña.</summary>
    public class UserTests
    {
        private static readonly DateTime Now = new(2026, 10, 3, 15, 0, 0, DateTimeKind.Utc);
        private static readonly string Hash = new('a', RefreshToken.HashLength);

        private static User Employee() =>
            User.Create("Ana Pérez", "ana@pizarro.pe", "clave-segura-1", p => "hash:" + p, UserRole.Employee, UserRole.Admin);

        [Fact]
        public void Un_administrador_gestiona_empleados_pero_no_a_otro_administrador()
        {
            Assert.Null(User.AssignRoleError(UserRole.Admin, UserRole.Employee));
            Assert.NotNull(User.AssignRoleError(UserRole.Admin, UserRole.Admin));
            Assert.NotNull(User.AssignRoleError(UserRole.Admin, UserRole.SuperAdmin));
        }

        [Fact]
        public void Un_empleado_no_gestiona_a_nadie()
        {
            Assert.NotNull(Employee().ManageError(UserRole.Employee));
        }

        [Fact]
        public void Un_correo_con_nombre_delante_se_rechaza()
        {
            Assert.NotNull(User.EmailError("Juan <juan@pizarro.pe>"));
            Assert.Null(User.EmailError("juan@pizarro.pe"));
        }

        [Fact]
        public void Desactivar_cierra_todas_sus_sesiones()
        {
            var user = Employee();
            var sessions = new[] { RefreshToken.Start(user.Id, Hash, Now), RefreshToken.Start(user.Id, Hash, Now) };

            user.Deactivate(UserRole.Admin, sessions, Now);

            Assert.False(user.IsActive);
            Assert.All(sessions, s => Assert.True(s.IsRevoked));
        }

        [Fact]
        public void Restablecer_la_contrasena_cierra_todas_sus_sesiones()
        {
            var user = Employee();
            var session = RefreshToken.Start(user.Id, Hash, Now);

            user.ResetPassword("otra-clave-segura", p => "hash:" + p, UserRole.Admin, [session], Now);

            Assert.True(session.IsRevoked);
        }

        [Fact]
        public void No_se_cierran_sesiones_de_otro_usuario()
        {
            var user = Employee();
            var other = RefreshToken.Start(Guid.CreateVersion7(), Hash, Now);

            Assert.Throws<DomainException>(() => user.Deactivate(UserRole.Admin, [other], Now));
        }
    }
}
