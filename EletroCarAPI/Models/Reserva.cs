using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EletroCarAPI.Models
{
    [Table("Reserva")]
    public class Reserva
    {
        [Key]
        [Column("id_reserva")]
        public int Id { get; set; }

        [Column("id_cliente")]
        public int ClienteId { get; set; }

        [Column("id_veiculo")]
        public int VeiculoId { get; set; }

        [Column("id_funcionario")]
        public int? FuncionarioId { get; set; }  // pode ser nulo

        [Column("data_retirada")]
        public DateTime DataRetirada { get; set; }

        [Column("data_devolucao")]
        public DateTime DataDevolucao { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("token")]
        public string? Token { get; set; }

        [Column("ValorTotal")]
        public decimal ValorTotal { get; set; }

        // Navegação (opcional)
        public Cliente? Cliente { get; set; }
        public Veiculo? Veiculo { get; set; }
        public Funcionario? Funcionario { get; set; }
    }
}