using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestiónDeInventario.Models;

public class EntradasDetalle
{
    [Key]
    public int EntradaId { get; set; }

    public int DetalleId { get; set; }

    public int ProductoId { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Cantidad { get; set; }

    [Required(ErrorMessage = "El costo es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El costo debe ser mayor a 0")]
    public decimal Costo { get; set; }

    [ForeignKey("EntradaId")]
    [InverseProperty("EntradasDetalle")]
    public virtual Entradas Entrada { get; set; } = null!;

    [ForeignKey("ProductoId")]
    public virtual Productos Producto { get; set; } = null!;

}