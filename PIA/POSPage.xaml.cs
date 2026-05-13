using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PIA.Models;
using PIA.Services;
using PIA.Services.Tickets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace PIA
{
    public class ItemCarrito
    {
        // - Declaramos las variables para cada columna de Producto.
        public Guid ProductId { get; set; }
        public string Nombre { get; set; } = "";
        public string Marca { get; set; } = "";
        public decimal Precio { get; set; }
        public int Cantidad { get; set; } = 1;
        public decimal Subtotal => Precio * Cantidad;
    }

    public class CategoriaItem
    {
        // - Declaramos las variables para cada categoria.
        public string Nombre { get; set; } = "";
        public string Icono { get; set; } = "📦";  
        public string ColorFondo { get; set; } = "#E8611A";
        public string Tag { get; set; } = "";
    }

    public sealed partial class POSPage : Page
    {
        // ── Estado interno
        private readonly List<ItemCarrito> _carrito = new();
        private bool _panelAbierto = false;
        private string _metodoPago = "Efectivo";

        // ── Constante IVA
        private const decimal TasaIva = 0.16m;

        // ── Filtros activos
        private string _categoriaFiltro = "Todos";
        private string _textoBusqueda = "";

        // - Una variable global para llamar el servicio de escritura en json de Producto /Services/ProductService.cs
        ProductService servicio = new ProductService();
        // - Los datos se centralizan en una lista del modelo de Producto. /Models/Product.cs
        List<Product> productos = new List<Product>();

        // - Una variable global para llamar el servicio de escritura en json de Ventas /Services/VentaService.cs
        private VentaService ventaService = new VentaService();

        // - Función para cargar productos usando el Servicio de Productos
        private void CargarProductos()
        {
            productos = servicio.Obtener();
        }

        // ── Catálogo de categorías para la pantalla inicial
        //    Agrega, quita o cambia colores aquí según tus categorías reales.
        private readonly List<CategoriaItem> _categorias = new()
        {
            new CategoriaItem { Nombre = "Todos",        Icono = "🏪", ColorFondo = "#1E2333", Tag = "Todos"        },
            new CategoriaItem { Nombre = "Herramientas", Icono = "🔨", ColorFondo = "#E8611A", Tag = "Herramientas" },
            new CategoriaItem { Nombre = "Herramientas Eléctricas",   Icono = "⚡", ColorFondo = "#F39C12", Tag = "Herramientas eléctricas"  },
            new CategoriaItem { Nombre = "Pintura",      Icono = "🪣", ColorFondo = "#8E44AD", Tag = "Pintura"     },
            new CategoriaItem { Nombre = "Plomería",     Icono = "💧", ColorFondo = "#2980B9", Tag = "Plomería"    },
            new CategoriaItem { Nombre = "Tornillos",    Icono = "🔩", ColorFondo = "#7F8C8D", Tag = "Tornillos"   },
            new CategoriaItem { Nombre = "Cerraduras",   Icono = "🔒", ColorFondo = "#27AE60", Tag = "Cerraduras"  },
            new CategoriaItem { Nombre = "Materiales", Icono = "🧱", ColorFondo = "#A0522D", Tag = "Materiales" },
            new CategoriaItem { Nombre = "Iluminación", Icono = "💡", ColorFondo = "#F1C40F", Tag = "Iluminación" },
            new CategoriaItem { Nombre = "Adhesivos", Icono = "🧴", ColorFondo = "#16A085", Tag = "Adhesivos" },
            new CategoriaItem { Nombre = "Seguridad", Icono = "🦺", ColorFondo = "#D41515", Tag = "Seguridad" }
        };

        // ═══════════════════════════════════════════════════════════
        //  CONSTRUCTOR DE LA PAGINA
        public POSPage()
        {
            this.InitializeComponent();
            CargarProductos();
            BuscadorBox.AddHandler(
                PreviewKeyDownEvent,
                new KeyEventHandler(BuscadorBox_KeyDown),
                true); // true = captura antes que el control interno
            this.PointerPressed += POSPage_PointerPressed;
            RenderizarCategorias();
        }

        // ═══════════════════════════════════════════════════════════
        //  RECIBIR EL NOMBRE DE USUARIO DESDE EL LOGIN
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            CargarProductos();

            if (e.Parameter is string username && !string.IsNullOrEmpty(username))
            {
                TxtNombreEmpleado.Text = username;

                var partes = username.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string iniciales = partes.Length >= 2
                    ? $"{partes[0][0]}{partes[1][0]}"
                    : username[..Math.Min(2, username.Length)];
            }
            else
            {
                TxtNombreEmpleado.Text = "Empleado";
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  PANTALLA INICIAL: TARJETAS DE CATEGORÍAS
        private void RenderizarCategorias()
        {
            var tarjetas = _categorias.Select(c => CrearTarjetaCategoria(c)).ToList();
            CategoriasGrid.ItemsSource = tarjetas;
        }

        // - Opera mediante el catalago de categorias
        private Border CrearTarjetaCategoria(CategoriaItem cat)
        {
            var emoji = new TextBlock
            {
                Text = cat.Icono,
                FontSize = 90,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };

            // Contar productos de esta categoría
            int total = cat.Tag == "Todos"
                ? productos.Count
                : productos.Count(p => p.Categoria == cat.Tag);

            var nombre = new TextBlock
            {
                Text = cat.Nombre,
                FontSize = 15,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var cantidad = new TextBlock
            {
                Text = $"{total} producto{(total != 1 ? "s" : "")}",
                FontSize = 11,
                Foreground = new SolidColorBrush(ColorFromHex("#FFFFFF")),
                Opacity = 0.75,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var stack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 4
            };
            stack.Children.Add(emoji);
            stack.Children.Add(nombre);
            stack.Children.Add(cantidad);

            return new Border
            {
                Background = new SolidColorBrush(ColorFromHex(cat.ColorFondo)),
                CornerRadius = new CornerRadius(14),
                Width = 255,
                Height = 215,
                Margin = new Thickness(6),
                Child = stack,
                Tag = cat.Tag    // Se usa en CategoriasGrid_ItemClick
            };
        }

        // Clic en una tarjeta de categoría → ir a vista de productos
        private void CategoriasGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not Border border) return;
            string tag = border.Tag?.ToString() ?? "Todos";

            _categoriaFiltro = tag;
            _textoBusqueda = "";
            BuscadorBox.Text = "";

            TituloCategoria.Text = tag == "Todos" ? "Todos los productos" : tag;
            RenderizarProductos();

            // Cambiar vistas
            VistaCategorias.Visibility = Visibility.Collapsed;
            VistaProductos.Visibility = Visibility.Visible;
        }

        // Botón "← Categorías" → regresar a la pantalla inicial
        private void BtnVolverCategorias_Click(object sender, RoutedEventArgs e)
        {
            CerrarPanel();
            VistaProductos.Visibility = Visibility.Collapsed;
            VistaCategorias.Visibility = Visibility.Visible;
        }

        // ═══════════════════════════════════════════════════════════
        //  RENDERIZAR PRODUCTOS (aplica categoría + búsqueda)
        private void RenderizarProductos()
        {
            var filtrados = productos.Where(p =>
                (_categoriaFiltro == "Todos" || p.Categoria == _categoriaFiltro) &&
                (string.IsNullOrEmpty(_textoBusqueda) ||
                 p.Nombre.Contains(_textoBusqueda, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            var tarjetas = filtrados.Select(p => CrearTarjetaProducto(p)).ToList();
            ProductosGrid.ItemsSource = tarjetas;

            ContadorProductos.Text = $"{filtrados.Count} producto{(filtrados.Count != 1 ? "s" : "")}";
        }

        // - Crea la tarjeta de cada producto agarrando los datos centralizados del modelo.
        private Border CrearTarjetaProducto(Product producto)
        {
            // - Valida su stock y le da un color
            string colorFondoBadge;
            string colorTextoBadge;

            if (producto.Stock <= 10)
            {
                // 🔴 ROJO (crítico)
                colorFondoBadge = "#FDECEA";
                colorTextoBadge = "#C0392B";
            }
            else if (producto.Stock <= 20)
            {
                // 🟡 AMARILLO (bajo)
                colorFondoBadge = "#FFF8E1";
                colorTextoBadge = "#F39C12";
            }
            else
            {
                // 🟢 VERDE (bien)
                colorFondoBadge = "#E6F9EE";
                colorTextoBadge = "#27AE60";
            }

            // - Crea la tarjeta en un nuevo StackPanel
            var stackInfo = new StackPanel { Padding = new Thickness(12), Spacing = 4 };

            // - Le pone su nombre
            stackInfo.Children.Add(new TextBlock
            {
                Text = producto.Nombre,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 13,
                Foreground = new SolidColorBrush(ColorFromHex("#1E2333")),
                TextWrapping = TextWrapping.Wrap
            });

            // - Le pone su marca
            stackInfo.Children.Add(new TextBlock
            {
                Text = producto.Marca,
                FontSize = 11,
                Foreground = new SolidColorBrush(ColorFromHex("#7A8099"))
            });

            // - Le pone su precio
            stackInfo.Children.Add(new TextBlock
            {
                Text = $"${producto.Precio:N2}",
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new SolidColorBrush(producto.Activo
                                                ? ColorFromHex("#E8611A")
                                                : ColorFromHex("#A0A4B8")),
                Margin = new Thickness(0, 6, 0, 4)
            });

            // - Acomoda en una misma fila "Stock y Estado"
            var filaEstado = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Margin = new Thickness(0, 6, 0, 0)
            };

            // - Le pone su stock
            filaEstado.Children.Add(new Border
            {
                Background = new SolidColorBrush(producto.Activo
                                                ? ColorFromHex(colorFondoBadge)
                                                : ColorFromHex("#F2F3F7")),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6, 3, 6, 3),

                Child = new TextBlock
                {
                    Text = $"Stock: {producto.Stock}",
                    Foreground = new SolidColorBrush(producto.Activo
                                                    ? ColorFromHex(colorTextoBadge)
                                                    : ColorFromHex("#9AA0B5")),
                    FontSize = 11,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
                }
            });

            // - Le pone su estado
            filaEstado.Children.Add(new Border
            {
                Background = new SolidColorBrush(
                    producto.Activo
                        ? ColorFromHex("#E6F9EE")
                        : ColorFromHex("#FDECEA")),

                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6, 3, 6, 3),

                Child = new TextBlock
                {
                    Text = producto.Activo ? "Activo" : "Inactivo",

                    Foreground = new SolidColorBrush(
                        producto.Activo
                            ? ColorFromHex("#27AE60")
                            : ColorFromHex("#C0392B")),

                    FontSize = 11,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
                }
            });

            // - Nos lo regresa en la misma fila
            stackInfo.Children.Add(filaEstado);

            // - Le pone su icono
            var imagenBorder = new Border
            {
                Background = new SolidColorBrush(ColorFromHex("#FFF4E8")),
                CornerRadius = new CornerRadius(12, 12, 0, 0),
                Height = 85,
                Child = new TextBlock
                {
                    Text = producto.Icono,
                    FontSize = 42,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            // - Le pone su respectivo fondo
            var contenido = new StackPanel
            {
                Background = new SolidColorBrush(Colors.White)
            };
            contenido.Children.Add(imagenBorder);
            contenido.Children.Add(stackInfo);

            // - Nos retorna las tarjeta hecha correctamente
            return new Border
            {
                Background = new SolidColorBrush(Colors.White),
                Opacity = producto.Activo ? 1 : 0.55,
                CornerRadius = new CornerRadius(12),
                Width = 170,
                Margin = new Thickness(8),
                Child = contenido,
                Tag = producto
            };
        }

        // ═══════════════════════════════════════════════════════════
        //  REPORTE DE VENTAS
        private void BtnReporte_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ReportPage));
        }

        // ═══════════════════════════════════════════════════════════
        //  CERRAR SESIÓN
        private async void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            _carrito.Clear();
            await ConfirmarCerrarSesion();
        }

        private async Task ConfirmarCerrarSesion()
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Cerrar sesión",
                Content = "¿Estás seguro de realizar esta acción?",
                PrimaryButtonText = "Sí",
                CloseButtonText = "Volver",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var resultado = await dialog.ShowAsync();

            if (resultado == ContentDialogResult.Primary)
            {
                Frame.Navigate(typeof(LoginPage));
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  ABRIR / CERRAR PANEL DE VENTA
        private void BtnAbrirVenta_Click(object sender, RoutedEventArgs e) => TogglePanel();

        private void TogglePanel()
        {
            if (_panelAbierto) CerrarPanel();
            else AbrirPanel();
        }

        private void AbrirPanel()
        {
            _panelAbierto = true;
            ColCarrito.Width = new GridLength(340);
            VentaPanel.Visibility = Visibility.Visible;
        }

        private void CerrarPanel()
        {
            _panelAbierto = false;
            ColCarrito.Width = new GridLength(0);
            VentaPanel.Visibility = Visibility.Collapsed;
        }

        private void POSPage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!_panelAbierto) return;

            var punto = e.GetCurrentPoint(VentaPanel).Position;

            bool dentroDelPanel = punto.X >= 0 && punto.X <= VentaPanel.ActualWidth &&
                                  punto.Y >= 0 && punto.Y <= VentaPanel.ActualHeight;

            if (!dentroDelPanel)
                CerrarPanel();
        }

        // ═══════════════════════════════════════════════════════════
        //  CLIC EN UN PRODUCTO → AGREGAR AL CARRITO
        private async void ProductosGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not Border border) return;
            if (border.Tag is not Product producto) return;

            if (!producto.Activo)
            {
                await MostrarMensaje(
                    $"El producto \"{producto.Nombre}\" está inactivo y no puede venderse.");

                return;
            }

            string nombre = producto.Nombre;
            string marca = producto.Marca;
            decimal precio = producto.Precio;

            var existente = _carrito.Find(i => i.ProductId == producto.Id);

            // VALIDAR TOTAL EN CARRITO VS STOCK
            var totalEnCarrito = _carrito
                .Where(i => i.Nombre == producto.Nombre)
                .Sum(i => i.Cantidad);

            if (totalEnCarrito >= producto.Stock)
            {
                await MostrarMensaje($"Stock insuficiente para {producto.Nombre}");
                return;
            }

            if (existente != null)
            {
                // VALIDAR STOCK ANTES DE SUMAR
                if (existente.Cantidad >= producto.Stock)
                {
                    await MostrarMensaje($"Stock insuficiente para {producto.Nombre}");
                    return;
                }

                existente.Cantidad++;
                ActualizarFilaCarrito(existente);
            }
            else
            {
                // VALIDAR SI NO HAY STOCK
                if (producto.Stock <= 0)
                {
                    await MostrarMensaje($"Sin stock disponible para {producto.Nombre}");
                    return;
                }

                var item = new ItemCarrito
                {
                    ProductId = producto.Id,
                    Nombre = nombre,
                    Marca = marca,
                    Precio = precio,
                    Cantidad = 1
                };

                _carrito.Add(item);
                AgregarFilaCarrito(item);
            }

            MensajeVacio.Visibility = Visibility.Collapsed;
            ActualizarTotales();
            ContadorCarrito.Text = _carrito.Count.ToString();

            if (!_panelAbierto) AbrirPanel();
        }

        // ═══════════════════════════════════════════════════════════
        //  RENDERIZAR FILAS DEL CARRITO
        private void AgregarFilaCarrito(ItemCarrito item)
        {
            PanelCarrito.Children.Add(CrearFilaCarrito(item));
        }

        private void ActualizarFilaCarrito(ItemCarrito item)
        {
            foreach (var child in PanelCarrito.Children)
            {
                if (child is Border border && border.Tag is ItemCarrito tagItem
                    && tagItem.Nombre == item.Nombre)
                {
                    if (border.Child is Grid grid)
                        ActualizarTextosFila(grid, item);
                    break;
                }
            }
        }

        private Border CrearFilaCarrito(ItemCarrito item)
        {
            var grid = new Grid { Padding = new Thickness(16, 10, 16, 10) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var colIzq = new StackPanel { Spacing = 4 };

            colIzq.Children.Add(new TextBlock
            {
                Text = item.Nombre,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 13,
                Foreground = new SolidColorBrush(ColorFromHex("#1E2333")),
                TextWrapping = TextWrapping.Wrap
            });

            colIzq.Children.Add(new TextBlock
            {
                Text = item.Marca,
                FontSize = 11,
                Foreground = new SolidColorBrush(ColorFromHex("#7A8099"))
            });

            var stackCantidad = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Margin = new Thickness(0, 8, 0, 0)
            };

            var btnMenos = new Button
            {
                Content = "−",
                Width = 28,
                Height = 28,
                Padding = new Thickness(0),
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(ColorFromHex("#F0F2F8")),
                BorderThickness = new Thickness(0),
                FontSize = 16,
                Tag = item
            };
            btnMenos.Click += BtnMenos_Click;

            var txtCantidad = new TextBlock
            {
                Text = item.Cantidad.ToString(),
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new SolidColorBrush(ColorFromHex("#1E2333")),
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 20,
                TextAlignment = TextAlignment.Center,
                Tag = $"cantidad_{item.Nombre}"
            };

            var btnMas = new Button
            {
                Content = "+",
                Width = 28,
                Height = 28,
                Padding = new Thickness(0),
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(ColorFromHex("#E8611A")),
                BorderThickness = new Thickness(0),
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.White),
                Tag = item
            };
            btnMas.Click += BtnMas_Click;

            stackCantidad.Children.Add(btnMenos);
            stackCantidad.Children.Add(txtCantidad);
            stackCantidad.Children.Add(btnMas);
            colIzq.Children.Add(stackCantidad);

            var colDer = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 6
            };

            colDer.Children.Add(new TextBlock
            {
                Text = $"${item.Subtotal:N2}",
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new SolidColorBrush(ColorFromHex("#E8611A")),
                HorizontalAlignment = HorizontalAlignment.Right,
                Tag = $"subtotal_{item.Nombre}"
            });

            var btnEliminar = new Button
            {
                Content = new FontIcon
                {
                    Glyph = "\uE74D",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(ColorFromHex("#C0392B"))
                },
                Width = 28,
                Height = 28,
                Padding = new Thickness(0),
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(ColorFromHex("#FDECEA")),
                BorderThickness = new Thickness(0),
                Tag = item
            };
            btnEliminar.Click += BtnEliminar_Click;
            colDer.Children.Add(btnEliminar);

            Grid.SetColumn(colIzq, 0);
            Grid.SetColumn(colDer, 1);
            grid.Children.Add(colIzq);
            grid.Children.Add(colDer);

            return new Border
            {
                Child = grid,
                BorderBrush = new SolidColorBrush(ColorFromHex("#F0F2F8")),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Tag = item
            };
        }

        private void ActualizarTextosFila(Grid grid, ItemCarrito item)
        {
            foreach (var col in grid.Children)
            {
                if (col is not StackPanel sp) continue;
                foreach (var hijo in sp.Children)
                {
                    if (hijo is StackPanel stackCant)
                    {
                        foreach (var ctrl in stackCant.Children)
                            if (ctrl is TextBlock tb && tb.Tag?.ToString() == $"cantidad_{item.Nombre}")
                                tb.Text = item.Cantidad.ToString();
                    }
                    if (hijo is TextBlock tbSub && tbSub.Tag?.ToString() == $"subtotal_{item.Nombre}")
                        tbSub.Text = $"${item.Subtotal:N2}";
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  CONTROLES DE CANTIDAD (+/−) Y ELIMINAR
        private async void BtnMas_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ItemCarrito item)
            {
                var producto = productos.FirstOrDefault(p => p.Nombre == item.Nombre);
                if (producto == null) return;

                if (item.Cantidad >= producto.Stock)
                {
                    await MostrarMensaje($"Stock insuficiente para {producto.Nombre}");
                    return;
                }

                item.Cantidad++;
                RedibujarCarrito();
            }
        }

        private void BtnMenos_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ItemCarrito item)
            {
                item.Cantidad--;
                if (item.Cantidad <= 0) _carrito.Remove(item);
                RedibujarCarrito();
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ItemCarrito item)
            {
                _carrito.Remove(item);
                RedibujarCarrito();
            }
        }

        private void RedibujarCarrito()
        {
            PanelCarrito.Children.Clear();

            if (_carrito.Count == 0)
            {
                MensajeVacio.Visibility = Visibility.Visible;
                PanelCarrito.Children.Add(MensajeVacio);
            }
            else
            {
                MensajeVacio.Visibility = Visibility.Collapsed;
                foreach (var item in _carrito)
                    PanelCarrito.Children.Add(CrearFilaCarrito(item));
            }

            ActualizarTotales();
            ContadorCarrito.Text = _carrito.Count.ToString();
        }

        // ═══════════════════════════════════════════════════════════
        //  CÁLCULO DE TOTALES
        private void ActualizarTotales()
        {
            decimal subtotal = _carrito.Sum(i => i.Subtotal);
            decimal iva = subtotal * TasaIva;
            decimal total = subtotal + iva;

            TxtSubtotal.Text = $"${subtotal:N2}";
            TxtIva.Text = $"${iva:N2}";
            TxtTotal.Text = $"${total:N2}";
        }

        // ═══════════════════════════════════════════════════════════
        //  MÉTODO DE PAGO
        private void BtnMetodoPago_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Border border ) return;
            _metodoPago = border.Tag?.ToString() ?? "Efectivo";
            ActualizarEstiloMetodoPago();
        }

        private void ActualizarEstiloMetodoPago()
        {
            var naranja = new SolidColorBrush(ColorFromHex("#E8611A"));
            var gris = new SolidColorBrush(ColorFromHex("#D0D4E8"));
            var textoNaranja = new SolidColorBrush(ColorFromHex("#E8611A"));
            var textoGris = new SolidColorBrush(ColorFromHex("#5A6080"));

            if (_metodoPago == "Efectivo")
            {
                BtnEfectivo.BorderBrush = naranja;
                BtnEfectivo.BorderThickness = new Thickness(1.5);
                BtnTarjeta.BorderBrush = gris;
                BtnTarjeta.BorderThickness = new Thickness(1);
                TxtEfectivo.Foreground = textoNaranja;
                TxtTarjeta.Foreground = textoGris;
                IconTarjeta.Foreground = textoGris;
            }
            else
            {
                BtnTarjeta.BorderBrush = naranja;
                BtnTarjeta.BorderThickness = new Thickness(1.5);
                BtnEfectivo.BorderBrush = gris;
                BtnEfectivo.BorderThickness = new Thickness(1);
                TxtTarjeta.Foreground = textoNaranja;
                TxtEfectivo.Foreground = textoGris;
                IconTarjeta.Foreground = textoNaranja;
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  CANCELAR VENTA
        private void BtnCancelarVenta_Click(object sender, RoutedEventArgs e)
        {
            _carrito.Clear();
            RedibujarCarrito();
            CerrarPanel();
        }

        // ═══════════════════════════════════════════════════════════
        //  COBRAR
        private bool _procesandoVenta = false;

        private async Task<bool> AplicarVenta()
        {
            foreach (var item in _carrito)
            {
                var producto = productos.FirstOrDefault(p => p.Id == item.ProductId);

                if (producto != null && producto.Stock < item.Cantidad)
                {
                    await MostrarMensaje($"Stock insuficiente para {producto.Nombre}");
                    return false;
                }
            }

            foreach (var item in _carrito)
            {
                var producto = productos.FirstOrDefault(p => p.Id == item.ProductId);
                if (producto != null)
                {
                    producto.Stock -= item.Cantidad;

                    producto.VecesVendido += item.Cantidad;
                    producto.TotalGenerado += item.Cantidad * producto.Precio;
                }
            }

            var venta = new Venta
            {
                MetodoPago = _metodoPago,
                Items = _carrito.Select(i => new ItemVenta
                {
                    ProductId = i.ProductId,
                    Nombre = i.Nombre,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.Precio
                }).ToList(),

                Total = _carrito.Sum(i => i.Cantidad * i.Precio)
            };

            var ventas = ventaService.Obtener();

            ventas.Add(venta);

            ventaService.Guardar(ventas);

            // Primero guardar productos correctamente
            servicio.Guardar(productos);

            // SOLO si todo salió bien generar ticket
            string ticketPath = TicketService.GenerarTicket(venta);

            ContentDialog dialog = new ContentDialog
            {
                Title = "Ticket generado",
                Content = "¿Desea visualizar el ticket de compra?",
                PrimaryButtonText = "Sí",
                CloseButtonText = "No",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = ticketPath,
                    UseShellExecute = true
                });
            }

            return true;
        }

        private async void BtnCobrar_Click(object sender, RoutedEventArgs e)
        {
            if (_procesandoVenta) return;
            _procesandoVenta = true;

            if (_carrito.Count == 0)
            {
                var dlg = new ContentDialog
                {
                    Title = "Carrito vacío",
                    Content = "Debes agregar un producto antes de cobrar.",
                    CloseButtonText = "Entendido",
                    XamlRoot = this.XamlRoot
                };
                await dlg.ShowAsync();
                _procesandoVenta = false;
                return;
            }


            // Si el método de pago es tarjeta, pedir NIP
            if (_metodoPago == "Tarjeta")
            {
                var nipBox = new PasswordBox
                {
                    PlaceholderText = "Ingresa tu NIP (4)",
                    MaxLength = 4,
                    Margin = new Thickness(0, 8, 0, 0)
                };

                var dialogNip = new ContentDialog
                {
                    Title = "Pago con tarjeta",
                    Content = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text = $"Total a cobrar: {TxtTotal.Text}",
                                Foreground = new SolidColorBrush(ColorFromHex("#E8611A")),
                                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                                FontSize = 16
                            },
                            nipBox
                        }
                    },
                    PrimaryButtonText = "Confirmar pago",
                    CloseButtonText = "Cancelar",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot
                };

                var resultadoNip = await dialogNip.ShowAsync();

                // Si canceló el NIP, no procesar venta
                if (resultadoNip != ContentDialogResult.Primary)
                {
                    _procesandoVenta = false;
                    return;
                }

                // Validar que el NIP tenga exactamente 4 dígitos 
                var nip = nipBox.Password;
                if (nip.Length != 4 || !nip.All(char.IsDigit))
                {
                    await MostrarMensaje("El NIP debe ser de exactamente 4 dígitos numéricos.");
                    _procesandoVenta = false;
                    return;
                }
            }
            else
            {
                // Confirmación normal para efectivo
                var confirmacion = new ContentDialog
                {
                    Title = "Venta registrada",
                    Content = $"Método de pago: {_metodoPago}\nTotal: {TxtTotal.Text}",
                    PrimaryButtonText = "Aceptar",
                    XamlRoot = this.XamlRoot
                };
                await confirmacion.ShowAsync();
            }



            bool ventaOk = await AplicarVenta();

            if (!ventaOk)
            {
                _procesandoVenta = false;
                return;
            }

            _carrito.Clear();
            RedibujarCarrito();
            CerrarPanel();
            RenderizarProductos();

            _procesandoVenta = false;
        }

        // ═══════════════════════════════════════════════════════════
        //  Barra de busqueda
        private void BuscadorBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            _textoBusqueda = sender.Text.Trim();

            if (!string.IsNullOrEmpty(_textoBusqueda))
            {
                // Si estamos buscando, mostrar todos los productos filtrados por texto
                _categoriaFiltro = "Todos";
                TituloCategoria.Text = $"Resultados: \"{_textoBusqueda}\"";
                RenderizarProductos();

                // Cambiar a la vista de productos si aún estamos en categorías
                if (VistaCategorias.Visibility == Visibility.Visible)
                {
                    VistaCategorias.Visibility = Visibility.Collapsed;
                    VistaProductos.Visibility = Visibility.Visible;
                }
            }
            else
            {
                // Si borraron el texto, volver a la pantalla de categorías
                VistaProductos.Visibility = Visibility.Collapsed;
                VistaCategorias.Visibility = Visibility.Visible;
            }
        }

        // Handler que bloquea la tecla antes de que escriba:
        private void BuscadorBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            const int MaxCaracteres = 50;

            // Permite siempre: Backspace, Delete, flechas, Tab, Enter
            bool esTeclaControl =
                e.Key == Windows.System.VirtualKey.Back ||
                e.Key == Windows.System.VirtualKey.Delete ||
                e.Key == Windows.System.VirtualKey.Left ||
                e.Key == Windows.System.VirtualKey.Right ||
                e.Key == Windows.System.VirtualKey.Tab ||
                e.Key == Windows.System.VirtualKey.Enter;

            if (esTeclaControl) return;

            // Si ya llegó al límite, bloquea la tecla completamente
            if (BuscadorBox.Text.Length >= MaxCaracteres)
                e.Handled = true; // ← esto evita que la tecla se procese
        }

        // ═══════════════════════════════════════════════════════════
        //  UTILIDAD: Color desde cadena hexadecimal
        private static Windows.UI.Color ColorFromHex(string hex)
        {
            hex = hex.TrimStart('#');
            byte a = 255, r, g, b;

            if (hex.Length == 6)
            {
                r = Convert.ToByte(hex[0..2], 16);
                g = Convert.ToByte(hex[2..4], 16);
                b = Convert.ToByte(hex[4..6], 16);
            }
            else if (hex.Length == 8)
            {
                a = Convert.ToByte(hex[0..2], 16);
                r = Convert.ToByte(hex[2..4], 16);
                g = Convert.ToByte(hex[4..6], 16);
                b = Convert.ToByte(hex[6..8], 16);
            }
            else return Colors.Gray;

            return Windows.UI.Color.FromArgb(a, r, g, b);
        }

        // - Clase generica para Mostrar Mensajes en pantalla
        private async Task MostrarMensaje(string texto)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Información",
                Content = texto,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
