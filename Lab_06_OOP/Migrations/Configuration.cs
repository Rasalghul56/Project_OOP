namespace Confectionery.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Confectionery.Helpers;
    using Confectionery.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<Confectionery.Data.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "Confectionery.Data.AppDbContext";
        }

        protected override void Seed(Confectionery.Data.AppDbContext ctx)
        {
            // ── Пользователи ──────────────────────────────────────────────────
            ctx.Users.AddOrUpdate(u => u.Email,
                new User { Name = "Администратор",  Email = "admin@mail.ru",        PasswordHash = PasswordHelper.Hash("admin"),      Role = "Admin",  Phone = "+375291234567" },
                new User { Name = "Анна Ковалёва",  Email = "anna@mail.ru",         PasswordHash = PasswordHelper.Hash("anna123"),    Role = "Client", Phone = "+375291112233" },
                new User { Name = "Иван Петров",    Email = "ivan@mail.ru",         PasswordHash = PasswordHelper.Hash("ivan123"),    Role = "Client", Phone = "+375441234567" },
                new User { Name = "Мария Сидорова", Email = "maria@mail.ru",        PasswordHash = PasswordHelper.Hash("maria123"),   Role = "Client", Phone = "+375339876543" },
                new User { Name = "Дарья Кузьмина", Email = "darya@mail.ru",        PasswordHash = PasswordHelper.Hash("darya123"),  Role = "Client", Phone = "+375297654321" },
                new User { Name = "Алексей Борисов",Email = "alexey@mail.ru",       PasswordHash = PasswordHelper.Hash("alex123"),    Role = "Client", Phone = "+375443334455" }
            );
            ctx.SaveChanges();

            // ── Категории ─────────────────────────────────────────────────────
            ctx.Categories.AddOrUpdate(c => c.Name,
                new Category { Name = "Торты"    },
                new Category { Name = "Пирожные" },
                new Category { Name = "Печенье"  },
                new Category { Name = "Конфеты"  },
                new Category { Name = "Зефир"    },
                new Category { Name = "Шоколад"  }
            );
            ctx.SaveChanges();

            // Получаем ID категорий по имени
            var cats = ctx.Categories.ToDictionary(c => c.Name, c => c.Id);

            // ── Товары ────────────────────────────────────────────────────────
            ctx.Products.AddOrUpdate(p => p.Name,
                // Торты
                new Product {
                    Name = "Торт «Наполеон»",
                    Description = "Классический многослойный торт с нежным заварным кремом. Тонкие хрустящие коржи пропитываются кремом в течение ночи — результат всегда безупречен.",
                    Composition = "Мука пшеничная, масло сливочное 82%, молоко цельное, яйца, сахар-рафинад, ванилин",
                    Weight = 1.5, Price = 45.00m, CategoryId = cats["Торты"], IsAvailable = true
                },
                new Product {
                    Name = "Торт «Медовик»",
                    Description = "Душистые медовые коржи и нежный сметанный крем — сочетание, проверенное временем. Торт выстаивается 12 часов для идеальной пропитки.",
                    Composition = "Мёд натуральный, мука, масло сливочное, сметана 25%, сахар, яйца, сода",
                    Weight = 1.2, Price = 38.00m, CategoryId = cats["Торты"], IsAvailable = true
                },
                new Product {
                    Name = "Торт «Красный бархат»",
                    Description = "Эффектный американский торт с бархатистым бисквитом и воздушным кремом из сыра. Идеален для особых случаев.",
                    Composition = "Мука, кефир, масло растительное, краситель пищевой, сливочный сыр, масло, сахарная пудра",
                    Weight = 1.4, Price = 52.00m, CategoryId = cats["Торты"], IsAvailable = true
                },
                new Product {
                    Name = "Торт «Прага»",
                    Description = "Советская классика с насыщенным шоколадным бисквитом, сливочно-масляным кремом и шоколадной глазурью.",
                    Composition = "Шоколад тёмный 72%, мука, яйца, масло, сметана, какао, сгущённое молоко",
                    Weight = 1.3, Price = 48.00m, CategoryId = cats["Торты"], IsAvailable = true
                },
                new Product {
                    Name = "Торт «Птичье молоко»",
                    Description = "Нежнейший суфле-торт на агар-агаре с тонкими бисквитными коржами и шоколадной глазурью. ГОСТ-рецептура.",
                    Composition = "Яичные белки, сахар, агар-агар, масло сливочное, сгущёнка, шоколад, бисквит",
                    Weight = 1.0, Price = 42.00m, CategoryId = cats["Торты"], IsAvailable = true
                },
                new Product {
                    Name = "Торт «Захер»",
                    Description = "Венская легенда: плотный шоколадный бисквит с абрикосовым джемом, покрытый тёмной ганашевой глазурью.",
                    Composition = "Шоколад 55%, масло, яйца, мука, сахар, джем абрикосовый, сливки",
                    Weight = 1.1, Price = 55.00m, CategoryId = cats["Торты"], IsAvailable = true
                },
                new Product {
                    Name = "Морковный торт",
                    Description = "Ароматный влажный бисквит с морковью, грецкими орехами и нежным кремом из маскарпоне с ноткой цитруса.",
                    Composition = "Морковь свежая, мука, масло растительное, яйца, сахар, орехи грецкие, маскарпоне, цедра апельсина",
                    Weight = 1.2, Price = 47.00m, CategoryId = cats["Торты"], IsAvailable = true
                },

                // Пирожные
                new Product {
                    Name = "Эклер классический",
                    Description = "Французское заварное пирожное с ванильным кремом «Дипломат» и зеркальной шоколадной глазурью. Изготавливается в день продажи.",
                    Composition = "Заварное тесто, масло, яйца, крем патисьер, шоколад, сливки 33%",
                    Weight = 0.10, Price = 5.50m, CategoryId = cats["Пирожные"], IsAvailable = true
                },
                new Product {
                    Name = "Макарон ассорти (6 шт.)",
                    Description = "Шесть французских миндальных пирожных с разными начинками: малина, ваниль, фисташка, шоколад, лимон, карамель.",
                    Composition = "Миндальная мука, яичный белок, сахарная пудра, пищевые красители, ганаш, конфитюр",
                    Weight = 0.09, Price = 12.50m, CategoryId = cats["Пирожные"], IsAvailable = true
                },
                new Product {
                    Name = "Тирамису",
                    Description = "Итальянский десерт в исполнении нашего кондитера: печенье савоярди, пропитанное крепким эспрессо, с воздушным кремом из маскарпоне.",
                    Composition = "Маскарпоне, яйца, савоярди, эспрессо, сахар, какао Barry",
                    Weight = 0.25, Price = 8.50m, CategoryId = cats["Пирожные"], IsAvailable = true
                },
                new Product {
                    Name = "Чизкейк «Нью-Йорк»",
                    Description = "Нежный сливочный чизкейк на хрустящей основе из крекеров. Классический рецепт без лишних добавок — только сыр, сливки и ваниль.",
                    Composition = "Сыр сливочный, сливки 33%, яйца, сахар, крекеры, масло, ваниль",
                    Weight = 0.30, Price = 9.00m, CategoryId = cats["Пирожные"], IsAvailable = true
                },
                new Product {
                    Name = "Профитроли с кремом",
                    Description = "Нежные заварные шарики, наполненные кремом шантийи и покрытые шоколадным соусом. Порция — 5 штук.",
                    Composition = "Заварное тесто, сливки 35%, сахарная пудра, ваниль, шоколад",
                    Weight = 0.15, Price = 7.00m, CategoryId = cats["Пирожные"], IsAvailable = true
                },
                new Product {
                    Name = "Пирожное «Картошка»",
                    Description = "Классическое советское пирожное из бисквитной крошки со сливочным кремом, в виде небольшой «картошки».",
                    Composition = "Бисквит, масло сливочное, сгущёнка, какао, коньяк, вафельная крошка",
                    Weight = 0.12, Price = 4.50m, CategoryId = cats["Пирожные"], IsAvailable = true
                },

                // Печенье
                new Product {
                    Name = "Печенье «Домашнее»",
                    Description = "Рассыпчатое сливочное печенье по старинному рецепту. Каждое изделие лепится вручную. Упаковка 500 г.",
                    Composition = "Мука высший сорт, масло сливочное 82%, сахарная пудра, яйца, ваниль",
                    Weight = 0.50, Price = 12.00m, CategoryId = cats["Печенье"], IsAvailable = true
                },
                new Product {
                    Name = "Овсяное печенье с изюмом",
                    Description = "Полезное и вкусное овсяное печенье с крупным изюмом. Выпекается без добавления разрыхлителей. Упаковка 400 г.",
                    Composition = "Овсяные хлопья, мука, масло, сахар тростниковый, яйца, изюм султана, корица",
                    Weight = 0.40, Price = 10.00m, CategoryId = cats["Печенье"], IsAvailable = true
                },
                new Product {
                    Name = "Имбирное печенье (набор 12 шт.)",
                    Description = "Ароматное имбирное печенье в форме снежинок и сердечек, украшенное сахарной глазурью вручную.",
                    Composition = "Мука, масло, сахар, мёд, имбирь молотый, корица, гвоздика, яйца, сахарная глазурь",
                    Weight = 0.30, Price = 14.00m, CategoryId = cats["Печенье"], IsAvailable = true
                },
                new Product {
                    Name = "Брауни шоколадный",
                    Description = "Плотный влажный американский брауни с хрустящей корочкой и тянущейся серединой. Порция 2 штуки.",
                    Composition = "Шоколад 70%, масло сливочное, сахар, яйца, мука, орехи пекан",
                    Weight = 0.18, Price = 8.00m, CategoryId = cats["Печенье"], IsAvailable = true
                },

                // Конфеты
                new Product {
                    Name = "Трюфели из бельгийского шоколада (12 шт.)",
                    Description = "Ручные трюфели из шоколада Callebaut: ганаш тёмный, молочный, белый — по 4 штуки каждого. В элегантной коробке.",
                    Composition = "Шоколад Callebaut 54%, сливки 35%, масло, какао, сахар",
                    Weight = 0.20, Price = 18.00m, CategoryId = cats["Конфеты"], IsAvailable = true
                },
                new Product {
                    Name = "Конфеты «Пралине» (10 шт.)",
                    Description = "Шоколадные конфеты с начинкой из фундукового пралине. Каждая конфета ручной лепки, украшена орехом.",
                    Composition = "Шоколад молочный, фундук, сахар, сливки, масло какао",
                    Weight = 0.15, Price = 15.00m, CategoryId = cats["Конфеты"], IsAvailable = true
                },
                new Product {
                    Name = "Мармелад ягодный (200 г)",
                    Description = "Натуральный мармелад из свежих ягод без красителей и консервантов. Малина, черника, клубника.",
                    Composition = "Ягодное пюре, сахар, пектин яблочный, лимонный сок",
                    Weight = 0.20, Price = 9.50m, CategoryId = cats["Конфеты"], IsAvailable = true
                },

                // Зефир
                new Product {
                    Name = "Зефир ванильный (6 шт.)",
                    Description = "Воздушный домашний зефир из яблочного пюре. Нежная текстура, натуральный аромат ванили. Без искусственных красителей.",
                    Composition = "Яблочное пюре, яичный белок, сахар, агар-агар, ваниль",
                    Weight = 0.24, Price = 8.50m, CategoryId = cats["Зефир"], IsAvailable = true
                },
                new Product {
                    Name = "Зефир клубничный (6 шт.)",
                    Description = "Розовый зефир с натуральным клубничным пюре. Нежный, тающий, с ярким ягодным вкусом.",
                    Composition = "Клубничное пюре, яичный белок, сахар, агар-агар",
                    Weight = 0.24, Price = 9.00m, CategoryId = cats["Зефир"], IsAvailable = true
                },
                new Product {
                    Name = "Зефир в шоколаде (8 шт.)",
                    Description = "Классический ванильный зефир, покрытый тонким слоем тёмного бельгийского шоколада. Контраст вкусов и текстур.",
                    Composition = "Яблочное пюре, белок, агар, шоколад тёмный Callebaut, сахар",
                    Weight = 0.28, Price = 11.00m, CategoryId = cats["Зефир"], IsAvailable = true
                },

                // Шоколад
                new Product {
                    Name = "Плитка «Горький шоколад 72%»",
                    Description = "Авторская плитка из какао-бобов Tanzania с нотами сухофруктов и терпкостью. Для истинных ценителей тёмного шоколада.",
                    Composition = "Какао-масса, какао-масло, сахар. Содержание какао ≥ 72%",
                    Weight = 0.10, Price = 7.50m, CategoryId = cats["Шоколад"], IsAvailable = true
                },
                new Product {
                    Name = "Плитка «Молочный шоколад с фундуком»",
                    Description = "Нежный молочный шоколад с крупными кусочками поджаренного фундука. Идеально к кофе или чаю.",
                    Composition = "Какао-масло, молоко сухое цельное, сахар, фундук, лецитин",
                    Weight = 0.10, Price = 6.50m, CategoryId = cats["Шоколад"], IsAvailable = true
                }
            );
            ctx.SaveChanges();

            // ── Отзывы ────────────────────────────────────────────────────────
            // Получаем пользователей и товары из БД
            var anna    = ctx.Users.FirstOrDefault(u => u.Email == "anna@mail.ru");
            var ivan    = ctx.Users.FirstOrDefault(u => u.Email == "ivan@mail.ru");
            var maria   = ctx.Users.FirstOrDefault(u => u.Email == "maria@mail.ru");
            var darya   = ctx.Users.FirstOrDefault(u => u.Email == "darya@mail.ru");
            var alexey  = ctx.Users.FirstOrDefault(u => u.Email == "alexey@mail.ru");

            var napoleon = ctx.Products.FirstOrDefault(p => p.Name == "Торт «Наполеон»");
            var honey    = ctx.Products.FirstOrDefault(p => p.Name == "Торт «Медовик»");
            var eclair   = ctx.Products.FirstOrDefault(p => p.Name == "Эклер классический");
            var macaroon = ctx.Products.FirstOrDefault(p => p.Name == "Макарон ассорти (6 шт.)");
            var tiramisu = ctx.Products.FirstOrDefault(p => p.Name == "Тирамису");
            var zefir    = ctx.Products.FirstOrDefault(p => p.Name == "Зефир ванильный (6 шт.)");
            var truffle  = ctx.Products.FirstOrDefault(p => p.Name == "Трюфели из бельгийского шоколада (12 шт.)");
            var cheesecake = ctx.Products.FirstOrDefault(p => p.Name == "Чизкейк «Нью-Йорк»");

            if (anna != null && napoleon != null && !ctx.Reviews.Any(r => r.UserId == anna.Id && r.ProductId == napoleon.Id))
                ctx.Reviews.Add(new Review { UserId = anna.Id, ProductId = napoleon.Id, Rating = 5, Text = "Торт просто объедение! Коржи нежнейшие, крем ароматный. Заказываю уже в третий раз — результат всегда стабильный. Спасибо мастерам!", CreatedAt = DateTime.Now.AddDays(-15) });

            if (ivan != null && napoleon != null && !ctx.Reviews.Any(r => r.UserId == ivan.Id && r.ProductId == napoleon.Id))
                ctx.Reviews.Add(new Review { UserId = ivan.Id, ProductId = napoleon.Id, Rating = 5, Text = "Лучший Наполеон, что я пробовал. Заказал на день рождения мамы — все гости в восторге. Упаковка красивая, торт доехал целым.", CreatedAt = DateTime.Now.AddDays(-8), AdminReply = "Спасибо за тёплые слова! Поздравляем маму с днём рождения! 🎂" });

            if (maria != null && honey != null && !ctx.Reviews.Any(r => r.UserId == maria.Id && r.ProductId == honey.Id))
                ctx.Reviews.Add(new Review { UserId = maria.Id, ProductId = honey.Id, Rating = 4, Text = "Медовик очень вкусный, чувствуется натуральный мёд. Немного сладковат для меня, но это дело вкуса. Точно закажу снова!", CreatedAt = DateTime.Now.AddDays(-20) });

            if (anna != null && eclair != null && !ctx.Reviews.Any(r => r.UserId == anna.Id && r.ProductId == eclair.Id))
                ctx.Reviews.Add(new Review { UserId = anna.Id, ProductId = eclair.Id, Rating = 5, Text = "Эклеры свежайшие, тесто хрустящее, крем воздушный. Взяла сразу 10 штук — разлетелись моментально! Буду брать регулярно.", CreatedAt = DateTime.Now.AddDays(-5) });

            if (darya != null && macaroon != null && !ctx.Reviews.Any(r => r.UserId == darya.Id && r.ProductId == macaroon.Id))
                ctx.Reviews.Add(new Review { UserId = darya.Id, ProductId = macaroon.Id, Rating = 5, Text = "Макароны просто потрясающие! Каждый вкус уникален. Особенно понравились фисташковый и малиновый. Красиво упакованы — отличный подарок.", CreatedAt = DateTime.Now.AddDays(-3), AdminReply = "Благодарим за отзыв! Рады, что вам понравилось наше ассорти. Ждём вас снова! 🌸" });

            if (alexey != null && tiramisu != null && !ctx.Reviews.Any(r => r.UserId == alexey.Id && r.ProductId == tiramisu.Id))
                ctx.Reviews.Add(new Review { UserId = alexey.Id, ProductId = tiramisu.Id, Rating = 4, Text = "Тирамису на высоте — маскарпоне качественный, кофейная пропитка не перебивает нежность крема. Свежий, не сухой. 4 звезды только потому что хотелось большей порции.", CreatedAt = DateTime.Now.AddDays(-12) });

            if (maria != null && zefir != null && !ctx.Reviews.Any(r => r.UserId == maria.Id && r.ProductId == zefir.Id))
                ctx.Reviews.Add(new Review { UserId = maria.Id, ProductId = zefir.Id, Rating = 5, Text = "Зефир нежнейший! Муж теперь требует каждую неделю. Дети едят вместо конфет. Вкус настоящий, домашний — чувствуется что без химии.", CreatedAt = DateTime.Now.AddDays(-7) });

            if (ivan != null && truffle != null && !ctx.Reviews.Any(r => r.UserId == ivan.Id && r.ProductId == truffle.Id))
                ctx.Reviews.Add(new Review { UserId = ivan.Id, ProductId = truffle.Id, Rating = 5, Text = "Трюфели — лучший подарок! Подарил коллегам на 8 марта. Качество шоколада чувствуется сразу. Коробка красивая, не стыдно дарить. Закажу ещё на Новый год!", CreatedAt = DateTime.Now.AddDays(-25) });

            if (darya != null && cheesecake != null && !ctx.Reviews.Any(r => r.UserId == darya.Id && r.ProductId == cheesecake.Id))
                ctx.Reviews.Add(new Review { UserId = darya.Id, ProductId = cheesecake.Id, Rating = 5, Text = "Чизкейк — это что-то с чем-то! Нежная начинка, хрустящая основа, идеально сбалансированная сладость. Обожаю нью-йоркский стиль. Точно войдёт в мой топ.", CreatedAt = DateTime.Now.AddDays(-2) });

            ctx.SaveChanges();

            // ── Заказы (примеры для демонстрации) ────────────────────────────
            if (!ctx.Orders.Any() && anna != null && napoleon != null)
            {
                var praga     = ctx.Products.FirstOrDefault(p => p.Name == "Торт «Прага»");
                var brownie   = ctx.Products.FirstOrDefault(p => p.Name == "Брауни шоколадный");
                var ovsyanoe  = ctx.Products.FirstOrDefault(p => p.Name == "Овсяное печенье с изюмом");
                var zefirChoc = ctx.Products.FirstOrDefault(p => p.Name == "Зефир в шоколаде (8 шт.)");
                var praline   = ctx.Products.FirstOrDefault(p => p.Name == "Конфеты «Пралине» (10 шт.)");

                var orders = new List<Order>
                {
                    new Order {
                        UserId       = anna.Id,
                        OrderNumber  = 10047,
                        Status       = OrderStatus.Completed,
                        DeliveryType = DeliveryType.Delivery,
                        DeliveryAddress = "г. Минск, пр. Независимости, 45, кв. 12",
                        DeliveryPhone   = "+375291112233",
                        PaymentMethod   = PaymentMethod.Card,
                        TotalPrice   = napoleon.Price + eclair.Price * 4,
                        CreatedAt    = DateTime.Now.AddDays(-30),
                        OrderItems   = new List<OrderItem> {
                            new OrderItem { ProductId = napoleon.Id, Quantity = 1, UnitPrice = napoleon.Price },
                            new OrderItem { ProductId = eclair.Id,   Quantity = 4, UnitPrice = eclair.Price  }
                        }
                    },
                    new Order {
                        UserId       = ivan.Id,
                        OrderNumber  = 10052,
                        Status       = OrderStatus.Completed,
                        DeliveryType = DeliveryType.Pickup,
                        PickupLocation = "г. Минск, ул. Бобруйская, 25",
                        PaymentMethod  = PaymentMethod.Cash,
                        TotalPrice   = (honey?.Price ?? 38) + (tiramisu?.Price ?? 8.5m) * 2,
                        CreatedAt    = DateTime.Now.AddDays(-22),
                        OrderItems   = new List<OrderItem> {
                            new OrderItem { ProductId = honey?.Id ?? 2,    Quantity = 1, UnitPrice = honey?.Price ?? 38m },
                            new OrderItem { ProductId = tiramisu?.Id ?? 6, Quantity = 2, UnitPrice = tiramisu?.Price ?? 8.5m }
                        }
                    },
                    new Order {
                        UserId       = maria.Id,
                        OrderNumber  = 10063,
                        Status       = OrderStatus.Preparing,
                        DeliveryType = DeliveryType.Delivery,
                        DeliveryAddress = "г. Минск, ул. Якуба Коласа, 20, оф. 301",
                        DeliveryPhone   = "+375339876543",
                        PaymentMethod   = PaymentMethod.Card,
                        TotalPrice   = (macaroon?.Price ?? 12.5m) * 2 + (zefir?.Price ?? 8.5m),
                        CreatedAt    = DateTime.Now.AddDays(-2),
                        OrderItems   = new List<OrderItem> {
                            new OrderItem { ProductId = macaroon?.Id ?? 9, Quantity = 2, UnitPrice = macaroon?.Price ?? 12.5m },
                            new OrderItem { ProductId = zefir?.Id ?? 23,   Quantity = 1, UnitPrice = zefir?.Price ?? 8.5m    }
                        }
                    },
                    new Order {
                        UserId       = darya?.Id ?? anna.Id,
                        OrderNumber  = 10071,
                        Status       = OrderStatus.Ready,
                        DeliveryType = DeliveryType.Pickup,
                        PickupLocation = "г. Минск, ул. Бобруйская, 25",
                        PaymentMethod  = PaymentMethod.Cash,
                        TotalPrice   = (truffle?.Price ?? 18m) + (praline?.Price ?? 15m),
                        CreatedAt    = DateTime.Now.AddDays(-1),
                        OrderItems   = new List<OrderItem> {
                            new OrderItem { ProductId = truffle?.Id ?? 19, Quantity = 1, UnitPrice = truffle?.Price ?? 18m  },
                            new OrderItem { ProductId = praline?.Id ?? 20, Quantity = 1, UnitPrice = praline?.Price ?? 15m  }
                        }
                    },
                    new Order {
                        UserId       = anna.Id,
                        OrderNumber  = 10088,
                        Status       = OrderStatus.Accepted,
                        DeliveryType = DeliveryType.Delivery,
                        DeliveryAddress = "г. Минск, ул. Захарова, 7, кв. 3",
                        DeliveryPhone   = "+375291112233",
                        PaymentMethod   = PaymentMethod.Card,
                        TotalPrice   = (praga?.Price ?? 48m) + (brownie?.Price ?? 8m) * 3,
                        CreatedAt    = DateTime.Now.AddHours(-3),
                        OrderItems   = new List<OrderItem> {
                            new OrderItem { ProductId = praga?.Id ?? 4,   Quantity = 1, UnitPrice = praga?.Price ?? 48m    },
                            new OrderItem { ProductId = brownie?.Id ?? 18, Quantity = 3, UnitPrice = brownie?.Price ?? 8m   }
                        }
                    }
                };

                ctx.Orders.AddRange(orders);
                ctx.SaveChanges();
            }
        }
    }
}
