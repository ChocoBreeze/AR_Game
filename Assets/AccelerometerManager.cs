using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Diagnostics;
using UnityEngine.UI;

public class AccelerometerManager : MonoBehaviour
{

    public TextMeshProUGUI zText;
    public TextMeshProUGUI correctSectionText;
    public TextMeshProUGUI correctWatchText;
    public TextMeshProUGUI inCorrectWatchText;
    public TextMeshProUGUI numOfSuccessText;
    public TextMeshProUGUI numOfFailureText;


    public float timeInterval = 0.1f;
    public float failureTime = 5f;
    public float successTime = 3f;
    public float z;
    public List<float> goalInterval = new List<float> { };


    private int numOfInterval;
    private int numOfSuccess = 0;
    private int numOfFailure = 0;
    private Stopwatch correctWatch;
    private Stopwatch inCorrectWatch;

    // Start is called before the first frame update
    void Start()
    {
        correctWatch = new Stopwatch();
        inCorrectWatch = new Stopwatch();
        numOfInterval = (int) (2 / timeInterval);
        UnityEngine.Debug.Log(zText.text);
        zText.text = "asdasd";
        UnityEngine.Debug.Log(zText.text);
        setGoalInterval();
        UnityEngine.Debug.Log(correctWatch.Elapsed.TotalMilliseconds.ToString());

        UnityEngine.Debug.Log(inCorrectWatch.Elapsed.TotalMilliseconds.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        z = Input.acceleration.z;

        if(z > goalInterval[0] && z < goalInterval[1])
        {
            //목표 구간 안에 있을 때
            inCorrectWatch.Reset();
            if (correctWatch.Elapsed.TotalMilliseconds == 0)
            {
                correctWatch.Start();
            }
        } else
        {
            //목표 구간 안에 없을 때
            correctWatch.Reset();
            if (inCorrectWatch.Elapsed.TotalMilliseconds == 0)
            {
                inCorrectWatch.Start();
            }
        }
        
        if(inCorrectWatch.Elapsed.TotalMilliseconds > failureTime * 1000)
        {
            //실패
            numOfFailure += 1;
            inCorrectWatch.Reset();
            setGoalInterval();
            numOfFailureText.text = numOfFailure.ToString();

        } else if (correctWatch.Elapsed.TotalMilliseconds > successTime * 1000)
        {
            //성공
            numOfSuccess += 1;
            correctWatch.Reset();
            setGoalInterval();
            numOfSuccessText.text = numOfSuccess.ToString();

        }
         

        zText.text = z.ToString();
        correctWatchText.text = correctWatch.Elapsed.TotalMilliseconds.ToString();
        inCorrectWatchText.text = inCorrectWatch.Elapsed.TotalMilliseconds.ToString();
        correctSectionText.text = goalInterval[0].ToString() + "~ " + goalInterval[1].ToString();
    }

    void setGoalInterval()
    {
        int tmp = Random.Range(0, numOfInterval);
        if (goalInterval.Count == 0)
        {
            goalInterval.Add(-1f + tmp * timeInterval);
            goalInterval.Add(goalInterval[0] + timeInterval);

        } else {
            goalInterval[0] = -1f + tmp * timeInterval;
            goalInterval[1] = goalInterval[0] + timeInterval;
        }

    }

}
