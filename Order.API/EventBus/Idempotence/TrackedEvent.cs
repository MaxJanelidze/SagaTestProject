using System;

namespace Order.API.EventBus.Idempotence
{
    public class TrackedEvent
    {
        public TrackedEvent(string messageId, string eventTypeName)
        {
            MessageId = messageId;
            EventTypeName = eventTypeName;
            TrackStatus = TrackStatus.Processed;
            CreateDate = DateTime.Now;
        }

        public string MessageId { get; private set; }

        public string EventTypeName { get; private set; }

        public TrackStatus TrackStatus { get; private set; }

        public DateTime CreateDate { get; private set; }
    }

    public enum TrackStatus
    {
        Processed = 1
    }
}
