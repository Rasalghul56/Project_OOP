using System;
using System.Collections.Generic;
using Confectionery.Models;

namespace Confectionery.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        IEnumerable<Order> GetByUser(int userId);
        IEnumerable<Order> GetByStatus(OrderStatus status);
        IEnumerable<Order> GetByDateRange(DateTime from, DateTime to);
        Order GetWithDetails(int id);
        IEnumerable<Order> GetAllWithDetails();

        /// <summary>
        /// Returns the count of orders that have a pending status notification for the user.
        /// Uses AsNoTracking to always read fresh data from DB, bypassing EF identity cache.
        /// </summary>
        int GetNotificationCount(int userId);
    }
}
