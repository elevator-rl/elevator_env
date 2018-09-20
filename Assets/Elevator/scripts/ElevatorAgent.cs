using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using TMPro;


public enum MOVE_DIR:int
{
    Down = -1,
    Stop = 0,
    Up = 1,

    end,
}

public class ElevatorAgent : Agent
{
    public enum State
    {
        Ready,          //문닫고 멈춰있는 상태
        NormalMove,     //위아래 어느쪽이든 정상적인 이동상태
        Decelerate,     //다음층에 멈추기 위한 감속상태
        DoorOpening,    //문열는중
        DoorOpened,     //문열린 상태에서 승객내리고 타고
        DoorClosing,    //문닫히는 동안.
        Accelate,       //이동에 대한 가속상태
        Turn,
        End,
    };

    public enum Event
    {
        None,
        Call,               //각층에서 호출이 왔을 경우.
        DecelerateStart,     //이동중에 감속지점을 통과 했을때
        Arrived,            //각 층에 도착했을때
        DoorOpenEnd,        //문열기 끝
        DoorCloseStart,     //문닫기 시작
        DoorCloseEnd,       //문닫기 끝
        AccelateEnd,        //가속끝 정상속도 도달
        EmptyPassinger,     //승객이없다. 전부 내렸다.
        End

    }



    static GameObject resFloor;

    public bool[] floorBtnflag;

    int moveDirState;           //이동 상태방향 여부(-1,0,1) 아래,멈춤,위
    bool bOpendDoor;            //문열린상태
    float currentFloor;        //현재엘베가 있는 층수
    int nextFloor;
    int callFloor = -1;

    float requestFloor;

    public GameObject[] listFloor;
    public GameObject up, down;
    public GameObject car;

    public TextMeshPro textNo;
    public TextMeshPro textPassinger;
    public TextMeshPro textDoor;


    int elno;
    float preUpdateTime = 0;
    float coolTime = 0;
   

    float currentMoveSpeed;


   

    delegate void ElevatorAction();

    ElevatorAction[] elevatorAction = new ElevatorAction[(int)Event.End];
    public List<ElevatorPassenger> listPassinger = new List<ElevatorPassenger>();


    Fsm<Event, State> fsm = new Fsm<Event, State>();

    Event nextEvent = Event.None;
    float nextTransitionTime =0;


    public int GetNo()
    {
        return elno;
    }


    public int GetDir()
    {
        return moveDirState;
    }

    public Fsm<Event, State> GetFsm()
    {
        return fsm;
    }



    public override void InitializeAgent()
    {
        InitFsmFunc();
        Init();
    }

    public void InitFsmFunc()
    {
        fsm.AddStateTransition(State.Ready, Event.Call, State.Accelate);
        fsm.AddStateTransition(State.Ready, Event.Arrived, State.DoorOpening);
        fsm.AddStateTransition(State.Accelate, Event.AccelateEnd, State.NormalMove);
        fsm.AddStateTransition(State.NormalMove, Event.DecelerateStart, State.Decelerate);
        fsm.AddStateTransition(State.Decelerate, Event.Arrived, State.DoorOpening);
        fsm.AddStateTransition(State.DoorOpening, Event.DoorOpenEnd, State.DoorOpened);
        fsm.AddStateTransition(State.DoorOpened, Event.DoorCloseStart, State.DoorClosing);
        fsm.AddStateTransition(State.DoorClosing, Event.DoorCloseEnd, State.Accelate);
        fsm.AddStateTransition(State.DoorClosing, Event.EmptyPassinger, State.Ready);

        elevatorAction[(int)State.Ready] = Ready; //문닫고 멈춰있는 상태
        elevatorAction[(int)State.NormalMove] = NormalMove;   //위아래 어느쪽이든 정상적인 이동상태
        elevatorAction[(int)State.Decelerate] = Decelerate;   //다음층에 멈추기 위한 감속상태
        elevatorAction[(int)State.DoorOpening] = DoorOpening;   //문열는중
        elevatorAction[(int)State.DoorOpened] = DoorOpened;  //문열린 상태에서 승객내리고 타고
        elevatorAction[(int)State.DoorClosing] = DoorClosing;  //문닫히는 동안.
        elevatorAction[(int)State.Accelate] = Accelate;  //이동에 대한 가속상태
        elevatorAction[(int)State.Turn] = Turn;

        fsm.SetCurrentState(State.Ready);


    }

    public void Init()
    {
        textPassinger.text = listPassinger.Count.ToString();
    }

    public void InitFloor(int no, int floors)
    {
        if (resFloor == null)
            resFloor = (GameObject)Resources.Load("Elevator/vertical_line");

        textNo.text = no.ToString();
        elno = no;



        SetPosFloor(Random.Range(0, floors));

        if (listFloor != null)
        {
            if (listFloor.Length == floors)
                return;

            for (int f = floors; f < listFloor.Length; ++f)
            {
                Destroy(listFloor[f]);
            }

            GameObject[] temp = new GameObject[floors];

            for (int f = 0; f < floors; ++f)
            {
                GameObject of;

                if (f >= listFloor.Length || !listFloor[f])
                    of = (GameObject)Instantiate(resFloor, transform);
                else
                    of = listFloor[f];

                of.transform.position = transform.position + (Vector3.up * f * ElevatorAcademy.height);
                temp[f] = of;
            }

            listFloor = temp;
        }
        else
        {
            listFloor = new GameObject[floors];

            for (int f = 0; f < floors; ++f)
            {
                GameObject of = (GameObject)Instantiate(resFloor, transform);
                of.transform.position = transform.position + (Vector3.up * f * ElevatorAcademy.height);
                listFloor[f] = of;

            }
        }

        floorBtnflag = new bool[floors];
    }


    public override void CollectObservations()
    {
        for (int i = 0; i < brain.brainParameters.vectorObservationSize; ++i)
            AddVectorObs(0);

    }

    // to be implemented by the developer
    public override void AgentAction(float[] vectorAction, string textAction)
    {


    }


    public void SetDirction(MOVE_DIR dir)
    {
        up.SetActive(false);
        down.SetActive(false);

        moveDirState = (int)dir;

        if (dir == MOVE_DIR.Down)
        {
            down.SetActive(true);

        }
        else if (dir == MOVE_DIR.Up)
        {
            up.SetActive(true);
        }

    }

    public void SetLeaveFloor(int floor, bool bOn = true)
    {
        if (listFloor == null || floor >= listFloor.Length)
        {
            Debug.LogError(string.Format("No:{0} SetLeaveFloor VerticalLine{1} Error", textNo.text, floor));
            return;
        }

        listFloor[floor].GetComponent<VerticalLine>().SetDestResquest(bOn);

    }

    public void SetCallFloor(int floor, bool bOn = true)
    {
        if (listFloor == null || floor >= listFloor.Length)
        {
            Debug.LogError(string.Format("No:{0} SetCallFloor VerticalLine{0} Error", textNo.text, floor));
            return;
        }

        listFloor[floor].GetComponent<VerticalLine>().SetDestResquest(bOn);
    }

    public void SetPosFloor(int floor)
    {
        car.transform.position = transform.position + (Vector3.up * floor * ElevatorAcademy.height);
        currentFloor = floor;
    }



    public void UpdatePos()
    {
        if (preUpdateTime == 0)
        {
            preUpdateTime = Time.fixedTime;
            return;
        }

        float delta = Time.fixedTime - preUpdateTime;

        preUpdateTime = Time.fixedTime;

        if (car == null)
        {
            int fssf = 0;
            return;
        }

        if (coolTime > Time.fixedTime)
            return;

        Vector3 movePos = car.transform.position + Vector3.up * moveDirState * currentMoveSpeed * delta;
        car.transform.position = movePos;

        if (car.transform.position.y >= listFloor[listFloor.Length - 1].transform.position.y)
        {
            car.transform.position = listFloor[listFloor.Length - 1].transform.position;
            SetDirction(MOVE_DIR.Down);
            coolTime = preUpdateTime + ElevatorAcademy.turn;

        }
        else if (car.transform.position.y <= listFloor[0].transform.position.y)
        {
            car.transform.position = listFloor[0].transform.position;
            SetDirction(MOVE_DIR.Up);
            coolTime = preUpdateTime + ElevatorAcademy.turn;
        }

        currentFloor = (car.transform.localPosition.y / ElevatorAcademy.height);

    }

    public void CheckFloor()
    {
        int floor = -1, nextfloor = -1;

        switch ((MOVE_DIR)moveDirState)
        {
            case MOVE_DIR.Up:
                floor = (int)currentFloor;
                nextfloor = Mathf.RoundToInt(currentFloor);
                break;

            case MOVE_DIR.Stop:
                floor = (int)currentFloor;
                nextfloor = floor;
                break;

            case MOVE_DIR.Down:
                floor = Mathf.CeilToInt(currentFloor);
                nextfloor = Mathf.RoundToInt(currentFloor);
                break;
        }


        if (floor != nextfloor)
        {
            if (!floorBtnflag[nextfloor])
            {

            }
            else if (fsm.GetCurrentState() != State.Decelerate)
            {
                fsm.StateTransition(Event.DecelerateStart);
            }

        }

        if (callFloor != floor && callFloor == nextfloor)
        {
            if (fsm.GetCurrentState() != State.Decelerate)
            {
                fsm.StateTransition(Event.DecelerateStart);
            }
        }

    }

    public float GetFloor()
    {
        return currentFloor;
    }

    public void UpdateAction()
    {
        if(CheckStateDelay())
          elevatorAction[(int)fsm.GetCurrentState()]();

        UpdatePos();

        //CheckFloorButton();
    }

    public void SetTransitionDelay(Event evt,float delay = 0.0f,bool bAdd = false)
    {
        if (bAdd)
            nextTransitionTime += delay;
        else
            nextTransitionTime = delay;

        nextEvent = evt;
    }

    public bool CheckStateDelay()
    {

        nextTransitionTime -= Time.fixedDeltaTime;

        if (nextTransitionTime > 0)
            return false;


        if (nextEvent == Event.None || nextTransitionTime==0)
            return true;


        nextTransitionTime = 0;
        fsm.StateTransition(nextEvent);

        nextEvent = Event.None;

        return true;

    }

    public void Ready()
    {
        //아무것도 안하고 대기..
        currentMoveSpeed = 0;

    }

    public void Accelate()
    {
        //정상속도로 되기 위해서 가속상태..

        currentMoveSpeed += Time.fixedDeltaTime * ElevatorAcademy.acelerate;

        if (currentMoveSpeed < ElevatorAcademy.speed)
            return;

        currentMoveSpeed = ElevatorAcademy.speed;

        fsm.StateTransition(Event.AccelateEnd);
    }

    public void NormalMove()
    {
        CheckFloor();
    }

    public void Decelerate()
    {
        int nextfloor = Mathf.RoundToInt(currentFloor);

        float dist = listFloor[nextfloor].transform.position.y - car.transform.position.y;

        if(Mathf.Abs(dist)< currentMoveSpeed*Time.fixedDeltaTime)
        {
            car.transform.position = new Vector3(car.transform.position.x, listFloor[nextfloor].transform.position.y, car.transform.position.z);
            fsm.StateTransition(Event.Arrived);
            SetFloorButton(nextfloor, false);
            currentMoveSpeed = 0;
            return;
        }

        if (currentMoveSpeed < 0.3)
            return;

        currentMoveSpeed -= Time.fixedDeltaTime * ElevatorAcademy.decelerate;

    }

    public void DoorOpening()
    {
 
        SetTransitionDelay(Event.DoorOpenEnd, ElevatorAcademy.open);
        textDoor.gameObject.SetActive(!textDoor.gameObject.activeSelf);

        if (callFloor == GetFloor())
            callFloor = -1;
    }

    public void DoorOpened()
    {
        ///승객 내림처리
     
        float boardingDelay =0;
        int idx = 0;

        int stayfloor = (int)GetFloor();
        switch ((MOVE_DIR)moveDirState)
        {
            case MOVE_DIR.Up:
               
                stayfloor = Mathf.RoundToInt(currentFloor);
                break;

            case MOVE_DIR.Stop:
                stayfloor = (int)currentFloor;
                break;

            case MOVE_DIR.Down:
                stayfloor = Mathf.RoundToInt(currentFloor);
                break;
        }


        while (idx < listPassinger.Count)
        {
            var p = listPassinger[idx];
            if (p.destFloor == stayfloor)
            {
                listPassinger.RemoveAt(idx);
                boardingDelay += Random.Range(0.6f, 1.0f);
                p.Dispose();
            }
            else
            {
                ++idx;
            }
        }

        SetTransitionDelay(Event.DoorCloseStart, boardingDelay);
     
    }

    public void DoorClosing()
    {
        if (listPassinger.Count > 0)
        {
            ///승객이 있을 경우는 다시 이동을 하도록 셋팅해준다..
            SetTransitionDelay(Event.DoorCloseEnd, 1.0f);
        }

        else
        {
            ///승객이 없을 경우는 일단 해당층에 서 대기한다.
            SetTransitionDelay(Event.EmptyPassinger, 1.0f);                                                                            
        }

        textDoor.gameObject.SetActive(true);

        textPassinger.text = listPassinger.Count.ToString();
    }

    public void Turn()
    {

    }

    public IEnumerator SetTranstionEvent(Event e, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        fsm.StateTransition(e);

        yield break;
    }

    public void SetFloorButton(int floor,bool bOn)
    {
        listFloor[floor].GetComponent<VerticalLine>().SetDestResquest(bOn);

        floorBtnflag[floor] = bOn;

        if(moveDirState == (int)MOVE_DIR.Stop&& bOn)
        {
            if(floor > currentFloor)
            {
                SetDirction(MOVE_DIR.Up);
            }
            else if (floor < currentFloor)
            {
                SetDirction(MOVE_DIR.Down);
            }          
        }

    }


    public void CheckFloorButton()
    {
        if(moveDirState == (int)MOVE_DIR.Stop)
        {

        }
    }

    public bool EnterPassinger(ElevatorPassenger p)
    {
        SetFloorButton(p.destFloor, true);

        listPassinger.Add(p);

        return true;
    }

    public float GetFloorDist(int floor,MOVE_DIR dir)
    {

        float dist = listFloor[floor].transform.position.y - car.transform.position.y;

        if (moveDirState == (int)MOVE_DIR.Stop)
            return Mathf.Abs(dist);

        if (moveDirState == (int)MOVE_DIR.Up)
        {
            if(dist > 0)
            {
                return Mathf.Abs(dist);
            }
            else
            {
                dist = Mathf.Abs(listFloor[ElevatorAcademy.floors - 1].transform.position.y - car.transform.position.y);
                dist += Mathf.Abs(listFloor[ElevatorAcademy.floors - 1].transform.position.y - listFloor[floor].transform.position.y);
                return dist;

            }

        }


        if (dist < 0)
        {
            return Mathf.Abs(dist);
        }
        else                                   ///내려올경우..
        {
            dist = Mathf.Abs(listFloor[0].transform.position.y - car.transform.position.y);
            dist += Mathf.Abs(listFloor[0].transform.position.y - listFloor[floor].transform.position.y);

        }

        return dist;
        
    }

    public void SetCallRequest(int floor,MOVE_DIR dir)
    {
        if(fsm.GetCurrentState() == State.Ready)
        {
            if (currentFloor == floor)
            {
                fsm.StateTransition(Event.Arrived);
                callFloor = -1;
                return;
            }

            fsm.StateTransition(Event.Call);
        }
  

        callFloor = floor;


        if(moveDirState != (int)MOVE_DIR.Stop)
        {
            return;
        }

        if (floor > currentFloor)
        {
            SetDirction(MOVE_DIR.Up);
        }
        else if (floor < currentFloor)
        {
            SetDirction(MOVE_DIR.Down);
        }

    }

    public bool IsEnterableState()
    {
        if (fsm.GetCurrentState() != State.DoorOpened
            ||listPassinger.Count>=ElevatorAcademy.capacity)
            return false;

        return true;
    }

    public bool AddPassinger(ElevatorPassenger p)
    {
        if (listPassinger.Count >= ElevatorAcademy.capacity)
            return false;

        if (GetDir() == (int)MOVE_DIR.Up && p.destFloor > GetFloor())
        {
            listPassinger.Add(p);
            SetTransitionDelay(Event.DoorCloseStart, Random.Range(0.6f, 1.0f),true);         
            SetFloorButton(p.destFloor, true);
        }
        else if (GetDir() == (int)MOVE_DIR.Down && p.destFloor < GetFloor())
        {
            listPassinger.Add(p);
            SetTransitionDelay(Event.DoorCloseStart, Random.Range(0.6f, 1.0f), true);
            SetFloorButton(p.destFloor, true);
        }
        else
        {
            return false;
        }

      
        return true;
    }

   
}
