using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    // https://www.youtube.com/watch?v=UCAo-uyb94c (ProgressBar)
    // https://prosto.tistory.com/247 (Awake, Start, OnEnable, OnDisable)
    private Slider slider;
    private ParticleSystem particleSys;
    private Stopwatch watch;
    public float failureTime;
    public float lossSpeed; // 깎이는 속도
    public float fillSpeed; // 차는 속도
    private float targetProgress; // full

    public int result; // 실패 = 0, 성공 = 1
    public bool now_Gaming = false;

    private void Awake()
    {
        slider = gameObject.GetComponent<Slider>();
        particleSys = GameObject.Find("Progress Bar Particles").GetComponent<ParticleSystem>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        now_Gaming = true;
        slider.value = 0.6f; // 시작점 조절
        lossSpeed = 0.1f;
        fillSpeed = 0.8f;
        targetProgress = 1.0f;
        result = 0;
        failureTime = 10; // 10초
        watch = new Stopwatch();
        watch.Restart();
    }



    // Update is called once per frame
    void Update()
    {

        // UnityEngine.Debug.Log("slider.value : " + slider.value);
        //Debug.Log("loss Speed : " + lossSpeed);
        if (slider.value > 0)
        {
            // slider.value -= 0.1f * Time.deltaTime;
            slider.value -= lossSpeed * Time.deltaTime; // 0보다 작은거 걱정 안해도 됨
        }
        if (slider.value < targetProgress)
        {
            // slider.value += fillSpeed * Time.deltaTime;
            if (!particleSys.isPlaying)
            {
                particleSys.Play();
            }
        }
        if (slider.value > 0.98f) // 성공
        {
            //UnityEngine.Debug.Log("now_Gaming : " + now_Gaming);
            //UnityEngine.Debug.Log("result : " + result);
            now_Gaming = false;
            result = 1;
        }
        if (watch.Elapsed.TotalMilliseconds > failureTime * 1000 || slider.value == 0) // 실패
        {
            now_Gaming = false;
            result = 0;
        }

        // IncrementProgress(1);
    }

    public void IncrementProgress(int touchCount)
    {
        slider.value += touchCount * fillSpeed * Time.deltaTime;
    }

    public void SetDifficulty(float loss_s, float fill_s)
    {
        lossSpeed = loss_s;
        fillSpeed = fill_s;
    }

}
