using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BookManagement.Data;
using BookManagement.Models;

namespace BookManagement.Controllers;

[Authorize(Roles = "admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
        var firstDayOfYear = new DateTime(now.Year, 1, 1);

        // Total pendapatan bulan ini
        var monthlyRevenue = await _context.Orders
            .Where(o => o.OrderDate >= firstDayOfMonth && o.Status != "Cancelled")
            .SumAsync(o => o.TotalAmount);

        // Total buku terjual bulan ini
        var monthlyBooksSold = await _context.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.OrderDate >= firstDayOfMonth && oi.Order.Status != "Cancelled")
            .SumAsync(oi => oi.Quantity);

        // Total transaksi bulan ini
        var monthlyTransactions = await _context.Orders
            .CountAsync(o => o.OrderDate >= firstDayOfMonth);

        // Total buku dalam katalog
        var totalBooks = await _context.Books.CountAsync();

        // Data penjualan per bulan (12 bulan terakhir)
        var salesData = new List<MonthlyData>();
        var revenueData = new List<MonthlyData>();

        for (int i = 11; i >= 0; i--)
        {
            var monthStart = now.AddMonths(-i).Date;
            var monthStartUtc = new DateTime(monthStart.Year, monthStart.Month, 1);
            var monthEnd = monthStartUtc.AddMonths(1);

            var monthlySales = await _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.OrderDate >= monthStartUtc && 
                            oi.Order.OrderDate < monthEnd && 
                            oi.Order.Status != "Cancelled")
                .SumAsync(oi => oi.Quantity);

            var monthlyRev = await _context.Orders
                .Where(o => o.OrderDate >= monthStartUtc && 
                           o.OrderDate < monthEnd && 
                           o.Status != "Cancelled")
                .SumAsync(o => o.TotalAmount);

            salesData.Add(new MonthlyData 
            { 
                Month = monthStartUtc.ToString("MMM yyyy"), 
                Value = monthlySales 
            });

            revenueData.Add(new MonthlyData 
            { 
                Month = monthStartUtc.ToString("MMM yyyy"), 
                Value = (int)monthlyRev 
            });
        }

        var viewModel = new DashboardViewModel
        {
            MonthlyRevenue = monthlyRevenue,
            MonthlyBooksSold = monthlyBooksSold,
            MonthlyTransactions = monthlyTransactions,
            TotalBooks = totalBooks,
            SalesData = salesData,
            RevenueData = revenueData
        };

        return View(viewModel);
    }

    // GET: Admin/Books
    public async Task<IActionResult> Books(string? search, string? category)
    {
        var booksQuery = _context.Books.AsQueryable();

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

    // GET: Admin/BookDetails/5
    public async Task<IActionResult> BookDetails(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }

    // GET: Admin/CreateBook
    public IActionResult CreateBook()
    {
        return View();
    }

    // POST: Admin/CreateBook
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBook(Book book)
    {
        if (string.IsNullOrEmpty(book.Title) || string.IsNullOrEmpty(book.Author))
        {
            TempData["ErrorMessage"] = "Judul dan Author harus diisi.";
            return View(book);
        }

        book.CreatedAt = DateTime.UtcNow;
        book.UpdatedAt = DateTime.UtcNow;

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Buku berhasil ditambahkan.";
        return RedirectToAction(nameof(Books));
    }

    // GET: Admin/EditBook/5
    public async Task<IActionResult> EditBook(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }

    // POST: Admin/EditBook/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBook(int id, Book book)
    {
        if (id != book.Id)
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(book.Title) || string.IsNullOrEmpty(book.Author))
        {
            TempData["ErrorMessage"] = "Judul dan Author harus diisi.";
            return View(book);
        }

        var existingBook = await _context.Books.FindAsync(id);
        if (existingBook == null)
        {
            return NotFound();
        }

        existingBook.Title = book.Title;
        existingBook.Author = book.Author;
        existingBook.Publisher = book.Publisher;
        existingBook.PublicationYear = book.PublicationYear;
        existingBook.ISBN = book.ISBN;
        existingBook.Price = book.Price;
        existingBook.Stock = book.Stock;
        existingBook.Description = book.Description;
        existingBook.Category = book.Category;
        existingBook.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Buku berhasil diupdate.";
        return RedirectToAction(nameof(Books));
    }

    // POST: Admin/DeleteBook/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            TempData["ErrorMessage"] = "Buku tidak ditemukan.";
            return RedirectToAction(nameof(Books));
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Buku berhasil dihapus.";
        return RedirectToAction(nameof(Books));
    }

    // POST: Admin/UpdateStock
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStock(int id, int stock)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            TempData["ErrorMessage"] = "Buku tidak ditemukan.";
            return RedirectToAction(nameof(Books));
        }

        if (stock < 0)
        {
            TempData["ErrorMessage"] = "Stock tidak boleh negatif.";
            return RedirectToAction(nameof(BookDetails), new { id });
        }

        book.Stock = stock;
        book.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Stock berhasil diupdate.";
        return RedirectToAction(nameof(BookDetails), new { id });
    }

    // GET: Admin/Transactions
    public async Task<IActionResult> Transactions(string? status, string? search)
    {
        var ordersQuery = _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Book)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            ordersQuery = ordersQuery.Where(o => o.Status == status);
        }

        if (!string.IsNullOrEmpty(search))
        {
            ordersQuery = ordersQuery.Where(o => 
                o.OrderNumber.Contains(search) ||
                o.User.Email.Contains(search));
        }

        var orders = await ordersQuery
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        ViewBag.Statuses = new[] { "Pending", "Paid", "Completed", "Cancelled" };
        ViewBag.CurrentStatus = status;
        ViewBag.SearchTerm = search;

        return View(orders);
    }

    // GET: Admin/TransactionDetails/5
    public async Task<IActionResult> TransactionDetails(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Book)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    // POST: Admin/UpdateOrderStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int id, string status)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
        {
            TempData["ErrorMessage"] = "Pesanan tidak ditemukan.";
            return RedirectToAction(nameof(Transactions));
        }

        var validStatuses = new[] { "Pending", "Paid", "Completed", "Cancelled" };
        if (!validStatuses.Contains(status))
        {
            TempData["ErrorMessage"] = "Status tidak valid.";
            return RedirectToAction(nameof(TransactionDetails), new { id });
        }

        order.Status = status;
        
        if (status == "Paid" && order.PaidAt == null)
        {
            order.PaidAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Status pesanan berhasil diupdate.";
        return RedirectToAction(nameof(TransactionDetails), new { id });
    }
}
