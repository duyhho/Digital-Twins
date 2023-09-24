using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class openandclosedoor : MonoBehaviour
{

    public Animator openandclose1;
    public bool open;
    // public Transform Player;

    void Start()
    {
        open = false;
    }

    void OnMouseOver()
    {


        if (open == false)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(opening());
            }
        }
        else
        {
            if (open == true)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    StartCoroutine(closing());
                }
            }

        }




    }

    public void OpenDoor()
    {
        if (open == false)
        {
            StartCoroutine(opening());

        }
    }
    public void CloseDoor()
    {
        if (open)
        {
            StartCoroutine(closing());

        }
    }

    IEnumerator opening()
    {
        print("you are opening the door");
        openandclose1.Play("Opening 1");
        open = true;
        yield return new WaitForSeconds(.5f);
    }

    IEnumerator closing()
    {
        print("you are closing the door");
        openandclose1.Play("Closing 1");
        open = false;
        yield return new WaitForSeconds(.5f);
    }


}
