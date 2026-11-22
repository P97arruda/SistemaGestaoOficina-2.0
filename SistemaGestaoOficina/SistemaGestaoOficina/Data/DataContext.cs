
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<Veiculo> Veiculos { get; set; }

        public DbSet<Mecanico> Mecanicos { get; set; }

        public DbSet<Marcacao> Marcacoes { get; set; }

        public DbSet<Reparacao> Reparacoes { get; set; }

        public DbSet<TipoReparacao> TipoReparacoes { get; set; }

        public DbSet<MecanicoReparacao> MecanicoReparacoes { get; set; }

        public DbSet<ReparacaoTipoReparacao> ReparacaoTipoReparacoes { get; set; }

        public DbSet<Peca> Pecas { get; set; }

        public DbSet<ReparacaoPeca> ReparacaoPecas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CUSTOS DAS REPARAÇÕES
            modelBuilder.Entity<Reparacao>()
                .Property(r => r.CustoTotal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Reparacao>()
                .Property(r => r.CustoMaoDeObra)
                .HasColumnType("decimal(18,2)");

            // UTILIZADORES
            modelBuilder.Entity<User>()
                .HasIndex(u => u.NIF)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            // MARCAÇÃO - VEÍCULO
            modelBuilder.Entity<Marcacao>()
                .HasOne(m => m.Veiculo)
                .WithMany(v => v.Marcacaos)
                .OnDelete(DeleteBehavior.Restrict);

            // MECÂNICO - UTILIZADOR (IDENTITY)
            modelBuilder.Entity<Mecanico>()
                .HasOne(m => m.User)
                .WithOne()
                .HasForeignKey<Mecanico>(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // MARCAÇÃO - UTILIZADOR
            modelBuilder.Entity<Marcacao>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // VEÍCULO - UTILIZADOR
            modelBuilder.Entity<Veiculo>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // MECÂNICO - REPARAÇÃO
            modelBuilder.Entity<MecanicoReparacao>()
                .HasOne(mr => mr.Reparacao)
                .WithMany(r => r.MecanicoReparacaos)
                .OnDelete(DeleteBehavior.Restrict);

            // REPARAÇÃO - TIPO DE REPARAÇÃO
            modelBuilder.Entity<ReparacaoTipoReparacao>()
                .HasOne(rtr => rtr.Reparacao)
                .WithMany(r => r.ReparacaoTipoReparacoes)
                .HasForeignKey(rtr => rtr.ReparacaoId)
                .OnDelete(DeleteBehavior.Restrict);

            // TIPO DE REPARAÇÃO - ASSOCIAÇÃO
            modelBuilder.Entity<ReparacaoTipoReparacao>()
                .HasOne(rtr => rtr.TipoReparacao)
                .WithMany(tr => tr.ReparacaoTipoReparacoes)
                .HasForeignKey(rtr => rtr.TipoReparacaoId)
                .OnDelete(DeleteBehavior.Restrict);

            // REPARAÇÃO - PEÇAS
            modelBuilder.Entity<ReparacaoPeca>()
                .HasOne(rp => rp.Reparacao)
                .WithMany(r => r.ReparacaoPecas)
                .HasForeignKey(rp => rp.ReparacaoId)
                .OnDelete(DeleteBehavior.Restrict);

            // PEÇA - ASSOCIAÇÃO
            modelBuilder.Entity<ReparacaoPeca>()
                .HasOne(rp => rp.Peca)
                .WithMany(p => p.ReparacaoPecas)
                .HasForeignKey(rp => rp.PecaId)
                .OnDelete(DeleteBehavior.Restrict);

            // ÍNDICE ÚNICO DAS PEÇAS
            modelBuilder.Entity<ReparacaoPeca>()
                .HasIndex(rp => new
                {
                    rp.ReparacaoId,
                    rp.PecaId
                })
                .IsUnique();

            // PREÇO UNITÁRIO
            modelBuilder.Entity<ReparacaoPeca>()
                .Property(rp => rp.PrecoUnitario)
                .HasColumnType("decimal(18,2)");

            // PREÇO DAS PEÇAS
            modelBuilder.Entity<Peca>()
                .Property(p => p.Preco)
                .HasColumnType("decimal(18,2)");

            // ÍNDICE ÚNICO DOS TIPOS DE REPARAÇÃO
            modelBuilder.Entity<ReparacaoTipoReparacao>()
                .HasIndex(rtr => new
                {
                    rtr.ReparacaoId,
                    rtr.TipoReparacaoId
                })
                .IsUnique();
        }
    }
}
