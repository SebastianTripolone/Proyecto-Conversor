using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configuración de autenticación
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = "881839903286-v2j02lviuafr7gmfkkt4g5s8ibmb7fvs.apps.googleusercontent.com";
    options.ClientSecret = "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";
    options.CallbackPath = "/Acceso/ExternalLoginCallback";
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;

});

var app = builder.Build();

// Configuración de middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.UseEndpoints(endpoints =>
{
    // Rutas Personalizadas que conecta con el Controlador Default ( En este caso seria: HomeController )
    endpoints.MapControllerRoute(
        name: "Inicio",
        pattern: "Inicio",
        defaults: new { controller = "Home", action = "Index" });

    endpoints.MapControllerRoute(
        name: "Integrantes",
        pattern: "Integrantes",
        defaults: new { controller = "Home", action = "Privacy" });

    endpoints.MapControllerRoute(
        name: "Historia",
        pattern: "Historia",
        defaults: new { controller = "Home", action = "Historia" });

    endpoints.MapControllerRoute(
        name: "Graficos",
        pattern: "Graficos",
        defaults: new { controller = "Home", action = "Graficos" });

    endpoints.MapControllerRoute(
        name: "Registrar",
        pattern: "Registrar",
        defaults: new { controller = "Acceso", action = "Registrar" });

    endpoints.MapControllerRoute(
        name: "Restablecer",
        pattern: "Restablecer",
        defaults: new { controller = "Acceso", action = "Restablecer" });

    // Ruta Default
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Acceso}/{action=Login}/{id?}");
});

app.Run();
