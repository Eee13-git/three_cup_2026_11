using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 关卡预制体里的视窗初位标记。编辑时用半透明框显示"透光口一开始照哪儿"，
    /// 运行时由 LevelSession 读取它的位置与缩放来推导视窗 offset，然后隐藏自己。
    /// 关卡设计师直接拖动这个物体就能调整视窗初位，不必再手填 LevelConfig。
    /// </summary>
    [ExecuteAlways]
    public sealed class VisionWindowPreview : MonoBehaviour
    {
        [SerializeField] Color frameColor = new Color(1f, 0.92f, 0.55f, 0.75f);

        void OnDrawGizmos()
        {
            Gizmos.color = frameColor;
            Vector3 size = transform.lossyScale;
            Gizmos.DrawWireCube(transform.position, new Vector3(size.x, size.y, 0f));
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.55f, 0.2f, 0.9f);
            Gizmos.DrawWireCube(transform.position, transform.lossyScale);
        }
    }
}
