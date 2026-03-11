using System;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public abstract class EnemyBase : MonoBehaviour
{
    [HideInInspector] public bool isScared = false;
    [HideInInspector] public float scaredTimer = 0f;
    public float scaredDuration = 3f;   // 도망 지속 시간 (초)
    [Header("공통 설정")]
    public FirstPersonController nearestPlayer;
    public float _range = 10f;
    public float _speed = 3f;
    public float Hp = 20;
    public GameObject _itemDrop;

    [Header("랜덤 이동 설정 (공통)")]
    public float _speedRandomMove = 1f;
    public float _randomMovingTime = 2f;
    public float _randomMoveStopTime = 1f;

    [Header("점프 설정")]
    public float jumpForce = 5f;
    public float obstacleDetectDistance = 1f;

    [Header("사운드 설정")]
    public AudioSource audioSource;

    // 기본 상태(걷기, 서있기) 때 가끔 재생되는 소리
    public AudioClip[] idleSounds;

    // 피격 사운드
    public AudioClip[] hitSounds;
    public AudioClip[] deathSounds;

    // 사운드 딜레이 (지나치게 자주 재생되지 않도록)
    private float soundCooldown = 0f;
    private float soundInterval = 3f; // 기본 사운드 간격

    protected Rigidbody rb;
    // _currentMoveVelocity 변수는 rb.MovePosition 방식에서 사용되던 것으로, velocity 제어 시 필요 없습니다.

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Y축 회전을 포함하여 모든 회전을 고정하여 RotateTowards에서 직접 제어합니다.
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        //_player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<FirstPersonController>();
        //가장 가까운 플레이어 받아오게 수정 혹은 그냥 다 받아오던가 어차피 가까운 플레이어 따라가게 되어있으니.
    }

    protected virtual void Start()
    {
        if (audioSource == null)
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1f;  // 3D 사운드
    }
    

    protected virtual void Update()
    {
        FindNearestPlayer();
        CheckFallDeath();
        CheckBurnByDaylight();  // 아침이면 데미지입어가면서 죽기
         PlayIdleSound(); 

        if (isScared)
        {
            scaredTimer -= Time.deltaTime;
            if (scaredTimer <= 0)
                isScared = false;
        }
    }

    // FixedUpdate는 비워두고, Move 메서드에서 rb.velocity를 직접 제어합니다.
    protected virtual void FixedUpdate() { }

    private void CheckFallDeath()
    {
        if (transform.position.y < -20f)
        {
            Die();
        }
    }

    public void Move(Vector3 moveDir, bool isRandom = false)
    {
        moveDir.y = 0f;

        if (moveDir.sqrMagnitude < 0.001f)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }

        if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 3f))
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }

        float speedToUse = isRandom ? _speedRandomMove : _speed;
        Vector3 targetVelocity = moveDir.normalized * speedToUse;

        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
    }

    public bool TryJump(Vector3 moveDir)
    {
        if (moveDir.magnitude < 0.01f) return false;

        float bodyHeightOffset = 0.5f;
        float forwardOffset = 0.6f;

        Vector3 rayOrigin = transform.position +
                            transform.up * bodyHeightOffset +
                            transform.forward * forwardOffset;

        Vector3 horizontalDir = transform.forward;

        Vector3 tiltedDir = (horizontalDir + Vector3.down * 0.1f).normalized;

        Debug.DrawRay(rayOrigin, tiltedDir * obstacleDetectDistance, Color.red);

        if (Physics.Raycast(rayOrigin, tiltedDir, out RaycastHit hit, obstacleDetectDistance))
        {
            if (hit.collider != null && IsGrounded())
            {
                Debug.Log($"{name}이 쟁애물을 발견하고 점프 시도", gameObject);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }


    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.3f);
    }

    public virtual void RotateTowards(Vector3 dir, float rotationSpeed = 3f)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
    }

    public virtual void TakeDamage(float dmg, bool applyKnockback = true)
    {
         Hp -= dmg;
    isScared = true;
    scaredTimer = scaredDuration;

    // 사운드 재생
    if (hitSounds.Length > 0)
    {
        var clip = hitSounds[UnityEngine.Random.Range(0, hitSounds.Length)];
        PlaySFX(clip);
    }

    if (applyKnockback)
        ApplyKnockbackJump();

    if (Hp <= 0) Die();
    }


    private void ApplyKnockbackJump()
    {
        if (nearestPlayer == null) return;

        Vector3 knockDir = (transform.position - nearestPlayer.transform.position).normalized;
        knockDir.y = 0f;

        float knockForce = 25f;

        //  몬스터가 땅에 있을 때만 약간 뜨기
        float upwardForce = IsGrounded() ? 3f : 0.2f;

        Vector3 finalForce = knockDir * knockForce + Vector3.up * upwardForce;

        rb.AddForce(finalForce, ForceMode.VelocityChange);
    }

    public virtual void Die()
    {
     // 0) 더 이상 AI 로직 안 타도록 비활성화
    enabled = false;

    // 물리 속도 초기화
    if (rb != null)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;            // 물리 힘은 끄고, 우리가 직접 회전시킴
        rb.constraints = RigidbodyConstraints.FreezeRotation; // 물리 회전 잠금
    }

    // 1) BoxCollider를 "시체용" 작게 만들기
    BoxCollider box = GetComponent<BoxCollider>();
    if (box != null)
    {
        box.size   = new Vector3(box.size.x * 0.8f, 0.3f, box.size.z * 0.8f);
        box.center = new Vector3(0, 0.15f, 0);   // 바닥 쪽으로 낮춰줌
    }

   
    StartCoroutine(DeathFallRoutine());
    }

    private IEnumerator DeathFallRoutine()
    {
        // 시작/목표 회전값
        Quaternion startRot  = transform.rotation;
        // Z축으로 90도 눕히기 (옆으로 쓰러지는 느낌)
        Quaternion targetRot = startRot * Quaternion.Euler(0f, 0f, 90f);

        // 살짝 주저앉는 느낌
        Vector3 startPos  = transform.position;
        Vector3 targetPos = startPos + Vector3.down * 0.1f;

        float duration = 0.25f;   // 쓰러지는 시간 (마크처럼 짧게 "툭")
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = Mathf.SmoothStep(0f, 1f, t); // 부드럽게

            transform.rotation = Quaternion.Slerp(startRot, targetRot, k);
            transform.position = Vector3.Lerp(startPos, targetPos, k);

            yield return null;
        }

    // 1) 죽음 사운드 재생
    if (deathSounds != null && deathSounds.Length > 0)
    {
        var clip = deathSounds[UnityEngine.Random.Range(0, deathSounds.Length)];
        PlaySFX(clip);
    }

    // 2) 아이템 드롭
    if (_itemDrop)
        Instantiate(_itemDrop, transform.position + Vector3.up, Quaternion.identity);

    // 3) 사운드 재생 길이만큼 기다렸다가 삭제
    float delay = (audioSource != null && audioSource.clip != null)
        ? audioSource.clip.length
        : 2f;

    Destroy(gameObject, delay);
}


    [Header("태양 데미지 여부")]
    public bool takeSunDamage = true;

    private void CheckBurnByDaylight()
    {
        if (!takeSunDamage) return;
        if (MinecraftDayNightCycle.IsNight) return;

        float burnDamage = 5f * Time.deltaTime;
        TakeDamage(burnDamage, false);   // 태양 데미지는 넉백 X
    }

    private void FindNearestPlayer()
    {
        FirstPersonController[] players = FindObjectsOfType<FirstPersonController>();
        if (players.Length == 0) 
        {
            nearestPlayer = null;
            return;
        }

        float nearestDist = float.MaxValue;
        FirstPersonController nearest = null;

        foreach (var p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);

            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = p;
            }
        }

        nearestPlayer = nearest;
    }
    private void PlayIdleSound()
    {
        if (idleSounds == null || idleSounds.Length == 0) return;

        soundCooldown -= Time.deltaTime;
        if (soundCooldown > 0) return;

        soundCooldown = UnityEngine.Random.Range(soundInterval * 0.7f, soundInterval * 1.3f);

        int index = UnityEngine.Random.Range(0, idleSounds.Length);
        audioSource.PlayOneShot(idleSounds[index]);
    }
    // private void PlayHitSound()
    // {
    //     if (hitSounds == null || hitSounds.Length == 0) return;

    //     int index = UnityEngine.Random.Range(0, hitSounds.Length);
    //     audioSource.PlayOneShot(hitSounds[index]);
    // }
    // private void PlayDeathSound()
    // {
    //     if (deathSounds == null || deathSounds.Length == 0) return;

    //     int index = UnityEngine.Random.Range(0, deathSounds.Length);
    //     audioSource.PlayOneShot(deathSounds[index]);
    // }
    public void PlaySFX(AudioClip clip)
{
    if (clip == null) return;

    if (audioSource != null)
    {
        audioSource.volume = SoundManager.Instance.sfx.volume; // SFX 설정 따라가기
        audioSource.PlayOneShot(clip);
    }
    else
    {
        // audioSource가 없다면 2D SFX로라도 재생
        SoundManager.Instance.sfx.PlayOneShot(clip);
    }
}
}