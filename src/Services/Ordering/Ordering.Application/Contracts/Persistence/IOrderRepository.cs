using Ordering.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Ordering.Application.Contracts.Persistence.IAsyncRepository;

namespace Ordering.Application.Contracts.Persistence
{
    public interface IOrderRepository : IAsyncRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserName(string userName);
    }
}
