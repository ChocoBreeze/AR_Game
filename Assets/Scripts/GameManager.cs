using Lean.Touch;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GameManager : MonoBehaviour
{
    ARRaycastManager m_RaycastManager;
    ARPlaneManager m_PlaneManager;

    public Camera arCamera;
    public Material oceanMaterial;
    public GameObject spawnPrefab;

    public GameObject Canvas; // https://artiper.tistory.com/114 (setActive 설정)
    
    private GameObject spawnedObject;
    private bool gameStart = false;
    private bool settingGameStart = false;

    public List<GameObject> cameras = new List<GameObject> { };

    public GameObject Bowl = null; // spawn된 어항 저장
    public GameObject[] fish_List; // 낚을 수 있는 물고기 리스트

    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();

    private int fish_Index;

    public ChangeButton changebutton;

    bool is_Ocean;
    bool throw_Bob;
    bool do_Tilt_game;
    bool do_Touch_game;
    bool finish_game;

    // Start is called before the first frame update
    void Start()
    {
        //m_RaycastManager = GetComponent<ARRaycastManager>();
        //m_PlaneManager = GetComponent<ARPlaneManager>();

        //Canvas.transform.Find("Progress Bar").gameObject.SetActive(false); // 낚시 아니니 canvas 끄기
        //Canvas.transform.Find("Game Bar").gameObject.SetActive(false);

        //is_Ocean = false;
        //throw_Bob = false;
        //do_Tilt_game = false;
        //do_Touch_game = false;
        //finish_game = false;

        //Bowl = null;
    }

    private void OnDisable()
    {
        foreach (var plane in m_PlaneManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }
        m_RaycastManager = GetComponent<ARRaycastManager>();
        m_PlaneManager = GetComponent<ARPlaneManager>();

        Canvas.transform.Find("Progress Bar").gameObject.SetActive(false); // 낚시 아니니 canvas 끄기
        Canvas.transform.Find("Game Bar").gameObject.SetActive(false);

        is_Ocean = false;
        throw_Bob = false;
        do_Tilt_game = false;
        do_Touch_game = false;
        finish_game = false;

        Bowl = null;
        Destroy(spawnedObject);

    }

    private void OnEnable()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
        m_PlaneManager = GetComponent<ARPlaneManager>();

        Canvas.transform.Find("Progress Bar").gameObject.SetActive(false); // 낚시 아니니 canvas 끄기
        Canvas.transform.Find("Game Bar").gameObject.SetActive(false);

        is_Ocean = false;
        throw_Bob = false;
        do_Tilt_game = false;
        do_Touch_game = false;
        finish_game = false;

        Bowl = null;

        m_RaycastManager.enabled = true;
        m_PlaneManager.enabled = true;
    }

    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!Bowl)
        {
            // Bowl = GameObject.Find("Bowl"); // Prefab에서 생성했을 때는 안 되는 건가?
            Bowl = GameObject.FindWithTag("Bowl");
        }

        if (changebutton.touchActive)
        {
            return; // true인 경우 bowl 설정
        }

        // Plane 인식 -> 바다 설정
        if (Input.touchCount > 0 && !is_Ocean) 
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray;
                RaycastHit hitobj;
                ray = arCamera.ScreenPointToRay(touch.position);

                if (m_RaycastManager.Raycast(touch.position, m_Hits, TrackableType.PlaneWithinPolygon))
                {
                    m_RaycastManager.enabled = false;
                    m_PlaneManager.enabled = false;

                    if (Physics.Raycast(ray, out hitobj))
                    {
                        if (hitobj.collider.name.Contains("Plane"))
                        {
                            hitobj.collider.GetComponent<PlaneManager>().isActive = true;
                            hitobj.collider.GetComponent<MeshRenderer>().material = oceanMaterial;
                            // hitobj.collider.gameObject.layer = 3;
                            is_Ocean = true;
                            foreach (var plane in m_PlaneManager.trackables)
                            {
                                if (plane.gameObject.GetComponent<PlaneManager>().isActive != true)
                                {
                                    plane.gameObject.SetActive(false);
                                    //t.GetComponent<MeshRenderer>().material = null;
                                }
                            }
                            return;
                        }
                    }
                }
            }
        }

        // 터치 게임 중.. 터치 개수만큼 판정해서 progress 올리기
        if (Input.touchCount > 0 && do_Touch_game) 
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Canvas.GetComponentInChildren<ProgressBar>().IncrementProgress(Input.touchCount);
            }
        }

        // 바다가 인식된 이후 낚시찌 던지기 or 이미 던져진 경우 다시 터치해서 낚시찌 회수
        if (Input.touchCount > 0 && is_Ocean) 
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (throw_Bob && !do_Tilt_game && !do_Touch_game) // 낚시찌만 던져놓고 게임은 시작 x -> 낚시찌 다시 회수
                {
                    Destroy(spawnedObject);
                    throw_Bob = false;
                }
                else if (!throw_Bob) // 낚시찌도 던지지 않은 상황
                {
                    if (m_RaycastManager.Raycast(touch.position, m_Hits, TrackableType.PlaneWithinPolygon))
                    {
                        spawnedObject = Instantiate(spawnPrefab, m_Hits[0].pose.position, m_Hits[0].pose.rotation);
                        throw_Bob = true;
                    }
                }
                return;
            }
        }

        // 물 위에 낚시찌가 던져진 상황 -> 확률에 따라 tilt game 시작
        if (is_Ocean && throw_Bob) 
        {
            if (!do_Tilt_game && !do_Touch_game)
            {
                if (!gameStart && !settingGameStart)
                {
                    InvokeRepeating("setGameStart", 0f, 2f);
                    settingGameStart = true;
                }
                else if (gameStart)
                {
                    Canvas.transform.Find("Game Bar").gameObject.SetActive(true);
                    fish_Index = Random.Range(0, fish_List.Length); // [0, fish_List.Length)
                    do_Tilt_game = true;
                    gameStart = false;
                    SetDifficultyTilt(); // tilt 게임 난이도 설정(물고기의 종류에 따라..)
                }
                else
                {
                    return;
                }
            }
        }

        // tilt game이 끝난 후 성공과 실패에 따라 진행
        if (do_Tilt_game && !Canvas.GetComponentInChildren<GameBarManager>().now_Gaming)
        {
            if (Canvas.GetComponentInChildren<GameBarManager>().result == 1) // 성공
            {
                Canvas.transform.Find("Game Bar").gameObject.SetActive(false); // 틸트 게임 끝
                do_Touch_game = true;
                do_Tilt_game = false;

                Canvas.transform.Find("Progress Bar").gameObject.SetActive(true); // 터치게임 시작.
                SetDifficultyTouch(); // Touch 게임 난이도 설정(물고기의 종류에 따라..)
                return;
            }
            else // 실패
            {
                Canvas.transform.Find("Game Bar").gameObject.SetActive(false);
                Destroy(spawnedObject); // 낚시 찌 회수
                throw_Bob = false;
                do_Tilt_game = false;
            }
        }

        // touch game이 끝난 후 성공과 실패에 따라 진행
        if (do_Touch_game && !Canvas.GetComponentInChildren<ProgressBar>().now_Gaming) // 터치 게임 끝났을 때 처리
        {
            if (Canvas.GetComponentInChildren<ProgressBar>().result == 1 && !finish_game)
            {
                // 성공 처리
                // var fish_Index = Random.Range(0, fish_List.Length); // [0, fish_List.Length)
                
                // 낚시 성공(어떤 물고기를 잡았는지 2초간 표시)
                StartCoroutine(Show_Fish(fish_Index));
                finish_game = true; // 물고기가 여러 번 표시되는 오류 해결

                // 어항 처리
                if (Bowl != null)
                {
                    Bowl.GetComponent<BowlManager>().Spawn_Fish(fish_Index);
                }

            }
            else
            {
                // 실패 처리
            }

            Canvas.transform.Find("Progress Bar").gameObject.SetActive(false); // 터치게임 끝.
            Destroy(spawnedObject); // 낚시 찌 회수
            throw_Bob = false;
            do_Touch_game = false;
        }
    }

    // 바로 tilt 게임을 시작하는 것이 아닌 입질을 기다리는 느낌을 주기 위해
    void setGameStart()
    {
        if (Random.Range(1, 11) < 3) // 확률로 시작
        {
            // 낚시(tilt 게임) 시작
            gameStart = true;
            settingGameStart = false;
            CancelInvoke("setGameStart");
        }
    }

    // 낚시 성공 시 어떤 물고기를 잡았는지 잠깐 보여주는 역할(Plane 뒤로 가는 오류는 미해결..)
    IEnumerator Show_Fish(int index)
    {
        Canvas.transform.Find("Spawn Point").Find("SuccessText").gameObject.SetActive(true);
        if (index == 0)
        {
            Canvas.transform.Find("Spawn Point").Find("Whale_HighPoly").gameObject.SetActive(true);
        }
        else if (index == 1)
        {
            Canvas.transform.Find("Spawn Point").Find("Whale_HighPoly_Purple").gameObject.SetActive(true);
        }
        else
        {
            Canvas.transform.Find("Spawn Point").Find("Whale_HighPoly_Red").gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(2.0f);
        Canvas.transform.Find("Spawn Point").Find("SuccessText").gameObject.SetActive(false);
        if (index == 0)
        {
            Canvas.transform.Find("Spawn Point").Find("Whale_HighPoly").gameObject.SetActive(false);
        }
        else if (index == 1)
        {
            Canvas.transform.Find("Spawn Point").Find("Whale_HighPoly_Purple").gameObject.SetActive(false);
        }
        else
        {
            Canvas.transform.Find("Spawn Point").Find("Whale_HighPoly_Red").gameObject.SetActive(false);
        }
        finish_game = false;
    }

    // tiltgame 난이도 설정
    private void SetDifficultyTilt() 
    {
        var difficulty = 0f;
        if(fish_Index == 0)
        {
            difficulty = 0.4f;
        }
        else if(fish_Index == 1) 
        {
            difficulty = 0.2f;
        }
        else if(fish_Index == 2)
        {
            difficulty = 0.1f;
        }
        Canvas.GetComponentInChildren<GameBarManager>().SetDifficulty(difficulty);
    }

    // touchgame 난이도 설정
    private void SetDifficultyTouch()
    {
        float lossS = 0f, fillS = 0f;
        if (fish_Index == 0)
        {
            lossS = 0.1f;
            fillS = 0.8f;
        }
        else if (fish_Index == 1)
        {
            lossS = 0.3f;
            fillS = 0.8f;
        }
        else if (fish_Index == 2)
        {
            lossS = 0.3f;
            fillS = 0.6f;
        }
        Canvas.GetComponentInChildren<ProgressBar>().SetDifficulty(lossS, fillS);
    }
}

