using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using TMPro;

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
    int  nextFloor;

    float requestFloor;

    public GameObject[] listFloor;
    public GameObject up, down;
    public GameObject car;

    public TextMeshPro textNo;
    public TextMeshPro textPassinger;
    public TextMeshPro textDoor;


    float preUpdateTime = 0;
    float coolTime = 0;

    float currentMoveSpeed;


    float doorActionStartTime;

    delegate void ElevatorAction();

    ElevatorAction[] elevatorAction = new ElevatorAction[(int)Event.End];

    Fsm<Event, State> fsm = new Fsm<Event, State>();


    public int GetDir()
    {
        return moveDirState;
    }

    public List<ElevatorPassenger> listPassinger = new List<ElevatorPassenger>();

    public enum MOVE_DIR
    {
        Down = -1,
        Stop = 0,
        Up = 1,

        end,
    }


    public override void InitializeAgent()
    {
        InitFsmFunc();


        Init();
    }

    public void InitFsmFunc()
    {
        fsm.AddStateTransition(State.Ready, Event.Call, State.Accelate);
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

    public void InitFloor(int no, int floor)
    {
        if (resFloor == null)
            resFloor = (GameObject)Resources.Load("Elevator/vertical_line");

        textNo.text = no.ToString();

        while (true)
        {
            SetDirction((MOVE_DIR)Random.Range(-1, 2));

            if (moveDirState != 0)
                break;
        }

        SetPosFloor(Random.Range(0, 10));

        if (listFloor != null)
        {
            if (listFloor.Length == floor)
                return;

            for (int f = floor; f < listFloor.Length; ++f)
            {
                Destroy(listFloor[f]);
            }

            GameObject[] temp = new GameObject[floor];

            for (int f = 0; f < floor; ++f)
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
            listFloor = new GameObject[floor];

            for (int f = 0; f < floor; ++f)
            {
                GameObject of = (GameObject)Instantiate(resFloor, transform);
                of.transform.position = transform.position + (Vector3.up * f * ElevatorAcademy.height);
                listFloor[f] = of;

            }
        }

        floorBtnflag = new bool[floor];
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

        if (car.transform.position.y > listFloor[listFloor.Length - 1].transform.position.y)
        {
            car.transform.position = listFloor[listFloor.Length - 1].transform.position;
            SetDirction(MOVE_DIR.Down);
            coolTime = preUpdateTime + ElevatorAcademy.turn; 

        }
        else if (car.transform.position.y < listFloor[0].transform.position.y)
        {
            car.transform.position = listFloor[0].transform.position;
            SetDirction(MOVE_DIR.Up);
            coolTime = preUpdateTime + ElevatorAcademy.turn;
        }

        currentFloor = (transform.localPosition.y / ElevatorAcademy.height);

    }

    public void CheckFloor()
    {
        int floor = (int)currentFloor;

        int nextfloor = Mathf.RoundToInt(currentFloor);

        if(floor != nextfloor)
        {
            if(!floorBtnflag[nextfloor])
            {

            }
            else if(fsm.GetCurrentState() != State.Decelerate)
            {
                fsm.StateTransition(Event.DecelerateStart);
            }
            
        }

    }

    public void UpdateAction()
    {
        elevatorAction[(int)fsm.GetCurrentState()]();
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

    }

    public void Decelerate()
    {
        int nextfloor = Mathf.RoundToInt(currentFloor);

        float dist = listFloor[nextfloor].transform.position.y - car.transform.position.y;

        if(Mathf.Abs(dist)< currentMoveSpeed*Time.fixedDeltaTime)
        {
            car.transform.position = new Vector3(car.transform.position.x, listFloor[nextfloor].transform.position.y, car.transform.position.z);
            fsm.StateTransition(Event.DoorCloseStart);
            return;
        }

        if (currentMoveSpeed < 0.3)
            return;

        currentMoveSpeed -= Time.fixedDeltaTime * ElevatorAcademy.decelerate;

    }

    public void DoorOpening()
    {
        float interval = Time.fixedDeltaTime - fsm.GetTransitionTime();

        if (interval >= ElevatorAcademy.open)
        {
            fsm.StateTransition(Event.DoorOpenEnd);
            textDoor.gameObject.SetActive(false);
            return;
        }

        textDoor.gameObject.SetActive(!textDoor.gameObject.activeSelf);
    }

    public void DoorOpened()
    {
        ///승객 내림처리
        int i = 0;

        Queue<int> destPassinger = new Queue<int>();
        foreach(var p in listPassinger)
        {
            if(p.destFloor == currentFloor)
            {
                destPassinger.Enqueue(i);
            }

            ++i;
        }

        foreach (var p in destPassinger)
        {
            listPassinger.RemoveAt(p);
        }


        StartCoroutine(SetTranstionEvent(Event.DoorCloseStart, destPassinger.Count * 0.6f));  ///승객이 내릴때까지의 시간을 승객수로 곱해준다.

        
    }

    public void DoorClosing()
    {
        if (listPassinger.Count > 0)
            StartCoroutine(SetTranstionEvent(Event.DoorCloseEnd, 1.0f));    ///승객이 있을 경우는 다시 이동을 하도록 셋팅해준다..
        else
        {
            StartCoroutine(SetTranstionEvent(Event.EmptyPassinger, 1.0f)); ///승객이 없을 경우는 일단 해당층에 서 대기한다.
            ///
        }

        textDoor.gameObject.SetActive(true);
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




}
