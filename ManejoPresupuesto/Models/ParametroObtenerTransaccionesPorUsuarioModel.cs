using System;

namespace ManejoPresupuesto.Models
{
	public class ParametroObtenerTransaccionesPorUsuarioModel
	{
		public int UsuarioID { get; set; }
		public DateTime FechaInicio { get; set; }
		public DateTime FechaFin { get; set; }
	}
}
