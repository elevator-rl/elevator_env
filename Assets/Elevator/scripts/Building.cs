using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;



public class Building : MonoBehaviour
{

    static ElevatorAcademy academy;
    public static Brain elevatorBrain;

    // Use this for initialization

    static GameObject resElevator;
    static GameObject resfloor;
    static float simulation_interval = 3f;
  
   


    List<ElevatorAgent> listElve = new List<ElevatorAgent>();
    List<Buildfloor> listFloor = new List<Buildfloor>();

    ElevatorAgent[,] callReqReserveCar;


    static int episodeTotalPassinger ;

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


        callReqReserveCar = new ElevatorAgent[ElevatorAcademy.floors, 2];

        ElevatorPassenger.InitPooler();


        if (elevatorBrain == null)
            elevatorBrain = academy.gameObject.transform.Find("ElevatorBrain").GetComponent<MLAgents.Brain>();



        restPassinger = ElevatorAcademy.passinger;


        int dist = 4;
        int rest = ElevatorAcademy.elevatorCount% 2;
        int mok = ElevatorAcademy.elevatorCount / 2;

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


        for (int i = listElve.Count; i< ElevatorAcademy.elevatorCount; ++i)
        {
            GameObject ele = (GameObject)Instantiate(resElevator, this.transform);
            ele.transform.position = startPos + (Vector3.right * dist * i);

            var agent = ele.GetComponent<ElevatorAgent>();
            listElve.Add(agent);
            agent.GiveBrain(elevatorBrain);
            agent.InitFloor(i, ElevatorAcademy.floors);
            agent.AgentReset();

        }

        for (int i = 0; i < ElevatorAcademy.floors; ++i)
        {
            GameObject fl = (GameObject)Instantiate(resfloor, this.transform);
            fl.transform.position = transform.position + (Vector3.up * ElevatorAcademy.height * i);
            fl.GetComponent<Buildfloor>().SetFloor(i,this);
            listFloor.Add(fl.GetComponent<Buildfloor>());

        }


    }

    public void UpdateEnv()
    {

        SimulationFloorPassinger();
        SimulationEnterElevator();

        UpdatePos();
    }

    public void UpdatePos()
    {
        foreach (var e in listElve)
        {
            e.UpdateAction();
        }
    }


    public void SimulationFloorPassinger()
    {
        if (simulattion_time > Time.fixedTime)
            return;

        if (currentPassinger > episodeTotalPassinger * 0.3)
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


    public void SimulationEnterElevator()
    {

        for (int i = 0; i < listFloor.Count; ++i)
        {
            foreach(var el in listElve)
            {
                listFloor[i].EnterElevator(el);
            }
        }

    }


    public void CallRequest(int floor, MOVE_DIR dir)
    {
      
        switch(elevatorBrain.brainType)
        {
            case BrainType.Player:
            case BrainType.Heuristic:
                SearchNearstElevator(floor, dir);
                break;

            case BrainType.External:
            case BrainType.Internal:
                break;


            default:
                break;
        }
        
    }

    public int SearchNearstElevator(int floor,MOVE_DIR dir)
    {

        float min = 1000000f;
        float dist = 0;
        int buttonDir = 0;

        if(dir != MOVE_DIR.Down)
        {
            buttonDir = 1;
        }
       

        foreach(var e in listElve)
        {
            dist = e.GetFloorDist(floor, dir);

            if (dist < min)
            {
                callReqReserveCar[floor, buttonDir] = e;
                min = dist;
            }
        }

        if (callReqReserveCar[floor, buttonDir] != null)
        {
            var el = callReqReserveCar[floor, buttonDir];
            el.SetCallRequest(floor, dir);
            return el.GetNo();
        }

        return -1;
    }


    private void OnGUI()
    {
        
    }



}
