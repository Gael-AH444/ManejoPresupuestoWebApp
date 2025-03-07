using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
	public class TransaccionCreacionViewModel : TransaccionModel
	{
		public IEnumerable<SelectListItem> Cuentas { get; set; }
		public IEnumerable<SelectListItem> Categorias { get; set; }
		
	}
}
