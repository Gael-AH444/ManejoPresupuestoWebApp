using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;

namespace ManejoPresupuesto.Servicios
{

	public interface IServicioUsuarios
	{
		int ObtenerUsuarioID();
	}

	public class ServicioUsuarios : IServicioUsuarios
	{
		private readonly HttpContext httpContext;

		public ServicioUsuarios(IHttpContextAccessor httpContextAccessor)
		{
			httpContext = httpContextAccessor.HttpContext;
		}

		public int ObtenerUsuarioID()
		{
			if (httpContext.User.Identity.IsAuthenticated)
			{
				//Claim = Hace referencia a la informacion de usuario, como email, nombre, etc.
				var idClaim = httpContext.User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();

				var id = int.Parse(idClaim.Value);

				return id;
			}
			else
			{
				throw new ApplicationException("El usuario no esta autenticado");
			}

			return 2;
		}
	}
}
