using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ManejoPresupuesto.Controllers
{
	public class CategoriasController : Controller
	{
		private readonly IRespositorioCategorias respositorioCategorias;
		private readonly IServicioUsuarios servicioUsuarios;

		public CategoriasController(IRespositorioCategorias respositorioCategorias, IServicioUsuarios servicioUsuarios)
		{
			this.respositorioCategorias = respositorioCategorias;
			this.servicioUsuarios = servicioUsuarios;
		}



		//*** INDEX ***
		[HttpGet]
		public async Task<IActionResult> Index(PaginacionViewModel paginacionVM)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var categorias = await respositorioCategorias.Obtener(usuarioID, paginacionVM);
			var totalCategorias = await respositorioCategorias.Contar(usuarioID);

			var respuestaVM = new PaginacionRespuestaVM<CategoriaModel>
			{
				Elementos = categorias,
				Pagina = paginacionVM.Pagina,
				RecordsPorPagina = paginacionVM.RecordsPorPagina,
				CantidadTotalRecords = totalCategorias,
				BaseURL = Url.Action()  /*Con Url.Action obtenemos la ruta actual en la que nos encontramos, para este ejemplo estaremos en /categorias*/
			};

			return View(respuestaVM);
		}



		//*** CREAR ***
		[HttpGet]
		public IActionResult Crear()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Crear(CategoriaModel categoria)
		{

			if (!ModelState.IsValid)
			{
				return View(categoria);
			}

			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			categoria.UsuarioID = usuarioID;
			await respositorioCategorias.Crear(categoria);
			return RedirectToAction("Index");

		}



		//*** EDITAR ***
		[HttpGet]
		public async Task<IActionResult> Editar(int id)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var categoria = await respositorioCategorias.ObtenerPorID(id, usuarioID);

			if (categoria is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			return View(categoria);
		}

		[HttpPost]
		public async Task<IActionResult> Editar(CategoriaModel categoriaEditar)
		{
			if (!ModelState.IsValid)
			{
				return View(categoriaEditar);
			}

			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var categoria = await respositorioCategorias.ObtenerPorID(categoriaEditar.id, usuarioID);

			if (categoria is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			categoriaEditar.UsuarioID = usuarioID;

			await respositorioCategorias.Actualizar(categoriaEditar);
			return RedirectToAction("Index");
		}



		//*** ELIMINAR ***
		[HttpGet]
		public async Task<IActionResult> Borrar(int id)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var categoria = await respositorioCategorias.ObtenerPorID(id, usuarioID);

			if (categoria is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			return View(categoria);
		}

		[HttpPost]
		public async Task<IActionResult> BorrarCategoria(int id)
		{
			var usuarioID = servicioUsuarios.ObtenerUsuarioID();
			var categoria = await respositorioCategorias.ObtenerPorID(id, usuarioID);

			if (categoria is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			await respositorioCategorias.Borrar(id);
			return RedirectToAction("Index");
		}

	}
}
