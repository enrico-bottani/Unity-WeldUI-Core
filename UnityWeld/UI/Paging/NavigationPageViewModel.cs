using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityWeld.Binding;
using UnityWeld.Binding.Exceptions;
using UnityWeld.Binding.Internal;
using UnityWeld.UI.Messaging.Dispatcher;
using UnityWeld.UI.Messaging.Messenger;

namespace UnityWeld.UI.Paging
{
    [Serializable]
    public class DictionaryOfStringBool : SerializableDictionary<string, bool>
    {
    }
    
    [Binding]
    public class NavigationPageViewModel : BaseMonoBehaviourViewModel
    {
        private string _name = string.Empty;
        private static Stack _backMessages = new Stack();
        private MessagesStorageComponent MessagesStorageComponent => MessagesStorageComponent.Get(gameObject);

        private Type baseMessage = typeof(GeneralPageNavigationMessage);
        
        [SerializeField]
        private DictionaryOfStringBool defaultBehaviourUnactiveMessages;
        public DictionaryOfStringBool DefaultBehaviourUnactiveMessages
        {
            get
            {
                return defaultBehaviourUnactiveMessages;
            }
            set { defaultBehaviourUnactiveMessages = value; }
        }

        private void Awake()
        {
        }

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
                Messenger.Register(typeof(GeneralPageNavigationMessage), GeneralUpdateMessage_Handler);
                gameObject.SetActive(false);
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

                bool doDefault;
                if (DefaultBehaviourUnactiveMessages.TryGetValue(msg.GetType().ToString(),out doDefault))
                {
                    //Debug.Log(msg.GetType()+" is set to be default: "+doDefault);
                    if (doDefault)
                    {
                        gameObject.SetActive(true);
                    }
                }
                else
                {
                    gameObject.SetActive(true);
                }
                
                //Handle(genPageNavMessage);
            }
            else
            {
                
                bool doDefault;
                if (DefaultBehaviourUnactiveMessages.TryGetValue(msg.GetType().ToString(),out doDefault))
                {

                    if (doDefault)
                    {
                        gameObject.SetActive(false);
                    }
                }
                else
                {
                    gameObject.SetActive(false);
                }
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
        
        
        /// <summary>
        /// This method will audit all the MessageDispatcher components assigned
        /// </summary>
        public void AuditAllMessageDispatchers()
        {
            
            var newDict = NewMessageDictionary<GeneralPageNavigationMessage>();
            var updatedDictionary = KeepExistingAddNewRemoveOutdated(newDict, defaultBehaviourUnactiveMessages);
            defaultBehaviourUnactiveMessages = updatedDictionary;
        }

        /// <summary>
        /// This method will update the values of the first dictionary and update them with the latter 
        /// </summary>
        /// <param name="newDict"></param>
        /// <param name="oldDict"></param>
        /// <returns></returns>
        private DictionaryOfStringBool KeepExistingAddNewRemoveOutdated(Dictionary<string, bool> newDict, DictionaryOfStringBool oldDict)
        {
            var tmpDict = new DictionaryOfStringBool();
            foreach (var rowEntry in newDict)
            {
                if (oldDict.ContainsKey(rowEntry.Key)) 
                    tmpDict[rowEntry.Key] = oldDict[rowEntry.Key];
                else tmpDict.Add(rowEntry.Key,rowEntry.Value);
            }

            return tmpDict;
        }
        
        /// <summary>
        /// Generate a new dictionary containing all the messages handled by all the MessageDispatcher componenents
        /// </summary>
        /// <typeparam name="T">The reserved Message Class accountable about the Page behaviour</typeparam>
        /// <returns>A new dictionary containing all the messages</returns>
        private DictionaryOfStringBool NewMessageDictionary<T>()
        {
            DictionaryOfStringBool tmpDictionary = new DictionaryOfStringBool();

            // Get all available Messages Types
            var availableMessagesTypes = TypeResolver.TypesWithMessageAttribute.OrderBy(message => message.ToString()).ToArray();

            // Getting al message dispatchers
            MessagesDispatcher[] messagesDispatcherComponents = GetComponents<MessagesDispatcher>();
            foreach (var messagesDispatcherComponent in messagesDispatcherComponents)
            {
                // Choose the index chosen by unity editor
                int indexChosenByComponent =  messagesDispatcherComponent.UnityEditorSelectedMessageTypeIndex;
                // Optaining the type of the message
                if (indexChosenByComponent<0 || indexChosenByComponent> availableMessagesTypes.Length)
                {
                    // Not a valid Message has been chosen nothing to do here... 
                }
                else
                {
                    var type = (Type)availableMessagesTypes[indexChosenByComponent];
                    // Checking if the type is a Subclass of GeneralUpdateMessage
                    if (type.IsSubclassOf(typeof(T)))
                    {
                        if (tmpDictionary.ContainsKey(type.Name))
                        {
                            tmpDictionary[type.Name] = true;
                        }
                        tmpDictionary.Add(type.Name, true);
                    }  
                }

            }
            return tmpDictionary;
        }
    }


}