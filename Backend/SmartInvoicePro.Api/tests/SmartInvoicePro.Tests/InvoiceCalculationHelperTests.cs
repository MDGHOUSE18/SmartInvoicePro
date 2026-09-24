using SmartInvoicePro.Application.DTOs.Invoice;
using SmartInvoicePro.Application.Helpers;
using SmartInvoicePro.Domain.Enums;

namespace SmartInvoicePro.Tests;

public class InvoiceCalculationHelperTests
{
    [Fact]
    public void DetermineTaxType_SameState_ReturnsCgstSgst()
    {
        var result = InvoiceCalculationHelper.DetermineTaxType("Karnataka", "Karnataka");
        Assert.Equal(TaxTypes.CgstSgst, result);
    }

    [Fact]
    public void DetermineTaxType_DifferentState_ReturnsIgst()
    {
        var result = InvoiceCalculationHelper.DetermineTaxType("Karnataka", "Maharashtra");
        Assert.Equal(TaxTypes.Igst, result);
    }

    [Fact]
    public void Calculate_IntraState_SplitsCgstAndSgst()
    {
        var items = new List<CreateInvoiceItemDto>
        {
            new() { ProductName = "Service", Quantity = 1, UnitPrice = 1000, Discount = 0 }
        };

        var totals = InvoiceCalculationHelper.Calculate(items, 18m, TaxTypes.CgstSgst);

        Assert.Equal(1000m, totals.Subtotal);
        Assert.Equal(90m, totals.CGSTAmount);
        Assert.Equal(90m, totals.SGSTAmount);
        Assert.Equal(0m, totals.IGSTAmount);
        Assert.Equal(180m, totals.TaxAmount);
        Assert.Equal(1180m, totals.GrandTotal);
        Assert.Equal(TaxTypes.CgstSgst, totals.TaxType);
    }

    [Fact]
    public void Calculate_InterState_UsesIgst()
    {
        var items = new List<CreateInvoiceItemDto>
        {
            new() { ProductName = "Service", Quantity = 2, UnitPrice = 500, Discount = 0 }
        };

        var totals = InvoiceCalculationHelper.Calculate(items, 18m, TaxTypes.Igst);

        Assert.Equal(1000m, totals.Subtotal);
        Assert.Equal(0m, totals.CGSTAmount);
        Assert.Equal(0m, totals.SGSTAmount);
        Assert.Equal(180m, totals.IGSTAmount);
        Assert.Equal(1180m, totals.GrandTotal);
    }

    [Fact]
    public void CalculateLineTotal_AppliesDiscount()
    {
        var total = InvoiceCalculationHelper.CalculateLineTotal(3, 100, 50);
        Assert.Equal(250m, total);
    }
}
