using System;
using System.Diagnostics;

namespace ZGameFramework.Core
{
    public abstract class GameEvent : IPoolable
    {
        public abstract void OnRecycled();

#if UNITY_EDITOR
        internal bool _recycled;
        [Conditional("UNITY_EDITOR")]
        public void AssertSafe()
        {
            if (_recycled) throw new InvalidOperationException("已被回收，不要异步持有");
        }
#endif
    }
}