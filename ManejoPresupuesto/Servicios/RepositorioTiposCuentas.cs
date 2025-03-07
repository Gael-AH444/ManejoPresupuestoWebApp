using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace ManejoPresupuesto.Servicios
{

	public interface IRepositorioTiposCuentas
	{
		Task Actualizar(TipoCuentaModel tipoCuenta);
		Task Borrar(int id);
		Task Crear(TipoCuentaModel tipoCuenta);
		Task<bool> Existe(string nombre, int usuarioId, int id = 0);
		Task<IEnumerable<TipoCuentaModel>> Obtener(int usuarioID);
		Task<TipoCuentaModel> ObtenerPorId(int id, int usuarioId);
		Task Ordenar(IEnumerable<TipoCuentaModel> tiposCuentasOrdenados);
	}

	public class RepositorioTiposCuentas : IRepositorioTiposCuentas
	{
		private readonly string connectionString;
		public RepositorioTiposCuentas(IConfiguration configuration)
		{
			connectionString = configuration.GetConnectionString("DefaultConnection");
		}

		//Metodo asincrono para crear un nueno tipo de cuenta
		public async Task Crear(TipoCuentaModel tipoCuenta)
		{
			using var connection = new SqlConnection(connectionString);
			var id = await connection.QuerySingleAsync<int>("TiposCuentas_Insertar",
																new
																{
																	UsuarioID = tipoCuenta.UsuarioID,
																	Nombre = tipoCuenta.Nombre
																},
																commandType: System.Data.CommandType.StoredProcedure);
			tipoCuenta.id = id;
		}

		//Validar que no existan 2 nombres de cuenta repetidos para el mismo usuario
		public async Task<bool> Existe(string nombre, int usuarioId, int id = 0)
		{
			using var connection = new SqlConnection(connectionString);
			var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1 FROM TiposCuentas 
																		WHERE Nombre = @Nombre AND UsuarioID = @UsuarioID AND id <> @id;",
																		new { nombre, usuarioId, id });

			return existe == 1;
		}

		//Obtener un listado de Tipos Cuentas
		public async Task<IEnumerable<TipoCuentaModel>> Obtener(int usuarioID)
		{
			using var connection = new SqlConnection(connectionString);

			return await connection.QueryAsync<TipoCuentaModel>(@"SELECT id, Nombre, Orden 
																FROM TiposCuentas
																WHERE UsuarioID = @UsuarioID
																ORDER BY Orden",
																new { usuarioID });
		}

		public async Task Actualizar(TipoCuentaModel tipoCuenta)
		{
			using var connection = new SqlConnection(connectionString);
			await connection.ExecuteAsync(@"UPDATE TiposCuentas
											SET Nombre = @Nombre
											WHERE id = @id", tipoCuenta);
		}

		public async Task<TipoCuentaModel> ObtenerPorId(int id, int usuarioId)
		{
			using var connection = new SqlConnection(connectionString);
			return await connection.QueryFirstOrDefaultAsync<TipoCuentaModel>(@"SELECT id, Nombre, Orden 
																				FROM TiposCuentas
																				WHERE id = @id
																				AND UsuarioID = @UsuarioID",
																				new { id, usuarioId });
		}

		public async Task Borrar(int id)
		{
			using var connection = new SqlConnection(connectionString);
			await connection.ExecuteAsync(@"DELETE TiposCuentas WHERE id = @id", new { id });
		}



		/*Metodo para ordenar los registro de Tipo cuentas
		 * Nota: Este metodo se ejecutara varias veces de manera automatica, las repeticiones las definira el numero 
		 * de items o elementos que contenga el 'IEnumerable<TipoCuentaModel>'
		 */
		public async Task Ordenar(IEnumerable<TipoCuentaModel> tiposCuentasOrdenados)
		{
			var query = "UPDATE TiposCuentas SET Orden = @Orden WHERE id = @id";
			using var connection = new SqlConnection(connectionString);

			await connection.ExecuteAsync(query, tiposCuentasOrdenados);
		}
	}
}
