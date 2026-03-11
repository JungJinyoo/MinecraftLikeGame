using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Projectile : MonoBehaviour
{
    Rigidbody rb;
    float _damage;
    bool _stuck = false;

    public void Init(Vector3 dir, float speed, float damage)
    {
        rb = GetComponent<Rigidbody>();
        _damage = damage;

        if(_damage <= 0)
        {
            _damage = 10f;
        }

        if (rb != null)
        {
            rb.velocity = dir * speed;
            rb.useGravity = false;
        }

        transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(90f, 0f, 0f);

        Destroy(gameObject, 2f);
    }

    void FixedUpdate()
    {
        if (!_stuck && rb != null && rb.velocity.sqrMagnitude > 0.01f)
        {
            transform.rotation =
                Quaternion.LookRotation(rb.velocity.normalized) * Quaternion.Euler(90f, 0f, 0f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (_stuck) return;

        FirstPersonController player = other.GetComponentInParent<FirstPersonController>();
        if(player != null)
        {
            Debug.Log("플레이어가 데미지를 받았습니다! 데미지: " + _damage);
            player.TakeDamage(_damage);

            Destroy(gameObject);
            return;
        }

        if(!other.isTrigger)
        {
            StickToSurface(other);
        }
    }

    void StickToSurface(Collider surface)
    {
        _stuck = true;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        transform.SetParent(surface.transform);

        float deleteTime = Random.Range(1f, 2f);
        Destroy(gameObject, deleteTime);
    }
}