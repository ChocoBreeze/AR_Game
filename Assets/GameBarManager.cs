using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameBarManager : MonoBehaviour
{
    public GameObject accelerometer;
    public GameObject zIndicator;
    public GameObject goalSectionIndicator;
    public TextMeshProUGUI debug1;
    public TextMeshProUGUI debug2;


    private AccelerometerManager accManager;
    private RectTransform zIndicatorRect;
    private RectTransform goalSectionIndicatorRect;

    private float width;
    
    public float indicatorSize = 10;

    // Start is called before the first frame update
    void Start()
    {

        width = GetComponent<RectTransform>().rect.width;

    }

    private void Awake()
    {

        accManager = accelerometer.GetComponent<AccelerometerManager>();
        zIndicatorRect = zIndicator.GetComponent<RectTransform>();
        goalSectionIndicatorRect = goalSectionIndicator.GetComponent<RectTransform>();

    }

    // Update is called once per frame
    void Update()
    {

        //set zIndicator pos

        {
            float z = accManager.z;
            float zPortion = (z + 1f) / 2;

            //left, bottom
            zIndicatorRect.offsetMin = new Vector2(width * zPortion, 0);
            debug1.text = (width * zPortion).ToString();

            //right, top
            zIndicatorRect.offsetMax = new Vector2(-((width * (1f - zPortion)) - indicatorSize), -(0));
            debug2.text = ((width * (1f - zPortion)) - indicatorSize).ToString();

        }

        //set goalSectionIndicator pos

        {

            float sectionStart = accManager.goalInterval[0];
            float sectionFinish = accManager.goalInterval[1];
            float startPortion = (sectionStart + 1f) / 2;
            float finishPortion = (sectionFinish + 1f) / 2;

            //left, bottom
            goalSectionIndicatorRect.offsetMin = new Vector2(width * startPortion, 0);

            //right, top
            goalSectionIndicatorRect.offsetMax = new Vector2(-((width * (1f - finishPortion))), -(0));

        }




    }
}
