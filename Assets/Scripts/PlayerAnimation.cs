using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField]
    private Animator anim;

    // private
    private bool goingRight = false;
    private bool goingLeft = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        var h = Input.GetAxis("Horizontal");
        var right = h > 0;
        var left = h < 0;

        if (right && !goingRight)
        {
            anim.SetBool("TurnRight", true);
            goingRight = true;
        }
        if (!right && goingRight)
        {
            anim.SetBool("TurnRight", false);
            goingRight = false;
        }
        
        if (left && !goingLeft)
        {
            anim.SetBool("TurnLeft", true);
            goingLeft = true;
        }
        if (!left && goingLeft)
        {
            anim.SetBool("TurnLeft", false);
            goingLeft = false;
        }
    }
}
