# BookManagement - Sistem Manajemen Toko Buku

Aplikasi web untuk mengelola toko buku online dengan fitur lengkap untuk customer dan admin. Dibangun menggunakan ASP.NET Core MVC 9.0, Entity Framework Core, dan MySQL.

## 📋 Fitur Utama

### Customer
- 📚 Melihat katalog buku yang tersedia (ready stock)
- 🔍 Filter dan pencarian buku
- 🛒 Menambahkan buku ke keranjang belanja
- ✏️ Update jumlah item di keranjang
- 🗑️ Menghapus item dari keranjang
- 💳 Checkout dan pembayaran
- 📦 Melihat riwayat pesanan

### Admin
- 📊 Dashboard dengan statistik penjualan
- 📈 Chart penjualan dan pendapatan per bulan
- 📚 Kelola katalog buku (CRUD)
- 📦 Update stock buku
- 💰 Melihat dan mengelola transaksi
- ✅ Update status pembayaran

## 🔐 Login Credentials

### Admin
- **Email:** `admin@bookmanagement.com`
- **Password:** `admin123`

### Customer
- **Email:** `customer@bookmanagement.com`
- **Password:** `customer123`

## 🛠️ Teknologi yang Digunakan

- **Framework:** ASP.NET Core MVC 9.0
- **Database:** MySQL
- **ORM:** Entity Framework Core 9.0
- **Authentication:** Cookie-based Authentication
- **Password Hashing:** BCrypt.Net-Next
- **Charts:** Chart.js
- **UI Framework:** Bootstrap 5

## 📦 Package Dependencies

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="9.0.0" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.Cookies" Version="2.3.0" />
```

## 🚀 Cara Menjalankan

### Prerequisites
- .NET 9.0 SDK
- MySQL Server
- Visual Studio Code atau Visual Studio 2022

### Langkah-langkah

1. **Clone repository**
   ```bash
   git clone <repository-url>
   cd BookManagement
   ```

2. **Setup Database**
   - Pastikan MySQL server berjalan
   - Update connection string di `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=bookmanagement;User=root;Password=;"
   }
   ```

3. **Apply Migrations**
   ```bash
   dotnet ef database update
   ```

4. **Run Application**
   ```bash
   dotnet run
   ```

5. **Akses Aplikasi**
   - Buka browser: `http://localhost:5096`
   - Login dengan credentials di atas

## 📁 Struktur Project

```
BookManagement/
├── Controllers/
│   ├── AccountController.cs      # Authentication
│   ├── AdminController.cs        # Admin features
│   ├── BookController.cs         # Book catalog (Customer)
│   ├── CartController.cs         # Shopping cart
│   └── CheckoutController.cs     # Checkout & orders
├── Data/
│   ├── ApplicationDbContext.cs   # Database context
│   └── DbSeeder.cs              # Initial data seeder
├── Models/
│   ├── User.cs                  # User model
│   ├── Book.cs                  # Book model
│   ├── Cart.cs                  # Cart model
│   └── Order.cs                 # Order & OrderItem models
├── Views/
│   ├── Account/                 # Login, Register, Logout
│   ├── Admin/                   # Admin dashboard & management
│   ├── Book/                    # Book catalog
│   ├── Cart/                    # Shopping cart
│   └── Checkout/                # Checkout & orders
└── Migrations/                  # EF Core migrations
```

## 🎯 Fitur Keamanan

- ✅ Password di-hash menggunakan BCrypt
- ✅ Role-based Authorization (Admin & Customer)
- ✅ Anti-forgery token untuk form submission
- ✅ Validasi input pada semua form
- ✅ Cookie-based authentication dengan timeout 24 jam

## 📊 Database Schema

### Users
- Id (PK), Email (Unique), PasswordHash, Role, CreatedAt

### Books
- Id (PK), Title, Author, Publisher, PublicationYear, ISBN, Price, Stock, Description, Category, CreatedAt, UpdatedAt

### Carts
- Id (PK), UserId (FK), BookId (FK), Quantity, CreatedAt

### Orders
- Id (PK), UserId (FK), OrderNumber (Unique), TotalAmount, Status, OrderDate, PaidAt

### OrderItems
- Id (PK), OrderId (FK), BookId (FK), Quantity, Price, Subtotal

## 📝 Catatan

- Data sample (users & books) akan otomatis ditambahkan saat aplikasi pertama kali dijalankan
- Stock buku akan otomatis berkurang setelah checkout berhasil
- Admin dapat mengubah status pembayaran dari Pending → Paid → Completed
- Customer hanya dapat melihat buku dengan stock > 0

## 👨‍💻 Developer

Developed by Wahyu Utomo Putra

## 📄 License

This project is for educational purposes.
