namespace Krepim.Mobile.Exceptions
{
    public enum UserMessageType
    {
        Info,
        Warning,
        Error
    }

    public class UserMessageException : Exception
    {
        public string Title { get; }
        public UserMessageType Type { get; }

        public UserMessageException(
            string message,
            string title = "Внимание",
            UserMessageType type = UserMessageType.Warning) : base(message)
        {
            Title = title;
            Type = type;
        }
    }
}
