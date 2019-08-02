using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UnityWeld.Messaging.Dispatcher
{
    public class MessagesDispatcher : AbstractMemberBinding
    {
        private void OnScriptsReloaded()
        {
            handledMessages.Clear();
        }

        private readonly Dictionary<Type, AbstractMessageBehaviour> handledMessages = new Dictionary<Type, AbstractMessageBehaviour>();

        /// <summary>
        /// Add a new MessageType to be handled
        /// </summary>
        /// <param name="handleMessage">Callback action when the specified message has been sent</param>
        /// <typeparam name="TMessage">Message Type to be handled</typeparam>
        internal void Add<TMessage>(Action<TMessage> handleMessage)
        {
            if (!handledMessages.ContainsKey(typeof(TMessage)))
            {
                Messenger.Messenger.Register(handleMessage);
                
                handledMessages.Add(typeof(TMessage), new MessageBehaviour<TMessage>(handleMessage));
            }
        }

        private void Add(Type t, Action<object> handleMessage)
        {
            if (!handledMessages.ContainsKey(t))
            {
                Messenger.Messenger.Register(t, DispatchMessage);
                handledMessages.Add(t, new MessageBehaviour<object>(handleMessage) { Type = t });
            }
        }

        public HashSet<Type> GetAllHandledMessages()
        {
            var rtnSet = new HashSet<Type>();
            foreach (var a in handledMessages)
            {
                rtnSet.Add(a.Key);
            }
            return rtnSet;
        }

        internal void Clear()
        {
            handledMessages.Clear();
        }

        public void DispatchMessage(object msg)
        {
            foreach (var msgHolder in handledMessages)
                msgHolder.Value.TryAction(msg);
        }

        public bool CanHandleMessage(object msg)
        {
            foreach (var msgHolder in handledMessages)
            {
                if (msgHolder.Value.CanHandle(msg)) return true;
            }
            return false;
        }

        public int Count => handledMessages.Count;



        // Unity editor related code
    
        [SerializeField] private string unityEditorSelectedMessageName;

        public string UnityEditorSelectedMessageName
        {
            get { return unityEditorSelectedMessageName; }
            set { unityEditorSelectedMessageName = value; }
        }

        [SerializeField] private string unityEditorViewModelMethodName;

        public string UnityEditorViewModelMethodHandler
        {
            get { return unityEditorViewModelMethodName; }
            set { unityEditorViewModelMethodName = value; }
        }

        [SerializeField] private int unityEditorSelectedMessageTypeIndex;

        public int UnityEditorSelectedMessageTypeIndex
        {
            get { return unityEditorSelectedMessageTypeIndex; }
            set { unityEditorSelectedMessageTypeIndex = value; }
        }

        public override void Connect()
        {
            // Get the methodName and the viewModel object
            string methodName;
            object viewModel;
            ParseViewModelEndPointReference(UnityEditorViewModelMethodHandler,
                out methodName,
                out viewModel
            );

            // Recover all the Messages, like we did on the editor script, to recover the type of message
            var availableMessagesTypes =
                TypeResolver.TypesWithMessageAttribute.OrderBy(message => message.ToString()).ToArray();
            var messageType = (Type) availableMessagesTypes[unityEditorSelectedMessageTypeIndex];

            // Get the method to execute when the corresponding message is send, specified on the ViewModel
            var messageCallback = viewModel.GetType().GetMethod(methodName, new[] {messageType});

            // Add the callback to MessageDispatcher to handle it
            Add(messageType, msg =>
            {
                if (messageCallback != null) messageCallback.Invoke(viewModel, new object[] {msg});
                else throw new Exception("Message Callback is null");

            });
        }

        public override void Disconnect()
        {
        }
    }
}


