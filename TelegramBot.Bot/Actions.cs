using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using Refit;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Bot
{
    public class Actions
    {
        private readonly TelegramBotClient _bot;
        private readonly ILogger _logger;
        private readonly Register _register = new Register();
        private string action = string.Empty;
        private int step = 1;

        public Actions(TelegramBotClient bot, ILogger logger)
        {
            _bot = bot;
            _logger = logger;
        }

        public async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            Message message = messageEventArgs.Message;

            _logger.Information("New message received from user: {user}", message.From.Id);

            if (message == null || message.Type != MessageType.Text)
                return;

            if (message.Text.Split(' ').First().Equals("/cadastro"))
                action = "cadastro";

            if (message.Text.Split(' ').First().Equals("/cep"))
                action = "cep";

            if (action.Equals("cadastro"))
            {
                if (step == 1)
                {
                    await _bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Qual o seu Nome?",
                        replyMarkup: new ReplyKeyboardRemove());

                    step++;

                    return;
                }

                if (step == 2)
                {
                    _register.Name = message.Text;

                    await _bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Qual a sua idade?",
                        replyMarkup: new ReplyKeyboardRemove());

                    step++;

                    return;
                }

                if (step == 3)
                {
                    _register.Age = Convert.ToInt32(message.Text);

                    await _bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Olá {_register.Name}, você tem {_register.Age} anos.",
                        replyMarkup: new ReplyKeyboardRemove());

                    action = string.Empty;

                    step = 1;

                    return;
                }
            }

            if (action.Equals("cep"))
            {
                if (step == 1)
                {
                    await _bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Qual o cep?",
                        replyMarkup: new ReplyKeyboardRemove());

                    step++;

                    return;
                }

                if (step == 2)
                {
                    var gitHubApi = RestService.For<IViaCepApi>("https://viacep.com.br/ws");

                    var octocat = await gitHubApi.GetAsync(message.Text);

                    await _bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Cidade: {octocat.Localidade}{Environment.NewLine}" +
                              $"UF: {octocat.Uf}",
                        replyMarkup: new ReplyKeyboardRemove());

                    action = string.Empty;

                    step = 1;

                    return;
                }
            }

            await Usage(message);

            async Task SendInlineKeyboardAgendar(Message message)
            {
                await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Nome", "name"),
                        InlineKeyboardButton.WithCallbackData("Sexo", "Sexo"),
                    }
                });

  
            }










            async Task Usage(Message message)
            {
                const string usage = "Usage:\n" +
                                        "/cep   - send inline keyboard\n" +
                                        "/cadastro   - Agendar Consulta";

                await _bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: usage,
                    replyMarkup: new ReplyKeyboardRemove()
                );
            }
        }

        // Process Inline Keyboard callback data
        public async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;

            await _bot.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"Received {callbackQuery.Data}"
            );

            await _bot.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Received {callbackQuery.Data}"
            );
        }

        public async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            Console.WriteLine($"Received inline query from: {inlineQueryEventArgs.InlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                // displayed result
                new InlineQueryResultArticle(
                    id: "3",
                    title: "TgBots",
                    inputMessageContent: new InputTextMessageContent(
                        "hello"
                    )
                )
            };
            await _bot.AnswerInlineQueryAsync(
                inlineQueryId: inlineQueryEventArgs.InlineQuery.Id,
                results: results,
                isPersonal: true,
                cacheTime: 0
            );
        }

        public async void BotOnMessageEdited(object sender, MessageEventArgs messageEventArgs)
        {
            Message message = messageEventArgs.Message;

            _logger.Information("New message received (edited) from user: {user}", message.From.Id);
        }

        public void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        public void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message
            );
        }
    }
}
