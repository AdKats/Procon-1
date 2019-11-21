using System;

namespace PRoCon.Core.Events
{
    public class CapturedEvent
    {
        public CapturedEvent(EventType eventType, CapturableEvents capturableEvent, string eventText, DateTime loggedTime, string instigatingAdmin)
        {
            EventType = eventType;
            Event = capturableEvent;
            EventText = eventText;
            LoggedTime = loggedTime;

            InstigatingAdmin = instigatingAdmin;
        }

        public EventType EventType { get; private set; }

        public CapturableEvents Event { get; private set; }

        public string EventText { get; private set; }

        public DateTime LoggedTime { get; set; }

        public string InstigatingAdmin { get; private set; }
    }
}