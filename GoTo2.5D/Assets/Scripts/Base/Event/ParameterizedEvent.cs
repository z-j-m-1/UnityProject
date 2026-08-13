using System;

namespace ZGameFramework.Core
{
    public abstract class ParameterizedEvent<T> : GameEvent where T : ParameterizedEvent<T>, new()
    {
        public abstract override void OnRecycled();

        public static void Trigger(Action<T> initializer)
        {
            var evt = ClassPool<T>.Get();
#if UNITY_EDITOR
            evt._recycled = false;
#endif
            initializer?.Invoke(evt);
            try
            {
                EventBus.Publish((T)evt);
            }
            finally
            {
#if UNITY_EDITOR
                evt._recycled = true;
#endif
                ClassPool<T>.Recycle(evt);
            }
        }

        public static void TriggerAsync(Action<T> initializer)
        {
            var evt = new T();
#if UNITY_EDITOR
            evt._recycled = false;
#endif
            initializer?.Invoke(evt);
            EventBus.Publish((T)evt);
        }

        /// <summary>
        /// 异步发布，外部必须手动回收,用于异步高频事件避免GC
        /// </summary>
        /// <param name="initializer"></param>
        public static T TriggerAsyncRecycle(Action<T> initializer)
        {
            var evt = ClassPool<T>.Get();
#if UNITY_EDITOR
            evt._recycled = false;
#endif
            initializer?.Invoke(evt);
            EventBus.Publish((T)evt);
            return evt;
        }

        public static void Subscribe(Action<T> listener) => EventBus.Subscribe<T>(listener);
        public static void Unsubscribe(Action<T> listener) => EventBus.Unsubscribe<T>(listener);
        public static void Subscribe(Action listener) => EventBus.Subscribe<T>(listener);
        public static void Unsubscribe(Action listener) => EventBus.Unsubscribe<T>(listener);
    }
}