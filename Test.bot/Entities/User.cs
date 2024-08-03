
namespace Test.bot.Entities;
public class User
{
    public User()
    {
        IsBlocked = false;
    }
    public long ChatId { get; set; } 
    public string Language { get; set; }
    public string FirstName { get; set; }
    public string? UserName { get; set; }
    public Step UserStep { get; set; }
    public string? PhoneNumber { get; set; }
    public string Grade { get; set; }
    public UserRole Role { get; set; }
    public bool IsBlocked { get; set; }
    public int TestId { get; set; } = 0;

}
