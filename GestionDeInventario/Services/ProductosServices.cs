using GestionDeInventario.DAL;
using GestionDeInventario.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GestionDeInventario.Services;

public class ProductosService(IDbContextFactory<Contexto> DbFactory)
{
    private async Task<bool> Existe(int productoId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Productos.AnyAsync(p => p.ProductoId == productoId);
    }

    private async Task<bool> Insertar(Productos producto)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        contexto.Productos.Add(producto);
        return await contexto.SaveChangesAsync() > 0;
    }

    private async Task<bool> Modificar(Productos producto)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        contexto.Update(producto);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> Guardar(Productos producto)
    {
        if (!await Existe(producto.ProductoId))
        {
            return await Insertar(producto);
        }
        else
        {
            return await Modificar(producto);
        }
    }
    public async Task<Productos?> Buscar(int productoId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Productos
            .FirstOrDefaultAsync(p => p.ProductoId == productoId);
    }
    public async Task<bool> Eliminar(int productoId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        var producto = await contexto.Productos
            .FirstOrDefaultAsync(p => p.ProductoId == productoId);
        if (producto == null) return false;
        var tieneEntradas = await contexto.EntradasDetalle
            .AnyAsync(d => d.ProductoId == productoId);
        if (tieneEntradas) return false;
        contexto.Productos.Remove(producto);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<List<Productos>> Listar(Expression<Func<Productos, bool>> criterio)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Productos
            .Where(criterio)
            .AsNoTracking()
            .ToListAsync();
    }
}
