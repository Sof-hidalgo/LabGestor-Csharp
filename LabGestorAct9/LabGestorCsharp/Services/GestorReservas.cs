using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LabGestorCsharp.Models;

namespace LabGestorCsharp.Services
{
    public class GestorReservas
    {
        private readonly Dictionary<string, Espacio> _espacios = new Dictionary<string, Espacio>();
        private readonly Dictionary<string, Usuario> _usuarios = new Dictionary<string, Usuario>();
        private readonly List<Reserva> _reservas = new List<Reserva>();
        private readonly List<Prestamo> _prestamos = new List<Prestamo>();
        private int _contadorReservas = 0;

        public IReadOnlyDictionary<string, Espacio> Espacios => _espacios;
        public IReadOnlyDictionary<string, Usuario> Usuarios => _usuarios;
        public IReadOnlyList<Reserva> Reservas => _reservas.AsReadOnly();
        public IReadOnlyList<Prestamo> Prestamos => _prestamos.AsReadOnly();

        public void RegistrarEspacio(Espacio espacio) => _espacios[espacio.Id] = espacio;
        public void RegistrarUsuario(Usuario usuario) => _usuarios[usuario.DocumentoIdentidad] = usuario;

        public Reserva CrearReserva(string usuarioId, string espacioId, string fecha, string franja, Dictionary<string, object> opciones = null)
        {
            if (!_usuarios.ContainsKey(usuarioId)) throw new ArgumentException("Usuario no registrado.");
            if (!_espacios.ContainsKey(espacioId)) throw new ArgumentException("Espacio no registrado.");

            var usuario = _usuarios[usuarioId];
            var espacio = _espacios[espacioId];

            espacio.ValidarRequisitosReserva(usuario, opciones);
            string puestoId = opciones != null && opciones.ContainsKey("puesto_id") ? (string)opciones["puesto_id"] : null;
            ValidarDisponibilidad(espacio, fecha, franja, puestoId);

            PuestoTrabajo puesto = espacio is SalaComputo salaComp && !string.IsNullOrEmpty(puestoId) ? salaComp.ObtenerPuesto(puestoId) : null;

            _contadorReservas++;
            int tamanoGrupo = opciones != null && opciones.ContainsKey("tamano_grupo") ? (int)opciones["tamano_grupo"] : 1;
            
            var reserva = new Reserva($"RES-{_contadorReservas}", usuario, espacio, fecha, franja, puesto, tamanoGrupo);
            _reservas.Add(reserva);
            return reserva;
        }

        private void ValidarDisponibilidad(Espacio espacio, string fecha, string franja, string puestoId)
        {
            var activas = _reservas.Where(r => r.Espacio.Id == espacio.Id && r.ActivaEn(fecha, franja)).ToList();
            if (espacio is SalaComputo)
            {
                if (string.IsNullOrEmpty(puestoId) && activas.Any()) throw new InvalidOperationException("Sala reservada en su totalidad.");
                if (!string.IsNullOrEmpty(puestoId) && (activas.Any(r => r.Puesto == null) || activas.Any(r => r.Puesto?.Id == puestoId)))
                    throw new InvalidOperationException("Puesto o sala ocupados.");
            }
            else if (activas.Any()) { throw new InvalidOperationException("Espacio ocupado."); }
        }

        public void GuardarEstadoCSV(string rutaArchivo)
        {
            using (var writer = new StreamWriter(rutaArchivo))
            {
                writer.WriteLine("# TIPO, ID, DESCRIPCION, VALOR_REPOSICION");
                foreach (var espacio in _espacios.Values)
                {
                    writer.WriteLine($"ESPACIO,{espacio.Id},{espacio.Nombre},{espacio.AforoMaximo}");
                }
                foreach (var usuario in _usuarios.Values)
                {
                    writer.WriteLine($"USUARIO,{usuario.DocumentoIdentidad},{usuario.NombreCompleto},{usuario.CorreoInstitucional}");
                }
            }
        }

        public void CargarEstadoCSV(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo)) throw new FileNotFoundException("El archivo de persistencia no existe.");

            foreach (var linea in File.ReadAllLines(rutaArchivo))
            {
                if (string.IsNullOrWhiteSpace(linea) || linea.StartsWith("#")) continue;

                try
                {
                    var partes = linea.Split(',');
                    string tipo = partes[0].Trim();

                    if (tipo == "EQUIPO_COMPUTO")
                    {
                        string codigo = partes[1].Trim();
                        string desc = partes[2].Trim();
                        double valorReposicion = double.Parse(partes[3].Trim());

                        var equipo = new EquipoComputo(codigo, desc, valorReposicion, "i5", 8, "Win11", true);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[E-11 ALERTA DE CORRUPCIÓN EN ARCHIVO]: Registro descartado. Motivo: {ex.Message}");
                }
            }
        }
    }
}