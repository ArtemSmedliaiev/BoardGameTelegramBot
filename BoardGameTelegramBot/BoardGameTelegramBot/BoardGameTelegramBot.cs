using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BoardGameTelegramBot.Models;
using BoardGameTelegramBot.APIclient;

namespace BoardGameTelegramBot
{
    public class BGTelegramBot
    {
        TelegramBotClient botClient = new TelegramBotClient("5239203424:AAGBJBX7gCUc_y5z7TwaiMfKA07y6AjCHYA");
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        private CategoriesInfo Categories { get; set; }
        private MechanicsInfo Mechanics { get; set; }
        private GamesInfo GamesInfo { get; set; }
        private string GameName { get; set; }
        private int switcher { get; set; }
        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();
            Client client = new Client();
            GamesInfo = client.GetGamesAsync("Catan").Result;
            Categories = client.GetCategoriesAsync().Result;
            Mechanics = client.GetMechanicsAsync().Result;
            Console.WriteLine($"Bot {botMe.Username} start working");
            Console.ReadKey();
        }

        private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Error in telegram bot API:\n{apiRequestException.ErrorCode}" + $"\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                await HandleMessageAsync(botClient, update.Message);
            }
            if (update.Type == UpdateType.CallbackQuery )
            {
                await HandleCallBackQuery(botClient, update.CallbackQuery);                
            }
        }

        private async Task HandleCallBackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            foreach(BoardGame gamesInfo in GamesInfo.games)
            {
                if (gamesInfo.name.StartsWith(callbackQuery.Data))
                {
                    await GetGameAsync(callbackQuery.Message, callbackQuery.Data);
                    return;
                }
            }
        }

        private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Hi! This bot will help you to find some information about board games");
                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] { new KeyboardButton[] { "Find by name" }, new KeyboardButton[] { "Find by categorie", "Find by mechanic" }, new KeyboardButton[] { "Find by year" }, new KeyboardButton[] { "Get random game" } })
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(message.Chat.Id, "Choose what you want:", replyMarkup: replyKeyboardMarkup);
            }
            else if (message.Text == "Find by name")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter game's name:");
                switcher = 1;
            }
            else if(message.Text == "Find by year")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter game's year:");
                switcher = 2;
            }
            else if(message.Text == "Find by categorie")
            {
                await GetCategoriesAsync(message);
                switcher = 3;
            }
            else if(message.Text == "Find by mechanic")
            {
                await GetMechanicsAsync(message);
                switcher = 4;
            }
            else if(message.Text == "Get random game")
            {
                await GetRandomGame(message);
            }
            else if(switcher == 1)
            {                
                await GetGamesAsync(message);
                switcher = 0;
            }
            else if (switcher == 2)
            {
                await GetGamesOnYearAsync(message);
                switcher = 0;                
            }
            else if(switcher == 3)
            {
                await GetGamesOnCategorieAsync(message);
                switcher = 0;
            }
            else if(switcher == 4)
            {
                await GetGamesOnMechanicAsync(message);
                switcher = 0;
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Sorry, шо?");
            }
        }


        private async Task GetGameAsync(Message message, string game)
        {
            Client client = new Client();
            BoardGame boardGame = client.GetGameAsync(game).Result;
            string text = $"{boardGame.name} \n Year published: {boardGame.year_published} \n Players: {boardGame.players} \n Playtime: {boardGame.playtime} \n Description: \n {boardGame.description_preview}";
            if (boardGame.rules_url != null && boardGame.rules_url.StartsWith("https://"))
            {
                InlineKeyboardMarkup inlineKeyboardMarkup = new(new[] { InlineKeyboardButton.WithUrl("Show rules", boardGame.rules_url) });
                await botClient.SendTextMessageAsync(message.Chat.Id, text, replyMarkup: inlineKeyboardMarkup);
                await botClient.SendPhotoAsync(message.Chat.Id, photo: boardGame.image_url);
            }
            else
            {                
                await botClient.SendTextMessageAsync(message.Chat.Id, text);
                await botClient.SendPhotoAsync(message.Chat.Id, boardGame.image_url);
            }
        }

        private async Task GetGamesAsync(Message message)
        {
            Client client = new Client();
            GamesInfo = client.GetGamesAsync(message.Text).Result;
            List<string> names_list = client.GetGamesNamesAsync(message.Text).Result;
            List<List<InlineKeyboardButton>> nameslist = new List<List<InlineKeyboardButton>>();
            InlineKeyboardMarkup inlineKeyboardMarkup = new(nameslist);
            foreach (string name in names_list)
            {
                List<InlineKeyboardButton> keyboardMarkup = new List<InlineKeyboardButton>();
                InlineKeyboardButton _name = new(text: name) { CallbackData = name.Split("...")[0]};       
                keyboardMarkup.Add(_name);
                nameslist.Add(keyboardMarkup);
            }
            await botClient.SendTextMessageAsync(message.Chat.Id, text: "Choose game:", replyMarkup: inlineKeyboardMarkup);
        }

        private async Task GetGamesOnYearAsync(Message message)
        {
            Client client = new Client();
            GamesInfo = client.GetGamesOnYearAsync(message.Text).Result;
            List<string> names_list = client.GetGamesNamesOnYearAsync(message.Text).Result;
            List<List<InlineKeyboardButton>> nameslist = new List<List<InlineKeyboardButton>>();
            InlineKeyboardMarkup inlineKeyboardMarkup = new(nameslist);
            foreach (string name in names_list)
            {
                List<InlineKeyboardButton> keyboardMarkup = new List<InlineKeyboardButton>();                                                                      
                InlineKeyboardButton _name = new InlineKeyboardButton(name);
                _name.CallbackData = name;
                keyboardMarkup.Add(_name);
                nameslist.Add(keyboardMarkup);                 
            }
            await botClient.SendTextMessageAsync(message.Chat.Id, "Choose game:", replyMarkup: inlineKeyboardMarkup);
        }

        private async Task GetGamesOnCategorieAsync(Message message)
        {
            Client client = new Client();
            GamesInfo = client.GetGamesOnCategorieAsync(message.Text).Result;
            var list = GamesInfo;
            List<string> names_list = new List<string>();
            foreach (var item in list.games)
            {
                string[] _name = item.name.Split(" ");
                if (_name.Length > 7)
                {
                    item.name = String.Join(" ", _name[..7]) + "...";
                    names_list.Add(item.name);
                }
                else
                {
                    names_list.Add(item.name);
                }
            }
            List<List<InlineKeyboardButton>> nameslist = new List<List<InlineKeyboardButton>>();
            InlineKeyboardMarkup inlineKeyboardMarkup = new(nameslist);
            foreach (string name in names_list)
            {
                List<InlineKeyboardButton> keyboardMarkup = new List<InlineKeyboardButton>();
                InlineKeyboardButton _name = new InlineKeyboardButton(name);
                _name.CallbackData = name;
                keyboardMarkup.Add(_name);
                nameslist.Add(keyboardMarkup);
            }
            await botClient.SendTextMessageAsync(message.Chat.Id, "Choose game:", replyMarkup: inlineKeyboardMarkup);
        }

        private async Task GetCategoriesAsync(Message message)
        {
            Client client = new Client();
            string[] nameslist = new string[Categories.categories.Count];
            int i = 0;
            foreach(CategorieInfo categorie in Categories.categories)
            {
                nameslist[i] = categorie.name.Replace(" ", "").Replace("-", "").Replace("/", "").Replace("'", "").Replace("&", "");
                i++;
            }
            string text = String.Join("\n /", nameslist[..99]);
            string text2 = String.Join("\n /", nameslist[99..]);
            await botClient.SendTextMessageAsync(message.Chat.Id, "Choose categorie: \n/" + text);
            await botClient.SendTextMessageAsync(message.Chat.Id, "/" + text2);
        }

        private async Task GetGamesOnMechanicAsync(Message message)
        {
            Client client = new Client();
            GamesInfo = client.GetGamesOnMechanicsAsync(message.Text).Result;
            var list = GamesInfo;
            List<string> names_list = new List<string>();
            foreach (var item in list.games)
            {
                string[] _name = item.name.Split(" ");
                if (_name.Length > 7)
                {
                    item.name = String.Join(" ", _name[..7]) + "...";
                    names_list.Add(item.name);
                }
                else
                {
                    names_list.Add(item.name);
                }
            }
            List<List<InlineKeyboardButton>> nameslist = new List<List<InlineKeyboardButton>>();
            InlineKeyboardMarkup inlineKeyboardMarkup = new(nameslist);
            foreach (string name in names_list)
            {
                List<InlineKeyboardButton> keyboardMarkup = new List<InlineKeyboardButton>();
                InlineKeyboardButton _name = new InlineKeyboardButton(name);
                _name.CallbackData = name;
                keyboardMarkup.Add(_name);
                nameslist.Add(keyboardMarkup);
            }
            await botClient.SendTextMessageAsync(message.Chat.Id, "Choose game:", replyMarkup: inlineKeyboardMarkup);
        }

        private async Task GetMechanicsAsync(Message message)
        {
            Client client = new Client();
            string[] nameslist = new string[Mechanics.mechanics.Count];
            int i = 0;
            foreach (MechanicInfo mechanic in Mechanics.mechanics)
            {
                nameslist[i] = mechanic.name.Replace(" ", "").Replace("-", "").Replace("/", "").Replace("'", "").Replace("&", "").Replace(",", "").Replace("(", "").Replace(")", "").Replace(":", "");
                i++;
            }
            string text = String.Join("\n /", nameslist[..99]);
            string text2 = String.Join("\n /", nameslist[99..]);
            await botClient.SendTextMessageAsync(message.Chat.Id, "Choose mechanic: \n/" + text);
            await botClient.SendTextMessageAsync(message.Chat.Id, "/" + text2);
        }

        private async Task GetRandomGame(Message message)
        {
            Client client = new Client();
            BoardGame boardGame = client.GetRandomGameAsync().Result;
            string text = $"{boardGame.name} \n Year published: {boardGame.year_published} \n Players: {boardGame.players} \n Playtime: {boardGame.playtime} \n Description: \n {boardGame.description_preview}";
            if (boardGame.rules_url != null)
            {
                InlineKeyboardMarkup inlineKeyboardMarkup = new(new[] { InlineKeyboardButton.WithUrl("Show rules", boardGame.rules_url) });
                await botClient.SendTextMessageAsync(message.Chat.Id, text, replyMarkup: inlineKeyboardMarkup);
                await botClient.SendPhotoAsync(message.Chat.Id, photo: boardGame.image_url);
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, text);
                await botClient.SendPhotoAsync(message.Chat.Id, boardGame.image_url);
            }
        }

    }
}
