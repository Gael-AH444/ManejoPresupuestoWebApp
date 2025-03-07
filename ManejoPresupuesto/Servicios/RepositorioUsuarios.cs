using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ManejoPresupuesto.Servicios
{
	public interface IRepositorioUsuarios
	{
		Task Actualizar(Usuario usuario);
		Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
		Task<int> CrearUsuario(Usuario usuario);
	}


	public class RepositorioUsuarios : IRepositorioUsuarios
	{
		private readonly string connectionString;
		public RepositorioUsuarios(IConfiguration configuration)
		{
			connectionString = configuration.GetConnectionString("DefaultConnection");
		}

		public async Task<int> CrearUsuario(Usuario usuario)
		{
			using var connex = new SqlConnection(connectionString);
			var usuarioID = await connex.QuerySingleAsync<int>(@"INSERT INTO Usuarios(Email, EmailNormalizado, PasswordHash)
														VALUES (@Email, @EmailNormalizado, @PasswordHash);
														SELECT SCOPE_IDENTITY();
														", usuario);

			await connex.ExecuteAsync("CrearDatosUsuarioNuevo", new { usuarioID },
			commandType: System.Data.CommandType.StoredProcedure);

			return usuarioID;
		}


		public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
		{
			using var connex = new SqlConnection(connectionString);
			return await connex.QuerySingleOrDefaultAsync<Usuario>(
				"SELECT * FROM Usuarios WHERE EmailNormalizado = @emailNormalizado",
				new { emailNormalizado });
		}

		//Metodo para actualizar contraseña del usuario
		public async Task Actualizar(Usuario usuario)
		{
			using var connex = new SqlConnection(connectionString);
			await connex.ExecuteAsync(@"UPDATE Usuarios
										SET PasswordHash = @PasswordHash
										WHERE id = @id", usuario);
		}
	}
}
