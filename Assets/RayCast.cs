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
    public GameObject gCanvas; // https://artiper.tistory.com/114 (setActive ����)

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

        
        gCanvas.transform.Find("Progress Bar").gameObject.SetActive(false); // ���� �ƴϴ� canvas ����

        is_Ocean = false;
        throw_Bob = false;
        do_Touch_game= false;
    }

    // Update is called once per frame
    void Update()
    {
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
                gCanvas.GetComponentInChildren<ProgressBar>().IncrementProgress(Input.touchCount);
            }
        }

        if (Input.touchCount > 0 && is_Ocean) // ������ ������
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (throw_Bob && !do_Touch_game) // ����� �������� ������ ���� x -> ������ �ٽ� ȸ��
                {
                    Destroy(spawnedObject);
                    throw_Bob = false;
                }
                else if(!throw_Bob) // ����� ������ ���� ��Ȳ
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

        if(is_Ocean && throw_Bob) // �� ���� ����� ������ ��Ȳ
        {
            if(!do_Touch_game) // ������ ���۵��� ���� ���
            {
                if (Random.Range(1, 11) < 3) // Ȯ���� ����
                {
                    // ����(��ġ ����) ����
                    gCanvas.transform.Find("Progress Bar").gameObject.SetActive(true);
                    do_Touch_game = true;
                    return;
                }
            }
        }

        if (do_Touch_game && !gCanvas.GetComponentInChildren<ProgressBar>().now_Gaming) // ��ġ ���� ������ �� ó��
        {
            if(gCanvas.GetComponentInChildren<ProgressBar>().result == 1)
            {
                // ���� ó��
            } 
            else
            {
                // ���� ó��
            }
            
            gCanvas.transform.Find("Progress Bar").gameObject.SetActive(false); // ���� ��.
            Destroy(spawnedObject); // ���� �� ȸ��
            throw_Bob = false;
            do_Touch_game = false;
        }

        // gCanvas.GetComponentInChildren<ProgressBar>().IncrementProgress(2); (ȣ�� ����)
        // �ٴٸ� ���� �����ϴ� ����?
    }
}