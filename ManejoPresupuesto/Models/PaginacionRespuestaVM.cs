using System;
using System.Collections;
using System.Collections.Generic;

namespace ManejoPresupuesto.Models
{
	public class PaginacionRespuestaVM
	{
		public int Pagina { get; set; } = 1;
		public int RecordsPorPagina { get; set; } = 5;
		public int CantidadTotalRecords { get; set; }
		//Math.Celing redondea hacia arriba el resultado de la divicion
		public int CantidadTotalDePaginas => (int)Math.Ceiling((double)CantidadTotalRecords / RecordsPorPagina);
		public string BaseURL { get; set; }
	}

	//Se crea una nueva clase con un valor generico <T> que hereda de PaginacionRepuestaVM
	public class PaginacionRespuestaVM<T> : PaginacionRespuestaVM
	{
		public IEnumerable<T> Elementos { get; set; }
	}
}
