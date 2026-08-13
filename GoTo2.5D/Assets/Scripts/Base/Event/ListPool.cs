using System;
using System.Collections.Generic;

namespace ZGameFramework.Core
{
    /// <summary>
    /// 列表对象池 - 用于复用临时列表，减少GC
    /// </summary>
    public static class ListPool<T>
    {
        // 使用Stack存储空闲列表，Lazy初始化避免过早分配
        private static readonly Stack<List<T>> _pool = new Stack<List<T>>();
        private static readonly object _lock = new object();

        /// <summary>
        /// 从池中获取一个列表
        /// </summary>
        public static List<T> Get()
        {
            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    var list = _pool.Pop();
                    // 清空但保留容量，避免重复分配
                    list.Clear();
                    return list;
                }
            }
            // 池为空，创建新列表（初始容量16可减少后续扩容）
            return new List<T>(16);
        }

        /// <summary>
        /// 将列表归还到池中
        /// </summary>
        public static void Recycle(List<T> list)
        {
            if (list == null) return;

            lock (_lock)
            {
                // 避免池中积累过多空闲列表（限制最大数量防止内存泄漏）
                if (_pool.Count < 64)
                {
                    // 清空数据但保留容量
                    list.Clear();
                    _pool.Push(list);
                }
                // 超过限制则交给GC处理
            }
        }

        /// <summary>
        /// 清空池（用于场景切换时释放内存）
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _pool.Clear();
            }
        }

        /// <summary>
        /// 获取池中当前空闲列表数量
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