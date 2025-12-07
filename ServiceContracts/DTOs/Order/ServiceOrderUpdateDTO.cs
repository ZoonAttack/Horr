using Entities.Enums;

namespace ServiceContracts.DTOs.Order
{
    /// <summary>
    /// DTO for updating existing Order status.
    /// </summary>
    public class ServiceOrderUpdateDTO
    {
        public OrderStatus Status { get; set; }
    }
}
