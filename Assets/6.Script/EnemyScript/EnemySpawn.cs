using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰할 몬스터 프리팹 목록")]
    public GameObject[] enemyPrefabs;  // 여러 몬스터 프리팹 넣기

    [Header("스폰 포인트")]
    public Transform[] spawnPoints;

    [Header("스폰 간격")]
    public float spawnInterval = 5f;

    [Header("최대 스폰 수")]
    public int maxEnemies = 10;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnInterval);
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0 || enemyPrefabs.Length == 0) return;

        // 최대 스폰 수 체크
        spawnedEnemies.RemoveAll(e => e == null); // 이미 죽은 몬스터 제거
        if (spawnedEnemies.Count >= maxEnemies) return;

        // 랜덤 몬스터 선택
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // 랜덤 스폰 포인트 선택
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // 몬스터 생성
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        spawnedEnemies.Add(enemy);
    }
}