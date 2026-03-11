using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;


public class Enemy2 : EnemyBase
{
    [Header("폭발 설정")]
    public float explodeRange = 3f;       // 폭발 범위
    public float explodeDamage = 20f;     // 폭발 데미지
    public float detectionRange = 10f;    // 플레이어 감지 거리
    public float speedChase = 3f;         // 플레이어 추적 속도
    public GameObject explodeEffectPrefab;
    private ParticleSystem particle;
    public float explosionForce = 10f;

    private bool exploded = false;

    // 랜덤 이동용
    private Vector3 randomDir;
    private float randomTimer;
    private bool isRandomMoving = false;

    protected override void Start()
    {
        base.Start();

        PickRandomDirection();
        randomTimer = _randomMovingTime;

        particle=transform.GetComponentInChildren<ParticleSystem>();        

        if (nearestPlayer == null)
            Debug.LogWarning($"{name}: 플레이어를 찾을 수 없음!");
    }

    protected override void Update()
    {
        base.Update();
        if (exploded || nearestPlayer == null) return;

        float distance = Vector3.Distance(transform.position, nearestPlayer.transform.position);

        // 폭발 범위 체크
        if (distance <= explodeRange)
        {
            Explode();
        }
    }

    protected override void FixedUpdate()
    {
        if (exploded || nearestPlayer == null) return;

        float distance = Vector3.Distance(transform.position, nearestPlayer.transform.position);
        Vector3 moveDir;

        if (distance < detectionRange)
        {
            moveDir = (nearestPlayer.transform.position - transform.position).normalized;

            TryJump(moveDir);
            Move(moveDir * speedChase);
            RotateTowards(moveDir, 5f);
        }
        else
        {
            RandomMoveLogic();
        }
    }

    private void RandomMoveLogic()
    {
        randomTimer -= Time.fixedDeltaTime;

        if (randomTimer <= 0f)
        {
            if (isRandomMoving)
            {
                isRandomMoving = false;
                randomTimer = _randomMoveStopTime;
            }
            else
            {
                isRandomMoving = true;
                randomTimer = _randomMovingTime;
                PickRandomDirection();
            }
        }

        if (isRandomMoving)
        {
            TryJump(randomDir.normalized);
            Move(randomDir, true);
            RotateTowards(randomDir, 3f);
        }
    }

    private void PickRandomDirection()
    {
        float rx = Random.Range(-1f, 1f);
        float rz = Random.Range(-1f, 1f);
        randomDir = new Vector3(rx, 0, rz).normalized;
    }

    private void Explode()
    {
        exploded = true;

        Vector3 explosionPos = transform.position;

        // if (explodeEffectPrefab != null)
        //     Instantiate(explodeEffectPrefab, explosionPos, Quaternion.identity);
        var emit = new ParticleSystem.EmitParams();
        emit.startSize = 10f;
        particle.Emit(emit, 1);

        Collider[] hitColliders = Physics.OverlapSphere(explosionPos, explodeRange);

        foreach(Collider col in hitColliders)
        {
            if(col.CompareTag("Chunk"))
            {
                ChunkManager.Instance.DestroyBlocks(explosionPos, explodeRange);
                
            }
            if (col.CompareTag("Player"))
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 forceDir = (col.transform.position - explosionPos).normalized;
                    rb.AddForce(forceDir * explosionForce, ForceMode.Impulse);
                }

                Debug.Log("플레이어가 폭발 피해를 받음: " + explodeDamage);

                var playerHealth = col.GetComponent<FirstPersonController>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(explodeDamage);
                }
            }
        }

        // 4. 폭발 몬스터 삭제
        StartCoroutine(kill());
    }

    IEnumerator kill()
    {
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }

}