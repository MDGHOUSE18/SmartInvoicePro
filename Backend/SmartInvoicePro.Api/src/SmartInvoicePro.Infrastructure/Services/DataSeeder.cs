using Microsoft.EntityFrameworkCore;
using SmartInvoicePro.Application.DTOs.Invoice;
using SmartInvoicePro.Application.Helpers;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Domain.Entities;
using SmartInvoicePro.Domain.Enums;
using SmartInvoicePro.Infrastructure.Constants;
using SmartInvoicePro.Infrastructure.Data;
using SmartInvoicePro.Infrastructure.Security;

namespace SmartInvoicePro.Infrastructure.Services;

public class DataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasherService _passwordHasher;

    public DataSeeder(ApplicationDbContext context, PasswordHasherService passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();

        await EnsureRolesAsync();
        await EnsureDemoUsersAsync();
        await EnsureCompanySettingsAsync();
        await SeedTransactionalDataAsync();
    }

    public async Task ResetDemoDataAsync()
    {
        await ClearTransactionalDataAsync();
        await EnsureDemoUsersAsync();
        await SeedTransactionalDataAsync();
    }

    public Task EnsureDemoCredentialsAsync() => EnsureDemoUsersAsync();

    private async Task EnsureRolesAsync()
    {
        if (!await _context.Roles.AnyAsync(r => r.RoleName == RoleNames.Admin))
            _context.Roles.Add(new Role { RoleName = RoleNames.Admin, Description = "Administrator" });

        if (!await _context.Roles.AnyAsync(r => r.RoleName == RoleNames.Staff))
            _context.Roles.Add(new Role { RoleName = RoleNames.Staff, Description = "Staff user" });

        await _context.SaveChangesAsync();
    }

    private async Task EnsureDemoUsersAsync()
    {
        var adminRole = await _context.Roles.FirstAsync(r => r.RoleName == RoleNames.Admin);
        var staffRole = await _context.Roles.FirstAsync(r => r.RoleName == RoleNames.Staff);

        await EnsureDemoUserAsync(
            "admin@acmeconsulting.in", "Rajesh", "Kumar", "Admin@123", adminRole.RoleId,
            new Guid("11111111-1111-1111-1111-111111111111"));

        await EnsureDemoUserAsync(
            "staff@acmeconsulting.in", "Priya", "Sharma", "Staff@123", staffRole.RoleId,
            new Guid("22222222-2222-2222-2222-222222222222"));
    }

    private async Task EnsureDemoUserAsync(
        string email, string firstName, string lastName, string password, int roleId, Guid? preferredUserId = null)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            user = new User
            {
                UserId = preferredUserId ?? Guid.NewGuid(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Phone = "+91 98765 43210",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            _context.Users.Add(user);
            _context.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = roleId });
        }
        else
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            user.IsActive = true;
            user.FirstName = firstName;
            user.LastName = lastName;

            if (!user.UserRoles.Any(ur => ur.RoleId == roleId))
                _context.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = roleId });
        }

        await _context.SaveChangesAsync();
    }

    private async Task EnsureCompanySettingsAsync()
    {
        if (await _context.CompanySettings.AnyAsync()) return;

        _context.CompanySettings.Add(new CompanySettings
        {
            CompanyName = "Acme Consulting Pvt Ltd",
            Address = "42 MG Road, Brigade Towers",
            City = "Bengaluru",
            State = "Karnataka",
            Country = "India",
            PostalCode = "560001",
            Phone = "+91 80 4567 8900",
            Email = "billing@acmeconsulting.in",
            Website = "https://acmeconsulting.in",
            TaxNumber = "29AABCA1234A1Z5",
            DefaultCurrency = "INR",
            TermsAndConditions = "Payment due within 30 days. Late payments attract 1.5% monthly interest.",
            BankName = "HDFC Bank",
            BankAccountNumber = "50200012345678",
            BankIFSC = "HDFC0001234"
        });

        await _context.SaveChangesAsync();
    }

    private async Task SeedTransactionalDataAsync()
    {
        if (await _context.Customers.AnyAsync()) return;

        var products = CreateProducts();
        var customers = CreateCustomers();
        _context.Products.AddRange(products);
        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();

        var companyState = "Karnataka";
        var random = new Random(42);
        var invoices = new List<Invoice>();
        var payments = new List<Payment>();
        var notifications = new List<Notification>();
        var statuses = new[]
        {
            InvoiceStatuses.Draft, InvoiceStatuses.Sent, InvoiceStatuses.Paid,
            InvoiceStatuses.PartiallyPaid, InvoiceStatuses.Overdue, InvoiceStatuses.Cancelled
        };

        for (var i = 1; i <= 55; i++)
        {
            var customer = customers[random.Next(customers.Count)];
            var taxType = InvoiceCalculationHelper.DetermineTaxType(companyState, customer.State);
            var itemCount = random.Next(1, 4);
            var items = new List<CreateInvoiceItemDto>();

            for (var j = 0; j < itemCount; j++)
            {
                var product = products[random.Next(products.Count)];
                var qty = random.Next(1, 5);
                items.Add(new CreateInvoiceItemDto
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Description = product.Description,
                    Quantity = qty,
                    UnitPrice = product.UnitPrice,
                    Discount = random.Next(0, 3) == 0 ? random.Next(100, 500) : 0
                });
            }

            var avgTax = products.Average(p => p.TaxPercentage);
            var discount = random.Next(0, 5) == 0 ? random.Next(200, 1000) : 0m;
            var totals = InvoiceCalculationHelper.Calculate(items, avgTax, taxType, discount);
            var invoiceDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-random.Next(1, 180)));
            var status = statuses[random.Next(statuses.Length)];

            var invoice = new Invoice
            {
                InvoiceId = Guid.NewGuid(),
                InvoiceNumber = $"INV-{invoiceDate.Year}-{i:D4}",
                InvoiceDate = invoiceDate,
                DueDate = invoiceDate.AddDays(30),
                CustomerId = customer.CustomerId,
                Status = status,
                TaxType = totals.TaxType,
                Subtotal = totals.Subtotal,
                CGSTAmount = totals.CGSTAmount,
                SGSTAmount = totals.SGSTAmount,
                IGSTAmount = totals.IGSTAmount,
                TaxAmount = totals.TaxAmount,
                DiscountAmount = discount,
                GrandTotal = totals.GrandTotal,
                AmountPaid = 0,
                BalanceAmount = totals.GrandTotal,
                CreatedDate = invoiceDate.ToDateTime(TimeOnly.MinValue),
                Items = BuildInvoiceItems(items, taxType, avgTax)
            };

            if (status is InvoiceStatuses.Paid or InvoiceStatuses.PartiallyPaid)
            {
                var paidAmount = status == InvoiceStatuses.Paid
                    ? invoice.GrandTotal
                    : Math.Round(invoice.GrandTotal * ((decimal)random.NextDouble() * 0.8m + 0.1m), 2);

                invoice.AmountPaid = paidAmount;
                invoice.BalanceAmount = invoice.GrandTotal - paidAmount;
            }

            if (status == InvoiceStatuses.Overdue)
            {
                invoice.DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-random.Next(5, 30)));
                invoice.BalanceAmount = invoice.GrandTotal;
            }

            if (status == InvoiceStatuses.Cancelled)
            {
                invoice.BalanceAmount = 0;
            }

            invoices.Add(invoice);

            if (i % 10 == 0)
            {
                notifications.Add(new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    Title = $"Invoice {invoice.InvoiceNumber} created",
                    Message = $"Invoice for {customer.CustomerName} worth INR {invoice.GrandTotal:N2}",
                    Type = "Invoice",
                    ReferenceId = invoice.InvoiceId.ToString(),
                    CreatedDate = DateTime.UtcNow
                });
            }
        }

        _context.Invoices.AddRange(invoices);
        await _context.SaveChangesAsync();

        foreach (var invoice in invoices.Where(i => i.AmountPaid > 0))
        {
            payments.Add(new Payment
            {
                PaymentId = Guid.NewGuid(),
                InvoiceId = invoice.InvoiceId,
                PaymentDate = invoice.InvoiceDate.AddDays(5),
                AmountPaid = invoice.AmountPaid,
                PaymentMethod = random.Next(2) == 0 ? PaymentMethods.Upi : PaymentMethods.BankTransfer,
                BalanceAmount = invoice.BalanceAmount,
                ReferenceNumber = $"TXN{random.Next(100000, 999999)}",
                CreatedDate = DateTime.UtcNow
            });
        }

        _context.Payments.AddRange(payments);
        _context.Notifications.AddRange(notifications);
        _context.Notifications.Add(new Notification
        {
            NotificationId = Guid.NewGuid(),
            Title = "Welcome to SmartInvoice Pro",
            Message = "Demo data has been loaded successfully.",
            Type = "System",
            CreatedDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    private async Task ClearTransactionalDataAsync()
    {
        _context.Payments.RemoveRange(_context.Payments);
        _context.InvoiceItems.RemoveRange(_context.InvoiceItems);
        _context.Invoices.RemoveRange(_context.Invoices);
        _context.Notifications.RemoveRange(_context.Notifications);
        _context.Customers.RemoveRange(_context.Customers);
        _context.Products.RemoveRange(_context.Products);
        _context.EmailLogs.RemoveRange(_context.EmailLogs);
        _context.AuditLogs.RemoveRange(_context.AuditLogs);
        await _context.SaveChangesAsync();
    }

    private static List<Product> CreateProducts() =>
    [
        new() { ProductId = Guid.NewGuid(), ProductName = "IT Consulting - Hourly", Description = "Senior consultant hourly rate", UnitPrice = 5000, TaxPercentage = 18, CGSTPercentage = 9, SGSTPercentage = 9, IGSTPercentage = 18, CreatedDate = DateTime.UtcNow },
        new() { ProductId = Guid.NewGuid(), ProductName = "Cloud Migration Package", Description = "End-to-end cloud migration", UnitPrice = 150000, TaxPercentage = 18, CGSTPercentage = 9, SGSTPercentage = 9, IGSTPercentage = 18, CreatedDate = DateTime.UtcNow },
        new() { ProductId = Guid.NewGuid(), ProductName = "Software License - Annual", Description = "Enterprise software license", UnitPrice = 75000, TaxPercentage = 18, CGSTPercentage = 9, SGSTPercentage = 9, IGSTPercentage = 18, CreatedDate = DateTime.UtcNow },
        new() { ProductId = Guid.NewGuid(), ProductName = "Training Workshop", Description = "Full-day corporate training", UnitPrice = 35000, TaxPercentage = 18, CGSTPercentage = 9, SGSTPercentage = 9, IGSTPercentage = 18, CreatedDate = DateTime.UtcNow },
        new() { ProductId = Guid.NewGuid(), ProductName = "API Integration Service", Description = "Third-party API integration", UnitPrice = 45000, TaxPercentage = 18, CGSTPercentage = 9, SGSTPercentage = 9, IGSTPercentage = 18, CreatedDate = DateTime.UtcNow },
        new() { ProductId = Guid.NewGuid(), ProductName = "Managed Support - Monthly", Description = "24x7 managed support retainer", UnitPrice = 25000, TaxPercentage = 18, CGSTPercentage = 9, SGSTPercentage = 9, IGSTPercentage = 18, CreatedDate = DateTime.UtcNow },
        new() { ProductId = Guid.NewGuid(), ProductName = "Security Audit", Description = "Comprehensive security assessment", UnitPrice = 80000, TaxPercentage = 18, CGSTPercentage = 9, SGSTPercentage = 9, IGSTPercentage = 18, CreatedDate = DateTime.UtcNow },
        new() { ProductId = Guid.NewGuid(), ProductName = "Data Analytics Dashboard", Description = "Custom BI dashboard development", UnitPrice = 120000, TaxPercentage = 18, CGSTPercentage = 9, SGSTPercentage = 9, IGSTPercentage = 18, CreatedDate = DateTime.UtcNow }
    ];

    private static List<Customer> CreateCustomers()
    {
        var names = new[]
        {
            ("Rajesh Kumar", "Kumar Enterprises", "rajesh@kumar.in", "Karnataka"),
            ("Priya Sharma", "Sharma Tech Solutions", "priya@sharmatech.in", "Maharashtra"),
            ("Amit Patel", "Patel Industries", "amit@patelind.com", "Gujarat"),
            ("Sneha Reddy", "Reddy Corp", "sneha@reddycorp.in", "Telangana"),
            ("Vikram Singh", "Singh Logistics", "vikram@singhlog.in", "Delhi"),
            ("Anita Desai", "Desai Retail", "anita@desairetail.in", "Karnataka"),
            ("Rahul Mehta", "Mehta Finance", "rahul@mehtafin.in", "Maharashtra"),
            ("Kavita Nair", "Nair Exports", "kavita@nairexport.in", "Kerala"),
            ("Suresh Iyer", "Iyer Systems", "suresh@iyersys.in", "Tamil Nadu"),
            ("Deepa Gupta", "Gupta Healthcare", "deepa@guptahealth.in", "Uttar Pradesh"),
            ("Arun Joshi", "Joshi Media", "arun@joshimedia.in", "Rajasthan"),
            ("Meera Krishnan", "Krishnan Foods", "meera@krishnanfoods.in", "Karnataka"),
            ("Sanjay Verma", "Verma Constructions", "sanjay@vermabuild.in", "Madhya Pradesh"),
            ("Lakshmi Rao", "Rao Textiles", "lakshmi@raotex.in", "Andhra Pradesh"),
            ("Harish Choudhary", "Choudhary Agro", "harish@choudharyagro.in", "Punjab"),
            ("Nisha Banerjee", "Banerjee EduTech", "nisha@banerjeeedu.in", "West Bengal"),
            ("Gopal Menon", "Menon Travels", "gopal@menontravels.in", "Kerala"),
            ("Pooja Saxena", "Saxena Legal", "pooja@saxenalegal.in", "Haryana"),
            ("Ravi Shankar", "Shankar Motors", "ravi@shankarmotors.in", "Karnataka"),
            ("Divya Pillai", "Pillai Pharma", "divya@pillaipharma.in", "Karnataka")
        };

        return names.Select((n, i) => new Customer
        {
            CustomerId = Guid.NewGuid(),
            CustomerName = n.Item1,
            CompanyName = n.Item2,
            Email = n.Item3,
            Phone = $"+91 98{i:D2} {randomSuffix(i)}",
            Address = $"{100 + i} Business Park",
            City = n.Item4 == "Karnataka" ? "Bengaluru" : "Metro City",
            State = n.Item4,
            Country = "India",
            TaxNumber = $"29AABCT{i + 1000:D4}F1Z5",
            CreatedDate = DateTime.UtcNow
        }).ToList();
    }

    private static string randomSuffix(int i) => $"{1000 + i * 111} {2000 + i}";

    private static List<InvoiceItem> BuildInvoiceItems(IReadOnlyList<CreateInvoiceItemDto> items, string taxType, decimal taxPercentage)
    {
        var (cgst, sgst, igst) = InvoiceCalculationHelper.GetItemTaxPercentages(taxPercentage, taxType);
        return items.Select(item => new InvoiceItem
        {
            InvoiceItemId = Guid.NewGuid(),
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Description = item.Description,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            CGSTPercentage = cgst,
            SGSTPercentage = sgst,
            IGSTPercentage = igst,
            Discount = item.Discount,
            LineTotal = InvoiceCalculationHelper.CalculateLineTotal(item.Quantity, item.UnitPrice, item.Discount)
        }).ToList();
    }
}
