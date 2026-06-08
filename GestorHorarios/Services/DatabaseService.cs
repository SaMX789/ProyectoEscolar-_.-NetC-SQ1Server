using System.Configuration;

namespace GestorHorarios.Services
{
    public static class DatabaseService
    {
        public static string GetConnectionString()
        {
            var connString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;

            if (string.IsNullOrEmpty(connString))
                throw new InvalidOperationException(
                    "La cadena de conexión 'DefaultConnection' no está configurada en App.config.");

            return connString;
        }
    }
}
