using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ItemUVCut : MonoBehaviour
{
    private Vector2 atlasSize; // 아틀라스 크기 (가로7, 세로3)
    [Tooltip("0,0 = 왼쪽 아래")]
    public Vector2Int itemIndex; // 선택할 아이템 좌표

    private Mesh mesh;

    private void Awake()
    {
        // 항상 7x3 아틀라스로 고정
        atlasSize = new Vector2(7, 3);
    }

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        UpdateUVs();
    }

    /// <summary>
    /// UV를 갱신해서 선택한 아이템 표시
    /// </summary>
    public void UpdateUVs()
    {
        if (mesh == null)
            return;

        Vector2[] uvs = mesh.uv;

        // 한 아이템의 UV 크기
        float uvWidth = 1f / atlasSize.x;
        float uvHeight = 1f / atlasSize.y;

        // 시작 UV 좌표 계산
        float uStart = itemIndex.x * uvWidth;
        float vStart = 1f - ((itemIndex.y + 1) * uvHeight); // bottom-left 기준

        // Quad 기본 UV 순서: 0=(0,0), 1=(1,0), 2=(0,1), 3=(1,1)
        uvs[0] = new Vector2(uStart, vStart);
        uvs[1] = new Vector2(uStart + uvWidth, vStart);
        uvs[2] = new Vector2(uStart, vStart + uvHeight);
        uvs[3] = new Vector2(uStart + uvWidth, vStart + uvHeight);

        mesh.uv = uvs;
    }

    /// <summary>
    /// Inspector나 코드에서 호출하면 아이템을 바꿀 수 있음
    /// </summary>
    /// <param name="x">열 0~6</param>
    /// <param name="y">행 0~2 (아래부터)</param>
    public void SetItem(int x, int y)
    {
        itemIndex = new Vector2Int(x, y);
        UpdateUVs();
    }
}