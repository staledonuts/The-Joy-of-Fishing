using System;
using System.Collections.Generic;

namespace DonutPackage.EventBus
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> s_Events = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> action) where T : IEvent
        {
            if (s_Events.TryGetValue(typeof(T), out var internalAction))
            {
                s_Events[typeof(T)] = (Action<T>)internalAction + action;
            }
            else
            {
                s_Events[typeof(T)] = action;
            }
        }

        public static void Unsubscribe<T>(Action<T> action) where T : IEvent
        {
            if (s_Events.TryGetValue(typeof(T), out var internalAction))
            {
                s_Events[typeof(T)] = (Action<T>)internalAction - action;
            }
        }

        public static void Publish<T>(T eventData) where T : IEvent
        {
            if (s_Events.TryGetValue(typeof(T), out var internalAction))
            {
                (internalAction as Action<T>)?.Invoke(eventData);
            }
        }
    }
}
