using BaseCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.Repository
{
    /// <summary>
    /// Entity Framework Core DbContext backed by SQL Server.
    /// </summary>
    public class BaseCoreDbContext : DbContext
    {
        public BaseCoreDbContext(DbContextOptions<BaseCoreDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<CarModel> CarModels => Set<CarModel>();
        public DbSet<Showroom> Showrooms => Set<Showroom>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<Favorite> Favorites => Set<Favorite>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Voucher> Vouchers => Set<Voucher>();
        public DbSet<OrderVoucher> OrderVouchers => Set<OrderVoucher>();
        public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
        public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
        public DbSet<Faq> Faqs => Set<Faq>();
        public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureUsers(modelBuilder);
            ConfigureCatalog(modelBuilder);
            ConfigureShopping(modelBuilder);
            ConfigureContent(modelBuilder);
            SeedData(modelBuilder);
        }

        private static void ConfigureUsers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("NGUOIDUNG");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaNguoiDung");
                entity.Property(e => e.Name).HasColumnName("HoTen").HasMaxLength(150).IsRequired();
                entity.Property(e => e.Email).HasColumnName("Email").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Phone).HasColumnName("SoDienThoai").HasMaxLength(20).IsRequired();
                entity.Property(e => e.Password).HasColumnName("MatKhauHash").HasMaxLength(500).IsRequired();
                entity.Property(e => e.IsActive)
                    .HasColumnName("TrangThai")
                    .HasMaxLength(20)
                    .HasConversion(v => v ? "Active" : "Inactive", v => v == "Active");
                entity.Property(e => e.Created).HasColumnName("NgayTao");
                entity.Ignore(e => e.UserName);
                entity.Ignore(e => e.Salt);
                entity.Ignore(e => e.Contact);
                entity.Ignore(e => e.Position);
                entity.Ignore(e => e.Image);
                entity.Ignore(e => e.UserType);
                entity.HasIndex(e => e.Email).IsUnique();
            });
        }

        private static void ConfigureCatalog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("DANHMUC");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaDanhMuc");
                entity.Property(e => e.ParentCategoryId).HasColumnName("MaDanhMucCha");
                entity.Property(e => e.Name).HasColumnName("TenDanhMuc").HasMaxLength(150).IsRequired();
                entity.Property(e => e.Slug).HasColumnName("Slug").HasMaxLength(180).IsRequired();
                entity.Property(e => e.Description).HasColumnName("MoTa").HasMaxLength(500);
                entity.Property(e => e.SortOrder).HasColumnName("ThuTuHienThi");
                entity.Property(e => e.IsActive).HasColumnName("DangHoatDong");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => new { e.ParentCategoryId, e.IsActive });
                entity.HasOne(e => e.ParentCategory)
                    .WithMany()
                    .HasForeignKey(e => e.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Brand>(entity =>
            {
                entity.ToTable("HANGXE");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaHangXe");
                entity.Property(e => e.Name).HasColumnName("TenHang").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Slug).HasColumnName("Slug").HasMaxLength(150).IsRequired();
                entity.Property(e => e.LogoUrl).HasColumnName("LogoUrl").HasMaxLength(500);
                entity.Property(e => e.IsActive).HasColumnName("DangHoatDong");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.Slug).IsUnique();
            });

            modelBuilder.Entity<CarModel>(entity =>
            {
                entity.ToTable("DONGXE");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaDongXe");
                entity.Property(e => e.BrandId).HasColumnName("MaHangXe");
                entity.Property(e => e.Name).HasColumnName("TenDongXe").HasMaxLength(120).IsRequired();
                entity.Property(e => e.Slug).HasColumnName("Slug").HasMaxLength(160).IsRequired();
                entity.Property(e => e.IsActive).HasColumnName("DangHoatDong");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => new { e.BrandId, e.Name }).IsUnique();
                entity.HasOne(e => e.Brand)
                    .WithMany()
                    .HasForeignKey(e => e.BrandId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Showroom>(entity =>
            {
                entity.ToTable("SHOWROOM");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaShowroom");
                entity.Property(e => e.Name).HasColumnName("TenShowroom").HasMaxLength(180).IsRequired();
                entity.Property(e => e.Slug).HasColumnName("Slug").HasMaxLength(220).IsRequired();
                entity.Property(e => e.AddressLine).HasColumnName("DiaChi").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Ward).HasColumnName("PhuongXa").HasMaxLength(100);
                entity.Property(e => e.District).HasColumnName("QuanHuyen").HasMaxLength(100);
                entity.Property(e => e.Province).HasColumnName("TinhThanh").HasMaxLength(100).IsRequired();
                entity.Property(e => e.PhoneNumber).HasColumnName("SoDienThoai").HasMaxLength(20);
                entity.Property(e => e.Email).HasColumnName("Email").HasMaxLength(255);
                entity.Property(e => e.OpeningHours).HasColumnName("GioMoCua").HasMaxLength(255);
                entity.Property(e => e.IsActive).HasColumnName("DangHoatDong");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => new { e.Province, e.District, e.IsActive });
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("SANPHAM");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaSanPham");
                entity.Property(e => e.ProductCode).HasColumnName("MaSanPhamKinhDoanh").HasMaxLength(50).IsRequired();
                entity.Property(e => e.Name).HasColumnName("TenSanPham").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Slug).HasColumnName("Slug").HasMaxLength(280).IsRequired();
                entity.Property(e => e.CategoryId).HasColumnName("MaDanhMuc");
                entity.Property(e => e.BrandId).HasColumnName("MaHangXe");
                entity.Property(e => e.CarModelId).HasColumnName("MaDongXe");
                entity.Property(e => e.ShowroomId).HasColumnName("MaShowroom");
                entity.Property(e => e.ProductType).HasColumnName("LoaiSanPham").HasMaxLength(20).IsRequired();
                entity.Property(e => e.ShortDescription).HasColumnName("MoTaNgan").HasMaxLength(500);
                entity.Property(e => e.Description).HasColumnName("MoTa");
                entity.Property(e => e.BasePrice).HasColumnName("GiaGoc").HasPrecision(18, 2);
                entity.Property(e => e.SalePrice).HasColumnName("GiaKhuyenMai").HasPrecision(18, 2);
                entity.Property(e => e.StockQuantity).HasColumnName("SoLuongTon");
                entity.Property(e => e.MainImageUrl).HasColumnName("AnhChinhUrl").HasMaxLength(500);
                entity.Property(e => e.IsActive).HasColumnName("DangHoatDong");
                entity.Property(e => e.Condition).HasColumnName("TinhTrangXe").HasMaxLength(20);
                entity.Property(e => e.Year).HasColumnName("NamSanXuat").HasConversion<short?>();
                entity.Property(e => e.Mileage).HasColumnName("SoKm");
                entity.Property(e => e.ExteriorColor).HasColumnName("MauNgoaiThat").HasMaxLength(80);
                entity.Property(e => e.InteriorColor).HasColumnName("MauNoiThat").HasMaxLength(80);
                entity.Property(e => e.Seats).HasColumnName("SoChoNgoi").HasConversion<byte?>();
                entity.Property(e => e.Transmission).HasColumnName("HopSo").HasMaxLength(30);
                entity.Property(e => e.FuelType).HasColumnName("NhienLieu").HasMaxLength(30);
                entity.Property(e => e.Engine).HasColumnName("DongCo").HasMaxLength(100);
                entity.Property(e => e.DriveType).HasColumnName("DanDong").HasMaxLength(30);
                entity.Property(e => e.Vin).HasColumnName("VIN").HasMaxLength(50);
                entity.Property(e => e.LicensePlate).HasColumnName("BienSo").HasMaxLength(30);
                entity.Property(e => e.Status).HasColumnName("TrangThaiSanPham").HasMaxLength(20).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.HasIndex(e => e.ProductCode).IsUnique();
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => new { e.CategoryId, e.ProductType, e.IsActive, e.Status });
                entity.HasIndex(e => new { e.BrandId, e.CarModelId, e.IsActive });
                entity.HasIndex(e => new { e.Condition, e.Year, e.FuelType, e.Transmission, e.ExteriorColor, e.ShowroomId });
                entity.HasOne(e => e.Category)
                    .WithMany()
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Brand)
                    .WithMany()
                    .HasForeignKey(e => e.BrandId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.CarModel)
                    .WithMany()
                    .HasForeignKey(e => e.CarModelId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Showroom)
                    .WithMany()
                    .HasForeignKey(e => e.ShowroomId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.Variants)
                    .WithOne()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Images)
                    .WithOne()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.ToTable("BIENSANPHAM");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaBienSanPham");
                entity.Property(e => e.ProductId).HasColumnName("MaSanPham");
                entity.Property(e => e.VariantName).HasColumnName("TenBienThe").HasMaxLength(180).IsRequired();
                entity.Property(e => e.Sku).HasColumnName("SKU").HasMaxLength(80).IsRequired();
                entity.Property(e => e.PriceOverride).HasColumnName("GiaGhiDe").HasPrecision(18, 2);
                entity.Property(e => e.StockQuantity).HasColumnName("SoLuongTon");
                entity.Property(e => e.Status).HasColumnName("TrangThai").HasMaxLength(20).IsRequired();
                entity.Property(e => e.Version).HasColumnName("PhienBan").HasMaxLength(100);
                entity.Property(e => e.ExteriorColor).HasColumnName("MauNgoaiThat").HasMaxLength(80);
                entity.Property(e => e.InteriorColor).HasColumnName("MauNoiThat").HasMaxLength(80);
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.HasIndex(e => e.Sku).IsUnique();
                entity.HasIndex(e => new { e.ProductId, e.Status });
            });

            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.ToTable("ANHSANPHAM");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaAnhSanPham");
                entity.Property(e => e.ProductId).HasColumnName("MaSanPham");
                entity.Property(e => e.ImageUrl).HasColumnName("UrlAnh").HasMaxLength(500).IsRequired();
                entity.Property(e => e.AltText).HasColumnName("AltText").HasMaxLength(255);
                entity.Property(e => e.IsPrimary).HasColumnName("LaAnhChinh");
                entity.Property(e => e.SortOrder).HasColumnName("ThuTuHienThi");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.HasIndex(e => new { e.ProductId, e.SortOrder });
                entity.HasIndex(e => e.ProductId).IsUnique().HasFilter("[LaAnhChinh] = 1");
            });
        }

        private static void ConfigureShopping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.ToTable("YEUTHICH");
                entity.HasKey(e => new { e.UserId, e.ProductId });
                entity.Property(e => e.UserId).HasColumnName("MaNguoiDung");
                entity.Property(e => e.ProductId).HasColumnName("MaSanPham");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("GIOHANG");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaGioHang");
                entity.Property(e => e.UserId).HasColumnName("MaNguoiDung").IsRequired();
                entity.Property(e => e.Status).HasColumnName("TrangThai").HasMaxLength(20).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.HasIndex(e => e.UserId).IsUnique().HasFilter("[TrangThai] = 'Active'");
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("CHITIET_GIOHANG");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaChiTietGioHang");
                entity.Property(e => e.CartId).HasColumnName("MaGioHang");
                entity.Property(e => e.ProductId).HasColumnName("MaSanPham");
                entity.Property(e => e.ProductVariantId).HasColumnName("MaBienSanPham");
                entity.Property(e => e.Quantity).HasColumnName("SoLuong");
                entity.Property(e => e.UnitPrice).HasColumnName("DonGia").HasPrecision(18, 2);
                entity.Property(e => e.LineTotal).HasColumnName("ThanhTien").HasPrecision(18, 2).HasComputedColumnSql("CONVERT([decimal](18,2),[DonGia]*[SoLuong])", stored: true);
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.HasIndex(e => new { e.CartId, e.ProductId }).IsUnique().HasFilter("[MaBienSanPham] IS NULL");
                entity.HasIndex(e => new { e.CartId, e.ProductId, e.ProductVariantId }).IsUnique().HasFilter("[MaBienSanPham] IS NOT NULL");
                entity.HasOne(e => e.Cart).WithMany(e => e.Items).HasForeignKey(e => e.CartId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ProductVariant).WithMany().HasForeignKey(e => e.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("DONHANG");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaDonHang");
                entity.Property(e => e.OrderCode).HasColumnName("MaDonHangKinhDoanh").HasMaxLength(50).IsRequired();
                entity.Property(e => e.UserId).HasColumnName("MaNguoiDung").IsRequired();
                entity.Property(e => e.ShowroomId).HasColumnName("MaShowroom");
                entity.Property(e => e.ShippingFullName).HasColumnName("HoTenNhanHang").HasMaxLength(150).IsRequired();
                entity.Property(e => e.ShippingPhoneNumber).HasColumnName("SoDienThoaiNhanHang").HasMaxLength(20).IsRequired();
                entity.Property(e => e.ShippingEmail).HasColumnName("EmailNhanHang").HasMaxLength(255);
                entity.Property(e => e.ShippingAddressLine).HasColumnName("DiaChiNhanHang").HasMaxLength(255).IsRequired();
                entity.Property(e => e.ShippingWard).HasColumnName("PhuongXa").HasMaxLength(100);
                entity.Property(e => e.ShippingDistrict).HasColumnName("QuanHuyen").HasMaxLength(100);
                entity.Property(e => e.ShippingProvince).HasColumnName("TinhThanh").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Subtotal).HasColumnName("TongTienHang").HasPrecision(18, 2);
                entity.Property(e => e.DiscountAmount).HasColumnName("TienGiam").HasPrecision(18, 2);
                entity.Property(e => e.ShippingFee).HasColumnName("PhiVanChuyen").HasPrecision(18, 2);
                entity.Property(e => e.TotalAmount).HasColumnName("TongThanhToan").HasPrecision(18, 2);
                entity.Property(e => e.OrderStatus).HasColumnName("TrangThaiDonHang").HasMaxLength(20).IsRequired();
                entity.Property(e => e.PaymentStatus).HasColumnName("TrangThaiThanhToan").HasMaxLength(20).IsRequired();
                entity.Property(e => e.Note).HasColumnName("GhiChu").HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.HasIndex(e => e.OrderCode).IsUnique();
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });
                entity.HasIndex(e => new { e.OrderStatus, e.CreatedAt });
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Showroom).WithMany().HasForeignKey(e => e.ShowroomId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.OrderDetails).WithOne(e => e.Order).HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("CHITIET_DONHANG");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaChiTietDonHang");
                entity.Property(e => e.OrderId).HasColumnName("MaDonHang");
                entity.Property(e => e.ProductId).HasColumnName("MaSanPham");
                entity.Property(e => e.ProductVariantId).HasColumnName("MaBienSanPham");
                entity.Property(e => e.ProductNameSnapshot).HasColumnName("TenSanPhamSnapshot").HasMaxLength(255).IsRequired();
                entity.Property(e => e.SkuSnapshot).HasColumnName("SKUSnapshot").HasMaxLength(80);
                entity.Property(e => e.UnitPrice).HasColumnName("DonGia").HasPrecision(18, 2);
                entity.Property(e => e.Quantity).HasColumnName("SoLuong");
                entity.Property(e => e.LineTotal).HasColumnName("ThanhTien").HasPrecision(18, 2).HasComputedColumnSql("CONVERT([decimal](18,2),[DonGia]*[SoLuong])", stored: true);
                entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ProductVariant).WithMany().HasForeignKey(e => e.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("THANHTOAN");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaThanhToan");
                entity.Property(e => e.PaymentCode).HasColumnName("MaThanhToanKinhDoanh").HasMaxLength(50).IsRequired();
                entity.Property(e => e.OrderId).HasColumnName("MaDonHang");
                entity.Property(e => e.Amount).HasColumnName("SoTien").HasPrecision(18, 2);
                entity.Property(e => e.PaymentMethod).HasColumnName("PhuongThuc").HasMaxLength(30).IsRequired();
                entity.Property(e => e.PaymentStatus).HasColumnName("TrangThai").HasMaxLength(20).IsRequired();
                entity.Property(e => e.TransactionRef).HasColumnName("MaGiaoDich").HasMaxLength(120);
                entity.Property(e => e.PaidAt).HasColumnName("DaThanhToanLuc");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.HasIndex(e => e.PaymentCode).IsUnique();
                entity.HasIndex(e => new { e.OrderId, e.PaymentStatus });
                entity.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.ToTable("VOUCHER");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaVoucher");
                entity.Property(e => e.Code).HasColumnName("MaVoucherCode").HasMaxLength(50).IsRequired();
                entity.Property(e => e.DiscountType).HasColumnName("LoaiGiamGia").HasMaxLength(20).IsRequired();
                entity.Property(e => e.DiscountValue).HasColumnName("GiaTriGiam").HasPrecision(18, 2);
                entity.Property(e => e.MinOrderValue).HasColumnName("GiaTriDonToiThieu").HasPrecision(18, 2);
                entity.Property(e => e.MaxDiscountValue).HasColumnName("GiaTriGiamToiDa").HasPrecision(18, 2);
                entity.Property(e => e.StartAt).HasColumnName("NgayBatDau");
                entity.Property(e => e.EndAt).HasColumnName("NgayKetThuc");
                entity.Property(e => e.UsageLimit).HasColumnName("GioiHanSuDung");
                entity.Property(e => e.UsedCount).HasColumnName("SoLanDaDung");
                entity.Property(e => e.IsActive).HasColumnName("DangHoatDong");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => new { e.IsActive, e.StartAt, e.EndAt });
            });

            modelBuilder.Entity<OrderVoucher>(entity =>
            {
                entity.ToTable("DONHANG_VOUCHER");
                entity.HasKey(e => new { e.OrderId, e.VoucherId });
                entity.Property(e => e.OrderId).HasColumnName("MaDonHang");
                entity.Property(e => e.VoucherId).HasColumnName("MaVoucher");
                entity.Property(e => e.VoucherCodeSnapshot).HasColumnName("MaVoucherCodeSnapshot").HasMaxLength(50).IsRequired();
                entity.Property(e => e.DiscountAmount).HasColumnName("SoTienGiam").HasPrecision(18, 2);
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Voucher).WithMany().HasForeignKey(e => e.VoucherId).OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureContent(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContactRequest>(entity =>
            {
                entity.ToTable("LIENHE_YEUCAU");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaLienHe");
                entity.Property(e => e.FullName).HasColumnName("HoTen").HasMaxLength(150).IsRequired();
                entity.Property(e => e.PhoneNumber).HasColumnName("SoDienThoai").HasMaxLength(20).IsRequired();
                entity.Property(e => e.Email).HasColumnName("Email").HasMaxLength(255);
                entity.Property(e => e.Subject).HasColumnName("TieuDe").HasMaxLength(255);
                entity.Property(e => e.Message).HasColumnName("NoiDung").IsRequired();
                entity.Property(e => e.InquiryType).HasColumnName("LoaiYeuCau").HasMaxLength(30).IsRequired();
                entity.Property(e => e.ProductId).HasColumnName("MaSanPham");
                entity.Property(e => e.ShowroomId).HasColumnName("MaShowroom");
                entity.Property(e => e.Status).HasColumnName("TrangThai").HasMaxLength(20).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.ProcessedAt).HasColumnName("DaXuLyLuc");
                entity.Property(e => e.ProcessedByUserId).HasColumnName("MaNguoiXuLy");
                entity.HasIndex(e => new { e.Status, e.CreatedAt });
                entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Showroom).WithMany().HasForeignKey(e => e.ShowroomId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ProcessedByUser).WithMany().HasForeignKey(e => e.ProcessedByUserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BlogPost>(entity =>
            {
                entity.ToTable("BAIVIET");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaBaiViet");
                entity.Property(e => e.Title).HasColumnName("TieuDe").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Slug).HasColumnName("Slug").HasMaxLength(280).IsRequired();
                entity.Property(e => e.Summary).HasColumnName("TomTat").HasMaxLength(500);
                entity.Property(e => e.Content).HasColumnName("NoiDung").IsRequired();
                entity.Property(e => e.ThumbnailUrl).HasColumnName("AnhDaiDienUrl").HasMaxLength(500);
                entity.Property(e => e.Category).HasColumnName("DanhMuc").HasMaxLength(100);
                entity.Property(e => e.AuthorUserId).HasColumnName("MaTacGia");
                entity.Property(e => e.PublishedAt).HasColumnName("XuatBanLuc");
                entity.Property(e => e.Status).HasColumnName("TrangThai").HasMaxLength(20).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => new { e.Status, e.PublishedAt });
                entity.HasOne(e => e.AuthorUser).WithMany().HasForeignKey(e => e.AuthorUserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Faq>(entity =>
            {
                entity.ToTable("FAQ");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaFAQ");
                entity.Property(e => e.Question).HasColumnName("CauHoi").HasMaxLength(500).IsRequired();
                entity.Property(e => e.Answer).HasColumnName("CauTraLoi").IsRequired();
                entity.Property(e => e.Category).HasColumnName("DanhMuc").HasMaxLength(100);
                entity.Property(e => e.SortOrder).HasColumnName("ThuTuHienThi");
                entity.Property(e => e.IsActive).HasColumnName("DangHoatDong");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.HasIndex(e => new { e.Category, e.IsActive, e.SortOrder });
            });

            modelBuilder.Entity<ProductReview>(entity =>
            {
                entity.ToTable("DANHGIASANPHAM");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaDanhGia");
                entity.Property(e => e.ProductId).HasColumnName("MaSanPham");
                entity.Property(e => e.UserId).HasColumnName("MaNguoiDung").IsRequired();
                entity.Property(e => e.OrderId).HasColumnName("MaDonHang");
                entity.Property(e => e.Rating).HasColumnName("Diem");
                entity.Property(e => e.Title).HasColumnName("TieuDe").HasMaxLength(255);
                entity.Property(e => e.Content).HasColumnName("NoiDung");
                entity.Property(e => e.Status).HasColumnName("TrangThai").HasMaxLength(20).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.HasIndex(e => new { e.ProductId, e.Status, e.CreatedAt });
                entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Cars", Slug = "cars", Description = "New and used cars", SortOrder = 1, IsActive = true },
                new Category { Id = 2, Name = "Accessories", Slug = "accessories", Description = "Car accessories and parts", SortOrder = 2, IsActive = true },
                new Category { Id = 3, ParentCategoryId = 1, Name = "Sedan", Slug = "sedan", Description = "Sedan cars", SortOrder = 1, IsActive = true },
                new Category { Id = 4, ParentCategoryId = 1, Name = "SUV", Slug = "suv", Description = "SUV cars", SortOrder = 2, IsActive = true },
                new Category { Id = 5, ParentCategoryId = 2, Name = "Interior Accessories", Slug = "interior-accessories", Description = "Interior car accessories", SortOrder = 1, IsActive = true },
                new Category { Id = 6, ParentCategoryId = 2, Name = "Dash Cameras", Slug = "dash-cameras", Description = "Dash cameras and recording devices", SortOrder = 2, IsActive = true }
            );

            modelBuilder.Entity<Brand>().HasData(
                new Brand { Id = 1, Name = "Toyota", Slug = "toyota", LogoUrl = "/images/brands/toyota.png", IsActive = true },
                new Brand { Id = 2, Name = "Mazda", Slug = "mazda", LogoUrl = "/images/brands/mazda.png", IsActive = true },
                new Brand { Id = 3, Name = "Hyundai", Slug = "hyundai", LogoUrl = "/images/brands/hyundai.png", IsActive = true }
            );

            modelBuilder.Entity<CarModel>().HasData(
                new CarModel { Id = 1, BrandId = 1, Name = "Vios", Slug = "toyota-vios", IsActive = true },
                new CarModel { Id = 2, BrandId = 2, Name = "CX-5", Slug = "mazda-cx-5", IsActive = true },
                new CarModel { Id = 3, BrandId = 3, Name = "Tucson", Slug = "hyundai-tucson", IsActive = true }
            );

            modelBuilder.Entity<Showroom>().HasData(
                new Showroom
                {
                    Id = 1,
                    Name = "Auto Showroom Quan 7",
                    Slug = "auto-showroom-quan-7",
                    AddressLine = "123 Nguyen Van Linh",
                    Ward = "Tan Phu",
                    District = "Quan 7",
                    Province = "TP. Ho Chi Minh",
                    PhoneNumber = "0900123456",
                    Email = "showroom.q7@autoshowroom.vn",
                    OpeningHours = "08:00 - 20:00",
                    IsActive = true
                }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    ProductCode = "CAR-VIOS-2024",
                    Name = "Toyota Vios 1.5G 2024",
                    Slug = "toyota-vios-15g-2024",
                    CategoryId = 3,
                    BrandId = 1,
                    CarModelId = 1,
                    ShowroomId = 1,
                    ProductType = "Car",
                    ShortDescription = "New sedan with strong fuel economy for family and service use.",
                    Description = "Toyota Vios 1.5G 2024, CVT transmission, practical cabin and full safety package.",
                    BasePrice = 592000000,
                    SalePrice = 575000000,
                    StockQuantity = 3,
                    MainImageUrl = "/images/products/vios-2024-main.jpg",
                    Condition = "New",
                    Year = 2024,
                    Mileage = 0,
                    ExteriorColor = "Pearl White",
                    InteriorColor = "Black",
                    Seats = 5,
                    Transmission = "CVT",
                    FuelType = "Gasoline",
                    Engine = "1.5L",
                    DriveType = "FWD",
                    Vin = "VINVIOS20240001",
                    Status = "Available",
                    IsActive = true
                },
                new Product
                {
                    Id = 2,
                    ProductCode = "CAR-CX5-2022",
                    Name = "Mazda CX-5 2.0 Premium 2022",
                    Slug = "mazda-cx-5-20-premium-2022",
                    CategoryId = 4,
                    BrandId = 2,
                    CarModelId = 2,
                    ShowroomId = 1,
                    ProductType = "Car",
                    ShortDescription = "Used SUV in good condition with low mileage.",
                    Description = "Mazda CX-5 2.0 Premium 2022, privately used and well maintained.",
                    BasePrice = 799000000,
                    SalePrice = 765000000,
                    StockQuantity = 1,
                    MainImageUrl = "/images/products/cx5-2022-main.jpg",
                    Condition = "Used",
                    Year = 2022,
                    Mileage = 28000,
                    ExteriorColor = "Soul Red",
                    InteriorColor = "Black",
                    Seats = 5,
                    Transmission = "Automatic",
                    FuelType = "Gasoline",
                    Engine = "2.0L SkyActiv-G",
                    DriveType = "FWD",
                    Vin = "VINCX520220001",
                    LicensePlate = "51H-123.45",
                    Status = "Available",
                    IsActive = true
                },
                new Product
                {
                    Id = 3,
                    ProductCode = "ACC-CAM-70MAI-A500S",
                    Name = "70mai A500S Dash Camera",
                    Slug = "70mai-a500s-dash-camera",
                    CategoryId = 6,
                    ProductType = "Accessory",
                    ShortDescription = "Front and rear dash camera with clear recording.",
                    Description = "70mai A500S dash camera with GPS, ADAS warning and 2K recording.",
                    BasePrice = 3200000,
                    SalePrice = 2950000,
                    StockQuantity = 25,
                    MainImageUrl = "/images/products/70mai-a500s-main.jpg",
                    Status = "Available",
                    IsActive = true
                },
                new Product
                {
                    Id = 4,
                    ProductCode = "ACC-MAT-SEDAN-3D",
                    Name = "3D Floor Mats for Sedan",
                    Slug = "3d-floor-mats-sedan",
                    CategoryId = 5,
                    ProductType = "Accessory",
                    ShortDescription = "Anti-slip floor mats, easy to clean.",
                    Description = "Premium 3D floor mats suitable for many sedan models.",
                    BasePrice = 1800000,
                    SalePrice = 1500000,
                    StockQuantity = 40,
                    MainImageUrl = "/images/products/tham-3d-sedan-main.jpg",
                    Status = "Available",
                    IsActive = true
                }
            );

            modelBuilder.Entity<ProductVariant>().HasData(
                new ProductVariant { Id = 1, ProductId = 1, VariantName = "1.5G CVT - Pearl White - Black interior", Sku = "VIOS-15G-WHITE-BLACK", PriceOverride = 575000000, StockQuantity = 2, Status = "Available", Version = "1.5G CVT", ExteriorColor = "Pearl White", InteriorColor = "Black" },
                new ProductVariant { Id = 2, ProductId = 1, VariantName = "1.5G CVT - Red - Black interior", Sku = "VIOS-15G-RED-BLACK", PriceOverride = 572000000, StockQuantity = 1, Status = "Available", Version = "1.5G CVT", ExteriorColor = "Red", InteriorColor = "Black" },
                new ProductVariant { Id = 3, ProductId = 2, VariantName = "2.0 Premium - Soul Red - Black interior", Sku = "CX5-20PRE-RED-BLACK", PriceOverride = 765000000, StockQuantity = 1, Status = "Available", Version = "2.0 Premium", ExteriorColor = "Soul Red", InteriorColor = "Black" },
                new ProductVariant { Id = 4, ProductId = 3, VariantName = "64GB bundle", Sku = "70MAI-A500S-64GB", PriceOverride = 2950000, StockQuantity = 15, Status = "Available", Version = "64GB" },
                new ProductVariant { Id = 5, ProductId = 3, VariantName = "128GB bundle", Sku = "70MAI-A500S-128GB", PriceOverride = 3350000, StockQuantity = 10, Status = "Available", Version = "128GB" },
                new ProductVariant { Id = 6, ProductId = 4, VariantName = "Black", Sku = "MAT-SEDAN-3D-BLACK", PriceOverride = 1500000, StockQuantity = 20, Status = "Available", Version = "Sedan", InteriorColor = "Black" }
            );

            modelBuilder.Entity<ProductImage>().HasData(
                new ProductImage { Id = 1, ProductId = 1, ImageUrl = "/images/products/vios-2024-main.jpg", AltText = "Toyota Vios 2024", IsPrimary = true, SortOrder = 1 },
                new ProductImage { Id = 2, ProductId = 1, ImageUrl = "/images/products/vios-2024-interior.jpg", AltText = "Toyota Vios 2024 interior", IsPrimary = false, SortOrder = 2 },
                new ProductImage { Id = 3, ProductId = 2, ImageUrl = "/images/products/cx5-2022-main.jpg", AltText = "Mazda CX-5 2022", IsPrimary = true, SortOrder = 1 },
                new ProductImage { Id = 4, ProductId = 3, ImageUrl = "/images/products/70mai-a500s-main.jpg", AltText = "70mai A500S Dash Camera", IsPrimary = true, SortOrder = 1 },
                new ProductImage { Id = 5, ProductId = 4, ImageUrl = "/images/products/tham-3d-sedan-main.jpg", AltText = "3D Floor Mats", IsPrimary = true, SortOrder = 1 }
            );

            modelBuilder.Entity<Voucher>().HasData(
                new Voucher
                {
                    Id = 1,
                    Code = "AUTO200K",
                    DiscountType = "Amount",
                    DiscountValue = 200000,
                    MinOrderValue = 2000000,
                    MaxDiscountValue = 200000,
                    StartAt = new DateTime(2026, 1, 1),
                    EndAt = new DateTime(2026, 12, 31),
                    UsageLimit = 100,
                    UsedCount = 0,
                    IsActive = true
                }
            );

            modelBuilder.Entity<Faq>().HasData(
                new Faq { Id = 1, Question = "Can I schedule a car viewing?", Answer = "Yes. Submit a consultation request or contact the nearest showroom.", Category = "Buying", SortOrder = 1, IsActive = true },
                new Faq { Id = 2, Question = "Does the website sell car accessories?", Answer = "Yes. Cars and accessories can be managed in the same product catalog.", Category = "Accessories", SortOrder = 2, IsActive = true }
            );
        }
    }
}
