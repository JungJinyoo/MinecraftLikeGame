using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Enemy1 : EnemyBase
{
    private State _currentState;

    [Header("근접 공격 설정")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    public float _attackRange = 3.5f;
    private float attackTimer;

    private Animator _animator;

    public float damageApplyDelay = 0.5f;

    protected override void Start()
    {
        base.Start();
        if (nearestPlayer == null) Debug.LogWarning($"{name}의 _player가 할당되지 않았습니다!");
        SetState(new IdleState(this));

        

        _animator = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();
        _currentState?.OnUpdate();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        _currentState?.OnFixedUpdate();
    }

    public void SetState(State next)
    {
        _currentState?.OnExit();
        _currentState = next;
        _currentState?.OnEnter();
    }

    public IEnumerator DelayedApplyDamage(FirstPersonController player, float damage, float delay)
    {
        yield return new WaitForSeconds(delay);
        if(player != null)
        {
            player.TakeDamage(damage);
            Debug.Log($"{name}근거리 공격! 데미지: {damage} 적용 완료.");
        }
    }

    public abstract class State
    {
        protected Enemy1 e1;
        public State(Enemy1 enemy) { e1 = enemy; }
        public abstract void OnEnter();
        public abstract void OnUpdate();
        public abstract void OnFixedUpdate();
        public abstract void OnExit();
    }

    // 대기 상태 (IdleState)
    public class IdleState : State
    {
        float timer;
        public IdleState(Enemy1 e) : base(e) { }

        public override void OnEnter()
        {
            timer = 0;
            e1.attackTimer = 0;
            e1.rb.velocity = new Vector3(0, e1.rb.velocity.y, 0);
        }

        public override void OnUpdate()
        {
            if (e1.nearestPlayer == null) return;
            timer += Time.deltaTime;

            float dist = Vector3.Distance(e1.transform.position, e1.nearestPlayer.transform.position);

            if (dist < e1._range)
                e1.SetState(new ChaseState(e1));
            else if (timer > e1._randomMoveStopTime)
                e1.SetState(new PatrolState(e1));
        }

        public override void OnFixedUpdate()
        {
            e1.rb.velocity = new Vector3(0, e1.rb.velocity.y, 0);
        }

        public override void OnExit() { }
    }

    // 순찰 상태 (PatrolState)
    public class PatrolState : State
    {
        Vector3 dir;
        float moveTime;

        public PatrolState(Enemy1 e) : base(e) { }

        public override void OnEnter()
        {
            Vector2 rand = Random.insideUnitCircle.normalized;
            dir = new Vector3(rand.x, 0f, rand.y).normalized;
            moveTime = Random.Range(1f, e1._randomMovingTime);
        }

        public override void OnUpdate()
        {
            if (e1.nearestPlayer == null) return;

            float dist = Vector3.Distance(e1.transform.position, e1.nearestPlayer.transform.position);
            if (dist < e1._range)
                e1.SetState(new ChaseState(e1));
        }

        public override void OnFixedUpdate()
        {
            moveTime -= Time.fixedDeltaTime;
            if (moveTime <= 0)
            {
                e1.SetState(new IdleState(e1));
                return;
            }

            e1.TryJump(dir);

            e1.RotateTowards(dir, 3f);

            e1.Move(dir, true);
        }

        public override void OnExit() { }
    }

    // 추격 상태 (ChaseState)
    public class ChaseState : State
    {
        public ChaseState(Enemy1 e) : base(e) { }

        public override void OnEnter() { }

        public override void OnUpdate()
        {
            if (e1.nearestPlayer == null) return;

            float dist = Vector3.Distance(e1.transform.position, e1.nearestPlayer.transform.position);
            if (dist <= e1._attackRange)
                e1.SetState(new AttackState(e1));
            else if (dist > e1._range * 1.5f)
                e1.SetState(new IdleState(e1));
        }

        public override void OnFixedUpdate()
        {
            if (e1.nearestPlayer == null) return;

            Vector3 dir = e1.nearestPlayer.transform.position - e1.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f)
            {
                e1.rb.velocity = new Vector3(0, e1.rb.velocity.y, 0);
                return;
            }
            Vector3 moveDir = dir.normalized;

            e1.TryJump(moveDir);

            e1.RotateTowards(moveDir, 7f);

            e1.Move(moveDir, false);
        }

        public override void OnExit() { }
    }

    // 공격 상태 (AttackState)
    public class AttackState : State
    {
        public AttackState(Enemy1 e) : base(e) { }

        public override void OnEnter()
        {
            e1.attackTimer = 0;
        }

        public override void OnUpdate()
        {
            if (e1.nearestPlayer == null) return;

            float dist = Vector3.Distance(e1.transform.position, e1.nearestPlayer.transform.position);
            if (dist > e1._attackRange)
                e1.SetState(new ChaseState(e1));
        }

        public override void OnFixedUpdate()
        {
            if (e1.nearestPlayer == null) return;

            e1.rb.velocity = new Vector3(0, e1.rb.velocity.y, 0);

            Vector3 dirToPlayer = e1.nearestPlayer.transform.position - e1.transform.position;
            dirToPlayer.y = 0;

            e1.RotateTowards(dirToPlayer, 7f);

            e1.attackTimer -= Time.fixedDeltaTime;
            if (e1.attackTimer <= 0)
            {
                e1._animator.SetTrigger("AttackTrigger");

                FirstPersonController player = e1.nearestPlayer.GetComponentInParent<FirstPersonController>();
                if(player != null)
                {
                    e1.StartCoroutine(e1.DelayedApplyDamage(player, e1.attackDamage, e1.damageApplyDelay));
                }
                else
                {
                    Debug.LogWarning("플레이어에게 FirstPersonController 컴포넌트가 없습니다.");
                }
            }
            else
            {
                FirstPersonController player = e1.nearestPlayer.GetComponentInParent<FirstPersonController>();
                if(player != null)
                {
                    player.TakeDamage(e1.attackDamage);
                    Debug.Log($"{e1.name}근거리 공격! 데미지: {e1.attackDamage}");
                }
                else
                {
                    Debug.LogWarning("플레이어에게 FirstPersonController 컴포넌트가 없어 데미지 적용 실패.");
                }
            }
            e1.attackTimer = e1.attackCooldown;
        }

        public override void OnExit() { }
    }
}