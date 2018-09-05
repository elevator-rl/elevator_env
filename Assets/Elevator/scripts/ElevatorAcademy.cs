using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
public class ElevatorAcademy : Academy
{

    // Use this for initialization

    Building building;

 

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
        building.InitEnv();
    }

    public override void AcademyStep()
    {
        building.UpdateEnv();
    }
}
