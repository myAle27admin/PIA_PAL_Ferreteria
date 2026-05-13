# PIA_PAL_Ferreteria

## Sistema POS para Ferretería "Casa del Herrero"

Proyecto Integrador de Aprendizaje (PIA) desarrollado para la materia **Programación de Aplicaciones Locales (PAL)**.

Este proyecto consiste en un sistema **Point of Sale (POS)** para la gestión y venta de productos en una ferretería, permitiendo administración de inventario, ventas, generación de tickets PDF y reportes.

---

## Información académica

**Universidad Autónoma de Nuevo León**
**Facultad de Contaduría Pública y Administración**

**Materia:** Programación de Aplicaciones Locales (PAL)
**Grupo:** 61

### Integrantes

* Alejandro Santiago Coronado — 2065785
* Hugo Alejandro Zapata Rosales — 2088379
* Erick Yovani Mata Cazares — 2089133

---

## Tecnologías utilizadas

* **C#**
* **WinUI 3**
* **.NET**
* **JSON** (persistencia de datos)
* **QuestPDF** (generación de tickets PDF)
* **Git / GitHub**

---

## Funcionalidades principales

### Sistema POS

* Visualización de catálogo de productos
* Búsqueda dinámica de productos
* Carrito de compras
* Control de stock en tiempo real
* Restricción de venta para productos inactivos
* Cálculo automático de subtotal, IVA y total
* Generación de tickets PDF por venta
* Historial de ventas

### Administración de productos

* Agregar productos
* Editar productos
* Eliminar productos
* Gestión de:

  * Nombre
  * Categoría
  * Precio
  * Stock
  * Marca
  * Icono
  * Estado (Activo / Inactivo)

### Reportes

* Visualización de reportes de ventas
* Historial de tickets generados

### Persistencia de datos

* Almacenamiento de productos mediante archivos JSON
* Persistencia del inventario entre ejecuciones

---

## Requisitos para ejecutar el proyecto

Se requiere:

* **Visual Studio 2022**
* **.NET SDK compatible**
* **Windows App SDK**
* Sistema operativo **Windows 10 / Windows 11**

---

## Instalación y ejecución

### 1. Clonar repositorio

```bash
git clone https://github.com/myAle27admin/PIA_PAL_Ferreteria.git
```

### 2. Abrir solución

Abrir el archivo:

```text
PIA.slnx
```

### 3. Restaurar dependencias

Visual Studio restaurará automáticamente los paquetes NuGet necesarios.

### 4. Ejecutar

Compilar y ejecutar el proyecto desde Visual Studio.

---

## Estructura general del proyecto

```text
PIA/
├── Assets/
├── Converters/
├── Models/
├── Properties/
├── Services/
├── TicketsGenerados/
├── AddProductPage.xaml
├── AdminPage.xaml
├── EditProductPage.xaml
├── LoginPage.xaml
├── MainWindow.xaml
├── POSPage.xaml
├── ReportPage.xaml
```

---

## Notas

* La carpeta **TicketsGenerados** se crea automáticamente al generar el primer ticket PDF.
* El proyecto fue desarrollado con fines académicos como parte del Proyecto Integrador de Aprendizaje.

---

## Descripción del sistema

"Casa del Herrero" es un sistema POS orientado a pequeñas ferreterías, diseñado para facilitar el control administrativo y operativo del negocio mediante una interfaz visual intuitiva y herramientas de gestión de inventario y ventas.
