using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChangeButton : MonoBehaviour
{
    public bool touchActive; // true -> bowl, false -> plane
    public TMP_Text text;
    // Start is called before the first frame update
    void Start()
    {
        touchActive = true;
    }

    // bowl <-> gamemanager (터치 제어)
    public void ChangeState()
    {
        touchActive = !touchActive; // 반전
        if(touchActive)
        {
            text.text = "Change State\n(now : Bowl)";
        }
        else
        {
            text.text = "Change State\n(now : Plane)";
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
