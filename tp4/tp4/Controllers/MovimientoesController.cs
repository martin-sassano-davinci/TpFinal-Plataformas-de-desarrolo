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
    
    public class MovimientoesController : Controller
    {
        
        private readonly MiContexto _context;

        public MovimientoesController(MiContexto context)
        {
            _context = context;
            _context.cajas
                .Include(c => c.movimientos)
                .Include(c => c.titulares)
                .Load();
        }

        // GET: Movimientoes
        public async Task<IActionResult> Index(int? id)
        {

            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            //Movimiento? movimientoCaja = _context.movimientos.Where(m => m.id_Caja == id).
            if (user == null)
            {
                ViewData["msg"] = "No tenes permiso para acceder, por favor inicia sesion";
                return View();
            }
            if (id == null)
            {
                ViewData["msg"] = "No se encontro la caja";
                return View();
            }
            ViewBag.id = id;
            var miContexto = _context.movimientos.Where(m => m.id_Caja == id);
            return View(await miContexto.ToListAsync());
        }
        // GET: FiltrarMovimientos
        public IActionResult FiltrarMovimientos()
        {
          
            return View();
            
        }

        // POST: FiltrarMovimientos
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FiltrarMovimientos(int id, String? detalle, float? monto, DateTime fecha)
        {
            var miContexto = _context.movimientos.Where(m => m.id_Caja == id).Include(m => m.caja);
            var salida = miContexto.ToList();
            if (detalle != null)
            {
                salida = salida.Where(u => u.detalle.Contains(detalle)).ToList();

            }
            if (monto != null)
            {
                salida = salida.Where(u => u.monto >= monto).ToList();

            }
            
            if (fecha != null)
            {
                salida = salida.Where(u => u.fecha == fecha ).ToList();
            }

            return View(salida.ToList());
        }

        public async Task<IActionResult> MostrarFiltrarMovimientos()
        {
            return View();
        }
        // GET: Movimientoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.movimientos == null)
            {
                return NotFound();
            }

            var movimiento = await _context.movimientos
                .Include(m => m.caja)
                .FirstOrDefaultAsync(m => m.id == id);
            if (movimiento == null)
            {
                return NotFound();
            }

            return View(movimiento);
        }

        // GET: Movimientoes/Create
        public IActionResult Create()
        {
            ViewData["id_Caja"] = new SelectList(_context.cajas, "id", "id");
            return View();
        }

        // POST: Movimientoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,detalle,monto,fecha,id_Caja")] Movimiento movimiento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movimiento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["id_Caja"] = new SelectList(_context.cajas, "id", "id", movimiento.id_Caja);
            return View(movimiento);
        }

        // GET: Movimientoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.movimientos == null)
            {
                return NotFound();
            }

            var movimiento = await _context.movimientos.FindAsync(id);
            if (movimiento == null)
            {
                return NotFound();
            }
            ViewData["id_Caja"] = new SelectList(_context.cajas, "id", "id", movimiento.id_Caja);
            return View(movimiento);
        }

        // POST: Movimientoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,detalle,monto,fecha,id_Caja")] Movimiento movimiento)
        {
            if (id != movimiento.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movimiento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovimientoExists(movimiento.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["id_Caja"] = new SelectList(_context.cajas, "id", "id", movimiento.id_Caja);
            return View(movimiento);
        }

        // GET: Movimientoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.movimientos == null)
            {
                return NotFound();
            }

            var movimiento = await _context.movimientos
                .Include(m => m.caja)
                .FirstOrDefaultAsync(m => m.id == id);
            if (movimiento == null)
            {
                return NotFound();
            }

            return View(movimiento);
        }

        // POST: Movimientoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.movimientos == null)
            {
                return Problem("Entity set 'MiContexto.movimientos'  is null.");
            }
            var movimiento = await _context.movimientos.FindAsync(id);
            if (movimiento != null)
            {
                _context.movimientos.Remove(movimiento);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovimientoExists(int id)
        {
            return _context.movimientos.Any(e => e.id == id);
        }
    }
}
