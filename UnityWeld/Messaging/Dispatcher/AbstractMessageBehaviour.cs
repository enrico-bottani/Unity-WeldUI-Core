using System;

namespace UnityWeld.Messaging.Dispatcher
{
    /// <summary>
    /// Abstract Class to hold all the base information regarding a Message.
    /// </summary>
    public abstract class AbstractMessageBehaviour
    {
        public abstract Type Type { get; set; }
        public abstract bool CanHandle(object msg);
        public abstract void TryAction(object msg);
    }
}