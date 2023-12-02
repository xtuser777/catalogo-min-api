using CatalogoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Context;

public class CatalogoApiContext : DbContext
{
    public CatalogoApiContext(DbContextOptions<CatalogoApiContext> options) : base(options)
    {}

    public DbSet<Categoria>? Categorias { get; set; }
    public DbSet<Produto>? Produtos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>().HasKey(c => c.Id);
        modelBuilder.Entity<Categoria>().Property(c => c.Nome)
                                        .HasMaxLength(100)
                                        .IsRequired();
        modelBuilder.Entity<Categoria>().Property(c => c.Descricao)
                                        .HasMaxLength(150)
                                        .IsRequired();

        modelBuilder.Entity<Produto>().HasKey(c => c.Id);
        modelBuilder.Entity<Produto>().Property(c => c.Nome)
                                      .HasMaxLength(100)
                                      .IsRequired();
        modelBuilder.Entity<Produto>().Property(c => c.Descricao).HasMaxLength(150);
        modelBuilder.Entity<Produto>().Property(c => c.Imagem).HasMaxLength(100);
        modelBuilder.Entity<Produto>().Property(c => c.Preco).HasPrecision(14, 2);

        modelBuilder.Entity<Produto>()
            .HasOne<Categoria>(c => c.Categoria)
            .WithMany(p => p.Produtos)
            .HasForeignKey(c => c.CategoriaId);
        //base.OnModelCreating(modelBuilder);
    }
}