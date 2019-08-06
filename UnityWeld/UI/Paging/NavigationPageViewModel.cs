using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityWeld.Binding;
using UnityWeld.Binding.Exceptions;
using UnityWeld.Binding.Internal;
using UnityWeld.UI.Messaging.Dispatcher;
using UnityWeld.UI.Messaging.Messenger;

namespace UnityWeld.UI.Paging
{
    [Binding]
    public class NavigationPageViewModel : BaseMonoBehaviourViewModel
    {
        private string _name = string.Empty;
        private static Stack _backMessages = new Stack();
        private MessagesStorageComponent MessagesStorageComponent => MessagesStorageComponent.Get(gameObject);

        private Type baseMessage = typeof(GeneralPageNavigationMessage);
        private Type BaseMessage
        {
            get { return baseMessage;}
            set {
                if (value.IsSubclassOf(typeof(GeneralPageNavigationMessage)))
                {
                    baseMessage = value;
                }
                else
                {
                    throw new InvalidTypeException(nameof(NavigationPageViewModel)+" requires a message type subclass of "+nameof(GeneralPageNavigationMessage));
                }
            }
        }

        private void Start()
        {
            if (Application.IsPlaying(gameObject))
            {
                // Play logic
                Initialize();
            }
        }

        
        public void Update()
        {
            if (!Application.IsPlaying(gameObject)){
            }
        }

            [Binding]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        
        [Binding]
        public bool IsBackAvailable
        {
            get { return _backMessages.Count > 1; }
        }

        protected void Initialize()
        {
            Messenger.Register(typeof(GeneralPageNavigationMessage), GeneralUpdateMessage_Handler);
            gameObject.SetActive(false);
        }

        private void GeneralUpdateMessage_Handler(object msg)
        {
            
            var genPageNavMessage = (GeneralPageNavigationMessage) msg;
            if (CanHandle(genPageNavMessage))
            {
                if (!genPageNavMessage.IsBackMessage)
                {
                    if (genPageNavMessage.GetType().IsSubclassOf(typeof(RootPageNavigationMessage)))
                    {
                        _backMessages.Clear();
                    }

                    _backMessages.Push(genPageNavMessage);
                }

                RaisePropertyChanged("IsBackAvailable");
                gameObject.SetActive(true);

    //            Handle(genPageNavMessage);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        protected bool CanHandle(GeneralPageNavigationMessage msg)
        {
            return MessagesStorageComponent.CanHandle(msg);
        }

        protected void Handle(GeneralPageNavigationMessage msg)
        {
            MessagesStorageComponent.Handle(msg);
        }

    [Binding]
        public void Back()
        {
            if (_backMessages == null || _backMessages.Count <= 1) return;
            _backMessages.Pop();
            GeneralPageNavigationMessage msg = _backMessages.Peek() as GeneralPageNavigationMessage;
            msg.IsBackMessage = true;
            Messenger.Send(msg);
        }

        [Binding]
        public void Close()
        {
            Messenger.Send(new GeneralPageNavigationMessage());
        }
        public List<string> messagesUnderWatch =new List<string>();

        public void UpdateMessagesUnderWatch()
        {
            messagesUnderWatch.Clear();
            // Getting al messages dispatcher
            MessagesDispatcher[] list = GetComponents<MessagesDispatcher>();
            foreach (var messagesDispatcherComponent in list)
            {
                // Choose the index chosen by unity editor
                int chosenIndex =  messagesDispatcherComponent.UnityEditorSelectedMessageTypeIndex;
                // Get all available Messages Types
                var availableMessagesTypes = TypeResolver.TypesWithMessageAttribute.OrderBy(message => message.ToString()).ToArray();
                // Optaining the type of the message
                var type = (Type)availableMessagesTypes[chosenIndex];
                // Checking if the type is a Subclass of GeneralUpdateMessage
                if (type.IsSubclassOf(typeof(GeneralPageNavigationMessage)))
                    // If yes, add it under the messagesUnderWatch
                    messagesUnderWatch.Add(type.FullName);
            }
        }
    }
}