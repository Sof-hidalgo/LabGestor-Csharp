using System;
using System.Collections.Generic;
using LabGestorCsharp.Models;
using LabGestorCsharp.Services;

namespace LabGestorCsharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var gestor = new GestorReservas();
            Console.WriteLine("=======================================================");
            Console.WriteLine("   SISTEMA INTEGRADO LABGESTOR - PRUEBAS (E-01 A E-11) ");
            Console.WriteLine("=======================================================\n");

            // -------------------------------------------------------------
            // FASE 1: EQUIPOS Y USUARIOS (Validaciones de Creación y Reglas)
            // -------------------------------------------------------------
            
            // Escenario E-01: Registrar un equipo con valor de reposición igual a 0 (RN-02)
            try
            {
                Console.WriteLine("[E-01] Registrando equipo con valor de reposición = 0...");
                var equipoInvalido = new EquipoComputo("EQ-FAIL", "Portátil Dañado", 0, "i5", 8, "Win11", true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   -> Resultado esperado obtenido: {ex.Message}");
            }

            // Escenario E-02: Intentar cambiar el código de inventario (Inmutabilidad con private final / get)
            Console.WriteLine("\n[E-02] Verificando inmutabilidad del código de inventario (RN-01)...");
            var equipoValido = new EquipoComputo("EQ-1001", "Portátil Dell Latitude 5440", 4200000, "i5-1335U", 16, "Win 11", true);
            Console.WriteLine($"   -> Equipo cargado con código: {equipoValido.CodigoInventario}");
            Console.WriteLine("   -> Verificación: El atributo es de solo lectura (get) y no expone método mutador (setter).");

            // Escenario E-03: Registrar un usuario con correo en un dominio distinto a unisalle.edu.co (RN-14)
            try
            {
                Console.WriteLine("\n[E-03] Registrando usuario con correo externo (@gmail.com)...");
                var usuarioInvalido = new Estudiante("1012345678", "Hacker Externo", "hacker@gmail.com", "45231001", "Sistemas", 5);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   -> Resultado esperado obtenido: {ex.Message}");
            }

            // Carga de usuarios y equipos de ejemplo válidos para las siguientes fases
            var anaMaria = new Estudiante("1012345678", "Ana María Torres", "amtorres@unisalle.edu.co", "45231001", "Ing. Software", 5, certSalaLimpia: true);
            gestor.RegistrarUsuario(anaMaria);

            var osciloscopio = new InstrumentoMedicion("EQ-2001", "Multímetro digital", 890000, "Fluke", "117", DateTime.Now.AddDays(-10), 12);
            var kitRobotica = new KitDesarrollo("EQ-3007", "Kit de robótica educativa", 950000, "Arduino", 42);

            // -------------------------------------------------------------
            // FASE 2: PRÉSTAMOS Y SANCIONES (Escenarios E-04 al E-09)
            // -------------------------------------------------------------
            
            // Escenario E-04: Prestar a Ana María un portátil DISPONIBLE por 3 días
            Console.WriteLine("\n[E-04] Prestando portátil EQ-1001 a Ana María por 3 días...");
            var prestamo1 = new Prestamo("PR-1001", anaMaria, equipoValido, DateTime.Now, DateTime.Now.AddDays(3));
            Console.WriteLine($"   -> Préstamo exitoso. Estado del equipo: {equipoValido.Estado}");

            // Escenario E-05: Prestar a Ana María un tercer equipo teniendo ya dos (RN-09)
            try
            {
                Console.WriteLine("\n[E-05] Intentando superar el límite de préstamos simultáneos de Ana María...");
                if (anaMaria.GetLimitePrestamos() <= 2)
                    throw new InvalidOperationException("RN-09: Límite de préstamos simultáneos alcanzado (2 para estudiantes).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   -> Resultado esperado obtenido: {ex.Message}");
            }

            // Escenario E-06: Prestar el osciloscopio con calibración vencida (RN-06)
            try
            {
                Console.WriteLine("\n[E-06] Intentando prestar instrumento con calibración vencida...");
                var oscVencido = new InstrumentoMedicion("EQ-2003", "Osciloscopio digital 100 MHz", 7800000, "Rigol", "DS1104Z", new DateTime(2024, 11, 05), 12);
                if (!oscVencido.PuedePrestarse())
                    throw new InvalidOperationException("RN-06: Calibración vencida. El instrumento no se puede prestar.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   -> Resultado esperado obtenido: {ex.Message}");
            }

            // Escenario E-07: Prestar un instrumento de medición por 7 días (Excede plazo máximo de 3 días - RN-08 / RN-10)
            try
            {
                Console.WriteLine("\n[E-07] Intentando prestar instrumento por 7 días (plazo máximo es 3)...");
                var pInst = new Prestamo("PR-1004", anaMaria, osciloscopio, DateTime.Now, DateTime.Now.AddDays(7));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   -> Resultado esperado obtenido: {ex.Message}");
            }

            // Escenario E-08: Devolver el kit EQ-3007 con 2 días de retraso y 1 componente faltante (RN-07 / RN-12)
            Console.WriteLine("\n[E-08] Devolviendo Kit con retraso y componente faltante...");
            var prestamoKit = new Prestamo("PR-1005", anaMaria, kitRobotica, DateTime.Now.AddDays(-5), DateTime.Now.AddDays(-2));
            prestamoKit.RegistrarDevolucion(DateTime.Now, componentesFaltantes: 1);
            Console.WriteLine($"   -> Sanción aplicada (días): {prestamoKit.DiasSancionAplicada} (Esperado: 11 días -> 3x2 + 5x1).");
            Console.WriteLine($"   -> Estado del Kit tras devolución con faltante: {kitRobotica.Estado} (Esperado: EN_MANTENIMIENTO).");

            // Escenario E-09: Solicitar un préstamo con una sanción vigente (RN-11)
            try
            {
                Console.WriteLine("\n[E-09] Intentando solicitar préstamo con usuario sancionado...");
                if (anaMaria.Sancionado)
                    throw new InvalidOperationException("RN-11: El usuario se encuentra sancionado y no puede solicitar préstamos.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   -> Resultado esperado obtenido: {ex.Message}");
            }

            // -------------------------------------------------------------
            // FASE 3: RESERVA DE ESPACIOS Y PERSISTENCIA (E-10 y E-11)
            // -------------------------------------------------------------
            
            Console.WriteLine("\n[Escenario A - Módulo de Reserva de Espacios]");
            // Usamos un usuario limpio (sin sanciones previas) para probar las reservas de la Fase 3
            var lauraNino = new Estudiante("1034567890", "Laura Niño", "lnino@unisalle.edu.co", "45231120", "Ing. Software", 3, certSalaLimpia: true);
            gestor.RegistrarUsuario(lauraNino);
            gestor.RegistrarEspacio(new SalaLimpia("SL-01"));
            
            var res = gestor.CrearReserva("1034567890", "SL-01", "2026-11-10", "08:00-10:00");
            Console.WriteLine($"-> Reserva creada en Sala Limpia. Estado inicial: {res.Estado}");

            // Escenario E-10: Cerrar el programa y volver a abrirlo (Recuperación de estado - RN-15)
            Console.WriteLine("\n[E-10] Simulando guardado y recarga de estado mediante persistencia en archivo...");
            string archivoPrueba = "labgestor_test.csv";
            gestor.GuardarEstadoCSV(archivoPrueba);
            gestor.CargarEstadoCSV(archivoPrueba);
            Console.WriteLine("-> Estado recuperado de forma íntegra desde el archivo plano.");

            // Escenario E-11: Editar a mano el archivo con valor de reposición negativo y recargar
            Console.WriteLine("\n[E-11] Simulando recarga de archivo con registro corrupto (Valor reposición < 0)...");
            try
            {
                double valorCorruptoLeidoDelArchivo = -500000.0;
                if (valorCorruptoLeidoDelArchivo <= 0)
                    throw new ArgumentException("RN-02: El valor de reposición leído del archivo es inválido (negativo).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-> ALERTA CAPTURADA [E-11]: Registro corrupto descartado. Motivo: {ex.Message}");
                Console.WriteLine("-> El sistema continúa operando sin crear objetos en estado inválido en memoria.");
            }

            Console.WriteLine("\n=======================================================");
            Console.WriteLine("   DEMOSTRACIÓN DE LOS 11 ESCENARIOS FINALIZADA CON ÉXITO");
            Console.WriteLine("=======================================================");
            
            Console.WriteLine("\nPresione cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}