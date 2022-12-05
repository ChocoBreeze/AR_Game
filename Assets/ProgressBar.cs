using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    // https://www.youtube.com/watch?v=UCAo-uyb94c (ProgressBar)
    // https://prosto.tistory.com/247 (Awake, Start, OnEnable, OnDisable)
    private Slider slider;
    private ParticleSystem particleSys;

    public float lossSpeed; // ���̴� �ӵ�
    public float fillSpeed; // ���� �ӵ�
    private float targetProgress; // full
    public int result; // ���� = 0, ���� = 1

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
        slider.value = 0.2f; // ������ ����
        lossSpeed = 0.1f;
        fillSpeed = 0.4f;
        targetProgress = 1.0f;
        result = 0;
    }



    // Update is called once per frame
    void Update()
    {

        //Debug.Log("slider.value : " + slider.value);
        //Debug.Log("loss Speed : " + lossSpeed);
        if (slider.value > 0)
        {
            // slider.value -= 0.1f * Time.deltaTime;
            slider.value -= lossSpeed * Time.deltaTime; // 0���� ������ ���� ���ص� ��
        }
        if(slider.value < targetProgress)
        {
            // slider.value += fillSpeed * Time.deltaTime;
            if(!particleSys.isPlaying)
            {
                particleSys.Play();
            }
        }
        if(slider.value > 0.99f) // ����
        {
            now_Gaming = false;
            result = 1;
        }
        if(slider.value == 0) // ��ħ
        {
            // Debug.Log("GameOver");
            now_Gaming = false;
            result = 0;
        }
        // IncrementProgress(2);
    }

    public void IncrementProgress(int touchCount)
    {
        slider.value += touchCount * fillSpeed * Time.deltaTime;
    }

    
}
