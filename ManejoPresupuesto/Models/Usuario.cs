﻿namespace ManejoPresupuesto.Models
{
	public class Usuario
	{
		public int id { get; set; }
		public string Email { get; set; }
		public string EmailNormalizado { get; set; }
		public string PasswordHash { get; set; }
	}
}
