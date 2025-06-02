# Movie Database (C# + Kinopoisk API Unofficial)

Приложение для поиска фильмов по названию и сохранения их в локальную базу данных.

## 🔧 Технологии
- C# (.NET 6)
- Entity Framework Core
- Kinopoisk API Unofficial

## Скриншоты  работы

![image](https://github.com/user-attachments/assets/f294c246-e4ed-4a89-9ac1-3a08956f596a)


## Скриншоты бд

![image](https://github.com/user-attachments/assets/5a5765b7-2700-4b60-8f0a-a6370f47d879)


  
# Код 
## Main.cs
```csharp
 internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Введите ключевое слово, которое встречается в названии фильма");
        string keyword = Console.ReadLine();
        string page = "1";
        MovieJSON result;
        var client = new HttpClient();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            WriteIndented = true
        };

        string jsonAPI = File.ReadAllText("appsettings.json");
        var getAPI = JsonSerializer.Deserialize<GetAPI>(jsonAPI, options);
        while (true)
        {

           

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://kinopoiskapiunofficial.tech/api/v2.2/films?order=RATING&type=FILM&ratingFrom=0&ratingTo=10&yearFrom=1000&yearTo=3000&keyword={keyword}&page={page}"),
                Headers =
        {
            { "X-API-KEY", $"{getAPI.ApiKey}" },
            { "Accept", "application/json" },
        },
            };

            using (var response = await client.SendAsync(request))
            {

                var json = await response.Content.ReadAsStringAsync();
                result = JsonSerializer.Deserialize<MovieJSON>(json, options);
            }

            Console.WriteLine($"\nВсего страниц {result.TotalPages} Вы на { page} страницы\n");
            foreach (var item in result.Items)
            {
                Console.WriteLine(item.NameRu);
            }

            Console.WriteLine("\n1:Сохранить данные о фильмах в бд\n" +
                              "2:Выбрать другую страницу\n" +
                              "3:Изменить ключевое слово\n" +
                              "4:Выход\n");
            var choise = Console.ReadLine();

            switch (choise)
            {
                case "1":
                    await SaveMoviesToDatabase(result);
                    break;

                case "2":
                    Console.Write("Введите страницу:");
                    page = Console.ReadLine();
                    break;

                case "3":
                    Console.Write("Введите страницу: ");
                    keyword = Console.ReadLine();
                    break;

                default:
                    return;
                    
            }

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
    public int TotalPages { get; set; }
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

public class GetAPI
{
    public string ApiKey { get; set; }
}
```
## Movie.cs класс для взаимодействия с бд через Entity framework
```csharp

public class Movie
{
    public int id { get; set; }
    public int kinopoisk_id { get; set; }
    public string name_ru { get; set; }
    public int? year { get; set; }
    public double? rating_imdb { get; set; }

    public ICollection<Genre> Genres { get; set; } = new List<Genre>();
    public ICollection<Country> Countries { get; set; } = new List<Country>();
}

public class Genre
{
    public int id { get; set; }
    public string name { get; set; }

    public ICollection<Movie> Movies { get; set; }
}
public class Country
{
    public int id { get; set; }
    public string name { get; set; }

    public ICollection<Movie> Movies { get; set; }
}
```

## MoviebaseContext.cs класс для подключение к бд
```csharp
public class AppDbContext : DbContext
{
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Country> Countries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=moviebase;Username=postgres;Password=1");
      
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Movie>()
            .ToTable("movies");
           
        modelBuilder.Entity<Genre>()
           .ToTable("genres");
        modelBuilder.Entity<Country>()
          .ToTable("countries");

        modelBuilder.Entity<Movie>()
            .HasMany(m => m.Genres)
            .WithMany(g => g.Movies)
            .UsingEntity<Dictionary<string, object>>(
            "movie_genres",
            j => j.HasOne<Genre>().WithMany().HasForeignKey("genre_id"),
            j => j.HasOne<Movie>().WithMany().HasForeignKey("movie_id"));


        modelBuilder.Entity<Movie>()
            .HasMany(m => m.Countries)
            .WithMany(c => c.Movies)
            .UsingEntity<Dictionary<string, object>>(
            "movie_countries",
            j => j.HasOne<Country>().WithMany().HasForeignKey("country_id"),
            j => j.HasOne<Movie>().WithMany().HasForeignKey("movie_id"));
    }
}
```
## appsetings.json там хранится API
```json
{
  "ApiKey": "58157c64-2c1a-43c5-a66a-fe35ec7ef2c7"
}
```
