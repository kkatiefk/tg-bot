using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using System.Text.Json;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text.Json.Serialization;


namespace tg
{
    public class MovieSeekerBot
    {
        private const string LocalApiUrl = "https://localhost:7096/Find";
        private readonly TelegramBotClient botClient;
        private readonly CancellationTokenSource cts;
        private readonly ReceiverOptions receiverOptions;
        private readonly HttpClient httpClient;

        public MovieSeekerBot()
        {
            botClient = new TelegramBotClient("MyApiKey");
            cts = new CancellationTokenSource();
            receiverOptions = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() };
            httpClient = new HttpClient();
        }

        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerErrorAsync, receiverOptions, cts.Token);
            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"Бот {botMe.Username} почав працювати!");
            Console.ReadKey();
            cts.Cancel();
        }

        private Task HandlerErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Error in telegram bot API:\n {apiRequestException.ErrorCode}" +
                                                           $"\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                await HandlerMessageAsync(botClient, update.Message);
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                await HandlerCallbackQueryAsync(botClient, update.CallbackQuery);
            }
        }

        public async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Вітаю! Я бот, який шукає інформацію про фільми, акторів. Також ти можеш оцінити фільм. \r\nОсь перелічені команди та їх опис: \r\n/search_title - знайти інформацію про фільм за назвою \r\n/search_release - знайти інформацію про фільм за роком релізу\r\n/search_actor - знайти коротку інформацію про актора \r\n/random - рандомна рекомендація\r\n/rate - оцінити фільм за айді від 1 до 10\r\n/get_rating - продвитися оцінку фільму за айді\r\n");
            }
            else if (message.Text == "/search_title")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Для пошуку інформації про фільм введіть: /search_title назва фільму");
            }
            else if (message.Text.StartsWith("/search_title "))
            {
                var searchTerm = Uri.EscapeDataString(message.Text.Substring("/search_title ".Length));
                try
                {
                    var response = await httpClient.GetAsync($"{LocalApiUrl}/find-by-title?filmName={searchTerm}");
                    response.EnsureSuccessStatusCode();
                    var responseString = await response.Content.ReadAsStringAsync();
                    var searchResult = JsonConvert.DeserializeObject<MovieSearchResult>(responseString);

                    if (searchResult.TitleResults != null && searchResult.TitleResults.Results.Count > 0)
                    {
                        var formattedInfo = string.Join("\n\n", searchResult.TitleResults.Results.Select(f => FormatFilmInfo(f)));
                        await botClient.SendTextMessageAsync(message.Chat.Id, formattedInfo);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Не знайдено жодного фільму за вказаною назвою.");
                    }
                }
                catch (HttpRequestException ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Error: " + ex.Message);
                }
            }
            else if (message.Text == "/search_release")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Для пошуку інформації про фільм введіть: /search_release рік релізу");
            }
            else if (message.Text.StartsWith("/search_release "))
            {
                var searchTerm = Uri.EscapeDataString(message.Text.Substring("/search_release ".Length));
                try
                {
                    var response = await httpClient.GetAsync($"{LocalApiUrl}/find-by-date?titleReleaseText={searchTerm}");
                    response.EnsureSuccessStatusCode();
                    var responseString = await response.Content.ReadAsStringAsync();
                    var searchResult = JsonConvert.DeserializeObject<MovieSearchResult>(responseString);

                    if (searchResult.TitleResults != null && searchResult.TitleResults.Results.Count > 0)
                    {
                        var formattedInfo = string.Join("\n\n", searchResult.TitleResults.Results.Select(f => FormatFilmInfo(f)));
                        await botClient.SendTextMessageAsync(message.Chat.Id, formattedInfo);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Не знайдено жодного фільму за вказаним роком.");
                    }
                }
                catch (HttpRequestException ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Error: " + ex.Message);
                }
            }
            else if (message.Text == "/search_actor")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Для пошуку інформації про актора введіть: /search_actor ім'я актора");
            }
            else if (message.Text.StartsWith("/search_actor "))
            {
                var searchTerm = Uri.EscapeDataString(message.Text.Substring("/search_actor ".Length));
                try
                {
                    var response = await httpClient.GetAsync($"{LocalApiUrl}/find-by-credits?topCredit={searchTerm}");
                    response.EnsureSuccessStatusCode();
                    var responseString = await response.Content.ReadAsStringAsync();
                    var searchResult = JsonConvert.DeserializeObject<ActorSearchResult>(responseString);

                    if (searchResult.NameResults != null && searchResult.NameResults.Results.Count > 0)
                    {
                        var formattedInfo = string.Join("\n\n", searchResult.NameResults.Results.Select(a => FormatActorInfo(a)));
                        await botClient.SendTextMessageAsync(message.Chat.Id, formattedInfo);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Не знайдено жодного актора за вказаним ім'ям.");
                    }
                }
                catch (HttpRequestException ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Error: " + ex.Message);
                }
            }
            else if (message.Text == "/random")
            {
                try
                {
                    var response = await httpClient.GetAsync($"{LocalApiUrl}/random-film");
                    response.EnsureSuccessStatusCode();
                    var responseString = await response.Content.ReadAsStringAsync();
                    var movieInfo = JsonConvert.DeserializeObject<MovieInfo>(responseString);
                    await botClient.SendTextMessageAsync(message.Chat.Id, FormatFilmInfo(movieInfo));
                }
                catch (HttpRequestException ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Error: " + ex.Message);
                }
            }
            else if (message.Text == "/rate")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введіть команду у форматі: /rate [айді фільму] [оцінка від 1 до 10]");
            }
            else if (message.Text.StartsWith("/rate "))
            {
                var parts = message.Text.Substring("/rate ".Length).Split(' ');
                if (parts.Length == 2 && int.TryParse(parts[1], out int rating) && rating >= 1 && rating <= 10)
                {
                    var movieId = parts[0];
                    try
                    {
                        var url = $"https://localhost:7096/Find/rate-film?filmId={movieId}&score={rating}";
                        var response = await httpClient.PostAsync(url, null);
                        response.EnsureSuccessStatusCode();
                        var responseMessage = await response.Content.ReadAsStringAsync();

                        await botClient.SendTextMessageAsync(message.Chat.Id, responseMessage);
                    }
                    catch (HttpRequestException ex)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Error: " + ex.Message);
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Неправильно введені дані! Спробуйте ще раз.");
                }
            }
            else if (message.Text == "/get_rating")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Щоб продивитися, чи є оцінка у фільма, введіть дані у такому форматі: /get_rating [айді фільму]");
            }
            else if (message.Text.StartsWith("/get_rating "))
            {
                var movieId = message.Text.Substring("/get_rating ".Length);
                try
                {
                    var url = $"https://localhost:7096/Find/get-rating?filmId={movieId}";
                    var response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    var ratingResponse = JsonConvert.DeserializeObject<MovieRatingResponse>(jsonResponse);

                    if (ratingResponse == null || ratingResponse.Score == 0)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Фільм ще не оцінено.");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Оцінка фільму: {ratingResponse.Score}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Error: " + ex.Message);
                }
            }


        }

        public async Task HandlerCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            // handle callback queries here
        }



        public string FormatFilmInfo(MovieInfo film)
        {
            var id = film.Id;
            var titleName = film.TitleNameText ?? "відсутні дані";
            var posterUrl = film.TitlePosterImageModel?.Url ?? "відсутні дані";
            var titleType = film.TitleTypeText ?? "відсутні дані";
            var topCredits = film.TopCredits != null ? string.Join(", ", film.TopCredits) : "відсутні дані";
            var releaseDate = film.TitleReleaseText ?? "відсутні дані";

            var formattedInfo = $"id: {id}\n" +
                                $"Назва: {titleName}\n" +
                                $"Тип: {titleType}\n" +
                                $"Дата випуску: {releaseDate}\n" +
                                $"Кредити: {topCredits}\n" +
                                $"Постер: {posterUrl}\n";

            return formattedInfo;
        }
        public string FormatActorInfo(ActorInfo actor)
        {
            var name = actor.DisplayNameText ?? "відсутні дані";
            var knownFor = actor.KnownForTitleText ?? "відсутні дані";
            var job = actor.KnownForJobCategory ?? "відсутні дані";
            var year = actor.KnownForTitleYear ?? "відсутні дані";
            var avatarUrl = actor.AvatarImageModel?.Url ?? "відсутні дані";

            var formattedInfo = $"Ім'я: {name}\n" +
                                $"Відома роль: {knownFor} ({year})\n" +
                                $"Професія: {job}\n" +
                                $"Аватар: {avatarUrl}\n";

            return formattedInfo;
        }

        

    }

}
