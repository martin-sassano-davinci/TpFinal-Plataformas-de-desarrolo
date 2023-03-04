using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using tp4.Data;
using tp4.Models;

namespace tp4.Controllers
{
    
    public class MovimientoesController : Controller
    {
        
        private readonly MiContexto _context;

        public MovimientoesController(MiContexto context)
        {
            _context = context;
            _context = context;
            _context.usuarios
                   .Include(u => u.tarjetas)
                   .Include(u => u.cajas)
                   .Include(u => u.pf)
                   .Include(u => u.pagos)
                   .Load();
            _context.cajas
                .Include(c => c.movimientos)
                .Include(c => c.titulares)
                .Load();
            _context.tarjetas.Load();
            _context.pagos.Load();
            _context.movimientos.Load();
            _context.plazosFijos.Load();
        }

        // GET: Movimientoes
        public async Task<IActionResult> Index(int? id)
        {

            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            if (id == null)
            {
                ViewData["msg"] = "No se encontro la caja";
                return View();
            }
            ViewBag.idCajaIn = id; //asp - route - id = "@item.id"
            
            var miContexto = _context.movimientos.Where(m => m.id_Caja == id);
            return View(await miContexto.ToListAsync());
        }

        // GET: Movimientos
        public IActionResult FiltrarMovimientos(int? id, string detalle, float? monto, DateTime? fecha, bool busqueda = false)
        {
            var movimientos = _context.movimientos.Where(m => m.id_Caja == id).AsQueryable();
            

            if (busqueda && !string.IsNullOrEmpty(detalle))
            {
                    movimientos = movimientos.Where(m => m.detalle.Contains(detalle));
 
            }

            if (busqueda && monto.HasValue)
            {

                    movimientos = movimientos.Where(m => m.monto == monto.Value);
             
            }

            if (busqueda && fecha.HasValue)
            {
                
                    movimientos = movimientos.Where(m => m.fecha.Date == fecha.Value.Date);             
            }
            if (!busqueda)
            {
                movimientos = movimientos.Take(0); // no devuelve ningún movimiento
            }
           
            return View(movimientos.ToList());
        }

        [HttpPost]
        public IActionResult FiltrarMovimientos(string detalle, float? monto, DateTime? fecha, string buscar)
        {
            return RedirectToAction("FiltrarMovimientos", new { detalle = detalle, monto = monto, fecha = fecha, busqueda = true });
        }


        private bool MovimientoExists(int id)
        {
            return _context.movimientos.Any(e => e.id == id);
        }
    }
}
