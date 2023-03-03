using System;
using System.Collections.Generic;
using System.Text;

namespace tp4.Models
{
    public class UsuarioCaja
    {
        public int idUsuario { get; set; }
        public Usuario usuario { get; set; }
        public CajaDeAhorro caja { get; set; }
        public int idCaja { get; set; }

        public UsuarioCaja() { }
        public UsuarioCaja(int IdCaja, int IdUsuario)
        {
            this.idUsuario = IdUsuario;
            this.idCaja = IdCaja;
        }
    }
}
