using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;
using UnityEngine.InputSystem.HID;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    // Pieces
    public GameObject T;
    public GameObject S;
    public GameObject Z;
    public GameObject I;
    public GameObject J;
    public GameObject L;
    public GameObject O;

    // Ghost Pieces
    public GameObject GhostT;
    public GameObject GhostS;
    public GameObject GhostZ;
    public GameObject GhostI;
    public GameObject GhostJ;
    public GameObject GhostL;
    public GameObject GhostO;

    // Misc
    public GameObject NextPieceParent;
    GameObject hi;
    GameObject HeldPiece;
    List<GameObject> NextPiece = new();
    GameObject GhostPiece;

    // Cheese
    public GameObject cheese;
    public float CheeseInterval = 5f;

    // Values for different mechanics
    float MovePiece = 0f;
    float DownTimer = 0f;
    float DelayTime = 1f;
    public float LockTime = 0f;
    float ClosestDistance = 10000f;
    float bufferDelay = 1f;
    public float MoveDelay = 0.5f; // Time u need to hold before it auto moves
    public float MoveInterval = 0.1f; // Interval between each auto move
    public float SoftDropIncrease = 0f; // Softdrop speed

    int RowCount = 0;
    int ComboCount = -1;
    int LinesCleared = 0;

    List<Vector3> SpawnPos = new();
    Vector3 HeldSpawnPos;

    bool SpawnNew = false;
    bool InARow = false;
    bool CanHold = true;
    bool isMoving = false;

    public SecretProject InputManager;
    UIHandling UIHandling;
    Process Handle;

    List<IEnumerator> ActiveMoveCoroutines = new();
    private IEnumerator coroutine;

    private void Awake()
    {
        InputManager = new SecretProject();
        UIHandling = GameObject.Find("IngameUI").GetComponent<UIHandling>();
    }

    public void SpawnCheese()
    {
        Debug.LogWarning("Spawning Cheese");
        GameObject[] objects = FindObjectsAboveRowByPosition(0);
        foreach (GameObject obj in objects)
        {
            obj.transform.Translate(0, 1, 0, Space.World);
        }
        GameObject newCheese = GameObject.Instantiate(cheese);
        newCheese.transform.position = new Vector3(5, 1, 0);

        Destroy(newCheese.transform.GetChild(Random.Range(0, newCheese.transform.childCount)).gameObject);
    }

    private GameObject RandomBlock()
    {
        int numpick = Random.Range(0, 7);
        Debug.Log(numpick);
        if (numpick == 0)
        {
            SpawnPos.Add(new Vector3(5, 20, -0.054f));
            return T;
        }
        else if (numpick == 1)
        {
            SpawnPos.Add(new Vector3(5, 20, -0.054f));
            return S;
        }
        else if (numpick == 2)
        {
            SpawnPos.Add(new Vector3(5, 20, -0.054f));
            return Z;
        }
        else if (numpick == 3)
        {
            SpawnPos.Add(new Vector3(5f, 21, -0.054f));
            return I;
        }
        else if (numpick == 4)
        {
            SpawnPos.Add(new Vector3(6, 20, -0.054f));
            return J;            
        }
        else if (numpick == 5)
        {
            SpawnPos.Add(new Vector3(6f, 20, -0.054f));
            return L;            
        }
        else if (numpick == 6)
        {
            SpawnPos.Add(new Vector3(5, 20, -0.054f));
            return O;
        }
        else
        {
            return null;
        }
        
    }
    void SpawnBlock()
    {
        hi = NextPiece.First();
        hi.GetComponent<RectTransform>().SetParent(null);
        NextPiece.RemoveAt(0);
        hi.transform.position = SpawnPos[0];
        SpawnPos.RemoveAt(0);
        Handle = hi.GetComponent<Process>();
        Handle.enabled = true;
        NextPiece.Add(GameObject.Instantiate(RandomBlock(), NextPieceParent.GetComponent<RectTransform>()));
        //NextPiece = GameObject.Instantiate(GameObject.Find("TESTT"));
        
    }
    
    void Hold()
    {
        if(HeldPiece == null)
        {
            HeldPiece = hi;
            HeldSpawnPos = SpawnPos[0];
            HeldPiece.transform.position = new Vector3(-5, 19, -0.054f);
            SpawnBlock();
        }
        else if(HeldPiece != null)
        {
            HeldPiece.transform.position = HeldSpawnPos;
            GameObject temp = HeldPiece;
            HeldPiece = hi;
            HeldSpawnPos = SpawnPos[0];
            hi = temp;
            HeldPiece.transform.position = new Vector3(-5, 19, -0.054f);
            Handle = hi.GetComponent<Process>();
            Handle.enabled = true;
        }
        CanHold = false;
        return;
    }

    void HardDrop(bool soft)
    {
        float pointer = hi.transform.position.y;
        foreach (Transform square in hi.transform)
        {
            RaycastHit2D hit = Physics2D.Raycast(square.position, Vector3.down, Mathf.Infinity, LayerMask.GetMask("ground"));
            float distanceToFall = hit.distance;

            if (ClosestDistance > distanceToFall)
            {
                ClosestDistance = distanceToFall;
            }


        }
        hi.transform.position += new Vector3(0, -ClosestDistance, 0);
        ClosestDistance = 10000f;
        float rounded = Mathf.Ceil(hi.transform.position.y);
        hi.transform.position = new Vector3(hi.transform.position.x, rounded, hi.transform.position.z);
        UIHandling.PointCount += (pointer - hi.transform.position.y) * 2;
        if (!soft)
        {
            Lock();
        }
    }
    static int FindAllOccurances(float[] array, float row)
    {
        int count = 0;
        float epsilon = 0.001f;
        foreach(float i in array)
        {
            if (Mathf.Abs(i - row) < epsilon)
            {
                count++;
            }
        }   
        return count;
    }
    GameObject[] FindObjectsInRowByPosition(float row)
    {
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Pieces");

        // Filter objects that are in the specified row
        GameObject[] objectsInRow = System.Array.FindAll(allObjects, obj => Mathf.Round(obj.transform.position.y) == row);

        return objectsInRow;
    }
    GameObject[] FindObjectsAboveRowByPosition(float row)
    {
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Pieces");

        // Filter objects that are in the specified row
        GameObject[] objectsInRow = System.Array.FindAll(allObjects, obj => Mathf.Round(obj.transform.position.y) > row);

        return objectsInRow;
    }
    void Lock()
    {
        Debug.LogWarning("Run");
        hi.tag = "Tetromino";
        for (int i = 0; i < hi.transform.childCount; i++)
        {
            Transform child = hi.transform.GetChild(i);
            child.gameObject.layer = LayerMask.NameToLayer("ground");
            child.tag = "Pieces";

            if (child.position.y >= 21)
            {
                float newScore = UIHandling.PointCount;

                // Load current high scores
                List<float> scores = new();
                //Debug.LogWarning("Old scores");
                for (int m = 0; m < 5; m++)
                {
                    scores.Add(PlayerPrefs.GetFloat("Score" + (m + 1)));
                    //Debug.LogWarning(m + ": " + scores[m]);
                }

                int score = scores.FindIndex(x => x < newScore);
                if (score != -1)
                {
                    scores.Insert(score, newScore);
                    scores.Remove(scores[5]);
                }

                //Debug.LogWarning("New Scores");
                for (int l = 0; l < 5; l++)
                {
                    //Debug.LogWarning(l + ": " + scores[l]);
                    PlayerPrefs.SetFloat("Score" + (l + 1), scores[l]);
                }
                PlayerPrefs.Save();

                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
        }
        GameObject[] Pieces = GameObject.FindGameObjectsWithTag("Pieces");
        int o = 0;
        float[] YValues = new float[10000];
        foreach (GameObject Piece in Pieces)
        {
            YValues[o] = Piece.transform.position.y;
            o++;
        }
        for (float i = 20; i >= 1; i--)
        {
            int count = FindAllOccurances(YValues, i);
            Debug.Log("Row: " + i + "\nOccurances: " + count);
            if (count >= 10)
            {
                RowCount++;
                LinesCleared++;
                GameObject[] Destroying = FindObjectsInRowByPosition(i);
                foreach (GameObject Destroyed in Destroying)
                {
                    Destroy(Destroyed.gameObject);
                }

                GameObject[] MoveObjects = FindObjectsAboveRowByPosition(i);
                foreach(GameObject MoveObject in MoveObjects)
                {
                    MoveObject.transform.Translate(0, -1, 0, Space.World);
                }
            }
        }
        if (RowCount == 1)
        {
            InARow = false;
            UIHandling.PointCount += 100 * UIHandling.Level;
            RowCount = 0;
            ComboCount++;
        }
        else if (RowCount == 2)
        {
            InARow = false;
            UIHandling.PointCount += 300 * UIHandling.Level;
            RowCount = 0;
            ComboCount++;
        }
        else if (RowCount == 3)
        {
            InARow = false;
            UIHandling.PointCount += 500 * UIHandling.Level;
            RowCount = 0;
            ComboCount++;
        }
        else if (RowCount == 4)
        {
            if (InARow == true)
            {
                UIHandling.PointCount += 800 * UIHandling.Level * 1.5f;
            }
            else if (InARow == false)
            {
                UIHandling.PointCount += 800 * UIHandling.Level;
                InARow = true;
            }
            RowCount = 0;
            ComboCount++;
        }    
        else if (RowCount == 0)
        {
            ComboCount = -1;
        }
        if (ComboCount == 2 || ComboCount == 3)
        {
            UIHandling.PointCount += 50 * 1 * UIHandling.Level;
        }
        else if (ComboCount >= 4 &&  ComboCount <= 6) 
        {
            UIHandling.PointCount += 50 * 2 * UIHandling.Level;
        }
        else if (ComboCount >= 7 && ComboCount <= 12)
        {
            UIHandling.PointCount += 50 * 3 * UIHandling.Level;
        }
        else if (ComboCount >= 13)
        {
            UIHandling.PointCount += 50 * 4 * UIHandling.Level;
        }
        CanHold = true;
        SpawnBlock();
        LockTime = 0;
        DelayTime = bufferDelay;
        if (LinesCleared >= 10)
        {
            UIHandling.Level++;
            if (DelayTime > 0.05f)
            {
                DelayTime -= 0.095f;
                bufferDelay = DelayTime;
                LinesCleared = 0;
            }
            else
            {
                LinesCleared = 0;
            }
            
        }
    }
    
    void SetAnchorBottomCenter(Transform transform)
    {
        float height = transform.localScale.y;

        Vector3 newPosition = new Vector3(transform.localPosition.x, height / 2f, transform.localPosition.z);

        transform.localPosition = newPosition;
    }

    bool IsCollision(Vector3 position, GameObject other)
    {
        Collider2D collider = other.GetComponent<Collider2D>();
        return Physics2D.OverlapPoint(position, LayerMask.GetMask("ground")) != null;
    }
    void SpawnGhostPiece()
    {
        if (GhostPiece != null)
        {
            Destroy(GhostPiece.gameObject);
        }
        if (hi.CompareTag("I"))
        {
            GhostPiece = GameObject.Instantiate(GhostI, hi.transform.position, hi.transform.rotation);
        }
        else if (hi.CompareTag("L"))
        {
            GhostPiece = GameObject.Instantiate(GhostL, hi.transform.position, hi.transform.rotation);
        }
        else if (hi.CompareTag("J"))
        {
            GhostPiece = GameObject.Instantiate(GhostJ, hi.transform.position, hi.transform.rotation);
        }
        else if (hi.CompareTag("S"))
        {
            GhostPiece = GameObject.Instantiate(GhostS, hi.transform.position, hi.transform.rotation);
        }
        else if (hi.CompareTag("Z"))
        {
            GhostPiece = GameObject.Instantiate(GhostZ, hi.transform.position, hi.transform.rotation);
        }
        else if (hi.CompareTag("T"))
        {
            GhostPiece = GameObject.Instantiate(GhostT, hi.transform.position, hi.transform.rotation);
        }
        else if (hi.CompareTag("O"))
        {
            GhostPiece = GameObject.Instantiate(GhostO, hi.transform.position, hi.transform.rotation);
        }
        while (IsValidGhostPosition())
        {
            GhostPiece.transform.Translate(0, -1, 0, Space.World);
        }
        GhostPiece.transform.Translate(0, 1, 0, Space.World);
    }
    bool IsValidGhostPosition()
    {
        foreach (Transform square in GhostPiece.transform)
        {
            Collider2D[] hits = Physics2D.OverlapPointAll(square.position);
            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject.CompareTag("Pieces") || hit.gameObject.CompareTag("Bottom"))
                {
                    return false; // Invalid position
                }
            }
            // Check if the new position is within the bounds of the grid
            if (square.position.y < 0)
            {
                return false; // Invalid position
            }
        }
        return true; // Valid position
    }
    void GhostRayCastHandler()
    {
        foreach (Transform square in hi.transform)
        {
            RaycastHit2D hit = Physics2D.Raycast(square.position, Vector3.down, Mathf.Infinity, LayerMask.GetMask("ground"));
            float distanceToFall = hit.distance;

            if (ClosestDistance > distanceToFall)
            {
                ClosestDistance = distanceToFall;
            }
        }
        GhostPiece.transform.position += new Vector3(0, -ClosestDistance, 0);
        ClosestDistance = 10000f;
        float rounded = Mathf.Ceil(GhostPiece.transform.position.y);
        GhostPiece.transform.position = new Vector3(hi.transform.position.x, rounded, hi.transform.position.z);

        foreach (Transform square in GhostPiece.transform)
        {
            GameObject[] Pieces = GameObject.FindGameObjectsWithTag("Pieces");
            foreach (GameObject piece in Pieces)
            {
                while (IsCollision(square.position, piece))
                {
                    hi.transform.Translate(0, 1, 0, Space.World);
                }
            }
            GameObject[] Ground = GameObject.FindGameObjectsWithTag("Bottom");
            foreach (GameObject groundObject in Ground)
            {
                while (IsCollision(square.position, groundObject))
                {
                    hi.transform.Translate(0, 1, 0, Space.World);
                }
            }

        }
    }
void Start()
    {
        Transform transform = I.GetComponent<Transform>();
        SetAnchorBottomCenter(transform);

        transform = J.GetComponent<Transform>();
        SetAnchorBottomCenter(transform);

        transform = L.GetComponent<Transform>();
        SetAnchorBottomCenter(transform);

        NextPiece.Add(GameObject.Instantiate(RandomBlock(), NextPieceParent.GetComponent<RectTransform>()));
        NextPiece.Add(GameObject.Instantiate(RandomBlock(), NextPieceParent.GetComponent<RectTransform>()));
        NextPiece.Add(GameObject.Instantiate(RandomBlock(), NextPieceParent.GetComponent<RectTransform>()));
        SpawnBlock();

        /*for (int i = 1; i <= 5; i++)
        {
            PlayerPrefs.SetFloat("Score" + i, 0);
        }*/

        Debug.LogWarning(PlayerPrefs.GetFloat("Score1"));
        UIHandling.Score1 = PlayerPrefs.GetFloat("Score1");
        UIHandling.Score2 = PlayerPrefs.GetFloat("Score2");
        UIHandling.Score3 = PlayerPrefs.GetFloat("Score3");
        UIHandling.Score4 = PlayerPrefs.GetFloat("Score4");
        UIHandling.Score5 = PlayerPrefs.GetFloat("Score5");

        coroutine = Moving();

        StartCoroutine(Cheesing());
    }
    private void Update()
    {
        LockTime += Time.deltaTime;
        DownTimer += 1 * Time.deltaTime;
        
        if (DownTimer >= DelayTime && Handle.Checker == false)
        {
            hi.transform.Translate(0, -1, 0, Space.World);
            UIHandling.PointCount++;
            DownTimer = 0;
            LockTime = 0;
        }
        if (InputManager.Player.SoftDrop.WasPressedThisFrame())
        {
            if (SoftDropIncrease == 0)
            {
                HardDrop(true);
            } else
            {
                DelayTime *= SoftDropIncrease;
            }
        }
        else if (InputManager.Player.SoftDrop.WasReleasedThisFrame())
        {
            if (SoftDropIncrease != 0)
            {
                DelayTime = bufferDelay;
            }
        }
        if (InputManager.Player.HardDrop.WasPressedThisFrame())
        {
            HardDrop(false);
        }
        if (InputManager.Player.Hold.WasPressedThisFrame() && CanHold == true)
        {
            Hold();
        }
        MovePiece = InputManager.Player.Move.ReadValue<float>();

        if (InputManager.Player.Move.WasPressedThisFrame())
        {
            StartMove();
        }

        if (InputManager.Player.Move.WasReleasedThisFrame() || MovePiece == 0)
        {
            StopMove();
        }

        //if (isMoving)
        //{
        //    Move();
        //    //foreach (Transform square in hi.transform)
        //    //{
        //    //    GameObject[] pieces = GameObject.FindGameObjectsWithTag("Pieces");

        //    //    foreach (GameObject piece in pieces)
        //    //    {
        //    //        if (piece.transform.position == square.position)
        //    //        {
        //    //            hi.transform.Translate(-MovePiece, 0, 0, Space.World);
        //    //        }
        //    //    }

        //    //    while (square.position.x <= 0)
        //    //    {
        //    //        hi.transform.Translate(1, 0, 0, Space.World);
        //    //    }

        //    //    while (square.position.x >= 11)
        //    //    {
        //    //        hi.transform.Translate(-1, 0, 0, Space.World);
        //    //    }
        //    //}
        //    isMoving = false;
        //}

        if (InputManager.Player.Rotate.WasPressedThisFrame() && !hi.CompareTag("O"))
        {
            float Rotating = InputManager.Player.Rotate.ReadValue<float>();
            hi.transform.Rotate(0, 0, -Rotating * 90);
            bool check = CheckAction();
            if (!check)
            {
                hi.transform.Rotate(0, 0, Rotating * 90);
            }
            //foreach (Transform square in hi.transform)
            //{
            //    GameObject[] Pieces = GameObject.FindGameObjectsWithTag("Pieces");
            //    foreach (GameObject piece in Pieces)
            //    {
            //        if (piece.transform.position == square.position)
            //        {
            //            hi.transform.Translate(-MovePiece, 0, 0, Space.World);
            //        }
            //    }
            //    while (square.position.x <= 0)
            //    {
            //        hi.transform.Translate(1, 0, 0, Space.World);
            //    }
            //    while (square.position.x >= 11)
            //    {
            //        hi.transform.Translate(-1, 0, 0, Space.World);
            //    }
            //}

        }
        if (InputManager.Player.Flip.WasPressedThisFrame() && !hi.CompareTag("O") && !hi.CompareTag("I") && !hi.CompareTag("Z") && !hi.CompareTag("S"))
        {
            hi.transform.Rotate(0, 0, 180);
            bool check = CheckAction();
            if (!check)
            {
                hi.transform.Rotate(0, 0, -180);
            }
            //foreach (Transform square in hi.transform)
            //{
            //    GameObject[] Pieces = GameObject.FindGameObjectsWithTag("Pieces");
            //    foreach (GameObject piece in Pieces)
            //    {
            //        if (piece.transform.position == square.position)
            //        {
            //            hi.transform.Translate(-MovePiece, 0, 0, Space.World);
            //        }
            //    }
            //    while (square.position.x <= 0)
            //    {
            //        hi.transform.Translate(1, 0, 0, Space.World);
            //    }
            //    while (square.position.x >= 11)
            //    {
            //        hi.transform.Translate(-1, 0, 0, Space.World);
            //    }
            //}

        }

        foreach (Transform square in hi.transform)
        {
            GameObject[] Ground = GameObject.FindGameObjectsWithTag("Bottom");
            foreach (GameObject groundObject in Ground)
            {
                while (IsCollision(square.position, groundObject))
                {
                    hi.transform.Translate(0, 1, 0, Space.World);
                }
            }
        }


        SpawnGhostPiece();
        if (Handle.Checker == true && LockTime > 1f)
        {
            Debug.LogWarning("Run again");
            LockTime = 0;
            Lock();
        }
    }

    private bool CheckAction()
    {
        foreach (Transform square in hi.transform)
        {
            Vector3 position = hi.transform.position;

            while (square.position.x <= 0)
            {
                hi.transform.Translate(1, 0, 0, Space.World);
            }
            while (square.position.x >= 11)
            {
                hi.transform.Translate(-1, 0, 0, Space.World);

            }

            GameObject[] Pieces = GameObject.FindGameObjectsWithTag("Pieces");
            foreach (GameObject piece in Pieces)
            {
                int maxUp = 0;
                while (IsCollision(square.position, piece))
                {
                    hi.transform.Translate(0, 1, 0, Space.World);
                    maxUp++;
                    if (maxUp >= 3)
                    {
                        hi.transform.position = position;
                        return false;
                    }
                }
            }
            GameObject[] Ground = GameObject.FindGameObjectsWithTag("Bottom");
            foreach (GameObject groundObject in Ground)
            {
                while (IsCollision(square.position, groundObject))
                {
                    hi.transform.Translate(0, 1, 0, Space.World);

                }
            }
        }
        return true;
    }
    private void OnEnable()
    {
        InputManager.Enable();
    }
    private void OnDisable()
    {
        InputManager.Disable();
    }
    void StartMove()
    {
        isMoving = true;
        StartCoroutine(coroutine);
    }

    void StopMove()
    {
        Debug.Log(ActiveMoveCoroutines.Count);
        //foreach (IEnumerator coroutine in ActiveMoveCoroutines)
        //{
        //    StopCoroutine(coroutine);
        //}
        //ActiveMoveCoroutines.Clear();
        StopCoroutine(coroutine);
    }
    void Move()
    {
        hi.transform.Translate(MovePiece, 0, 0, Space.World);

        foreach (Transform square in hi.transform)
        {
            Vector3 position = hi.transform.position;

            GameObject[] pieces = GameObject.FindGameObjectsWithTag("Pieces");

            foreach (GameObject piece in pieces)
            {
                if (IsCollision(square.position, piece))
                {
                    hi.transform.Translate(-MovePiece, 0, 0, Space.World);
                    break;
                }
            }

            while (square.position.x <= 0)
            {
                hi.transform.Translate(1, 0, 0, Space.World);
            }

            while (square.position.x >= 11)
            {
                hi.transform.Translate(-1, 0, 0, Space.World);
            }
        }
    }

    IEnumerator Moving()
    {
        yield return new WaitForSeconds(MoveDelay);
        while (true)
        {
            Debug.LogWarning("OnGoing");
            Move();
            yield return new WaitForSeconds(MoveInterval);
        }
    }

    IEnumerator Cheesing()
    {
        while(true)
        {
            Debug.LogWarning("Cheese!");
            SpawnCheese();
            yield return new WaitForSeconds(CheeseInterval);
        }
    }


}
