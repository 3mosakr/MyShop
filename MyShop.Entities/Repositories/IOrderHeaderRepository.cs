using MyShop.Entities.Models;


namespace MyShop.Entities.Repositories
{
    public interface IOrderHeaderRepository : IGenericRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);
        void UpdateStatus(int id, string? OrderStatus, string? PaymentStatus);
    }
}
