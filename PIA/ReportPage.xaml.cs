using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PIA.Services;

using SkiaSharp;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PIA
{
    /// <summary>
    /// =========================================================
    /// PÁGINA DE REPORTES Y DASHBOARD
    /// =========================================================
    ///
    /// Esta página se encarga de:
    ///
    /// - Mostrar estadísticas generales
    /// - Mostrar métricas de ventas
    /// - Mostrar gráfica semanal
    /// - Mostrar historial de ventas
    /// - Abrir carpeta de tickets PDF
    /// - Navegar entre dashboard e historial
    ///
    /// =========================================================
    /// </summary>

    public sealed partial class ReportPage : Page
    {
        // =========================================================
        // SERVICIO DE VENTAS
        // Permite obtener las ventas almacenadas en JSON
        // =========================================================

        private readonly VentaService ventaService = new();

        /// <summary>
        /// =========================================================
        /// CONSTRUCTOR
        /// =========================================================
        /// Inicializa componentes y carga toda la información
        /// del dashboard.
        /// =========================================================
        /// </summary>

        public ReportPage()
        {
            this.InitializeComponent();

            // Cargar métricas principales
            CargarDashboard();

            // Cargar gráfica de ventas
            CargarGrafica();

            // Cargar historial de ventas
            CargarHistorial();
        }

        /// <summary>
        /// =========================================================
        /// CARGAR DASHBOARD
        /// =========================================================
        ///
        /// Obtiene:
        /// - total vendido
        /// - número de ventas
        /// - producto más vendido
        /// - método de pago más usado
        ///
        /// =========================================================
        /// </summary>

        private void CargarDashboard()
        {
            // Obtener todas las ventas almacenadas
            var ventas = ventaService.Obtener();

            // Si no existen ventas, terminar
            if (!ventas.Any())
                return;

            // =========================================================
            // TOTAL GENERAL VENDIDO
            // =========================================================

            decimal totalVentas = ventas.Sum(v => v.Total);

            // =========================================================
            // NÚMERO TOTAL DE VENTAS
            // =========================================================

            int numeroVentas = ventas.Count;

            // =========================================================
            // PRODUCTO MÁS VENDIDO
            // =========================================================

            var productoTop = ventas
                .SelectMany(v => v.Items)
                .GroupBy(i => i.Nombre)
                .OrderByDescending(g => g.Sum(x => x.Cantidad))
                .FirstOrDefault();

            // =========================================================
            // MÉTODO DE PAGO MÁS UTILIZADO
            // =========================================================

            var metodoTop = ventas
                .GroupBy(v => v.MetodoPago)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            // =========================================================
            // MOSTRAR DATOS EN PANTALLA
            // =========================================================

            TotalVentasText.Text = $"${totalVentas:F2}";

            NumeroVentasText.Text = numeroVentas.ToString();

            ProductoTopText.Text = productoTop?.Key ?? "N/A";

            MetodoPagoText.Text = metodoTop?.Key ?? "N/A";
        }

        /// <summary>
        /// =========================================================
        /// CARGAR GRÁFICA
        /// =========================================================
        ///
        /// Genera gráfica de barras con ventas agrupadas
        /// por día de la semana.
        ///
        /// =========================================================
        /// </summary>

        private void CargarGrafica()
        {
            // Obtener ventas registradas
            var ventas = ventaService.Obtener();

            // =========================================================
            // ETIQUETAS DE LOS DÍAS
            // =========================================================

            var dias = new[]
            {
                "Lun",
                "Mar",
                "Mié",
                "Jue",
                "Vie",
                "Sáb",
                "Dom"
            };

            // =========================================================
            // ARREGLO DE VALORES
            // Cada posición representa un día
            // =========================================================

            double[] valores = new double[7];

            // =========================================================
            // RECORRER VENTAS Y SUMAR TOTALES
            // =========================================================

            foreach (var venta in ventas)
            {
                // Convertir DayOfWeek a índice compatible
                int index = ((int)venta.Fecha.DayOfWeek + 6) % 7;

                valores[index] += (double)venta.Total;
            }

            // =========================================================
            // CONFIGURAR SERIES DE LA GRÁFICA
            // =========================================================

            VentasChart.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    // Valores de ventas
                    Values = valores,

                    // Color principal de barras
                    Fill = new SolidColorPaint(
                        new SKColor(220, 38, 38)
                    ),

                    // Sin borde negro
                    Stroke = null,

                    // Tamaño máximo barras
                    MaxBarWidth = 60,

                    // Bordes redondeados
                    Rx = 12,
                    Ry = 12
                }
            };

            // =========================================================
            // EJE X (DÍAS)
            // =========================================================

            VentasChart.XAxes = new[]
            {
                new Axis
                {
                    Labels = dias,

                    LabelsPaint = new SolidColorPaint(
                        new SKColor(127, 29, 29)
                    ),

                    TextSize = 16
                }
            };

            // =========================================================
            // EJE Y (VALORES)
            // =========================================================

            VentasChart.YAxes = new[]
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(
                        new SKColor(127, 29, 29)
                    ),

                    TextSize = 16
                }
            };

            // =========================================================
            // ELIMINAR BORDE OSCURO
            // =========================================================

            VentasChart.DrawMarginFrame = null;

            // =========================================================
            // FONDO CLARO DE LA GRÁFICA
            // =========================================================

            VentasChart.Background =
                new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(
                        255,
                        255,
                        245,
                        245
                    )
                );
        }

        /// <summary>
        /// =========================================================
        /// CARGAR HISTORIAL
        /// =========================================================
        ///
        /// Muestra todas las ventas ordenadas
        /// desde la más reciente.
        ///
        /// =========================================================
        /// </summary>

        private void CargarHistorial()
        {
            var ventas = ventaService.Obtener()

                .OrderByDescending(v => v.Fecha)

                .ToList();

            // Asignar ventas al ListView
            VentasListView.ItemsSource = ventas;
        }

        /// <summary>
        /// =========================================================
        /// MOSTRAR HISTORIAL
        /// =========================================================
        ///
        /// Oculta dashboard y muestra historial.
        ///
        /// =========================================================
        /// </summary>

        private void HistorialButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DashboardPanel.Visibility = Visibility.Collapsed;

            HistorialPanel.Visibility = Visibility.Visible;

            HistorialButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// =========================================================
        /// REGRESAR AL DASHBOARD
        /// =========================================================
        ///
        /// Oculta historial y muestra dashboard.
        ///
        /// =========================================================
        /// </summary>

        private void DashboardButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DashboardPanel.Visibility = Visibility.Visible;

            HistorialPanel.Visibility = Visibility.Collapsed;

            HistorialButton.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// =========================================================
        /// BOTÓN VOLVER
        /// =========================================================
        ///
        /// Regresa a la página anterior.
        ///
        /// =========================================================
        /// </summary>

        private void VolverButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        /// <summary>
        /// =========================================================
        /// ABRIR CARPETA DE TICKETS
        /// =========================================================
        ///
        /// Abre el directorio donde se almacenan
        /// todos los tickets PDF generados.
        ///
        /// =========================================================
        /// </summary>

        private void TicketsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Ruta de carpeta de tickets
            string carpeta = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "TicketsGenerados"
            );

            // Crear carpeta si no existe
            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            // Abrir carpeta en explorador de Windows
            Process.Start(new ProcessStartInfo
            {
                FileName = carpeta,

                UseShellExecute = true
            });
        }
    }
}