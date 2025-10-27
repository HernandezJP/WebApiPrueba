using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiPrueba.Models.Entities
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string Nit { get; set; } = null!;
        public string Nombre { get; set; } = null!;
    }
}
