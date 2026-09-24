using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartInvoicePro.Application.DTOs.CompanySettings;
using SmartInvoicePro.Application.DTOs.Invoice;
using SmartInvoicePro.Application.Interfaces;

namespace SmartInvoicePro.Infrastructure.Services;

public class PdfService : IPdfService
{
    private static readonly string Primary = "#0F766E";
    private static readonly string PrimaryLight = "#F0FDFA";
    private static readonly string Accent = "#14B8A6";
    private static readonly string TextDark = "#0F172A";
    private static readonly string TextMuted = "#64748B";
    private static readonly string Border = "#E2E8F0";
    private static readonly string TableHeader = "#CCFBF1";

    static PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenerateInvoicePdfAsync(InvoiceDto invoice, CompanySettingsDto company)
    {
        var symbol = invoice.CurrencyCode == "INR" ? "₹" : $"{invoice.CurrencyCode} ";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(45);
                page.MarginVertical(40);
                page.DefaultTextStyle(x => x.FontSize(9.5f).FontColor(TextDark));

                page.Header().Element(c => ComposeHeader(c, invoice, company));
                page.Content().Element(c => ComposeContent(c, invoice, company, symbol));
                page.Footer().Element(c => ComposeFooter(c));
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return Task.FromResult(stream.ToArray());
    }

    private static void ComposeHeader(IContainer container, InvoiceDto invoice, CompanySettingsDto company)
    {
        container.Column(col =>
        {
            col.Item().Height(4).Background(Primary);

            col.Item().PaddingTop(18).Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Row(r =>
                    {
                        r.ConstantItem(52).Height(52).Background(PrimaryLight).Border(1).BorderColor(Accent)
                            .AlignCenter().AlignMiddle()
                            .DefaultTextStyle(x => x.Bold().FontSize(16).FontColor(Primary))
                            .Text(GetCompanyInitials(company.CompanyName));

                        r.RelativeItem().PaddingLeft(12).Column(c =>
                        {
                            c.Item().DefaultTextStyle(x => x.Bold().FontSize(17).FontColor(TextDark))
                                .Text(company.CompanyName);
                            if (!string.IsNullOrWhiteSpace(company.Address))
                                c.Item().PaddingTop(2).DefaultTextStyle(x => x.FontColor(TextMuted))
                                    .Text(company.Address);
                            c.Item().DefaultTextStyle(x => x.FontColor(TextMuted))
                                .Text(FormatCityLine(company.City, company.State, company.PostalCode));
                            if (!string.IsNullOrWhiteSpace(company.TaxNumber))
                                c.Item().PaddingTop(4).Text(t =>
                                {
                                    t.Span("GSTIN: ").SemiBold().FontColor(TextDark);
                                    t.Span(company.TaxNumber).FontColor(TextMuted);
                                });
                            c.Item().PaddingTop(2).Text(t =>
                            {
                                if (!string.IsNullOrWhiteSpace(company.Email))
                                {
                                    t.Span("Email: ").SemiBold().FontColor(TextDark);
                                    t.Span(company.Email).FontColor(TextMuted);
                                }
                                if (!string.IsNullOrWhiteSpace(company.Phone))
                                {
                                    t.Span("  |  Phone: ").SemiBold().FontColor(TextDark);
                                    t.Span(company.Phone).FontColor(TextMuted);
                                }
                            });
                        });
                    });
                });

                row.ConstantItem(200).AlignRight().Column(right =>
                {
                    right.Item().AlignRight()
                        .DefaultTextStyle(x => x.Bold().FontSize(22).FontColor(Primary))
                        .Text("TAX INVOICE");
                    right.Item().PaddingTop(6).AlignRight()
                        .DefaultTextStyle(x => x.SemiBold().FontSize(11).FontColor(TextDark))
                        .Text($"#{invoice.InvoiceNumber}");
                    right.Item().PaddingTop(10).AlignRight().Column(meta =>
                    {
                        meta.Item().Element(e => MetaRow(e, "Invoice Date", invoice.InvoiceDate.ToString("dd MMM yyyy")));
                        meta.Item().PaddingTop(3).Element(e => MetaRow(e, "Due Date", invoice.DueDate.ToString("dd MMM yyyy")));
                        meta.Item().PaddingTop(3).Element(e => MetaRow(e, "Tax Type", FormatTaxType(invoice.TaxType)));
                        meta.Item().PaddingTop(6).AlignRight().Element(e => StatusBadge(e, invoice.Status));
                    });
                });
            });

            col.Item().PaddingTop(18).LineHorizontal(1.5f).LineColor(Primary);
        });
    }

    private static void ComposeContent(IContainer container, InvoiceDto invoice, CompanySettingsDto company, string symbol)
    {
        container.PaddingTop(16).Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Element(e => BillToBox(e, invoice));
                row.ConstantItem(24);
                row.RelativeItem().Element(e => InvoiceSummaryBox(e, invoice, symbol));
            });

            col.Item().PaddingTop(20).Element(e => ItemsTable(e, invoice, symbol));

            col.Item().PaddingTop(16).Row(row =>
            {
                row.RelativeItem();
                row.ConstantItem(280).Element(e => TotalsBox(e, invoice, symbol));
            });

            if (!string.IsNullOrWhiteSpace(invoice.Notes))
            {
                col.Item().PaddingTop(18).Element(e =>
                {
                    e.Background(PrimaryLight).Border(1).BorderColor(Border).Padding(12).Column(n =>
                    {
                        n.Item().DefaultTextStyle(x => x.Bold().FontSize(10).FontColor(Primary)).Text("Notes");
                        n.Item().PaddingTop(4).DefaultTextStyle(x => x.FontColor(TextMuted)).Text(invoice.Notes);
                    });
                });
            }

            col.Item().PaddingTop(20).Row(row =>
            {
                if (!string.IsNullOrWhiteSpace(company.BankName))
                    row.RelativeItem().Element(e => BankBox(e, company));

                if (!string.IsNullOrWhiteSpace(company.TermsAndConditions))
                {
                    if (!string.IsNullOrWhiteSpace(company.BankName))
                        row.ConstantItem(16);
                    row.RelativeItem().Element(e => TermsBox(e, company));
                }
            });
        });
    }

    private static void BillToBox(IContainer container, InvoiceDto invoice)
    {
        container.Border(1).BorderColor(Border).Background(Colors.White).Padding(14).Column(col =>
        {
            col.Item().DefaultTextStyle(x => x.Bold().FontSize(9).FontColor(Primary)).Text("BILL TO");
            col.Item().PaddingTop(8).DefaultTextStyle(x => x.Bold().FontSize(12).FontColor(TextDark))
                .Text(invoice.CustomerName);

            if (!string.IsNullOrWhiteSpace(invoice.CustomerCompanyName))
                col.Item().PaddingTop(2).DefaultTextStyle(x => x.SemiBold().FontColor(TextMuted))
                    .Text(invoice.CustomerCompanyName);

            if (!string.IsNullOrWhiteSpace(invoice.CustomerAddress))
                col.Item().PaddingTop(4).DefaultTextStyle(x => x.FontColor(TextMuted)).Text(invoice.CustomerAddress);

            var cityLine = FormatCityLine(invoice.CustomerCity, invoice.CustomerState, null);
            if (!string.IsNullOrWhiteSpace(cityLine))
                col.Item().DefaultTextStyle(x => x.FontColor(TextMuted)).Text(cityLine);

            if (!string.IsNullOrWhiteSpace(invoice.CustomerCountry))
                col.Item().DefaultTextStyle(x => x.FontColor(TextMuted)).Text(invoice.CustomerCountry);

            if (!string.IsNullOrWhiteSpace(invoice.CustomerTaxNumber))
                col.Item().PaddingTop(4).Text(t =>
                {
                    t.Span("GSTIN: ").SemiBold();
                    t.Span(invoice.CustomerTaxNumber).FontColor(TextMuted);
                });

            if (!string.IsNullOrWhiteSpace(invoice.CustomerEmail))
                col.Item().PaddingTop(2).DefaultTextStyle(x => x.FontColor(TextMuted)).Text(invoice.CustomerEmail);

            if (!string.IsNullOrWhiteSpace(invoice.CustomerPhone))
                col.Item().DefaultTextStyle(x => x.FontColor(TextMuted)).Text(invoice.CustomerPhone);
        });
    }

    private static void InvoiceSummaryBox(IContainer container, InvoiceDto invoice, string symbol)
    {
        container.Border(1).BorderColor(Border).Background(PrimaryLight).Padding(14).Column(col =>
        {
            col.Item().DefaultTextStyle(x => x.Bold().FontSize(9).FontColor(Primary)).Text("AMOUNT SUMMARY");
            col.Item().PaddingTop(10).Element(e => SummaryLine(e, "Subtotal", FormatMoney(symbol, invoice.Subtotal)));

            var (cgstRate, sgstRate, igstRate, totalTaxRate) = GetInvoiceTaxRates(invoice);
            if (invoice.CGSTAmount > 0)
                col.Item().PaddingTop(4).Element(e => SummaryLine(e, $"CGST ({cgstRate:N1}%)", FormatMoney(symbol, invoice.CGSTAmount)));
            if (invoice.SGSTAmount > 0)
                col.Item().PaddingTop(4).Element(e => SummaryLine(e, $"SGST ({sgstRate:N1}%)", FormatMoney(symbol, invoice.SGSTAmount)));
            if (invoice.IGSTAmount > 0)
                col.Item().PaddingTop(4).Element(e => SummaryLine(e, $"IGST ({igstRate:N1}%)", FormatMoney(symbol, invoice.IGSTAmount)));
            if (invoice.TaxAmount > 0)
                col.Item().PaddingTop(4).Element(e => SummaryLine(e, $"Total Tax ({totalTaxRate:N1}%)", FormatMoney(symbol, invoice.TaxAmount)));
            if (invoice.DiscountAmount > 0)
                col.Item().PaddingTop(4).Element(e => SummaryLine(e, "Discount", $"-{FormatMoney(symbol, invoice.DiscountAmount)}"));

            col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Accent);
            col.Item().PaddingTop(8).Element(e => SummaryLine(e, "Grand Total", FormatMoney(symbol, invoice.GrandTotal), bold: true, large: true));
            col.Item().PaddingTop(6).Element(e => SummaryLine(e, "Paid", FormatMoney(symbol, invoice.AmountPaid)));
            col.Item().PaddingTop(4).Element(e => SummaryLine(e, "Balance Due", FormatMoney(symbol, invoice.BalanceAmount), bold: true));
        });
    }

    private static void ItemsTable(IContainer container, InvoiceDto invoice, string symbol)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(28);
                columns.RelativeColumn(3.5f);
                columns.RelativeColumn(0.9f);
                columns.RelativeColumn(1.1f);
                columns.RelativeColumn(0.9f);
                columns.RelativeColumn(1f);
                columns.RelativeColumn(1.2f);
            });

            table.Header(header =>
            {
                header.Cell().Element(c => HeaderCell(c).DefaultTextStyle(x => x.Bold().FontColor(TextDark)).Text("#"));
                header.Cell().Element(c => HeaderCell(c).DefaultTextStyle(x => x.Bold().FontColor(TextDark)).Text("Item / Description"));
                header.Cell().Element(c => HeaderCell(c).AlignRight().DefaultTextStyle(x => x.Bold().FontColor(TextDark)).Text("Qty"));
                header.Cell().Element(c => HeaderCell(c).AlignRight().DefaultTextStyle(x => x.Bold().FontColor(TextDark)).Text("Rate"));
                header.Cell().Element(c => HeaderCell(c).AlignRight().DefaultTextStyle(x => x.Bold().FontColor(TextDark)).Text("Disc"));
                header.Cell().Element(c => HeaderCell(c).AlignRight().DefaultTextStyle(x => x.Bold().FontColor(TextDark)).Text("Tax %"));
                header.Cell().Element(c => HeaderCell(c).AlignRight().DefaultTextStyle(x => x.Bold().FontColor(TextDark)).Text("Amount"));
            });

            var index = 1;
            foreach (var item in invoice.Items)
            {
                var isEven = index % 2 == 0;
                table.Cell().Element(c => BodyCell(c, isEven).AlignCenter().DefaultTextStyle(x => x.FontColor(TextMuted)).Text(index.ToString()));
                table.Cell().Element(c => BodyCell(c, isEven).Column(col =>
                {
                    col.Item().DefaultTextStyle(x => x.SemiBold()).Text(item.ProductName);
                    if (!string.IsNullOrWhiteSpace(item.Description))
                        col.Item().PaddingTop(2).DefaultTextStyle(x => x.FontSize(8.5f).FontColor(TextMuted))
                            .Text(item.Description);
                    var taxLabel = GetTaxLabel(item, invoice.TaxType);
                    if (!string.IsNullOrWhiteSpace(taxLabel))
                        col.Item().PaddingTop(2).DefaultTextStyle(x => x.FontSize(8f).FontColor(Accent)).Text(taxLabel);
                }));
                table.Cell().Element(c => BodyCell(c, isEven).AlignRight().Text(item.Quantity.ToString("N2")));
                table.Cell().Element(c => BodyCell(c, isEven).AlignRight().Text(FormatMoney(symbol, item.UnitPrice)));
                table.Cell().Element(c => BodyCell(c, isEven).AlignRight().Text(FormatMoney(symbol, item.Discount)));
                table.Cell().Element(c => BodyCell(c, isEven).AlignRight()
                    .DefaultTextStyle(x => x.FontSize(8.5f).FontColor(Accent))
                    .Text(GetItemTaxPercentDisplay(item, invoice.TaxType)));
                table.Cell().Element(c => BodyCell(c, isEven).AlignRight().DefaultTextStyle(x => x.SemiBold()).Text(FormatMoney(symbol, item.LineTotal)));
                index++;
            }
        });
    }

    private static void TotalsBox(IContainer container, InvoiceDto invoice, string symbol)
    {
        container.Border(1).BorderColor(Primary).Background(Colors.White).Padding(14).Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().DefaultTextStyle(x => x.Bold().FontSize(11).FontColor(TextDark)).Text("Total Payable");
                row.ConstantItem(120).AlignRight()
                    .DefaultTextStyle(x => x.Bold().FontSize(14).FontColor(Primary))
                    .Text(FormatMoney(symbol, invoice.GrandTotal));
            });
            col.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().DefaultTextStyle(x => x.FontColor(TextMuted)).Text("Amount Paid");
                row.ConstantItem(120).AlignRight().DefaultTextStyle(x => x.FontColor(TextMuted))
                    .Text(FormatMoney(symbol, invoice.AmountPaid));
            });
            col.Item().PaddingTop(4).Row(row =>
            {
                var balanceColor = invoice.BalanceAmount > 0 ? "#DC2626" : Primary;
                row.RelativeItem().DefaultTextStyle(x => x.SemiBold().FontColor(TextDark)).Text("Balance Due");
                row.ConstantItem(120).AlignRight()
                    .DefaultTextStyle(x => x.SemiBold().FontColor(balanceColor))
                    .Text(FormatMoney(symbol, invoice.BalanceAmount));
            });

            if (invoice.TaxAmount > 0)
            {
                var (_, _, _, totalTaxRate) = GetInvoiceTaxRates(invoice);
                col.Item().PaddingTop(10).LineHorizontal(0.5f).LineColor(Border);
                col.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem().DefaultTextStyle(x => x.FontSize(8.5f).FontColor(TextMuted))
                        .Text($"GST @ {totalTaxRate:N1}% included in total");
                    row.ConstantItem(120).AlignRight().DefaultTextStyle(x => x.FontSize(8.5f).FontColor(TextMuted))
                        .Text(FormatMoney(symbol, invoice.TaxAmount));
                });
            }
        });
    }

    private static void BankBox(IContainer container, CompanySettingsDto company)
    {
        container.Border(1).BorderColor(Border).Padding(12).Column(col =>
        {
            col.Item().DefaultTextStyle(x => x.Bold().FontSize(10).FontColor(Primary)).Text("Bank Details");
            col.Item().PaddingTop(6).DefaultTextStyle(x => x.FontColor(TextMuted)).Text($"Bank: {company.BankName}");
            col.Item().PaddingTop(2).DefaultTextStyle(x => x.FontColor(TextMuted)).Text($"A/C: {company.BankAccountNumber}");
            col.Item().PaddingTop(2).DefaultTextStyle(x => x.FontColor(TextMuted)).Text($"IFSC: {company.BankIFSC}");
        });
    }

    private static void TermsBox(IContainer container, CompanySettingsDto company)
    {
        container.Border(1).BorderColor(Border).Padding(12).Column(col =>
        {
            col.Item().DefaultTextStyle(x => x.Bold().FontSize(10).FontColor(Primary)).Text("Terms & Conditions");
            col.Item().PaddingTop(6).DefaultTextStyle(x => x.FontSize(8.5f).FontColor(TextMuted).LineHeight(1.4f))
                .Text(company.TermsAndConditions!);
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(0.5f).LineColor(Border);
            col.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().DefaultTextStyle(x => x.FontSize(8).FontColor(TextMuted))
                    .Text("Generated by SmartInvoice Pro");
                row.RelativeItem().AlignCenter().DefaultTextStyle(x => x.FontSize(8).FontColor(TextMuted))
                    .Text("Thank you for your business");
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Page ").FontSize(8).FontColor(TextMuted);
                    text.CurrentPageNumber().FontSize(8).FontColor(TextMuted);
                    text.Span(" of ").FontSize(8).FontColor(TextMuted);
                    text.TotalPages().FontSize(8).FontColor(TextMuted);
                });
            });
        });
    }

    private static IContainer HeaderCell(IContainer container) =>
        container.Background(TableHeader).BorderBottom(1).BorderColor(Accent).PaddingVertical(8).PaddingHorizontal(6);

    private static IContainer BodyCell(IContainer container, bool alternate) =>
        container.Background(alternate ? "#FAFAFA" : Colors.White).BorderBottom(1).BorderColor(Border)
            .PaddingVertical(8).PaddingHorizontal(6);

    private static void MetaRow(IContainer container, string label, string value)
    {
        container.Row(row =>
        {
            row.ConstantItem(80).AlignRight()
                .DefaultTextStyle(x => x.FontSize(8.5f).FontColor(TextMuted))
                .Text($"{label}:");
            row.RelativeItem().PaddingLeft(8).AlignRight()
                .DefaultTextStyle(x => x.SemiBold().FontSize(8.5f))
                .Text(value);
        });
    }

    private static void SummaryLine(IContainer container, string label, string value, bool bold = false, bool large = false)
    {
        container.Row(row =>
        {
            row.RelativeItem().DefaultTextStyle(x =>
            {
                var style = x.FontSize(large ? 11f : 9f).FontColor(TextMuted);
                return bold ? style.SemiBold().FontColor(TextDark) : style;
            }).Text(label);

            row.ConstantItem(110).AlignRight().DefaultTextStyle(x =>
            {
                var style = x.FontSize(large ? 12f : 9f);
                return bold ? style.Bold().FontColor(large ? Primary : TextDark) : style;
            }).Text(value);
        });
    }

    private static void StatusBadge(IContainer container, string status)
    {
        var (bg, fg, label) = status switch
        {
            "Paid" => ("#DCFCE7", "#166534", "PAID"),
            "Sent" => ("#DBEAFE", "#1E40AF", "SENT"),
            "Overdue" => ("#FEE2E2", "#991B1B", "OVERDUE"),
            "PartiallyPaid" => ("#FFEDD5", "#9A3412", "PARTIAL"),
            "Partially Paid" => ("#FFEDD5", "#9A3412", "PARTIAL"),
            "Cancelled" => ("#F1F5F9", "#475569", "CANCELLED"),
            _ => ("#F1F5F9", "#475569", "DRAFT")
        };

        container.MinWidth(72).Background(bg).PaddingVertical(4).PaddingHorizontal(10).AlignCenter()
            .DefaultTextStyle(x => x.Bold().FontSize(8).FontColor(fg))
            .Text(label);
    }

    private static string GetCompanyInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant()
            : name.Length >= 2 ? name[..2].ToUpperInvariant() : name.ToUpperInvariant();
    }

    private static string FormatCityLine(string? city, string? state, string? postal)
    {
        var parts = new[] { city, state, postal }.Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", parts);
    }

    private static string FormatTaxType(string taxType) =>
        taxType == "IGST" ? "IGST (Inter-State)" : "CGST + SGST (Intra-State)";

    private static string FormatMoney(string symbol, decimal amount) =>
        symbol == "₹" ? $"₹ {amount:N2}" : $"{symbol}{amount:N2}";

    private static string GetTaxLabel(InvoiceItemDto item, string taxType)
    {
        if (taxType == "IGST" && item.IGSTPercentage > 0)
            return $"IGST @ {item.IGSTPercentage:N1}%";
        if (item.CGSTPercentage > 0 || item.SGSTPercentage > 0)
            return $"CGST @ {item.CGSTPercentage:N1}% + SGST @ {item.SGSTPercentage:N1}%";
        return string.Empty;
    }

    private static string GetItemTaxPercentDisplay(InvoiceItemDto item, string taxType) =>
        taxType == "IGST"
            ? $"{item.IGSTPercentage:N1}%"
            : $"{item.CGSTPercentage:N1}% + {item.SGSTPercentage:N1}%";

    private static (decimal cgstRate, decimal sgstRate, decimal igstRate, decimal totalTaxRate) GetInvoiceTaxRates(InvoiceDto invoice)
    {
        if (invoice.Items.Count == 0)
            return (0, 0, 0, 0);

        if (invoice.TaxType == "IGST")
        {
            var igst = invoice.Items.Max(i => i.IGSTPercentage);
            return (0, 0, igst, igst);
        }

        var cgst = invoice.Items.Max(i => i.CGSTPercentage);
        var sgst = invoice.Items.Max(i => i.SGSTPercentage);
        return (cgst, sgst, 0, cgst + sgst);
    }
}
