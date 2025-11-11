namespace Proyecto_Taller_2.Domain.Entities
{
    public class ClienteEmail
    {
        // Clave Foránea (parte de la clave primaria compuesta)
        public int IdCliente { get; set; }

        public string Email { get; set; } // (Parte de la clave primaria compuesta)
        public bool EsPrincipal { get; set; }

        // Propiedad de navegación de vuelta al Cliente
        public virtual Cliente Cliente { get; set; }
    }
}