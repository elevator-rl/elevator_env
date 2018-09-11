using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Buildfloor : MonoBehaviour
{

    static ElevatorAcademy academy;
    static int maxFloor;


    public TextMeshPro textUp;
    public TextMeshPro textDn;
    public TextMeshPro textPassinger;
    public TextMeshPro textFloor;


    public List<ElevatorPassenger> listPassinger = new List<ElevatorPassenger>();

    int floorNo;
    int passingerCount = 0;
    bool upButton = false;
    bool downButton = false;

	// Use this for initialization
	void Start ()
    {
        passingerCount = 0;

        if (academy == null)
        {
            academy = Object.FindObjectOfType<ElevatorAcademy>();
            maxFloor = (int)academy.resetParameters["floor"];
        }


    }

    // Update is called once per frame
    void Update ()
    {
		
	}

    public void SetFloor(int floor)
    {
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
                p.destFloor = Random.Range(1, maxFloor + 1);
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
        upButton = bOn;
    }

    public void DownButton(bool bOn)
    {
        textDn.gameObject.SetActive(bOn);
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

    }

}



