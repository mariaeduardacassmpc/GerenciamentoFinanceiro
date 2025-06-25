using GerenciamentoFinanceiro.Models;
using Microsoft.EntityFrameworkCore;

namespace GerenciamentoFinanceiro.Data
{
    public class AppDbContext : DbContext 
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Transacao> Transacoes { get; set; }
        public DbSet<Financeiro> Financas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { CategoriaId = "educacao", Nome = "Educacao"},
                new Categoria { CategoriaId = "salario", Nome = "Salario" },
                new Categoria { CategoriaId = "viagem", Nome = "Viagem" },
                new Categoria { CategoriaId = "mercado", Nome = "Mercado" },
                new Categoria { CategoriaId = "comissao", Nome = "Comissao" }
            );

            modelBuilder.Entity<Transacao>().HasData(
                new Transacao { TransacaoId = "ganho", Nome = "Ganho" },
                new Transacao { TransacaoId = "gasto", Nome = "Gasto" }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
