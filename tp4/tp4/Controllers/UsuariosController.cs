using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using tp4.Data;
using tp4.Models;

namespace tp4.Controllers
{
    
    public class UsuariosController : Controller
    {
       
        private readonly MiContexto _context;
        
        public UsuariosController(MiContexto context)
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

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            //funca
            //usuarioLogueado = HttpContext.Session.GetString();
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            //var admin = HttpContext.Session.GetInt32("admin");

            if (sesion != null && user.isAdmin == true)
            {
                return View(await _context.usuarios.ToListAsync());
            } else if (sesion != null)
            {
                return View(await _context.usuarios.Where(u => u.id == sesion).ToListAsync());
                
            } else
            {
                return RedirectToAction("Login", "Home");
            }
            
           
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.usuarios == null)
            {
                return NotFound();
            }

            var usuario = await _context.usuarios
                .FirstOrDefaultAsync(m => m.id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Nombre, string Apellido, int Dni, string Mail, string Password/*[Bind("id,dni,nombre,apellido,mail,intentosFallidos,bloqueado,password,isAdmin")] Usuario usuario*/)
            
        {
                    try
                    {
                        Usuario nuevo = new Usuario(Dni, Nombre, Apellido, Mail, Password, 0, false, false);
                    _context.usuarios.Add(nuevo);
                    await _context.SaveChangesAsync();
                ViewData["msg"] = "Usuario creado con exito, por favor inicia sesion para acceder al Home Banking";
                return View();
            }
                    catch
                    {
                ViewData["msg"] = "Error al crear usuario";
                return View();
            }
                
 
                /*
                if (ModelState.IsValid)
                {
                    _context.Add(usuario);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(usuario);
                */
            }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.usuarios == null)
            {
                return NotFound();
            }

            var usuario = await _context.usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int dni, string nombre, string apellido, string mail, int intentosFallidos, bool bloqueado, string password, bool isAdmin )
        {
            /*[Bind("id,dni,nombre,apellido,mail,intentosFallidos,bloqueado,password,isAdmin")] Usuario usuario*/
            /*
            if (id != usuario.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.id))
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
            return View(usuario);
            @Html.DisplayNameFor(model => model.isAdmin)
            */



            try
            {
                //Usuario usuarioAModificar = _context.usuarios.Where(u => u.id == id).FirstOrDefault();
                var sesion = HttpContext.Session.GetInt32("usuario");
                Usuario? usuarioAModificar = await _context.usuarios.FindAsync(id);
                Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
                if (usuarioAModificar == null)
                    {
                        ViewData["msg"] = "No se encontro el usuario";
                        return View();
                    }
                if(user == null)
                {
                    ViewData["msg"] = "No tiene permiso para editar usuarios";
                    return View();
                }
                    if (user != null && user.isAdmin == false)
                    {
                        usuarioAModificar.dni = dni;
                        usuarioAModificar.nombre= nombre;
                        usuarioAModificar.apellido= apellido;
                        usuarioAModificar.mail = mail;
                        usuarioAModificar.password = password;
                        _context.Update(usuarioAModificar);
                        await _context.SaveChangesAsync();
                        ViewData["msg"] = "Usuario modificado con exito";
                        return View();
                    }
                    if(user?.isAdmin == true)
                    {
                        usuarioAModificar.dni = dni;
                        usuarioAModificar.nombre = nombre;
                        usuarioAModificar.apellido = apellido;
                        usuarioAModificar.mail = mail;
                        usuarioAModificar.intentosFallidos= intentosFallidos;
                        usuarioAModificar.bloqueado = bloqueado;
                        usuarioAModificar.password = password;
                        usuarioAModificar.isAdmin = isAdmin;
                    if (intentosFallidos >= 3 && bloqueado == false)
                    {
                        usuarioAModificar.bloqueado = true;
                    }
                    if(intentosFallidos < 3 && bloqueado == true)
                    {
                        usuarioAModificar.bloqueado = false;
                    }
                        _context.Update(usuarioAModificar);
                        await _context.SaveChangesAsync();
                        ViewData["msg"] = "Usuario modificado con exito";
                        return View();
                    }
                    return View();
                }
                catch (Exception ex)
                {
                    ViewData["msg"] = "Error: "+ex.Message;
                    return RedirectToAction("Edit", "Usuarios");
                }
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if(user == null || user.isAdmin == false)
            {
                ViewData["msg"] = "No tiene permiso para eliminar usuarios";
                return View();
            }

            if (id == null || _context.usuarios == null)
            {
                return NotFound();
            }

            var usuario = await _context.usuarios
                .FirstOrDefaultAsync(m => m.id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var sesion = HttpContext.Session.GetInt32("usuario");
            Usuario user = _context.usuarios.Where(u => u.id == sesion).FirstOrDefault();
            if (user == null || user.isAdmin == false)
            {
                ViewData["msg"] = "No tiene permiso para eliminar usuarios";
                return View();
            }

            Usuario usuarioARemover = _context.usuarios.Where(u => u.id == id).FirstOrDefault();

            if (usuarioARemover.tarjetas.Where(t => t.consumo != 0.0).FirstOrDefault() != null)
            {
                ViewData["msg"] = "Este usuario posee consumos pendientes de pago, no se puede eliminar";
                return View();
            }
            if (usuarioARemover.pf.Where(p => p.pagado == false).FirstOrDefault() != null)
            {
                ViewData["msg"] = "Este usuario posee plazo fijo pendiente de pago, no se puede eliminar";
                return View();
            }
           
                var usuario = await _context.usuarios.FindAsync(id);
                if (usuario != null)
                {
                _context.usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
                
            
            else
            {
                ViewData["msg"] = "Error al eliminar usuario";
                return View();
            }
        
        /*
        if (_context.usuarios == null)
        {
            return Problem("Entity set 'MiContexto.usuarios'  is null.");
        }
        var usuario = await _context.usuarios.FindAsync(id);
        if (usuario != null)
        {
            _context.usuarios.Remove(usuario);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
        */

    }

        private bool UsuarioExists(int id)
        {
          return _context.usuarios.Any(e => e.id == id);
        }
    }
}
