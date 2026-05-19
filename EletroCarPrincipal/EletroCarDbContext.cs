using Microsoft.EntityFrameworkCore;
using EletroCarDB.Models;

namespace EletroCarDB
{
    public class EletroCarContext : DbContext
    {
        public DbSet<ClienteBD> Clientes { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Carro> Carros { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Vistoria> Vistorias { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=EletroCarDB;Trusted_Connection=True;TrustServerCertificate=True");
        }
    }
}