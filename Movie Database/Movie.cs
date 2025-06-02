using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movie_Database;

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
