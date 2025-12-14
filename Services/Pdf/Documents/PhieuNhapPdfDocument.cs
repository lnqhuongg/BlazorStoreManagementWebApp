using BlazorStoreManagementWebApp.DTOs.Admin.PhieuNhap;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using QuestPDF.Fluent;

namespace BlazorStoreManagementWebApp.Services.Pdf.Documents
{
    public class PhieuNhapPdfDocument : IDocument
    {
        private readonly PhieuNhapDTO phieu;

        public PhieuNhapPdfDocument(PhieuNhapDTO phieu)
        {
            this.phieu = phieu;
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata();

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(40);

                // ===== HEADER =====
                page.Header()
                    .Text($"PHIẾU NHẬP #{phieu.ImportId}")
                    .SemiBold()
                    .FontSize(22)
                    .AlignCenter();

                // ===== CONTENT =====
                page.Content().Column(col =>
                {
                    col.Spacing(12);

                    col.Item().Text($"Ngày nhập: {phieu.ImportDate:dd/MM/yyyy}");
                    col.Item().Text($"Nhà cung cấp: {phieu.Supplier.Name}");

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);


                    // BẢNG CHI TIẾT
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(4);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                        });

                        // Header bảng
                        table.Header(header =>
                        {
                            header.Cell().Text("Sản phẩm").SemiBold();
                            header.Cell().Text("Số lượng").SemiBold();
                            header.Cell().Text("Đơn giá").SemiBold();
                        });

                        // Dữ liệu bảng
                        foreach (var ct in phieu.ImportDetails)
                        {
                            table.Cell().Text(ct.Product.ProductName);
                            table.Cell().Text(ct.Quantity.ToString());
                            table.Cell().Text(ct.Price.ToString("N0") + " VND");
                        }
                    });

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // Tổng tiền
                    col.Item().AlignRight().Text($"Tổng tiền: {phieu.TotalAmount:N0} VND")
                        .SemiBold().FontSize(14);
                });

                // ===== FOOTER =====
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
