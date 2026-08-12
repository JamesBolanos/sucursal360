using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Domain.Entities;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.Security;

namespace Sucursal360.Web.Data.Seed;

public static class ModelSeedData
{
    private static readonly DateTimeOffset SeededAtUtc = new(2026, 8, 12, 0, 0, 0, TimeSpan.Zero);

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityRole>().HasData(
            CreateRole(SeedIds.AdministratorRoleId, AppRoles.Administrator),
            CreateRole(SeedIds.CorporateManagerRoleId, AppRoles.CorporateManager),
            CreateRole(SeedIds.BranchManagerRoleId, AppRoles.BranchManager));

        modelBuilder.Entity<ReviewCategory>().HasData(
            new ReviewCategory
            {
                Id = SeedIds.CategoryServicio,
                Code = "SERVICIO",
                Name = "Servicio",
                Description = "Atencion, cortesia, conocimiento o actitud del personal",
                IsActive = true
            },
            new ReviewCategory
            {
                Id = SeedIds.CategoryEspera,
                Code = "ESPERA",
                Name = "Tiempo de espera",
                Description = "Filas, demora en ordenar, preparacion o entrega",
                IsActive = true
            },
            new ReviewCategory
            {
                Id = SeedIds.CategoryCalidad,
                Code = "CALIDAD",
                Name = "Calidad del producto",
                Description = "Sabor, temperatura, presentacion o consistencia",
                IsActive = true
            },
            new ReviewCategory
            {
                Id = SeedIds.CategoryLimpieza,
                Code = "LIMPIEZA",
                Name = "Limpieza",
                Description = "Mesas, banos, utensilios o percepcion de higiene",
                IsActive = true
            },
            new ReviewCategory
            {
                Id = SeedIds.CategoryPrecio,
                Code = "PRECIO",
                Name = "Precio",
                Description = "Valor percibido, promociones o relacion precio-calidad",
                IsActive = true
            },
            new ReviewCategory
            {
                Id = SeedIds.CategoryInstalaciones,
                Code = "INSTALACIONES",
                Name = "Instalaciones",
                Description = "Ambiente, espacio, estacionamiento, comodidad o ruido",
                IsActive = true
            },
            new ReviewCategory
            {
                Id = SeedIds.CategoryOtros,
                Code = "OTROS",
                Name = "Otros",
                Description = "Tema relevante que no corresponde a las categorias anteriores",
                IsActive = true
            });

        modelBuilder.Entity<Branch>().HasData(
            CreateBranch(SeedIds.BranchCentro, "SUC-001", "Cafe Horizonte Centro", "DEMO-SUC-001"),
            CreateBranch(SeedIds.BranchCarreteraSur, "SUC-002", "Cafe Horizonte Carretera Sur", "DEMO-SUC-002"),
            CreateBranch(SeedIds.BranchGalerias, "SUC-003", "Cafe Horizonte Galerias", "DEMO-SUC-003"),
            CreateBranch(SeedIds.BranchMetrocentro, "SUC-004", "Cafe Horizonte Metrocentro", "DEMO-SUC-004"),
            CreateBranch(SeedIds.BranchLasColinas, "SUC-005", "Cafe Horizonte Las Colinas", "DEMO-SUC-005"));
    }

    private static IdentityRole CreateRole(string id, string name)
    {
        return new IdentityRole
        {
            Id = id,
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            ConcurrencyStamp = id
        };
    }

    private static Branch CreateBranch(Guid id, string code, string name, string externalPlaceId)
    {
        return new Branch
        {
            Id = id,
            Code = code,
            Name = name,
            IsActive = true,
            Provider = PublicDataProvider.Demo,
            ExternalPlaceId = externalPlaceId,
            CreatedAtUtc = SeededAtUtc,
            UpdatedAtUtc = SeededAtUtc
        };
    }
}
