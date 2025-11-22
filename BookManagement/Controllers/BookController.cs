using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BookManagement.Data;
using BookManagement.Models;
using System.Security.Claims;

namespace BookManagement.Controllers;

[Authorize(Roles = "customer")]
public class BookController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Book
    public async Task<IActionResult> Index(string? category, string? search)
    {
        var booksQuery = _context.Books.Where(b => b.Stock > 0);

        if (!string.IsNullOrEmpty(category))
        {
            booksQuery = booksQuery.Where(b => b.Category == category);
        }

        if (!string.IsNullOrEmpty(search))
        {
            booksQuery = booksQuery.Where(b => 
                b.Title.Contains(search) || 
                b.Author.Contains(search) ||
                b.Publisher.Contains(search));
        }

        var books = await booksQuery.OrderBy(b => b.Title).ToListAsync();
        
        ViewBag.Categories = await _context.Books
            .Select(b => b.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
        
        ViewBag.CurrentCategory = category;
        ViewBag.SearchTerm = search;

        return View(books);
    }

    // GET: Book/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }

    // POST: Book/AddToCart
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int bookId, int quantity = 1)
    {
        var book = await _context.Books.FindAsync(bookId);

        if (book == null)
        {
            TempData["ErrorMessage"] = "Buku tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        if (book.Stock < quantity)
        {
            TempData["ErrorMessage"] = "Stock tidak mencukupi.";
            return RedirectToAction(nameof(Details), new { id = bookId });
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var existingCart = await _context.Carts
            .FirstOrDefaultAsync(c => c.UserId == userId && c.BookId == bookId);

        if (existingCart != null)
        {
            if (book.Stock < existingCart.Quantity + quantity)
            {
                TempData["ErrorMessage"] = "Stock tidak mencukupi.";
                return RedirectToAction(nameof(Details), new { id = bookId });
            }

            existingCart.Quantity += quantity;
        }
        else
        {
            var cart = new Cart
            {
                UserId = userId,
                BookId = bookId,
                Quantity = quantity,
                CreatedAt = DateTime.UtcNow
            };
            _context.Carts.Add(cart);
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Buku berhasil ditambahkan ke keranjang.";
        return RedirectToAction(nameof(Index));
    }
}
