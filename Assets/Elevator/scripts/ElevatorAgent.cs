using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using TMPro;

public class ElevatorAgent : Agent
{
    public static float moveSpeed = 1.5f;

    public static float doorDelay = 2f;

    public static float takeOnOffDelay = 2f;

    public static float height = 3f;

    static GameObject resFloor;

    public bool[] floorBtnflag;
    int  moveDirState;           //이동 상태방향 여부(-1,0,1) 아래,멈춤,위
    bool bOpendDoor;            //문열린상태
    int  nCurrentFloor;          //현재엘베가 있는 층수

    public GameObject[] listFloor;
    public GameObject up, down;
    public GameObject car;

    public TextMeshPro textNo;
    public TextMeshPro textPassinger;
    public TextMeshPro textDoor;


    float preUpdateTime = 0;
    float coolTime = 0;

    public enum MOVE_DIR
    {
        Down = -1,
        Stop = 0,
        Up = 1,

        end,
    }


    public override void InitializeAgent()
    {

        Init();
    }

    public void Init()
    {

    }

    public void InitFloor(int no, int floor)
    {
        if (resFloor == null)
            resFloor = (GameObject)Resources.Load("Elevator/floor");

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

                of.transform.position = transform.position + (Vector3.up * f * height);
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
                of.transform.position = transform.position + (Vector3.up * f * height);
                listFloor[f] = of;

            }
        }
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
            Debug.LogError(string.Format("No:{0} SetLeaveFloor Floor{1} Error", textNo.text, floor));
            return;
        }

        listFloor[floor].GetComponent<Floor>().SetDestResquest(bOn);

    }

    public void SetCallFloor(int floor, bool bOn = true)
    {
        if (listFloor == null || floor >= listFloor.Length)
        {
            Debug.LogError(string.Format("No:{0} SetCallFloor Floor{0} Error", textNo.text, floor));
            return;
        }

        listFloor[floor].GetComponent<Floor>().SetDestResquest(bOn);
    }

    public void SetPosFloor(int floor)
    {
        car.transform.position = transform.position + (Vector3.up * floor * height);
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

        Vector3 movePos = car.transform.position + Vector3.up * moveDirState * moveSpeed * delta;
        car.transform.position = movePos;

        if (car.transform.position.y > listFloor[listFloor.Length - 1].transform.position.y)
        {
            car.transform.position = listFloor[listFloor.Length - 1].transform.position;
            SetDirction(MOVE_DIR.Down);
            coolTime = preUpdateTime + 1.0f;

        }
        else if (car.transform.position.y < listFloor[0].transform.position.y)
        {
            car.transform.position = listFloor[0].transform.position;
            SetDirction(MOVE_DIR.Up);
            coolTime = preUpdateTime + 1.0f;
        }

    }

}
