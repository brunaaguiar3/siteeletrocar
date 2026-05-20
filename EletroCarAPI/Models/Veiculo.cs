using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EletroCarAPI.Models
{
    [Table("Veiculo")]
    public class Veiculo
    {
        [Key]
        [Column("Id_veiculo")]
        public int Id { get; set; }

        [Column("modelo")]
        public string? Nome { get; set; }

        [Column("marca")]
        public string? Marca { get; set; }

        [Column("autonomia")]
        public string? Autonomia { get; set; }

        [Column("bateria")]
        public string? Bateria { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("localizacao")]
        public string? Localizacao { get; set; }
    }
}