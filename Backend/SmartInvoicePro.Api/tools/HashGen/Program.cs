using Microsoft.AspNetCore.Identity;

var admin = new DemoUser { UserId = Guid.Parse("11111111-1111-1111-1111-111111111111") };
var staff = new DemoUser { UserId = Guid.Parse("22222222-2222-2222-2222-222222222222") };
var hasher = new PasswordHasher<DemoUser>();

Console.WriteLine("Admin@123:");
Console.WriteLine(hasher.HashPassword(admin, "Admin@123"));
Console.WriteLine();
Console.WriteLine("Staff@123:");
Console.WriteLine(hasher.HashPassword(staff, "Staff@123"));

class DemoUser
{
    public Guid UserId { get; set; }
}
