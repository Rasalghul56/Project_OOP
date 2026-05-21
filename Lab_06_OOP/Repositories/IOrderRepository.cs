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


        int GetNotificationCount(int userId);


        void DetachProductFromOrderItems(int productId, string productName);
    }
}
