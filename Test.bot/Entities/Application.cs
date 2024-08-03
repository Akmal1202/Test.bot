
namespace Test.bot.Entities;
public class Application
{
    public string Message { get; set; }
    public string FirstName { get; set; }
    public string? UserName { get; set; }
    public string PhoneNumber { get; set; }
    public string Role { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
