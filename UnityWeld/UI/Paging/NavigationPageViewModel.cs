using System;
using System.Collections;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Exceptions;
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
            Initialize();
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
                Debug.Log("I can't handle");
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
    }
}