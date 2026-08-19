using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    /// <summary>
    /// 带长按功能的按钮：继承 UnityEngine.UI.Button（保留 onClick 与选中/按下视觉状态），
    /// 额外提供 onLongPress 长按事件（Inspector 面板可配置，仿 Button.onClick 格式），
    /// 以及指针按下/抬起/进入/离开事件（onPointerDown/Up/Enter/Exit）。
    /// 用法：把物体上的 Button 组件换成本脚本（继承自 Button，字段兼容）。
    /// </summary>
    [AddComponentMenu("UI/Long Press Button", 31)]
    public class LongPressButton : Button, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Serializable]
        /// <summary>
        /// Function definition for a long press event.
        /// </summary>
        public class LongPressEvent : UnityEvent { }

        // Event delegates triggered on long press.
        [FormerlySerializedAs("onLongClick")]
        [SerializeField]
        private LongPressEvent m_OnLongPress = new LongPressEvent();

        /// <summary>长按阈值（秒）：按下超过该时长触发 onLongPress</summary>
        [SerializeField]
        private float m_LongPressDuration = 0.5f;

        /// <summary>按住期间重复触发间隔（秒）：0 = 只触发一次</summary>
        [SerializeField]
        private float m_RepeatInterval = 0f;

        /// <summary>长按触发后是否抑制本次点击（onClick）</summary>
        [SerializeField]
        private bool m_SuppressClickAfterLongPress = true;

        // 指针事件（uGUI Button 基类未提供，需自建并 override Selectable 的虚方法触发）
        [SerializeField]
        private UnityEvent m_OnPointerDown = new UnityEvent();

        [SerializeField]
        private UnityEvent m_OnPointerUp = new UnityEvent();

        [SerializeField]
        private UnityEvent m_OnPointerEnter = new UnityEvent();

        [SerializeField]
        private UnityEvent m_OnPointerExit = new UnityEvent();

        private bool m_Pressing;
        private float m_PressStartTime;
        private float m_LastInvokeTime;
        private bool m_LongPressFired;

        protected LongPressButton()
        { }

        /// <summary>
        /// UnityEvent that is triggered when the button is long pressed.
        /// Note: Triggered while held after longPressDuration seconds.
        /// </summary>
        public LongPressEvent onLongPress
        {
            get { return m_OnLongPress; }
            set { m_OnLongPress = value; }
        }

        /// <summary>长按阈值（秒）</summary>
        public float longPressDuration
        {
            get { return m_LongPressDuration; }
            set { m_LongPressDuration = value; }
        }

        /// <summary>按住期间重复触发间隔（秒），0 = 只触发一次</summary>
        public float repeatInterval
        {
            get { return m_RepeatInterval; }
            set { m_RepeatInterval = value; }
        }

        /// <summary>长按触发后是否抑制本次点击</summary>
        public bool suppressClickAfterLongPress
        {
            get { return m_SuppressClickAfterLongPress; }
            set { m_SuppressClickAfterLongPress = value; }
        }

        /// <summary>指针按下事件（按下即触发，不区分左右键）</summary>
        public UnityEvent onPointerDown
        {
            get { return m_OnPointerDown; }
            set { m_OnPointerDown = value; }
        }

        /// <summary>指针抬起事件（抬起即触发，不区分左右键）</summary>
        public UnityEvent onPointerUp
        {
            get { return m_OnPointerUp; }
            set { m_OnPointerUp = value; }
        }

        /// <summary>指针进入事件（进入矩形区域即触发）</summary>
        public UnityEvent onPointerEnter
        {
            get { return m_OnPointerEnter; }
            set { m_OnPointerEnter = value; }
        }

        /// <summary>指针离开事件（离开矩形区域即触发）</summary>
        public UnityEvent onPointerExit
        {
            get { return m_OnPointerExit; }
            set { m_OnPointerExit = value; }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            m_OnPointerEnter.Invoke();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            m_OnPointerDown.Invoke();
            m_Pressing = true;
            m_LongPressFired = false;
            m_PressStartTime = Time.unscaledTime;
            m_LastInvokeTime = -1f;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            m_Pressing = false;
            m_OnPointerUp.Invoke();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            m_Pressing = false;
            m_OnPointerExit.Invoke();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            // 长按已触发且设置抑制点击：本次松手不触发 onClick（短按仍走基类点击）
            if (m_SuppressClickAfterLongPress && m_LongPressFired)
            {
                m_LongPressFired = false;
                return;
            }
            base.OnPointerClick(eventData);
        }

        private void Update()
        {
            if (!m_Pressing || !IsActive() || !IsInteractable())
                return;

            float now = Time.unscaledTime;
            if (now - m_PressStartTime < m_LongPressDuration)
                return;

            if (m_RepeatInterval <= 0f)
            {
                // 只触发一次
                if (m_LastInvokeTime < 0f)
                {
                    m_LastInvokeTime = now;
                    m_LongPressFired = true;
                    m_OnLongPress.Invoke();
                }
            }
            else
            {
                // 按住期间按间隔重复触发
                if (m_LastInvokeTime < 0f || now - m_LastInvokeTime >= m_RepeatInterval)
                {
                    m_LastInvokeTime = now;
                    m_LongPressFired = true;
                    m_OnLongPress.Invoke();
                }
            }
        }
    }
}