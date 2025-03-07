using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace ManejoPresupuesto.Controllers
{
	public class CuentasController : Controller
	{
		private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
		private readonly IServicioUsuarios servicioUsuarios;
		private readonly IRepositorioCuentas repositorioCuentas;
		private readonly IMapper mapper;
		private readonly IRepositorioTransacciones repositorioTransacciones;
		private readonly IServicioReportes servicioReportes;

		public CuentasController(IRepositorioTiposCuentas repositorioTiposCuentas,
			IServicioUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas,
			IMapper mapper, IRepositorioTransacciones repositorioTransacciones,
			IServicioReportes servicioReportes)
		{
			this.repositorioTiposCuentas = repositorioTiposCuentas;
			this.servicioUsuarios = servicioUsuarios;
			this.repositorioCuentas = repositorioCuentas;
			this.mapper = mapper;
			this.repositorioTransacciones = repositorioTransacciones;
			this.servicioReportes = servicioReportes;
		}



		//*** INDEX ***
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var cuentasConTipoCuenta = await repositorioCuentas.Buscar(usuarioID);

			var modelo = cuentasConTipoCuenta
				.GroupBy(x => x.TipoCuenta)
				.Select(grupo => new IndiceCuentaModel
				{
					TipoCuenta = grupo.Key,
					Cuentas = grupo.AsEnumerable()
				}).ToList();

			return View(modelo);
		}



		/*** DETALLE DE CUENTAS POR MES ***/
		[HttpGet]
		public async Task<IActionResult> Detalle(int id, int mes, int año)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var cuenta = await repositorioCuentas.ObtenerPorID(id, usuarioID);

			if (cuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			ViewBag.Cuenta = cuenta.Nombre;

			var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladasPorCuenta(usuarioID, id, mes, año, ViewBag);

			return View(modelo);
		}



		//*** CREAR ***
		[HttpGet]
		public async Task<IActionResult> Crear()
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var modelo = new CuentaCreacionModel();
			modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioID);

			return View(modelo);
		}

		[HttpPost]
		public async Task<IActionResult> Crear(CuentaCreacionModel cuenta)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuenta.TipoCuentaID, usuarioID);

			if (tipoCuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			if (string.IsNullOrEmpty(cuenta.Nombre) && cuenta.Balance <= 0 && string.IsNullOrEmpty(cuenta.Descripcion))
			{
				cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioID);
				return View(cuenta);
			}

			await repositorioCuentas.Crear(cuenta);

			return RedirectToAction("Index");
		}



		//*** EDITAR ***
		[HttpGet]
		public async Task<IActionResult> Editar(int id)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var cuenta = await repositorioCuentas.ObtenerPorID(id, usuarioID);

			if (cuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			//Mapeando objetos desde CuentCreacionModel hacia CuentaModel
			var modelo = mapper.Map<CuentaCreacionModel>(cuenta);
			//Simplificando Mapeo con AutoMapper
			//var modelo = new CuentaCreacionModel
			//{
			//	id = cuenta.id,
			//	Nombre = cuenta.Nombre,
			//	TipoCuentaID = cuenta.TipoCuentaID,
			//	Descripcion = cuenta.Descripcion,
			//	Balance = cuenta.Balance
			//};

			modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioID);
			return View(modelo);

		}

		[HttpPost]
		public async Task<IActionResult> Editar(CuentaCreacionModel cuentaEditar)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var cuenta = await repositorioCuentas.ObtenerPorID(cuentaEditar.id, usuarioID);

			if (cuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuentaEditar.TipoCuentaID, usuarioID);
			if (tipoCuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			await repositorioCuentas.Actualizar(cuentaEditar);
			return RedirectToAction("Index");
		}



		//*** ELIMINAR ***
		[HttpGet]
		public async Task<IActionResult> Borrar(int id)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var cuenta = await repositorioCuentas.ObtenerPorID(id, usuarioID);

			if (cuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			return View(cuenta);
		}

		[HttpPost]
		public async Task<IActionResult> BorrarCuenta(int id)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var cuenta = await repositorioCuentas.ObtenerPorID(id, usuarioID);

			if (cuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			await repositorioCuentas.Borrar(id);
			return RedirectToAction("Index");
		}



		//Metodo para obtener los tipos cuentas para llenar la lista desplegable
		private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioID)
		{
			var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioID);
			return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.id.ToString()));
		}
	}
}
