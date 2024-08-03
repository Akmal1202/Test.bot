using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.bot.Entities;

namespace Test.bot.Services;
public class ResultService
{
    public List<Result> Results { get; set; }
    private const string Path = "users.json";

    public ResultService()
    {
        Results = new();
        ReadFromFile();
    }
    public void AddResult(int id) 
    {

    }
    public void UpdateResult()
    {
        WriteToFile();
    }
    void WriteToFile()
    {
        var jsonData = JsonConvert.SerializeObject(Results);
        File.WriteAllText(Path, jsonData);
    }
    void ReadFromFile()
    {
        if (File.Exists(Path))
        {
            var jsonData = File.ReadAllText(Path);
            Results = JsonConvert.DeserializeObject<List<Result>>(jsonData)!;
        }
    }
}
