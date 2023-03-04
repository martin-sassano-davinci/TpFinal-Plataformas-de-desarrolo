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
    
    public class CajaDeAhorroesController : Controller
    {
        private readonly MiContexto _context;
        
        public CajaDeAhorroesController(MiContexto context)
        {
            _context = context;
            _context.cajas
                    .Include(c => c.movimientos)
                    .Include(c => c.titulares)
                    .Load();

        }
       
        // GET: CajaDeAhorroes
        public async Task<IActionResult> Index()
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else if(user.isAdmin == true)
            {
                return View(await _context.cajas.ToListAsync());
            } else
            {
              user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
              return View(await _context.cajas.Where(c => c.titulares.Contains(user)).ToListAsync());
            }
        }

        

        // GET: CajaDeAhorroes/Create
        public IActionResult Create()
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            Random random = new Random();
            int nuevoCbu = random.Next(100000000, 999999999);
            while (_context.cajas.Any(caja => caja.cbu == nuevoCbu))
            {  // Mientras haya alguna caja con ese CBU se crea otro CBU
                nuevoCbu = random.Next(100000000, 999999999);
                Debug.WriteLine("El CBU generado ya existe, creado uno nuevo...");
            }
            //Ahora sí lo agrego en la lista
            CajaDeAhorro nuevo = new CajaDeAhorro(nuevoCbu, user);
            _context.cajas.Add(nuevo);
            _context.Update(user);
            _context.SaveChanges();
            return RedirectToAction("Index", "CajaDeAhorroes");
             //return View();
        }

        // POST: CajaDeAhorroes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,cbu,saldo")] CajaDeAhorro cajaDeAhorro)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            if (ModelState.IsValid)
            {
                _context.Add(cajaDeAhorro);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cajaDeAhorro);
        }

        // GET: CajaDeAhorroes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            if (id == null || _context.cajas == null)
            {
                return NotFound();
            }

            var cajaDeAhorro = await _context.cajas
                .FirstOrDefaultAsync(m => m.id == id);
            if (cajaDeAhorro == null)
            {
                return NotFound();
            }

            return View(cajaDeAhorro);
        }

        // POST: CajaDeAhorroes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            try
            {
                CajaDeAhorro? cajaARemover = _context.cajas.Where(caja => caja.id == id).FirstOrDefault();
                if (cajaARemover == null)
                {
                    ViewData["msg"] = "No se encontró la caja";
                    return View();
                }
                if (cajaARemover.saldo != 0)
                {
                    ViewData["msg"] = "No se puede eliminar una caja con saldo";
                    return View();
                }
                foreach (Usuario titular in cajaARemover.titulares) //Itero entre los titulares de la caja de ahorro
                {
                    titular.cajas.Remove(cajaARemover);  //Saco la caja de ahorro de los titulares.
                }
                _context.cajas.Remove(cajaARemover); //Saco la caja de ahorro del banco
                _context.SaveChanges();

                return RedirectToAction("Index", "CajaDeAhorroes");
            }
            catch (Exception ex)
            {
                ViewData["msg"] = "Error: " + ex.Message;
                return View();
            }

        }

        private bool CajaDeAhorroExists(int id)
        {
            return _context.cajas.Any(e => e.id == id);
        }

        //////////////////////////
        // GET: CajaDeAhorroes/Create
        // GET: Usuarios/Create
        public IActionResult AgregarUsuarioACaja()
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarUsuarioACaja(int id, int dni)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            try
            {
                CajaDeAhorro? caja = _context.cajas.Where(caja => caja.id == id).FirstOrDefault();
                Usuario? userAdd = _context.usuarios.Where(usuario => usuario.dni == dni).FirstOrDefault();
                if (userAdd == null)
                {
                    //No se encontró usuario con este DNI en la lista de Usuarios del Banco
                    ViewData["msg"] = "No se encontró usuario con este DNI";
                    return View();
                }
                if (caja == null)
                {  //No se encontró la caja de ahorro con ese ID
                    ViewData["msg"] = "No se encontró la caja de ahorro";                  
                    return View();
                }

                if (caja.titulares.Contains(userAdd))
                {
                    //El usuario ya posee esta caja de ahorro en el sistema.
                    ViewData["msg"] = "El usuario ya posee esta caja de ahorro en el sistema";
                    return View();
                }
                caja.titulares.Add(userAdd);
                userAdd.cajas.Add(caja);
                _context.Update(caja);
                _context.Update(userAdd);
                _context.SaveChanges();
                return RedirectToAction("Index", "CajaDeAhorroes");
            }
            catch (Exception ex)
            {
                ViewData["msg"] = "Error: " + ex.Message;
                return View();
            }
        }
        ///////////////////////
        // GET: CajaDeAhorroes/Delete/5
        public async Task<IActionResult> EliminarUsuarioDeCaja(int? id)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            if (id == null || _context.cajas == null)
            {
                return NotFound();
            }

            var cajaDeAhorro = await _context.cajas
                .FirstOrDefaultAsync(m => m.id == id);
            if (cajaDeAhorro == null)
            {
                return NotFound();
            }

            return View(cajaDeAhorro);
        }

        // POST: CajaDeAhorroes/Delete/5
        [HttpPost, ActionName("EliminarUsuarioDeCaja")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUsuarioDeCaja(int? id, int dni) // agregar persistencia
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            try
            {
                Usuario titular = _context.usuarios.Where(usuario => usuario.dni == dni).FirstOrDefault();
                CajaDeAhorro? caja = _context.cajas.Where(caja => caja.id == id).FirstOrDefault();
                if (titular == null)
                {
                    //No se encontró usuario con este DNI en la lista de Usuarios del Banco
                    ViewData["msg"] = "No se encontró usuario con este DNI";
                    //return View("Index", _context.cajas.);
                    return View("Index",  await _context.cajas.Where(c => c.titulares.Contains(user)).ToListAsync());
                }
                if (caja == null)
                {
                    //No se encontró la caja de ahorro en la lista de cajas de ahorro
                    ViewData["msg"] = "No se encontró la caja de ahorro";
                    return View();
                }
                if (!caja.titulares.Contains(titular) || caja.titulares.Count < 2)
                {
                    // El usuario no se pudo eliminar de la lista en el sistema
                    ViewData["msg"] = "El usuario no se pudo eliminar de la lista en el sistema";
                    return View();
                }
                caja.titulares.Remove(titular);
                titular.cajas.Remove(caja);
                _context.Update(caja);
                _context.Update(titular);
                //contexto.usuarios.Remove(titular);
                //contexto.cajas.Remove(caja);
                _context.SaveChanges();
                return RedirectToAction("Index", "CajaDeAhorroes");
            }
            catch (Exception ex)
            {
                ViewData["msg"] = "Error: " + ex.Message;
                return View();
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


        // GET: CajaDeAhorroes depositar
        public async Task<IActionResult> Depositar(int? id)
        {
           
            
                var sesion = HttpContext.Session.GetInt32("usuario");
                Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
                if (user == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                if (id == null || _context.cajas == null)
                {
                    return NotFound();
                }

                var cajaDeAhorro = await _context.cajas
                    .FirstOrDefaultAsync(m => m.id == id);
                if (cajaDeAhorro == null)
                {
                    return NotFound();
                }

                return View(cajaDeAhorro);
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Depositar(int id, float monto) //Probar si se guarda 2 veces
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            CajaDeAhorro cajaDestino = _context.cajas.Where(caja => caja.id == id).FirstOrDefault();
            if (cajaDestino == null)
            {
                //Si no se encuentra la Caja    
                ViewData["msg"] = "No se encontró la caja" ;
                return View();
            }
            if (monto <= 0)
            {
                ViewData["msg"] = "Error al retirar, el monto tiene que ser mayor a 0";
                return View();
            }
            cajaDestino.saldo += monto;
            _context.Update(cajaDestino);
            this.altaMovimiento(cajaDestino, "Deposito", monto);
            _context.SaveChanges();
            ViewData["msg"] = "Deposito realizado correctamente";
            return View();
        }


        // GET: CajaDeAhorroes retirar
        public async Task<IActionResult> Retirar(int? id)
        {


            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            if (id == null || _context.cajas == null)
            {
                return NotFound();
            }

            var cajaDeAhorro = await _context.cajas
                .FirstOrDefaultAsync(m => m.id == id);
            if (cajaDeAhorro == null)
            {
                return NotFound();
            }

            return View(cajaDeAhorro);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Retirar(int id, float monto)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            CajaDeAhorro cajaSeleccionada = _context.cajas.Where(caja => caja.id == id).FirstOrDefault(); 
            if (cajaSeleccionada == null)
            {
               //No se encontró la caja
                ViewData["msg"] = "No se encontró la caja";
                return View();
            }
            if (cajaSeleccionada.saldo < monto)
            {
                 //El saldo es menor al monto que se desea retirar
                ViewData["msg"] = "El saldo es menor al monto que desea retirar";
                return View();
            }
            if(monto <= 0)
            {
                ViewData["msg"] = "Error al retirar, el monto tiene que ser mayor a 0";
                return View();
            }
            cajaSeleccionada.saldo -= monto;
            _context.Update(cajaSeleccionada);
            this.altaMovimiento(cajaSeleccionada, "Retiro", monto);
            _context.SaveChanges();
            ViewData["msg"] = "Retiro realizado correctamente";
            return View();
        }

        // GET: CajaDeAhorroes Transferir
        public async Task<IActionResult> Transferir(int? id)
        {


            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            if (id == null || _context.cajas == null)
            {
                return NotFound();
            }

            var cajaDeAhorro = await _context.cajas
                .FirstOrDefaultAsync(m => m.id == id);
            if (cajaDeAhorro == null)
            {
                return NotFound();
            }

            return View(cajaDeAhorro);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transferir(int id, int cbuDestino, float monto)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            try
            {
                CajaDeAhorro cajaOrigen = _context.cajas.Where(caja => caja.id == id).FirstOrDefault();
                CajaDeAhorro cajaDestino = _context.cajas.Where(caja => caja.cbu == cbuDestino).FirstOrDefault();
                if (cajaDestino == null)
                {
                    //No se encontró la caja
                    ViewData["msg"] = "No se encontró la caja";
                    return View();
                }
                if (cajaOrigen.saldo < monto)
                {
                    ViewData["msg"] = "El saldo es menor al monto que desea transferir";
                    return View();
                }
                if (monto <= 0)
                {
                    ViewData["msg"] = "Error al retirar, el monto tiene que ser mayor a 0";
                    return View();
                }
                if(cajaOrigen == cajaDestino)
                {
                    ViewData["msg"] = "No se puede transferir a la misma cuenta";
                    return View();
                }
                cajaOrigen.saldo -= monto;
                this.altaMovimiento(cajaOrigen, "Transferencia realizada", monto);
                cajaDestino.saldo += monto;
                this.altaMovimiento(cajaDestino, "Transferencia recibida", monto);
                ViewData["msg"] = "Transferencia realizada correctamente";
                return View();
            }
            catch (Exception ex)
            {
                ViewData["msg"] = "Error: " + ex.Message;
                return View();
            }

        }
       
    }
}
