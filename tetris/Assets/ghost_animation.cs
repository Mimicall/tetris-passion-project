using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public class ghost_animation : MonoBehaviour
{
    Transform parent;

    public Sprite Ghost;
    public Sprite Square;

    GameObject[] children;

    int[] indexGhost = new int[100];

    // Start is called before the first frame update
    void Start()
    {
        parent = GetComponent<Transform>();

        StartCoroutine(ToggleGhost());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ToggleGhost()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (indexGhost.Length > 0)
            {
                for (int i = 0; i < indexGhost.Length; i++)
                {
                    Transform childObject = parent.GetChild(indexGhost[i]);
                    foreach (Transform child in childObject)
                    {
                        child.gameObject.GetComponent<SpriteRenderer>().sprite = Square;
                    }
                }
                indexGhost = new int[100];
            }

            for (int i = 0; i < 20; i++)
            {
                int childNum = Random.Range(0, parent.childCount);
                if (indexGhost.Contains(childNum))
                {
                    i--;
                    continue;
                }

                Transform childObject = parent.GetChild(childNum);
                foreach (Transform child in childObject)
                {
                    child.gameObject.GetComponent<SpriteRenderer>().sprite = Ghost;
                }
                indexGhost[i] = childNum;

                //yield return new WaitForSeconds(0.1f);
            }
            
        }
    }
}
