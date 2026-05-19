using Microsoft.EntityFrameworkCore;
using EletroCarAPI.Models;

namespace EletroCarAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }  
         public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Vistoria> Vistorias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>().ToTable("Cliente");
            modelBuilder.Entity<Funcionario>().ToTable("Funcionario");
            modelBuilder.Entity<Veiculo>().ToTable("Veiculo");
            modelBuilder.Entity<Reserva>().ToTable("Reserva");
            modelBuilder.Entity<Vistoria>().ToTable("Vistoria");
        }
    }
}