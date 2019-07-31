using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

public class MessagesDispatcher : AbstractMemberBinding
{
    private void OnScriptsReloaded()
    {
        messagesHandler.Clear();
    }

    public Dictionary<Type, AbstractMessageHolder> messagesHandler = new Dictionary<Type, AbstractMessageHolder>();

    internal void Add<T>(Action<T> handleMessage)
    {
        if (!messagesHandler.ContainsKey(typeof(T)))
        {
            Messenger.Register<T>( handleMessage);
            messagesHandler.Add(typeof(T), new MessagesHolder<T>(handleMessage));
        }
    }

       private void Add(Type t, Action<object> handleMessage)
    {
        if (!messagesHandler.ContainsKey(t))
        {
                    Messenger.Register(t, (msg)=>DispatchMessage((BaseMessage)msg));
            messagesHandler.Add(t, new MessagesHolder<object>(handleMessage){ Type =t });
        }
    }

    public HashSet<Type> GetAllHandledMessages()
    {
        HashSet<Type> rtnSet = new HashSet<Type>();
        foreach (var a in messagesHandler)
        {
            rtnSet.Add(a.Key);
        }
        return rtnSet;

    }

    internal void Clear()
    {
        messagesHandler.Clear();
    }

    public void DispatchMessage(BaseMessage msg)
    {
        foreach (var msgHolder in messagesHandler)
            msgHolder.Value.TryAction(msg);
    }

    public bool CanHandleMessage(BaseMessage msg)
    {
        foreach (var msgHolder in messagesHandler)
        {
            if (msgHolder.Value.CanHandle(msg)) return true;
        }
        return false;
    }

    public int Count
    {
        get
        {
            return messagesHandler.Count;
        }
    }

    [SerializeField]
    private string _selectedMessage;
    public string SelectedMessage { get { return _selectedMessage; } set { _selectedMessage = value; } }

    [SerializeField]
    private string _viewModelPropertyName;
    public string ViewModelMethodHandler
    {
        get { return _viewModelPropertyName; }
        set
        {
            _viewModelPropertyName = value;
        }
    }

     [SerializeField]
    private int _selectedIndex;
    public int SelectedIndex { get{return _selectedIndex; } set{_selectedIndex = value;} }

    private void Update()
    {
    }


    public List<Type> HandledMessages = new List<Type>();



    public override void Connect()
    {
        string backup = ViewModelMethodHandler;
        string methodName;
        object viewModel;
        ParseViewModelEndPointReference(ViewModelMethodHandler,
               out methodName,
               out viewModel
           );

        var availableMessagesTypes = TypeResolver.TypesWithMessageAttribute.OrderBy(message=> message.ToString()).ToArray();
     Type aimType = ((Type)availableMessagesTypes[_selectedIndex]);
        
        // get the method, given the name and the paramete
        var viewModelMethod = viewModel.GetType().GetMethod(methodName, new Type[]{aimType });
        
        this.Add(aimType, msg => {viewModelMethod.Invoke(viewModel,new object[]{msg});});
    }

    public override void Disconnect()
    {
    }
}


public abstract class AbstractMessageHolder
{
    public abstract Type Type { get;set; }
    public abstract bool CanHandle(BaseMessage msg);
    public abstract void TryAction(BaseMessage msg);
}

public class MessagesHolder<T> : AbstractMessageHolder
{
    private Action<T> handleMessage;
    private Action<T> handleMessage1;

    public override void TryAction(BaseMessage msg)
    {
        if (CanHandle(msg)) Action(Cast(msg));
    }
    public Action<T> Action
    {
        get;
    }
    public override Type Type
    {
        get;set;
    }

    public override bool CanHandle(BaseMessage msg)
    {
        return Type == msg.GetType();
    }

    public MessagesHolder(Action<T> action)
    {
        Action = action;
        Type = typeof(T);
    }

    public T Cast(BaseMessage g1)
    {
        return (T)(object)g1;
    }
}