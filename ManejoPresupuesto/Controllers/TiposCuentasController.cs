using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace ManejoPresupuesto.Controllers
{
	public class TiposCuentasController : Controller
	{
		private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
		private readonly IServicioUsuarios servicioUsuarios;

		public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IServicioUsuarios servicioUsuarios)
		{
			this.repositorioTiposCuentas = repositorioTiposCuentas;
			this.servicioUsuarios = servicioUsuarios;
		}


		public async Task<IActionResult> Index()
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioID);
			return View(tiposCuentas);
		}


		[HttpGet]
		public IActionResult Crear()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Crear(TipoCuentaModel tipoCuenta)
		{
			if (!ModelState.IsValid)
			{
				return View(tipoCuenta);
			}

			tipoCuenta.UsuarioID = servicioUsuarios.ObtenerUsuarioID(); ;

			//Validar si exiate o no un registro de nombre del tipo cuenta, repetido para un mismo usuario
			var yaExiste = await repositorioTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioID);
			if (yaExiste)
			{
				ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe");

				return View(tipoCuenta);
			}

			await repositorioTiposCuentas.Crear(tipoCuenta);

			return RedirectToAction("Index");
		}

		[HttpGet]
		public async Task<IActionResult> Editar(int id)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioID);

			if (tipoCuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			return View(tipoCuenta);
		}

		[HttpPost]
		public async Task<IActionResult> Editar(TipoCuentaModel tipoCuenta)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var tipoCuentaExiste = await repositorioTiposCuentas.ObtenerPorId(tipoCuenta.id, usuarioID);

			if (tipoCuentaExiste is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			await repositorioTiposCuentas.Actualizar(tipoCuenta);
			return RedirectToAction("Index");
		}



		[HttpGet]
		public async Task<IActionResult> Borrar(int id)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioID);

			if (tipoCuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			return View(tipoCuenta);
		}

		[HttpPost]
		public async Task<IActionResult> BorrarTipoCuenta(int id)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioID);

			if (tipoCuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			await repositorioTiposCuentas.Borrar(id);
			return RedirectToAction("Index");
		}

		//Verificar que no exista duoicado en el nombre de tipo cuenta para un mismo usuario, con JSON y JS
		//Validacion por parte del controllador
		[HttpGet]
		public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre, int id)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID(); ;
			var yaExusteTipoCuenta = await repositorioTiposCuentas.Existe(nombre, usuarioID, id);

			if (yaExusteTipoCuenta)
			{
				return Json($"El nombre {nombre} ya existe");
			}

			return Json(true);
		}


		[HttpPost]
		public async Task<IActionResult> Ordenar([FromBody] int[] ids) /*El parámetro[FromBody] indica que los datos vienen en el cuerpo de la solicitud en formato JSON*/
		{
			var usuarioId = servicioUsuarios.ObtenerUsuarioID();
			var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId); //Obtiene la informacion de Tipos Cuentas apartir del Id del usuario
			var idsTiposCuentas = tiposCuentas.Select(x => x.id); //Selecciona el id de los registro de Tipos Cuentas

			//Validar que los ids que vienen desde la BD y desde el Front sean los mismos
			var idsTiposCuentasNoPertenecenAlUsuario = ids.Except(idsTiposCuentas).ToList();
			if (idsTiposCuentasNoPertenecenAlUsuario.Count > 0)
			{
				return Forbid(); //Forbid = Prohibido
			}

			//Ordena 
			var tiposCuentasOrdenados = ids.Select((valor, indice) =>
				new TipoCuentaModel { id = valor, Orden = indice + 1 }).AsEnumerable();

			await repositorioTiposCuentas.Ordenar(tiposCuentasOrdenados);

			return Ok();
		}
	}
}
