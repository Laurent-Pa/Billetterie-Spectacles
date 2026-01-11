namespace Billetterie_Spectacles.Domain.Exceptions
{
    /// <summary>
    /// HTTP Status: 404 Not Found
    /// ressource introuvable
    /// Exception levée lorsqu'une entité recherchée n'existe pas en base de données
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
