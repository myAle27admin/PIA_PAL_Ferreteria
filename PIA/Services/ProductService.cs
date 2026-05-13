using PIA.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Windows.Storage;

namespace PIA.Services
{
    public class ProductService
    {
        private readonly string ruta =
            Path.Combine(
                Windows.Storage.ApplicationData.Current.LocalFolder.Path,
                "products.json");

        public List<Product> Obtener()
        {
            if (!File.Exists(ruta))
                return new List<Product>();

            string json = File.ReadAllText(ruta);

            var productos = JsonSerializer.Deserialize<List<Product>>(json)
               ?? new List<Product>();

            foreach (var p in productos)
            {
                if (p.Id == Guid.Empty)
                    p.Id = Guid.NewGuid();
            }

            return productos;
        }

        public void Guardar(List<Product> lista)
        {
            string json = JsonSerializer.Serialize(lista, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);

            File.WriteAllText(ruta, json);
        }
    }
}
