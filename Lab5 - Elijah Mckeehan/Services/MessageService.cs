namespace Lab5_Elijah_Mckeehan.Services
{
    // Define the IMessageService interface here
    public interface IMessageService
    {
        void AddMessage(string message);
        void ClearMessages();
        IReadOnlyList<string> Messages { get; }
    }

    // Implement the IMessageService interface
    public class MessageService : IMessageService
    {
        private readonly List<string> messages = new List<string>();

        // Property to expose messages as a read-only collection
        public IReadOnlyList<string> Messages => messages.AsReadOnly();

        // Adds a message if it's not already in the list (optional check)
        public void AddMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message) && !messages.Contains(message))
            {
                messages.Add(message);
            }
        }

        // Clears the list of messages
        public void ClearMessages()
        {
            messages.Clear();
        }
    }
}
