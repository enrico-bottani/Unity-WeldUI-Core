using System;
using System.Collections.Generic;

namespace UnityWeld.UI.Messaging.Messenger
{
    public class Messenger
    {
        private static Dictionary<Type, List<IActionWrapper>> _recipientsAction = new Dictionary<Type, List<IActionWrapper>>();

        public static void Register<TMessage>(Action<TMessage> action)
        {
            Type messageType = typeof(TMessage);
            List<IActionWrapper> list;
            if (_recipientsAction.ContainsKey(messageType))
            {
                list = _recipientsAction[messageType];
            }
            else
            {
                list = new List<IActionWrapper>();
                _recipientsAction.Add(messageType, list);
            }
            list.Add(new ActionWrapper<TMessage>(action));
        }

        public static void Register(Type tMessage,Action<object> action)
        {
            Type messageType = tMessage;
            List<IActionWrapper> list;
            if (_recipientsAction.ContainsKey(messageType))
            {
                list = _recipientsAction[messageType];
            }
            else
            {
                list = new List<IActionWrapper>();
                _recipientsAction.Add(messageType, list);
            }
            list.Add(new ActionWrapper<object>(action));
        }

        public static void Unregister<TMessage>(Action<TMessage> action)
        {
            Type messageType = typeof(TMessage);
            if (!_recipientsAction.ContainsKey(messageType)) return;
            List<IActionWrapper> list = _recipientsAction[messageType];
            list.Remove(new ActionWrapper<TMessage>(action));
        }

        public static void Send<TMessage>(TMessage message)
        {
            Type messageType = typeof(TMessage);
            if (!_recipientsAction.ContainsKey(messageType)) return;
            List<IActionWrapper> list = _recipientsAction[messageType];
            foreach (var action in list)
            {
                // FIXME è capitato che qui si ingoiava le exceptions
                action.Execute(message);
            }
        }

        public static void Clear()
        {
            _recipientsAction.Clear();
        }
    }
}
