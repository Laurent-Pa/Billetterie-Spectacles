namespace Billeterie_Spectacles.Domain.Exceptions
{
    public class InsufficientTicketsException : DomainException
    {
        public InsufficientTicketsException(int requested, int available)
            : base($"Impossible de réserver {requested} billet(s). Seulement {available} place(s) disponible(s).")
        {
        }
    }
}