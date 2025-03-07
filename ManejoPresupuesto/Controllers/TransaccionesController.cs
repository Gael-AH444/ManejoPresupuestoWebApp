using AutoMapper;
using ClosedXML.Excel;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ManejoPresupuesto.Controllers
{
	public class TransaccionesController : Controller
	{
		private readonly IServicioUsuarios servicioUsuarios;
		private readonly IRepositorioCuentas repositorioCuentas;
		private readonly IRespositorioCategorias respositorioCategorias;
		private readonly IRepositorioTransacciones repositorioTransacciones;
		private readonly IMapper mapper;
		private readonly IServicioReportes servicioReportes;

		public TransaccionesController(IServicioUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas,
			IRespositorioCategorias respositorioCategorias, IRepositorioTransacciones repositorioTransacciones,
			IMapper mapper, IServicioReportes servicioReportes)
		{
			this.servicioUsuarios = servicioUsuarios;
			this.repositorioCuentas = repositorioCuentas;
			this.respositorioCategorias = respositorioCategorias;
			this.repositorioTransacciones = repositorioTransacciones;
			this.mapper = mapper;
			this.servicioReportes = servicioReportes;
		}



		/*** INDEX ***/
		[HttpGet]
		public async Task<IActionResult> Index(int mes, int año)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();

			var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladas(usuarioID, mes, año, ViewBag);

			return View(modelo);
		}



		/*** REPORTE SEMANAL  ***/
		[HttpGet]
		public async Task<IActionResult> Semanal(int mes, int año)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();

			IEnumerable<ResultadoObtenerPorSemanaModel> transaccionesPorSemana = await
				servicioReportes.ObtenerReporteSemanal(usuarioID, mes, año, ViewBag);

			//Agrupando transacciones por semana, si son ingresos o gastos
			var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana).Select(x => new ResultadoObtenerPorSemanaModel
			{
				Semana = x.Key,
				Ingresos = x.Where(x => x.TipoOperacionID == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
				Gastos = x.Where(x => x.TipoOperacionID == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault()

			}).ToList();


			//Algoritmo para obtener la fecha de inicio y fin de la transaccion por semana
			if (año == 0 || mes == 0)
			{
				var hoy = DateTime.Today;
				año = hoy.Year;
				mes = hoy.Month;
			}

			var fechaReferencia = new DateTime(año, mes, 1);
			var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);

			var diasSegmentados = diasDelMes.Chunk(7).ToList();

			for (var i = 0; i < diasSegmentados.Count; i++)
			{
				var semana = i + 1;
				var fechaInicio = new DateTime(año, mes, diasSegmentados[i].First());
				var fechaFin = new DateTime(año, mes, diasSegmentados[i].Last());
				var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);

				if (grupoSemana is null)
				{
					agrupado.Add(new ResultadoObtenerPorSemanaModel()
					{
						Semana = semana,
						FechaInicio = fechaInicio,
						FechaFin = fechaFin

					});
				}
				else
				{
					grupoSemana.FechaInicio = fechaInicio;
					grupoSemana.FechaFin = fechaFin;
				}
			}

			agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();

			var modelo = new ReporteSemanalViewModel();
			modelo.TransaccionesPorSemana = agrupado;
			modelo.FechaReferencia = fechaReferencia;

			return View(modelo);
		}



		/*** REPORTE MENSUAL  ***/
		[HttpGet]
		public async Task<IActionResult> Mensual(int año)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();

			if (año == 0)
			{
				año = DateTime.Today.Year;
			}

			var transaccionesPorMes = await repositorioTransacciones.ObtenerPorMes(usuarioID, año);

			var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes)
				.Select(x => new ResultadoObtenerPorMesModel()
				{
					Mes = x.Key,
					Ingreso = x.Where(x => x.TipoOperacionID == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
					Gasto = x.Where(x => x.TipoOperacionID == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault()

				}).ToList();

			for (int mes = 1; mes <= 12; mes++)
			{
				var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
				var fechaReferencia = new DateTime(año, mes, 1);
				if (transaccion is null)
				{
					transaccionesAgrupadas.Add(new ResultadoObtenerPorMesModel()
					{
						Mes = mes,
						FechaReferencia = fechaReferencia
					});
				}
				else
				{
					transaccion.FechaReferencia = fechaReferencia;
				}
			}

			transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();

			var modelo = new ReporteMensualViewModel();
			modelo.Año = año;
			modelo.TransaccionesPorMes = transaccionesAgrupadas;

			return View(modelo);
		}



		/*** REPORTE EXEL  ***/
		[HttpGet]
		public async Task<IActionResult> ExcelReporte()
		{

			return View();
		}

		//* Exportar el excel por mes *
		[HttpGet]
		public async Task<FileResult> ExportarExcelPorMes(int mes, int año)
		{
			var fechaInicio = new DateTime(año, mes, 1);
			var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();

			var transacciones = await repositorioTransacciones.ObtenerPorUsuarioID(new ParametroObtenerTransaccionesPorUsuarioModel
			{
				UsuarioID = usuarioID,
				FechaInicio = fechaInicio,
				FechaFin = fechaFin
			});

			var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("MMM yyy")}.xlsx";

			return GenerarExcel(nombreArchivo, transacciones);
		}

		// * Genenrar archivo de excel con la libreria ClosedXML *
		private FileResult GenerarExcel(string nombreArchivo, IEnumerable<TransaccionModel> transacciones)
		{
			//Crear columnas
			DataTable dataTable = new DataTable("Transacciones");
			dataTable.Columns.AddRange(new DataColumn[]
			{
				new DataColumn("Fecha"),
				new DataColumn("Cuenta"),
				new DataColumn("Categoria"),
				new DataColumn("Nota"),
				new DataColumn("Monto"),
				new DataColumn("Ingreso/Gasto")
			});

			//Crear filas
			foreach (var tran in transacciones)
			{
				dataTable.Rows.Add(tran.FechaTransaccion.ToString("d"),
								   tran.Cuenta,
								   tran.Categoria,
								   tran.Nota,
								   tran.Monto,
								   tran.tipoOperacionID);
			}

			//Generar exel
			using (XLWorkbook wb = new XLWorkbook())
			{
				wb.Worksheets.Add(dataTable);

				//Guardar el excel en memoria para despues devolver el archivo al usuario
				using (MemoryStream stream = new MemoryStream())
				{
					wb.SaveAs(stream);
					return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
						nombreArchivo);
				}
			}
		}

		//* Exportar excel por año *
		[HttpGet]
		public async Task<FileResult> ExportarExcelPorAño(int año)
		{
			var fechaInicio = new DateTime(año, 1, 1);
			var fechaFin = fechaInicio.AddYears(1).AddDays(-1);
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();

			var transacciones = await repositorioTransacciones.ObtenerPorUsuarioID(new ParametroObtenerTransaccionesPorUsuarioModel
			{
				UsuarioID = usuarioID,
				FechaInicio = fechaInicio,
				FechaFin = fechaFin
			});

			var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("yyyy")}.xlsx";
			return GenerarExcel(nombreArchivo, transacciones);
		}

		//* Exportar todo *
		[HttpGet]
		public async Task<FileResult> ExportarExcelTodo()
		{
			var fechaInicio = DateTime.Today.AddYears(-100);
			var fechaFin = DateTime.Today.AddYears(1000);
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();

			var transacciones = await repositorioTransacciones.ObtenerPorUsuarioID(new ParametroObtenerTransaccionesPorUsuarioModel
			{
				UsuarioID = usuarioID,
				FechaInicio = fechaInicio,
				FechaFin = fechaFin
			});

			var nombreArchivo = $"Manejo Presupuestos - {DateTime.Today.ToString("dd-MM-yyyy")}.xlsx";
			return GenerarExcel(nombreArchivo, transacciones);
		}



		/*** REPORTE CALENDARIO  ***/
		[HttpGet]
		public IActionResult Calendario()
		{
			return View();
		}

		// * Metodo para obtener las transacciones y pasarlas al calendario *
		[HttpGet]
		public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start, DateTime end)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();

			var transacciones = await repositorioTransacciones.ObtenerPorUsuarioID(new ParametroObtenerTransaccionesPorUsuarioModel
			{
				UsuarioID = usuarioID,
				FechaInicio = start,
				FechaFin = end
			});

			var eventosCalendario = transacciones.Select(t => new EventoCalendarioModel
			{
				Title = t.Monto.ToString("N"), /*Al usar 'N' dentro del string, formatea el numero con separadores de miles y decimales*/
				Start = t.FechaTransaccion.ToString("yyyy-MM-dd"),
				End = t.FechaTransaccion.ToString("yyyy-MM-dd"),
				Color = (t.tipoOperacionID == TipoOperacion.Gasto) ? "Red" : null
			});

			return Json(eventosCalendario);
		}

		// * Metodo para obtener transacciones por fecha y mostrarlas al hacer click en el calendario * 
		[HttpGet]
		public async Task<JsonResult> ObtenerTransaccionesPorFecha(DateTime fecha)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();

			var transacciones = await repositorioTransacciones.ObtenerPorUsuarioID(new ParametroObtenerTransaccionesPorUsuarioModel
			{
				UsuarioID = usuarioID,
				FechaInicio = fecha,
				FechaFin = fecha
			});

			return Json(transacciones);
		}



		/**** CREAR ****/
		[HttpGet]
		public async Task<IActionResult> Crear()
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var modelo = new TransaccionCreacionViewModel();
			modelo.Cuentas = await ObtenerCuentas(usuarioID);
			modelo.Categorias = await ObtenerCategorias(usuarioID, modelo.tipoOperacionID);
			return View(modelo);
		}

		[HttpPost]
		public async Task<IActionResult> Crear(TransaccionCreacionViewModel modelo)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();

			if (modelo.CategoriaID == 0 && modelo.Monto == 0)
			{
				modelo.Cuentas = await ObtenerCuentas(usuarioID);
				modelo.Categorias = await ObtenerCategorias(usuarioID, modelo.tipoOperacionID);
				return View(modelo);
			}

			var cuenta = await repositorioCuentas.ObtenerPorID(modelo.CuentaID, usuarioID);
			if (cuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			var categoria = await respositorioCategorias.ObtenerPorID(modelo.CategoriaID, usuarioID);
			if (categoria is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			modelo.UsuarioID = usuarioID;
			if (modelo.tipoOperacionID == TipoOperacion.Gasto)
			{
				modelo.Monto *= -1;
			}

			await repositorioTransacciones.Crear(modelo);
			return RedirectToAction("Index");
		}



		/**** ACTUALIZAR ****/
		[HttpGet]
		public async Task<IActionResult> Editar(int id, string urlRetorno = null)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var transaccion = await repositorioTransacciones.ObtenerPorID(id, usuarioID);

			if (transaccion is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			var modelo = mapper.Map<TransaccionActualizacionVM>(transaccion);

			modelo.MontoAnterior = modelo.Monto;

			if (modelo.tipoOperacionID == TipoOperacion.Gasto)
			{
				modelo.MontoAnterior = modelo.Monto * -1;
			}

			modelo.CuentaAnteriorID = transaccion.CuentaID;
			modelo.Categorias = await ObtenerCategorias(usuarioID, transaccion.tipoOperacionID);
			modelo.Cuentas = await ObtenerCuentas(usuarioID);
			modelo.UrlRetorno = urlRetorno;

			return View(modelo);
		}

		[HttpPost]
		public async Task<IActionResult> Editar(TransaccionActualizacionVM modelo)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();

			if (modelo.CategoriaID == 0 && modelo.Monto == 0)
			{
				modelo.Cuentas = await ObtenerCuentas(usuarioID);
				modelo.Categorias = await ObtenerCategorias(usuarioID, modelo.tipoOperacionID);
				return View(modelo);
			}

			var cuenta = await repositorioCuentas.ObtenerPorID(modelo.CuentaID, usuarioID);
			if (cuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			var categoria = await respositorioCategorias.ObtenerPorID(modelo.CategoriaID, usuarioID);
			if (categoria is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			var transaccion = mapper.Map<TransaccionModel>(modelo);
			if (modelo.tipoOperacionID == TipoOperacion.Gasto)
			{
				transaccion.Monto *= -1;
			}

			await repositorioTransacciones.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorID);

			if (string.IsNullOrEmpty(modelo.UrlRetorno))
			{
				return RedirectToAction("Index");

			}
			else
			{
				//Redirigir a una url especifica dentro del proyecto
				return LocalRedirect(modelo.UrlRetorno);
			}
		}



		/*** BORRAR ***/
		[HttpPost]
		public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var transaccion = await repositorioTransacciones.ObtenerPorID(id, usuarioID);
			if (transaccion is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			await repositorioTransacciones.Borrar(id);

			if (string.IsNullOrEmpty(urlRetorno))
			{
				return RedirectToAction("Index");

			}
			else
			{
				//Redirigir a una url especifica dentro del proyecto
				return LocalRedirect(urlRetorno);
			}
		}



		//Metodo para obtener cuentas
		private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioID)
		{
			var cuentas = await repositorioCuentas.Buscar(usuarioID);
			return cuentas.Select(x => new SelectListItem(x.Nombre, x.id.ToString()));
		}

		//Metodo para obtener categorias
		private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioID, TipoOperacion tipoOperacion)
		{
			var categorias = await respositorioCategorias.Obtener(usuarioID, tipoOperacion);
			var resultado = categorias.Select(x => new SelectListItem(x.Nombre, x.id.ToString())).ToList();

			//Insertando la opcion por defecto en el DropDownList
			var opcionPorDefecto = new SelectListItem("-- Seleccione una categoria --", "0", true);
			resultado.Insert(0, opcionPorDefecto);

			return resultado;
		}

		//Metodo para enviar las categorias a la vista de crear
		[HttpPost]
		public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var categorias = await ObtenerCategorias(usuarioID, tipoOperacion);
			return Ok(categorias);

		}
	}
}
