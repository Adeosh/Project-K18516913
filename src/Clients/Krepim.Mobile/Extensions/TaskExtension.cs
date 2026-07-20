namespace Krepim.Mobile.Extensions
{
    public static class TaskExtension
    {
        public static Action<Exception>? GlobalErrorHandler { get; set; }

        public static async void SafeFireAndForget(this Task task, Action<Exception>? localErrorHandler = null)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                if (localErrorHandler != null)
                    localErrorHandler.Invoke(ex);
                else
                    GlobalErrorHandler?.Invoke(ex);
            }
        }
    }
}
