namespace GestorHorarios.Models
{
    public class Salon
    {
        public int IdSalon { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Capacidad { get; set; }
        public int? IdEdificio { get; set; }
        public string NombreEdificio { get; set; } = string.Empty;

        public int? IdCarrera { get; set; }
        public string NombreCarrera { get; set; } = string.Empty;

        public int? IdCarreraSecundaria { get; set; }
        public string NombreCarreraSecundaria { get; set; } = string.Empty;

        public int? IdCarreraTerciaria { get; set; }
        public string NombreCarreraTerciaria { get; set; } = string.Empty;
    }
}
