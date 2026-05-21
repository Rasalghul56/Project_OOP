using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ConfectioneryShop.Models;

namespace ConfectioneryShop.Data
{


    public static class ConfectioneryRepository
    {
        private static SqlConnection OpenConnection()
        {
            var cs = ConfigurationManager.ConnectionStrings["Confectionery"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Строка подключения Confectionery не найдена.");
            var c = new SqlConnection(cs);
            c.Open();
            return c;
        }

        public static List<Product> LoadProducts()
        {
            var list = new List<Product>();
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(@"
SELECT p.ProductId, p.ShortName, p.FullName, p.Description, p.ImagePath, p.Photo,
       c.Name AS CategoryName, p.Rating, p.Price, p.Quantity, p.Color, p.Size,
       m.Country, p.Discount, p.IsOutOfStock, p.SoldCount, m.Name AS ManufacturerName,
       p.CategoryId, p.ManufacturerId
FROM dbo.Product p
INNER JOIN dbo.Category c ON c.CategoryId = p.CategoryId
INNER JOIN dbo.Manufacturer m ON m.ManufacturerId = p.ManufacturerId
ORDER BY p.ShortName", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                    list.Add(ReadProduct(r));
            }

            return list;
        }

        public static async Task<List<Tuple<int, string>>> LoadCategoriesAsync()
        {
            var res = new List<Tuple<int, string>>();
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand("SELECT CategoryId, Name FROM dbo.Category ORDER BY Name", conn))
            using (var r = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await r.ReadAsync().ConfigureAwait(false))
                    res.Add(Tuple.Create(r.GetInt32(0), r.GetString(1)));
            }

            return res;
        }

        private static Product ReadProduct(SqlDataReader r)
        {
            byte[] photo = null;
            if (!r.IsDBNull(5))
            {
                var len = (int)r.GetBytes(5, 0, null, 0, 0);
                photo = new byte[len];
                r.GetBytes(5, 0, photo, 0, len);
            }

            return new Product
            {
                Id = r.GetInt32(0),
                ShortName = r.GetString(1),
                FullName = r.IsDBNull(2) ? "" : r.GetString(2),
                Description = r.IsDBNull(3) ? "" : r.GetString(3),
                ImagePath = r.IsDBNull(4) ? "" : r.GetString(4),
                PhotoData = photo,
                Category = r.GetString(6),
                Rating = r.GetDouble(7),
                Price = r.GetDecimal(8),
                Quantity = r.GetInt32(9),
                Color = r.IsDBNull(10) ? "" : r.GetString(10),
                Size = r.IsDBNull(11) ? "" : r.GetString(11),
                Country = r.IsDBNull(12) ? "" : r.GetString(12),
                Discount = r.GetInt32(13),
                IsOutOfStock = r.GetBoolean(14),
                SoldCount = r.GetInt32(15),
                Manufacturer = r.GetString(16),
                CategoryId = r.GetInt32(17),
                ManufacturerId = r.GetInt32(18)
            };
        }

        private static int EnsureCategory(SqlConnection conn, SqlTransaction tran, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "Прочее";
            using (var cmd = new SqlCommand("SELECT CategoryId FROM dbo.Category WHERE Name=@n", conn, tran))
            {
                cmd.Parameters.AddWithValue("@n", name.Trim());
                var o = cmd.ExecuteScalar();
                if (o != null && o != DBNull.Value)
                    return Convert.ToInt32(o);
            }

            using (var cmd = new SqlCommand("INSERT INTO dbo.Category (Name) OUTPUT INSERTED.CategoryId VALUES (@n)",
                       conn, tran))
            {
                cmd.Parameters.AddWithValue("@n", name.Trim());
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static int EnsureManufacturer(SqlConnection conn, SqlTransaction tran, string name, string country)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "Неизвестный";
            using (var cmd = new SqlCommand(
                       "SELECT ManufacturerId FROM dbo.Manufacturer WHERE Name=@n", conn, tran))
            {
                cmd.Parameters.AddWithValue("@n", name.Trim());
                var o = cmd.ExecuteScalar();
                if (o != null && o != DBNull.Value)
                    return Convert.ToInt32(o);
            }

            using (var cmd = new SqlCommand(
                       "INSERT INTO dbo.Manufacturer (Name, Country) OUTPUT INSERTED.ManufacturerId VALUES (@n,@c)",
                       conn, tran))
            {
                cmd.Parameters.AddWithValue("@n", name.Trim());
                cmd.Parameters.AddWithValue("@c", string.IsNullOrWhiteSpace(country) ? (object)DBNull.Value : country.Trim());
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        public static int InsertProduct(Product p)
        {
            using (var conn = OpenConnection())
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    var cid = EnsureCategory(conn, tran, p.Category);
                    var mid = EnsureManufacturer(conn, tran, p.Manufacturer, p.Country);
                    int newId;
                    using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Product (CategoryId, ManufacturerId, ShortName, FullName, Description, ImagePath, Photo,
    Price, Quantity, Rating, Discount, Color, Size, SoldCount, IsOutOfStock)
OUTPUT INSERTED.ProductId
VALUES (@cid,@mid,@sn,@fn,@desc,@img,@photo,@price,@qty,@rating,@discount,@color,@size,@sold,@oos)", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@cid", cid);
                        cmd.Parameters.AddWithValue("@mid", mid);
                        cmd.Parameters.AddWithValue("@sn", p.ShortName ?? "");
                        cmd.Parameters.AddWithValue("@fn", (object)p.FullName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@desc", (object)p.Description ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@img", string.IsNullOrWhiteSpace(p.ImagePath) ? (object)DBNull.Value : p.ImagePath);
                        cmd.Parameters.Add("@photo", SqlDbType.VarBinary, -1).Value =
                            p.PhotoData != null && p.PhotoData.Length > 0 ? (object)p.PhotoData : DBNull.Value;
                        cmd.Parameters.AddWithValue("@price", p.Price);
                        cmd.Parameters.AddWithValue("@qty", p.Quantity);
                        cmd.Parameters.AddWithValue("@rating", p.Rating);
                        cmd.Parameters.AddWithValue("@discount", p.Discount);
                        cmd.Parameters.AddWithValue("@color", (object)p.Color ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@size", (object)p.Size ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@sold", p.SoldCount);
                        cmd.Parameters.AddWithValue("@oos", p.IsOutOfStock);
                        newId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    using (var up = new SqlCommand("UPDATE dbo.Category SET LastUpdated = SYSUTCDATETIME() WHERE CategoryId=@id",
                               conn, tran))
                    {
                        up.Parameters.AddWithValue("@id", cid);
                        up.ExecuteNonQuery();
                    }

                    tran.Commit();
                    p.CategoryId = cid;
                    p.ManufacturerId = mid;
                    return newId;
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }


        public static void UpdateProduct(Product p)
        {
            using (var conn = OpenConnection())
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    var cid = EnsureCategory(conn, tran, p.Category);
                    var mid = EnsureManufacturer(conn, tran, p.Manufacturer, p.Country);
                    using (var cmd = new SqlCommand(@"
UPDATE dbo.Product SET
    CategoryId=@cid, ManufacturerId=@mid, ShortName=@sn, FullName=@fn, Description=@desc,
    ImagePath=@img, Photo=@photo, Price=@price, Quantity=@qty, Rating=@rating, Discount=@discount,
    Color=@color, Size=@size, SoldCount=@sold, IsOutOfStock=@oos
WHERE ProductId=@id", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@id", p.Id);
                        cmd.Parameters.AddWithValue("@cid", cid);
                        cmd.Parameters.AddWithValue("@mid", mid);
                        cmd.Parameters.AddWithValue("@sn", p.ShortName ?? "");
                        cmd.Parameters.AddWithValue("@fn", (object)p.FullName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@desc", (object)p.Description ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@img", string.IsNullOrWhiteSpace(p.ImagePath) ? (object)DBNull.Value : p.ImagePath);
                        cmd.Parameters.Add("@photo", SqlDbType.VarBinary, -1).Value =
                            p.PhotoData != null && p.PhotoData.Length > 0 ? (object)p.PhotoData : DBNull.Value;
                        cmd.Parameters.AddWithValue("@price", p.Price);
                        cmd.Parameters.AddWithValue("@qty", p.Quantity);
                        cmd.Parameters.AddWithValue("@rating", p.Rating);
                        cmd.Parameters.AddWithValue("@discount", p.Discount);
                        cmd.Parameters.AddWithValue("@color", (object)p.Color ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@size", (object)p.Size ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@sold", p.SoldCount);
                        cmd.Parameters.AddWithValue("@oos", p.IsOutOfStock);
                        cmd.ExecuteNonQuery();
                    }

                    using (var up = new SqlCommand("UPDATE dbo.Category SET LastUpdated = SYSUTCDATETIME() WHERE CategoryId=@id",
                               conn, tran))
                    {
                        up.Parameters.AddWithValue("@id", cid);
                        up.ExecuteNonQuery();
                    }

                    tran.Commit();
                    p.CategoryId = cid;
                    p.ManufacturerId = mid;
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        public static void DeleteProduct(int productId)
        {
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand("DELETE FROM dbo.Product WHERE ProductId=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", productId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateProductStock(int productId, int quantity, bool isOutOfStock)
        {
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(
                       "UPDATE dbo.Product SET Quantity=@q, IsOutOfStock=@o WHERE ProductId=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", productId);
                cmd.Parameters.AddWithValue("@q", quantity);
                cmd.Parameters.AddWithValue("@o", isOutOfStock);
                cmd.ExecuteNonQuery();
            }
        }


        public static void SyncProductsWithList(List<Product> products)
        {
            using (var conn = OpenConnection())
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    var idsMem = new HashSet<int>(products.Select(x => x.Id));
                    List<int> dbIds;
                    using (var q = new SqlCommand("SELECT ProductId FROM dbo.Product", conn, tran))
                    using (var r = q.ExecuteReader())
                    {
                        dbIds = new List<int>();
                        while (r.Read())
                            dbIds.Add(r.GetInt32(0));
                    }

                    foreach (var id in dbIds.Where(id => !idsMem.Contains(id)))
                    {
                        using (var del = new SqlCommand("DELETE FROM dbo.Product WHERE ProductId=@id", conn, tran))
                        {
                            del.Parameters.AddWithValue("@id", id);
                            del.ExecuteNonQuery();
                        }
                    }

                    foreach (var p in products)
                        UpsertProductCore(conn, tran, p);

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        private static void UpsertProductCore(SqlConnection conn, SqlTransaction tran, Product p)
        {
            var cid = EnsureCategory(conn, tran, p.Category);
            var mid = EnsureManufacturer(conn, tran, p.Manufacturer, p.Country);
            int exists;
            using (var chk = new SqlCommand("SELECT COUNT(*) FROM dbo.Product WHERE ProductId=@id", conn, tran))
            {
                chk.Parameters.AddWithValue("@id", p.Id);
                exists = Convert.ToInt32(chk.ExecuteScalar());
            }

            if (exists == 0)
            {
                if (p.Id <= 0)
                {
                    using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Product (CategoryId, ManufacturerId, ShortName, FullName, Description, ImagePath, Photo,
    Price, Quantity, Rating, Discount, Color, Size, SoldCount, IsOutOfStock)
VALUES (@cid,@mid,@sn,@fn,@desc,@img,@photo,@price,@qty,@rating,@discount,@color,@size,@sold,@oos);
SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@cid", cid);
                        cmd.Parameters.AddWithValue("@mid", mid);
                        cmd.Parameters.AddWithValue("@sn", p.ShortName ?? "");
                        cmd.Parameters.AddWithValue("@fn", (object)p.FullName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@desc", (object)p.Description ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@img", string.IsNullOrWhiteSpace(p.ImagePath) ? (object)DBNull.Value : p.ImagePath);
                        cmd.Parameters.Add("@photo", SqlDbType.VarBinary, -1).Value =
                            p.PhotoData != null && p.PhotoData.Length > 0 ? (object)p.PhotoData : DBNull.Value;
                        cmd.Parameters.AddWithValue("@price", p.Price);
                        cmd.Parameters.AddWithValue("@qty", p.Quantity);
                        cmd.Parameters.AddWithValue("@rating", p.Rating);
                        cmd.Parameters.AddWithValue("@discount", p.Discount);
                        cmd.Parameters.AddWithValue("@color", (object)p.Color ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@size", (object)p.Size ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@sold", p.SoldCount);
                        cmd.Parameters.AddWithValue("@oos", p.IsOutOfStock);
                        p.Id = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                else
                {
                    using (var on = new SqlCommand("SET IDENTITY_INSERT dbo.Product ON", conn, tran))
                        on.ExecuteNonQuery();
                    try
                    {
                        using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Product (ProductId, CategoryId, ManufacturerId, ShortName, FullName, Description, ImagePath, Photo,
    Price, Quantity, Rating, Discount, Color, Size, SoldCount, IsOutOfStock)
VALUES (@id,@cid,@mid,@sn,@fn,@desc,@img,@photo,@price,@qty,@rating,@discount,@color,@size,@sold,@oos)", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", p.Id);
                            cmd.Parameters.AddWithValue("@cid", cid);
                            cmd.Parameters.AddWithValue("@mid", mid);
                            cmd.Parameters.AddWithValue("@sn", p.ShortName ?? "");
                            cmd.Parameters.AddWithValue("@fn", (object)p.FullName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@desc", (object)p.Description ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@img", string.IsNullOrWhiteSpace(p.ImagePath) ? (object)DBNull.Value : p.ImagePath);
                            cmd.Parameters.Add("@photo", SqlDbType.VarBinary, -1).Value =
                                p.PhotoData != null && p.PhotoData.Length > 0 ? (object)p.PhotoData : DBNull.Value;
                            cmd.Parameters.AddWithValue("@price", p.Price);
                            cmd.Parameters.AddWithValue("@qty", p.Quantity);
                            cmd.Parameters.AddWithValue("@rating", p.Rating);
                            cmd.Parameters.AddWithValue("@discount", p.Discount);
                            cmd.Parameters.AddWithValue("@color", (object)p.Color ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@size", (object)p.Size ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@sold", p.SoldCount);
                            cmd.Parameters.AddWithValue("@oos", p.IsOutOfStock);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    finally
                    {
                        using (var off = new SqlCommand("SET IDENTITY_INSERT dbo.Product OFF", conn, tran))
                            off.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using (var cmd = new SqlCommand(@"
UPDATE dbo.Product SET CategoryId=@cid, ManufacturerId=@mid, ShortName=@sn, FullName=@fn, Description=@desc,
    ImagePath=@img, Photo=@photo, Price=@price, Quantity=@qty, Rating=@rating, Discount=@discount,
    Color=@color, Size=@size, SoldCount=@sold, IsOutOfStock=@oos
WHERE ProductId=@id", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@id", p.Id);
                    cmd.Parameters.AddWithValue("@cid", cid);
                    cmd.Parameters.AddWithValue("@mid", mid);
                    cmd.Parameters.AddWithValue("@sn", p.ShortName ?? "");
                    cmd.Parameters.AddWithValue("@fn", (object)p.FullName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@desc", (object)p.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@img", string.IsNullOrWhiteSpace(p.ImagePath) ? (object)DBNull.Value : p.ImagePath);
                    cmd.Parameters.Add("@photo", SqlDbType.VarBinary, -1).Value =
                        p.PhotoData != null && p.PhotoData.Length > 0 ? (object)p.PhotoData : DBNull.Value;
                    cmd.Parameters.AddWithValue("@price", p.Price);
                    cmd.Parameters.AddWithValue("@qty", p.Quantity);
                    cmd.Parameters.AddWithValue("@rating", p.Rating);
                    cmd.Parameters.AddWithValue("@discount", p.Discount);
                    cmd.Parameters.AddWithValue("@color", (object)p.Color ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@size", (object)p.Size ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@sold", p.SoldCount);
                    cmd.Parameters.AddWithValue("@oos", p.IsOutOfStock);
                    cmd.ExecuteNonQuery();
                }
            }

            p.CategoryId = cid;
            p.ManufacturerId = mid;
        }


        public static DataTable SearchProductsByMinPrice(decimal minPrice, string categoryPart)
        {
            var dt = new DataTable();
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(@"
SELECT p.ProductId, p.ShortName, p.Price, c.Name AS CategoryName
FROM dbo.Product p
INNER JOIN dbo.Category c ON c.CategoryId = p.CategoryId
WHERE p.Price >= @minPrice
  AND (@cat IS NULL OR @cat = N'' OR c.Name LIKE N'%' + @cat + N'%')
ORDER BY p.Price", conn))
            {
                cmd.Parameters.AddWithValue("@minPrice", minPrice);
                cmd.Parameters.AddWithValue("@cat", (object)categoryPart ?? DBNull.Value);
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }

            return dt;
        }


        public static DataTable ExecUspProductsFilter(string categoryPart, decimal minPrice)
        {
            var dt = new DataTable();
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand("dbo.usp_Products_Filter", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CategoryPart", (object)categoryPart ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MinPrice", minPrice);
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }

            return dt;
        }


        public static async Task<DataTable> ExecUspCategoryStatsAsync()
        {
            var dt = new DataTable();
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand("dbo.usp_Category_ProductStats", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using (var r = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    dt.Load(r);
            }

            return dt;
        }

        public static DataTable LoadCategoriesTable()
        {
            var dt = new DataTable();
            using (var conn = OpenConnection())
            using (var da = new SqlDataAdapter("SELECT CategoryId, Name, LastUpdated FROM dbo.Category ORDER BY Name", conn))
                da.Fill(dt);
            return dt;
        }

        public static DataTable LoadManufacturersTable()
        {
            var dt = new DataTable();
            using (var conn = OpenConnection())
            using (var da = new SqlDataAdapter(
                       "SELECT ManufacturerId, Name, Country, CASE WHEN LogoImage IS NULL THEN 0 ELSE 1 END AS HasLogo FROM dbo.Manufacturer ORDER BY Name",
                       conn))
                da.Fill(dt);
            return dt;
        }

        public static DataTable LoadProductsTable()
        {
            var dt = new DataTable();
            using (var conn = OpenConnection())
            using (var da = new SqlDataAdapter(@"
SELECT p.ProductId, p.ShortName, p.FullName, p.Price, p.Quantity, c.Name AS Category, m.Name AS Manufacturer
FROM dbo.Product p
JOIN dbo.Category c ON c.CategoryId=p.CategoryId
JOIN dbo.Manufacturer m ON m.ManufacturerId=p.ManufacturerId
ORDER BY p.ShortName", conn))
                da.Fill(dt);
            return dt;
        }

        public static void DeleteCategory(int categoryId)
        {
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand("DELETE FROM dbo.Category WHERE CategoryId=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", categoryId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void InsertCategory(string name)
        {
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand("INSERT INTO dbo.Category (Name) VALUES (@n)", conn))
            {
                cmd.Parameters.AddWithValue("@n", name);
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteManufacturer(int manufacturerId)
        {
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand("DELETE FROM dbo.Manufacturer WHERE ManufacturerId=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", manufacturerId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void InsertManufacturer(string name, string country)
        {
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand("INSERT INTO dbo.Manufacturer (Name, Country) VALUES (@n,@c)", conn))
            {
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@c", string.IsNullOrWhiteSpace(country) ? (object)DBNull.Value : country);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
