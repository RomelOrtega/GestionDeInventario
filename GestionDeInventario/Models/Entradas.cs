using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionDeInventario.Models;

public class Entradas
{
    [Key]
    public int EntradaId { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria")]
    public string Concepto { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "El concepto es obligatorio")]
    [StringLength(300, ErrorMessage = "El concepto no puede exceder 300 caracteres")]

    public decimal Total { get; set; }

    [InverseProperty("Entrada")]
    public virtual ICollection<EntradasDetalle> EntradasDetalle { get; set; } = new List<EntradasDetalle>();

}