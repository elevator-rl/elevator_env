using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Buildfloor : MonoBehaviour
{

  
    public TextMeshPro textUp;
    public TextMeshPro textDn;
    public TextMeshPro textPassinger;
    public TextMeshPro textFloor;

    Building building;


    public List<ElevatorPassenger> listPassinger = new List<ElevatorPassenger>();

    public List<ElevatorAgent> LandingElevators = new List<ElevatorAgent>();

    int floorNo;
    int passingerCount = 0;
    bool upButton = false;
    bool downButton = false;

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

    public void SetFloor(int floor,Building building_)
    {
        building = building_;
        textFloor.text = floor.ToString();
        floorNo = floor;

    }

    public  void OnUpButton(bool bOn = true)
    {
        textUp.gameObject.SetActive(bOn);
    }

    public void OnDownButton(bool bOn = true)
    {
        textDn.gameObject.SetActive(bOn);
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

    public void UpButton( bool bOn)
    {
        textUp.gameObject.SetActive(bOn);
  
        if (bOn)
        {
            building.CallRequest(floorNo, MOVE_DIR.Up);
        }
        
        upButton = bOn;
    }

    public void DownButton(bool bOn)
    {
        textDn.gameObject.SetActive(bOn);


        if (bOn)
        {
            building.CallRequest(floorNo, MOVE_DIR.Down);
        }

        downButton = bOn;
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

        UpButton(up);
        DownButton(dn);


        checkTime = Time.fixedTime;

    }

    public void EnterElevator(ElevatorAgent el)
    {

        if (floorNo != el.GetFloor() || !el.IsEnterableState())
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


        if(el.GetDir() == (int)MOVE_DIR.Up)
            UpButton(false);
        else
            DownButton(false);


        LandingElevators.Add(el);
        return;

    }

   

}



