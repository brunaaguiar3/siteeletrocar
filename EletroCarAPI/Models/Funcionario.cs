using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EletroCarAPI.Models
{
    [Table("Funcionario")]
    public class Funcionario
    {
        [Key]
        [Column("Id_funcionario")]
        public int Id { get; set; }

        [Column("nome")]
        public string? Nome { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("senha")]
        public string? Senha { get; set; }

        [Column("nivel_acesso")]
        public string? NivelAcesso { get; set; }
    }
}