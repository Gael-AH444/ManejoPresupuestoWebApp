using System;

namespace ManejoPresupuesto.Models
{
	public class ResultadoObtenerPorSemanaModel
	{
		public int Semana { get; set; }
		public decimal Monto { get; set; }
		public TipoOperacion TipoOperacionID { get; set; }
		public decimal Ingresos { get; set; }
		public decimal Gastos { get; set; }
		public DateTime FechaInicio { get; set; }
		public DateTime FechaFin { get; set; }
	}
}
