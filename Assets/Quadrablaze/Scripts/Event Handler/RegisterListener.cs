using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.Events;

public static class RegisterListener {

    //class StrEvent : UnityEvent<string> { }

    //public static void Idk() {
    //    StrEvent myEvent = new StrEvent();
    //    UnityAction<string> action = s => Bla();
    //    object unknownAction = action;

    //    AssignEvent(myEvent, action);

    //    var eventType = myEvent.GetType();
    //    var eventMethod = eventType.GetMethod("AddListener");
    //    var eventParameters = eventMethod.GetParameters();
    //    var eventParameter = eventParameters[0];
    //    var eventActionType = eventParameter.ParameterType;
    //    var actionTypeParameters = eventActionType.GetGenericParameterConstraints();
    //    //var parameter = unknownAction as ParameterType;

    //    if(eventActionType.IsGenericTypeDefinition) {
    //        eventActionType.MakeGenericType(actionTypeParameters);
    //    }

    //    //Debug.Log("Event Parameter Type = " + eventParameter.ParameterType.FullName);

    //    //var obj = unknownAction as 

    //    //myEvent.RemoveListener(unknownAction);
    //}

    //static void AssignEvent(StrEvent thatEvent, UnityAction<string> x) {
    //    thatEvent.AddListener(x);
    //}

    //static string Bla() {
    //    return "Bla";
    //}
    //static Dictionary<GameObject, List<EventActionPair>> storedEvents = null;
    //static bool initialized = false;

    //static void Initialize() {
    //    storedEvents = new Dictionary<GameObject, List<EventActionPair>>();
    //}

    //public static void Register(GameObject gameObject, UnityEvent unityEvent, UnityAction action) {
    //    if(!initialized) Initialize();

    //    List<EventActionPair> list;

    //    if(!storedEvents.TryGetValue(gameObject, out list)) {
    //        list = new List<EventActionPair>();
    //        storedEvents.Add(gameObject, list);
    //        gameObject.AddComponent<RegisteredListener>();
    //    }

    //    //list.Add(action);
    //    unityEvent.AddListener(action);
    //}

    //public static void Register<T>(GameObject gameObject, UnityEvent<T> unityEvent, UnityAction<T> action) {
    //    if(!initialized) Initialize();

    //    List<EventActionPair> list;

    //    if(!storedEvents.TryGetValue(gameObject, out list)) {
    //        list = new List<EventActionPair>();
    //        storedEvents.Add(gameObject, list);
    //        gameObject.AddComponent<RegisteredListener>();
    //    }

    //    list.Add(new EventActionPair(unityEvent, action));
    //    unityEvent.AddListener(action);
    //}

    ////public static void UnregisterEvents(GameObject gameObject) {
    ////    if(storedEvents.TryGetValue(gameObject, out List<object> list)) {

    ////    }
    ////}

    //struct EventActionPair {
    //    public UnityEvent unityEvent;
    //    //public UnityAction unityAction;

    //    public object action;
    //    //public Type dooby;
    //    //public UnityAction<Type> unityActionsa;

    //    public EventActionPair(UnityEvent unityEvent, object action) {
    //        this.unityEvent = unityEvent;
    //        this.action = action;
    //    }
    //}
}