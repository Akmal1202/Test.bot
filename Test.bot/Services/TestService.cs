using Newtonsoft.Json;
using Test.bot.Entities;

namespace Test.bot.Services;
public class TestService
{
    public List<Tests> Tests { get; set; }
    private const string Path = "tests.json";

    public TestService()
    {
        Tests = new();
        ReadFromFile();
    }
    public Tests AddTest(int testsId, List<char>? answers)
    {
        var test = Tests.FirstOrDefault(t =>t.Id  == testsId);
        if (test is null)
        {
            test = new()
            {
                Id = testsId,
                Answers = answers
            };
            Tests.Add(test);
            WriteToFile();
        }
        return test;
    }
    public Tests GetTests(int testsId)
    {
        var test = Tests.FirstOrDefault(t => t.Id == testsId);
        if (test is not null)
        {
            return test;
        }
        return null;
    }
    public int GetId() 
    {
        Tests.Count();
        if(Tests.Count() == 0) 
        {
            return 1;
        }
        return Tests.Count()+1;
    }
    void WriteToFile()
    {
        var jsonData = JsonConvert.SerializeObject(Tests);
        File.WriteAllText(Path, jsonData);
    }
    void ReadFromFile()
    {
        if (File.Exists(Path))
        {
            var jsonData = File.ReadAllText(Path);
            Tests = JsonConvert.DeserializeObject<List<Tests>>(jsonData)!;
        }
    }
}
