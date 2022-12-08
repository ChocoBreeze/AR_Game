using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{

    public GameObject gameManagerObj;
    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        gameManager= gameManagerObj.GetComponent<GameManager>();
    }

    // gamemanager ²¯´Ù ÄÑ±â
    public void resetGameManager()
    {
        gameManager.enabled = false;
        gameManager.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
