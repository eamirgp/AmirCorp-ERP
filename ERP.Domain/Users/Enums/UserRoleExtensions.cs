namespace ERP.Domain.Users.Enums
{
    public static class UserRoleExtensions
    {
        extension(UserRole userRole)
        {
            public bool CanManage(UserRole target) => Level(userRole) > Level(target);

            public string Description => userRole switch
            {
                UserRole.SuperAdmin => "Super Administrador",
                UserRole.Admin => "Administrador",
                UserRole.Employee => "Empleado",
                _ => userRole.ToString()
            };
        }

        private static int Level(UserRole userRole) => userRole switch
        {
            UserRole.SuperAdmin => 100,
            UserRole.Admin => 50,
            _ => 0
        };
    }
}
