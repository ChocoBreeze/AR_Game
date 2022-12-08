using Lean.Touch;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class BowlManager : MonoBehaviour
{
    [SerializeField] private ARRaycastManager m_RaycastManager;
    public ChangeButton changebutton;
    [SerializeField] private Camera arCamera;

    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();
    public GameObject[] fish_List; // 물고기 목록
    public List<GameObject> spawn_Fishes = new();

    // https://velog.io/@gkswh4860/Unity-%ED%8A%B9%EC%A0%95-%EB%B2%94%EC%9C%84-%EB%82%B4%EC%97%90%EC%84%9C-%EB%9E%9C%EB%8D%A4%ED%95%9C-%EC%9C%84%EC%B9%98%EC%97%90-%EC%98%A4%EB%B8%8C%EC%A0%9D%ED%8A%B8-%EC%8A%A4%ED%8F%B0%ED%95%98%EA%B8%B0
    public GameObject rangeObject;
    SphereCollider rangeCollider;

    private float initialDistance;
    private Vector3 initialScale;
    private bool Touched = false;

    public TMP_Text text;

    private void Awake()
    {
        m_RaycastManager= FindObjectOfType<ARRaycastManager>();
        rangeCollider = rangeObject.GetComponent<SphereCollider>();
        arCamera = FindObjectOfType<Camera>();
        text.text = "Start!!";
    }
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnForDebug", 0f, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        if(!changebutton.touchActive) // false
        {
            return;
        }

        // https://www.youtube.com/watch?v=ISBIu6Jzfk8
        // scale using pinch involves two touches
        // we need to count both the touches, store it somewhere, measure the distance between pinch
        // and scale gameobject depending on the pinch
        // we also need to ignore if the pinch distance is small (cases where two touches are registered accidently)

        if(Input.touchCount == 2)
        {
            var touchZero = Input.GetTouch(0);
            var touchOne = Input.GetTouch(1);

            // if any one of touchZero or touchOne is cancelled or maybe ended then do nothing
            if(touchZero.phase == TouchPhase.Ended || touchZero.phase == TouchPhase.Canceled ||
                touchOne.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Canceled)
            {
                return;
            }
            if(touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began) 
            {
                initialDistance = Vector2.Distance(touchZero.position, touchOne.position);
                initialScale = gameObject.transform.localScale;

            }
            else // if touch is moved
            {
                var currentDistance = Vector2.Distance(touchZero.position, touchOne.position);
                // if accidentally touched or pinch movement is very very small
                if (Mathf.Approximately(initialDistance, 0))
                {
                    return; // do nothing if it can be ignored where initial distance is very close to zero
                }

                var factor = currentDistance / initialDistance;
                gameObject.transform.localScale = initialScale * factor;
            }
        }

        // https://simpleneed.tistory.com/51
        // touch to translate
        if(Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began)
            {
                Ray ray;
                RaycastHit hitobj;
                ray = arCamera.ScreenPointToRay(touch.position);

                //text.text = "Touch(One)";
                //Ray를 통한 오브젝트 인식
                if (Physics.Raycast(ray, out hitobj))
                {
                    //터치한 곳에 오브젝트 이름이 Cube를 포함하면
                    if (hitobj.collider.name.Contains("Bowl"))
                    {
                        //text.text = "Touch(Bowl On)";
                        //그 오브젝트를 SelectObj에 놓는다 //터치하고 있는다
                        Touched = true;
                    }

                    else // 오브젝트 선택 아닐 시
                    {
                        // Rotate 구현도 가능할 듯.
                    }
                }
            }
            //터치가 끝나면 터치 끝.
            if (touch.phase == TouchPhase.Ended)
            {
                //text.text = "Touch(Bowl Off)";
                Touched = false;
            }

            if (m_RaycastManager.Raycast(touch.position, m_Hits))
            {
                //터치 시 해당 오브젝트 위치 초기화
                if (Touched)
                {
                    //text.text = "Move(Bowl)";
                    gameObject.transform.position = m_Hits[0].pose.position;
                }
            }
        }
        // text.text = Touched.ToString();

    }

    Vector3 Return_RandomPosition()
    {
        Vector3 originPosition = rangeObject.transform.position; // 구의 중심
        // Debug.Log("originPos : " + originPosition);
        float radius = rangeCollider.radius * gameObject.transform.localScale.x; // 콜라이더의 반지름 * Scale Factor
        // Debug.Log("radius :" + radius);

        float X = Random.Range(-radius, radius);
        float max_Y = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(X, 2));
        float Y = Random.Range(-max_Y, max_Y);
        float max_Z = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(X, 2) - Mathf.Pow(Y, 2));
        float Z = Random.Range(-max_Z, max_Z);
        // Debug.Log("X : " + X + " Y : " + Y + " Z : " + Z);
        // Debug.Log("X^2 + Y^2 + Z^2 : " + (Mathf.Pow(X, 2) + Mathf.Pow(Y, 2) + Mathf.Pow(Z, 2)));

        Vector3 ret_Vector = originPosition + new Vector3(X, Y, Z);
        // Vector3 randSpherePos = Random.insideUnitSphere; // 반지름이 1인 경우에 반지름이 1인 구 안에서 랜덤 위치 이용 가능
        // Debug.Log("Final Pos : " + ret_Vector);
        return ret_Vector;
    }   
    
    Vector3 Return_RandomRotation()
    {
        float X = Random.Range(-90, 91);
        float Y = Random.Range(-90, 91);
        float Z = Random.Range(-90, 91);
        return new Vector3(X, Y, Z);
    }


    public void Spawn_Fish(int index)
    {
        var temp = Instantiate(fish_List[index], Return_RandomPosition(), Quaternion.Euler(Return_RandomRotation())) as GameObject;
        Vector3 parent_Scale = gameObject.transform.localScale;
        temp.transform.localScale = new Vector3(parent_Scale.x * 0.01f, parent_Scale.y * 0.01f, parent_Scale.z * 0.01f);
        temp.transform.parent = gameObject.transform;
        spawn_Fishes.Add(temp);
    }

    private void SpawnForDebug()
    {
        int index = Random.Range(0, 3);
        Spawn_Fish(index);
    }

}
