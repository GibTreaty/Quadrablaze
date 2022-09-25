using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ProxyEvent<Key> where Key : IEquatable<Key> {
    public event ProxyEventHandler<Key> OnEvent;

    public void RaiseEvent(Key key, EventArgs args = null) {
        OnEvent?.Invoke(key, args);
    }
}

public class ProxyListener<Key> where Key : IEquatable<Key> {
    public event ProxyEventHandler<Key> OnListenEvent;

    Dictionary<Key, HashSet<ProxyListenerHandler>> listeners = new Dictionary<Key, HashSet<ProxyListenerHandler>>();

    public void RaiseEvent(Key key, EventArgs args = null) {
        if(listeners.TryGetValue(key, out var list))
            foreach(var callback in list)
                callback?.Invoke(args);

        OnListenEvent?.Invoke(key, args);
    }

    public void Subscribe(Key key, ProxyListenerHandler callback) {
        HashSet<ProxyListenerHandler> list;

        if(!listeners.TryGetValue(key, out list)) {
            list = new HashSet<ProxyListenerHandler>();
            listeners.Add(key, list);
        }

        list.Add(callback);
    }

    public void Unsubscribe(Key key, ProxyListenerHandler callback) {
        if(listeners.TryGetValue(key, out var list))
            if(list.Remove(callback))
                if(list.Count == 0)
                    listeners.Remove(key);
    }
}

public delegate void ProxyEventHandler<Key>(Key key, EventArgs args) where Key : IEquatable<Key>;
public delegate void ProxyListenerHandler(EventArgs args);
//public delegate void ProxyListenerHandler<TEventArgs>(TEventArgs args);

public class ProxyAction : IEquatable<string>, IEquatable<ProxyAction> {
    public string Name { get; }

    public ProxyAction([CallerMemberName] string name = null) {
        Name = name;
    }

    public bool Equals(string other) => Name == other;
    public bool Equals(ProxyAction other) => Name == other.Name;

    public static implicit operator string(ProxyAction item) => item.Name;
    public static implicit operator ProxyAction(string item) => new ProxyAction(item);
}

//public void 

//public class ListenerList {
//    public Dictionary<string, Listener> a;
//}

//public class Listener<TEventArgs> {
//    //public string Key { get; set; }
//    HashSet<Handler> Listeners { get; set; } = new HashSet<Handler>();

//    public void RaiseEvent(TEventArgs args) {
//        foreach(var listener in Listeners)
//            listener.Invoke(args);
//    }

//    public void Subscribe(Handler callback) {
//        Listeners.Add(callback);
//    }

//    public void Unsubscribe(Handler callback) {
//        Listeners.Remove(callback);
//    }

//    public delegate void Handler(TEventArgs args);
//}

//public class ProxyKey<TEventArgs> : IProxyHandler {
//    public string Key { get; set; }
//    //public TEventArgs Data { get; set; }

//    //public delegate void Handler(TEventArgs args);

//    public void RaiseEvent(TEventArgs args) {

//    }
//}

//public class Test {
//    public ProxyKey<int> x = new ProxyKey<int>();
//}

//public interface IProxyHandler {
//    string Key { get; }

//    void RaiseEvent(object args);
//}

public class EventArgs<T> : EventArgs {
    public T Value { get; set; }

    public EventArgs(T value) {
        Value = value;
    }
}