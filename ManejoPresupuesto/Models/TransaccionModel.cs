using Microsoft.VisualBasic;
using System;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
	public class TransaccionModel
	{
		public int id { get; set; }
		public int UsuarioID { get; set; }
		[Display(Name = "Fecha transacción")]
		[DataType(DataType.Date)]
		public DateTime FechaTransaccion { get; set; } = DateTime.Today;
		public decimal Monto { get; set; }
		[Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una categoría")]
		[Display(Name ="Categoria")]
		public int CategoriaID { get; set; }
		[StringLength(maximumLength:1000, ErrorMessage ="La nota no puede pasar de {1} caracteres")]
		public string Nota { get; set; }
		[Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una cuenta")]
		[Display(Name = "Cuenta")]
		public int CuentaID { get; set; }
		[Display(Name ="Tipo Operación")]
		public TipoOperacion tipoOperacionID { get; set; } = TipoOperacion.Ingreso;

		public string Cuenta { get; set; }
		public string Categoria { get; set; }
	}	
}
