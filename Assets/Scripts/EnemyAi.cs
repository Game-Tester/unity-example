using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAi : MonoBehaviour
{
    [SerializeField]
    private GameObject DestroyAnimation;
    [SerializeField]
    private float Speed = 3f;
    [SerializeField]
    private float HorizontalBoundsRadius = 9.6f;
    [SerializeField]
    private float VerticalBoundsRadius = 5.4f;
    [SerializeField]
    private AudioClip AudioExplode;

    // Events
    void Update()
    {
        handleMovement();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            var player = other.GetComponent<Player>();
            player.TakeDamage();
            RunDestroy();
        }
    }

    // public
    public void RunDestroy()
    {
        var ui = FindObjectOfType<UiManager>();
        ui.UpdateScore(1);
        if (DestroyAnimation != null)
            Instantiate(DestroyAnimation, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(AudioExplode, Camera.main.transform.position);
        Destroy(gameObject);
    }

    // private
    void handleMovement()
    {
        if (transform.position.y < -VerticalBoundsRadius)
        {
            var x = Random.Range(-HorizontalBoundsRadius, HorizontalBoundsRadius);
            transform.position = new Vector3(x, VerticalBoundsRadius);
        }
        else
        {
            transform.Translate(Vector3.down * Speed * Time.deltaTime);
        }

    }
}
