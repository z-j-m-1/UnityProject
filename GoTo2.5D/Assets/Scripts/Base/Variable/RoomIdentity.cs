using UnityEngine;

/// <summary>
/// 房间身份组件 - 挂在每个房间场景的一个 GameObject 上
/// 提供：房间稳定 ID（roomId，抗场景改名）+ 房间变量资产直接引用（按资产GUID绑定，抗场景改名）
/// 使用方式：把本组件挂到房间场景的常驻根物体上，拖入房间变量资产（或留空，Play 后编辑器会自动创建并回填）
/// </summary>
public class RoomIdentity : MonoBehaviour, ISerializationCallbackReceiver
{
    [Tooltip("房间稳定 ID（自动生成）。存档匹配与房间变量资产绑定使用它，场景文件改名不影响")]
    [SerializeField] private string roomId;

    [Tooltip("房间变量资产（Assets/Resources/PersistentVariables/Room/ 下的 VariableBundleObject）")]
    [SerializeField] private VariableBundleObject roomVariableAsset;

    /// <summary>房间稳定 ID（为空时自动生成）</summary>
    public string RoomId
    {
        get
        {
            EnsureRoomId();
            return roomId;
        }
    }

    /// <summary>直接引用的房间变量资产（可为空，为空时 RoomVariableManager 会退回按场景名加载）</summary>
    public VariableBundleObject VariableAsset => roomVariableAsset;

    public void SetVariableAsset(VariableBundleObject asset)
    {
        roomVariableAsset = asset;
    }

    private void EnsureRoomId()
    {
        if (string.IsNullOrEmpty(roomId))
        {
            roomId = System.Guid.NewGuid().ToString();
        }
    }

    public void OnBeforeSerialize()
    {
        EnsureRoomId();
    }

    public void OnAfterDeserialize()
    {
        EnsureRoomId();
    }

    private void Reset()
    {
        EnsureRoomId();
    }
}
