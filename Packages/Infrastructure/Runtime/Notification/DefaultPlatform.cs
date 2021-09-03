using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origine
{
    public class DefaultNotification : IGameNotification
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Subtitle { get; set; }
        public string Data { get; set; }
        public string Group { get; set; }
        public int? BadgeNumber { get; set; }
        public bool ShouldAutoCancel { get; set; }
        public DateTime? DeliveryTime { get; set; }

        public bool Scheduled => false;

        public string SmallIcon { get; set; }
        public string LargeIcon { get; set; }
    }

    public class DefaultPlatform : IGameNotificationsPlatform
    {
        public event Action<IGameNotification> NotificationReceived;

        public void CancelAllScheduledNotifications()
        {
        }

        public void CancelNotification(int notificationId)
        {
        }

        public IGameNotification CreateNotification() => new DefaultNotification();

        public void DismissAllDisplayedNotifications()
        {
        }

        public void DismissNotification(int notificationId)
        {
        }

        public IGameNotification GetLastNotification() => default;

        public void OnBackground()
        {
        }

        public void OnForeground()
        {
        }

        public void ScheduleNotification(IGameNotification gameNotification)
        {
        }

        public void ScheduleNotification(DefaultNotification notification)
        {
        }
    }
}
