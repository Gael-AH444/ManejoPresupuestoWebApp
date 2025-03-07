using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManejoPresupuesto.Servicios
{
	public interface IServicioReportes
	{
		Task<IEnumerable<ResultadoObtenerPorSemanaModel>> ObtenerReporteSemanal(int usuarioID, int mes, int año, dynamic ViewBag);
		Task<ReporteTransaccionesDetalladasModel> ObtenerReporteTransaccionesDetalladas(int usuarioID, int mes, int año, dynamic ViewBag);
		Task<ReporteTransaccionesDetalladasModel> ObtenerReporteTransaccionesDetalladasPorCuenta(int usuarioID, int cuentaID, int mes, int año, dynamic ViewBag);
	}


	public class ServicioReportes : IServicioReportes
	{
		private readonly IRepositorioTransacciones repositorioTransacciones;
		private readonly HttpContext httpContext;


		//Constructor
		//Inyectar la interfaz de IRepositorioTransacciones y el servicio de IHttpContextAccessor
		public ServicioReportes(IRepositorioTransacciones repositorioTransacciones, IHttpContextAccessor httpContextAccessor)
		{
			this.repositorioTransacciones = repositorioTransacciones;
			this.httpContext = httpContextAccessor.HttpContext;
		}



		public async Task<IEnumerable<ResultadoObtenerPorSemanaModel>> ObtenerReporteSemanal(int usuarioID, int mes, int año, dynamic ViewBag)
		{
			//Obtener fecha de inicio y fin 
			(DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, año);

			var parametro = new ParametroObtenerTransaccionesPorUsuarioModel()
			{
				UsuarioID = usuarioID,
				FechaFin = fechaFin,
				FechaInicio = fechaInicio
			};

			AsignarValoresAlViewBag(ViewBag, fechaInicio);

			var modelo = await repositorioTransacciones.ObtenerPorSemana(parametro);
			return modelo;

		}


		//Obtener transacciones detalladas
		public async Task<ReporteTransaccionesDetalladasModel> ObtenerReporteTransaccionesDetalladas(int usuarioID, int mes, int año, dynamic ViewBag)
		{
			//Obtener fecha de inicio y fin 
			(DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, año);

			var parametro = new ParametroObtenerTransaccionesPorUsuarioModel()
			{
				UsuarioID = usuarioID,
				FechaFin = fechaFin,
				FechaInicio = fechaInicio
			};

			var transacciones = await repositorioTransacciones.ObtenerPorUsuarioID(parametro);

			var modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);

			AsignarValoresAlViewBag(ViewBag, fechaInicio);

			return modelo;

		}


		//Metodo para obener transacciones detalladas pero por cuenta
		public async Task<ReporteTransaccionesDetalladasModel> ObtenerReporteTransaccionesDetalladasPorCuenta(
			int usuarioID, int cuentaID, int mes, int año, dynamic ViewBag)
		{
			(DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, año);

			var obtenerTransaccionesPorCuenta = new ObtenerTransaccionesPorCuentaModel()
			{
				CuentaID = cuentaID,
				UsuarioID = usuarioID,
				FechaInicio = fechaInicio,
				FechaFin = fechaFin
			};

			var transacciones = await repositorioTransacciones.ObtenerPorCuentaID(obtenerTransaccionesPorCuenta);

			var modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);
			AsignarValoresAlViewBag(ViewBag, fechaInicio);

			return modelo;

		}


		//Asignar valores al view bag
		private void AsignarValoresAlViewBag(dynamic ViewBag, DateTime fechaInicio)
		{
			ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
			ViewBag.añoAnterior = fechaInicio.AddMonths(-1).Year;
			ViewBag.mesPosterior = fechaInicio.AddMonths(1).Month;
			ViewBag.añoPosterior = fechaInicio.AddMonths(1).Year;

			//Trae por defecto la url en la que se encuentra el controlador y el metodo
			ViewBag.urlRetorno = httpContext.Request.Path + httpContext.Request.QueryString;
		}


		//Generar reporte de transacciones detalladas
		private static ReporteTransaccionesDetalladasModel GenerarReporteTransaccionesDetalladas(DateTime fechaInicio, DateTime fechaFin, IEnumerable<TransaccionModel> transacciones)
		{
			var modelo = new ReporteTransaccionesDetalladasModel();

			var transaccionesPorFecha = transacciones.OrderByDescending(x => x.FechaTransaccion)
				.GroupBy(x => x.FechaTransaccion)
				.Select(grupo => new ReporteTransaccionesDetalladasModel.TransaccionesPorFecha()
				{
					FechaTransaccion = grupo.Key,
					Transacciones = grupo.AsEnumerable()
				});

			modelo.TransaccionesAgrupadas = transaccionesPorFecha;
			modelo.FechaInicio = fechaInicio;
			modelo.FechaFin = fechaFin;
			return modelo;
		}


		//Funcion privada para obtener las fechas (Una funcion siempre devuelve un valor, es este caso son 2 valores DateTime)
		private (DateTime fechaInicio, DateTime fechaFin) GenerarFechaInicioYFin(int mes, int año)
		{
			DateTime fechaInicio;
			DateTime fechaFin;

			if (mes <= 0 || mes > 12 || año <= 1900)
			{
				var hoy = DateTime.Today;
				fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
			}
			else
			{
				fechaInicio = new DateTime(año, mes, 1);
			}

			fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

			return (fechaInicio, fechaFin);
		}
	}
}
