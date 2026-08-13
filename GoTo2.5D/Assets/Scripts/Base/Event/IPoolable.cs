namespace ZGameFramework.Core
{
    /// <summary>
    /// 可池化对象接口
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 对象被回收时调用，用于重置状态
        /// </summary>
        void OnRecycled();
    }
}