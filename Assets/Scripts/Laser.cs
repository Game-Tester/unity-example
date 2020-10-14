using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private float Speed = 10f;
    [SerializeField]
    private float KillDistance = 6f;

    //events
    void Update()
    {
        transform.Translate(Vector3.up * Speed * Time.deltaTime);
        if (transform.position.y > KillDistance)
        {
            RunDestroy();
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Enemy")
        {
            var enemy = other.GetComponent<EnemyAi>();
            enemy.RunDestroy();
            RunDestroy();
        }
    }

    // public
    public void RunDestroy()
    {
        Destroy(gameObject);
    }
}
