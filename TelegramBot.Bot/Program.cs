using Serilog;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.Bot
{
    class Program
    {
        private static TelegramBotClient Bot;

        public static async Task Main()
        {
            Log.Logger = FactoryLogger.Factory();

            Log.Information("Starting up");

            Bot = new TelegramBotClient("1065990287:AAEHORH8nzdde9RT6YRKi9_gVAPMpQaonaE");

            User me = await Bot.GetMeAsync();

            Actions actions = new Actions(Bot, Log.Logger);

            Bot.OnMessage += actions.BotOnMessageReceived;
            Bot.OnMessageEdited += actions.BotOnMessageEdited;
            Bot.OnCallbackQuery += actions.BotOnCallbackQueryReceived;
            Bot.OnInlineQuery += actions.BotOnInlineQueryReceived;
            Bot.OnInlineResultChosen += actions.BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += actions.BotOnReceiveError;

            Bot.StartReceiving(Array.Empty<UpdateType>());

            Log.Information("Start listening for {me}", me.Username);

            Console.ReadLine();

            Bot.StopReceiving();
        }
    }
}