using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using System.Text.Json;
using PIA.Models;
using PIA.Services;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace PIA;

public sealed partial class AdminPage : Page
{
    // Funciones globales para hacer funcionar Admin.
    private List<Product> productos = new();

    private ProductService servicio = new();

    private Product? seleccionado = null;

    private string nombreAdmin = "Admin";

    private string? mensajePendiente;

    // - Función para hacer que el botón de "Agregar" te mande a la pestaña de Agregar Producto
    private void GoToAdd_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(AddProductPage));
    }

    // - Función para hacer que el botón de "Editar" te mande a la pestaña de Editar Producto
    private void GoToEdit_Click(object sender, RoutedEventArgs e)
    {
        if (ListaProductos.SelectedItem is Product seleccionado)
        {
            Frame.Navigate(typeof(EditProductPage), seleccionado);
        }
    }

    // - Función para hacer que el botoón de "Eliminar" elimine correctamente el producto del Json.
    private async void GoToDelete_Click(object sender, RoutedEventArgs e)
    {
        if (ListaProductos.SelectedItem is not Product seleccionado)
            return;

        ContentDialog dialog = new ContentDialog
        {
            Title = "Eliminar producto",
            Content = $"¿Deseas eliminar el producto \"{seleccionado.Nombre}\"?",
            PrimaryButtonText = "Eliminar",
            CloseButtonText = "Cancelar",
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            productos.Remove(seleccionado);
            servicio.Guardar(productos);

            Cargar();

            await MostrarMensaje($"Producto \"{seleccionado.Nombre}\" eliminado correctamente.");
        }
    }

    // - Función para activar y/o desactivar productos mediante el Toggle
    private void EstadoToggle_Toggled(object sender, RoutedEventArgs e)
    {
        servicio.Guardar(productos);
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

    // ═══════════════════════════════════════════════════════════
    //  BARRA DE BUSQUEDA
    // 1. Se activa cuando el usuario escribe, borra o pega texto en la barra
    private void BuscadorBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (productos == null)
            return;

        // 3. Se ejecuta el filtro de busqueda. Toma lo que el usuario escribió y convierte el texto a minusculas.
        // La busqueda NO distingue mayúsculas/minusculas
        string filtro = sender.Text.ToLower();

        // 4. Busca coincidencias en los productos. Teniendo en cuenta (Nombre, Categoria, Marca)
        var filtrados = productos
            .Where(p =>
                p.Nombre.ToLower().Contains(filtro) ||
                p.Categoria.ToLower().Contains(filtro) ||
                p.Marca.ToLower().Contains(filtro))
            .ToList();

        // 5. Actualiza la lista visual
        ListaProductos.ItemsSource = filtrados;
    }

    // 2. Se le aplica un limite REAL de 50 caracteres
    private void BuscadorBox_Loaded(object sender, RoutedEventArgs e)
    {
        // 2.1. Busca el Textbox interno oculto dentro del AutoSuggestBox que es la barra de busqueda.
        TextBox? textBox = FindVisualChild<TextBox>(BuscadorBox);

        // 2.2. Valida que el texto que escribe el usuario esté en el limite 
        if (textBox != null)
        {
            textBox.MaxLength = 50;
        }
    }

    // Recorre visualmente TODOS los controles hijos que existen dentro de otro control. (punto 2.1)
    private T? FindVisualChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        // Cuenta cuántos controles internos tiene el control actual
        int count = VisualTreeHelper.GetChildrenCount(parent);

        // Los recorre
        for (int i = 0; i < count; i++)
        {
            // Obtiene el actual
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);

            //Verifica si es el tipo buscado. Si es 'Textbox', lo devuelve inmediatamente.
            if (child is T typedChild)
                return typedChild;

            // Si no lo encuentra, sigue buscando
            T? descendant = FindVisualChild<T>(child);

            if (descendant != null)
                return descendant;
        }

        return null;
    }

    // ═══════════════════════════════════════════════════════════
    //  DESELECCIONAR PRODUCTO HACIENDO CLICK EN ESPACIO VACIO
    private void Background_Click(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ListaProductos.SelectedItem = null;
    }

    // ═══════════════════════════════════════════════════════════
    //  CONSTRUCTOR DE LA PAGINA
    public AdminPage()
    {
        InitializeComponent();

        Cargar();

        this.Loaded += AdminPage_Loaded;
    }

    // ═══════════════════════════════════════════════════════════
    // - Cuando Admin termine de abrirse, si alguien dejó un mensaje pendiente, mostrarlo automáticamente.
    // - (Se usa para mostrar los mensajes de Agregar y Editar producto correctamente)
    private async void AdminPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(mensajePendiente))
        {
            await MostrarMensaje(mensajePendiente);
            mensajePendiente = null;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  CARGAR PRODUCTOS DESDE JSON
    private void Cargar()
    {
        productos = servicio.Obtener();

        ListaProductos.ItemsSource = null;
        ListaProductos.ItemsSource = productos;
    }

    // ═══════════════════════════════════════════════════════════
    //  RECIBIR NOMBRE ADMIN DESDE LOGIN
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is string param)
        {
            if (param.StartsWith("MSG|"))
            {
                mensajePendiente = param.Replace("MSG|", "");
            }
            else
            {
                nombreAdmin = param;
            }
        }

        TxtNombreAdmin.Text = nombreAdmin;
    }


    // ═══════════════════════════════════════════════════════════
    //  CERRAR SESIÓN
    private async void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
    {
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
}