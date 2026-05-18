using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace ConfectioneryShop.Data
{
    /// <summary>
    /// Создание БД LocalDB и схемы при первом запуске (п. 8 ТЗ).
    /// </summary>
    public static class DatabaseInitializer
    {
        private const string MasterConnectionString =
            "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=15";

        public static string DatabaseName =>
            ConfigurationManager.AppSettings["DatabaseName"] ?? "ConfectioneryLab";

        public static string DataDirectory =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                ConfigurationManager.AppSettings["DataSubfolder"] ?? "Data");

        public static string WorkingConnectionString =>
            ConfigurationManager.ConnectionStrings["Confectionery"]?.ConnectionString
            ?? throw new InvalidOperationException("В App.config не задана строка подключения Confectionery.");

        public static void EnsureDatabaseCreated()
        {
            Directory.CreateDirectory(DataDirectory);
            var dbName = DatabaseName;
            var mdf = Path.Combine(DataDirectory, dbName + ".mdf");
            var ldf = Path.Combine(DataDirectory, dbName + "_log.ldf");

            using (var master = new SqlConnection(MasterConnectionString))
            {
                master.Open();
                int exists;
                using (var q = new SqlCommand("SELECT COUNT(*) FROM sys.databases WHERE name = @n", master))
                {
                    q.Parameters.AddWithValue("@n", dbName);
                    exists = Convert.ToInt32(q.ExecuteScalar());
                }

                if (exists == 0)
                {
                    var mdfSql = mdf.Replace("'", "''");
                    var ldfSql = ldf.Replace("'", "''");
                    var sql = $@"
CREATE DATABASE [{dbName}] ON PRIMARY
 (NAME = N'{dbName}_data', FILENAME = N'{mdfSql}')
 LOG ON
 (NAME = N'{dbName}_log', FILENAME = N'{ldfSql}');";
                    using (var cmd = new SqlCommand(sql, master))
                        cmd.ExecuteNonQuery();
                }
            }

            using (var conn = new SqlConnection(WorkingConnectionString))
            {
                conn.Open();
                if (!SchemaExists(conn))
                    CreateSchemaAndObjects(conn);
                EnsureTagTables(conn);
                SeedIfEmpty(conn);
                SeedTagsIfNeeded(conn);
            }
        }

        /// <summary>Таблицы для связи многие-ко-многим Product–Tag (лаб. 9, EF).</summary>
        private static void EnsureTagTables(SqlConnection conn)
        {
            Exec(conn, @"
IF OBJECT_ID(N'dbo.Tag', N'U') IS NULL
CREATE TABLE dbo.Tag (
    TagId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(80) NOT NULL
);");
            Exec(conn, @"
IF OBJECT_ID(N'dbo.ProductTag', N'U') IS NULL
CREATE TABLE dbo.ProductTag (
    ProductId INT NOT NULL,
    TagId INT NOT NULL,
    CONSTRAINT PK_ProductTag PRIMARY KEY (ProductId, TagId),
    CONSTRAINT FK_ProductTag_Product FOREIGN KEY (ProductId) REFERENCES dbo.Product(ProductId) ON DELETE CASCADE,
    CONSTRAINT FK_ProductTag_Tag FOREIGN KEY (TagId) REFERENCES dbo.Tag(TagId) ON DELETE CASCADE
);");
        }

        private static void SeedTagsIfNeeded(SqlConnection conn)
        {
            int tags;
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Tag", conn))
                tags = Convert.ToInt32(cmd.ExecuteScalar());
            if (tags > 0)
                return;

            int products;
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Product", conn))
                products = Convert.ToInt32(cmd.ExecuteScalar());
            if (products == 0)
                return;

            void insTag(string name)
            {
                using (var c = new SqlCommand("INSERT INTO dbo.Tag (Name) VALUES (@n)", conn))
                {
                    c.Parameters.AddWithValue("@n", name);
                    c.ExecuteNonQuery();
                }
            }

            insTag("Хит");
            insTag("Скидка");
            insTag("Ручная работа");

            int tagId(string name)
            {
                using (var c = new SqlCommand("SELECT TagId FROM dbo.Tag WHERE Name=@n", conn))
                {
                    c.Parameters.AddWithValue("@n", name);
                    return Convert.ToInt32(c.ExecuteScalar());
                }
            }

            int firstProductId()
            {
                using (var c = new SqlCommand("SELECT TOP 1 ProductId FROM dbo.Product ORDER BY ProductId", conn))
                    return Convert.ToInt32(c.ExecuteScalar());
            }

            void link(int productId, int tagIdVal)
            {
                using (var c = new SqlCommand(
                           "IF NOT EXISTS (SELECT 1 FROM dbo.ProductTag WHERE ProductId=@p AND TagId=@t) INSERT INTO dbo.ProductTag (ProductId, TagId) VALUES (@p,@t)",
                           conn))
                {
                    c.Parameters.AddWithValue("@p", productId);
                    c.Parameters.AddWithValue("@t", tagIdVal);
                    c.ExecuteNonQuery();
                }
            }

            var pid = firstProductId();
            link(pid, tagId("Хит"));
            link(pid, tagId("Ручная работа"));
        }

        private static bool SchemaExists(SqlConnection conn)
        {
            using (var cmd = new SqlCommand(
                       "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Product'",
                       conn))
            {
                return cmd.ExecuteScalar() != null;
            }
        }

        private static void Exec(SqlConnection conn, string sql)
        {
            using (var cmd = new SqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        private static void CreateSchemaAndObjects(SqlConnection conn)
        {
            Exec(conn, @"
CREATE TABLE dbo.Category (
    CategoryId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(120) NOT NULL UNIQUE,
    LastUpdated DATETIME2 NULL
);

CREATE TABLE dbo.Manufacturer (
    ManufacturerId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Country NVARCHAR(120) NULL,
    LogoImage VARBINARY(MAX) NULL
);

CREATE TABLE dbo.Product (
    ProductId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CategoryId INT NOT NULL CONSTRAINT FK_Product_Category REFERENCES dbo.Category(CategoryId),
    ManufacturerId INT NOT NULL CONSTRAINT FK_Product_Manufacturer REFERENCES dbo.Manufacturer(ManufacturerId),
    ShortName NVARCHAR(120) NOT NULL,
    FullName NVARCHAR(300) NULL,
    Description NVARCHAR(MAX) NULL,
    ImagePath NVARCHAR(500) NULL,
    Photo VARBINARY(MAX) NULL,
    Price DECIMAL(18,2) NOT NULL CONSTRAINT DF_Product_Price DEFAULT (0),
    Quantity INT NOT NULL CONSTRAINT DF_Product_Qty DEFAULT (0),
    Rating FLOAT NOT NULL CONSTRAINT DF_Product_Rating DEFAULT (0),
    Discount INT NOT NULL CONSTRAINT DF_Product_Discount DEFAULT (0),
    Color NVARCHAR(80) NULL,
    Size NVARCHAR(80) NULL,
    SoldCount INT NOT NULL CONSTRAINT DF_Product_Sold DEFAULT (0),
    IsOutOfStock BIT NOT NULL CONSTRAINT DF_Product_OOS DEFAULT (0)
);");

            Exec(conn, @"
CREATE TRIGGER dbo.trg_Product_OutOfStock
ON dbo.Product
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE p
    SET IsOutOfStock = CASE WHEN p.Quantity <= 0 THEN 1 ELSE 0 END
    FROM dbo.Product p
    INNER JOIN inserted i ON p.ProductId = i.ProductId;
END;");

            Exec(conn, @"
IF OBJECT_ID(N'dbo.usp_Products_Filter', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Products_Filter;");
            Exec(conn, @"
CREATE PROCEDURE dbo.usp_Products_Filter
    @CategoryPart NVARCHAR(120),
    @MinPrice DECIMAL(18,2)
AS
    SELECT p.ProductId, p.ShortName, p.FullName, p.Price, p.Quantity, c.Name AS CategoryName
    FROM dbo.Product p
    INNER JOIN dbo.Category c ON c.CategoryId = p.CategoryId
    WHERE (@CategoryPart IS NULL OR @CategoryPart = N'' OR c.Name LIKE N'%' + @CategoryPart + N'%')
      AND p.Price >= @MinPrice
    ORDER BY p.ShortName;");

            Exec(conn, @"
IF OBJECT_ID(N'dbo.usp_Category_ProductStats', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Category_ProductStats;");
            Exec(conn, @"
CREATE PROCEDURE dbo.usp_Category_ProductStats
AS
    SELECT c.CategoryId, c.Name AS CategoryName,
           COUNT(p.ProductId) AS ProductCount,
           ISNULL(SUM(p.Quantity), 0) AS TotalQuantity
    FROM dbo.Category c
    LEFT JOIN dbo.Product p ON p.CategoryId = c.CategoryId
    GROUP BY c.CategoryId, c.Name
    ORDER BY c.Name;");
        }

        private static void SeedIfEmpty(SqlConnection conn)
        {
            int n;
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Category", conn))
                n = Convert.ToInt32(cmd.ExecuteScalar());
            if (n > 0)
                return;

            void insCat(string name)
            {
                using (var c = new SqlCommand("INSERT INTO dbo.Category (Name) VALUES (@n)", conn))
                {
                    c.Parameters.AddWithValue("@n", name);
                    c.ExecuteNonQuery();
                }
            }

            void insMan(string name, string country)
            {
                using (var c = new SqlCommand("INSERT INTO dbo.Manufacturer (Name, Country) VALUES (@n,@c)", conn))
                {
                    c.Parameters.AddWithValue("@n", name);
                    c.Parameters.AddWithValue("@c", (object)country ?? DBNull.Value);
                    c.ExecuteNonQuery();
                }
            }

            insCat("Пирожные");
            insCat("Торты");
            insCat("Кексы");

            insMan("Французская кондитерская", "Франция");
            insMan("Минская кондитерская фабрика", "Беларусь");
            insMan("Слодыч", "Беларусь");
            insMan("Кондитерский комбинат", "Беларусь");
            insMan("Домашняя кондитерская", "Беларусь");

            int cat(string name)
            {
                using (var c = new SqlCommand("SELECT CategoryId FROM dbo.Category WHERE Name=@n", conn))
                {
                    c.Parameters.AddWithValue("@n", name);
                    return Convert.ToInt32(c.ExecuteScalar());
                }
            }

            int man(string name)
            {
                using (var c = new SqlCommand("SELECT ManufacturerId FROM dbo.Manufacturer WHERE Name=@n", conn))
                {
                    c.Parameters.AddWithValue("@n", name);
                    return Convert.ToInt32(c.ExecuteScalar());
                }
            }

            void insProduct(int cid, int mid, string sn, string fn, string desc, string img, decimal price,
                int qty, double rating, int discount, string color, string size, int sold, bool oos)
            {
                using (var c = new SqlCommand(@"
INSERT INTO dbo.Product (CategoryId, ManufacturerId, ShortName, FullName, Description, ImagePath, Photo,
    Price, Quantity, Rating, Discount, Color, Size, SoldCount, IsOutOfStock)
VALUES (@cid,@mid,@sn,@fn,@desc,@img,NULL,@price,@qty,@rating,@discount,@color,@size,@sold,@oos)", conn))
                {
                    c.Parameters.AddWithValue("@cid", cid);
                    c.Parameters.AddWithValue("@mid", mid);
                    c.Parameters.AddWithValue("@sn", sn);
                    c.Parameters.AddWithValue("@fn", fn);
                    c.Parameters.AddWithValue("@desc", (object)desc ?? DBNull.Value);
                    c.Parameters.AddWithValue("@img", (object)img ?? DBNull.Value);
                    c.Parameters.AddWithValue("@price", price);
                    c.Parameters.AddWithValue("@qty", qty);
                    c.Parameters.AddWithValue("@rating", rating);
                    c.Parameters.AddWithValue("@discount", discount);
                    c.Parameters.AddWithValue("@color", (object)color ?? DBNull.Value);
                    c.Parameters.AddWithValue("@size", (object)size ?? DBNull.Value);
                    c.Parameters.AddWithValue("@sold", sold);
                    c.Parameters.AddWithValue("@oos", oos);
                    c.ExecuteNonQuery();
                }
            }

            var c1 = cat("Пирожные");
            var c2 = cat("Торты");
            var c3 = cat("Кексы");
            var m1 = man("Французская кондитерская");
            var m2 = man("Минская кондитерская фабрика");
            var m3 = man("Слодыч");
            var m4 = man("Кондитерский комбинат");
            var m5 = man("Домашняя кондитерская");

            insProduct(c1, m1, "Макарон", "Макарон с малиной",
                "Нежное французское пирожное с малиновым кремом.", "macaron.jpg", 3.5m, 50, 4.8, 10, "Розовый", "5 см", 150, false);
            insProduct(c2, m2, "Торт Наполеон", "Торт Наполеон классический",
                "Слоеный торт с заварным кремом.", "napoleon.jpg", 28m, 15, 4.9, 0, "Золотистый", "1 кг", 320, false);
            insProduct(c1, m3, "Эклер", "Эклер с шоколадным кремом",
                "Заварное пирожное с шоколадным кремом.", "eclair.jpg", 2.8m, 30, 4.7, 5, "Коричневый", "10 см", 280, false);
            insProduct(c3, m4, "Кекс", "Кекс с изюмом",
                "Классический кекс с изюмом.", "cake.jpg", 2.2m, 0, 4.5, 0, "Бежевый", "100 г", 450, true);
            insProduct(c1, m5, "Безе", "Безе воздушное",
                "Легкое безе.", "meringue.jpg", 1.5m, 100, 4.6, 0, "Белый", "4 см", 620, false);
            insProduct(c2, m2, "Торт Красный бархат", "Торт Красный бархат с сырным кремом",
                "Бисквит с сырным кремом.", "redvelvet.jpg", 35m, 8, 5.0, 15, "Красный", "1.2 кг", 95, false);
        }
    }
}
