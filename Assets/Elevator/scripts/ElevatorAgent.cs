using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class ElevatorAgent : Agent
{
    public static float moveSpeed = 1f;

    public static float doorDelay = 2f;

    public static float takeOnOffDelay = 2f;


    public bool[] floorBtnflag;
    int moveDirState;           //이동 상태방향 여부(0,멈춤,1위로 이동,2아래로 이동
    bool bOpendDoor;            //문열린상태
    int nCurrentFloor;          //현재엘베가 있는 층수


    public override void InitializeAgent()
    {

        Init();
    }

    public void Init()
    {

    }


    public override void CollectObservations()
    {
   

    }

    // to be implemented by the developer
    public override void AgentAction(float[] vectorAction, string textAction)
    {


    }
}
