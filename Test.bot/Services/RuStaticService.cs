using Test.bot.Entities;
using OfficeOpenXml;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = Test.bot.Entities.User;

namespace Test.bot.Services;
public static class RuStaticService
{
    private static List<List<KeyboardButton>> Buttons = new List<List<KeyboardButton>>();
    public static Tuple<long, string?, string, int, bool, bool> GetData(Update update)
    {
        long chatId;
        string? username;
        string message;
        bool isPollAnswer;
        bool check;
        int messageId;
        if (update.Type == UpdateType.Message)
        {
            chatId = update.Message.From.Id;
            username = update.Message.From.Username;
            message = update.Message.Text;
            messageId = update.Message.MessageId;
            check = false;
            isPollAnswer = false;
        }
        else if (update.Type == UpdateType.CallbackQuery)
        {
            chatId = update.CallbackQuery!.From.Id;
            username = update.CallbackQuery.From.Username;
            message = update.CallbackQuery.Data!;
            messageId = update.CallbackQuery.Message.MessageId;
            check = false;
            isPollAnswer = false;
        }
        else if (update.Type == UpdateType.PollAnswer)
        {
            var answer = update.PollAnswer;
            chatId = answer.User.Id;
            username = answer.User.Username;
            var selectedId = answer.OptionIds[0];

            message = selectedId.ToString();
            messageId = 0;
            isPollAnswer = true;
            check = false;
        }
        else
        {
            chatId = default;
            username = default;
            message = default;
            check = true;
            isPollAnswer = false;
            messageId = 0;
        }

        return new(chatId, username, message, messageId, isPollAnswer, check);
    }
    public static ReplyKeyboardMarkup Back()
    {
        var buttons = new List<List<KeyboardButton>>();
        var rows = new List<KeyboardButton>()
        {
            new KeyboardButton(RuConstants.BackText)
        };

        buttons.Add(rows);

        return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
    }
    public static ReplyKeyboardMarkup GetUserMenu()
    {
        Buttons = new();
        var rows1 = new List<KeyboardButton>()
        {
            new (RuConstants.TakeTestText),
        };

        var rows2 = new List<KeyboardButton>()
        {
            new (RuConstants.MessageToAdminText),
            new (RuConstants.AboutText)
        };

        Buttons.Add(rows1);
        Buttons.Add(rows2);

        return new ReplyKeyboardMarkup(Buttons) { ResizeKeyboard = true };
    }
    public static ReplyKeyboardMarkup GetAdminMenu()
    {
        Buttons = new();
        var rows1 = new List<KeyboardButton>()
        {
            new (RuConstants.TakeTestText),
            new (RuConstants.AddTest)
        };

        var rows2 = new List<KeyboardButton>()
        {
            new (RuConstants.MessageOfUsers)
        };



        Buttons.Add(rows1);
        Buttons.Add(rows2);


        return new ReplyKeyboardMarkup(Buttons) { ResizeKeyboard = true };
    }
    public static ReplyKeyboardMarkup GetSuperAdminMenu()
    {
        Buttons = new();

        var rows1 = new List<KeyboardButton>()
        {
            new (RuConstants.TakeTestText),
            new (RuConstants.AddTest)
        };
        var rows2 = new List<KeyboardButton>()
        {
            new (RuConstants.MessageOfUsers),
            new (RuConstants.GetAllUsers)
        }; 
        var rows3 = new List<KeyboardButton>()
        {
            new (EnConstants.ShowResultText)
        };
        var rows4 = new List<KeyboardButton>()
        {
            new (RuConstants.AddAdmin),
            new (RuConstants.RemoveAdmin)
        };
        var rows5 = new List<KeyboardButton>()
        {
            new (RuConstants.AddChannelLink),
            new (RuConstants.RemoveChannelLinks)
        };

        Buttons.Add(rows1);
        Buttons.Add(rows2);
   //     Buttons.Add(rows3);
        Buttons.Add(rows4);
        Buttons.Add(rows5);

        return new ReplyKeyboardMarkup(Buttons) { ResizeKeyboard = true };
    }
    public static void GetUsers(List<User> users)
    {
        using var package = new ExcelPackage();
        var userInfo = package.Workbook.Worksheets.Add("лист1");
        userInfo.Cells[1, 2].Value = "номера";
        userInfo.Cells[1, 2].Value = "Имя";
        userInfo.Cells[1, 3].Value = "Имя пользователя";
        userInfo.Cells[1, 4].Value = "Номер телефона";
        userInfo.Cells[1, 5].Value = "Роль";
        var row = 2;
        foreach (var user in users)
        {
            userInfo.Cells[row, 1].Value = row - 1;
            userInfo.Cells[row, 2].Value = user.FirstName;
            userInfo.Cells[row, 3].Value = user.UserName;
            userInfo.Cells[row, 4].Value = user.PhoneNumber;
            userInfo.Cells[row, 5].Value = user.Role;
            row++;
        }
        package.SaveAs(new FileInfo(EnConstants.UserInfoPath));
    }
    public static ReplyKeyboardMarkup GetGrade()
    {
        Buttons = new();

        var rows = new List<KeyboardButton>();
        for (int i = 1; i < 16; i++)
        {
            KeyboardButton row;
            if (i < 12)
            {
                row = new($"{i}-класс");
            }
            else if (i >= 12 && i < 15)
            {
                row = new($"{i - 11}-курс");
            }
            else
            {
                row = new("закончил");
            }
            rows.Add(row);

            if (i % 2 == 0 || i == 15)
            {
                Buttons.Add(rows);
                rows = new();
            }

        }

        return new ReplyKeyboardMarkup(Buttons) { ResizeKeyboard = true };
    }
    public static void GetApplications(List<Application> applications)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("лист1");
        worksheet.Cells[1, 2].Value = "номера";
        worksheet.Cells[1, 2].Value = "Имя";
        worksheet.Cells[1, 3].Value = "Имя пользователя";
        worksheet.Cells[1, 4].Value = "Номер телефона";
        worksheet.Cells[1, 5].Value = "Роль";
        worksheet.Cells[1, 6].Value = "ОтправитьДата";
        worksheet.Cells[1, 7].Value = "Сообщение";
        var row = 2;
        foreach (var application in applications)
        {
            worksheet.Cells[row, 1].Value = row - 1;
            worksheet.Cells[row, 2].Value = application.FirstName;
            worksheet.Cells[row, 3].Value = application.UserName;
            worksheet.Cells[row, 4].Value = application.PhoneNumber;
            worksheet.Cells[row, 5].Value = application.Role;
            worksheet.Cells[row, 6].Value = application.CreatedDate.ToString();
            worksheet.Cells[row, 7].Value = application.Message;
            row++;
        }
        package.SaveAs(new FileInfo(EnConstants.ApplicationPath));
    }
    public static List<Application> SortApplicationsByDate(List<Application> applications, string message)
    {
        var data = message.Split(',').ToArray();
        var fromDateData = data[0].Split('.').ToArray();
        var toDateData = data[1].Split('.').ToArray();
        var fromDate = new DateTime(year: int.Parse(fromDateData[2]), month: int.Parse(fromDateData[1]), day: int.Parse(fromDateData[0]));
        var toDate = new DateTime(year: int.Parse(toDateData[2]), month: int.Parse(toDateData[1]), day: int.Parse(toDateData[0]));
        toDate = toDate.AddHours(23).AddMinutes(59).AddSeconds(59);
        var sortedApplications = applications.Where(a => a.CreatedDate >= fromDate && a.CreatedDate <= toDate).ToList();
        return sortedApplications;
    }

}