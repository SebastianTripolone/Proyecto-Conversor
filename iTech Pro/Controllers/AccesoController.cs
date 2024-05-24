using iTech_Pro.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using iTech_Pro.Models;
using iTech_Pro.Datos;
using iTech_Pro.Servicios;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Policy;
using Microsoft.AspNetCore.Authorization;

namespace iTech_Pro.Controllers
{
    public class AccesoController : Controller
    {

        private readonly IWebHostEnvironment _hostingEnvironment;

        public AccesoController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Inicio
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string correo, string clave)
        {
            var isCaptchaValid = IsReCaptchValid();

            if (!isCaptchaValid)
            {
                ViewData["Mensaje"] = "Error en reCAPTCHA.";
                return View();
            }


            UsuarioDTO usuario = DBUsuario.Validar(correo, UtilidadServicio.ConvertirSHA256(clave));

            if (usuario != null)
            {
                if (!usuario.Confirmado)
                {
                    ViewBag.Mensaje = $"Falta confirmar su cuenta. Se le envio un correo a {correo}";
                }
                else if (usuario.Restablecer)
                {
                    ViewBag.Mensaje = $"Se ha solicitado restablecer su cuenta, favor revise su bandeja del correo {correo}";
                }
                else
                {
                    // guarda el objeto usuario en la sesión
                    HttpContext.Session.SetString("usuario", JsonConvert.SerializeObject(usuario));

                    return RedirectToAction("Index", "Home");
                }

            }
            else
            {
                ViewBag.Mensaje = "No se encontraron coincidencias";
            }

            return View();
        }

        private bool IsReCaptchValid()
        {
            var captchaResponse = Request.Form["g-recaptcha-response"];
            var secretKey = "6Ld17_UnAAAAAOWs0zTT-UxMYONIZ9Eo4vdUM1aG";  // clave secreta de reCAPTCHA

            using (var httpClient = new HttpClient())
            {
                var response = httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={captchaResponse}").Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    dynamic data = JObject.Parse(jsonResponse);
                    return data.success == "true";
                }
                else
                {
                    // Maneja el error de reCAPTCHA
                    return false;
                }
            }
        }

        public ActionResult Registrar()
        {
            return View();
        }

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear(); // Limpia la sesión
            return RedirectToAction("Login", "Acceso"); // Redirige al login
        }

        [HttpPost]
        public ActionResult Registrar(UsuarioDTO usuario)
        {
            if (usuario.Clave != usuario.ConfirmarClave)
            {
                ViewBag.Nombre = usuario.Nombre;
                ViewBag.Correo = usuario.Correo;
                ViewBag.Mensaje = "Las contraseñas no coinciden";
                return View();
            }

            if (DBUsuario.Obtener(usuario.Correo) == null)
            {
                usuario.Clave = UtilidadServicio.ConvertirSHA256(usuario.Clave);
                usuario.Token = UtilidadServicio.GenerarToken();
                usuario.Restablecer = false;
                usuario.Confirmado = false;
                bool respuesta = DBUsuario.Registrar(usuario);

                if (respuesta)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "Plantilla", "Confirmar.html");
                    string content = System.IO.File.ReadAllText(path);
                    string url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, "/Acceso/Confirmar?token=" + usuario.Token);

                    string htmlBody = string.Format(content, usuario.Nombre, url);

                    CorreoDTO correoDTO = new CorreoDTO()
                    {
                        Para = usuario.Correo,
                        Asunto = "Verificación de correo electrónico",
                        Contenido = htmlBody
                    };

                    bool enviado = CorreoServicio.Enviar(correoDTO);
                    ViewBag.Creado = true;
                    ViewBag.Mensaje = $"Su cuenta ha sido creada. Hemos enviado un mensaje al correo {usuario.Correo} para confirmar su cuenta";
                }
                else
                {
                    ViewBag.Mensaje = "No se pudo crear su cuenta";
                }



            }
            else
            {
                ViewBag.Mensaje = "El correo ya se encuentra registrado";
            }


            return View();
        }

        public ActionResult Confirmar(string token)
        {
            ViewBag.Respuesta = DBUsuario.Confirmar(token);
            return View();
        }

        public ActionResult Restablecer()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Restablecer(string correo)
        {
            UsuarioDTO usuario = DBUsuario.Obtener(correo);
            ViewBag.Correo = correo;
            if (usuario != null)
            {
                bool respuesta = DBUsuario.RestablecerActualizar(1, usuario.Clave, usuario.Token);

                if (respuesta)
                {
                    string path = Path.Combine(_hostingEnvironment.ContentRootPath, "Plantilla", "Restablecer.html");
                    string content = System.IO.File.ReadAllText(path);
                    string url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, "/Acceso/Actualizar?token=" + usuario.Token);

                    string htmlBody = string.Format(content, usuario.Nombre, url);

                    CorreoDTO correoDTO = new CorreoDTO()
                    {
                        Para = correo,
                        Asunto = "Restablecer cuenta",
                        Contenido = htmlBody
                    };

                    bool enviado = CorreoServicio.Enviar(correoDTO);
                    ViewBag.Restablecido = true;
                }
                else
                {
                    ViewBag.Mensaje = "No se pudo restablecer la cuenta";
                }

            }
            else
            {
                ViewBag.Mensaje = "No se encontraron coincidencias con el correo";
            }

            return View();
        }

        public ActionResult Actualizar(string token)
        {
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public ActionResult Actualizar(string token, string clave, string confirmarClave)
        {
            ViewBag.Token = token;
            if (clave != confirmarClave)
            {
                ViewBag.Mensaje = "Las contraseñas no coinciden";
                return View();
            }

            bool respuesta = DBUsuario.RestablecerActualizar(0, UtilidadServicio.ConvertirSHA256(clave), token);

            if (respuesta)
                ViewBag.Restablecido = true;
            else
                ViewBag.Mensaje = "No se pudo actualizar";

            return View();
        }

        [HttpGet]
        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback"),
                Items =
        {
            { "returnUrl", returnUrl },
            { "scheme", GoogleDefaults.AuthenticationScheme }
        }
            };

            return Challenge(authenticationProperties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded)
            {
                return RedirectToAction("Login");
            }

            var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
            var name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name);

            // verifica si el usuario ya existe en la base de datos
            var usuarioExistente = DBUsuario.Obtener(email);
            if (usuarioExistente == null)
            {
                // registra nuevo usuario
                var nuevoUsuario = new UsuarioDTO
                {
                    Nombre = name,
                    Correo = email,
                    // genera una contraseña aleatoria o enviar al usuario a una página para establecer una contraseña
                    Clave = UtilidadServicio.ConvertirSHA256("ContraseñaTemporal"),
                    Token = UtilidadServicio.GenerarToken(),
                    Restablecer = false,
                    Confirmado = true // establece como confirmado si estás utilizando Google para autenticación
                };

                bool respuesta = DBUsuario.Registrar(nuevoUsuario);
                if (!respuesta)
                {
                    // maneja el error de registro
                    return RedirectToAction("Login");
                }
            }

            // inicia sesión con el usuario
            HttpContext.Session.SetString("usuario", email);

            return RedirectToAction("Index", "Home");
        }
    }
}
