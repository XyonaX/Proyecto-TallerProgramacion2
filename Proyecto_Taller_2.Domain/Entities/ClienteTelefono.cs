namespace Proyecto_Taller_2.Domain.Entities
{
    public class ClienteTelefono
    {
        // Clave Foránea (parte de la clave primaria compuesta)
        public int IdCliente { get; set; }

        public string Telefono { get; set; } // (Parte de la clave primaria compuesta)
        public string Tipo { get; set; } // "Fijo", "Móvil", "Trabajo"
        public bool EsPrincipal { get; set; }

        // Propiedad de navegación de vuelta al Cliente
        public virtual Cliente Cliente { get; set; }
    }
}