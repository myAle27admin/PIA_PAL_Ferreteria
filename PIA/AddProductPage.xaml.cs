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
    public sealed partial class AddProductPage : Page
    {
        // - Una variable global para llamar el servicio de escritura en json de Producto /Services/ProductService.cs
        private ProductService servicio = new();
        // - Los datos se centralizan en una lista del modelo de Producto. /Models/Product.cs
        private List<Product> productos = new();

        // ═══════════════════════════════════════════════════════════
        //  CONSTRUCTOR DE LA PAGINA
        public AddProductPage()
        {
            this.InitializeComponent();

            productos = servicio.Obtener();
        }

        // GUARDAR PRODUCTO
        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
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

            // - Normaliza el poner "mArtillo, martillo, etc." para que sea Martillo
            string nombreNormalizado = NombreBox.Text.Trim();

            if (productos.Any(p => p.Nombre.Trim().ToLower() == nombreNormalizado.ToLower()))
            {
                await MostrarMensaje("Ese producto ya existe.");
                return;
            }

            // - Determina la categoria por lo seleccionado en el combobox
            var categoria = CategoriaBox.SelectedItem.ToString();

            // - Escribe un nuevo producto en la Lista centralizada de productos
            Product nuevo = new Product
            {
                Nombre = nombreNormalizado,
                Icono = string.IsNullOrWhiteSpace(IconoBox.Text) ? "📦" : IconoBox.Text,
                Categoria = categoria,
                Precio = precio,
                Stock = stock,
                Marca = MarcaBox.Text,
                FechaRegistro = DateTime.Now
            };
            productos.Add(nuevo);
            servicio.Guardar(productos);
            Frame.Navigate(typeof(AdminPage), $"MSG|Producto \"{nuevo.Nombre}\" agregado correctamente.");
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
                Guardar_Click(this, new RoutedEventArgs());
            }
        }
    }
}