using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BookManagement.Data;
using BookManagement.Models;
using System.Security.Claims;

namespace BookManagement.Controllers;

[Authorize(Roles = "customer")]
public class CheckoutController : Controller
{
    private readonly ApplicationDbContext _context;

    public CheckoutController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Checkout
    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var cartItems = await _context.Carts
            .Include(c => c.Book)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
        {
            TempData["ErrorMessage"] = "Keranjang kosong.";
            return RedirectToAction("Index", "Cart");
        }

        // Check stock availability
        foreach (var item in cartItems)
        {
            if (item.Book.Stock < item.Quantity)
            {
                TempData["ErrorMessage"] = $"Stock {item.Book.Title} tidak mencukupi.";
                return RedirectToAction("Index", "Cart");
            }
        }

        var total = cartItems.Sum(c => c.Book.Price * c.Quantity);
        ViewBag.Total = total;
        ViewBag.CartItems = cartItems;

        return View();
    }

    // POST: Checkout/Process
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Process()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var cartItems = await _context.Carts
            .Include(c => c.Book)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
        {
            TempData["ErrorMessage"] = "Keranjang kosong.";
            return RedirectToAction("Index", "Cart");
        }

        // Check stock availability again
        foreach (var item in cartItems)
        {
            if (item.Book.Stock < item.Quantity)
            {
                TempData["ErrorMessage"] = $"Stock {item.Book.Title} tidak mencukupi.";
                return RedirectToAction("Index", "Cart");
            }
        }

        // Create order
        var orderNumber = GenerateOrderNumber();
        var totalAmount = cartItems.Sum(c => c.Book.Price * c.Quantity);

        var order = new Order
        {
            UserId = userId,
            OrderNumber = orderNumber,
            TotalAmount = totalAmount,
            Status = "Pending",
            OrderDate = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Create order items and update stock
        foreach (var item in cartItems)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                BookId = item.BookId,
                Quantity = item.Quantity,
                Price = item.Book.Price,
                Subtotal = item.Book.Price * item.Quantity
            };

            _context.OrderItems.Add(orderItem);

            // Update book stock
            item.Book.Stock -= item.Quantity;
            item.Book.UpdatedAt = DateTime.UtcNow;
        }

        // Clear cart
        _context.Carts.RemoveRange(cartItems);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Pesanan berhasil dibuat dengan nomor: {orderNumber}";
        return RedirectToAction("Success", new { orderNumber });
    }

    // GET: Checkout/Success
    public async Task<IActionResult> Success(string orderNumber)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Book)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.UserId == userId);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    // GET: Checkout/MyOrders
    public async Task<IActionResult> MyOrders()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    // GET: Checkout/OrderDetails
    public async Task<IActionResult> OrderDetails(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Book)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";
    }
}
