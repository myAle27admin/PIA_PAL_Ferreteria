using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PIA;

public sealed partial class LoginPage : Page
{
    private readonly string _dataFolder;
    private readonly string _usersFilePath;

    private string _perfilActivo = "Empleado";

    public LoginPage()
    {
        InitializeComponent();
        _dataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "UsuariosJson");
        _usersFilePath = Path.Combine(_dataFolder, "users.json");
    }

    // ═══════════════════════════════════════════════════════════
    //  MODELOS
    private record User(string Username, string Password, string Rol);

    // ═══════════════════════════════════════════════════════════
    //  SELECTOR DE PERFIL
    private void CardEmpleado_Tapped(object sender, TappedRoutedEventArgs e)
        => SeleccionarPerfil("Empleado");

    private void CardAdmin_Tapped(object sender, TappedRoutedEventArgs e)
        => SeleccionarPerfil("Admin");

    private void SeleccionarPerfil(string perfil)
    {
        _perfilActivo = perfil;
        StatusTextBlock.Text = "";

        // Perfil sin seleccion
        CardEmpleado.BorderBrush = new SolidColorBrush(ColorFromHex("#3A5068"));
        CardEmpleado.BorderThickness = new Thickness(1);
        DotEmpleado.Fill = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        IconEmpleado.Foreground = new SolidColorBrush(ColorFromHex("#8A99AA"));
        LblEmpleado.Foreground = new SolidColorBrush(ColorFromHex("#8A99AA"));

        CardAdmin.BorderBrush = new SolidColorBrush(ColorFromHex("#3A5068"));
        CardAdmin.BorderThickness = new Thickness(1);
        DotAdmin.Fill = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        IconAdmin.Foreground = new SolidColorBrush(ColorFromHex("#8A99AA"));
        LblAdmin.Foreground = new SolidColorBrush(ColorFromHex("#8A99AA"));

        // Activar al seleccionar un perfil
        if (perfil == "Empleado")
        {
            CardEmpleado.BorderBrush = new SolidColorBrush(ColorFromHex("#E8611A"));
            CardEmpleado.BorderThickness = new Thickness(2);
            DotEmpleado.Fill = new SolidColorBrush(ColorFromHex("#E8611A"));
            IconEmpleado.Foreground = new SolidColorBrush(ColorFromHex("#E8611A"));
            LblEmpleado.Foreground = new SolidColorBrush(ColorFromHex("#E8611A"));

            LinkCrearCuenta.Visibility = Visibility.Visible;
        }
        else
        {
            CardAdmin.BorderBrush = new SolidColorBrush(ColorFromHex("#E8611A"));
            CardAdmin.BorderThickness = new Thickness(2);
            DotAdmin.Fill = new SolidColorBrush(ColorFromHex("#E8611A"));
            IconAdmin.Foreground = new SolidColorBrush(ColorFromHex("#E8611A"));
            LblAdmin.Foreground = new SolidColorBrush(ColorFromHex("#E8611A"));

            LinkCrearCuenta.Visibility = Visibility.Collapsed;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  PERSISTENCIA
    private async Task<List<User>> LoadUsersAsync()
    {
        try
        {
            if (!Directory.Exists(_dataFolder)) Directory.CreateDirectory(_dataFolder);
            if (!File.Exists(_usersFilePath)) return new List<User>();

            using var stream = File.OpenRead(_usersFilePath);
            var users = await JsonSerializer.DeserializeAsync<List<User>>(stream);
            return users ?? new List<User>();
        }
        catch { return new List<User>(); }
    }

    private async Task SaveUsersAsync(List<User> users)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        using var stream = File.Create(_usersFilePath);
        await JsonSerializer.SerializeAsync(stream, users, options);
    }

    // ═══════════════════════════════════════════════════════════
    //  STATUS
    private void SetStatus(string text, bool isError = true)
    {
        StatusTextBlock.Text = text.ToUpper();
        StatusTextBlock.Foreground = isError
            ? new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 220, 80, 80))
            : new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 74, 180, 160));
    }

    // ═══════════════════════════════════════════════════════════
    //  CREAR CUENTA
    private async void LinkCrearCuenta_Click(object sender, RoutedEventArgs e)
    {
        var username = UsernameTextBox.Text?.Trim();
        var password = PasswordBox.Password ?? string.Empty;

        if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
        {
            SetStatus("Ingresa un usuario y contraseña para crear la cuenta");
            return;
        }

        if (string.IsNullOrEmpty(username))
        {
            SetStatus("Escribe el nombre de usuario para continuar");
            return;
        }

        if(username.Length < 4)
        {
            SetStatus("El usuario debe contener al menos 4 caracteres");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            SetStatus("Escribe una contraseña para continuar");
            return;
        }

        if (password.Length < 6)
        {
            SetStatus("La contraseña debe contener al menos 6 caracteres");
            return;
        }

        if (!password.Any(char.IsUpper))
        {
            SetStatus("Tu contraseña debe contener al menos 1 mayúscula");
            return;
        }

        if (!password.Any(char.IsDigit))
        {
            SetStatus("Tu contraseña debe contener al menos 1 número");
            return;
        }

        var users = await LoadUsersAsync();
        if (users.Any(u => u.Username == username))
        {
            SetStatus("El usuario ya existe");
            return;
        }

        users.Add(new User(username, password, "Empleado"));
        await SaveUsersAsync(users);
        SetStatus("¡Cuenta creada!", isError: false);
    }

    // ═══════════════════════════════════════════════════════════
    //  LOGIN
    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        var username = UsernameTextBox.Text?.Trim();
        var password = PasswordBox.Password ?? string.Empty;

        if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
        {
            SetStatus("Ingresa tu usuario y contraseña");
            return;
        }

        if (string.IsNullOrEmpty(username))
        {
            SetStatus("Escribe tu nombre de usuario");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            SetStatus("Escribe tu contraseña para continuar");
            return;
        }

        if (_perfilActivo == "Admin") // HARDCODEADO
        {
            if (username == "Admin" && password == "123456")
            {
                SetStatus("¡Acceso concedido!", isError: false);
                Frame.Navigate(typeof(AdminPage), username);
            }
            else
            {
                SetStatus("Credenciales de administrador incorrectas");
            }
            return;
        }

        var users = await LoadUsersAsync();
        var match = users.FirstOrDefault(u =>
            u.Username == username &&
            u.Password == password &&
            u.Rol == _perfilActivo);

        if (match is null)
        {
            SetStatus("Usuario o contraseña incorrectos");
            return;
        }

        SetStatus("¡Acceso concedido!", isError: false);

        if (_perfilActivo == "Empleado")
            Frame.Navigate(typeof(POSPage), username);
        else
            Frame.Navigate(typeof(AdminPage), username);
    }

    // Funcion de ENTER
    private void CampoLogin_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            Login_Click(sender, new RoutedEventArgs());
    }

    // ═══════════════════════════════════════════════════════════
    //  UTILIDAD
    private static Windows.UI.Color ColorFromHex(string hex)
    {
        hex = hex.TrimStart('#');
        byte r = Convert.ToByte(hex[0..2], 16);
        byte g = Convert.ToByte(hex[2..4], 16);
        byte b = Convert.ToByte(hex[4..6], 16);
        return Windows.UI.Color.FromArgb(255, r, g, b);
    }
}