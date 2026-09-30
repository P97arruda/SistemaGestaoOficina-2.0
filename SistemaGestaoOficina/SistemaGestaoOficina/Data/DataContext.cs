using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Veiculo> Veiculos { get; set; }

        public DbSet<Mecanico> Mecanicos { get; set; }

        public DbSet<Marcacao> Marcacoes { get; set; }

        public DbSet<Reparacao> Reparacoes { get; set; }

        public DbSet<MecanicoReparacao> MecanicoReparacoes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reparacao>()
                .Property(r => r.CustoTotal)
                .HasColumnType("decimal(18,2)");


            modelBuilder.Entity<User>()
                .HasIndex(u => u.NIF)
                .IsUnique();


            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();


            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();


            modelBuilder.Entity<Marcacao>()
                .HasOne(m => m.Veiculo)
                .WithMany(v => v.Marcacaos)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<MecanicoReparacao>()
                .HasOne(mr => mr.Reparacao)
                .WithMany(r => r.MecanicoReparacaos)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Veiculo>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Marcacao>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            base.OnModelCreating(modelBuilder);
        }
    }
}