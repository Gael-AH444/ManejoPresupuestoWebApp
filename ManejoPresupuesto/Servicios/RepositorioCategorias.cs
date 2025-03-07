using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Extensions.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ManejoPresupuesto.Servicios
{
	public interface IRespositorioCategorias
	{
		Task Crear(CategoriaModel categoria);
		Task Actualizar(CategoriaModel categoria);
		Task<IEnumerable<CategoriaModel>> Obtener(int UsuarioID, PaginacionViewModel paginacion);
		Task<CategoriaModel> ObtenerPorID(int id, int UsuarioID);
		Task Borrar(int id);
		Task<IEnumerable<CategoriaModel>> Obtener(int UsuarioID, TipoOperacion tipoOperacionID);
		Task<int> Contar(int usuarioID);
	}

	public class RepositorioCategorias : IRespositorioCategorias
	{
		private readonly string connectionString;

		//Obtener cadena de conexion
		public RepositorioCategorias(IConfiguration configuration)
		{
			connectionString = configuration.GetConnectionString("DefaultConnection");
		}

		//Crear categoria
		public async Task Crear(CategoriaModel categoria)
		{
			using var connex = new SqlConnection(connectionString);
			var id = await connex.QuerySingleAsync<int>(@"INSERT INTO Categorias (Nombre, TipoOperacionID, UsuarioID)
														VALUES (@Nombre, @TipoOperacionID, @UsuarioID);

														SELECT SCOPE_IDENTITY();", categoria);
			categoria.id = id;
		}

		//Listar categorias con paginacion
		public async Task<IEnumerable<CategoriaModel>> Obtener(int UsuarioID, PaginacionViewModel paginacion)
		{
			using var connex = new SqlConnection(connectionString);
			return await connex.QueryAsync<CategoriaModel>(
				@$"SELECT * 
				   FROM Categorias 
				   WHERE UsuarioID = @UsuarioID
				   ORDER BY Nombre
				   OFFSET {paginacion.RecordsASaltar} ROWS FETCH NEXT {paginacion.RecordsPorPagina} 
				   ROWS ONLY
				   ", new { UsuarioID });
		}

		public async Task<int> Contar(int usuarioID)
		{
			using var connex = new SqlConnection(connectionString);
			//El scalar devuelve un numero 
			return await connex.ExecuteScalarAsync<int>(
				"SELECT COUNT(*) FROM Categorias WHERE UsuarioID = @usuarioID", new { usuarioID }
				);
		}

		//Listar categorias
		public async Task<IEnumerable<CategoriaModel>> Obtener(int UsuarioID, TipoOperacion tipoOperacionID)
		{
			using var connex = new SqlConnection(connectionString);
			return await connex.QueryAsync<CategoriaModel>(@"SELECT * FROM Categorias WHERE UsuarioID = @UsuarioID AND 
															TipoOperacionID = @tipoOperacionID", new { UsuarioID, tipoOperacionID });
		}


		//Obtener informacion de categorias por ID
		public async Task<CategoriaModel> ObtenerPorID(int id, int UsuarioID)
		{
			using var connex = new SqlConnection(connectionString);
			return await connex.QueryFirstOrDefaultAsync<CategoriaModel>(@"SELECT * FROM Categorias 
																		WHERE id = @id AND UsuarioID = @UsuarioID",
																		new { id, UsuarioID });
		}


		//Editar categoria
		public async Task Actualizar(CategoriaModel categoria)
		{
			using var connex = new SqlConnection(connectionString);
			await connex.ExecuteAsync(@"UPDATE Categorias 
										SET Nombre = @Nombre, TipoOperacionID = @TipoOperacionID
										WHERE id = @id", categoria);
		}


		//Eliminar categoria
		public async Task Borrar(int id)
		{
			using var connex = new SqlConnection(connectionString);
			await connex.ExecuteAsync("DELETE Categorias WHERE id = @id", new { id });
		}
	}
}
