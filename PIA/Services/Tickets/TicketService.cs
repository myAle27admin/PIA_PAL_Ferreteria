using PIA.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Diagnostics;
using System.IO;

namespace PIA.Services.Tickets
{
    public static class TicketService
    {
        public static string GenerarTicket(Venta venta)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            string carpeta = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "TicketsGenerados"
            );

            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            string folio = $"VTA-{DateTime.Now:yyyyMMddHHmmss}";

            string archivo = Path.Combine(
                carpeta,
                $"{folio}.pdf"
            );

            decimal subtotal = venta.Total;
            decimal iva = subtotal * 0.16m;
            decimal totalConIVA = subtotal + iva;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    // TAMAÑO TICKET
                    page.Size(226, 600);

                    page.Margin(10);

                    page.DefaultTextStyle(x =>
                        x.FontSize(10)
                    );

                    page.Content().Column(column =>
                    {
                        // LOGO / TITULO

                        column.Item()
                            .AlignCenter()
                            .Text("CASA DEL HERRERO")
                            .Bold()
                            .FontSize(16);

                        column.Item()
                            .AlignCenter()
                            .Text("FERRETERÍA");

                        column.Item()
                            .PaddingVertical(5)
                            .LineHorizontal(1);

                        // INFO

                        column.Item().Text($"Folio: {folio}");
                        column.Item().Text($"Fecha: {venta.Fecha:g}");
                        column.Item().Text($"Pago: {venta.MetodoPago}");

                        column.Item()
                            .PaddingVertical(5)
                            .LineHorizontal(1);

                        // PRODUCTOS

                        foreach (var item in venta.Items)
                        {
                            column.Item().PaddingVertical(2).Column(prod =>
                            {
                                prod.Item().Text(item.Nombre).Bold();

                                prod.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(
                                        $"{item.Cantidad} x ${item.PrecioUnitario:F2}"
                                    );

                                    row.ConstantItem(60)
                                        .AlignRight()
                                        .Text($"${item.Subtotal:F2}");
                                });
                            });
                        }

                        column.Item()
                            .PaddingVertical(5)
                            .LineHorizontal(1);

                        // TOTALES

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Subtotal");
                            row.ConstantItem(70)
                                .AlignRight()
                                .Text($"${subtotal:F2}");
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("IVA");
                            row.ConstantItem(70)
                                .AlignRight()
                                .Text($"${iva:F2}");
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem()
                                .Text("TOTAL")
                                .Bold();

                            row.ConstantItem(70)
                                .AlignRight()
                                .Text($"${totalConIVA:F2}")
                                .Bold();
                        });

                        column.Item()
                            .PaddingVertical(5)
                            .LineHorizontal(1);

                        // FOOTER

                        column.Item()
                            .PaddingTop(10)
                            .AlignCenter()
                            .Text("¡Gracias por su compra!")
                            .Bold();

                        column.Item()
                            .AlignCenter()
                            .Text("Vuelva pronto");
                    });
                });
            })
            .GeneratePdf(archivo);

            return archivo;
        }
}
}