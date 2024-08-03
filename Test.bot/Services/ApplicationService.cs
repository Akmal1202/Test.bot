using Newtonsoft.Json;
using Test.bot.Entities;

namespace Test.bot.Services;
public class ApplicationService
{
    public List<Application> Applications { get; set; }
    private const string Path = "applications.json";

    public ApplicationService()
    {
        Applications = new();
        ReadFromFile();
    }
    public void AddApplication(User user, string message)
    {
        var application = new Application()
        {
            Message = message,
            UserName = user.UserName,
            Role = user.Role.ToString(),
            PhoneNumber = user.PhoneNumber!,
            FirstName = user.FirstName
        };
        Applications.Add(application);
        WriteToFile();

    }
    void WriteToFile()
    {
        var jsonData = JsonConvert.SerializeObject(Applications);
        File.WriteAllText(Path, jsonData);
    }
    void ReadFromFile()
    {
        if (File.Exists(Path))
        {
            var jsonData = File.ReadAllText(Path);
            Applications = JsonConvert.DeserializeObject<List<Application>>(jsonData)!;
        }
    }
}
