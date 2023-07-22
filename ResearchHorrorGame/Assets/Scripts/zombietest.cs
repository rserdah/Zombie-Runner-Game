using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zombietest : MonoBehaviour
{
    private Animator anim;
    public Color[] colors;


    private void Start()
    {
        anim = GetComponent<Animator>();

        //anim.SetFloat("speed", 2.25f + Random.Range(-0.05f, 0.05f));

        transform.localScale = transform.localScale.x * Random.Range(0.9f, 1.1f) * Vector3.one;

        anim.Play("zombie run");

        /*switch(Random.Range(0, 2))
        {
            case 0:
                anim.Play("running crawl");
                break;

            case 1:
                anim.Play("zombie run");
                break;

            default:
                anim.Play("zombie run");
                break;
        }*/

        int i = Random.Range(0, colors.Length);

        GetComponentInChildren<Renderer>().material.color = colors[i];
    }
}
