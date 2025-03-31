using UnityEngine;
using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<Type, Dictionary<long, object>> InvocationDict = new();
    private const long GapFactor = long.MaxValue / int.MaxValue / 2;

    public static void UnSubscribe<T>(Action<T> action) where T : IEvent
    {
        UnSubscribe(typeof(T), action);
    }

    public static void UnSubscribe<T>(Action action) where T : IEvent
    {
        UnSubscribe(typeof(T), action);
    }

    public static void UnSubscribe(Type type, Action action)
    {
        UnSubscribe(type, (object)action);
    }

    public static void UnSubscribe(Type type, object action)
    {
        if (action == null) return;

        if (!InvocationDict.TryGetValue(type, out var actionDic)) return;

        foreach (var act in actionDic)
        {
            if (act.Value.Equals(action))
            {
                actionDic.Remove(act.Key);
                break;
            }
        }

        if (actionDic.Count == 0)
        {
            InvocationDict.Remove(type);
        }
    }

    public static void Subscribe<T>(Action<T> action, int invocationOrder = 0) where T : IEvent
    {
        Subscribe(typeof(T), action, invocationOrder);
    }

    public static void Subscribe<T>(Action action, int invocationOrder = 0) where T : IEvent
    {
        Subscribe(typeof(T), action, invocationOrder);
    }

    public static void Subscribe(Type type, Action action, int invocationOrder = 0)
    {
        Subscribe(type, (object)action, invocationOrder);
    }

    public static void Subscribe(Type type, object action, int invocationOrder = 0)
    {
        if (action == null) return;

        var longInvocationOrder = invocationOrder * GapFactor;

        if (InvocationDict.TryGetValue(type, out var actionsDict))
        {
            while (actionsDict.ContainsKey(longInvocationOrder))
            {
                longInvocationOrder++;
            }
        }
        else
        {
            actionsDict = new Dictionary<long, object>();
            InvocationDict[type] = actionsDict;
        }

        actionsDict.Add(longInvocationOrder, action);
    }


    public static void Publish<T>(T eventInstance) where T : IEvent
    {
        var eventType = typeof(T);

        if (InvocationDict.TryGetValue(eventType, out var actionsDict))
        {
            SortedList<long, object> sortedActions = new();

            foreach (var actionPair in actionsDict)
            {
                sortedActions.Add(actionPair.Key, actionPair.Value);
            }

            foreach (var sortedActionPair in sortedActions)
            {
                if (sortedActionPair.Value is Action<T> typedAction)
                {
                    if (!actionsDict.ContainsValue(typedAction)) continue;
                    typedAction.Invoke(eventInstance);
                }
                else
                {
                    Action typelessAction = (Action)sortedActionPair.Value;
                    if (!actionsDict.ContainsValue(typelessAction)) continue;
                    typelessAction.Invoke();
                }
            }
        }
    }

    public static void GetAllEventsName()
    {
        foreach (var events in InvocationDict)
        {
            Debug.Log($"Event name: {events.Key.Name}");
        }
    }

    public static void RemoveAllListeners<T>() where T : IEvent
    {
        var eventType = typeof(T);

        if (InvocationDict.ContainsKey(eventType))
        {
            InvocationDict.Remove(eventType);
        }
    }
}