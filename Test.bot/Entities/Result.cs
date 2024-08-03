namespace Test.bot.Entities;
public class Result
{
    public int Id { get; set; }
    public byte TotalAnswerCount { get; set; }
    public byte CorrectAnswerCount { get; set; }
    public byte InCorrectAnswerCount => (byte)(TotalAnswerCount - CorrectAnswerCount);
}