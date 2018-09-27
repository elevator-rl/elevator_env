using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
public class ElevatorAcademy : Academy
{

    // Use this for initialization

    Building building;

    public static int elevatorCount;
    public static int floors;
    public static int passinger;
    public static float height;
    public static float speed;
    public static float decelerate;
    public static float acelerate;
    public static float open;
    public static float close;
    public static float turn;
    public static int capacity;



    void Start ()
    {
       
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}


    public override void InitializeAcademy()
    {
        building = FindObjectOfType<Building>();


        floors = (int)resetParameters["floor"];
        elevatorCount = (int)resetParameters["elevators"];
        passinger = (int)resetParameters["passinger"];
        height= resetParameters["height"]; 
        speed = resetParameters["speed"]; 
        decelerate = resetParameters["decelerate"];
        acelerate = resetParameters["acelerate"];
        open = resetParameters["open"];
        close = resetParameters["close"];
        turn = resetParameters["turn"];
        capacity = (int)resetParameters["capacity"];




        building.InitEnv();
    }

    public override void AcademyStep()
    {
        building.UpdateEnv();
    }
}
