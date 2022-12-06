using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
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
    private List<CameraManager> cameraManagers = new List<CameraManager> { };

    public GameObject Bowl = null; // spawn된 어항 저장
    public GameObject[] fish_List; // 낚을 수 있는 물고기 리스트

    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();

    bool is_Ocean;
    bool throw_Bob;
    bool do_Tilt_game;
    bool do_Touch_game;
    bool finish_game;

    Vector3 ScreenCenter;
    public Vector3 placeFish;


    // Start is called before the first frame update
    void Start()
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
        ScreenCenter = new Vector3(arCamera.pixelWidth / 2, arCamera.pixelHeight / 2, 2);
        placeFish = new Vector3(Camera.main.pixelWidth * 0.9f, Camera.main.pixelHeight * 0.9f);
    }

    void Awake()
    {
        for (int i = 0; i < cameras.Count; i++)
        {
            // Debug.Log(cameras.Count.ToString());
            cameraManagers.Add(cameras[i].GetComponent<CameraManager>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!Bowl)
        {
            Bowl = GameObject.Find("Bowl");
        }

        // Debug.Log("Spawn Point : " + Canvas.transform.Find("Spawn Point").position);

        if (Input.touchCount > 0 && !is_Ocean) // 바다 설정
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

        if (Input.touchCount > 0 && do_Touch_game) // 터치 게임 중..
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Canvas.GetComponentInChildren<ProgressBar>().IncrementProgress(Input.touchCount);
            }
        }

        if (Input.touchCount > 0 && is_Ocean) // 낚시찌 던지기
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

        if (is_Ocean && throw_Bob) // 물 위에 낚시찌가 던져진 상황
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
                    do_Tilt_game = true;
                    gameStart = false;
                }
                else
                {
                    return;
                }
            }

        }

        if (do_Tilt_game && !Canvas.GetComponentInChildren<GameBarManager>().now_Gaming)
        {
            if (Canvas.GetComponentInChildren<GameBarManager>().result == 1) // 성공
            {
                Canvas.transform.Find("Game Bar").gameObject.SetActive(false); // 틸트 게임 끝
                do_Touch_game = true;
                do_Tilt_game = false;

                Canvas.transform.Find("Progress Bar").gameObject.SetActive(true); // 터치게임 시작.
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

        if (do_Touch_game && !Canvas.GetComponentInChildren<ProgressBar>().now_Gaming) // 터치 게임 끝났을 때 처리
        {
            
            if (Canvas.GetComponentInChildren<ProgressBar>().result == 1 && !finish_game)
            {
                // 성공 처리
                // cameraManagers[0].showUp(); 
                var fish_Index = Random.Range(0, fish_List.Length); // [0, fish_List.Length)
                // var temp_Spawn = Instantiate(fish_List[fish_Index], Canvas.transform.Find("Spawn Point").position, Quaternion.Euler(new Vector3(0,90,0)));
                // var temp_Spawn = Instantiate(fish_List[fish_Index], spawnedObject.transform.position, Quaternion.Euler(new Vector3(0, 90, 0)));
                // var temp_Spawn = Instantiate(fish_List[fish_Index], bob_Pose, Quaternion.Euler(new Vector3(0, 90, 0)));
                // var temp_Spawn = Instantiate(fish_List[fish_Index], ScreenCenter, Quaternion.Euler(new Vector3(0, 90, 0)));
                // temp_Spawn.transform.GetChild(0).localScale = new Vector3(0.07f, 0.07f, 0.07f);
                // temp_Spawn.layer = 5;
                // Destroy(temp_Spawn, 3f);
                StartCoroutine(Show_Fish(fish_Index));
                finish_game = true;

                // 어항 처리
                Bowl.GetComponent<BowlManager>().Spawn_Fish(fish_Index);
                

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

        // gCanvas.GetComponentInChildren<ProgressBar>().IncrementProgress(2); (호출 가능)
        // 바다를 새로 설정하는 구현?
    }

    void setGameStart()
    {
        if (Random.Range(1, 11) < 3) // 확률로 시작
        {
            // 낚시(터치 게임) 시작
            gameStart = true;
            settingGameStart = false;
            CancelInvoke("setGameStart");
        }
    }

    IEnumerator Show_Fish(int index)
    {
        Canvas.transform.Find("Spawn Point").Find("SuccessText").gameObject.SetActive(true);
        if (index == 0)
        {
            Canvas.transform.Find("Spawn Point").Find("Fish").gameObject.SetActive(true);
        }
        else if (index == 1)
        {
            Canvas.transform.Find("Spawn Point").Find("Fish_Red").gameObject.SetActive(true);
        }
        else
        {
            Canvas.transform.Find("Spawn Point").Find("Fish_Purple").gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(2.0f);
        Canvas.transform.Find("Spawn Point").Find("SuccessText").gameObject.SetActive(false);
        if (index == 0)
        {
            Canvas.transform.Find("Spawn Point").Find("Fish").gameObject.SetActive(false);
        }
        else if (index == 1)
        {
            Canvas.transform.Find("Spawn Point").Find("Fish_Red").gameObject.SetActive(false);
        }
        else
        {
            Canvas.transform.Find("Spawn Point").Find("Fish_Purple").gameObject.SetActive(false);
        }
        finish_game = false;
    }

}

