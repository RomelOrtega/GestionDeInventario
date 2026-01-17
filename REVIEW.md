# Revisión de Pares (Peer Review)

## Sistema de Gestión de Inventario
**Asignatura:** Programación Aplicada I  
**Tecnología:** Blazor Server + .NET 10, SQL Server  
**Mecanismo de Seguridad:** ASP.NET Core Identity

---

## Información del Revisor
- **Nombre del Revisor:** [FELIX ALBERTO MUÑOZ]
- **Fecha de Revisión:** [16/01/2026]

---

## Resumen De La Revision 

He revisado el código fuente y probado la funcionalidad del Sistema de Gestión de Inventario. A continuación, presento lo que he encontrado organizado por cada requerimiento de la asignación.

---

## 1. Requerimientos Técnicos

| Requerimiento | Estado | Observaciones |
|---------------|--------|---------------|
| Framework Blazor Server | ✅ Cumple | Implementado con `AddInteractiveServerComponents()` |
| SQL Server + EF Core (Code First) | ✅ Cumple | Migraciones presentes en carpeta `/Migrations` |
| ASP.NET Core Identity | ✅ Cumple | Login y Register funcionales |
| .gitignore adecuado | ✅ Cumple | Excluye bin, obj correctamente |

---

## 2. Módulo de Productos

| Funcionalidad | Estado | Observaciones |
|---------------|--------|---------------|
| Crear productos | ✅ Cumple | Formulario con validaciones |
| Editar productos | ✅ Cumple | Permite modificar todos los campos excepto Existencia |
| Eliminar productos | ✅ Cumple | Validación de entradas asociadas |
| Campo Existencia readonly | ✅ Cumple | Solo se modifica por Entradas |
| Validaciones (obligatorios, > 0) | ✅ Cumple | DataAnnotations implementadas |
| Filtros en Index | ✅ Cumple | Filtro por ProductoId y Descripción |

**Modelo de Productos verificado:**
```csharp
public class Productos
{
    [Key]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El costo es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El costo debe ser mayor a 0")]
    public decimal Costo { get; set; }

    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Precio { get; set; }

    public int Existencia { get; set; } = 0;
}
```

**Protección de eliminación verificada en `ProductosServices.cs` líneas 53-55:**
```csharp
var tieneEntradas = await contexto.EntradasDetalle
    .AnyAsync(d => d.ProductoId == productoId);
if (tieneEntradas) return false;
```

---

## 3. Módulo de Entradas (Maestro-Detalle)

| Funcionalidad | Estado | Observaciones |
|---------------|--------|---------------|
| Entidad Maestra (Entrada) | ✅ Cumple | EntradaId, Fecha, Concepto, Total |
| Entidad Detalle (EntradaDetalle) | ✅ Cumple | DetalleId, EntradaId, ProductoId, Cantidad, Costo |
| Costo readonly en detalle | ✅ Cumple | Se carga automáticamente del producto |
| Total calculado automáticamente | ✅ Cumple | Suma de (Cantidad × Costo) |
| Filtro por rango de fechas | ✅ Cumple | Campos FechaDesde y FechaHasta |
| Filtro por otros criterios | ✅ Cumple | EntradaId y Concepto |

---

## 4. Confirmación de Lógica de Inventario (CRÍTICO)

### 4.1 Al Crear (Guardar) una Entrada
- [x] **VERIFICADO:** Las cantidades ingresadas en el detalle se SUMAN a la Existencia de los productos correspondientes.

**Código verificado en `EntradasServices.cs` línea 21:**
```csharp
await AfectarExistencia(entrada.EntradasDetalle.ToArray(), TipoOperacion.Suma);
```

### 4.2 Al Modificar (Editar) una Entrada
- [x] **VERIFICADO:** El sistema revierte las cantidades originales (resta) y aplica las nuevas (suma).
- [x] **VERIFICADO:** Si se elimina una fila del detalle durante la edición, esa cantidad se resta del inventario.
- [x] **VERIFICADO:** Si se agrega una fila nueva, se suma al inventario.

**Código verificado en `EntradasServices.cs` líneas 34-36:**
```csharp
// Primero resta las cantidades anteriores
await AfectarExistencia(anterior.EntradasDetalle.ToArray(), TipoOperacion.Resta);
// Luego suma las nuevas cantidades
await AfectarExistencia(entrada.EntradasDetalle.ToArray(), TipoOperacion.Suma);
```

### 4.3 Al Eliminar una Entrada
- [x] **VERIFICADO:** Se reversa la operación completa. Las cantidades que entraron se restan del inventario de los productos.

**Código verificado en `EntradasServices.cs` línea 92:**
```csharp
await AfectarExistencia(entrada.EntradasDetalle.ToArray(), TipoOperacion.Resta);
```

**Método AfectarExistencia verificado (líneas 51-63):**
```csharp
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
```

---

## 5. Estructura del Proyecto

```
GestionDeInventario/
├── Components/
│   ├── Account/        
│   ├── Layout/           
│   └── Pages/
│       ├── ProductosPage/
│       │   ├── ProductosIndex.razor
│       │   ├── ProductosCreate.razor
│       │   └── ProductosEdit.razor
│       └── EntradasPage/
│           ├── EntradasIndex.razor
│           ├── EntradasCreate.razor
│           └── EntradasEdit.razor
├── DAL/
│   └── Contexto.cs      
├── Models/
│   ├── Productos.cs
│   ├── Entradas.cs
│   └── EntradasDetalle.cs
├── Services/
│   ├── ProductosServices.cs
│   └── EntradasServices.cs
├── Extensors/
│   └── ToastServiceExtensions.cs
└── Migrations/           
```

---

## 6. Seguridad

| Aspecto | Estado | Observaciones |
|---------|--------|---------------|
| Autenticación | ✅ Cumple | ASP.NET Core Identity |
| Protección de rutas | ✅ Cumple | `[Authorize]` en páginas protegidas |
| Navegación condicional | ✅ Cumple | `<AuthorizeView>` en NavMenu |

---

## 7. Conclusión

**El proyecto CUMPLE con todos los requerimientos establecidos en la asignación.**

La lógica de negocio crítica para el manejo de inventario está correctamente implementada:
- ✅ Crear entrada → Suma existencias
- ✅ Modificar entrada → Revierte y aplica nuevas cantidades
- ✅ Eliminar entrada → Resta existencias

---

## Firma del Revisor

Confirmo que he revisado el código fuente y probado la aplicación, verificando que la lógica funciona correctamente en los tres escenarios requeridos (Crear, Editar, Eliminar).

**Firma:** FELIX ALBERTO MUÑOZ

**Fecha:** 16/08/2026

