using AuditoriaQuimicos.Data;
using AuditoriaQuimicos.Services; // Importar el namespace de EmailService
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor.
builder.Services.AddControllersWithViews();

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar EmailService
builder.Services.AddScoped<IEmailService, EmailService>();  // Agregar el servicio de correo electr�nico

// Configurar servicios de autenticaci�n
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Duraci�n de la sesi�n
    });

// Configurar servicios de sesi�n
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiraci�n de la sesi�n
    options.Cookie.HttpOnly = true; // Asegurar que la cookie de sesi�n no sea accesible por el lado del cliente
    options.Cookie.IsEssential = true; // Marcar la cookie como esencial
});

// Configurar servicios de autorizaci�n
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IncomingPolicy", policy => policy.RequireRole("IncomingSupervisor"));
    options.AddPolicy("StoragePolicy", policy => policy.RequireRole("StorageSupervisor"));
    options.AddPolicy("AuditorPolicy", policy => policy.RequireRole("Auditor"));
});

var app = builder.Build();

// Configuraci�n del pipeline de manejo de solicitudes HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Deshabilitar la cach� para evitar que las p�ginas protegidas se carguen desde el historial del navegador
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "-1";
    await next();
});

app.UseRouting();

app.UseAuthentication(); // Middleware de autenticaci�n
app.UseAuthorization();

// Configurar middleware de sesi�n
app.UseSession();

// Configurar las rutas para los controladores.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
