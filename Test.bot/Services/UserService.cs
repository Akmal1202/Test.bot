using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;
using Test.bot.Entities;

namespace Test.bot.Services;
public class UserService
{
    private static List<List<KeyboardButton>> Buttons = new List<List<KeyboardButton>>();
    public List<User> Users { get; set; }
    private const string Path = "users.json";

    public UserService()
    {
        Users = new();
        ReadFromFile();
    }
    public User AddUser(long chatId, string? username)
    {
        var user = Users.FirstOrDefault(u => u.ChatId == chatId);

        if (user is null)
        {
            user = new()
            {
                ChatId = chatId,
                UserName = username,
                Role = UserRole.User
            };
            if (user.ChatId == EnConstants.SuperAdmin)
                user.Role = UserRole.SuperAdmin;

            Users.Add(user);
            WriteToFile();
        }
        return user;
    }
    public void UpdateUser()
    {
        WriteToFile();
    }
    void WriteToFile()
    {
        var jsonData = JsonConvert.SerializeObject(Users);
        File.WriteAllText(Path, jsonData);
    }
    void ReadFromFile()
    {
        if (File.Exists(Path))
        {
            var jsonData = File.ReadAllText(Path);
            Users = JsonConvert.DeserializeObject<List<User>>(jsonData)!;
        }
    }
    public User UpdateUserInfo(long chatId, string? username, string language, string grade)
    {
        var user = Users.FirstOrDefault(u => u.ChatId == chatId);
        if (user is not null)
        { 
            user.UserName = username;
            user.Language = language;
            user.Grade = grade;
            WriteToFile();
        }
        return user;
    }
}