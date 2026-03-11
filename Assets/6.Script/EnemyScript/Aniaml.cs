using UnityEngine;

public class Aniaml : EnemyBase
{
    private State _currentState;

    [Header("도망 AI 설정")]
    public float safeDistance = 3f;      // 플레이어와 이 거리보다 가까우면 도망
    public float detectionRange = 6f;    // 플레이어 감지 거리

    protected override void Start()
    {
        base.Start();
        SetState(new PatrolState(this));
        
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        _currentState?.OnUpdate();   // Animal은 FixedUpdate에서 AI 처리
    }

    public void SetState(State next)
    {
        _currentState?.OnExit();
        _currentState = next;
        _currentState?.OnEnter();
    }

    // -----------------------------
    //  State Base Class
    // -----------------------------
    public abstract class State
    {
        protected Aniaml ani;
        public State(Aniaml enemy) { ani = enemy; }
        public abstract void OnEnter();
        public abstract void OnUpdate();
        public abstract void OnExit();
    }

    // -----------------------------
    //  순찰 상태
    // -----------------------------
    public class PatrolState : State
    {
        Vector3 dir;
        float moveTime;

        public PatrolState(Aniaml enemy) : base(enemy) { }

        public override void OnEnter()
        {
            PickNewDirection();
        }

        void PickNewDirection()
        {
            Vector2 rand = Random.insideUnitCircle.normalized;
            dir = new Vector3(rand.x, 0, rand.y).normalized;
            moveTime = Random.Range(1f, ani._randomMovingTime);
        }

        public override void OnUpdate()
        {
            // 공격받았으면 즉시 도망
            if (ani.isScared)
            {
                ani.SetState(new RunAwayState(ani));
                return;
            }

            // 랜덤 움직임
            ani.TryJump(dir);
            ani.RotateTowards(dir, 3f);
            ani.Move(dir, true);

            // 일정 시간 이동 후 방향 변경
            moveTime -= Time.fixedDeltaTime;
            if (moveTime <= 0)
                PickNewDirection();
        }

        public override void OnExit() { }
    }

    // -----------------------------
    //  도망 상태
    // -----------------------------
    public class RunAwayState : State
    {
        public RunAwayState(Aniaml enemy) : base(enemy) { }

        public override void OnEnter() { }

        public override void OnUpdate()
        {
            if (ani.nearestPlayer == null)
            {
                ani.SetState(new PatrolState(ani));
                return;
            }

            // 아직 공포 상태 유지 → 계속 도망
            if (ani.isScared)
            {
                Vector3 moveDir = (ani.transform.position - ani.nearestPlayer.transform.position).normalized;
                moveDir.y = 0;

                // 점프 및 이동
                ani.TryJump(moveDir);
                ani.RotateTowards(moveDir, 7f);
                ani.Move(moveDir, false);
                return;
            }

            // 공포 끝 → 순찰로 복귀
            ani.SetState(new PatrolState(ani));
        }

        public override void OnExit() { }
    }
}
