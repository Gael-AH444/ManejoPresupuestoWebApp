using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace ManejoPresupuesto.Models
{
	public class ReporteMensualViewModel
	{
		public IEnumerable<ResultadoObtenerPorMesModel> TransaccionesPorMes { get; set; }
		public decimal Ingresos => TransaccionesPorMes.Sum(x => x.Ingreso);
		public decimal Gastos => TransaccionesPorMes.Sum(x => x.Gasto);
		public decimal Total => Ingresos - Gastos;
		public int Año  { get; set; }
	}
}
