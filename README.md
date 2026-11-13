# LabGestor: Sistema Integrado de Control para Laboratorios

Sistema transaccional desarrollado en C# (.NET) bajo principios estrictos de Programación Orientada a Objetos, diseñado para la gestión integral, segura y automatizada del Laboratorio de Ingeniería.

## 📌 Descripción del Proyecto
LabGestor soluciona la problemática derivada del uso de hojas de cálculo compartidas, eliminando los falsos estados de disponibilidad, los préstamos de instrumental descalibrado, el cálculo discrecional de sanciones y la pérdida de trazabilidad en equipos y espacios de la facultad.

## ✨ Características Principales
* **Gestión de Inventario y Equipos:** Registro tipificado de equipos, herramientas e instrumentos con validaciones estrictas de integridad en tiempo de ejecución.
* **Control de Préstamos y Devoluciones:** Automatización de plazos, restricciones de seguridad y cálculo matemático equitativo de sanciones por retrasos o faltantes.
* **Módulo de Reservas de Espacios:** Administración y control de acceso a salas y puestos de trabajo de la facultad.
* **Blindaje y Persistencia Robusta:** Mecanismos de defensa ante datos corruptos en archivos locales y recuperación íntegra de estados transaccionales.

## 🚀 Demostración de Escenarios (E-01 a E-11)
El sistema incluye un banco completo de pruebas que valida casos de éxito y situaciones críticas de seguridad:
* **E-04:** Registro de préstamo normal de equipos disponibles.
* **E-06:** Bloqueo automático ante instrumentos con calibración vencida.
* **E-08:** Cálculo de sanciones y paso a mantenimiento preventivo por devolución con retraso y componentes faltantes.
* **E-11:** Detección, descarte y aislamiento seguro de registros corruptos en archivos de almacenamiento.

## 🛠️ Tecnologías y Arquitectura
* **Lenguaje:** C# (.NET)
* **Paradigma:** Programación Orientada a Objetos (Diseño modular, encapsulamiento, polimorfismo y clases abstractas).
* **Persistencia:** Archivos locales planos con validación defensiva ante manipulación externa.

---
*Desarrollado por **Sofía Hidalgo Echeverri** para el espacio académico de Lenguaje de Programación II, Facultad de Ciencia de Datos, Universidad de La Salle.*
