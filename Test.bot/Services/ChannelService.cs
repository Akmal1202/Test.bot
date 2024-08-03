using Newtonsoft.Json;
using Test.bot.Entities;

namespace Test.bot.Services;

public class ChannelService
{
    private const string Path = "channels.json";
    public List<Channel> Channels { get; set; }
    public ChannelService()
    {
        Channels = new();
        ReadFromFile();
    }
    public Channel AddChannel(long channelId, string channelName, string channelLink)
    {
        var channel = Channels.FirstOrDefault(c => c.ChannelId == channelId);
        if (channel == null)
        {
            channel = new()
            {
                ChannelId = channelId,
                ChannelName = channelName,
                Link = channelLink
            };
            Channels.Add(channel);
            WriteToFile();
        }
        return channel;
    }
    public void RemoveChannel(long channelId)
    {
        var channel = Channels.FirstOrDefault(c => c.ChannelId == channelId);
        if (channel != null)
        {
            Channels.Remove(channel);
            WriteToFile();
        }
    }
    void WriteToFile()
    {
        var jsonData = JsonConvert.SerializeObject(Channels);
        File.WriteAllText(Path, jsonData);
    }
    void ReadFromFile()
    {
        if (File.Exists(Path))
        {
            var jsonData = File.ReadAllText(Path);
            Channels = JsonConvert.DeserializeObject<List<Channel>>(jsonData)!;
        }
    }
}

