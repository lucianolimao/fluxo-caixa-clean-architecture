namespace FluxoCaixa.Infrastructure.Data;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class FluxoCaixaDbContext : DbContext
{
    public FluxoCaixaDbContext(DbContextOptions<FluxoCaixaDbContext> options) 
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbSet<Lancamento> Lancamentos { get; set; } = null!;
    public DbSet<ConsolidadoDiario> ConsolidadosDiarios { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Lancamento>(entity =>
        {
            entity.ToTable("Lancamentos");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Tipo)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Valor)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(e => e.Descricao)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Data)
                .IsRequired();

            entity.Property(e => e.DataCriacao)
                .IsRequired();
            
            entity.HasIndex(e => e.Data);
            entity.HasIndex(e => new { e.Data, e.Tipo });
        });
        
        modelBuilder.Entity<ConsolidadoDiario>(entity =>
        {
            entity.ToTable("ConsolidadosDiarios");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Data)
                .IsRequired();

            entity.Property(e => e.TotalCreditos)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(e => e.TotalDebitos)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(e => e.SaldoFinal)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(e => e.QuantidadeLancamentos)
                .IsRequired();

            entity.Property(e => e.UltimaAtualizacao)
                .IsRequired();
            
            entity.HasIndex(e => e.Data)
                .IsUnique();
        });
    }
}
