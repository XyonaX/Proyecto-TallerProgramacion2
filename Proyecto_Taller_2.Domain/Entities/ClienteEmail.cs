using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Taller_2.Domain.Entities
{
    public class ClienteEmail
    {
        public int IdCliente { get; set; }
        public string Email { get; set; } = "";
        public bool EsPrincipal { get; set; }
    }
}
