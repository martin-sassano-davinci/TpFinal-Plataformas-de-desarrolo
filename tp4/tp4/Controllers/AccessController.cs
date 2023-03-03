using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using tp4.Data;
using tp4.Models;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace tp4.Controllers
{
    public class AccessController : Controller
    {
        private readonly MiContexto _context;
        private Usuario usuarioLogueado;
        public AccessController(MiContexto context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Enter(int dni, String password)
        {
            try
            {
                var msj = ViewData["msg"] = null;
                //using (MiContexto db = new MiContexto())

                //Usuario findUser = _context.usuarios.Where(u => u.nombre == user && u.password == password).FirstOrDefault();
                //var usuario =  _context.usuarios.FirstOrDefault(u => u.dni == dni && u.password == password);
               // usuarioLogueado = _context.usuarios.FirstOrDefault(u => u.dni == dni && u.password == password);


                /*  if (usuarioLogueado != null)
                  {
                      // Session["User"] = findUser.First();
                      //HttpContext.Session.SetString("usuario", user);
                      HttpContext.Session.SetInt32("usuario", usuarioLogueado.id);
                      return Content("1");
                  } else
                  {
                      return Content("-1");
                  }
                  */
                Usuario user = _context.usuarios.Where(u => u.dni == dni).FirstOrDefault();
                if (user == null)
                {
                     ViewData["msg"] = "No existe el usuario con dni " + dni;
                    return View();       // No se encontró el usuario
                    //return RedirectToAction("Login", "Access", msj);
                }
                if (user.bloqueado)
                {
                    msj = ViewData["msg"] = "El usuario con dni " + dni + " está bloqueado";
                    return Content("2");                // Usuario bloqueado
                   // return RedirectToAction("Login", "Access", msj);
                }
                if (user.password != password)
                {
                    user.intentosFallidos++;
                    _context.Update(user);
                    _context.SaveChanges();
                    if (user.intentosFallidos >= 3)              //Si alcanza los 3 intentos se bloquea la cuenta
                    {
                        user.bloqueado = true;
                        _context.Update(user);
                        _context.SaveChanges();
                        return Content("3");         
                       // msj = ViewData["msg"] = "Numero de intentos excedidos, usuario " + dni + " bloqueado";
                        //Numero de intentos excedidos
                        //return RedirectToAction("Login", "Access",msj );
                    }
                    else
                    {
                        int intentosRestantes = (3 - user.intentosFallidos);
                        msj = ViewData["msg"] = "Numero de intentos restantes: " + intentosRestantes;
                        
                        return Content("4");                    // intentos restantes
                       // return RedirectToAction("Login", "Access", msj);

                    }
                }
                this.usuarioLogueado = user;
                //HttpContext.Session.SetString("usuario", user);
                //HttpContext.Current.Session["usuario"] = usuarioLogueado;
                //Session["usuario"] = usuarioLogueado;
                HttpContext.Session.SetInt32("usuario", usuarioLogueado.id);
                return Content("0");

                //return RedirectToAction("Index", "Usuarios"); //usuario logueado

            }
            catch(Exception ex)
            {
                //return Content("Ocurrio un error: " + ex.Message);
                return View(ex.Message);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session?.Clear();
            return RedirectToAction("Index", "Access");
        }


    }
}
