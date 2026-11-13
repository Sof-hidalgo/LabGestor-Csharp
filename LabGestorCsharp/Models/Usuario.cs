using System;

namespace LabGestorCsharp.Models
{
    public abstract class Usuario
    {
        public string DocumentoIdentidad { get; }
        public string NombreCompleto { get; }
        public string CorreoInstitucional { get; }
        public int Inasistencias { get; protected set; }
        public bool Sancionado { get; protected set; }

        protected Usuario(string documentoIdentidad, string nombreCompleto, string correoInstitucional)
        {
            if (string.IsNullOrWhiteSpace(documentoIdentidad) || string.IsNullOrWhiteSpace(nombreCompleto))
                throw new ArgumentException("El documento y el nombre son obligatorios.");
            
            if (string.IsNullOrWhiteSpace(correoInstitucional) || !correoInstitucional.EndsWith("@unisalle.edu.co", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("RN-14: El correo debe pertenecer al dominio institucional (@unisalle.edu.co).");

            DocumentoIdentidad = documentoIdentidad;
            NombreCompleto = nombreCompleto;
            CorreoInstitucional = correoInstitucional;
            Inasistencias = 0;
            Sancionado = false;
        }

        public void RegistrarInasistencia()
        {
            Inasistencias++;
            if (Inasistencias >= 3) Sancionado = true;
        }

        public bool HabilitadoParaReservar() => !Sancionado;

        public abstract int GetLimitePrestamos();
        public abstract bool PuedeReservarGrupo();
        public abstract bool CertificadoSalaLimpia();
    }

    public class Estudiante : Usuario
    {
        public string CodigoEstudiantil { get; }
        public string ProgramaAcademico { get; }
        public int Semestre { get; }
        public bool CertificacionSalaLimpia { get; set; }

        public Estudiante(string doc, string nombre, string correo, string codigo, string programa, int semestre, bool certSalaLimpia = false) 
            : base(doc, nombre, correo)
        {
            CodigoEstudiantil = codigo;
            ProgramaAcademico = programa;
            Semestre = semestre;
            CertificacionSalaLimpia = certSalaLimpia;
        }

        public override int GetLimitePrestamos() => 2; // RN-09
        public override bool PuedeReservarGrupo() => false;
        public override bool CertificadoSalaLimpia() => CertificacionSalaLimpia;
    }

    public class Monitor : Estudiante
    {
        public string LaboratorioAsignado { get; }
        public int HorasSemanales { get; }

        public Monitor(string doc, string nombre, string correo, string codigo, string programa, int semestre, string laboratorio, int horas, bool certSalaLimpia = true) 
            : base(doc, nombre, correo, codigo, programa, semestre, certSalaLimpia)
        {
            LaboratorioAsignado = laboratorio;
            HorasSemanales = horas;
        }

        public override int GetLimitePrestamos() => 3; // RN-09
        public override bool PuedeReservarGrupo() => true;
    }

    public class Docente : Usuario
    {
        public string CodigoDocente { get; }
        public string Facultad { get; }

        public Docente(string doc, string nombre, string correo, string codigoDocente, string facultad) 
            : base(doc, nombre, correo)
        {
            CodigoDocente = codigoDocente;
            Facultad = facultad;
        }

        public override int GetLimitePrestamos() => 5; // RN-09
        public override bool PuedeReservarGrupo() => true;
        public override bool CertificadoSalaLimpia() => true;
    }
}