using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Fugitive : MonoBehaviour
{

    [Header("Specific to GridWorld")]
    public  GridAcademy academy;
    public float timeBetweenDecisionsAtInference;
    private float timeSinceDecision;

    public List<Vector3> moveAblePos = new List<Vector3>();

    public static Vector3[] actionPos;

    Vector3 prePosition;

    // Use this for initialization
    void Start ()
    {
        if(actionPos ==null)
        {
            actionPos = new Vector3[8];


            actionPos[0].Set(0f, 0, 1f);
            actionPos[1].Set(0f, 0, -1f);
            actionPos[2].Set(-1f, 0, 0f);
            actionPos[3].Set(1f, 0, 0f);

            actionPos[4].Set(1f, 0, -1f);
            actionPos[5].Set(-1f, 0, 1f);
            actionPos[6].Set(1f, 0, 1f);
            actionPos[7].Set(-1f, 0, -1f);
           
        }

    }

    private void OnEnable()
    {
        academy = FindObjectOfType(typeof(GridAcademy)) as GridAcademy;
    }

    // Update is called once per frame
    public void  RunawayAction()
    {
        //return;

        moveAblePos.Clear();

        for (int i=0;i< actionPos.Length;++i)
        {
            Vector3 targetPos = transform.position;

            targetPos = transform.position + actionPos[i];

            Collider[] blockTest = Physics.OverlapBox(targetPos, new Vector3(0.3f, 0.3f, 0.3f));

            bool bCol = false;

            foreach( var col in blockTest)
            {
                if (col.tag == "wall"|| col.tag == "agent"|| col.tag == "pit")
                {
                    bCol = true;
                    break;
                }
            }

            if (bCol)
                continue;


            moveAblePos.Add(targetPos);


        }


        if (moveAblePos.Count == 0)
        {
            academy.SetDone();
            return;
        }


        GameObject enermy = null;
        float minXDist = 0;


        foreach (var o in academy.trueAgent)
        {
            if (o == null)
                continue;

            {

                float xDist = Vector3.SqrMagnitude(transform.position - o.transform.position);
                if (enermy==null)
                {
                    enermy = o;
                    minXDist = xDist;
                }
                else if(minXDist> xDist)
                {
                    minXDist = xDist;
                    enermy = o;
                }
            }
        }



        if (Mathf.Sqrt(minXDist) > (academy.gridSize >> 1))
            return;


        int findMovePos = -1;
     

        findMovePos = -1;
        float maXDist = 0;
        for (int i=0;i <moveAblePos.Count;++i)
        {
            float xDist = Vector3.SqrMagnitude(enermy.transform.position - moveAblePos[i]);

            if(maXDist< xDist)
            {
                maXDist = xDist;
                findMovePos = i;
            }

        }

        transform.position = moveAblePos[findMovePos];

    }


    public void FixedUpdate()
    {
        WaitTimeInference();
    }

    private void WaitTimeInference()
    {
        if (!academy.GetIsInference())
        {
            RunawayAction();
        }
        else
        {
            if (timeSinceDecision >= timeBetweenDecisionsAtInference)
            {
                timeSinceDecision = 0f;
                RunawayAction();
            }
            else
            {
                timeSinceDecision += Time.fixedDeltaTime;
            }
        }
    }
}
