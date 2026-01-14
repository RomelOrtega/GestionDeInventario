using GestionDeInventario.DAL;
using GestionDeInventario.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GestionDeInventario.Services;

public class EntradasService(IDbContextFactory<Contexto> DbFactory)
{
    private async Task<bool> Existe(int entradaId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Entradas.AnyAsync(e => e.EntradaId == entradaId);
    }

    private async Task<bool> Insertar(Entradas entrada)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        entrada.Total = entrada.EntradasDetalle.Sum(d => d.Cantidad * d.Costo);
        contexto.Entradas.Add(entrada);
        await AfectarExistencia(entrada.EntradasDetalle.ToArray(), TipoOperacion.Suma);
        return await contexto.SaveChangesAsync() > 0;
    }

    private async Task<bool> Modificar(Entradas entrada)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        var anterior = await contexto.Entradas
            .Include(e => e.EntradasDetalle)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EntradaId == entrada.EntradaId);
        if (anterior != null)
        {
            await AfectarExistencia(anterior.EntradasDetalle.ToArray(), TipoOperacion.Resta);
        }
        await AfectarExistencia(entrada.EntradasDetalle.ToArray(), TipoOperacion.Suma);
        var detallesAnteriores = await contexto.EntradasDetalle
            .Where(d => d.EntradaId == entrada.EntradaId)
            .ToListAsync();
        contexto.EntradasDetalle.RemoveRange(detallesAnteriores);

        entrada.Total = entrada.EntradasDetalle.Sum(d => d.Cantidad * d.Costo);
        foreach (var detalle in entrada.EntradasDetalle)
        {
            detalle.DetalleId = 0;
            contexto.EntradasDetalle.Add(detalle);
        }
        contexto.Update(entrada);
        return await contexto.SaveChangesAsync() > 0;
    }
    private async Task AfectarExistencia(EntradasDetalle[] detalles, TipoOperacion tipoOperacion)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        foreach (var item in detalles)
        {
            var producto = await contexto.Productos.SingleAsync(p => p.ProductoId == item.ProductoId);
            if (tipoOperacion == TipoOperacion.Suma)
                producto.Existencia += item.Cantidad;
            else
                producto.Existencia -= item.Cantidad;
        }
        await contexto.SaveChangesAsync();
    }

    public async Task<bool> Guardar(Entradas entrada)
    {
        if (!await Existe(entrada.EntradaId))
        {
            return await Insertar(entrada);
        }
        else
        {
            return await Modificar(entrada);
        }
    }

    public async Task<Entradas?> Buscar(int entradaId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Entradas
            .Include(e => e.EntradasDetalle)
            .FirstOrDefaultAsync(e => e.EntradaId == entradaId);
    }

    public async Task<bool> Eliminar(int entradaId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        var entrada = await contexto.Entradas
            .Include(e => e.EntradasDetalle)
            .FirstOrDefaultAsync(e => e.EntradaId == entradaId);
        if (entrada == null) return false;
        await AfectarExistencia(entrada.EntradasDetalle.ToArray(), TipoOperacion.Resta);
        contexto.EntradasDetalle.RemoveRange(entrada.EntradasDetalle);
        contexto.Entradas.Remove(entrada);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<List<Entradas>> Listar(Expression<Func<Entradas, bool>> criterio)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Entradas
            .Include(e => e.EntradasDetalle)
            .Where(criterio)
            .AsNoTracking()
            .ToListAsync();
    }
}
public enum TipoOperacion
{
    Suma = 1,
    Resta = 2
}