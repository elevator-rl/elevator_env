using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{

    static ElevatorAcademy academy;
    static MLAgents.Brain elevatorBrain;

    // Use this for initialization

    static GameObject resElevator;
   

    List<GameObject> listElve = new List<GameObject>();



 	
	// Update is called once per frame
	void Update () {
		
	}


    public void InitEnv()
    {

        if (resElevator == null)
            resElevator = (GameObject)Resources.Load("Elevator/elevator_unit");


        if (academy ==null)
            academy = FindObjectOfType<ElevatorAcademy>();


        if (elevatorBrain == null)
            elevatorBrain = academy.gameObject.transform.Find("ElevatorBrain").GetComponent<MLAgents.Brain>();


        int floor = (int)academy.resetParameters["floor"];
        int elevators  = (int)academy.resetParameters["elevators"];


        int dist = 4;
        int rest = elevators%2;
        int mok =  elevators/2;

        Vector3 startPos = transform.position;
        if (rest<0.5f)
        {
            mok -= 1;
            startPos = transform.position - (Vector3.right * dist * mok)- (Vector3.right * (dist/2)); 
        }
        else
        {
            startPos = transform.position - (Vector3.right * dist * mok);
        }

        startPos += Vector3.back;


        for (int i = listElve.Count; i< elevators; ++i)
        {
            GameObject ele = (GameObject)Instantiate(resElevator, this.transform);
            ele.transform.position = startPos + (Vector3.right * dist * i);
            listElve.Add(ele);
            ele.GetComponent<ElevatorAgent>().brain = elevatorBrain;
            ele.GetComponent<ElevatorAgent>().InitFloor(i+1,floor);
            ele.GetComponent<ElevatorAgent>().OnEnable();

        }


    }

    public void UpdateEnv()
    {
        foreach( var e in listElve)
        {
            e.GetComponent<ElevatorAgent>().UpdatePos();
        }
    }
}
