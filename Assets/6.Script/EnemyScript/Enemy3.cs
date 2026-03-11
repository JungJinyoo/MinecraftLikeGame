using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Enemy3 : EnemyBase
{
    private State _currentState;

    [Header("원거리 공격 설정")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;
    public float fireCooldown = 2f;
    [HideInInspector] public float fireTimer;

    public float rangedAttackDamage = 10f;

    private Vector3 randomDir;
    private float randomTimer;
    private bool isRandomMoving = false;
    private Animator _animator;

    protected override void Start()
    {
        base.Start();
        SetState(new IdleState(this));
        PickRandomDirection();
        randomTimer = _randomMovingTime;

        

        _animator = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        //base.FixedUpdate();
        _currentState?.OnUpdate(); // 상태 머신 업데이트를 물리 주기에 맞춤
    }

    public void SetState(State next)
    {
        _currentState?.OnExit();
        _currentState = next;
        _currentState?.OnEnter();
    }

    public IEnumerator DelayedFireProjectile(float delay)
    {
        yield return new WaitForSeconds(delay);
        FireProjectile();
    }

    public abstract class State
    {
        protected Enemy3 e3;
        public State(Enemy3 enemy) { e3 = enemy; }
        public abstract void OnEnter();
        public abstract void OnUpdate();
        public abstract void OnExit();
    }

    public class IdleState : State
    {
        public IdleState(Enemy3 e3) : base(e3) { }

        public override void OnEnter() { }

        public override void OnUpdate() // FixedUpdate 주기로 호출
        {
            if (e3.nearestPlayer == null) return;

            float dist = Vector3.Distance(e3.transform.position, e3.nearestPlayer.transform.position);

            if (dist < e3._range)
            {
                e3.SetState(new AttackState(e3));
                return;
            }

            e3.RandomMoveLogic();
        }

        public override void OnExit() { }
    }

    public class AttackState : State
    {
        public AttackState(Enemy3 e3) : base(e3) { }

        public override void OnEnter()
        {
            e3.fireTimer = e3.fireCooldown;
        }

        public override void OnUpdate() // FixedUpdate 주기로 호출
        {
            if (e3.nearestPlayer == null) return;

            Vector3 dirToPlayer = e3.nearestPlayer.transform.position - e3.transform.position;
            dirToPlayer.y = 0;
            Vector3 moveDirNormalized = dirToPlayer.normalized;

            e3.RotateTowards(dirToPlayer, 7f);

            e3.TryJump(moveDirNormalized);

            e3.Move(moveDirNormalized, false);

            Vector3 fireDir = e3.nearestPlayer.transform.position - e3.firePoint.position;
            fireDir.y = 0;
            if (fireDir != Vector3.zero)
                e3.firePoint.rotation = Quaternion.LookRotation(fireDir);

            float dist = Vector3.Distance(e3.transform.position, e3.nearestPlayer.transform.position);
            if (dist > e3._range)
            {
                e3.SetState(new IdleState(e3));
                return;
            }

            e3.fireTimer -= Time.fixedDeltaTime;
            if (e3.fireTimer <= 0)
            {
                if(e3._animator != null)
                {
                    e3._animator.SetTrigger("AttackTrigger");

                    e3.StartCoroutine(e3.DelayedFireProjectile(0.3f));
                }
                else
                {
                    e3.FireProjectile();
                }

                e3.fireTimer = e3.fireCooldown;
            }
        }

        public override void OnExit() { }
    }

    public void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        var proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Projectile projScript = proj.GetComponent<Projectile>();

        if(projScript != null)
        {
            projScript.Init(firePoint.forward, projectileSpeed, rangedAttackDamage);

            Debug.Log($"{name} 투사체 발사!");
        }
        else
        {
            var rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.velocity = firePoint.forward * projectileSpeed;

            Debug.LogWarning("Projectile 스크립트를 찾을 수 없어 데미지 전달 로직을 실행할 수 없습니다.");
        }
    }

    public void RandomMoveLogic()
    {
        randomTimer -= Time.fixedDeltaTime;

        if (randomTimer <= 0)
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

            // ⭐ 수정: Move에 정규화된 방향 벡터 전달 (Move 내부에서 _speedRandomMove 적용)
            Move(randomDir.normalized, true);

            RotateTowards(randomDir, 5f);
        }
    }

    private void PickRandomDirection()
    {
        float rx = Random.Range(-1f, 1f);
        float rz = Random.Range(-1f, 1f);
        randomDir = new Vector3(rx, 0, rz).normalized;
    }
}