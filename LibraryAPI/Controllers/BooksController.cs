using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers
{
    public class BooksController : ControllerBase
    {
        private static List<Book> books = new List<Book>
        {
            new Book
            {
                Id = 1,
                Title = "C# Basics",
                Author = "Aman",
                Category = "Programming",
                Price = 500
            },
            new Book
            {
                Id = 2,
                Title = "ASP.NET Core",
                Author = "Riya",
                Category = "Programming",
                Price = 700
            },
            new Book
            {
                Id = 3,
                Title = "Physics",
                Author = "Ankit",
                Category = "Science",
                Price = 400
            },
            new Book 
            { 
                Id = 4, Title = "Advanced C#",
                Author = "Riya",
                Category = "Programming",
                Price = 900 },

            new Book {
                Id = 5, 
                Title = "Chemistry",
                Author = "Aman", 
                Category = "Science",
                Price = 600 }
        };

        // GET: /Books/GetAllBooks
        [HttpGet]
        public IActionResult GetAllBooks()
        {
            return Ok(books);
        }

        // GET: /Books/GetBook/1/Programming
        [HttpGet]
        public IActionResult GetBook(int id, string category)
        {
            var book = books.FirstOrDefault(
                b => b.Id == id && b.Category == category
            );

            if (book == null)
                return NotFound("Book not found");

            return Ok(book);
        }

        // GET: /Books/SearchBooks/?category=Programming&author=Riya
        [HttpGet]
        public IActionResult SearchBooks(string category, string author)
        {
            var result = books.Where(
                b => b.Category == category && b.Author == author
            ).ToList();

            return Ok(result);
        }

        // POST: /Books/AddBook

        [HttpPost]
        public IActionResult AddBook([FromBody] Book book)
        {
            book.Id = books.Count + 1;

            books.Add(book);

            return Ok(book);
        }

        // GET: /Books/FilterBooksByRoute/0/Programming/Riya/500

        [HttpGet]
        public IActionResult FilterBooksByRoute(
            [FromRoute] string? category,
            [FromRoute] string? author,
            [FromRoute] decimal? minPrice)
        {
            var result = books.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                result = result.Where(b => b.Category == category);
            }

            if (!string.IsNullOrEmpty(author))
            {
                result = result.Where(b => b.Author == author);
            }

            if (minPrice.HasValue)
            {
                result = result.Where(b => b.Price >= minPrice.Value);
            }

            return Ok(result.ToList());
        }


        // GET: /Books/FilterBooksByQuery?category=Programming&author=Riya&minPrice=500

        [HttpGet]
        public IActionResult FilterBooksByQuery(
            [FromQuery] string? category,
            [FromQuery] string? author,
            [FromQuery] decimal? minPrice)
        {
            var result = books.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                result = result.Where(b => b.Category == category);
            }

            if (!string.IsNullOrEmpty(author))
            {
                result = result.Where(b => b.Author == author);
            }

            if (minPrice.HasValue)
            {
                result = result.Where(b => b.Price >= minPrice.Value);
            }

            return Ok(result.ToList());
        }
    }
}