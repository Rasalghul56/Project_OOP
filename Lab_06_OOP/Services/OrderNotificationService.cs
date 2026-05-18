using System.Collections.Generic;
using System.Linq;
using Confectionery.Models;

namespace Confectionery.Services
{
    /// <summary>
    /// Tracks the last-seen statuses of the current user's orders so the
    /// sidebar can show a notification badge when an admin changes a status.
    /// </summary>
    public static class OrderNotificationService
    {
        private static readonly Dictionary<int, OrderStatus> _lastSeen =
            new Dictionary<int, OrderStatus>();

        /// <summary>Call once after login to establish the baseline.</summary>
        public static void Initialize(IEnumerable<Order> orders)
        {
            _lastSeen.Clear();
            MarkSeen(orders);
        }

        /// <summary>Returns true if the order's current status differs from the last seen one.</summary>
        public static bool HasChanged(int orderId, OrderStatus currentStatus)
            => _lastSeen.TryGetValue(orderId, out var last) && last != currentStatus;

        /// <summary>Counts how many orders have a status different from last seen.</summary>
        public static int GetChangedCount(IEnumerable<Order> currentOrders)
            => currentOrders.Count(o => HasChanged(o.Id, o.Status));

        /// <summary>Record the current statuses as "seen" (call when user opens the Orders tab).</summary>
        public static void MarkSeen(IEnumerable<Order> orders)
        {
            foreach (var o in orders)
                _lastSeen[o.Id] = o.Status;
        }

        /// <summary>Clear all tracking data (call on logout).</summary>
        public static void Reset() => _lastSeen.Clear();
    }
}
