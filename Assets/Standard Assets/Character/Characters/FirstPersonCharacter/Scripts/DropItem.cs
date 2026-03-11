using UnityEngine;

namespace Items
{
    public class DropItem : MonoBehaviour
    {
        [Header("드롭 아이템 목록")]
        public GameObject[] itemPrefabs;

        [Header("드롭 힘 설정")]
        public float dropForce = 10f;

        public void Drop()
        {
            if (itemPrefabs == null || itemPrefabs.Length == 0)
            {
                Debug.LogWarning("[DropItem] 드롭할 아이템 프리팹이 없습니다!");
                return;
            }

            GameObject itemPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
            Vector3 spawnPos = transform.position + Vector3.up * 1f;

            GameObject droppedItem = Instantiate(itemPrefab, spawnPos, Quaternion.identity);

            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if (rb == null) rb = droppedItem.AddComponent<Rigidbody>();

            rb.useGravity = true;
            rb.isKinematic = false;

            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)).normalized;
            rb.AddForce(randomDir * dropForce, ForceMode.Impulse);

            Debug.Log($"[DropItem] Dropped Item: {droppedItem.name}");
        }
    }
}
