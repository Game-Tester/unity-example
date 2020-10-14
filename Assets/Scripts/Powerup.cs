using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField]
    private float VerticalBoundsRadius = 5.4f;
    [SerializeField]
    private float MoveSpeed = 3f;
    [SerializeField]
    private float TimeoutSeconds = 2f;
    [SerializeField]
    private Player.PowerupType Type;
    [SerializeField]
    private AudioClip sound;

    //events
    void Update()
    {
        transform.Translate(Vector3.down * MoveSpeed * Time.deltaTime);
        if (transform.position.y < -VerticalBoundsRadius)
            RunDestroy();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            var player = other.GetComponent<Player>();
            player.AddPowerUp(Type, TimeoutSeconds);
            AudioSource.PlayClipAtPoint(sound, Camera.main.transform.position);
            var ui = FindObjectOfType<UiManager>();
            ui.UpdatePickups(1);
            RunDestroy();
        }
    }

    // public
    public void RunDestroy()
    {
        Destroy(gameObject);
    }
}
