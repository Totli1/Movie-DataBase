using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Movie_Database;

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
