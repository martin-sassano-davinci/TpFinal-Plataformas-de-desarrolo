using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using tp4.Data;
using tp4.Models;

namespace tp4.Controllers
{
    
    public class UsuarioCajasController : Controller
    {
        
        private readonly MiContexto _context;

        public UsuarioCajasController(MiContexto context)
        {
            _context = context;
        }

        // GET: UsuarioCajas
        public async Task<IActionResult> Index()
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else if (user.isAdmin == true)
            {
                var miContexto = _context.UsuarioCaja.Include(u => u.caja).Include(u => u.usuario);
                return View(await miContexto.ToListAsync());
            }
            else
            {
                var miContexto1 = _context.UsuarioCaja.Include(u => u.caja).Include(u => u.usuario).Where(u => u.idUsuario == sesion);
                return View(await miContexto1.ToListAsync());
            }

           // var miContexto = _context.UsuarioCaja.Include(u => u.caja).Include(u => u.usuario);
            //return View(await miContexto.ToListAsync());
        }

    }
}
