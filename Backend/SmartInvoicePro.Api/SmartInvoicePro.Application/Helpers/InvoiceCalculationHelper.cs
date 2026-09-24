using SmartInvoicePro.Application.DTOs.Invoice;
using SmartInvoicePro.Domain.Enums;

namespace SmartInvoicePro.Application.Helpers;

public static class InvoiceCalculationHelper
{
    public record InvoiceTotals(
        decimal Subtotal,
        decimal CGSTAmount,
        decimal SGSTAmount,
        decimal IGSTAmount,
        decimal TaxAmount,
        decimal GrandTotal,
        string TaxType);

    public static string DetermineTaxType(string companyState, string? customerState)
    {
        if (string.IsNullOrWhiteSpace(companyState) || string.IsNullOrWhiteSpace(customerState))
            return TaxTypes.CgstSgst;

        return string.Equals(companyState.Trim(), customerState.Trim(), StringComparison.OrdinalIgnoreCase)
            ? TaxTypes.CgstSgst
            : TaxTypes.Igst;
    }

    public static InvoiceTotals Calculate(
        IEnumerable<CreateInvoiceItemDto> items,
        decimal taxPercentage,
        string taxType,
        decimal invoiceDiscount = 0)
    {
        decimal subtotal = 0;
        decimal cgstTotal = 0;
        decimal sgstTotal = 0;
        decimal igstTotal = 0;

        var halfRate = taxPercentage / 2m;

        foreach (var item in items)
        {
            var lineSubtotal = item.Quantity * item.UnitPrice - item.Discount;
            if (lineSubtotal < 0) lineSubtotal = 0;
            subtotal += lineSubtotal;

            if (taxType == TaxTypes.Igst)
            {
                igstTotal += Math.Round(lineSubtotal * taxPercentage / 100m, 2);
            }
            else
            {
                cgstTotal += Math.Round(lineSubtotal * halfRate / 100m, 2);
                sgstTotal += Math.Round(lineSubtotal * halfRate / 100m, 2);
            }
        }

        var taxAmount = cgstTotal + sgstTotal + igstTotal;
        var grandTotal = Math.Max(0, subtotal + taxAmount - invoiceDiscount);

        return new InvoiceTotals(
            Math.Round(subtotal, 2),
            Math.Round(cgstTotal, 2),
            Math.Round(sgstTotal, 2),
            Math.Round(igstTotal, 2),
            Math.Round(taxAmount, 2),
            Math.Round(grandTotal, 2),
            taxType);
    }

    public static decimal CalculateLineTotal(decimal quantity, decimal unitPrice, decimal discount)
    {
        var total = quantity * unitPrice - discount;
        return Math.Round(Math.Max(0, total), 2);
    }

    public static (decimal Cgst, decimal Sgst, decimal Igst) GetItemTaxPercentages(decimal taxPercentage, string taxType)
    {
        if (taxType == TaxTypes.Igst)
            return (0, 0, taxPercentage);

        var half = taxPercentage / 2m;
        return (half, half, 0);
    }
}
