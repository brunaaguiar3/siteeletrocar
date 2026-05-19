using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EletroCarAPI.Models
{
    [Table("Cliente")]
    public class Cliente
    {
        [Key]
        [Column("id_cliente")]
        public int Id { get; set; }

        [Column("nome")]
        public string? Nome { get; set; }

        [Column("cpf")]
        public string? CPF { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("telefone")]
        public string? Telefone { get; set; }

        [Column("cnh")]
        public string? CNH { get; set; }

        [Column("senha")]
        public string? Senha { get; set; }
    }
}