using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class RayCast : MonoBehaviour
{
    ARRaycastManager m_RaycastManager;
    ARPlaneManager m_PlaneManager;


    public Camera arCamera;
    public Material oceanMaterial;
    public GameObject spawnPrefab;
    public GameObject gCanvas; // https://artiper.tistory.com/114 (setActive 설정)

    private GameObject spawnedObject;

    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();

    bool throw_Bob;
    bool is_Ocean;
    bool do_Touch_game;

    // Start is called before the first frame update
    void Start()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
        m_PlaneManager = GetComponent<ARPlaneManager>();

        
        gCanvas.transform.Find("Progress Bar").gameObject.SetActive(false); // 낚시 아니니 canvas 끄기

        is_Ocean = false;
        throw_Bob = false;
        do_Touch_game= false;
    }

    // Update is called once per frame
    void Update()
    {
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
                gCanvas.GetComponentInChildren<ProgressBar>().IncrementProgress(Input.touchCount);
            }
        }

        if (Input.touchCount > 0 && is_Ocean) // 낚시찌 던지기
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (throw_Bob && !do_Touch_game) // 낚시찌만 던져놓고 게임은 시작 x -> 낚시찌 다시 회수
                {
                    Destroy(spawnedObject);
                    throw_Bob = false;
                }
                else if(!throw_Bob) // 낚시찌도 던지지 않은 상황
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

        if(is_Ocean && throw_Bob) // 물 위에 낚시찌가 던져진 상황
        {
            if(!do_Touch_game) // 게임이 시작되지 않은 경우
            {
                if (Random.Range(1, 11) < 3) // 확률로 시작
                {
                    // 낚시(터치 게임) 시작
                    gCanvas.transform.Find("Progress Bar").gameObject.SetActive(true);
                    do_Touch_game = true;
                    return;
                }
            }
        }

        if (do_Touch_game && !gCanvas.GetComponentInChildren<ProgressBar>().now_Gaming) // 터치 게임 끝났을 때 처리
        {
            if(gCanvas.GetComponentInChildren<ProgressBar>().result == 1)
            {
                // 성공 처리
            } 
            else
            {
                // 실패 처리
            }
            
            gCanvas.transform.Find("Progress Bar").gameObject.SetActive(false); // 낚시 끝.
            Destroy(spawnedObject); // 낚시 찌 회수
            throw_Bob = false;
            do_Touch_game = false;
        }

        // gCanvas.GetComponentInChildren<ProgressBar>().IncrementProgress(2); (호출 가능)
        // 바다를 새로 설정하는 구현?
    }
}