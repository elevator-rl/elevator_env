using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorPassenger
{

    public int startFloor,destFloor;



    public void Init()
    {
        startFloor = destFloor = 0;
    }

    public void SetFloor(int start,int dest)
    {
        startFloor = start;
        destFloor = dest;

    }

}
