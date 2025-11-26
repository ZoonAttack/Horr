using Entities.Marketplace;

namespace ServiceContracts.DTOs.Services
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Converts Service entity to ServiceReadDTO
        /// </summary>
        public static ServiceReadDTO Service_To_ServiceRead(this Service service)
        {
            if (service == null)
            {
                return null;
            }

            return new ServiceReadDTO
            {
                Id = service.Id,
                FreelancerId = service.FreelancerId,
                Title = service.Title,
                Description = service.Description,
                Price = service.Price,
                DeliveryTime = service.DeliveryTime,
                IsActive = service.IsActive,
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt
            };
        }

        /// <summary>
        /// Converts ServiceCreateDTO to Service entity
        /// </summary>
        public static Service ServiceCreate_To_Service(this ServiceCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Service
            {
                FreelancerId = createDto.FreelancerId,
                Title = createDto.Title,
                Description = createDto.Description,
                Price = createDto.Price,
                DeliveryTime = createDto.DeliveryTime,
                IsActive = true
            };
        }

        /// <summary>
        /// Applies ServiceUpdateDTO to an existing Service entity
        /// </summary>
        public static void ServiceUpdate_To_Service(this Service service, ServiceUpdateDTO updateDto)
        {
            if (service == null || updateDto == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(updateDto.Title))
                service.Title = updateDto.Title;

            if (!string.IsNullOrEmpty(updateDto.Description))
                service.Description = updateDto.Description;

            if (updateDto.Price.HasValue)
                service.Price = updateDto.Price;

            if (!string.IsNullOrEmpty(updateDto.DeliveryTime))
                service.DeliveryTime = updateDto.DeliveryTime;

            service.IsActive = updateDto.IsActive;
        }
    }
}
