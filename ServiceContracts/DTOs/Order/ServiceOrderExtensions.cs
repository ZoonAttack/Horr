using Entities.Enums;
using Entities.Marketplace;

namespace ServiceContracts.DTOs.Order
{
    public static class ServiceOrderExtensions
    {
        /// <summary>
        /// Converts Order entity to ServiceOrderReadDTO
        /// </summary>
        public static ServiceOrderReadDTO Order_To_ServiceOrderRead(this Entities.Marketplace.Order order)
        {
            if (order == null)
            {
                return null;
            }

            return new ServiceOrderReadDTO
            {
                Id = order.Id,
                ClientId = order.ClientId,
                FreelancerId = order.FreelancerId,
                ServiceId = order.ServiceId,
                ProjectId = order.ProjectId,
                OrderType = order.OrderType,
                Amount = order.Amount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }

        /// <summary>
        /// Converts ServiceOrderCreateDTO to Order entity
        /// </summary>
        public static Entities.Marketplace.Order ServiceOrderCreate_To_Order(this ServiceOrderCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Entities.Marketplace.Order
            {
                ClientId = createDto.ClientId,
                FreelancerId = createDto.FreelancerId,
                ServiceId = createDto.ServiceId,
                ProjectId = createDto.ProjectId,
                OrderType = createDto.OrderType,
                Amount = createDto.Amount,
                Status = OrderStatus.Pending
            };
        }

        /// <summary>
        /// Applies ServiceOrderUpdateDTO to an existing Order entity
        /// </summary>
        public static void ServiceOrderUpdate_To_Order(this Entities.Marketplace.Order order, ServiceOrderUpdateDTO updateDto)
        {
            if (order == null || updateDto == null)
            {
                return;
            }

            order.Status = updateDto.Status;
        }
    }
}
