using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandling : MonoBehaviour
{
    public GameManager GameManager;
    UIDocument UI;

    UnityEngine.UIElements.Label Scoring;
    UnityEngine.UIElements.Label Highscores;
    
    public float PointCount = 0;
    public float Level = 1;
    public float Score1 = 0;
    public float Score2 = 0;
    public float Score3 = 0;
    public float Score4 = 0;
    public float Score5 = 0;
    // Start is called before the first frame update
    void Start()
    {
        UI = GetComponent<UIDocument>();
        VisualElement root = UI.rootVisualElement;
        Scoring = root.Q<UnityEngine.UIElements.Label>("Info");
        Highscores = root.Q<UnityEngine.UIElements.Label>("Highscore");
    }

    // Update is called once per frame
    void Update()
    {
        Scoring.text = "Score:\n" + PointCount.ToString("N0") + "\nLevel:\n" + Level;
        Highscores.text = "Highscore:\n1st. " + Score1.ToString("N0") + "\n2nd. " + Score2.ToString("N0") + "\n3rd. " + Score3.ToString("N0") + "\n4th. " + Score4.ToString("N0") + "\n5th. " + Score5.ToString("N0");
    }
}
