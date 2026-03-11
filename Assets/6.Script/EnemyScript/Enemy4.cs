using UnityEngine;

public class Enemy4 : EnemyBase
{
    private State _currentState;

    [Header("도망 AI 설정")]
    public float safeDistance = 3f;      // 플레이어와 이 거리보다 가까우면 도망
    public float detectionRange = 6f;    // 플레이어를 감지하는 거리

    protected override void Start()
    {
        base.Start();
        SetState(new PatrolState(this));
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        _currentState?.OnUpdate();
    }

    public void SetState(State next)
    {
        _currentState?.OnExit();
        _currentState = next;
        _currentState?.OnEnter();
    }

    public abstract class State
    {
        protected Enemy4 e4;
        public State(Enemy4 enemy) { e4 = enemy; }
        public abstract void OnEnter();
        public abstract void OnUpdate();
        public abstract void OnExit();
    }

    public class PatrolState : State
    {
        Vector3 dir;
        float moveTime;

        public PatrolState(Enemy4 enemy) : base(enemy) { }

        public override void OnEnter()
        {
            PickNewDirection();
        }

        void PickNewDirection()
        {
            Vector2 rand = Random.insideUnitCircle.normalized;
            dir = new Vector3(rand.x, 0, rand.y).normalized;
            moveTime = Random.Range(1f, e4._randomMovingTime);
        }

        public override void OnUpdate() // FixedUpdate 주기로 호출
        {
            if (e4.nearestPlayer == null) return;

            float dist = Vector3.Distance(e4.transform.position, e4.nearestPlayer.transform.position);

            if (dist < e4.detectionRange)
            {
                e4.SetState(new RunAwayState(e4));
                return;
            }

            e4.TryJump(dir);

            e4.RotateTowards(dir, 3f);

            e4.Move(dir, true);

            moveTime -= Time.fixedDeltaTime;
            if (moveTime <= 0)
                PickNewDirection();
        }

        public override void OnExit() { }
    }

    public class RunAwayState : State
    {
        public RunAwayState(Enemy4 enemy) : base(enemy) { }

        public override void OnEnter() { }

        public override void OnUpdate() // FixedUpdate 주기로 호출
        {
            if (e4.nearestPlayer == null) return;

            float dist = Vector3.Distance(e4.transform.position, e4.nearestPlayer.transform.position);
            Vector3 moveDir = Vector3.zero;

            if (dist < e4.safeDistance)
            {
                moveDir = (e4.transform.position - e4.nearestPlayer.transform.position).normalized;
            }
            else if (dist >= e4.detectionRange)
            {
                e4.SetState(new PatrolState(e4));
                return;
            }

            if (moveDir != Vector3.zero)
            {
                e4.TryJump(moveDir);
            }

            e4.RotateTowards(moveDir, 7f);

            if (moveDir != Vector3.zero)
            {
                e4.Move(moveDir, false);
            }
            else
            {
                e4.rb.velocity = new Vector3(0, e4.rb.velocity.y, 0);
            }
        }

        public override void OnExit() { }
    }
}