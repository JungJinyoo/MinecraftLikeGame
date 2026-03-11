using UnityEngine;

public class MonsterDamageHandler : MonoBehaviour
{
    [Header("Ray Settings")]
    public float rayDistance = 10f;      // 레이 최대 거리
    public LayerMask monsterLayer;       // 몬스터 레이어만 감지

    [Header("Damage Settings")]

    //툴 데미지는 플레이어가 들고있는 툴의 데미지를 가져오면 되는데.
    Tooltag tooltag = Tooltag.NULL;
    public float toolDamage = 0.5f;
    float durability = 0;
    public ItemType itemtype;
    public GameObject debug;

    void Awake()
    {
        //itemtype = transform.GetChild(3).gameObject.GetComponent<DestructionRay>().itemtype;
        debug = transform.GetChild(3).gameObject;
    }
    void Update()
    {
        // 좌클릭 시 레이 발사
        if (Input.GetMouseButtonDown(0))
        {
            ShootRayAtMonster();
        }
        if (ToolStat.toolstats.TryGetValue(itemtype, out var data))
        {
            tooltag = data.tag;
            toolDamage = data.damage;
            durability = data.durability;
        }
        else
        {
            // 툴이 아니면 자동으로 null
            tooltag = Tooltag.NULL;
            toolDamage = 0.5f; // 기본데미지
           //durability = 나중에 내구도 동기화
        }
        itemtype = transform.GetChild(3).gameObject.GetComponent<DestructionRay>().itemtype;
    }

    void ShootRayAtMonster()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Main Camera가 없습니다!");
            return;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        // QueryTriggerInteraction.Collide 옵션 추가 → Trigger 콜라이더도 감지
        if (Physics.Raycast(ray, out hit, rayDistance, monsterLayer, QueryTriggerInteraction.Collide))
        {
            // 몬스터 맞음
            Debug.Log("Hit Monster: " + hit.collider.name);

            // EnemyBase 컴포넌트 가져오기 (부모에서 탐색)
            EnemyBase monster = hit.collider.GetComponentInParent<EnemyBase>();
            if (monster != null)
            {
                monster.TakeDamage(toolDamage);
                Debug.Log($"Monster {monster.name} takes {toolDamage} damage!");
            }
            else
            {
                Debug.LogWarning("Collider는 Monster지만 EnemyBase를 찾을 수 없음.");
            }
        }
        else
        {
            Debug.Log("몬스터에 맞지 않음");
        }

        // 시각화용 디버그 레이
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 1f);
    }
}
