using ClosedXML.Excel;
using SmartInvoicePro.Application.DTOs.Customer;
using SmartInvoicePro.Application.DTOs.Reports;
using SmartInvoicePro.Application.Interfaces;

namespace SmartInvoicePro.Infrastructure.Services;

public class ExcelExportService : IExcelExportService
{
    public Task<byte[]> ExportSalesReportAsync(SalesReportDto report)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Sales Report");

        ws.Cell(1, 1).Value = "Sales Report";
        ws.Cell(2, 1).Value = $"Period: {report.FromDate:dd MMM yyyy} - {report.ToDate:dd MMM yyyy}";
        ws.Cell(4, 1).Value = "Invoice Date";
        ws.Cell(4, 2).Value = "Invoice #";
        ws.Cell(4, 3).Value = "Customer";
        ws.Cell(4, 4).Value = "Status";
        ws.Cell(4, 5).Value = "Subtotal";
        ws.Cell(4, 6).Value = "Tax";
        ws.Cell(4, 7).Value = "Grand Total";
        ws.Cell(4, 8).Value = "Paid";

        var row = 5;
        foreach (var line in report.Lines)
        {
            ws.Cell(row, 1).Value = line.InvoiceDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 2).Value = line.InvoiceNumber;
            ws.Cell(row, 3).Value = line.CustomerName;
            ws.Cell(row, 4).Value = line.Status;
            ws.Cell(row, 5).Value = line.Subtotal;
            ws.Cell(row, 6).Value = line.TaxAmount;
            ws.Cell(row, 7).Value = line.GrandTotal;
            ws.Cell(row, 8).Value = line.AmountPaid;
            row++;
        }

        ws.Cell(row + 1, 6).Value = "Total Sales:";
        ws.Cell(row + 1, 7).Value = report.TotalSales;
        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    public Task<byte[]> ExportTaxReportAsync(TaxReportDto report)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Tax Report");

        ws.Cell(1, 1).Value = "Tax Report";
        ws.Cell(2, 1).Value = $"Period: {report.FromDate:dd MMM yyyy} - {report.ToDate:dd MMM yyyy}";
        ws.Cell(4, 1).Value = "Invoice #";
        ws.Cell(4, 2).Value = "Date";
        ws.Cell(4, 3).Value = "Customer";
        ws.Cell(4, 4).Value = "Tax Type";
        ws.Cell(4, 5).Value = "CGST";
        ws.Cell(4, 6).Value = "SGST";
        ws.Cell(4, 7).Value = "IGST";
        ws.Cell(4, 8).Value = "Total Tax";

        var row = 5;
        foreach (var line in report.Lines)
        {
            ws.Cell(row, 1).Value = line.InvoiceNumber;
            ws.Cell(row, 2).Value = line.InvoiceDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 3).Value = line.CustomerName;
            ws.Cell(row, 4).Value = line.TaxType;
            ws.Cell(row, 5).Value = line.CGSTAmount;
            ws.Cell(row, 6).Value = line.SGSTAmount;
            ws.Cell(row, 7).Value = line.IGSTAmount;
            ws.Cell(row, 8).Value = line.TaxAmount;
            row++;
        }

        ws.Cell(row + 1, 7).Value = "Totals:";
        ws.Cell(row + 1, 8).Value = report.TotalTax;
        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    public Task<byte[]> ExportCustomersAsync(IReadOnlyList<CustomerListDto> customers)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Customers");

        ws.Cell(1, 1).Value = "Customer Name";
        ws.Cell(1, 2).Value = "Company";
        ws.Cell(1, 3).Value = "Email";
        ws.Cell(1, 4).Value = "Phone";
        ws.Cell(1, 5).Value = "City";
        ws.Cell(1, 6).Value = "State";
        ws.Cell(1, 7).Value = "Invoices";

        var row = 2;
        foreach (var c in customers)
        {
            ws.Cell(row, 1).Value = c.CustomerName;
            ws.Cell(row, 2).Value = c.CompanyName ?? "";
            ws.Cell(row, 3).Value = c.Email;
            ws.Cell(row, 4).Value = c.Phone ?? "";
            ws.Cell(row, 5).Value = c.City ?? "";
            ws.Cell(row, 6).Value = c.State ?? "";
            ws.Cell(row, 7).Value = c.InvoiceCount;
            row++;
        }

        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
}
