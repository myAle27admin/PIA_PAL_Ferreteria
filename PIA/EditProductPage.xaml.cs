using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using PIA.Models;
using PIA.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PIA
{
    public sealed partial class EditProductPage : Page
    {
        // - Una variable global para llamar el servicio de escritura en json de Producto /Services/ProductService.cs
        private ProductService servicio = new();
        // - Los datos se centralizan en una lista del modelo de Producto. /Models/Product.cs
        private List<Product> productos = new();
        // - Variable global que determina el producto que se selecciono para editar
        private Product? productoEditar;

        // ═══════════════════════════════════════════════════════════
        //  CONSTRUCTOR DE LA PAGINA
        public EditProductPage()
        {
            this.InitializeComponent();

            productos = servicio.Obtener();
        }

        // RECIBE PRODUCTO DESDE AdminPage
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Product producto)
            {
                productoEditar = producto;

                NombreBox.Text = producto.Nombre;
                IconoBox.Text = producto.Icono;
                CategoriaBox.SelectedItem = producto.Categoria;
                PrecioBox.Text = producto.Precio.ToString();
                StockBox.Text = producto.Stock.ToString();
                MarcaBox.Text = producto.Marca;
            }
        }

        // GUARDAR CAMBIOS
        private async void GuardarCambios_Click(object sender, RoutedEventArgs e)
        {
            if (productoEditar == null)
                return;

            // VALIDACIONES
            // - Hacer que Nombre y Categoria sean obligatorios
            if (string.IsNullOrWhiteSpace(NombreBox.Text) ||
                CategoriaBox.SelectedItem == null)
            {
                await MostrarMensaje("Nombre y categoría son obligatorios.");
                return;
            }

            // - Hacer que Precio no sea negativo
            if (!decimal.TryParse(PrecioBox.Text, out decimal precio) || precio <= 0)
            {
                await MostrarMensaje("Precio inválido.");
                return;
            }

            // - Hace que el stock no sea negativo
            if (!int.TryParse(StockBox.Text, out int stock) || stock < 0)
            {
                await MostrarMensaje("Stock inválido.");
                return;
            }

            // - Detecta si ya existe ese producto mediante el nombre normalizado en Agregar
            if (productos.Any(p => p.Nombre == NombreBox.Text && p.Id != productoEditar.Id))
            {
                await MostrarMensaje("Ya existe un producto con ese nombre.");
                return;
            }

            var producto = productos.FirstOrDefault(p => p.Id == productoEditar.Id);

            // - Escribe los cambios al producto en la Lista centralizada de productos
            if (producto != null)
            {
                producto.Nombre = NombreBox.Text;
                producto.Icono = string.IsNullOrWhiteSpace(IconoBox.Text) ? "📦" : IconoBox.Text;
                producto.Categoria = CategoriaBox.SelectedItem.ToString();
                producto.Precio = precio;
                producto.Stock = stock;
                producto.Marca = MarcaBox.Text ?? "";

                servicio.Guardar(productos);

                Frame.Navigate(typeof(AdminPage), $"MSG|Producto \"{producto.Nombre}\" actualizado correctamente.");
            }
        }

        // - Valida que solamente se utilicen numeros enteros en Stock
        private void SoloNumeros_Entero(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (!int.TryParse(args.NewText, out _) && args.NewText != "")
            {
                args.Cancel = true;
            }
        }

        // BOTÓN CANCELAR
        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AdminPage));
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

        // - Hace que el botón de "Guardar" funcione dandole a Enter
        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && !(FocusManager.GetFocusedElement() is TextBox tb && tb.AcceptsReturn))
            {
                GuardarCambios_Click(this, new RoutedEventArgs());
            }
        }
    }
}