using System;

namespace UnityWeld.Messaging.Messenger
{
    public class ActionWrapper<TMessage> : IActionWrapper
    {
        private Action<TMessage> _action;

        public ActionWrapper(Action<TMessage> action)
        {
            _action = action;
        }

        public void Execute(object message)
        {
            if (_action == null) return;

            // FIXME apparentemente questa chiamata si ingoia le exceptions
            _action.Invoke((TMessage)message);
        }

        public override bool Equals(object obj)
        {
            return _action == (obj as ActionWrapper<TMessage>)._action;
        }

        // FIXME è corretta queast'implementazione?
        public override int GetHashCode()
        {
            return 1;
        }
    }
}