using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Building : MonoBehaviour
{

    static ElevatorAcademy academy;
    static MLAgents.Brain elevatorBrain;

    // Use this for initialization

    static GameObject resElevator;
    static GameObject resfloor;
    static float simulation_interval = 3f;
    static int elevatorCount;
    static int floors;
   


    List<GameObject> listElve = new List<GameObject>();
    List<GameObject> listFloor = new List<GameObject>();


    int episodeTotalPassinger ;

    int currentPassinger;
    int restPassinger;

    public AnimationCurve simuPassinger;

    

    float simulattion_time = 0;

    // Update is called once per frame
    void Update () {
		
	}


    public void InitEnv()
    {

        if (resElevator == null)
            resElevator = (GameObject)Resources.Load("Elevator/elevator_unit");

        if(resfloor == null )
            resfloor = (GameObject)Resources.Load("Elevator/build_floor");

        if (academy ==null)
            academy = FindObjectOfType<ElevatorAcademy>();


        ElevatorPassenger.InitPooler();


        if (elevatorBrain == null)
            elevatorBrain = academy.gameObject.transform.Find("ElevatorBrain").GetComponent<MLAgents.Brain>();


        floors = (int)academy.resetParameters["floor"];
        elevatorCount = (int)academy.resetParameters["elevators"];
        restPassinger = episodeTotalPassinger = (int)academy.resetParameters["passinger"];


        int dist = 4;
        int rest = elevatorCount % 2;
        int mok = elevatorCount / 2;

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


        for (int i = listElve.Count; i< elevatorCount; ++i)
        {
            GameObject ele = (GameObject)Instantiate(resElevator, this.transform);
            ele.transform.position = startPos + (Vector3.right * dist * i);
            listElve.Add(ele);
            ele.GetComponent<ElevatorAgent>().brain = elevatorBrain;
            ele.GetComponent<ElevatorAgent>().InitFloor(i+1, floors);
            ele.GetComponent<ElevatorAgent>().OnEnable();

        }

        for (int i = 0; i < floors; ++i)
        {
            GameObject fl = (GameObject)Instantiate(resfloor, this.transform);
            fl.transform.position = transform.position + (Vector3.up * ElevatorAgent.height * i);
            fl.GetComponent<Buildfloor>().SetFloor(i + 1);
            listFloor.Add(fl);

        }


    }

    public void UpdateEnv()
    {

        SimulationFloorPassinger();

        UpdatePos();
    }

    public void UpdatePos()
    {
        foreach (var e in listElve)
        {
            e.GetComponent<ElevatorAgent>().UpdatePos();
        }
    }


    public void SimulationFloorPassinger()
    {
        if (simulattion_time > Time.fixedTime)
            return;

        if (currentPassinger > episodeTotalPassinger * 0.4)
            return;

        int newPassinger = Random.Range(0, restPassinger+1);

        int[] floorPassinger = new int[listFloor.Count];

        floorPassinger[0] = newPassinger;

        restPassinger -= newPassinger;

        for (int i=0; i<listFloor.Count;++i)
        {
            if(floorPassinger[i]>0)
                listFloor[i].GetComponent<Buildfloor>().AddPassinger(floorPassinger[i]);
        }

        simulattion_time = Time.fixedTime + 5f;
    }

    
}
