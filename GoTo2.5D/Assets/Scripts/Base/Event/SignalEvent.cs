using System;

namespace ZGameFramework.Core
{
    public abstract class SignalEvent<T> : GameEvent where T : SignalEvent<T>, new()
    {
        private static readonly T Instance = new T();

        public override void OnRecycled() { }

        public static void Trigger()
        {
            EventBus.PublishSignal(Instance);
        }

        public static void Subscribe(Action listener) => EventBus.Subscribe<T>(listener);
        public static void Unsubscribe(Action listener) => EventBus.Unsubscribe<T>(listener);
    }
}