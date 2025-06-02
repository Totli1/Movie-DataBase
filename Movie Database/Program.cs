using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Movie_Database
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            string keyword = Console.ReadLine();
            MovieJSON result;
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://kinopoiskapiunofficial.tech/api/v2.2/films?order=RATING&type=FILM&ratingFrom=0&ratingTo=10&yearFrom=1000&yearTo=3000&keyword={keyword}&page=1"),
                Headers =
            {
                { "X-API-KEY", "58157c64-2c1a-43c5-a66a-fe35ec7ef2c7" },
                { "Accept", "application/json" },
            },
            };
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
                WriteIndented = true
            };
            using (var response = await client.SendAsync(request))
            {

                var json = await response.Content.ReadAsStringAsync();
                result = JsonSerializer.Deserialize<MovieJSON>(json, options);
             

            }
            foreach (var item in result.Items)
            {
                Console.WriteLine(item.NameRu);
            }
            Console.WriteLine("Сохранить данные о фильме в бд?\n1:Да");
            var choise = Console.ReadLine();
            if (choise == "1")
            {
            await SaveMoviesToDatabase(result);
            }


        }

        public static async Task SaveMoviesToDatabase(MovieJSON movieData)
        {
            using var context = new AppDbContext();

            foreach (var item in movieData.Items)
            {
               
                var existingMovie = await context.Movies
                    .FirstOrDefaultAsync(m => m.kinopoisk_id == item.KinopoiskId);

                if (existingMovie != null) continue; 

                var movie = new Movie
                {
                    kinopoisk_id = item.KinopoiskId,
                    name_ru = item.NameRu,
                    year = item.Year,
                    rating_imdb = item.RatingImdb
                };

               
                foreach (var genre in item.Genres)
                {
                    var existingGenre = await context.Genres
                        .FirstOrDefaultAsync(g => g.name == genre.Genre);

                    if (existingGenre != null)
                    {
                        movie.Genres.Add(existingGenre);
                    }
                    else
                    {
                        movie.Genres.Add(new Genre { name = genre.Genre });
                    }
                }

             
                foreach (var country in item.Countries)
                {
                    var existingCountry = await context.Countries
                        .FirstOrDefaultAsync(c => c.name == country.Country);

                    if (existingCountry != null)
                    {
                        movie.Countries.Add(existingCountry);
                    }
                    else
                    {
                        movie.Countries.Add(new Country { name = country.Country });
                    }
                }

                context.Movies.Add(movie);
            }

            await context.SaveChangesAsync();
        }
        


    }
    public class MovieJSON
    {
        public List<Item> Items { get; set; }
    }
    public class Item
    {
        public int KinopoiskId { get; set; }

        public string? NameRu { get; set; }
        public int? Year { get; set; }
        public double? RatingImdb { get; set; }
        public List<Genres> Genres { get; set; }
        public List<CountryMovieJSON> Countries { get; set; }
    }

    public class CountryMovieJSON
    {
        public string Country { get; set; }
    }
    public class Genres
    {
        public string Genre { get; set; }
    }
}
