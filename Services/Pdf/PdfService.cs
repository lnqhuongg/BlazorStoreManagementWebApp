using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.DTOs.Admin.PhieuNhap;
using BlazorStoreManagementWebApp.Services.Pdf.Documents;
using QuestPDF.Fluent;

public class PdfService
{
    public byte[] ExportPhieuNhap(PhieuNhapDTO phieu)
    {
        var doc = new PhieuNhapPdfDocument(phieu);
        return doc.GeneratePdf();
    }

    public byte[] ExportDonHang(DonHangDTO donHang)
    {
        var doc = new DonHangPdfDocument(donHang);
        return doc.GeneratePdf();
    }
    
}
