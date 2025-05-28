using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuHandler : MonoBehaviour
{
    public SecretProject Inputting;
    GameManager GameManager;
    Button Starter;
    UIDocument UI;
    // Start is called before the first frame update
    void Start()
    {
        Inputting = new SecretProject();
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        UI = GetComponent<UIDocument>();
        VisualElement root = UI.rootVisualElement;
        Starter = root.Q<Button>("start");
    }

    // Update is called once per frame
    void Update()
    {
        Starter.RegisterCallback<ClickEvent>(evt =>
        {
            GameManager.enabled = true;
            this.gameObject.SetActive(false);
        });
    }
}
