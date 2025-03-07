using System.Collections.Generic;
using System.Linq;

namespace ManejoPresupuesto.Models
{
	public class IndiceCuentaModel
	{
		public string TipoCuenta { get; set; }
		public IEnumerable<CuentaModel> Cuentas { get; set; }
        public decimal Balance => Cuentas.Sum(x => x.Balance);
    }
}
