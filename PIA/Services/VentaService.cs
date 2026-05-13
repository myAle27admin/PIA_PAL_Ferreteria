using PIA.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Windows.Storage;

namespace PIA.Services
{
    public class VentaService
    {
        private readonly string ruta =
            Path.Combine(ApplicationData.Current.LocalFolder.Path, "ventas.json");

        public List<Venta> Obtener()
        {
            if (!File.Exists(ruta))
                return new List<Venta>();

            var json = File.ReadAllText(ruta);
            return JsonSerializer.Deserialize<List<Venta>>(json) ?? new();
        }

        public void Guardar(List<Venta> ventas)
        {
            var json = JsonSerializer.Serialize(ventas, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(ruta, json);
        }
    }
}
