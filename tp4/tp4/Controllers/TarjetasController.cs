using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public class TarjetasController : Controller
    {

        private readonly MiContexto _context;

        public TarjetasController(MiContexto context)
        {
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

        // GET: Tarjetas
        public async Task<IActionResult> Index()
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Tarjeta tarjetasUser = _context.tarjetas.Where(t => t.id_titular == sesion).FirstOrDefault();
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                ViewData["msg"] = "No tiene permiso para ver esta pagina, por favor inicie sesion";
                return View();
            }
            if (tarjetasUser == null)
            {
                ViewData["msg"] = "El usuario con dni " + user.dni + " no posee tarjetas a su nombre";
                return View();
            }
            if (user.isAdmin == true)
            {
                var miContextoAdmin = _context.tarjetas.Include(t => t.titular);
                return View(await miContextoAdmin.ToListAsync());
            }
            var miContexto = _context.tarjetas.Include(t => t.titular).Where(t => t.id_titular == sesion);
            return View(await miContexto.ToListAsync());


        }

        // GET: Tarjetas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null || user.isAdmin == false)
            {
                ViewData["msg"] = "No tiene permiso para ver esta pagina";
                return View();
            }
            if (id == null || _context.tarjetas == null)
            {
                return NotFound();
            }

            var tarjeta = await _context.tarjetas
                .Include(t => t.titular)
                .FirstOrDefaultAsync(m => m.id == id);
            if (tarjeta == null)
            {
                return NotFound();
            }

            return View(tarjeta);
        }

        // GET: Tarjetas/Create
        public IActionResult Create()
        {
            try
            {

                var sesion = HttpContext.Session.GetInt32("usuario");
                Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
                if (user == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                Random random = new Random();
                int nuevoNumero = random.Next(100000000, 999999999);
                while (_context.tarjetas.Any(tarjeta => tarjeta.numero == nuevoNumero))
                {  // Mientras haya alguna tarjeta con ese numero se crea otro numero
                    nuevoNumero = random.Next(100000000, 999999999);
                    Debug.WriteLine("El número de tarjeta generado ya existe, creado uno nuevo...");
                }
                int nuevoCodigo = random.Next(100, 999); //Creo un codigo de tarjeta aleatorio
                Tarjeta nuevo = new Tarjeta(user.id, nuevoNumero, nuevoCodigo, 20000, 0);
                nuevo.titular = user;
                _context.tarjetas.Add(nuevo);
                _context.Update(user);
                _context.SaveChanges();
                return RedirectToAction("Index", "Tarjetas");
            }
            catch (Exception ex)
            {
                ViewData["msg"] = "Error: " + ex.Message;
                return View();
            }
            //ViewData["id_titular"] = new SelectList(_context.usuarios, "id", "apellido");
            // return View();
        }

        // POST: Tarjetas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,id_titular,numero,codigoV,limite,consumo")] Tarjeta tarjeta)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tarjeta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["id_titular"] = new SelectList(_context.usuarios, "id", "apellido", tarjeta.id_titular);
            return View(tarjeta);
        }

        // GET: Tarjetas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.tarjetas == null)
            {
                return NotFound();
            }

            var tarjeta = await _context.tarjetas.FindAsync(id);
            if (tarjeta == null)
            {
                return NotFound();
            }
            ViewData["id_titular"] = new SelectList(_context.usuarios, "id", "apellido", tarjeta.id_titular);
            return View(tarjeta);
        }

        // POST: Tarjetas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, float limite/* [Bind("id,id_titular,numero,codigoV,limite,consumo")] Tarjeta tarjeta*/)
        {
            try
            {
                var sesion = HttpContext.Session.GetInt32("usuario");
                Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
                if (user == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                var buscarTarjeta = _context.tarjetas.Where(tarjeta => tarjeta.id == id).FirstOrDefault();
                Tarjeta? TarjetaAModificar = buscarTarjeta;
                if (TarjetaAModificar == null)
                {
                    ViewData["msg"] = "No se encontro la tarjeta";
                    return View();
                }
                TarjetaAModificar.limite = limite;
                _context.Update(TarjetaAModificar);
                _context.SaveChanges();
                return RedirectToAction("Index", "Tarjetas");
                
            }
            catch (Exception ex)
            {
                ViewData["msg"] = "Error: " + ex.Message;
                return View();
            }
            //return contexto.tarjetas.Where(tarjeta => tarjeta.id == Id).FirstOrDefault()
            /*
            if (id != tarjeta.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tarjeta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TarjetaExists(tarjeta.id))
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
            ViewData["id_titular"] = new SelectList(_context.usuarios, "id", "apellido", tarjeta.id_titular);
            return View(tarjeta);
            */
        }

        // GET: Tarjetas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {

            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            if (id == null || _context.tarjetas == null)
            {
                return NotFound();
            }

            var tarjeta = await _context.tarjetas
                .Include(t => t.titular)
                .FirstOrDefaultAsync(m => m.id == id);
            if (tarjeta == null)
            {
                return NotFound();
            }

            return View(tarjeta);

        }

        // POST: Tarjetas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            try
            {
                var sesion = HttpContext.Session.GetInt32("usuario");
                Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
                if (user == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                var buscarTarjeta = _context.tarjetas.Where(tarjeta => tarjeta.id == id).FirstOrDefault();
                Tarjeta? tarjetaARemover = buscarTarjeta;
                if (tarjetaARemover == null)
                {
                    ViewData["msg"] = "No se encontro la tarjeta";
                    return View();
                }
                if (tarjetaARemover.consumo != 0) // La condición para eliminar es que no tenga consumos sin pagar.
                {
                    ViewData["msg"] = "No se puede eliminar esta tarjeta ya que posee un consumo sin pagar";
                    return View();
                }
                _context.tarjetas.Remove(tarjetaARemover); //Borro la tarjeta de la lista de tarjetas del Banco
                tarjetaARemover.titular.tarjetas.Remove(tarjetaARemover);//Borro la tarjeta de la lista de tarjetas del usuario.
                _context.Update(tarjetaARemover.titular);
                _context.SaveChanges();
                return RedirectToAction("Index", "Tarjetas");
            }
            catch (Exception ex)
            {
                ViewData["msg"] = "Error: " + ex.Message;
                return View();
            }
        }

        /* if (_context.tarjetas == null)
        {
            return Problem("Entity set 'MiContexto.tarjetas'  is null.");
        }
        var tarjeta = await _context.tarjetas.FindAsync(idTarjeta);
        if (tarjeta != null)
        {
            _context.tarjetas.Remove(tarjeta);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
        */

        private bool TarjetaExists(int id)
        {
            return _context.tarjetas.Any(e => e.id == id);
        }
        public IActionResult PagarTarjeta(int id)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            Tarjeta buscarTarjeta = _context.tarjetas.Where(tarjeta => tarjeta.id == id).FirstOrDefault();
            if(buscarTarjeta == null)
            {
                ViewData["msg"] = "No se encontró la tarjeta";
                return View();
            }

            ViewBag.cajasCbuUsuario = user.cajas.Where(c=> c.saldo >= buscarTarjeta.consumo).Select(c =>c.cbu).ToList();
            ViewBag.cajasSaldoUsuario = user.cajas.Where(c => c.saldo >= buscarTarjeta.consumo).Select(c => c.saldo).ToList();
            ViewBag.tarjetaConsumo = buscarTarjeta.consumo;
            
            return View();  
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarTarjeta(int id, int cbu)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            CajaDeAhorro? caja = _context.cajas.Where(caja => caja.cbu == cbu).FirstOrDefault(); ;
            Tarjeta? tarjeta = _context.tarjetas.Where(tarjeta => tarjeta.id == id).FirstOrDefault();

            if (tarjeta == null)
            {
                //no se ecnontro tarjeta
                ViewData["msg"] = "No se encontró la tarjeta";
                return View();
            }
            if(tarjeta.consumo == 0)
            {
                ViewData["msg"] = "No hay consumo pendiente de pago";
                return View();
            }
            if (caja == null)
            {
                //no se encontro caja
                ViewData["msg"] = "No se encontró la caja";
                return View();
            }

            if (caja.saldo < tarjeta.consumo)
            {
                //no tiene saldo suficiente
                ViewData["msg"] = "Saldo insuficiente, por favor intente con otro cbu";
                return View();
            }
            caja.saldo -= tarjeta.consumo;
            this.altaMovimiento(caja, "Pago de Tarjeta " + tarjeta.numero, tarjeta.consumo);
            tarjeta.consumo = 0;
            _context.Update(tarjeta);
            _context.Update(caja);
           // if (caja.saldo > 100)
           // {
           //     caja.saldo -= 100;
           // }
          //  else
          //  {
           //     tarjeta.consumo += 100;
           // }
           // _context.Update(caja);
           // _context.Update(tarjeta);
            _context.SaveChanges();
            ViewData["msg"] = "Tarjeta pagada correctamente";
            return RedirectToAction("Index", "Tarjetas");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
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