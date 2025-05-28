using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Process : MonoBehaviour
{
    public bool Checker = false;
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.childCount == 0)
        {
            Destroy(this.gameObject);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pieces") || collision.gameObject.CompareTag("Bottom"))
        {
            Checker = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Checker = false;
    }
}
