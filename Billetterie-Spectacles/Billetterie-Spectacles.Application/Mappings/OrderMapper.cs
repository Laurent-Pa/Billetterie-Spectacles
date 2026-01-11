using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Domain.Entities;

namespace Billetterie_Spectacles.Application.Mappings
{
    /// <summary>
    /// Mapper pour les conversions entre entité Order et ses DTOs
    /// Note : Order n'a pas de CreateDtoToEntity car la création est gérée 
    /// programmatiquement dans le service (création de commande + tickets automatiques)
    /// </summary>
    public static class OrderMapper
    {
        /// <summary>
        /// Convertit une entité Order en OrderDto
        /// </summary>
        public static OrderDto EntityToDto(Order order)
        {
            return new OrderDto(
                Id: order.OrderId,
                Status: order.Status.ToString(),  // Enum → String
                Date: order.Date,
                TotalPrice: order.TotalPrice,
                UserId: order.UserId,
                UpdatedAt: order.UpdatedAt
            );
        }
    }
}
