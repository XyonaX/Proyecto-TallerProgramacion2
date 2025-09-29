using System;
using System.Windows.Forms;
using BCrypt.Net;
using Proyecto_Taller_2.Domain.Models;

public class Usuario
{
    public int IdUsuario { get; set; }
    public int Dni { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Email { get; set; } = "";
    public string Telefono { get; set; } // No nullable reference types en C# 7.3
    public string Password { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public bool Estado { get; set; }
    public int IdRol { get; set; }
    public string RolNombre { get; set; } = "";
    public string NombreCompleto
    {
        get
        {
            var ap = Apellido ?? "";
            var nom = Nombre ?? "";
            return (ap + ", " + nom).Trim(',', ' ');
        }
    }
}


