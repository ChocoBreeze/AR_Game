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

    public GameObject Canvas; // https://artiper.tistory.com/114 (setActive ����)
    
    private GameObject spawnedObject;
    private bool gameStart = false;
    private bool settingGameStart = false;

    public List<GameObject> cameras = new List<GameObject> { };
    private List<CameraManager> cameraManagers = new List<CameraManager> { };

    public GameObject Bowl = null; // spawn�� ���� ����
    public GameObject[] fish_List; // ���� �� �ִ� ����� ����Ʈ

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
        
        

        Canvas.transform.Find("Progress Bar").gameObject.SetActive(false); // ���� �ƴϴ� canvas ����
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

        if (Input.touchCount > 0 && !is_Ocean) // �ٴ� ����
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

        if (Input.touchCount > 0 && do_Touch_game) // ��ġ ���� ��..
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Canvas.GetComponentInChildren<ProgressBar>().IncrementProgress(Input.touchCount);
            }
        }

        if (Input.touchCount > 0 && is_Ocean) // ������ ������
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

        if (is_Ocean && throw_Bob) // �� ���� ����� ������ ��Ȳ
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
            if (Canvas.GetComponentInChildren<GameBarManager>().result == 1) // ����
            {
                Canvas.transform.Find("Game Bar").gameObject.SetActive(false); // ƿƮ ���� ��
                do_Touch_game = true;
                do_Tilt_game = false;

                Canvas.transform.Find("Progress Bar").gameObject.SetActive(true); // ��ġ���� ����.
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

        if (do_Touch_game && !Canvas.GetComponentInChildren<ProgressBar>().now_Gaming) // ��ġ ���� ������ �� ó��
        {
            
            if (Canvas.GetComponentInChildren<ProgressBar>().result == 1 && !finish_game)
            {
                // ���� ó��
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

                // ���� ó��
                Bowl.GetComponent<BowlManager>().Spawn_Fish(fish_Index);
                

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

        // gCanvas.GetComponentInChildren<ProgressBar>().IncrementProgress(2); (ȣ�� ����)
        // �ٴٸ� ���� �����ϴ� ����?
    }

    void setGameStart()
    {
        if (Random.Range(1, 11) < 3) // Ȯ���� ����
        {
            // ����(��ġ ����) ����
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

