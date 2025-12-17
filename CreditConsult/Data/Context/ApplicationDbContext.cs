using CreditConsult.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditConsult.Data.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CreditConsult> CreditConsults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CreditConsult>(entity =>
        {
            entity.ToTable("credit_consult");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(e => e.NumeroCredito)
                .HasColumnName("numero_credito")
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.NumeroNfse)
                .HasColumnName("numero_nfse")
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.DataConstituicao)
                .HasColumnName("data_constituicao")
                .IsRequired();

            entity.Property(e => e.ValorIssqn)
                .HasColumnName("valor_issqn")
                .HasColumnType("decimal(15,2)")
                .IsRequired();

            entity.Property(e => e.TipoCredito)
                .HasColumnName("tipo_credito")
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.SimplesNacional)
                .HasColumnName("simples_nacional")
                .IsRequired();

            entity.Property(e => e.Aliquota)
                .HasColumnName("aliquota")
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.ValorFaturado)
                .HasColumnName("valor_faturado")
                .HasColumnType("decimal(15,2)")
                .IsRequired();

            entity.Property(e => e.ValorDeducao)
                .HasColumnName("valor_deducao")
                .HasColumnType("decimal(15,2)")
                .IsRequired();

            entity.Property(e => e.BaseCalculo)
                .HasColumnName("base_calculo")
                .HasColumnType("decimal(15,2)")
                .IsRequired();

            entity.HasIndex(e => e.NumeroCredito);
            entity.HasIndex(e => e.NumeroNfse);
            entity.HasIndex(e => e.TipoCredito);
        });
    }
}

