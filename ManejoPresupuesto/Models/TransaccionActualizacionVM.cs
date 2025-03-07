namespace ManejoPresupuesto.Models
{
	public class TransaccionActualizacionVM : TransaccionCreacionViewModel
	{
		public int CuentaAnteriorID { get; set; }
		public decimal MontoAnterior { get; set; }
		public string UrlRetorno { get; set; }
	}
}
