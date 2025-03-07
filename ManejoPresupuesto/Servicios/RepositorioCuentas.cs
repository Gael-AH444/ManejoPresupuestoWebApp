using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ManejoPresupuesto.Servicios
{

	public interface IRepositorioCuentas
	{
		Task Actualizar(CuentaCreacionModel cuenta);
		Task Borrar(int id);
		Task<IEnumerable<CuentaModel>> Buscar(int usuarioID);
		Task Crear(CuentaModel cuenta);
		Task<CuentaModel> ObtenerPorID(int id, int usuarioID);
	}

	public class RepositorioCuentas : IRepositorioCuentas
	{
		private readonly string connectionString;

		public RepositorioCuentas(IConfiguration configuration)
		{
			connectionString = configuration.GetConnectionString("DefaultConnection");
		}


		public async Task Crear(CuentaModel cuenta)
		{
			using var connection = new SqlConnection(connectionString);
			var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Cuentas (Nombre, TipoCuentaID, Balance, Descripcion) 
														VALUES (@Nombre, @TipoCuentaID, @Balance, @Descripcion);
														SELECT SCOPE_IDENTITY();", cuenta);

			cuenta.id = id;
		}

		public async Task<IEnumerable<CuentaModel>> Buscar(int usuarioID)
		{
			using var connection = new SqlConnection(connectionString);
			return await connection.QueryAsync<CuentaModel>(@"SELECT Cuentas.id, Cuentas.Balance, Cuentas.Nombre, tc.Nombre AS TipoCuenta 
															  FROM Cuentas INNER JOIN TiposCuentas tc 
															  ON tc.id = Cuentas.TipoCuentaID
															  WHERE tc.UsuarioID = @UsuarioID
															  ORDER BY tc.Orden", new { usuarioID });
		}

		public async Task<CuentaModel> ObtenerPorID(int id, int usuarioID)
		{
			using var connex = new SqlConnection(connectionString);
			return await connex.QueryFirstOrDefaultAsync<CuentaModel>(@"SELECT Cuentas.id, Cuentas.Balance, Cuentas.Nombre, Descripcion, TipoCuentaID
															  FROM Cuentas INNER JOIN TiposCuentas tc 
															  ON tc.id = Cuentas.TipoCuentaID
															  WHERE tc.UsuarioID = @UsuarioID AND Cuentas.id = @id", new { id, usuarioID });
		}

		public async Task Actualizar(CuentaCreacionModel cuenta)
		{
			using var connex = new SqlConnection(connectionString);
			//Con Execute no retornamos ningun valor
			await connex.ExecuteAsync(@"UPDATE Cuentas 
										SET Nombre = @Nombre, Balance = @Balance, Descripcion = @Descripcion,
										TipoCuentaID = @TipoCuentaID
										WHERE id = @id;", cuenta);

		}

		public async Task Borrar(int id)
		{
			using var connex = new SqlConnection(connectionString);
			await connex.ExecuteAsync("DELETE Cuentas WHERE id = @id", new { id });
		}
	}
}
