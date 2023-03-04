using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    
    public class PagoesController : Controller
    {
        
        private readonly MiContexto _context;

        public PagoesController(MiContexto context)
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

        // GET: Pagoes
        public async Task<IActionResult> Index()
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario usuarioLogeado = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();

            if (usuarioLogeado == null)
            {
                return RedirectToAction("Login", "Home");
            }
            if(usuarioLogeado.isAdmin == true)
            {
                var admin = _context.pagos.Include(p => p.usuario);
                return View(await admin.ToListAsync());
            }
            var miContexto = _context.pagos.Include(p => p.usuario).Where(u => u.id_usuario == sesion);
            return View(await miContexto.ToListAsync());
        }

        // GET: Pagoes/Create
        public IActionResult Create()
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        // POST: Pagoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Nombre, float Monto/*[Bind("id,id_usuario,nombre,monto,pagado,metodo")] Pago pago*/)
  
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario usuarioLogeado = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();

            if (usuarioLogeado == null)
            {
                return RedirectToAction("Login", "Home");
            }
                Pago nuevoPago = new Pago(usuarioLogeado, Nombre, Monto);
                _context.pagos.Add(nuevoPago);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            
                /*if (ModelState.IsValid)
                {
                    _context.Add(pago);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                ViewData["id_usuario"] = new SelectList(_context.usuarios, "id", "apellido", pago.id_usuario);
                return View(pago);*/
            }

        // GET: Pagoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario usuarioLogeado = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();

            if (usuarioLogeado == null)
            {
                return RedirectToAction("Login", "Home");
            }
            if (id == null || _context.pagos == null)
            {
                return NotFound();
            }

            var pago = await _context.pagos.FindAsync(id);
            if (pago == null)
            {
                return NotFound();
            }
            ViewData["id_usuario"] = new SelectList(_context.usuarios, "id", "apellido", pago.id_usuario);
            return View(pago);
        }

        // POST: Pagoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,id_usuario,nombre,monto,pagado,metodo")] Pago pago)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario usuarioLogeado = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();

            if (usuarioLogeado == null)
            {
                return RedirectToAction("Login", "Home");
            }
            if (id != pago.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pago);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PagoExists(pago.id))
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
            ViewData["id_usuario"] = new SelectList(_context.usuarios, "id", "apellido", pago.id_usuario);
            return View(pago);
        }

        // GET: Pagoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario usuarioLogeado = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();

            if (usuarioLogeado == null)
            {
                return RedirectToAction("Login", "Home");
            }
            if (id == null || _context.pagos == null)
            {
                return NotFound();
            }

            var pago = await _context.pagos
                .Include(p => p.usuario)
                .FirstOrDefaultAsync(m => m.id == id);
            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // POST: Pagoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario usuarioLogeado = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();

            if (usuarioLogeado == null)
            {
                return RedirectToAction("Login", "Home");
            }
            Pago pagoABorrar = _context.pagos.Where(pago => pago.id == id).FirstOrDefault();
            if (pagoABorrar == null)
            {
                ViewData["msg"] = "No se encontro el pago";
                return View();
            }
            if (!pagoABorrar.pagado)
            {
                ViewData["msg"] = "No se puede eliminar un pago que no fue realizado";
                return View();
            }
            _context.pagos.Remove(pagoABorrar);
            pagoABorrar.usuario.pagos.Remove(pagoABorrar);
            _context.Update(pagoABorrar.usuario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        
        private bool PagoExists(int id)
        {
            return _context.pagos.Any(e => e.id == id);
        }
        public IActionResult PagarPago(int id)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            Pago buscarPago = _context.pagos.Where(p => p.id == id).FirstOrDefault();
            if (buscarPago == null)
            {
                ViewData["msg"] = "No se encontró el pago";
                return View();
            }

            ViewBag.cajasCbuUsu = user.cajas.Where(c => c.saldo >= buscarPago.monto).Select(c => c.cbu).ToList();
            ViewBag.cajasIdUsu = user.cajas.Where(c => c.saldo >= buscarPago.monto).Select(c => c.id).ToList();
            ViewBag.cajasSaldoUsu = user.cajas.Where(c => c.saldo >= buscarPago.monto).Select(c => c.saldo).ToList();
            ViewBag.pagoMonto = buscarPago.monto;

            Tarjeta buscarTarjeta = _context.tarjetas.Where(tarjeta => tarjeta.id_titular == sesion).FirstOrDefault();
            ViewBag.tarjetaIdUsu = user.tarjetas.Where(t => (t.limite - t.consumo) >= buscarPago.monto).Select(t => t.id).ToList();
            ViewBag.tarjetaUsu = user.tarjetas.Where(t => (t.limite - t.consumo) >= buscarPago.monto).Select(t => t.numero).ToList();
            ViewBag.tarjetaConsumoUsu = user.tarjetas.Where(t => (t.limite - t.consumo) >= buscarPago.monto).Select(t => t.consumo).ToList();
            ViewBag.tarjetaLimiteUsu = user.tarjetas.Where(t => (t.limite - t.consumo) >= buscarPago.monto).Select(t => t.limite).ToList();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarPago(int id, int? cajaId, int? tarjetaId)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario usuarioLogeado = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();

            if (usuarioLogeado == null)
            {
                return RedirectToAction("Login", "Home");
            }
            Pago pago = _context.pagos.Where(pago => pago.id == id).FirstOrDefault();
            if (pago == null)
            {
                //No se encuentra el pago
                ViewData["msg"] = "No se encuentró el pago";
                return View();
            }
            if (pago.monto == 0)
            {
                ViewData["msg"] = "No se puede abonar un pago con un monto de 0";
                return View();
            }
            if (pago.pagado)
            {
                //ya esta pagado
                ViewData["msg"] = "El pago que seleccionó ya está pago";
                return View();
            }
            CajaDeAhorro caja = _context.cajas.Where(caja => caja.id == cajaId).FirstOrDefault();
            Tarjeta tarjeta = _context.tarjetas.Where(t => t.id == tarjetaId && t.id_titular == sesion).FirstOrDefault();

            if (caja != null)
            {
                if (caja.saldo < pago.monto)
                {
                    //No se encuantra caja
                    ViewData["msg"] = "No se encontró la caja";
                    return View();
                }
                caja.saldo -= pago.monto;
                this.modificarPago(pago);
                this.altaMovimiento(caja, "Pago de " + pago.nombre, pago.monto);
                pago.metodo = "Caja de ahorro";
                _context.Update(caja);
                _context.Update(pago);
                _context.SaveChanges();

                ViewData["msg"] = "Pago abonado correctamente";
                return RedirectToAction("Index", "Pagoes");
            }
            else if (tarjeta != null)
            {
                if ((tarjeta.limite - tarjeta.consumo) < pago.monto)
                {
                    //No tiene saldo
                    ViewData["msg"] = "El monto del pago supera a la tarjeta que seleccionó";
                    return View();
                }
                tarjeta.consumo += pago.monto;
                this.modificarPago(pago);
                pago.metodo = "Tarjeta";
                _context.Update(tarjeta);
                _context.Update(pago);
                _context.SaveChanges();
                ViewData["msg"] = "Pago abonado correctamente";
                return RedirectToAction("Index", "Pagoes");
            }
            else
            {
                //No se encuentra ni la tarjeta ni la caja
                ViewData["msg"] = "No se encontró ni la tarjeta ni la caja";
                return View();
            }
        }
        public int modificarPago(Pago pago)
        {
          
            try
            {
                // Pago pagoAModificar = buscarPago(pago);
                if (pago == null)
                {
                    return 1;
                }
                pago.pagado = true;
                _context.Update(pago);
                _context.SaveChanges();
                return 0;
            }
            catch
            {
                return 2;
            }
        }
        public bool altaMovimiento(CajaDeAhorro Caja, string Detalle, float Monto)
        {
            try
            {
                Movimiento movimientoNuevo = new Movimiento(Caja, Detalle, Monto);
                _context.movimientos.Add(movimientoNuevo);
                Caja.movimientos.Add(movimientoNuevo);
                _context.Update(Caja);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
