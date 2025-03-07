﻿using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
	public class CategoriaModel
	{
		public int id { get; set; }
		[Required(ErrorMessage ="El campo {0} es requerido")]
		[StringLength(maximumLength:50, ErrorMessage = "No puede ser mayor a {1} caracteres")]
		public string Nombre { get; set; }
		[Display(Name ="Tipo Operación")]
		public TipoOperacion TipoOperacionID { get; set; }
		public int UsuarioID { get; set; }

	}
}
