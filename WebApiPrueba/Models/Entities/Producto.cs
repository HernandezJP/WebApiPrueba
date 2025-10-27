using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiPrueba.Models.Entities
{
    public class Producto
    {
        public string Sku { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public int Existencia { get; set; }
        public decimal Precio { get; set; }
        public decimal Costo { get; set; }
        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
