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
    
    public class PlazoFijoesController : Controller
    {
        
        private readonly MiContexto _context;
        
        public PlazoFijoesController(MiContexto context)
        {
            _context = context;
        }

        // GET: PlazoFijoes
        public async Task<IActionResult> Index()
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            if(user.isAdmin == true)
            {
                var admin = _context.plazosFijos.Include(p => p.titular);
                return View(await admin.ToListAsync());
            }
            var miContexto = _context.plazosFijos.Include(p => p.titular).Where(tit => tit.id_titular == sesion);
            return View(await miContexto.ToListAsync());

        }

        

        // GET: PlazoFijoes/Create
        public IActionResult Create()
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario usuarioLogeado = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();

            if (usuarioLogeado == null)
            {
                return RedirectToAction("Login", "Home");
            }

            ViewData["id_titular"] = new SelectList(_context.usuarios, "id", "apellido");
            return View();
            
        }
        // POST: PlazoFijoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int cbu, float monto, float tasa /*[Bind("id,monto,fechaIni,fechaFin,tasa,pagado,id_titular,cbu")] PlazoFijo plazoFijo*/)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario usuarioLogeado = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
           
            if (usuarioLogeado == null)
            {
                return RedirectToAction("Login", "Home");
            }
            try
            {
                if (monto < 1000)
                {
                    //Monto insuficiente para crear el pf
                    ViewData["msg"] = "Monto insuficiente para crear el plazo fijo";
                    return View();
                }
                CajaDeAhorro? caja = _context.cajas.Where(caja => caja.cbu == cbu).FirstOrDefault();
                if (caja == null)
                {
                    //No se encontró la caja
                    ViewData["msg"] = "No se encontró la caja";
                    return View();
                }
                if (caja.saldo < monto)
                {
                    bool i = false;
                    Usuario user = caja.titulares.FirstOrDefault();
                    foreach (CajaDeAhorro cajaAux in user.cajas)
                    {
                        if (cajaAux.saldo > monto)
                        {
                            caja = cajaAux;
                            i = true;
                        }
                    }
                    if (i == false)
                    {
                        //Fondos insuficientes
                        ViewData["msg"] = "Fondos insuficientes";
                        return View();
                    }

                }
                caja.saldo -= monto;
                this.altaMovimiento(caja, "Alta plazo fijo", monto);
                PlazoFijo nuevoPlazoFijo = new PlazoFijo(usuarioLogeado, monto, DateTime.Now.AddMonths(1), tasa, caja.cbu);
                _context.plazosFijos.Add(nuevoPlazoFijo);
                _context.Update(caja);
                _context.Update(usuarioLogeado);
                //_context.SaveChanges();
                //return RedirectToAction("Index", "PlazoFijoes"); ;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewData["msg"] = "Error: " + ex.Message;
                return View();
            }
        }
           
        

        
        // GET: PlazoFijoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            if (id == null || _context.plazosFijos == null)
            {
                return NotFound();
            }

            var plazoFijo = await _context.plazosFijos
                .Include(p => p.titular)
                .FirstOrDefaultAsync(m => m.id == id);
            if (plazoFijo == null)
            {
                return NotFound();
            }

            return View(plazoFijo);
        }

        // POST: PlazoFijoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");

            }
            try
            {
                PlazoFijo pFijo = _context.plazosFijos.Where(pf => pf.id == id).FirstOrDefault(); ;
                if (pFijo == null)
                {
                    ViewData["msg"] = "No se encontró plazo fijo";
                    return View();
                }
                if (!pFijo.pagado || DateTime.Now < pFijo.fechaFin.AddMonths(1))
                {
                    ViewData["msg"] = "El plazo fijo todavía no esta pago";
                    return View();
                }
                pFijo.titular.pf.Remove(pFijo);
                _context.Update(pFijo.titular);
                _context.plazosFijos.Remove(pFijo);
                await _context.SaveChangesAsync();
               return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewData["msg"] = "Error: " + ex.Message;
                return View();
            }

        }

        private bool PlazoFijoExists(int id)
        {
            return _context.plazosFijos.Any(e => e.id == id);
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
        public async Task<IActionResult> PagarPlazosFijos(PlazoFijo pFijo)// Hay que buscar la manera de pasarle una caja xq ya no tiene PF asociada
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");

            }
            DateTime fechaIni = pFijo.fechaIni;
            DateTime fechaFin = pFijo.fechaFin;
            if (DateTime.Now.CompareTo(fechaFin) <= 0 && pFijo.pagado == false) //Esto no se si va a alreves
            {
                double cantDias = (fechaFin - fechaIni).TotalDays;
                float montoFinal = (pFijo.monto + pFijo.monto * (float)(90.0 / 365.0) * (float)cantDias);
                decimal bar = Convert.ToDecimal(montoFinal);
                montoFinal = (float)Math.Round(bar, 2);//redondeo a 2 decimales
                CajaDeAhorro caja = _context.cajas.Where(c => c.cbu == pFijo.cbu).FirstOrDefault();
                caja.saldo += montoFinal;
                pFijo.pagado = true;
                _context.Update(caja);
                _context.Update(pFijo);
                _context.SaveChanges();
                altaMovimiento(caja, "Pago plazo fijo", montoFinal);
                return RedirectToAction("Index", "PlazoFijoEs");
            }
            return View();
        }

    }
}
