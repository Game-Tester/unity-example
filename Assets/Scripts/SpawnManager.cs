using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public struct PowerUpEntry
    {
        public GameObject Prefab;
        public float SpawnInterval;
    }

    [SerializeField]
    private float HorizontalBoundsRadius = 9.6f;
    [SerializeField]
    private float VerticalBoundsRadius = 5.4f;

    [SerializeField]
    private PowerUpEntry[] Spawns;

    // private
    private bool running = false;
    public bool Running
    {
        get { return running; }
        set
        {
            running = value;
            if (value)
                Start();
            else
                FindObjectOfType<UiManager>().ShowStart();
        }
    }

    // Use this for initialization
    void Start()
    {
        foreach (var s in Spawns)
            StartCoroutine(spawner(s));
    }

    IEnumerator spawner(PowerUpEntry entry)
    {
        while(running)
        {
            yield return new WaitForSeconds(entry.SpawnInterval);
            Instantiate(entry.Prefab, new Vector3(Random.Range(-HorizontalBoundsRadius, HorizontalBoundsRadius), VerticalBoundsRadius), Quaternion.identity);
        }
    }

}
