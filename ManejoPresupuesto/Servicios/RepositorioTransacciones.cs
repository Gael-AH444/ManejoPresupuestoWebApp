using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Extensions.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ManejoPresupuesto.Servicios
{
	public interface IRepositorioTransacciones
	{
		Task Actualizar(TransaccionModel transaccion, decimal montoAnterior, int cuentaAnterior);
		Task Borrar(int id);
		Task Crear(TransaccionModel transaccion);
		Task<IEnumerable<TransaccionModel>> ObtenerPorCuentaID(ObtenerTransaccionesPorCuentaModel modelo);
		Task<TransaccionModel> ObtenerPorID(int id, int usuarioID);
		Task<IEnumerable<ResultadoObtenerPorMesModel>> ObtenerPorMes(int usuarioID, int año);
		Task<IEnumerable<ResultadoObtenerPorSemanaModel>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuarioModel modelo);
		Task<IEnumerable<TransaccionModel>> ObtenerPorUsuarioID(ParametroObtenerTransaccionesPorUsuarioModel modelo);
	}

	public class RepositorioTransacciones : IRepositorioTransacciones
	{
		private readonly string connectionString;

		//Obtener cadena de conexion en el constructor
		public RepositorioTransacciones(IConfiguration configuration)
		{
			connectionString = configuration.GetConnectionString("DefaultConnection");
		}


		//Crear nueva transaccion
		public async Task Crear(TransaccionModel transaccion)
		{
			using var connex = new SqlConnection(connectionString);
			var id = await connex.QuerySingleAsync<int>("Transacciones_Insertar", new
			{
				transaccion.UsuarioID,
				transaccion.Monto,
				transaccion.FechaTransaccion,
				transaccion.CategoriaID,
				transaccion.CuentaID,
				transaccion.Nota
			},
			commandType: System.Data.CommandType.StoredProcedure);

			transaccion.id = id;
		}


		//Actualizar transaccion
		public async Task Actualizar(TransaccionModel transaccion, decimal montoAnterior, int cuentaAnteriorID)
		{
			using var connex = new SqlConnection(connectionString);
			await connex.ExecuteAsync("Transacciones_Actualizar", new
			{
				transaccion.id,
				transaccion.FechaTransaccion,
				transaccion.Monto,
				transaccion.CategoriaID,
				transaccion.CuentaID,
				transaccion.Nota,
				montoAnterior,
				cuentaAnteriorID
			}, commandType: System.Data.CommandType.StoredProcedure);
		}


		//Borrar transaccion
		public async Task Borrar(int id)
		{
			using var connex = new SqlConnection(connectionString);
			await connex.ExecuteAsync("Transacciones_Borrar",
				new { id }, commandType: System.Data.CommandType.StoredProcedure);
		}


		//Obtener transacciones por ID
		public async Task<TransaccionModel> ObtenerPorID(int id, int usuarioID)
		{
			using var connex = new SqlConnection(connectionString);

			return await connex.QueryFirstOrDefaultAsync<TransaccionModel>(@"SELECT Transacciones.*, cat.tipoOperacionID FROM Transacciones
																			INNER JOIN Categorias cat
																			ON cat.id = Transacciones.CateogriaID
																			WHERE Transacciones.id = @id AND Transacciones.UsuarioID = @usuarioID",
																			new { id, usuarioID });
		}


		//Obtener transacciones por cuenta ID
		public async Task<IEnumerable<TransaccionModel>> ObtenerPorCuentaID(ObtenerTransaccionesPorCuentaModel modelo)
		{
			using var connex = new SqlConnection(connectionString);
			return await connex.QueryAsync<TransaccionModel>(@"SELECT t.id, t.Monto, t.FechaTransaccion, c.Nombre AS Categoria,
																cu.Nombre AS Cuenta, c.TipoOperacionID
																FROM Transacciones t 
																INNER JOIN Categorias c
																ON c.id = t.CateogriaID
																INNER JOIN Cuentas cu
																ON cu.id = t.CuentaID
																WHERE t.CuentaID = @CuentaID AND t.UsuarioID = @UsuarioID
																AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin", modelo);
		}


		public async Task<IEnumerable<TransaccionModel>> ObtenerPorUsuarioID(ParametroObtenerTransaccionesPorUsuarioModel modelo)
		{
			using var connex = new SqlConnection(connectionString);
			return await connex.QueryAsync<TransaccionModel>(@"SELECT t.id, t.Monto, t.FechaTransaccion, c.Nombre AS Categoria,
																cu.Nombre AS Cuenta, c.TipoOperacionID, Nota
																FROM Transacciones t 
																INNER JOIN Categorias c
																ON c.id = t.CateogriaID
																INNER JOIN Cuentas cu
																ON cu.id = t.CuentaID
																WHERE t.UsuarioID = @UsuarioID
																AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
																ORDER BY t.FechaTransaccion DESC", modelo);
		}

		//Reporte semanal
		public async Task<IEnumerable<ResultadoObtenerPorSemanaModel>> ObtenerPorSemana(
			ParametroObtenerTransaccionesPorUsuarioModel modelo)
		{
			using var connex = new SqlConnection(connectionString);
			return await connex.QueryAsync<ResultadoObtenerPorSemanaModel>(@"SELECT DATEDIFF(d, @fechaInicio, FechaTransaccion) / 7 + 1 as Semana,
																			SUM(Monto) AS Monto, cat.TipoOperacionID
																			FROM Transacciones
																			INNER JOIN Categorias cat
																			ON cat.id = Transacciones.CateogriaID
																			WHERE Transacciones.UsuarioID = @usuarioID AND 
																			FechaTransaccion BETWEEN @fechaInicio and @fechaFin
																			GROUP BY DATEDIFF(d, @fechaInicio, FechaTransaccion) / 7, cat.TipoOperacionID", modelo);
		}

		//Reporte mensual
		public async Task<IEnumerable<ResultadoObtenerPorMesModel>> ObtenerPorMes(int usuarioID, int año)
		{
			using var connex = new SqlConnection(connectionString);
			return await connex.QueryAsync<ResultadoObtenerPorMesModel>(@"SELECT MONTH(t.FechaTransaccion) AS Mes,
																		SUM(Monto) AS Monto, cat.TipoOperacionID
																		FROM Transacciones t
																		INNER JOIN Categorias cat
																		ON cat.id = t.CateogriaID
																		WHERE t.UsuarioID = @usuarioID 
																		AND YEAR(t.FechaTransaccion) = @año
																		GROUP BY MONTH(t.FechaTransaccion), cat.TipoOperacionID",
																		new { usuarioID, año });
		}
	}
}
