using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Confectionery.Data;
using Confectionery.Models;

namespace Confectionery.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context) { }

        public IEnumerable<Order> GetByUser(int userId)
            => Context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

        public IEnumerable<Order> GetByStatus(OrderStatus status)
            => Context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

        public IEnumerable<Order> GetByDateRange(DateTime from, DateTime to)
            => Context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

        public Order GetWithDetails(int id)
            => Context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .FirstOrDefault(o => o.Id == id);

        public IEnumerable<Order> GetAllWithDetails()
            => Context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

        public int GetNotificationCount(int userId)
            => Context.Orders
                .AsNoTracking()
                .Count(o => o.UserId == userId && o.HasStatusNotification);
    }
}
