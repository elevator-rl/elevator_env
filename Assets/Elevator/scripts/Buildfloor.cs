﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Buildfloor : MonoBehaviour
{


    public TextMeshPro[] textCallButton = new TextMeshPro[(int)MOVE_STATE.end];
    public TextMeshPro textPassinger;
    public TextMeshPro textFloor;

    Building building;


    public List<ElevatorPassenger> listPassinger = new List<ElevatorPassenger>();

    public List<ElevatorAgent> LandingElevators = new List<ElevatorAgent>();

    public ElevatorAgent[] callReservedEl = new ElevatorAgent[(int)MOVE_STATE.end];

    

    int floorNo;
    int passingerCount = 0;
   

    static float checkInterval = 1;

    float checkTime = 0;

	// Use this for initialization
	void Start ()
    {
        passingerCount = 0;

    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        if ((Time.fixedTime-checkTime)>1f)
            ChkUpDownButton();

    }

    public void Init()
    {
        while( listPassinger.Count>0)
        {
            var p = listPassinger[0];
            p.Dispose();
            listPassinger.RemoveAt(0);

        }

        passingerCount = 0;
        textPassinger.text = passingerCount.ToString();

        SetButton(MOVE_STATE.Up, false);
        SetButton(MOVE_STATE.Down, false);

        checkTime = Time.fixedTime;
    }

    public int GetFloorNo()
    {
        return floorNo;
    }

    public int GetPassingerCount()
    {
        return passingerCount;
    }

    
    public bool IsCallRequest(MOVE_STATE dir)
    {
        if (textCallButton[(int)dir] == null
            || !textCallButton[(int)dir].gameObject.activeSelf)
            return false;


        return true;

    }

    public bool IsNoCall()
    {
        if (textCallButton[(int)MOVE_STATE.Down].gameObject.activeSelf
            || textCallButton[(int)MOVE_STATE.Up].gameObject.activeSelf)
            return false;

        return true;
    }

    public void SetFloor(int floor,Building building_)
    {
        building = building_;
        textFloor.text = floor.ToString();
        floorNo = floor;

    }



    public void SetPassinger(int passinger)
    {
        passingerCount = passinger;
        textPassinger.text = passingerCount.ToString();
    }

    public void AddPassinger(int passinger)
    {
        passingerCount += passinger;
        textPassinger.text = passingerCount.ToString();

        for (int i = 0; i < passinger; ++i)
        {
            ElevatorPassenger p = ElevatorPassenger.s_Pooler.Alloc();
            p.startFloor = floorNo;

            while(true)
            {
                p.destFloor = Random.Range(0,ElevatorAcademy.floors);
                if (p.destFloor != p.startFloor)
                    break;      
            }           
            listPassinger.Add(p);
        }

        textPassinger.text = listPassinger.Count.ToString();

        ChkUpDownButton();
    }


    public void SetButton(MOVE_STATE dir,bool bOn)
    {
        textCallButton[(int)dir].gameObject.SetActive(bOn);

        if (bOn)
        {
            building.CallRequest(floorNo, dir);

        }
    }

    public void ChkUpDownButton()
    {
        bool up = false, dn = false;

        foreach(var p in listPassinger)
        {
            if(p.destFloor> floorNo)
            {
                up = true;
            }
            else
            {
                dn = true;
            }
        }

        SetButton(MOVE_STATE.Up, up);
        SetButton(MOVE_STATE.Down, dn);

        checkTime = Time.fixedTime;

    }

    public void EnterElevator(ElevatorAgent el)
    {

        if (floorNo != (int)el.GetFloor() || !el.IsEnterableState())
            return;

        float delay=0;

        int idx = 0;
        while(idx< listPassinger.Count)
        {

            if (!el.IsEnterableState())
                break;

            if (el.AddPassinger(listPassinger[idx]))
            {
                listPassinger.RemoveAt(idx);
                delay += Random.Range(0.6f, 1.0f);
            }
            else
                ++idx;
            
        }


        textPassinger.text = listPassinger.Count.ToString();

       
        LandingElevators.Add(el);
        return;

    }

   

   

}



