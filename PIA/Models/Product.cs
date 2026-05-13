using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIA.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Icono { get; set; } = "📦";

        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public string Marca { get; set; } = "";

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public int VecesVendido { get; set; }
        public decimal TotalGenerado { get; set; }

        public bool Activo { get; set; } = true;
    }
}
