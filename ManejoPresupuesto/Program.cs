using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

//Inyeccion de dependecias

//Politica para que solo los usuarios autenticados puedan navegar los las diferentes secciones
//Estableciendo 'Authorize' de manera global
var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder()
	.RequireAuthenticatedUser()
	.Build();

builder.Services.AddControllersWithViews(opc =>
{
	opc.Filters.Add(new AuthorizeFilter(politicaUsuariosAutenticados));
});
builder.Services.AddTransient<IRepositorioTiposCuentas, RepositorioTiposCuentas>();
builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();
builder.Services.AddTransient<IRepositorioCuentas, RepositorioCuentas>();
builder.Services.AddTransient<IRepositorioTransacciones, RepositorioTransacciones>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IServicioReportes, ServicioReportes>();
builder.Services.AddTransient<IRepositorioUsuarios, RepositorioUsuarios>();
builder.Services.AddTransient<IUserStore<Usuario>, UsuarioStore>();
builder.Services.AddTransient<SignInManager<Usuario>>(); //Sing in manager
builder.Services.AddIdentityCore<Usuario>(opciones => //Configurar Identity para el registro de usuarios
{
	// * Reglas de validacion de la contrase�a * 
	opciones.Password.RequireDigit = false;
	opciones.Password.RequireLowercase = false;
	opciones.Password.RequireUppercase = false;
	opciones.Password.RequireNonAlphanumeric = false;
})/*.AddErrorDescriber<MensajesDeErrorIdentity>()*/
	.AddDefaultTokenProviders(); /*Con 'AddDefaultTokenProviders' permitimos al usuario poder recuperar la contrase�a*/

//Configuracion de cookies para autenticacion
builder.Services.AddAuthentication(opt =>
{
	opt.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
	opt.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
	opt.DefaultSignOutScheme = IdentityConstants.ApplicationScheme;
}).AddCookie(IdentityConstants.ApplicationScheme, opt =>
{
	opt.LoginPath = "/usuarios/login";
});
builder.Services.AddTransient<IServicioEmail, ServicioEmail>();

builder.Services.AddAutoMapper(typeof(Program)); //Configurar AutoMapper


builder.Services.AddTransient<IRespositorioCategorias, RepositorioCategorias>();



// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Transacciones}/{action=Index}/{id?}");

app.Run();
