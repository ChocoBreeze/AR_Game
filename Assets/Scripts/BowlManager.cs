using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlManager : MonoBehaviour
{

    public GameObject[] fish_List;
    public GameObject[] spawn_Fishes;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Spawn_Fish(int index)
    {
        Instantiate(fish_List[index]); // position 설정하기
    }
}
