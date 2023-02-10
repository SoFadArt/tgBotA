using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Reflection;
using tgtgbot;

var apiKey = "8537d9ef6386cb97156fd47d832f479c";
var botToken = "6128758584:AAFcamyQo9pOnQw9DbQgguEysWK74jkCvmU";
var offset = 0;
var client = new HttpClient();

while (true)
{
    var response = await client.GetAsync($"https://api.telegram.org/bot{botToken}/getUpdates?offset={offset}");

    if (response.IsSuccessStatusCode)
    {
        var modelTelegram = JsonConvert.DeserializeObject<Telegrambot>(response.Content.ReadAsStringAsync().Result);
        foreach (var model in modelTelegram.Result)
        {
            var message = model.Message.Text;
            var chatId = model.Message.Chat.Id;
            var textMessage = string.Empty;

            if (message == "/start")
            {
                textMessage = $"Привет {model.Message.From.FirstName}!" + "\n Напиши /weather чтобы узнать погоду." + "\n\n (напиши потом 'Cпасибо' если понял что как <3)";
                var pushMessage = await client.GetAsync($"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={textMessage}");
            }

            else
            {
                if (message == "/weather")
                {
                    textMessage = "Напиши название города.";
                    var pushMessage = await client.GetAsync($"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={textMessage}");
                }
                else
                {
                    var responseWeather = await client.GetAsync($"https://api.openweathermap.org/data/2.5/weather?q={message}&appid={apiKey}&lang=ru&units=metric");
                    if (responseWeather.IsSuccessStatusCode)
                    {
                        var resultWeather = await responseWeather.Content.ReadAsStringAsync();
                        var modelWeather = JsonConvert.DeserializeObject<WeatherModel>(resultWeather);

                        textMessage = $" Погода в городе {modelWeather.Name}:\n"
                                 + $"\n температура:   {Math.Round(modelWeather.Main.Temp)}°"
                                 + $"\n    ~   {modelWeather.Weather[0].Description}"
                                 + $"\n по ощущениям:   {modelWeather.Main.FeelsLike}°"
                                 + $"\n ветер:   {modelWeather.Wind.Speed} м/с, {Cardinaldirections(modelWeather.Wind.Deg)}"
                                 + $"\n влажность:  {modelWeather.Main.Humidity}%"
                                 + $"\n давление:   {modelWeather.Main.Pressure} мм рт.ст.";

                        var pushMessage = await client.GetAsync($"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={textMessage}");
                    }
                    else
                    {
                        if (message == "Спасибо")
                        {
                            textMessage = $"Обращайся)" +
                                $" \n...напиши /start чтобы снова узнать погоду <3";
                            var pushMessage = await client.GetAsync($"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={textMessage}");
                        }
                        else
                        {
                            textMessage = "Блин.. а не знаю такого...";
                            var pushMessage = await client.GetAsync($"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={textMessage}");
                        }
                    }
                }
            }
        }
        if (modelTelegram.Result.Length > 0)
        {
            offset = modelTelegram.Result[^1].Updateid + 1;
        }

    }
    string Cardinaldirections(int wet) =>
        wet switch
        {
            >= 0 and < 15 or >= 345 and <= 360 => "сев.",
            >= 15 and < 75 => "сев-вост.",
            >= 75 and < 105 => "вост.",
            >= 105 and < 165 => "юго-вост.",
            >= 165 and < 195 => "южный",
            >= 195 and < 255 => "юго-запад.",
            >= 255 and < 285 => "запад.",
            >= 285 and < 345 => "сев-запад.",
            _ => "",
        };
}
