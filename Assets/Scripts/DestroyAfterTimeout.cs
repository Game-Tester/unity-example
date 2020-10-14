using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTimeout : MonoBehaviour
{
    [SerializeField]
    private float TimeOut = 1f;

    void Start()
    {
        StartCoroutine(delayedDestroy());
    }

    IEnumerator delayedDestroy()
    {
        yield return new WaitForSeconds(TimeOut);
        Destroy(gameObject);
    }
}
