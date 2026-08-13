using System;
using System.Collections.Generic;

namespace ZGameFramework.Core
{
    /// <summary>
    /// 对象池 - 用于复用类实例，减少GC
    /// </summary>
    public static class ClassPool<T> where T : class, IPoolable, new()
    {
        private static readonly Stack<T> _pool = new Stack<T>();
        private static readonly object _lock = new object();

        /// <summary>
        /// 从池中获取一个实例
        /// </summary>
        public static T Get()
        {
            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    return _pool.Pop();
                }
            }
            // 池为空，创建新实例
            return new T();
        }

        /// <summary>
        /// 将实例归还到池中
        /// </summary>
        public static void Recycle(T obj)
        {
            if (obj == null) return;

            // 调用对象的回收方法，重置状态
            obj.OnRecycled();

            lock (_lock)
            {
                // 限制池大小，防止内存泄漏
                if (_pool.Count < 128)
                {
                    _pool.Push(obj);
                }
                // 超过限制则丢弃，交给GC
            }
        }

        /// <summary>
        /// 清空池（场景切换时释放内存）
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _pool.Clear();
            }
        }

        /// <summary>
        /// 获取池中当前空闲实例数量
        /// </summary>
        public static int PoolCount
        {
            get
            {
                lock (_lock)
                {
                    return _pool.Count;
                }
            }
        }
    }
}