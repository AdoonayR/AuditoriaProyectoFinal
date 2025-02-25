using AuditoriaQuimicos.Data;
using AuditoriaQuimicos.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configuración de controladores y vistas
builder.Services.AddControllersWithViews();

// Configuración de DbContext con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro de EmailService
builder.Services.AddScoped<IEmailService, EmailService>();

// Configuración de CORS para permitir peticiones desde cualquier origen (para propósitos de desarrollo)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});



// Usando Path.Combine para armar la ruta física
var rotativaPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "Rotativa");
RotativaConfiguration.Setup(rotativaPath);


// Configuración de autenticación y cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

        // Configuración para SameSite y Secure en cookies (requerido para HTTPS y contextos de sitio cruzado)
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Solo HTTPS
        options.Cookie.SameSite = SameSiteMode.None; // Permitir el uso en diferentes contextos
    });

// Configuración de sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // Configuración de SameSite y Secure para HTTPS en cookies de sesión
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
});

// Configuración de políticas de autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IncomingPolicy", policy => policy.RequireRole("IncomingSupervisor"));
    options.AddPolicy("StoragePolicy", policy => policy.RequireRole("StorageSupervisor"));
    options.AddPolicy("AuditorPolicy", policy => policy.RequireRole("Auditor"));
});

var app = builder.Build();

// Configuración de manejo de errores y seguridad
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Habilitar CORS para solicitudes de diferentes orígenes
app.UseCors("AllowAllOrigins");

// Deshabilitar caché para páginas protegidas
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "-1";
    await next();
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Configuración de las rutas para los controladores
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
