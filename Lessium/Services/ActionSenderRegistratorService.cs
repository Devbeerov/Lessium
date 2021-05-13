using Lessium.Interfaces;
using Lessium.Models;

namespace Lessium.Services
{
    public static class ActionSenderRegistratorService
    {
        private static SendActionEventHandler sendActionHandler = null;
        private static SendActionEventHandler SendActionHandler
        {
            get
            {
                if (sendActionHandler == null)
                {
                    sendActionHandler = RuntimeDataService.GetSendActionEventHandler();
                }

                return sendActionHandler;
            }
        }

        // Won't check if it's already registered, so be careful with it.
        public static void RegisterSender(IActionSender sender)
        {
            sender.SendAction += SendActionHandler;
        }

        // Won't check if it's already unregistered, so be careful with it.
        public static void UnregisterSender(IActionSender sender)
        {
            sender.SendAction -= SendActionHandler;
        }
    }
}
