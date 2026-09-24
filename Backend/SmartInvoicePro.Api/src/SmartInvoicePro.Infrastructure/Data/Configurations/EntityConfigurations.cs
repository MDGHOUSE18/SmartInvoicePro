using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInvoicePro.Domain.Entities;

namespace SmartInvoicePro.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.UserId);
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.PasswordHash).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.RefreshToken).HasMaxLength(500);
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(x => x.RoleId);
        builder.Property(x => x.RoleName).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.RoleName).IsUnique();
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(x => new { x.UserId, x.RoleId });
        builder.HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId);
        builder.HasOne(x => x.Role).WithMany(x => x.UserRoles).HasForeignKey(x => x.RoleId);
    }
}

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(x => x.CustomerId);
        builder.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.State).HasMaxLength(100);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.Country).HasMaxLength(100);
        builder.Property(x => x.TaxNumber).HasMaxLength(50);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(x => x.ProductId);
        builder.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.TaxPercentage).HasPrecision(5, 2);
        builder.Property(x => x.CGSTPercentage).HasPrecision(5, 2);
        builder.Property(x => x.SGSTPercentage).HasPrecision(5, 2);
        builder.Property(x => x.IGSTPercentage).HasPrecision(5, 2);
        builder.Property(x => x.Status).HasMaxLength(20);
    }
}

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(x => x.InvoiceId);
        builder.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.InvoiceNumber).IsUnique();
        builder.Property(x => x.Status).HasMaxLength(30);
        builder.Property(x => x.TaxType).HasMaxLength(20);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Property(x => x.CGSTAmount).HasPrecision(18, 2);
        builder.Property(x => x.SGSTAmount).HasPrecision(18, 2);
        builder.Property(x => x.IGSTAmount).HasPrecision(18, 2);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.GrandTotal).HasPrecision(18, 2);
        builder.Property(x => x.AmountPaid).HasPrecision(18, 2);
        builder.Property(x => x.BalanceAmount).HasPrecision(18, 2);
        builder.HasOne(x => x.Customer).WithMany(x => x.Invoices).HasForeignKey(x => x.CustomerId);
        builder.HasMany(x => x.Items).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Payments).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems");
        builder.HasKey(x => x.InvoiceItemId);
        builder.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 2);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.CGSTPercentage).HasPrecision(5, 2);
        builder.Property(x => x.SGSTPercentage).HasPrecision(5, 2);
        builder.Property(x => x.IGSTPercentage).HasPrecision(5, 2);
        builder.Property(x => x.Discount).HasPrecision(18, 2);
        builder.Property(x => x.LineTotal).HasPrecision(18, 2);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(x => x.PaymentId);
        builder.Property(x => x.AmountPaid).HasPrecision(18, 2);
        builder.Property(x => x.BalanceAmount).HasPrecision(18, 2);
        builder.Property(x => x.PaymentMethod).HasMaxLength(30);
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
    }
}

public class CompanySettingsConfiguration : IEntityTypeConfiguration<CompanySettings>
{
    public void Configure(EntityTypeBuilder<CompanySettings> builder)
    {
        builder.ToTable("CompanySettings");
        builder.HasKey(x => x.SettingId);
        builder.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DefaultCurrency).HasMaxLength(3);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.NotificationId);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Type).HasMaxLength(50);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.AuditLogId);
        builder.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
    }
}

public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.ToTable("EmailLogs");
        builder.HasKey(x => x.EmailLogId);
        builder.Property(x => x.ToEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50);
    }
}

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");
        builder.HasKey(x => x.TokenId);
        builder.Property(x => x.Token).HasMaxLength(500).IsRequired();
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
    }
}
