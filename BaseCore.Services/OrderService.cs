using BaseCore.Entities;
using BaseCore.Repository.EFCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaseCore.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepositoryEF _orderRepository;
        private readonly IOrderDetailRepositoryEF _orderDetailRepository;
        private readonly IProductRepositoryEF _productRepository;

        public OrderService(
            IOrderRepositoryEF orderRepository,
            IOrderDetailRepositoryEF orderDetailRepository,
            IProductRepositoryEF productRepository)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _productRepository = productRepository;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            order.OrderStatus = "Pending";
            return await _orderRepository.AddAsync(order);
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _orderRepository.GetByUserAsync(userId);
            foreach (var order in orders)
            {
                order.OrderDetails = await _orderDetailRepository.GetByOrderAsync(order.Id);

                if (order.OrderDetails == null)
                {
                    continue;
                }

                foreach (var detail in order.OrderDetails)
                {
                    detail.Product = await _productRepository.GetByIdAsync(detail.ProductId);
                }
            }

            return orders;
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order == null)
            {
                return null;
            }

            order.OrderDetails = await _orderDetailRepository.GetByOrderAsync(order.Id);
            foreach (var detail in order.OrderDetails)
            {
                detail.Product = await _productRepository.GetByIdAsync(detail.ProductId);
            }

            return order;
        }
    }
}
