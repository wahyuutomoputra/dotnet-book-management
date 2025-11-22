using BookManagement.Data;
using BookManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if users already exist
        if (!await context.Users.AnyAsync())
        {
            // Create admin user
            var admin = new User
            {
                Email = "admin@bookmanagement.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "admin",
                CreatedAt = DateTime.UtcNow
            };

            // Create customer user
            var customer = new User
            {
                Email = "customer@bookmanagement.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("customer123"),
                Role = "customer",
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(admin, customer);
            await context.SaveChangesAsync();
        }

        // Check if books already exist
        if (!await context.Books.AnyAsync())
        {
            var books = new List<Book>
            {
                new Book
                {
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    Publisher = "Prentice Hall",
                    PublicationYear = 2008,
                    ISBN = "9780132350884",
                    Price = 350000,
                    Stock = 15,
                    Description = "A Handbook of Agile Software Craftsmanship",
                    Category = "Programming",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Title = "The Pragmatic Programmer",
                    Author = "David Thomas, Andrew Hunt",
                    Publisher = "Addison-Wesley",
                    PublicationYear = 2019,
                    ISBN = "9780135957059",
                    Price = 400000,
                    Stock = 10,
                    Description = "Your Journey to Mastery",
                    Category = "Programming",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Title = "Design Patterns",
                    Author = "Gang of Four",
                    Publisher = "Addison-Wesley",
                    PublicationYear = 1994,
                    ISBN = "9780201633610",
                    Price = 450000,
                    Stock = 8,
                    Description = "Elements of Reusable Object-Oriented Software",
                    Category = "Programming",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Title = "Introduction to Algorithms",
                    Author = "Thomas H. Cormen",
                    Publisher = "MIT Press",
                    PublicationYear = 2009,
                    ISBN = "9780262033848",
                    Price = 500000,
                    Stock = 12,
                    Description = "A comprehensive textbook on algorithms",
                    Category = "Computer Science",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Title = "You Don't Know JS",
                    Author = "Kyle Simpson",
                    Publisher = "O'Reilly Media",
                    PublicationYear = 2015,
                    ISBN = "9781491904244",
                    Price = 250000,
                    Stock = 20,
                    Description = "Up & Going - JavaScript basics",
                    Category = "Web Development",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Title = "Eloquent JavaScript",
                    Author = "Marijn Haverbeke",
                    Publisher = "No Starch Press",
                    PublicationYear = 2018,
                    ISBN = "9781593279509",
                    Price = 280000,
                    Stock = 0,
                    Description = "A Modern Introduction to Programming",
                    Category = "Web Development",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.Books.AddRange(books);
            await context.SaveChangesAsync();
        }
    }
}
