using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using tp4.Data;
using tp4.Models;

namespace tp4.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MiContexto _context;

        public HomeController(ILogger<HomeController> logger, MiContexto context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        // Método para mostrar la página de inicio de sesión
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // Método para procesar la información del formulario de inicio de sesión
        [HttpPost]
        public ActionResult Login(int dni, String password) // cambiar
        {
            Usuario user = _context.usuarios.Where(u => u.dni == dni).FirstOrDefault();


            // Validar el nombre de usuario y la contraseña
          
                if (user == null)
                {

                    // No se encontró el usuario
                    ModelState.AddModelError("error", "No se encontró el usuario");
                // Si la validación falla, agregar un mensaje de error y mostrar la página de inicio de sesión nuevamente
                ViewData["msg"] = "No se encontró el usuario";
                return View();
                }
                if (user?.bloqueado == true)
                {
                    ModelState.AddModelError("", "Usuario bloqueado");                // Usuario bloqueado
                ViewData["msg"] = "Usuario bloqueado";
                return View();
                }
                if (user?.password != password)
                {
                    user.intentosFallidos++;
                    _context.Update(user);
                    _context.SaveChanges();
                    if (user.intentosFallidos >= 3)              //Si alcanza los 3 intentos se bloquea la cuenta
                    {
                        user.bloqueado = true;
                        _context.Update(user);
                        _context.SaveChanges();
                        ModelState.AddModelError("", "Numero de intentos excedidos, Usuario bloqueado");
                    // Si la validación falla, agregar un mensaje de error y mostrar la página de inicio de sesión nuevamente
                    //Numero de intentos excedidos
                    ViewData["msg"] = "Numero de intentos excedidos, Usuario bloqueado";
                    return View();
                    }
                    else
                    {

                        int intentosRestantes = (3 - user.intentosFallidos);
                        // intentos restantes
                        // Si la validación falla, agregar un mensaje de error y mostrar la página de inicio de sesión nuevamente
                        ModelState.AddModelError("", "Intentos restantes: " + intentosRestantes);
                    ViewData["msg"] = "Contraseña incorrecta, Intentos restantes: " + intentosRestantes;
                    return View();
                        
                    }

                }
                    /*if(user.isAdmin == true)
                    {
                         HttpContext.Session.SetInt32("admin", user.id);
                         return RedirectToAction("Index", "Usuarios");
                    }
                    */
                HttpContext.Session.SetInt32("usuario", user.id);
                ViewData["logged"] = "ok";
                return RedirectToAction("Index", "Home"); //usuario logueado
                // Si la validación es exitosa, redirigir al usuario a la página de inicio del home banking
                
        }

        [HttpGet]
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            ViewData["logged"] = "";
            return RedirectToAction("Login", "Home");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}