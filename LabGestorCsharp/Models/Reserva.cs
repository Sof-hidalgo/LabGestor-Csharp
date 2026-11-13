using System;

namespace LabGestorCsharp.Models
{
    public enum EstadoReserva { PendienteAprobacion, Confirmada, EnCurso, LiberadaPorInasistencia, CanceladaPorMantenimiento, Expirada }
    public enum EstadoPrestamo { PRESTADO, DEVUELTO, VENCIDO }

    public class Reserva
    {
        public string Id { get; }
        public Usuario Usuario { get; }
        public Espacio Espacio { get; }
        public PuestoTrabajo Puesto { get; }
        public string Fecha { get; }
        public string Franja { get; }
        public int TamanoGrupo { get; }
        public EstadoReserva Estado { get; private set; }

        public Reserva(string id, Usuario usuario, Espacio espacio, string fecha, string franja, PuestoTrabajo puesto = null, int tamanoGrupo = 1)
        {
            Id = id;
            Usuario = usuario;
            Espacio = espacio;
            Puesto = puesto;
            Fecha = fecha;
            Franja = franja;
            TamanoGrupo = tamanoGrupo;
            Estado = espacio.RequiereAprobacion() ? EstadoReserva.PendienteAprobacion : EstadoReserva.Confirmada;
        }

        public void Confirmar() => Estado = EstadoReserva.Confirmada;
        public void CheckIn(int minutosRetraso)
        {
            if (minutosRetraso > 15) { LiberarPorInasistencia(); throw new InvalidOperationException("Tolerancia de 15 min superada."); }
            Estado = EstadoReserva.EnCurso;
        }
        public void LiberarPorInasistencia() { Estado = EstadoReserva.LiberadaPorInasistencia; Usuario.RegistrarInasistencia(); }
        public void CancelarPorMantenimiento() => Estado = EstadoReserva.CanceladaPorMantenimiento;
        public bool ActivaEn(string fecha, string franja) => Fecha == fecha && Franja == franja && (Estado == EstadoReserva.PendienteAprobacion || Estado == EstadoReserva.Confirmada || Estado == EstadoReserva.EnCurso);
    }

    public class Prestamo
    {
        public string IdPrestamo { get; }
        public Usuario Usuario { get; }
        public Equipo Equipo { get; }
        public DateTime FechaPrestamo { get; }
        public DateTime FechaPactadaDevolucion { get; }
        public DateTime? FechaRealDevolucion { get; private set; }
        public EstadoPrestamo Estado { get; private set; }
        public int DiasSancionAplicada { get; private set; }

        public Prestamo(string id, Usuario usuario, Equipo equipo, DateTime fechaPrestamo, DateTime fechaPactada)
        {
            if (!equipo.PuedePrestarse()) throw new InvalidOperationException("El equipo no está disponible para préstamo (RN-04 o RN-06).");
            if (fechaPactada > fechaPrestamo.AddDays(equipo.GetPlazoMaximoDias())) throw new InvalidOperationException("RN-10: Excede el plazo máximo del equipo.");
            if (usuario.Sancionado) throw new InvalidOperationException("RN-11: Usuario sancionado no puede solicitar préstamos.");

            IdPrestamo = id;
            Usuario = usuario;
            Equipo = equipo;
            FechaPrestamo = fechaPrestamo;
            FechaPactadaDevolucion = fechaPactada;
            Estado = EstadoPrestamo.PRESTADO;
            equipo.CambiarEstado(EstadoEquipo.PRESTADO);
        }

        public void RegistrarDevolucion(DateTime fechaReal, int componentesFaltantes = 0)
        {
            FechaRealDevolucion = fechaReal;
            Estado = EstadoPrestamo.DEVUELTO;

            int diasRetraso = (fechaReal.Date - FechaPactadaDevolucion.Date).Days;
            if (diasRetraso > 0)
            {
                DiasSancionAplicada = Equipo.CalcularSancion(diasRetraso, componentesFaltantes);
                for (int i = 0; i < DiasSancionAplicada; i++) Usuario.RegistrarInasistencia(); // O acumular sanción temporal
            }

            if (Equipo is KitDesarrollo && componentesFaltantes > 0)
            {
                Equipo.CambiarEstado(EstadoEquipo.EN_MANTENIMIENTO); // RN-07
            }
            else
            {
                Equipo.CambiarEstado(EstadoEquipo.DISPONIBLE);
            }
        }
    }
}