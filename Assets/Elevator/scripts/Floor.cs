using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{

 

    public GameObject callFloorBtn;
    public GameObject destFloorBtn;

	// Use this for initialization
	void Start ()
    {
   
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void SetCallResquest(bool bCall)
    {
        callFloorBtn.SetActive(bCall);
    }

    public void SetDestResquest(bool bCall)
    {
        destFloorBtn.SetActive(bCall);
    }
}
