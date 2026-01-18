using Billetterie_Spectacles.Application.DTO.Response;
using Billetterie_Spectacles.Domain.Entities;

namespace Billetterie_Spectacles.Application.Mappings
{
    /// <summary>
    /// Mapper pour les conversions entre entité Ticket et ses DTOs
    /// Note : Ticket n'a pas de CreateDtoToEntity car les tickets sont créés
    /// automatiquement lors de la création d'une commande (pas de création directe)
    /// </summary>
    public static class TicketMapper
    {
        /// <summary>
        /// Convertit une entité Ticket en TicketDto
        /// </summary>
        public static TicketDto EntityToDto(Ticket ticket)
        {
            // Mapper la performance uniquement si elle est chargée
            PerformanceDto? performance = null;
            if (ticket.Performance != null)
            {
                performance = PerformanceMapper.EntityToDto(ticket.Performance);
            }

            return new TicketDto(
                Id: ticket.TicketId,
                Status: ticket.Status.ToString(),  // Enum → String
                UnitPrice: ticket.UnitPrice,
                OrderId: ticket.OrderId,
                PerformanceId: ticket.PerformanceId,
                CreatedAt: ticket.CreatedAt,
                UpdatedAt: ticket.UpdatedAt,
                Performance: performance
            );
        }
    }
}
