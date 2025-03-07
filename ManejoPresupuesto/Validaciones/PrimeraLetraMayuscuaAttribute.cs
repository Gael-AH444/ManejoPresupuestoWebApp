using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Validaciones
{
	public class PrimeraLetraMayuscuaAttribute : ValidationAttribute
	{
		//VALIDACION POR ATRIBUTO
		//Aqui se crea una validacion personalziada para verificar que el primer caracter este en Mayúsculas
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			//La validacion se omite si el campo es nullo o vacio
			if (value == null || string.IsNullOrEmpty(value.ToString()))
			{
				return ValidationResult.Success;
			}

			//Aqui validamos que la primera letra sea Mayúscula
			var stringValue = value.ToString();
			if (!char.IsUpper(stringValue[0]))
			{
				return new ValidationResult("La primera letra debe ser mayúscula.");
			}

			return ValidationResult.Success;
		}
	}
}
