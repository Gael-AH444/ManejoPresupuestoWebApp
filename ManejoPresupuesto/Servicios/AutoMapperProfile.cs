using AutoMapper;
using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Servicios
{
	public class AutoMapperProfile : Profile
	{
		//Contructor
		public AutoMapperProfile()
		{
			//Especificar de que tipo de datos a que tipo de datos vamos a mapear 
			CreateMap<CuentaModel, CuentaCreacionModel>();
			CreateMap<TransaccionActualizacionVM, TransaccionModel>().ReverseMap(); //Mapeo de manera visceversa 
		}
	}
}
