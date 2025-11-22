namespace BookManagement.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Paid, Completed, Cancelled
    public DateTime OrderDate { get; set; }
    public DateTime? PaidAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int BookId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
    public Book Book { get; set; } = null!;
}
