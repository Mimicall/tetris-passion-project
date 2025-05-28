using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    public LayerMask groundlayer;
    Process ParentProcess;
    // Start is called before the first frame update
    void Start()
    {
        ParentProcess = GetComponentInParent<Process>();
        // Check if the ray hits something on the ground layer

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Pieces") || collision.gameObject.CompareTag("Bottom") || collision.gameObject.CompareTag("Tetromino"))
        {
            if (Physics2D.BoxCast(GetComponent<CircleCollider2D>().bounds.center, GetComponent<CircleCollider2D>().bounds.size, 0f, Vector2.down, .2f, groundlayer))
            {
                ParentProcess.Checker = true;
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pieces") || collision.gameObject.CompareTag("Bottom") || collision.gameObject.CompareTag("Tetromino"))
        {
            if (!Physics2D.BoxCast(GetComponent<CircleCollider2D>().bounds.center, GetComponent<CircleCollider2D>().bounds.size, 0f, Vector2.down, .2f, groundlayer))
            {
                ParentProcess.Checker = false;
            }
        }
    }
}
