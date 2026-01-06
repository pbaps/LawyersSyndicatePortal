using Microsoft.AspNet.SignalR;

namespace LawyersSyndicatePortal.Hubs
{
    /// <summary>
    /// يمثل هذا الـ Hub نقطة اتصال مركزية لبث الرسائل إلى العملاء.
    /// يعمل كقناة بث للإشعارات من الخادم إلى جميع المستخدمين المتصلين.
    /// </summary>
    public class BroadcastHub : Hub
    {
        public void SendBroadcastNotification(string message, int unreadCount)
        {
            Clients.All.broadcastMessage(message, unreadCount);
        }
    }
}

 