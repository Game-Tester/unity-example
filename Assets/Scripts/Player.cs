using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // public 
    public enum PowerupType { TripleShot, Speed, Shields }

    // public
    [SerializeField]
    private float HorizontalBoundsRadius = 9.6f;
    [SerializeField]
    private float VerticalBoundsRadius = 5.4f;
    [SerializeField]
    private float Speed = 5f;

    [SerializeField]
    private GameObject LaserPrefab;
    [SerializeField]
    private Vector3 LaserSpawn = new Vector3();
    [SerializeField]
    private Vector3 LaserTripleRightSpawn = new Vector3();
    [SerializeField]
    private Vector3 LaserTripleLeftSpawn = new Vector3();
    [SerializeField]
    private float FireInterval = 0.25f;

    [SerializeField]
    private float SpeedBoost = 1.5f;

    [SerializeField]
    public int Health = 3;
    [SerializeField]
    public GameObject ShieldAnimation;
    [SerializeField]
    public GameObject DestroyAnimation;
    [SerializeField]
    private GameObject[] Fires;

    //private
    private AudioSource audioLaser;
    private float lastFire = 0;
    private Dictionary<PowerupType, float> timeouts = new Dictionary<PowerupType, float>();
    private PowerupType[] PowerupTypeList = (PowerupType[])System.Enum.GetValues(typeof(PowerupType));

    //events
    private void Start()
    {
        foreach (var s in Fires)
            s.SetActive(false);

        audioLaser = GetComponent<AudioSource>();

        var ui = FindObjectOfType<UiManager>();
        ui.UpdateLives(Health);

        foreach (var s in PowerupTypeList)
        {
            timeouts.Add(s, 0);
        }
    }
    private void Update()
    {
        if (Health <= 0)
            handleDeath();
        else
        {
            handleTimeouts();
            handleMovement();
            handleFire();
            handleShields();
        }
    }

    // public
    public void AddPowerUp(PowerupType type, float timeout)
    {
        timeouts[type] += timeout;
    }
    public void RunDestroy()
    {
        if (DestroyAnimation != null)
            Instantiate(DestroyAnimation, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
    public void TakeDamage()
    {
        if (timeouts[PowerupType.Shields] == 0)
        {
            Health--;
            var ui = FindObjectOfType<UiManager>();
            ui.UpdateLives(Health);
            if (Health == 2)
                Fires[0].SetActive(true);
            if (Health == 1)
                Fires[1].SetActive(true);
        }
    }

    // private 
    private void handleMovement()
    {
        float h;
        if (transform.position.x > HorizontalBoundsRadius)
            h = -(HorizontalBoundsRadius * 2);
        else if (transform.position.x < -HorizontalBoundsRadius)
            h = HorizontalBoundsRadius * 2;
        else
        {
            h = Input.GetAxis("Horizontal") * Speed * Time.deltaTime;
            if (timeouts[PowerupType.Speed] > 0)
                h *= SpeedBoost;
        }

        float v;
        float inputV = Input.GetAxis("Vertical");
        if (transform.position.y > VerticalBoundsRadius)
            v = inputV < 0 ? inputV : 0;
        else if (transform.position.y < -VerticalBoundsRadius)
            v = inputV > 0 ? inputV : 0;
        else
        {
            v = inputV * Speed * Time.deltaTime;
            if (timeouts[PowerupType.Speed] > 0)
                h *= SpeedBoost;
        }

        transform.Translate(new Vector3(h, v));
    }
    private void handleFire()
    {
        if (Input.GetAxis("Fire") == 1f && Time.time - lastFire > FireInterval)
        {
            audioLaser.Play();
            Instantiate(LaserPrefab, transform.position + LaserSpawn, Quaternion.identity);
            lastFire = Time.time;

            if (timeouts[PowerupType.TripleShot] > 0)
            {
                Instantiate(LaserPrefab, transform.position + LaserTripleLeftSpawn, Quaternion.identity);
                Instantiate(LaserPrefab, transform.position + LaserTripleRightSpawn, Quaternion.identity);
            }
        }
    }
    private void handleTimeouts()
    {
        foreach (var s in PowerupTypeList)
        {
            if (timeouts[s] > 0)
            {
                timeouts[s] -= Time.deltaTime;
                if (timeouts[s] < 0)
                    timeouts[s] = 0;
            }
        }
    }
    private void handleShields()
    {
        if (timeouts[PowerupType.Shields] > 0f && !ShieldAnimation.activeSelf)
            ShieldAnimation.SetActive(true);
        else if (timeouts[PowerupType.Shields] == 0f && ShieldAnimation.activeSelf)
            ShieldAnimation.SetActive(false);
    }
    private void handleDeath()
    {
        var manager = FindObjectOfType<SpawnManager>();
        manager.Running = false;
        RunDestroy();
    }

}
