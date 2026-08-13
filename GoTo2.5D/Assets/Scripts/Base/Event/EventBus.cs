using System;
using System.Collections.Generic;

namespace ZGameFramework.Core
{
    public static class EventBus
    {
        private static class EventRegistry<T> where T : GameEvent
        {
            public static readonly List<Action<T>> ParamListeners = new List<Action<T>>();
            public static readonly List<Action> SignalListeners = new List<Action>();
            public static readonly object Lock = new object();
        }

        public static void Subscribe<T>(Action<T> listener) where T : GameEvent
        {
            lock (EventRegistry<T>.Lock)
            {
                EventRegistry<T>.ParamListeners.Add(listener);
            }
        }

        public static void Subscribe<T>(Action listener) where T : GameEvent
        {
            lock (EventRegistry<T>.Lock)
            {
                EventRegistry<T>.SignalListeners.Add(listener);
            }
        }

        public static void Unsubscribe<T>(Action listener) where T : GameEvent
        {
            lock (EventRegistry<T>.Lock)
            {
                EventRegistry<T>.SignalListeners.Remove(listener);
            }
        }

        public static void Unsubscribe<T>(Action<T> listener) where T : GameEvent
        {
            lock (EventRegistry<T>.Lock)
            {
                EventRegistry<T>.ParamListeners.Remove(listener);
            }
        }

        public static void Publish<T>(T eventData) where T : GameEvent
        {
            List<Action<T>> paramSnapshot = null;
            List<Action> signalSnapshot = null;

            lock (EventRegistry<T>.Lock)
            {
                if (EventRegistry<T>.ParamListeners.Count > 0)
                {
                    paramSnapshot = ListPool<Action<T>>.Get();
                    paramSnapshot.AddRange(EventRegistry<T>.ParamListeners);
                }

                if (EventRegistry<T>.SignalListeners.Count > 0)
                {
                    signalSnapshot = ListPool<Action>.Get();
                    signalSnapshot.AddRange(EventRegistry<T>.SignalListeners);
                }
            }

            if (paramSnapshot != null)
            {
                foreach (var listener in paramSnapshot)
                    listener?.Invoke(eventData);
                ListPool<Action<T>>.Recycle(paramSnapshot);
            }

            if (signalSnapshot != null)
            {
                foreach (var listener in signalSnapshot)
                    listener?.Invoke();
                ListPool<Action>.Recycle(signalSnapshot);
            }
        }

        public static void PublishSignal<T>(T signal) where T : GameEvent
        {
            Publish(signal);
        }
    }
}