using ManejoPresupuesto.Validaciones;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
	public class TipoCuentaModel
	{
		public int id { get; set; }

		[Required(ErrorMessage = "El campo {0} es requerido")]
		[PrimeraLetraMayuscua]
		[Remote(action: "VerificarExisteTipoCuenta", controller: "TiposCuentas",
			AdditionalFields = nameof(id))] //Validacion para el controlador sin necesidad de ejecutar una peticion POST
		public string Nombre { get; set; }
		public int UsuarioID { get; set; }
		public int Orden { get; set; }



	}
}
