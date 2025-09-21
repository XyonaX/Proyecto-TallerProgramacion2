using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Taller_2.Domain.Entities
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string Tipo { get; set; } = "";
        public string NombreCliente { get; set; } = "";
        public string ApellidoCliente { get; set; } = "";
        public string RazonSocial { get; set; } = "";
        public string Direccion { get; set; } = "";
    }
}
