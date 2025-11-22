using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BookManagement.Data;
using BookManagement.Models;
using System.Security.Claims;

namespace BookManagement.Controllers;

[Authorize(Roles = "customer")]
public class CartController : Controller
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Cart
    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var cartItems = await _context.Carts
            .Include(c => c.Book)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        var total = cartItems.Sum(c => c.Book.Price * c.Quantity);
        ViewBag.Total = total;

        return View(cartItems);
    }

    // POST: Cart/Remove
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var cartItem = await _context.Carts
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (cartItem == null)
        {
            TempData["ErrorMessage"] = "Item tidak ditemukan di keranjang.";
            return RedirectToAction(nameof(Index));
        }

        _context.Carts.Remove(cartItem);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Item berhasil dihapus dari keranjang.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Cart/UpdateQuantity
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int id, int quantity)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var cartItem = await _context.Carts
            .Include(c => c.Book)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (cartItem == null)
        {
            TempData["ErrorMessage"] = "Item tidak ditemukan di keranjang.";
            return RedirectToAction(nameof(Index));
        }

        if (quantity < 1)
        {
            TempData["ErrorMessage"] = "Jumlah minimal 1.";
            return RedirectToAction(nameof(Index));
        }

        if (quantity > cartItem.Book.Stock)
        {
            TempData["ErrorMessage"] = "Stock tidak mencukupi.";
            return RedirectToAction(nameof(Index));
        }

        cartItem.Quantity = quantity;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Jumlah berhasil diupdate.";
        return RedirectToAction(nameof(Index));
    }
}
