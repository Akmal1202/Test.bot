using Test.bot.Entities;
using Test.bot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using JFA.Telegram.Console;
using File = System.IO.File;
using User = Test.bot.Entities.User;

UserService userService = new();
ChannelService channelService = new();
ApplicationService applicationService = new();
TestService testService = new ();

var botManager = new TelegramBotManager();
var bot = botManager.Create("7206368648:AAFGLnJA_DQxMPma0UUvF1shIhyM7SF-Gnk");
botManager.Start(BotFunction);
void BotFunction(Update update)
{
    var (chatId, username, message, messageId, isPollAnswer, check) = EnStaticService.GetData(update: update);

    if (check)
        return;
    var user = userService.AddUser(chatId, username);

    CheckingForChannel(user, update);
}
void UserActions(Update update)
{
    var (chatId, username, message, messageId, isPollAnswer, check) = EnStaticService.GetData(update: update);
    var user = userService.AddUser(chatId, username);
    if (message == "Check")
    {
        bot.DeleteMessageAsync(user.ChatId, messageId);
    }
    else if (message == EnConstants.BackText)
    {
        ShowMenu(user);
        return;
    }
    else
    {
        Console.WriteLine(message);

        switch (user.UserStep)
        {
            case Step.AskLanguage: AskLanguage(user); break;
            case Step.SaveLanguage: SaveLanguage(user, message); break;
            case Step.AskName: AskName(user); break;
            case Step.SaveName: SaveName(user, message); break;
            case Step.SavePhoneNumber: SavePhoneNumber(user, update); break;
            case Step.SaveGrade: SaveGrade(user, message);break;
            case Step.ChooseMenu: ChooseMenu(user, message); break;
            case Step.TakeTest: TakeTest(user,message); break;
            case Step.CheckTest: CheckTest(user,message); break;
            case Step.SaveTest: SaveTest(user, message); break;
            case Step.SaveMessageForAdmin: SaveMessageForAdmin(user, message); break;
            case Step.GetApplicationByDate: GetMessagesByDate(user, message); break;
            case Step.SaveChannel: SaveChannel(user, message); break;
            case Step.DeleteChannel: DeleteChannel(user, message); break;
            case Step.SendAllUsers: SendAllUsers(user); break;
            case Step.SaveAdmin: SaveAdmin(user,message); break;
            case Step.RemoveAdmin: RemoveAdminFromData(user, message); break;
            case Step.GetResult: GetResult(user, message); break;
        }

    }
}
async void AskLanguage(User user)
{   var text = "Choose the language \nВыберите язык";
    user.UserStep = Step.SaveLanguage;
    userService.UpdateUser();
    ReplyKeyboardMarkup keyboard = EnStaticService.ChooseLanguage();
    await bot.SendTextMessageAsync(user.ChatId, text, replyMarkup: keyboard);
}
void SaveLanguage(User user, string message)
{
    if (message == "Russian") 
    {
        user.Language = "Russian";
    }
    else if (message == "English") 
    {
        user.Language = "English";
    }
    else { 
        AskLanguage(user);
        return;
    }
    user.UserStep= Step.AskName;
    userService.UpdateUser();
    AskName(user);
}
async void AskName(User user)
{
    var text = "";
    if(user.Language == "English") 
    {
         text = "Please send your name";
    }
    else 
    {
        text = "Пожалуйста, пришлите свое имя";
    }

    user.UserStep = Step.SaveName;
    userService.UpdateUser();
    await bot.SendTextMessageAsync(user.ChatId, text);
}
void SaveName(User user, string message)
{   
    AskPhoneNumber(user);
    user.FirstName = message;
    user.UserStep = Step.SavePhoneNumber;
    userService.UpdateUser();

}
void AskPhoneNumber(User user, bool check = false)
{
    var buttons = new List<List<KeyboardButton>>();
    if (user.Language == "English")
    {
        var rows = new List<KeyboardButton>()
        { 
            KeyboardButton.WithRequestContact("Send your contact")
        };
        buttons.Add(rows);
    }
    else 
    {
        var rows = new List<KeyboardButton>()
        {
            KeyboardButton.WithRequestContact("Отправьте свой контакт")
        };
        buttons.Add(rows);
    }

    var keyboard = new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };

    user.UserStep = Step.SavePhoneNumber;
    userService.UpdateUser();
    string text;
    if (check && user.Language == "English")
    { 
        text = "You sent wrong info, so send your contact with this button.\n If you send, you can go on";
    }
    else if(check && user.Language == "Russian") 
    {
        text = "Вы отправили неверную информацию, поэтому отправьте свой контакт с помощью этой кнопки.\n Если отправите, можете продолжать.";
    }
    else if(user.Language == "English")
    {
        text = "number :";
    }
    else
    {
        text = "номер :";
    }

    bot.SendTextMessageAsync(user.ChatId, text, replyMarkup: keyboard);
}
void SavePhoneNumber(User user, Update update)
{
    string? number = update.Message?.Contact?.PhoneNumber;

    if (string.IsNullOrEmpty(number))
        AskPhoneNumber(user, true);
    else
    {
        user.PhoneNumber = number;
        user.UserStep = Step.SaveGrade;
        userService.UpdateUser();
    }
    AskGrade(user);
}
async void AskGrade(User user)
{
    ReplyKeyboardMarkup keyboard;
    string text;
    if (user.Language == "English")
    {
        text = "Choose your grade";
        keyboard = EnStaticService.GetGrade();
    }
    else
    {
        text = "Выберите свой класс";
        keyboard = RuStaticService.GetGrade();
    }
    user.UserStep = Step.SaveGrade;
    userService.UpdateUser();
    await bot.SendTextMessageAsync(user.ChatId, text, replyMarkup: keyboard);
}
void SaveGrade(User user,string grade) 
{
    user.Grade = grade;
    user.UserStep = Step.ChooseMenu;
    userService.UpdateUser();
    ShowMenu(user);

}
void ShowMenu(User user)
{
    user.UserStep = Step.ChooseMenu;
    userService.UpdateUser();
    ReplyKeyboardMarkup keyboard;
    if (user.Language == "English")
    {
        switch (user.Role)
        {
            case UserRole.User: keyboard = EnStaticService.GetUserMenu(); break;
            case UserRole.Admin: keyboard = EnStaticService.GetAdminMenu(); break;
            case UserRole.SuperAdmin: keyboard = EnStaticService.GetSuperAdminMenu(); break;
            default: keyboard = EnStaticService.GetUserMenu(); break;
        }
        bot.SendTextMessageAsync(user.ChatId, "Menu :", replyMarkup: keyboard);
    }
    else 
    {
        switch (user.Role)
        {
            case UserRole.User: keyboard = RuStaticService.GetUserMenu(); break;
            case UserRole.Admin: keyboard = RuStaticService.GetAdminMenu(); break;
            case UserRole.SuperAdmin: keyboard = RuStaticService.GetSuperAdminMenu(); break;
            default: keyboard = RuStaticService.GetUserMenu(); break;
        }
        bot.SendTextMessageAsync(user.ChatId, "Меню :", replyMarkup: keyboard);
    }
}
void ChooseMenu(User user, string message)
{
    switch (user.Role)
    {
        case UserRole.User: ChooseUserMenu(user, message); break;
        case UserRole.Admin: ChooseAdminMenu(user, message); break;
        case UserRole.SuperAdmin: ChooseSuperAdminMenu(user, message); break;
        default: ChooseUserMenu(user, message); break;
    }
}
void ChooseUserMenu(User user, string message)
{
    try
    {
        if (user.Language == "English")
        {
            switch (message)
            {
                case EnConstants.TakeTestText: GetTest(user); break;
                case EnConstants.MessageToAdminText: SendMessageToAdmin(user); break;
                default: ShowMenu(user); break;
            }
        }
        else
        {
            switch (message)
            {
                case RuConstants.TakeTestText: GetTest(user); break;
                case RuConstants.MessageToAdminText: SendMessageToAdmin(user); break;
                default: ShowMenu(user); break;
            }
        }
    }
    catch (Exception e)
    {
        ShowMenu(user);
    }
}
void ChooseAdminMenu(User user, string message)
{
    try
    {
        if (user.Language == "English")
        {
            switch (message)
            {
                case EnConstants.TakeTestText: GetTest(user); break;
                case EnConstants.AddTest: AddTest(user); break;
                case EnConstants.MessageOfUsers: GetMessages(user); break;
                default: ShowMenu(user); break;
            }
        }
        else 
        {
            switch (message)
            {
                case RuConstants.TakeTestText: GetTest(user); break;
                case RuConstants.AddTest: AddTest(user); break;
                case RuConstants.MessageOfUsers: GetMessages(user); break;
                default: ShowMenu(user); break;
            }
        }
    }
    catch (Exception e)
    {
        ShowMenu(user);
    }
}
void ChooseSuperAdminMenu(User user, string message)
{
    try
    {
        if (user.Language == "English")
        {
            switch (message)
            {
                case EnConstants.TakeTestText: GetTest(user); break;
                case EnConstants.AddTest: AddTest(user); break;
                case EnConstants.ShowResultText: ShowResult(user); break;
                case EnConstants.MessageOfUsers: GetMessages(user); break;
                case EnConstants.AddAdmin: AddAdmin(user); break;
                case EnConstants.RemoveAdmin: RemoveAdmin(user); break;
                case EnConstants.AddChannelLink: AddChannelLink(user); break;
                case EnConstants.RemoveChannelLinks: RemoveChannel(user); break;
                case EnConstants.GetAllUsers: GetAllUsers(user); break;
                default: ShowMenu(user); break;
            }
        }
        else
        {
            switch (message)
            {
                case RuConstants.TakeTestText: GetTest(user); break;
                case RuConstants.AddTest: AddTest(user); break;
                case RuConstants.ShowResultText:ShowResult(user); break;
                case RuConstants.MessageOfUsers: GetMessages(user); break;
                case RuConstants.AddAdmin: AddAdmin(user); break;
                case RuConstants.RemoveAdmin: RemoveAdmin(user); break;
                case RuConstants.AddChannelLink: AddChannelLink(user); break;
                case RuConstants.RemoveChannelLinks: RemoveChannel(user); break;
                case RuConstants.GetAllUsers: GetAllUsers(user); break;
                default: ShowMenu(user); break;
            }
        }
    }
    catch (Exception e)
    {
        ShowMenu(user);
    }
}
void ShowResult(User user)
{
    string text;
    if (user.Language == "English") 
    {
        text = "Write the Id of the test";
    }
    else 
    {
        text = "Напишите идентификатор теста";
    }
    user.UserStep = Step.GetResult;
    userService.UpdateUser();
    SendingBack(user);
}
void GetResult(User user, string message)
{

}
void GetTest(User user) 
{
    string text;
    if(user.Language == "English") 
    {
        text = "Write Id of the test";
    }
    else 
    {
        text = "Напишите идентификатор теста";
    }
    user.UserStep = Step.TakeTest;
    userService.UpdateUser();
    bot.SendTextMessageAsync(user.ChatId,text);
    SendingBack(user);
}
void TakeTest(User user,string message) 
{
    string text;
    if(!string.IsNullOrEmpty(message)) 
    {
        var check = EnStaticService.CheckNumber(message);
        if (check) 
        {
            GetTest(user);
            return;
        }
        var testsId = int.Parse(message);
        if(testsId > 0) 
        {   
            if (testService.GetTests(testsId) != null) 
            {
                user.TestId = testsId;
                if (user.Language == "English") 
                {
                    text = "Now write the answers in order.\n Example: abcd or 1a2b3c4d";
                }
                else 
                {
                    text = "Теперь запишите ответы по порядку.\n Пример: abcd или 1a2b3c4d.";
                }

                user.UserStep = Step.CheckTest;
                userService.UpdateUser();
            }
            else if(user.Language=="English")
            {
                text = "Test with this Id is not exist";
            }
            else 
            {
                text = "Тест с этим идентификатором не существует";
            }
        }
        else if(user.Language == "English")
        {
            text = "Make sure to type correct test Id.";
        }
        else 
        {
            text = "Обязательно введите правильный идентификатор теста.";
        }
        bot.SendTextMessageAsync(user.ChatId, text);
    }
}
void CheckTest(User user,string message) 
{
    string text;
    byte correctAnswerCount = 0;
    var tests = testService.GetTests(user.TestId);
    int b = 0;
    foreach(var c in message.ToLower())
    {
        if (!char.IsDigit(c) && tests.Answers.Count>b)                                  
        {
            if (tests.Answers[b] == c)
            {
                correctAnswerCount++;
            }
            b++;
        }
    }
    if (user.Language == "English")
    {
        text = $"Total question {tests.Answers.Count}\n" +
            $"Total correct answer {correctAnswerCount}\n" +
            $"Total mistakes {tests.Answers.Count - correctAnswerCount}";
    }
    else 
    {
        text = $"Общий вопрос {tests.Answers.Count}\n" +
            $"Всего правильный ответ {correctAnswerCount}\n" +
            $"Всего ошибок {tests.Answers.Count - correctAnswerCount}";
    }
    bot.SendTextMessageAsync(user.ChatId, text);
    ShowMenu(user);
}
void AddTest(User user) 
{
    string text;
    if(user.Language == "English") 
    {
        text = "Write the answers.\nExample:abcd";
    }
    else 
    {
        text = "Написать ответы.\nПример:abcd";
    }
    user.UserStep = Step.SaveTest;
    userService.UpdateUser();
    bot.SendTextMessageAsync(user.ChatId,text);
}
void SaveTest(User user,string message)
{
    int id = testService.GetId();
    List<char> chars = message.ToLower().ToList<char>();
    testService.AddTest(id, chars);
    string text;
    if (user.Language == "English") 
    {
        text = "Test saved successfully";
    }
    else 
    {
        text = "\nТест успешно сохранен.";
    }
    bot.SendTextMessageAsync(user.ChatId, text);
    ShowMenu(user);
    
}
async void SendMessageToAdmin(User user)
{
    string text;
    if (user.Language == "English") 
    {
        text = "Write anything that you want to let us know";
    }
    else 
    {
        text = "Напишите все, что вы хотите сообщить нам";
    }
    user.UserStep = Step.SaveMessageForAdmin;
    userService.UpdateUser();
    await bot.SendTextMessageAsync(user.ChatId, text);
    SendingBack(user);
}
async void SaveMessageForAdmin(User user, string message)
{
    applicationService.AddApplication(user, message);
    string text;
    if (user.Language == "English")
    {
        text = "Your message was sent to admin";
    }
    else 
    {
        text = "Ваше сообщение было отправлено администратору";
    }
    await bot.SendTextMessageAsync(user.ChatId, text);
    ShowMenu(user);
}
bool CheckDate(string message)
{
    return message.Contains(',');
}
void GetMessages(User user)
{
    string text;
    if (user.Language == "English")
    {
        text = "You need to enter from date to date \n" +
        "Example : \nday.month.year,day.month.year";
    }
    else 
    {
        text = "\r\nВам нужно ввести дату от даты \n" +
            "Пример: \nдень.месяц.год,день.месяц.год.";
    }

    user.UserStep = Step.GetApplicationByDate;
    userService.UpdateUser();
    bot.SendTextMessageAsync(user.ChatId, text);
    SendingBack(user);
}
void GetMessagesByDate(User user, string message)
{
    string text;
    string text2;

    if (!CheckDate(message))
    {

        if (user.Language == "English")
        {
            text = "You didn't follow instructions! please send dates as example";
        }
        else
        {
            text = "Вы не следовали инструкциям! пожалуйста, пришлите даты в качестве примера";
        }
        bot.SendTextMessageAsync(user.ChatId, text);
        GetMessages(user);
        return;
    }
    else if (user.Language == "English")
    {
        text2 = "application file";
    }
    else 
    {
        text2 = "файл приложения"; 
    }
    var applications = applicationService.Applications;
    var sortedApplications = EnStaticService.SortApplicationsByDate(applications, message);
    EnStaticService.GetApplications(sortedApplications);

    var data = File.ReadAllBytes(EnConstants.ApplicationPath);
    var ms = new MemoryStream(data);
    var file = new InputOnlineFile(ms);

    bot.SendDocumentAsync(user.ChatId, document: file, caption: text2);
    ShowMenu(user);
}
async void CheckingForChannel(User user, Update update)
{
    var channels = channelService.Channels.ToList();
    if (channels is null || channels.Count == 0)
    {
        UserActions(update);
    }
    else
    {
        bool check = false;
        foreach (var channel in channels)
        {
            var data = await bot.GetChatMemberAsync(chatId: channel.ChannelId, userId: user.ChatId);
            if (data.Status == ChatMemberStatus.Administrator || data.Status == ChatMemberStatus.Member || data.Status == ChatMemberStatus.Creator)
            {
                check = true;
            }
            else
            {
                check = false;
                break;
            }
        }
        if (check)
        {
            UserActions(update);
        }
        else
        {
            AskAboutJoining(user);
        }
    }
}
void AskAboutJoining(User user)
{
    string text;
    string text2;
    if (user.Language == "English")
    {
        text = "Please follow to this channels";
        text2 = "Check";
    }
    else 
    {
        text = "Пожалуйста, следуйте этим каналам";
        text2 = "Проверять";
    }
    var buttons = new List<List<InlineKeyboardButton>>();
    var channels = channelService.Channels;
    foreach (var channel in channels)
    {
        var row1 = new List<InlineKeyboardButton>()
        {
            InlineKeyboardButton.WithUrl(channel.ChannelName,channel.Link)
        };
        buttons.Add(row1);
    }
    var row2 = new List<InlineKeyboardButton>()
    {
        InlineKeyboardButton.WithCallbackData(text2)
    };
    buttons.Add(row2);
    var keyboard = new InlineKeyboardMarkup(buttons);

    bot.SendTextMessageAsync(user.ChatId, text, replyMarkup: keyboard);
}
void AddChannelLink(User user)
{
    string text;
    if (user.Language == "English")
    {
        text = "First make this bot admin in channel that you want to add" +
        "\nAfter that you should type channel id, name and link in order to finish this action" +
        "\nExample:" +
        "\n-1002198930587,Avto test,https://t.me/AvtoTest_1202" +
        "\nYou can get channel id from https://t.me/username_to_id_bot";
    }
    else
    {
        text = "Сначала назначьте этого бота администратором на канале, который вы хотите добавить." +
            "\nПосле этого вам следует ввести идентификатор канала, название и ссылку, чтобы завершить это действие." +
            "\nПример:" +
            "\n-1002198930587,Avto test,https://t.me/AvtoTest_1202" +
            "\nВы можете получить идентификатор канала по адресу https://t.me/username_to_id_bot.";
    }

    user.UserStep = Step.SaveChannel;
    userService.UpdateUser();
    bot.SendTextMessageAsync(user.ChatId, text);
    SendingBack(user);
}
async void SaveChannel(User user, string message)
{
    string text;
    string text2;
    string text3;
    string text4;
    if (user.Language == "English")
    {
        text = "success";
        text2 =  "Make sure to type correct id that is exist ";
        text3 = "Make sure to type correct name";
        text4 =  "Make sure to type correct link that is exist ";
    }
    else
    {
        text = "успех";
        text2 = "Обязательно введите правильный идентификатор, который существует.";
        text3 = "Обязательно введите правильное имя";
        text4 = "Обязательно введите правильную существующую ссылку.";
    }
    var channelInfo = message.Split(',');
    if (string.IsNullOrEmpty(channelInfo[0]))
    {
        await bot.SendTextMessageAsync(user.ChatId,text2);
        AddChannelLink(user);
    }
    else if (string.IsNullOrEmpty(channelInfo[1]))
    {
        await bot.SendTextMessageAsync(user.ChatId,text3 );
        AddChannelLink(user);
    }
    else if (string.IsNullOrEmpty(channelInfo[2]) || !channelInfo[2].Contains("https://t.me/"))
    {
        await bot.SendTextMessageAsync(user.ChatId,text4);
        AddChannelLink(user);
    }
    else
    {
        var channelId = long.Parse(channelInfo[0]);
        var channels = channelService.AddChannel(channelId, channelInfo[1], channelInfo[2]);
        await bot.SendTextMessageAsync(user.ChatId, text);
        ShowMenu(user);
    }
}
void RemoveChannel(User user)
{
    string text;
    if (user.Language == "English")
    {
        text = "You should type channel id in order to finish this action." +
            "\nYou can get channel id from https://t.me/username_to_id_bot ";
    }
    else
    {
        text = "Вам необходимо ввести идентификатор канала, чтобы завершить это действие." +
            "\nВы можете получить идентификатор канала по адресу https://t.me/username_to_id_bot.";
    }
    user.UserStep = Step.DeleteChannel;
    userService.UpdateUser();
    bot.SendTextMessageAsync(user.ChatId, text);
    SendingBack(user);
}
void DeleteChannel(User user, string message)
{
    string text;
    string text2;
    string text3;
    if (user.Language == "English")
    {
        text = "success";
        text2 = "This id is not exist in database. Please make sure you are sending correct id!";
        text3 = "There isn't any channel in database to delete";
    }
    else
    {
        text = "успех";
        text2 = "Этот идентификатор не существует в базе данных. Пожалуйста, убедитесь, что вы отправляете правильный идентификатор!";
        text3 = "There isn't any channel in database to delete";
    }
    var channels = channelService.Channels;
    if (channels is not null)
    {

        var channelId = long.Parse(message);
        foreach (var channel in channels)
        {
            if (channel.ChannelId == channelId)
            {
                channelService.RemoveChannel(channelId);
                bot.SendTextMessageAsync(user.ChatId, text);
                ShowMenu(user);
                return;
            }
        }
        bot.SendTextMessageAsync(user.ChatId,text2);
        RemoveChannel(user);

    }
    else
    {
        bot.SendTextMessageAsync(user.ChatId, text3);
        ShowMenu(user);
    }
}
void GetAllUsers(User user)
{
    string text;
    if (user.Language == "English")
    {
        text = "Sending user file";
    }
    else
    {
        text = "Отправка пользовательского файла";
    }
    user.UserStep = Step.SendAllUsers;
    userService.UpdateUser();
    bot.SendTextMessageAsync(user.ChatId, text);
}
void SendAllUsers(User user)
{
    string text;
    if (user.Language == "English")
    {
        text = "User file";
    }
    else
    {
        text = "пользовательский файл";
    }
    var users = userService.Users;
    EnStaticService.GetUsers(users);

    var data = File.ReadAllBytes(EnConstants.UserInfoPath);
    var ms = new MemoryStream(data);
    var file = new InputOnlineFile(ms);

    bot.SendDocumentAsync(user.ChatId, document: file, caption: text);
    ShowMenu(user);
}
async void SendingBack(User user)
{
    string text;
    if (user.Language == "English")
    {
        text = "Back";
    }
    else
    {
        text = "Назад";
    }
    var back = EnStaticService.Back();
    await bot.SendTextMessageAsync(user.ChatId, text, replyMarkup: back);
}
void AddAdmin(User user)
{
    string text;
    if (user.Language == "English")
    {
        text = "You should send id of person that you want to make Admin. " +
            "\nIf you dont know id of person, you can get it by using telegram bot https://t.me/username_to_id_bot ";
    }
    else
    {
        text = "Вы должны отправить идентификатор человека, которого вы хотите сделать администратором." +
            "Если вы не знаете идентификатор человека, вы можете получить его с помощью бота Telegram https://t.me/username_to_id_bot";
    }
    user.UserStep = Step.SaveAdmin;
    userService.UpdateUser();
    bot.SendTextMessageAsync(user.ChatId, text: text);
    SendingBack(user);
}
void SaveAdmin(User user, string message)
{
    string text;
    string text2;
    if (user.Language == "English")
    {
        text = "This is invalid Id";
        text2 = "Not found user, make sure user is registered";
    }
    else
    {
        text = "Это неверный идентификатор";
        text2 = "Пользователь не найден, убедитесь, что пользователь зарегистрирован";
    }
    var check = EnStaticService.CheckNumber(message);
    if (check)
    {
        bot.SendTextMessageAsync(user.ChatId, text);
        return;
    }
    long chatId = Convert.ToInt64(message);

    var userForAdmin = userService.Users.FirstOrDefault(u => u.ChatId == chatId);
    if (user is null)
    {
  
        bot.SendTextMessageAsync(user.ChatId, text2);
        return;
    }
    userForAdmin.Role = UserRole.Admin;
    userService.UpdateUser();
    ShowMenu(user);
    ShowMenu(userForAdmin);

}
void RemoveAdmin(User user)
{
    string text;
    if (user.Language == "English")
    {
        text = "You should send id of person that tou want to remove from being Admin. " +
            "\nIf you don't know id of person, you can get it by using telegram bot https://t.me/username_to_id_bot ";
    }
    else
    {
        text = "Вы должны отправить идентификатор человека, которого вы хотите отстранить от должности администратора." +
            "\nЕсли вы не знаете идентификатор человека, вы можете получить его с помощью бота Telegram https://t.me/username_to_id_bot";
    }
    user.UserStep = Step.RemoveAdmin;
    userService.UpdateUser();
    bot.SendTextMessageAsync(user.ChatId, text: text);
    SendingBack(user);
}
void RemoveAdminFromData(User user, string message)
{
    string text;
    string text2;
    string text3;
    if (user.Language == "English")
    {
        text = "Its invalid Id";
        text2 = "Not found user, make sure user is registered";
        text3 = "This only works on Admins";
    }
    else
    {
        text = "Его неверный идентификатор";
        text2 = "Пользователь не найден, убедитесь, что пользователь зарегистрирован";
        text3 = "Это работает только для администраторов";
    }
    var check = EnStaticService.CheckNumber(message);
    if (check)
    {
  
        bot.SendTextMessageAsync(user.ChatId, text);
        return;
    }
    long chatId = Convert.ToInt64(message);
    var forRemovingAdmin = userService.Users.FirstOrDefault(u => u.ChatId == chatId);
    if (forRemovingAdmin is null)
    {
        bot.SendTextMessageAsync(user.ChatId, text2);
        return;
    }
    if (forRemovingAdmin.Role == UserRole.User && forRemovingAdmin.Role == UserRole.SuperAdmin)
    {
        ShowMenu(user);
    }
    forRemovingAdmin.Role = UserRole.User;
    userService.UpdateUser();
    ShowMenu(user);
    ShowMenu(forRemovingAdmin);
}
