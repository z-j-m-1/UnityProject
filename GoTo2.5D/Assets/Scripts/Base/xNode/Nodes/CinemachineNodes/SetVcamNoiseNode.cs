using Cinemachine;
using UnityEngine;
using XNode;

/// <summary>相机-噪声振幅：虚拟相机加/取 BasicMultiChannelPerlin 噪声并设置振幅（持续震屏/呼吸感）。振幅 0 复位。</summary>
[CreateNodeMenu("相机/噪声振幅")]
[NodeTint("#FFAA44")]
public class SetVcamNoiseNode : ComponentActionNode<CinemachineVirtualCamera>
{
    [Header("振幅（0 = 关闭）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float amplitude = 1f;

    [Header("频率（可选）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float frequency = 1f;

    protected override void Apply(CinemachineVirtualCamera vcam)
    {
        CinemachineBasicMultiChannelPerlin noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (noise == null)
        {
            noise = vcam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
        noise.m_AmplitudeGain = GetInputValue<float>(nameof(amplitude), amplitude);
        if (GetPort(nameof(frequency)).IsConnected || frequency != 0f)
        {
            noise.m_FrequencyGain = GetInputValue<float>(nameof(frequency), frequency);
        }
    }
}