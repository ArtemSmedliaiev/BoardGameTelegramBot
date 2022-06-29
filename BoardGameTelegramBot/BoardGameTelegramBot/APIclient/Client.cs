using BoardGameTelegramBot.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BoardGameTelegramBot.Models;

namespace BoardGameTelegramBot.APIclient
{
    internal class Client
    {
        private HttpClient _client;
        private static string _adress;        
        public Client()
        {
            _adress = constants.adress;           
            _client = new HttpClient();
            _client.BaseAddress = new Uri(_adress);
        }

        public async Task<List<string>> GetGamesNamesAsync(string name)
        {
            var responce = await _client.GetAsync($"/GameInfo?Game={name}");
            var content = responce.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<GamesInfo>(content);
            List<string> names = new List<string>();
            foreach(var item in result.games)
            {
                string[] _name = item.name.Split(" ");
                if (_name.Length > 7)
                {
                    item.name = String.Join(" ", _name[..7]) + "...";
                    names.Add(item.name);
                }
                else
                {
                    names.Add(item.name);
                }
                
            }
            return names;
        }

        public async Task<GamesInfo> GetGamesAsync(string gamename)
        {
            var responce = await _client.GetAsync($"/GameInfo?Game={gamename}");
            var content = responce.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<GamesInfo>(content);
            return result;
        }

        public async Task<GamesInfo> GetGamesOnCategorieAsync(string categoriesname)
        {
            string categories = GetCategorieToIdAsync(categoriesname).Result;
            var responce = await _client.GetAsync($"/GamesCategories?Categories={categories}");
            var content = responce.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<GamesInfo>(content);
            return result;
        }
 
        public async Task<GamesInfo> GetGamesOnMechanicsAsync(string mechanicsname)
        {
            string mechanics = GetMechanicToIdAsync(mechanicsname).Result;
            var responce = await _client.GetAsync($"/GamesMechanics?Mechanics={mechanics}");
            var content = responce.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<GamesInfo>(content);
            return result;
        }

        public async Task<CategoriesInfo> GetCategoriesAsync()
        {
            var responce = await _client.GetAsync($"/Categories");
            var content = responce.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<CategoriesInfo>(content);
            return result;
        }

        public async Task<string> GetCategorieToIdAsync(string categoriename)
        {
            CategoriesInfo result = GetCategoriesAsync().Result;
            categoriename = categoriename.Replace("/", "");
            foreach (CategorieInfo Categorie in result.categories)
            {
                if (Categorie.name.Replace(" ", "").Replace("-", "").Replace("/", "").Replace("'", "").Replace("&", "").Replace(" ", "") == categoriename)
                {
                    return Categorie.id;
                }
            }
            return null;
        }

        public async Task<MechanicsInfo> GetMechanicsAsync()
        {
            var responce = await _client.GetAsync($"/Mechanics");
            var content = responce.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<MechanicsInfo>(content);
            return result;
        }

        public async Task<string> GetMechanicToIdAsync(string mechanicname)
        {
            MechanicsInfo result = GetMechanicsAsync().Result;
            mechanicname = mechanicname.Replace("/", "");
            foreach (MechanicInfo Mechanic in result.mechanics)
            {
                if (Mechanic.name.Replace(" ", "").Replace("-", "").Replace("/", "").Replace("'", "").Replace("&", "").Replace(",", "").Replace("(", "").Replace(")", "").Replace(":", "") == mechanicname)
                {
                    return Mechanic.id;
                }
            }
            return null;
        }

        public async Task<List<string>> GetGamesNamesOnYearAsync(string? year)
        {
            var result = GetGamesOnYearAsync(year).Result;
            List<string> names = new List<string>();
            foreach (var item in result.games)
            {
                names.Add(item.name);
            }
            return names;
        }

        public async Task<GamesInfo> GetGamesOnYearAsync(string? year)
        {
            var responce = await _client.GetAsync($"/GamesYear?Year={year}");
            var content = responce.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<GamesInfo>(content);
            return result;
        }

        public async Task<BoardGame> GetRandomGameAsync()
        {           
            var responce = await _client.GetAsync($"/RandomGame");
            var content = responce.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<GamesInfo>(content);            
            return result.games[0];
        }

        public async Task<BoardGame> GetGameAsync(string _name)
        {            
            var result = GetGamesAsync(_name).Result;
            BoardGame resultgame = result.games[0];
            return resultgame;
        }

    }
}
