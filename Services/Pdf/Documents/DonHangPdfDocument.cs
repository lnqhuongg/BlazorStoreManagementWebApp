using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using QuestPDF.Fluent;
using BlazorStoreManagementWebApp.Services.Interfaces;

namespace BlazorStoreManagementWebApp.Services.Pdf.Documents
{
    public class DonHangPdfDocument : IDocument
    {
        private readonly DonHangDTO donHang;

        public DonHangPdfDocument(DonHangDTO donHang)
        {
            this.donHang = donHang;
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata();

        public void Compose(IDocumentContainer container)
        {
            var details = donHang.Items ?? new List<ChiTietDonHangDTO>();
            Console.WriteLine($"Generating PDF for Order ID: {donHang.OrderId} with {details.Count} items.");
            container.Page(page =>
            {
                page.Margin(40);

                page.Header()
                    .Text($"ĐƠN HÀNG #{donHang.OrderId}")
                    .SemiBold()
                    .FontSize(22)
                    .AlignCenter();

                page.Content().Column(col =>
                {
                    col.Spacing(12);

                    col.Item().Text($"Ngày đặt: {donHang.OrderDate:dd/MM/yyyy}");
                    col.Item().Text($"Khách hàng: {donHang.CustomerName}");

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(4);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Sản phẩm").SemiBold();
                            header.Cell().Text("Số lượng").SemiBold();
                            header.Cell().Text("Giá").SemiBold();
                        });

                        foreach (var ct in details)
                        {
                            table.Cell().Text(ct.Product.ProductName);
                            table.Cell().Text(ct.Quantity.ToString());
                            table.Cell().Text(ct.Price.ToString("N0") + " VND");
                        }
                    });

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().AlignRight()
                        .Text($"Tổng tiền: {donHang.TotalAmount:N0} VND")
                        .SemiBold()
                        .FontSize(14);
                });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Trang ").FontSize(10);
                        x.CurrentPageNumber();
                    });
            });
        }
    }

}
