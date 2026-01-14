using GestionDeInventario.Data;
using GestionDeInventario.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestionDeInventario.DAL
{
    public class Contexto : IdentityDbContext<ApplicationUser>
    {
        public Contexto(DbContextOptions<Contexto> options) : base(options) { }

        public DbSet<Entradas> Entradas { get; set; }
        public DbSet<Productos> Productos { get; set; }
        public DbSet<EntradasDetalle> EntradasDetalle { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Productos>().HasData(
                new List<Productos>()
                {
                new()
                {
                    ProductoId = 1,
                    Descripcion = "Cinta métrica 5 metros",
                    Costo = 4.00m,
                    Precio = 7.00m,
                    Existencia = 0
                },
                new()
                {
                    ProductoId = 2,
                    Descripcion = "Taladro eléctrico 1/2 pulgada",
                    Costo = 45.00m,
                    Precio = 70.00m,
                    Existencia = 0
                },
                new()
                {
                    ProductoId = 3,
                    Descripcion = "Martillo de acero 16 oz",
                    Costo = 6.50m,
                    Precio = 10.00m,
                    Existencia = 0
                },
                }
            );
        }
    }
}