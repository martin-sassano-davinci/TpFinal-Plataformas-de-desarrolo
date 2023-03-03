﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


//using TrabajoPractico1;

namespace tp4.Models
{
    public class Usuario
    {
        
        public int id { get; set; }
        [Required(ErrorMessage = "El DNI es obligatorio")]
        [Display(Name = "DNI")]
        public int dni { get; set; }
        
        [Display(Name = "Nombre")]
        public string nombre { get; set; }
        
        [Display(Name = "Apellido")]
        public string apellido { get; set; }
        
        [EmailAddress]
        [Display(Name ="Email")]
        public string mail { get; set; }
        public int intentosFallidos { get; set; }
        public bool bloqueado { get; set; }
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string password { get; set; }
        public List<PlazoFijo> pf { get; set; }
        public List<Tarjeta> tarjetas { get; set; }
        public List<Pago> pagos { get; set; }
        public bool isAdmin { get; set; }
        public ICollection<CajaDeAhorro> cajas { get; } = new List<CajaDeAhorro>();
        public List<UsuarioCaja> usuarioCajas { get; set; }

        public Usuario() { }

        public Usuario(int Id, int Dni, string Nombre, string Apellido, string Mail, string Password, bool Bloqueado, bool Is_admin, int IntentosFallidos)
        {
            this.id = Id;
            this.dni = Dni;
            this.nombre = Nombre;
            this.apellido = Apellido;
            this.mail = Mail;
            this.password = Password;
            this.intentosFallidos = IntentosFallidos;
            this.bloqueado = Bloqueado;
            this.isAdmin = Is_admin;
            this.pagos = new List<Pago>();
            this.cajas = new List<CajaDeAhorro>();
            this.pf = new List<PlazoFijo>();
            this.tarjetas = new List<Tarjeta>();
        }

        public Usuario(int Dni, string Nombre, string Apellido, string Mail, string Password, int IntentosFallidos, bool Bloqueado, bool Is_admin)
        {
            this.dni = Dni;
            this.nombre = Nombre;
            this.apellido = Apellido;
            this.mail = Mail;
            this.password = Password;
            this.intentosFallidos = IntentosFallidos;
            this.bloqueado = Bloqueado;
            this.isAdmin = Is_admin;
            this.pagos = new List<Pago>();
            this.cajas = new List<CajaDeAhorro>();
            this.pf = new List<PlazoFijo>();
            this.tarjetas = new List<Tarjeta>();
        }

        public Usuario(string Nombre, string Apellido, int Dni, string Mail, string Password)
        {
            this.nombre = Nombre;
            this.apellido = Apellido;
            this.dni = Dni;
            this.mail = Mail;
            this.password = Password;
            this.pagos = new List<Pago>();
            this.cajas = new List<CajaDeAhorro>();
            this.pf = new List<PlazoFijo>();
            this.tarjetas = new List<Tarjeta>();
        }

        public override string ToString()
        {
            return string.Format("Nombre: {0}, Apellido: {1}, Email: {2}, Password: {3}", nombre, apellido, mail, password);
        }
    }
}
