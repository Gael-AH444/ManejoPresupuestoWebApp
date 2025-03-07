using System;
using System.Collections.Generic;
using System.Linq;

namespace ManejoPresupuesto.Models
{
	public class ReporteSemanalViewModel
	{
		public decimal Ingreso => TransaccionesPorSemana.Sum(x => x.Ingresos);
		public decimal Gasto => TransaccionesPorSemana.Sum(x => x.Gastos);
		public decimal Total => Ingreso - Gasto;
		public DateTime FechaReferencia { get; set; }
		public IEnumerable<ResultadoObtenerPorSemanaModel> TransaccionesPorSemana { get; set; }
	}
}
