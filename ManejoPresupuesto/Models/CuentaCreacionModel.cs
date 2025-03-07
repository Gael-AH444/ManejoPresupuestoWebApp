using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ManejoPresupuesto.Models
{
	public class CuentaCreacionModel : CuentaModel
	{
		public IEnumerable<SelectListItem> TiposCuentas { get; set; }
	}
}
