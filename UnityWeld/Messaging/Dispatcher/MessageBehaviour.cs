using System;

namespace UnityWeld.Messaging.Dispatcher
{
    /// <summary>
    /// Concrete Class inherited from AbstractMessageHolder to hold the information regarding a Message.
    /// </summary>
    public class MessageBehaviour<T> : AbstractMessageBehaviour
    {
    
        public override void TryAction(object msg)
        {
            if (CanHandle(msg)) Action(Cast(msg));
        }

        private Action<T> Action
        {
            get;
        }
        
        // Sealed, not inheritable
        public sealed override Type Type
        {
            get; set;
        }

        public override bool CanHandle(object msg)
        {
            return Type == msg.GetType();
        }

        public MessageBehaviour(Action<T> action)
        {
            Action = action;
            Type = typeof(T);
        }

        private static T Cast(object g1)
        {
            return (T)(object)g1;
        }
    }
}