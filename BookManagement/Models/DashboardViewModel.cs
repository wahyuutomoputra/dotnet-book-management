namespace BookManagement.Models;

public class DashboardViewModel
{
    public decimal MonthlyRevenue { get; set; }
    public int MonthlyBooksSold { get; set; }
    public int MonthlyTransactions { get; set; }
    public int TotalBooks { get; set; }
    public List<MonthlyData> SalesData { get; set; } = new();
    public List<MonthlyData> RevenueData { get; set; } = new();
}

public class MonthlyData
{
    public string Month { get; set; } = string.Empty;
    public int Value { get; set; }
}
