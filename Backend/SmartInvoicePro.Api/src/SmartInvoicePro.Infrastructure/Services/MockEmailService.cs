using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Domain.Entities;
using SmartInvoicePro.Infrastructure.Data;

namespace SmartInvoicePro.Infrastructure.Services;

public class MockEmailService : IEmailService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MockEmailService(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, string? attachmentName = null, byte[]? attachment = null)
    {
        var log = new EmailLog
        {
            EmailLogId = Guid.NewGuid(),
            ToEmail = toEmail,
            Subject = subject,
            Body = body + (attachment != null ? $"\n[Attachment: {attachmentName}, {attachment.Length} bytes]" : string.Empty),
            AttachmentName = attachmentName,
            Status = "Logged",
            SentBy = _currentUser.UserId,
            CreatedDate = DateTime.UtcNow
        };

        _context.EmailLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
