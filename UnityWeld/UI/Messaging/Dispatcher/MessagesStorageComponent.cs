using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityWeld.UI.Messaging.Dispatcher
{
    public class MessagesStorageComponent : MonoBehaviour
    {
        public readonly Dictionary<Type, AbstractMessageBehaviour> handledMessages = new Dictionary<Type, AbstractMessageBehaviour>();

        public static MessagesStorageComponent Get(GameObject gameObject)
        {
            if(gameObject.GetComponent<MessagesStorageComponent>() == null)
            {
                gameObject.AddComponent<MessagesStorageComponent>();
            }

            return gameObject.GetComponent<MessagesStorageComponent>();
        }
        

             
        public void Handle(GeneralPageNavigationMessage msg)
        {
            foreach (var msgHolder in handledMessages)
                msgHolder.Value.TryAction(msg);
        }

        public bool CanHandle(GeneralPageNavigationMessage msg)
        {
            foreach (var msgHolder in handledMessages)
            {
                if (msgHolder.Value.CanHandle(msg)) return true;
            }
            return false;
        }
    }
}