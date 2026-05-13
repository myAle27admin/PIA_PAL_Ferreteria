using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIA.Models
{
    public class ItemVenta
    {
        public Guid ProductId { get; set; }

        public string Nombre { get; set; } = "";

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
    public class Venta
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public DateTime Fecha { get; set; } = DateTime.Now;

        public List<ItemVenta> Items { get; set; } = new();

        public decimal Total { get; set; }

        public string MetodoPago { get; set; } = "";
    }
}
