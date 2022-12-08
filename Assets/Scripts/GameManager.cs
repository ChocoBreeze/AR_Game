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

    public GameObject Canvas; // https://artiper.tistory.com/114 (setActive ����)
    
    private GameObject spawnedObject;
    private bool gameStart = false;
    private bool settingGameStart = false;

    public List<GameObject> cameras = new List<GameObject> { };

    public GameObject Bowl = null; // spawn�� ���� ����
    public GameObject[] fish_List; // ���� �� �ִ� ����� ����Ʈ

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

        //Canvas.transform.Find("Progress Bar").gameObject.SetActive(false); // ���� �ƴϴ� canvas ����
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

        Canvas.transform.Find("Progress Bar").gameObject.SetActive(false); // ���� �ƴϴ� canvas ����
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

        Canvas.transform.Find("Progress Bar").gameObject.SetActive(false); // ���� �ƴϴ� canvas ����
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
            // Bowl = GameObject.Find("Bowl"); // Prefab���� �������� ���� �� �Ǵ� �ǰ�?
            Bowl = GameObject.FindWithTag("Bowl");
        }

        if (changebutton.touchActive)
        {
            return; // true�� ��� bowl ����
        }

        // Plane �ν� -> �ٴ� ����
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

        // ��ġ ���� ��.. ��ġ ������ŭ �����ؼ� progress �ø���
        if (Input.touchCount > 0 && do_Touch_game) 
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Canvas.GetComponentInChildren<ProgressBar>().IncrementProgress(Input.touchCount);
            }
        }

        // �ٴٰ� �νĵ� ���� ������ ������ or �̹� ������ ��� �ٽ� ��ġ�ؼ� ������ ȸ��
        if (Input.touchCount > 0 && is_Ocean) 
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (throw_Bob && !do_Tilt_game && !do_Touch_game) // ����� �������� ������ ���� x -> ������ �ٽ� ȸ��
                {
                    Destroy(spawnedObject);
                    throw_Bob = false;
                }
                else if (!throw_Bob) // ����� ������ ���� ��Ȳ
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

        // �� ���� ����� ������ ��Ȳ -> Ȯ���� ���� tilt game ����
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
                    SetDifficultyTilt(); // tilt ���� ���̵� ����(������� ������ ����..)
                }
                else
                {
                    return;
                }
            }
        }

        // tilt game�� ���� �� ������ ���п� ���� ����
        if (do_Tilt_game && !Canvas.GetComponentInChildren<GameBarManager>().now_Gaming)
        {
            if (Canvas.GetComponentInChildren<GameBarManager>().result == 1) // ����
            {
                Canvas.transform.Find("Game Bar").gameObject.SetActive(false); // ƿƮ ���� ��
                do_Touch_game = true;
                do_Tilt_game = false;

                Canvas.transform.Find("Progress Bar").gameObject.SetActive(true); // ��ġ���� ����.
                SetDifficultyTouch(); // Touch ���� ���̵� ����(������� ������ ����..)
                return;
            }
            else // ����
            {
                Canvas.transform.Find("Game Bar").gameObject.SetActive(false);
                Destroy(spawnedObject); // ���� �� ȸ��
                throw_Bob = false;
                do_Tilt_game = false;
            }
        }

        // touch game�� ���� �� ������ ���п� ���� ����
        if (do_Touch_game && !Canvas.GetComponentInChildren<ProgressBar>().now_Gaming) // ��ġ ���� ������ �� ó��
        {
            if (Canvas.GetComponentInChildren<ProgressBar>().result == 1 && !finish_game)
            {
                // ���� ó��
                // var fish_Index = Random.Range(0, fish_List.Length); // [0, fish_List.Length)
                
                // ���� ����(� ����⸦ ��Ҵ��� 2�ʰ� ǥ��)
                StartCoroutine(Show_Fish(fish_Index));
                finish_game = true; // ����Ⱑ ���� �� ǥ�õǴ� ���� �ذ�

                // ���� ó��
                if (Bowl != null)
                {
                    Bowl.GetComponent<BowlManager>().Spawn_Fish(fish_Index);
                }

            }
            else
            {
                // ���� ó��
            }

            Canvas.transform.Find("Progress Bar").gameObject.SetActive(false); // ��ġ���� ��.
            Destroy(spawnedObject); // ���� �� ȸ��
            throw_Bob = false;
            do_Touch_game = false;
        }
    }

    // �ٷ� tilt ������ �����ϴ� ���� �ƴ� ������ ��ٸ��� ������ �ֱ� ����
    void setGameStart()
    {
        if (Random.Range(1, 11) < 3) // Ȯ���� ����
        {
            // ����(tilt ����) ����
            gameStart = true;
            settingGameStart = false;
            CancelInvoke("setGameStart");
        }
    }

    // ���� ���� �� � ����⸦ ��Ҵ��� ��� �����ִ� ����(Plane �ڷ� ���� ������ ���ذ�..)
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

    // tiltgame ���̵� ����
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

    // touchgame ���̵� ����
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

