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


        

        floorPassinger[0] = Random.Range(0, (int)(newPassinger*0.8f));

        int rest = newPassinger - floorPassinger[0];


        while(rest>1)
        {
            int floor = Random.Range(1, listFloor.Count);
            int passinger = Random.Range(1, rest + 1);
            rest -= passinger;
            floorPassinger[floor] = passinger;
        }

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


    public void CallRequest(int floor, MOVE_STATE dir)
    {
      
        switch(elevatorBrain.brainType)
        {
            case BrainType.Player:
            case BrainType.Heuristic:
                SearchRuleBaseNearstElevator(floor, dir);
                break;

            case BrainType.External:
            case BrainType.Internal:
                break;


            default:
                break;
        }
        
    }

    public void ProcRuleBaseCallRequest()
    {
        foreach(var f in listFloor)
        {
            for(int i = (int)MOVE_STATE.Down;i< (int)MOVE_STATE.end;++i)
            {
                SearchRuleBaseNearstElevator(f.GetFloorNo(), (MOVE_STATE)i);
            }
        }

    }

    public int SearchRuleBaseNearstElevator(int floor,MOVE_STATE dir)
    {

        float min = 1000000f;
        float dist = 0;
        int buttonDir = 0;

        if(dir != MOVE_STATE.Down)
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

    public Buildfloor GetFloor(int floor)
    {

        return listFloor[floor];

    }

    public MOVE_STATE GetAction(int floor,ElevatorAgent el)
    {
        return MOVE_STATE.Stop;
    }

    public bool IsNoCallRequest()
    {
        foreach (var f in listFloor)
        {
            if (!f.IsNoCall())
                return false;
        }

        return true;
    }

    private void OnGUI()
    {
        
    }



}
