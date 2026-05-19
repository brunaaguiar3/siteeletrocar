using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EletroCarAPI.Models
{
    [Table("Vistoria")]
    public class Vistoria
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("ReservaId")]
        public int ReservaId { get; set; }

        [Column("Status")]
        public string Status { get; set; }

        [Column("Checklist")]
        public string Checklist { get; set; }

        [Column("Observacoes")]
        public string Observacoes { get; set; }

        [Column("Imagens")]
        public string? Imagens { get; set; }

        [Column("DataSolicitacao")]
        public DateTime DataSolicitacao { get; set; }

        [Column("DataAprovacao")]
        public DateTime? DataAprovacao { get; set; }

        [Column("MotivoReprovacao")]
        public string MotivoReprovacao { get; set; }

        [Column("FuncionarioId")]
        public int? FuncionarioId { get; set; }

        // Relacionamentos
        [ForeignKey("ReservaId")]
        public Reserva Reserva { get; set; }
    }
}