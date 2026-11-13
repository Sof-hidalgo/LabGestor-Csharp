using System;
using System.Collections.Generic;
using System.Linq;

namespace LabGestorCsharp.Models
{
    public enum EstadoEquipo { DISPONIBLE, PRESTADO, EN_MANTENIMIENTO, DADO_DE_BAJA }

    public class PuestoTrabajo
    {
        public string Id { get; }
        public bool Disponible { get; set; }

        public PuestoTrabajo(string id)
        {
            Id = id;
            Disponible = true;
        }
    }

    public abstract class Espacio
    {
        public string Id { get; }
        public string Nombre { get; }
        public int AforoMaximo { get; }
        public bool EnMantenimiento { get; private set; }

        protected Espacio(string id, string nombre, int aforoMaximo)
        {
            if (aforoMaximo <= 0) throw new ArgumentException("El aforo debe ser mayor a cero.");
            Id = id;
            Nombre = nombre;
            AforoMaximo = aforoMaximo;
            EnMantenimiento = false;
        }

        public void PonerEnMantenimiento() => EnMantenimiento = true;
        public void QuitarMantenimiento() => EnMantenimiento = false;
        public virtual bool RequiereAprobacion() => false;

        public virtual void ValidarRequisitosReserva(Usuario usuario, Dictionary<string, object> opciones = null)
        {
            if (EnMantenimiento) throw new InvalidOperationException($"El espacio {Nombre} está en mantenimiento.");
            if (!usuario.HabilitadoParaReservar()) throw new InvalidOperationException($"El usuario {usuario.NombreCompleto} está sancionado.");

            int tamano = opciones != null && opciones.ContainsKey("tamano_grupo") ? (int)opciones["tamano_grupo"] : 1;
            if (tamano > 1 && !usuario.PuedeReservarGrupo()) throw new InvalidOperationException("Solo docentes y monitores pueden reservar para grupos.");
            if (tamano > AforoMaximo) throw new InvalidOperationException($"El tamaño del grupo excede el aforo ({AforoMaximo}).");
        }
    }

    public class SalaComputo : Espacio
    {
        private readonly List<PuestoTrabajo> _puestos;
        
        // Colección estrictamente protegida hacia el exterior (Sugerencia del profesor)
        public IReadOnlyList<PuestoTrabajo> Puestos => _puestos.AsReadOnly();

        public SalaComputo(string id, string nombre, int totalPuestos) : base(id, nombre, totalPuestos)
        {
            _puestos = Enumerable.Range(1, totalPuestos).Select(n => new PuestoTrabajo($"{id}-P{n}")).ToList();
        }

        public PuestoTrabajo ObtenerPuesto(string puestoId) => _puestos.FirstOrDefault(p => p.Id == puestoId);

        public override void ValidarRequisitosReserva(Usuario usuario, Dictionary<string, object> opciones = null)
        {
            base.ValidarRequisitosReserva(usuario, opciones);
            if (opciones != null && opciones.ContainsKey("puesto_id"))
            {
                string puestoId = (string)opciones["puesto_id"];
                if (ObtenerPuesto(puestoId) == null) throw new ArgumentException($"El puesto {puestoId} no pertenece a esta sala.");
            }
        }
    }

    public class SalaLimpia : Espacio
    {
        public SalaLimpia(string id, string nombre = "Sala Limpia Principal") : base(id, nombre, 6) { }
        public override bool RequiereAprobacion() => true;
        public override void ValidarRequisitosReserva(Usuario usuario, Dictionary<string, object> opciones = null)
        {
            base.ValidarRequisitosReserva(usuario, opciones);
            if (!usuario.CertificadoSalaLimpia()) throw new InvalidOperationException($"El usuario {usuario.NombreCompleto} no posee certificación para Sala Limpia.");
        }
    }

    public class TallerPrototipado : Espacio
    {
        public TallerPrototipado(string id, string nombre, int aforoMaximo = 15) : base(id, nombre, aforoMaximo) { }
        public override bool RequiereAprobacion() => true;
    }

    public class CubiculoProyecto : Espacio
    {
        public CubiculoProyecto(string id, string nombre, int aforoMaximo = 5) : base(id, nombre, aforoMaximo) { }
    }

    // Equipo base con blindaje RN-01, RN-02 y RN-03
    public abstract class Equipo
    {
        public string CodigoInventario { get; } // RN-01: Inmutable
        public string Descripcion { get; }
        public double ValorReposicion { get; } // RN-02
        public EstadoEquipo Estado { get; private set; } // RN-03

        protected Equipo(string codigoInventario, string descripcion, double valorReposicion)
        {
            if (string.IsNullOrWhiteSpace(codigoInventario)) throw new ArgumentException("El código de inventario no puede estar vacío.");
            if (valorReposicion <= 0) throw new ArgumentException("RN-02: El valor de reposición debe ser estrictamente mayor a cero.");

            CodigoInventario = codigoInventario;
            Descripcion = descripcion;
            ValorReposicion = valorReposicion;
            Estado = EstadoEquipo.DISPONIBLE;
        }

        public void CambiarEstado(EstadoEquipo nuevoEstado)
        {
            if (Estado == EstadoEquipo.DADO_DE_BAJA) throw new InvalidOperationException("RN-05: Un equipo dado de baja no puede cambiar de estado.");
            Estado = nuevoEstado;
        }

        public abstract int GetPlazoMaximoDias();
        public abstract int CalcularSancion(int diasRetraso, int componentesFaltantes);
        public virtual bool PuedePrestarse() => Estado == EstadoEquipo.DISPONIBLE;
    }

    public class EquipoComputo : Equipo
    {
        public string Procesador { get; }
        public int MemoriaRamGb { get; }
        public string SistemaOperativo { get; }
        public bool EsPortatil { get; }

        public EquipoComputo(string codigo, string desc, double valor, string proc, int ram, string so, bool portatil) 
            : base(codigo, desc, valor)
        {
            Procesador = proc;
            MemoriaRamGb = ram;
            SistemaOperativo = so;
            EsPortatil = portatil;
        }

        public override int GetPlazoMaximoDias() => 5; // RN-08
        public override int CalcularSancion(int diasRetraso, int componentesFaltantes) => diasRetraso * 2;
    }

    public class InstrumentoMedicion : Equipo
    {
        public string Marca { get; }
        public string Modelo { get; }
        public DateTime FechaUltimaCalibracion { get; }
        public int VigenciaMeses { get; }

        public InstrumentoMedicion(string codigo, string desc, double valor, string marca, string modelo, DateTime fechaCalibracion, int vigenciaMeses) 
            : base(codigo, desc, valor)
        {
            Marca = marca;
            Modelo = modelo;
            FechaUltimaCalibracion = fechaCalibracion;
            VigenciaMeses = vigenciaMeses;
        }

        public override int GetPlazoMaximoDias() => 3; // RN-08

        public override bool PuedePrestarse()
        {
            if (!base.PuedePrestarse()) return false;
            return DateTime.Now <= FechaUltimaCalibracion.AddMonths(VigenciaMeses); // RN-06
        }

        public override int CalcularSancion(int diasRetraso, int componentesFaltantes) => diasRetraso * 5;
    }

    public class KitDesarrollo : Equipo
    {
        public string Plataforma { get; }
        public int NumeroComponentes { get; }

        public KitDesarrollo(string codigo, string desc, double valor, string plataforma, int componentes) 
            : base(codigo, desc, valor)
        {
            Plataforma = plataforma;
            NumeroComponentes = componentes;
        }

        public override int GetPlazoMaximoDias() => 10; // RN-08
        public override int CalcularSancion(int diasRetraso, int componentesFaltantes) => (diasRetraso * 3) + (componentesFaltantes * 5);
    }
}