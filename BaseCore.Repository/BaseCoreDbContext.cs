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
        public DbSet<PaymentRefund> PaymentRefunds => Set<PaymentRefund>();
        public DbSet<Voucher> Vouchers => Set<Voucher>();
        public DbSet<OrderVoucher> OrderVouchers => Set<OrderVoucher>();
        public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
        public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
        public DbSet<Faq> Faqs => Set<Faq>();
        public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
        public DbSet<SystemRole> SystemRoles => Set<SystemRole>();
        public DbSet<UserRoleAssignment> UserRoleAssignments => Set<UserRoleAssignment>();
        public DbSet<InventoryHold> InventoryHolds => Set<InventoryHold>();
        public DbSet<InstallmentPlan> InstallmentPlans => Set<InstallmentPlan>();
        public DbSet<PartCompatibility> PartCompatibilities => Set<PartCompatibility>();
        public DbSet<VoucherCategory> VoucherCategories => Set<VoucherCategory>();
        public DbSet<VoucherBrand> VoucherBrands => Set<VoucherBrand>();
        public DbSet<VoucherProduct> VoucherProducts => Set<VoucherProduct>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureUsers(modelBuilder);
            ConfigureCatalog(modelBuilder);
            ConfigureShopping(modelBuilder);
            ConfigureContent(modelBuilder);
            ConfigureRoles(modelBuilder);
            ConfigureInventoryAndInstallments(modelBuilder);
        
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
                    .IsUnicode(false)
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
                entity.Ignore(e => e.Ward);
                entity.Ignore(e => e.District);
                entity.Ignore(e => e.Province);
                entity.Property(e => e.PhoneNumber).HasColumnName("SoDienThoai").HasMaxLength(20);
                entity.Property(e => e.Email).HasColumnName("Email").HasMaxLength(255);
                entity.Property(e => e.OpeningHours).HasColumnName("GioMoCua").HasMaxLength(255);
                entity.Property(e => e.IsActive).HasColumnName("DangHoatDong");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.HasIndex(e => e.Slug).IsUnique();
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("SANPHAM", tb => tb.HasTrigger("trg_SANPHAM_Validate_HangXe_DongXe"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaSanPham");
                entity.Property(e => e.ProductCode).HasColumnName("MaSanPhamKinhDoanh").HasMaxLength(50).IsRequired();
                entity.Property(e => e.Name).HasColumnName("TenSanPham").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Slug).HasColumnName("Slug").HasMaxLength(280).IsRequired();
                entity.Property(e => e.CategoryId).HasColumnName("MaDanhMuc");
                entity.Property(e => e.BrandId).HasColumnName("MaHangXe");
                entity.Property(e => e.CarModelId).HasColumnName("MaDongXe");
                entity.Property(e => e.ShowroomId).HasColumnName("MaShowroom");
                entity.Property(e => e.ProductType).HasColumnName("LoaiSanPham").HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(e => e.ShortDescription).HasColumnName("MoTaNgan").HasMaxLength(500);
                entity.Property(e => e.Description).HasColumnName("MoTa");
                entity.Property(e => e.BasePrice).HasColumnName("GiaGoc").HasPrecision(18, 2);
                entity.Property(e => e.SalePrice).HasColumnName("GiaKhuyenMai").HasPrecision(18, 2);
                entity.Property(e => e.StockQuantity).HasColumnName("SoLuongTon");
                entity.Property(e => e.MainImageUrl).HasColumnName("AnhChinhUrl").HasMaxLength(500);
                entity.Property(e => e.IsActive).HasColumnName("DangHoatDong");
                entity.Property(e => e.MainColor).HasColumnName("MauSacChinh").HasMaxLength(80);
                entity.Property(e => e.MotorcycleType).HasColumnName("LoaiXeMay").HasMaxLength(50);
                entity.Property(e => e.EngineCapacity).HasColumnName("DungTichXiLanh");
                entity.Property(e => e.Power).HasColumnName("CongSuat").HasMaxLength(50);
                entity.Property(e => e.Torque).HasColumnName("MoMenXoan").HasMaxLength(50);
                entity.Property(e => e.FuelTankCapacity).HasColumnName("DungTichBinhXang").HasPrecision(6, 2);
                entity.Property(e => e.FrontBrake).HasColumnName("PhanhTruoc").HasMaxLength(80);
                entity.Property(e => e.RearBrake).HasColumnName("PhanhSau").HasMaxLength(80);
                entity.Property(e => e.HasAbs).HasColumnName("CoABS");
                entity.Property(e => e.Weight).HasColumnName("TrongLuong").HasPrecision(8, 2);
                entity.Property(e => e.SeatHeight).HasColumnName("ChieuCaoYen");
                entity.Property(e => e.Origin).HasColumnName("XuatXu").HasMaxLength(100);
                entity.Property(e => e.WarrantyMonths).HasColumnName("BaoHanhThang");
                entity.Ignore(e => e.Condition);
                entity.Ignore(e => e.Year);
                entity.Ignore(e => e.Mileage);
                entity.Ignore(e => e.ExteriorColor);
                entity.Ignore(e => e.InteriorColor);
                entity.Ignore(e => e.Seats);
                entity.Ignore(e => e.Transmission);
                entity.Ignore(e => e.FuelType);
                entity.Ignore(e => e.Engine);
                entity.Ignore(e => e.DriveType);
                entity.Ignore(e => e.Vin);
                entity.Ignore(e => e.LicensePlate);
                entity.Property(e => e.Status).HasColumnName("TrangThaiSanPham").HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.HasIndex(e => e.ProductCode).IsUnique();
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => new { e.CategoryId, e.ProductType, e.IsActive, e.Status });
                entity.HasIndex(e => new { e.BrandId, e.CarModelId, e.IsActive });
                entity.HasIndex(e => new { e.MotorcycleType, e.BrandId, e.CarModelId, e.IsActive });
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
                    .WithOne(e => e.Product)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Images)
                    .WithOne(e => e.Product)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.ToTable("BIENSANPHAM", tb => tb.HasTrigger("trg_BIENSANPHAM_Sync_SoLuongTon_SANPHAM"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaBienSanPham");
                entity.Property(e => e.ProductId).HasColumnName("MaSanPham");
                entity.Property(e => e.VariantName).HasColumnName("TenBienThe").HasMaxLength(180).IsRequired();
                entity.Property(e => e.Sku).HasColumnName("SKU").HasMaxLength(80).IsRequired();
                entity.Property(e => e.PriceOverride).HasColumnName("GiaGhiDe").HasPrecision(18, 2);
                entity.Property(e => e.StockQuantity).HasColumnName("SoLuongTon");
                entity.Property(e => e.Status).HasColumnName("TrangThai").HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(e => e.Version).HasColumnName("PhienBan").HasMaxLength(100);
                entity.Property(e => e.Color).HasColumnName("MauSac").HasMaxLength(80);
                entity.Ignore(e => e.ExteriorColor);
                entity.Ignore(e => e.InteriorColor);
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.HasIndex(e => e.Sku).IsUnique();
                entity.HasIndex(e => new { e.ProductId, e.Status });
                entity.HasOne(e => e.Product).WithMany(e => e.Variants).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.ToTable("ANHSANPHAM", tb => tb.HasTrigger("trg_ANHSANPHAM_Validate_MaBienSanPham"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaAnhSanPham");
                entity.Property(e => e.ProductId).HasColumnName("MaSanPham");
                entity.Property(e => e.ProductVariantId).HasColumnName("MaBienSanPham");
                entity.Property(e => e.ImageUrl).HasColumnName("UrlAnh").HasMaxLength(500).IsRequired();
                entity.Property(e => e.AltText).HasColumnName("AltText").HasMaxLength(255);
                entity.Property(e => e.IsPrimary).HasColumnName("LaAnhChinh");
                entity.Property(e => e.SortOrder).HasColumnName("ThuTuHienThi");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.HasIndex(e => new { e.ProductId, e.ProductVariantId, e.SortOrder });
                entity.HasOne(e => e.ProductVariant).WithMany(e => e.Images).HasForeignKey(e => e.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
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
                entity.Property(e => e.Status).HasColumnName("TrangThai").HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.HasIndex(e => e.UserId).IsUnique().HasFilter("[TrangThai] = 'Active'");
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("CHITIET_GIOHANG", tb => tb.HasTrigger("trg_CHITIET_GIOHANG_Validate_MaBienSanPham"));
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
                entity.Property(e => e.Subtotal).HasColumnName("TongTienHang").HasPrecision(18, 2);
                entity.Property(e => e.DiscountAmount).HasColumnName("TienGiam").HasPrecision(18, 2);
                entity.Property(e => e.ShippingFee).HasColumnName("PhiVanChuyen").HasPrecision(18, 2);
                entity.Property(e => e.TotalAmount).HasColumnName("TongThanhToan").HasPrecision(18, 2);
                entity.Property(e => e.OrderStatus).HasColumnName("TrangThaiDonHang").HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(e => e.PaymentStatus).HasColumnName("TrangThaiThanhToan").HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(e => e.Note).HasColumnName("GhiChu").HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.Ignore(e => e.ShippingWard);
                entity.Ignore(e => e.ShippingDistrict);
                entity.Ignore(e => e.ShippingProvince);
                entity.Ignore(e => e.CheckoutExpiresAt);
                entity.Property(e => e.PaidSuccessfullyAt).HasColumnName("NgayThanhToanThanhCong");
                entity.Property(e => e.CancelledAt).HasColumnName("NgayHuyDon");
                entity.Property(e => e.CancelReason).HasColumnName("LyDoHuyDon").HasMaxLength(500);
                entity.Property(e => e.CartId).HasColumnName("MaGioHang");
                entity.Property(e => e.ReceivingMethod).HasColumnName("PhuongThucNhanHang").HasMaxLength(30).IsUnicode(false).IsRequired();
                entity.Property(e => e.ShippingStatus).HasColumnName("TrangThaiVanChuyen").HasMaxLength(30).IsUnicode(false).IsRequired();
                entity.Property(e => e.OrderType).HasColumnName("LoaiDonHang").HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(e => e.DepositAmount).HasColumnName("TienDatCoc").HasPrecision(18, 2);
                entity.Property(e => e.RemainingAmount).HasColumnName("SoTienConLai").HasPrecision(18, 2);
                entity.Property(e => e.PickupAppointmentAt).HasColumnName("NgayHenNhanXe");
                entity.Property(e => e.FulfillmentNote).HasColumnName("GhiChuGiaoNhan").HasMaxLength(500);
                entity.HasIndex(e => e.OrderCode).IsUnique();
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });
                entity.HasIndex(e => new { e.OrderStatus, e.CreatedAt });
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Showroom).WithMany().HasForeignKey(e => e.ShowroomId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Cart).WithMany().HasForeignKey(e => e.CartId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.OrderDetails).WithOne(e => e.Order).HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Payments).WithOne(e => e.Order).HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("CHITIET_DONHANG", tb => tb.HasTrigger("trg_CHITIET_DONHANG_Validate_MaBienSanPham"));
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
                entity.Property(e => e.PaymentMethod).HasColumnName("PhuongThuc").HasMaxLength(30).IsUnicode(false).IsRequired();
                entity.Property(e => e.PaymentStatus).HasColumnName("TrangThai").HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(e => e.TransactionRef).HasColumnName("MaGiaoDich").HasMaxLength(120);
                entity.Property(e => e.PaidAt).HasColumnName("DaThanhToanLuc");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.PaymentType).HasColumnName("LoaiThanhToan").HasMaxLength(30).IsUnicode(false).IsRequired();
                entity.Property(e => e.RefundedAmount).HasColumnName("SoTienHoan").HasPrecision(18, 2);
                entity.Property(e => e.TransferContent).HasColumnName("NoiDungChuyenKhoan").HasMaxLength(500);
                entity.Property(e => e.BankCode).HasColumnName("MaNganHang").HasMaxLength(50);
                entity.Ignore(e => e.RefundTransactionRef);
                entity.Ignore(e => e.RefundedAt);
                entity.Property(e => e.CancelReason).HasColumnName("LyDoHuy").HasMaxLength(500);
                entity.Property(e => e.CancelledAt).HasColumnName("NgayHuy");
                entity.Property(e => e.RawResponse).HasColumnName("ResponseRaw");
                entity.HasIndex(e => e.PaymentCode).IsUnique();
                entity.HasIndex(e => new { e.OrderId, e.PaymentStatus });
                entity.HasOne(e => e.Order).WithMany(e => e.Payments).HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PaymentRefund>(entity =>
            {
                entity.ToTable("THANHTOAN_HOANTIEN");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaHoanTien");
                entity.Property(e => e.PaymentId).HasColumnName("MaThanhToan");
                entity.Property(e => e.OrderId).HasColumnName("MaDonHang");
                entity.Property(e => e.Amount).HasColumnName("SoTienHoan").HasPrecision(18, 2);
                entity.Property(e => e.RefundTransactionRef).HasColumnName("MaGiaoDichHoanTien").HasMaxLength(120);
                entity.Property(e => e.Reason).HasColumnName("LyDo").HasMaxLength(500);
                entity.Property(e => e.Status).HasColumnName("TrangThai").HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(e => e.RawResponse).HasColumnName("ResponseRaw");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.HasOne(e => e.Payment).WithMany(e => e.Refunds).HasForeignKey(e => e.PaymentId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.ToTable("VOUCHER");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaVoucher");
                entity.Property(e => e.Code).HasColumnName("MaVoucherCode").HasMaxLength(50).IsRequired();
                entity.Property(e => e.DiscountType).HasColumnName("LoaiGiamGia").HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(e => e.DiscountValue).HasColumnName("GiaTriGiam").HasPrecision(18, 2);
                entity.Property(e => e.MinOrderValue).HasColumnName("GiaTriDonToiThieu").HasPrecision(18, 2);
                entity.Property(e => e.MaxDiscountValue).HasColumnName("GiaTriGiamToiDa").HasPrecision(18, 2);
                entity.Property(e => e.StartAt).HasColumnName("NgayBatDau");
                entity.Property(e => e.EndAt).HasColumnName("NgayKetThuc");
                entity.Property(e => e.UsageLimit).HasColumnName("GioiHanSuDung");
                entity.Property(e => e.UsedCount).HasColumnName("SoLanDaDung");
                entity.Property(e => e.IsActive).HasColumnName("DangHoatDong");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.Description).HasColumnName("MoTa").HasMaxLength(500);
                entity.Property(e => e.MaxUsagePerUser).HasColumnName("SoLanToiDaMoiNguoiDung");
                entity.Property(e => e.Scope).HasColumnName("PhamViApDung").HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
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
                entity.Property(e => e.DiscountTypeSnapshot).HasColumnName("LoaiGiamGiaSnapshot").HasMaxLength(20).IsUnicode(false);
                entity.Property(e => e.DiscountValueSnapshot).HasColumnName("GiaTriGiamSnapshot").HasPrecision(18, 2);
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
                entity.Property(e => e.InquiryType).HasColumnName("LoaiYeuCau").HasMaxLength(30).IsUnicode(false).IsRequired();
                entity.Property(e => e.ProductId).HasColumnName("MaSanPham");
                entity.Property(e => e.ShowroomId).HasColumnName("MaShowroom");
                entity.Property(e => e.Status).HasColumnName("TrangThai").HasMaxLength(20).IsUnicode(false).IsRequired();
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
                entity.Property(e => e.Status).HasColumnName("TrangThai").HasMaxLength(20).IsUnicode(false).IsRequired();
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
                entity.Property(e => e.Status).HasColumnName("TrangThai").HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.HasIndex(e => new { e.ProductId, e.Status, e.CreatedAt });
                entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SystemRole>(entity =>
            {
                entity.ToTable("VAITRO");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaVaiTro").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("TenVaiTro").HasMaxLength(30).IsUnicode(false).IsRequired();
                entity.Property(e => e.Description).HasColumnName("MoTa").HasMaxLength(255);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            modelBuilder.Entity<UserRoleAssignment>(entity =>
            {
                entity.ToTable("NGUOIDUNG_VAITRO");
                entity.HasKey(e => new { e.UserId, e.RoleId });
                entity.Property(e => e.UserId).HasColumnName("MaNguoiDung");
                entity.Property(e => e.RoleId).HasColumnName("MaVaiTro");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Role).WithMany(e => e.UserAssignments).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureInventoryAndInstallments(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InventoryHold>(entity =>
            {
                entity.ToTable("TONKHO_GIUCHO", tb => tb.HasTrigger("trg_TONKHO_GIUCHO_Validate_MaBienSanPham"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaGiuCho");
                entity.Property(e => e.OrderId).HasColumnName("MaDonHang");
                entity.Property(e => e.OrderDetailId).HasColumnName("MaChiTietDonHang");
                entity.Property(e => e.ProductId).HasColumnName("MaSanPham");
                entity.Property(e => e.ProductVariantId).HasColumnName("MaBienSanPham");
                entity.Property(e => e.Quantity).HasColumnName("SoLuong");
                entity.Property(e => e.Status).HasColumnName("TrangThai").HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(e => e.ExpiresAt).HasColumnName("HetHanLuc");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.Property(e => e.Note).HasColumnName("GhiChu").HasMaxLength(500);
                entity.HasIndex(e => new { e.OrderDetailId, e.Status }).IsUnique().HasFilter("[MaChiTietDonHang] IS NOT NULL AND [TrangThai] = 'Active'");
                entity.HasIndex(e => new { e.ProductId, e.ProductVariantId, e.Status, e.ExpiresAt });
                entity.HasOne(e => e.Order).WithMany(e => e.InventoryHolds).HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.OrderDetail).WithMany().HasForeignKey(e => e.OrderDetailId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ProductVariant).WithMany().HasForeignKey(e => e.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<InstallmentPlan>(entity =>
            {
                entity.ToTable("TRA_GOP");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaTraGop");
                entity.Property(e => e.OrderId).HasColumnName("MaDonHang");
                entity.Property(e => e.DownPaymentAmount).HasColumnName("SoTienTraTruoc").HasPrecision(18, 2);
                entity.Property(e => e.FinancedAmount).HasColumnName("SoTienTraGop").HasPrecision(18, 2);
                entity.Property(e => e.Months).HasColumnName("SoThang");
                entity.Property(e => e.MonthlyInterestRate).HasColumnName("LaiSuatThang").HasPrecision(5, 2);
                entity.Property(e => e.MonthlyPaymentAmount).HasColumnName("SoTienMoiThang").HasPrecision(18, 2);
                entity.Property(e => e.PaidPeriods).HasColumnName("SoKyDaTra");
                entity.Property(e => e.BuyerFullName).HasColumnName("HoTenNguoiMua").HasMaxLength(150);
                entity.Property(e => e.PhoneNumber).HasColumnName("SoDienThoai").HasMaxLength(20);
                entity.Property(e => e.CitizenId).HasColumnName("CCCD").HasMaxLength(20);
                entity.Property(e => e.Address).HasColumnName("DiaChi").HasMaxLength(255);
                entity.Property(e => e.FinanceCompany).HasColumnName("DonViTaiChinh").HasMaxLength(150);
                entity.Property(e => e.StartDate).HasColumnName("NgayBatDau").HasColumnType("date");
                entity.Property(e => e.EndDate).HasColumnName("NgayKetThuc").HasColumnType("date");
                entity.Property(e => e.Status).HasColumnName("TrangThai").HasMaxLength(30).IsUnicode(false).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.Property(e => e.Note).HasColumnName("GhiChu").HasMaxLength(500);
                entity.HasIndex(e => e.OrderId).IsUnique();
                entity.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PartCompatibility>(entity =>
            {
                entity.ToTable("PHUTUNG_TUONGTHICH", tb => tb.HasTrigger("trg_PHUTUNG_TUONGTHICH_Validate"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("MaTuongThich");
                entity.Property(e => e.PartProductId).HasColumnName("MaPhuTung");
                entity.Property(e => e.BrandId).HasColumnName("MaHangXe");
                entity.Property(e => e.CarModelId).HasColumnName("MaDongXe");
                entity.Property(e => e.FromYear).HasColumnName("NamTu");
                entity.Property(e => e.ToYear).HasColumnName("NamDen");
                entity.Property(e => e.AppliesToAllMotorcycles).HasColumnName("ApDungTatCaXe");
                entity.Property(e => e.Note).HasColumnName("GhiChu").HasMaxLength(500);
                entity.Property(e => e.IsActive).HasColumnName("DangHoatDong");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.Property(e => e.UpdatedAt).HasColumnName("NgayCapNhat");
                entity.HasIndex(e => new { e.PartProductId, e.BrandId, e.CarModelId });
                entity.HasOne(e => e.PartProduct).WithMany().HasForeignKey(e => e.PartProductId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Brand).WithMany().HasForeignKey(e => e.BrandId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.CarModel).WithMany().HasForeignKey(e => e.CarModelId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<VoucherCategory>(entity =>
            {
                entity.ToTable("VOUCHER_DANHMUC");
                entity.HasKey(e => new { e.VoucherId, e.CategoryId });
                entity.Property(e => e.VoucherId).HasColumnName("MaVoucher");
                entity.Property(e => e.CategoryId).HasColumnName("MaDanhMuc");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.HasOne(e => e.Voucher).WithMany(e => e.Categories).HasForeignKey(e => e.VoucherId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<VoucherBrand>(entity =>
            {
                entity.ToTable("VOUCHER_HANGXE");
                entity.HasKey(e => new { e.VoucherId, e.BrandId });
                entity.Property(e => e.VoucherId).HasColumnName("MaVoucher");
                entity.Property(e => e.BrandId).HasColumnName("MaHangXe");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.HasOne(e => e.Voucher).WithMany(e => e.Brands).HasForeignKey(e => e.VoucherId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Brand).WithMany().HasForeignKey(e => e.BrandId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<VoucherProduct>(entity =>
            {
                entity.ToTable("VOUCHER_SANPHAM");
                entity.HasKey(e => new { e.VoucherId, e.ProductId });
                entity.Property(e => e.VoucherId).HasColumnName("MaVoucher");
                entity.Property(e => e.ProductId).HasColumnName("MaSanPham");
                entity.Property(e => e.CreatedAt).HasColumnName("NgayTao");
                entity.HasOne(e => e.Voucher).WithMany(e => e.Products).HasForeignKey(e => e.VoucherId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Motorcycles", Slug = "motorcycles", Description = "New and used motorcycles", SortOrder = 1, IsActive = true },
                new Category { Id = 2, Name = "Accessories", Slug = "accessories", Description = "Motorcycle accessories and parts", SortOrder = 2, IsActive = true },
                new Category { Id = 3, ParentCategoryId = 1, Name = "Scooter", Slug = "scooter", Description = "Scooter motorcycles", SortOrder = 1, IsActive = true },
                new Category { Id = 4, ParentCategoryId = 1, Name = "Underbone", Slug = "underbone", Description = "Underbone motorcycles", SortOrder = 2, IsActive = true },
                new Category { Id = 5, ParentCategoryId = 2, Name = "Helmets", Slug = "helmets", Description = "Safety helmets", SortOrder = 1, IsActive = true },
                new Category { Id = 6, ParentCategoryId = 2, Name = "Dash Cameras", Slug = "dash-cameras", Description = "Dash cameras for motorcycles", SortOrder = 2, IsActive = true }
            );

            modelBuilder.Entity<Brand>().HasData(
                new Brand { Id = 1, Name = "Honda", Slug = "honda", LogoUrl = "/images/brands/honda.png", IsActive = true },
                new Brand { Id = 2, Name = "Yamaha", Slug = "yamaha", LogoUrl = "/images/brands/yamaha.png", IsActive = true },
                new Brand { Id = 3, Name = "Suzuki", Slug = "suzuki", LogoUrl = "/images/brands/suzuki.png", IsActive = true }
            );

            modelBuilder.Entity<CarModel>().HasData(
                new CarModel { Id = 1, BrandId = 1, Name = "Air Blade", Slug = "honda-air-blade", IsActive = true },
                new CarModel { Id = 2, BrandId = 2, Name = "Exciter", Slug = "yamaha-exciter", IsActive = true },
                new CarModel { Id = 3, BrandId = 3, Name = "Raider", Slug = "suzuki-raider", IsActive = true }
            );

            modelBuilder.Entity<Showroom>().HasData(
                new Showroom
                {
                    Id = 1,
                    Name = "Motorcycle Showroom Quan 7",
                    Slug = "motor-showroom-quan-7",
                    AddressLine = "123 Nguyen Van Linh",
                    Ward = "Tan Phu",
                    District = "Quan 7",
                    Province = "TP. Ho Chi Minh",
                    PhoneNumber = "0900123456",
                    Email = "showroom.q7@motorshowroom.vn",
                    OpeningHours = "08:00 - 20:00",
                    IsActive = true
                }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    ProductCode = "MOTO-AB-2024",
                    Name = "Honda Air Blade 160 2024",
                    Slug = "honda-air-blade-160-2024",
                    CategoryId = 3,
                    BrandId = 1,
                    CarModelId = 1,
                    ShowroomId = 1,
                    ProductType = "Motorcycle",
                    ShortDescription = "New scooter with strong performance and fuel economy.",
                    Description = "Honda Air Blade 160 2024, practical design and full safety package.",
                    BasePrice = 57000000,
                    SalePrice = 55990000,
                    StockQuantity = 10,
                    MainImageUrl = "/images/products/air-blade-2024-main.jpg",
                    Condition = "New",
                    Status = "Available",
                    IsActive = true,
                    MotorcycleType = "Scooter",
                    EngineCapacity = 160,
                    MainColor = "Black",
                    Power = "11.2 kW",
                    Torque = "14.6 Nm",
                    FuelTankCapacity = 4.4m,
                    Weight = 114,
                    SeatHeight = 775,
                    WarrantyMonths = 36
                },
                new Product
                {
                    Id = 2,
                    ProductCode = "MOTO-EX-2024",
                    Name = "Yamaha Exciter 155 VVA 2024",
                    Slug = "yamaha-exciter-155-vva-2024",
                    CategoryId = 4,
                    BrandId = 2,
                    CarModelId = 2,
                    ShowroomId = 1,
                    ProductType = "Motorcycle",
                    ShortDescription = "Sporty underbone with VVA technology.",
                    Description = "Yamaha Exciter 155 VVA, sporty design, powerful acceleration.",
                    BasePrice = 52000000,
                    SalePrice = 50500000,
                    StockQuantity = 5,
                    MainImageUrl = "/images/products/exciter-2024-main.jpg",
                    Condition = "New",
                    Status = "Available",
                    IsActive = true,
                    MotorcycleType = "Underbone",
                    EngineCapacity = 155,
                    MainColor = "Blue",
                    Weight = 119,
                    SeatHeight = 795,
                    WarrantyMonths = 36
                },
                new Product
                {
                    Id = 3,
                    ProductCode = "ACC-HELMET-AGV-K3",
                    Name = "AGV K3 Helmet",
                    Slug = "agv-k3-helmet",
                    CategoryId = 5,
                    ProductType = "Accessory",
                    ShortDescription = "Full face safety helmet.",
                    Description = "AGV K3 helmet with high safety standard and comfortable fit.",
                    BasePrice = 4500000,
                    SalePrice = 4200000,
                    StockQuantity = 25,
                    MainImageUrl = "/images/products/agv-k3-main.jpg",
                    Status = "Available",
                    IsActive = true
                },
                new Product
                {
                    Id = 4,
                    ProductCode = "ACC-GLOVE-ALP",
                    Name = "Alpinestars Leather Gloves",
                    Slug = "alpinestars-leather-gloves",
                    CategoryId = 5,
                    ProductType = "Accessory",
                    ShortDescription = "Protective leather gloves.",
                    Description = "Premium leather gloves for riding.",
                    BasePrice = 1800000,
                    SalePrice = 1500000,
                    StockQuantity = 40,
                    MainImageUrl = "/images/products/alpinestars-gloves-main.jpg",
                    Status = "Available",
                    IsActive = true
                }
            );

            modelBuilder.Entity<ProductVariant>().HasData(
                new ProductVariant { Id = 1, ProductId = 1, VariantName = "160cc - Black", Sku = "AB-160-BLACK", PriceOverride = 55990000, StockQuantity = 6, Status = "Available", Version = "160cc"},
                new ProductVariant { Id = 2, ProductId = 1, VariantName = "160cc - Red", Sku = "AB-160-RED", PriceOverride = 56990000, StockQuantity = 4, Status = "Available", Version = "160cc"},
                new ProductVariant { Id = 3, ProductId = 2, VariantName = "155 VVA - Blue", Sku = "EX-155-BLUE", PriceOverride = 50500000, StockQuantity = 5, Status = "Available", Version = "155 VVA"},
                new ProductVariant { Id = 4, ProductId = 3, VariantName = "Size L", Sku = "AGV-K3-L", PriceOverride = 4200000, StockQuantity = 15, Status = "Available", Version = "L" },
                new ProductVariant { Id = 5, ProductId = 3, VariantName = "Size XL", Sku = "AGV-K3-XL", PriceOverride = 4200000, StockQuantity = 10, Status = "Available", Version = "XL" },
                new ProductVariant { Id = 6, ProductId = 4, VariantName = "Size M", Sku = "GLOVE-ALP-M", PriceOverride = 1500000, StockQuantity = 20, Status = "Available", Version = "M" }
            );

            modelBuilder.Entity<ProductImage>().HasData(
                new ProductImage { Id = 1, ProductId = 1, ImageUrl = "/images/products/air-blade-2024-main.jpg", AltText = "Honda Air Blade 2024", IsPrimary = true, SortOrder = 1 },
                new ProductImage { Id = 2, ProductId = 1, ImageUrl = "/images/products/air-blade-2024-side.jpg", AltText = "Honda Air Blade 2024 side", IsPrimary = false, SortOrder = 2 },
                new ProductImage { Id = 3, ProductId = 2, ImageUrl = "/images/products/exciter-2024-main.jpg", AltText = "Yamaha Exciter 2024", IsPrimary = true, SortOrder = 1 },
                new ProductImage { Id = 4, ProductId = 3, ImageUrl = "/images/products/agv-k3-main.jpg", AltText = "AGV K3 Helmet", IsPrimary = true, SortOrder = 1 },
                new ProductImage { Id = 5, ProductId = 4, ImageUrl = "/images/products/alpinestars-gloves-main.jpg", AltText = "Alpinestars Gloves", IsPrimary = true, SortOrder = 1 }
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
