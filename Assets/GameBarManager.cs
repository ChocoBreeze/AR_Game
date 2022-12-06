using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class GameBarManager : MonoBehaviour
{
    public float timeInterval = 0.1f;
    public float failureTime = 5f;
    public float successTime = 3f;
    public float z;
    public List<float> goalInterval = new List<float> { };

    private int numOfInterval;
    private int numOfFailure = 0;
    private Stopwatch correctWatch;
    private Stopwatch inCorrectWatch;

    public GameObject accelerometer;
    public GameObject zIndicator;
    public GameObject goalSectionIndicator;
    // public TextMeshProUGUI debug1;
    // public TextMeshProUGUI debug2;


    private RectTransform zIndicatorRect;
    private RectTransform goalSectionIndicatorRect;

    private float width;

    public float indicatorSize = 10;

    public int result; // 실패 = 0, 성공 = 1
    public bool now_Gaming = false;

    // Start is called before the first frame update
    void Start()
    {
        
        numOfInterval = (int)(2 / timeInterval);
        //UnityEngine.Debug.Log(zText.text);
        //zText.text = "asdasd";
        //UnityEngine.Debug.Log(zText.text);
        setGoalInterval();
    }

    private void Awake()
    {
        // width = GetComponent<RectTransform>().rect.width;
        zIndicatorRect = zIndicator.GetComponent<RectTransform>();
        goalSectionIndicatorRect = goalSectionIndicator.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        indicatorSize = 10;
        width = GetComponent<RectTransform>().rect.width;
        now_Gaming = true;
        correctWatch = new Stopwatch();
        inCorrectWatch = new Stopwatch();
        inCorrectWatch.Reset();
        correctWatch.Reset();
        numOfFailure = 0;
        setGoalInterval();
        result = 0;
    }



    // Update is called once per frame
    void Update()
    {

        if (!now_Gaming) { return; }

        z = Input.acceleration.z;

        if (z > goalInterval[0] && z < goalInterval[1])
        {
            //목표 구간 안에 있을 때
            inCorrectWatch.Reset();
            if (correctWatch.Elapsed.TotalMilliseconds == 0)
            {
                correctWatch.Start();
            }
        }
        else
        {
            //목표 구간 안에 없을 때
            correctWatch.Reset();
            if (inCorrectWatch.Elapsed.TotalMilliseconds == 0)
            {
                inCorrectWatch.Start();
            }
        }

        if (inCorrectWatch.Elapsed.TotalMilliseconds > failureTime * 1000)
        {
            //실패
            numOfFailure += 1;
            inCorrectWatch.Reset();
            setGoalInterval();

            if (numOfFailure > 2)
            {
                result = 0;
                now_Gaming = false;

            }

            // numOfFailureText.text = numOfFailure.ToString();

        }
        else if (correctWatch.Elapsed.TotalMilliseconds > successTime * 1000)
        {
            //성공
            result = 1;
            now_Gaming = false;
            // numOfSuccessText.text = numOfSuccess.ToString();

        }

        //set zIndicator pos

        {
            float zPortion = (z + 1f) / 2;

            //left, bottom
            zIndicatorRect.offsetMin = new Vector2(width * zPortion, 0);
            // debug1.text = (width * zPortion).ToString();

            //right, top
            zIndicatorRect.offsetMax = new Vector2(-((width * (1f - zPortion)) - indicatorSize), -(0));
            // debug2.text = ((width * (1f - zPortion)) - indicatorSize).ToString();

        }

        //set goalSectionIndicator pos

        {

            float sectionStart = goalInterval[0];
            float sectionFinish = goalInterval[1];
            float startPortion = (sectionStart + 1f) / 2;
            float finishPortion = (sectionFinish + 1f) / 2;

            //left, bottom
            goalSectionIndicatorRect.offsetMin = new Vector2(width * startPortion, 0);

            //right, top
            goalSectionIndicatorRect.offsetMax = new Vector2(-((width * (1f - finishPortion))), -(0));

        }
    }

    void setGoalInterval()
    {
        int tmp = Random.Range(0, numOfInterval);
        if (goalInterval.Count == 0)
        {
            goalInterval.Add(-1f + tmp * timeInterval);
            goalInterval.Add(goalInterval[0] + timeInterval);

        }
        else
        {
            goalInterval[0] = -1f + tmp * timeInterval;
            goalInterval[1] = goalInterval[0] + timeInterval;
        }

    }

}
