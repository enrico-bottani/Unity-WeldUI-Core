using System;
using System.Collections;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.UI.Messaging.Dispatcher;
using UnityWeld.UI.Messaging.Messenger;

namespace UnityWeld.UI.Paging
{
    [Binding]
    public class BasePanelViewModel : BaseMonoBehaviourViewModel
    {
        private string _name = string.Empty;
        private static Stack _backMessages = new Stack();
        private MessagesStorageComponent MessagesStorageComponent => MessagesStorageComponent.Get(gameObject);
        
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
            Messenger.Register<GeneralPageNavigationMessage>(GeneralUpdateMessage_Handler);
            gameObject.SetActive(false);
        }

        private void GeneralUpdateMessage_Handler(GeneralPageNavigationMessage msg)
        {
            Debug.Log(nameof(GeneralUpdateMessage_Handler)+"called. \n"+msg);
            if (CanHandle(msg))
            {
                if (!msg.IsBackMessage)
                {
                    if (msg.GetType().IsSubclassOf(typeof(RootPageNavigationMessage)))
                    {
                        _backMessages.Clear();
                    }

                    _backMessages.Push(msg);
                }

                RaisePropertyChanged("IsBackAvailable");
                gameObject.SetActive(true);

                Handle(msg);
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
    }
}