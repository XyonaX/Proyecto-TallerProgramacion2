using System;
using System.Collections.Generic;

namespace Proyecto_Taller_2.Domain.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string Tipo { get; set; } = ""; // 'PF' = Persona Física, 'PJ' = Persona Jurídica
        public string NombreCliente { get; set; } = "";
        public string ApellidoCliente { get; set; } = "";
        public string RazonSocial { get; set; } = ""; // Para personas jurídicas
        public string Direccion { get; set; } = "";
        public string CUIT { get; set; } = "";
        public string CUIL { get; set; } = "";
        public DateTime FechaAlta { get; set; }
        public bool Activo { get; set; }

        // Propiedades adicionales para facilitar el trabajo con la UI
        public string EmailPrincipal { get; set; } = "";
        public string TelefonoPrincipal { get; set; } = "";
        
        // Lista de emails y teléfonos (para casos donde se necesiten todos)
        public List<ClienteEmail> Emails { get; set; } = new List<ClienteEmail>();
        public List<ClienteTelefono> Telefonos { get; set; } = new List<ClienteTelefono>();

        // Propiedades calculadas para facilitar la visualización
        public string NombreCompleto 
        { 
            get 
            {
                if (Tipo == "PF")
                    return $"{NombreCliente} {ApellidoCliente}".Trim();
                else
                    return RazonSocial;
            } 
        }

        public string DocumentoIdentidad 
        { 
            get 
            {
                return Tipo == "PF" ? CUIL : CUIT;
            } 
        }

        public string TipoDescripcion 
        { 
            get 
            {
                return Tipo == "PF" ? "Persona Física" : "Persona Jurídica";
            } 
        }
    }

    public class ClienteEmail
    {
        public int IdCliente { get; set; }
        public string Email { get; set; } = "";
        public bool EsPrincipal { get; set; }
    }

    public class ClienteTelefono
    {
        public int IdCliente { get; set; }
        public string Telefono { get; set; } = "";
        public string Tipo { get; set; } = ""; // 'Móvil', 'Fijo', 'Trabajo', etc.
        public bool EsPrincipal { get; set; }
    }
}