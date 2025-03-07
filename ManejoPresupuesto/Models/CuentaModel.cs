using ManejoPresupuesto.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
	public class CuentaModel
	{
        public int id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength:50)]
        [PrimeraLetraMayuscua]
        public string Nombre { get; set; }
        [Display(Name ="Tipo Cuenta")]
        public int TipoCuentaID { get; set; }
        public decimal Balance { get; set; }
        [StringLength(maximumLength:1000)]
        public string Descripcion { get; set; }
        public string TipoCuenta { get; set; }
    }
}
