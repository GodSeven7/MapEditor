using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Operate_SceneObj : MonoBehaviour
{
    private bool isFinish = false;
    private bool isInNormalArea = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
	    if (isFinish)
	        return;

	    Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
	    RaycastHit hit;
        bool bInArea = Physics.Raycast(inputRay, out hit);
        if (bInArea)
        {
            this.transform.position = hit.point;
        }
        if (Input.GetMouseButtonDown(0))
	    {
	        isFinish = true;
            isInNormalArea = bInArea;
	    }
    }

    public bool IsInNormalArea()
    {
        return isInNormalArea;
    }
    public bool IsFinish()
    {
        return isFinish;
    }
}
