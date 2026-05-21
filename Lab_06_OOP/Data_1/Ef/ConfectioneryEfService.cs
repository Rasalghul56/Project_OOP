using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ConfectioneryShop.Models;

namespace ConfectioneryShop.Data.Ef
{


    public static class ConfectioneryEfService
    {
        public static Product MapToUi(ProductEntity p)
        {
            if (p == null) return null;
            return new Product
            {
                Id = p.ProductId,
                CategoryId = p.CategoryId,
                ManufacturerId = p.ManufacturerId,
                ShortName = p.ShortName ?? "",
                FullName = p.FullName ?? "",
                Description = p.Description ?? "",
                ImagePath = p.ImagePath ?? "",
                PhotoData = p.Photo,
                Category = p.Category != null ? p.Category.Name : "",
                Rating = p.Rating,
                Price = p.Price,
                Quantity = p.Quantity,
                Color = p.Color ?? "",
                Size = p.Size ?? "",
                Country = p.Manufacturer != null ? p.Manufacturer.Country ?? "" : "",
                Discount = p.Discount,
                IsOutOfStock = p.IsOutOfStock,
                SoldCount = p.SoldCount,
                Manufacturer = p.Manufacturer != null ? p.Manufacturer.Name : ""
            };
        }

        public static async Task<IReadOnlyList<Product>> GetAllProductsAsync()
        {
            using (var ctx = new ConfectioneryDbContext())
            {
                var list = await ctx.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.Manufacturer)
                    .OrderBy(p => p.ShortName)
                    .ToListAsync()
                    .ConfigureAwait(false);
                return list.Select(MapToUi).ToList();
            }
        }

        public static Product GetProductById(int id)
        {
            using (var ctx = new ConfectioneryDbContext())
            {
                var e = ctx.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.Manufacturer)
                    .FirstOrDefault(p => p.ProductId == id);
                return MapToUi(e);
            }
        }

        public static void AddProduct(Product model)
        {
            using (var ctx = new ConfectioneryDbContext())
            {
                var cat = ctx.Categories.FirstOrDefault(c => c.CategoryId == model.CategoryId)
                          ?? ctx.Categories.First(c => c.Name == model.Category);
                var man = ctx.Manufacturers.FirstOrDefault(m => m.ManufacturerId == model.ManufacturerId)
                          ?? ctx.Manufacturers.First(m => m.Name == model.Manufacturer);
                var e = new ProductEntity
                {
                    CategoryId = cat.CategoryId,
                    ManufacturerId = man.ManufacturerId,
                    ShortName = model.ShortName,
                    FullName = model.FullName,
                    Description = model.Description,
                    ImagePath = model.ImagePath,
                    Photo = model.PhotoData,
                    Price = model.Price,
                    Quantity = model.Quantity,
                    Rating = model.Rating,
                    Discount = model.Discount,
                    Color = model.Color,
                    Size = model.Size,
                    SoldCount = model.SoldCount,
                    IsOutOfStock = model.IsOutOfStock
                };
                ctx.Products.Add(e);
                ctx.SaveChanges();
                model.Id = e.ProductId;
                model.CategoryId = e.CategoryId;
                model.ManufacturerId = e.ManufacturerId;
            }
        }

        public static void UpdateProduct(Product model)
        {
            using (var ctx = new ConfectioneryDbContext())
            {
                var e = ctx.Products.FirstOrDefault(p => p.ProductId == model.Id);
                if (e == null) throw new InvalidOperationException("Товар не найден.");
                var cat = ctx.Categories.FirstOrDefault(c => c.CategoryId == model.CategoryId)
                          ?? ctx.Categories.First(c => c.Name == model.Category);
                var man = ctx.Manufacturers.FirstOrDefault(m => m.ManufacturerId == model.ManufacturerId)
                          ?? ctx.Manufacturers.First(m => m.Name == model.Manufacturer);
                e.CategoryId = cat.CategoryId;
                e.ManufacturerId = man.ManufacturerId;
                e.ShortName = model.ShortName;
                e.FullName = model.FullName;
                e.Description = model.Description;
                e.ImagePath = model.ImagePath;
                e.Photo = model.PhotoData;
                e.Price = model.Price;
                e.Quantity = model.Quantity;
                e.Rating = model.Rating;
                e.Discount = model.Discount;
                e.Color = model.Color;
                e.Size = model.Size;
                e.SoldCount = model.SoldCount;
                e.IsOutOfStock = model.IsOutOfStock;
                ctx.SaveChanges();
            }
        }

        public static void DeleteProduct(int productId)
        {
            using (var ctx = new ConfectioneryDbContext())
            {
                var e = ctx.Products.Include(p => p.Tags).FirstOrDefault(p => p.ProductId == productId);
                if (e == null) return;
                e.Tags.Clear();
                ctx.Products.Remove(e);
                ctx.SaveChanges();
            }
        }


        public static List<Product> SearchOneField(string term)
        {
            var t = (term ?? "").Trim();
            using (var ctx = new ConfectioneryDbContext())
            {
                var q = ctx.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.Manufacturer)
                    .Where(p => t == "" || p.ShortName.Contains(t) || (p.FullName != null && p.FullName.Contains(t))
                                || (p.Description != null && p.Description.Contains(t)));
                return q.OrderBy(p => p.ShortName).ToList().Select(MapToUi).ToList();
            }
        }


        public static List<Product> FilterAndSearchMulti(decimal? minPrice, string categoryPart, string spaceSeparatedWords)
        {
            using (var ctx = new ConfectioneryDbContext())
            {
                IQueryable<ProductEntity> q = ctx.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.Manufacturer);

                if (minPrice.HasValue)
                    q = q.Where(p => p.Price >= minPrice.Value);

                var cat = (categoryPart ?? "").Trim();
                if (cat.Length > 0)
                    q = q.Where(p => p.Category.Name.Contains(cat));

                var parts = (spaceSeparatedWords ?? "")
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0)
                    .ToList();

                foreach (var part in parts)
                {
                    var p1 = part;
                    q = q.Where(p =>
                        p.ShortName.Contains(p1)
                        || (p.FullName != null && p.FullName.Contains(p1))
                        || (p.Description != null && p.Description.Contains(p1)));
                }

                return q.OrderByDescending(p => p.Rating).ThenBy(p => p.ShortName).ToList().Select(MapToUi).ToList();
            }
        }

        public static List<Product> SortProducts(string sortKey)
        {
            using (var ctx = new ConfectioneryDbContext())
            {
                var q = ctx.Products.AsNoTracking().Include(p => p.Category).Include(p => p.Manufacturer);
                switch ((sortKey ?? "Name").Trim())
                {
                    case "Price":
                        return q.OrderBy(p => p.Price).ThenBy(p => p.ShortName).ToList().Select(MapToUi).ToList();
                    case "Rating":
                        return q.OrderByDescending(p => p.Rating).ThenBy(p => p.ShortName).ToList().Select(MapToUi).ToList();
                    default:
                        return q.OrderBy(p => p.ShortName).ToList().Select(MapToUi).ToList();
                }
            }
        }


        public static async Task<int> CountInStockInCategoryAsync(string categoryName)
        {
            using (var ctx = new ConfectioneryDbContext())
            {
                return await ctx.Products
                    .AsNoTracking()
                    .Where(p => p.Category.Name == categoryName && p.Quantity > 0)
                    .CountAsync()
                    .ConfigureAwait(false);
            }
        }


        public static string DemoTransactionRollback()
        {
            var marker = "__EF_LAB9_ROLLBACK__" + Guid.NewGuid().ToString("N").Substring(0, 6);
            using (var ctx = new ConfectioneryDbContext())
            using (var tran = ctx.Database.BeginTransaction())
            {
                try
                {
                    ctx.Categories.Add(new CategoryEntity { Name = marker });
                    ctx.SaveChanges();
                    tran.Rollback();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }

            using (var check = new ConfectioneryDbContext())
            {
                if (check.Categories.AsNoTracking().Any(c => c.Name == marker))
                    return "Ошибка демо: категория осталась после ROLLBACK.";
                return "Транзакция: INSERT категории выполнен и откатён — в БД записи «" + marker + "» нет.";
            }
        }


        public static string DemoCrudTagOnFirstProduct()
        {
            using (var ctx = new ConfectioneryDbContext())
            {
                var product = ctx.Products.Include(p => p.Tags).OrderBy(p => p.ProductId).FirstOrDefault();
                if (product == null) return "Нет товаров в БД.";

                var name = "__lab9_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                var tag = new TagEntity { Name = name };
                ctx.Tags.Add(tag);
                product.Tags.Add(tag);
                ctx.SaveChanges();

                var id = tag.TagId;
                var loaded = ctx.Tags.First(t => t.TagId == id);
                loaded.Name = name + "_edit";
                ctx.SaveChanges();

                product.Tags.Remove(loaded);
                ctx.Tags.Remove(loaded);
                ctx.SaveChanges();

                return "CRUD: метка создана, привязана к товару «" + product.ShortName + "», изменена и удалена.";
            }
        }
    }
}
