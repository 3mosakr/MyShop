using MyShop.Entities.Models;


namespace MyShop.Entities.Repositories
{
    public interface IOrderDetailRepository : IGenericRepository<OrderDetail>
    {
        void Update(OrderDetail orderDetail);
    }
}
