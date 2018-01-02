using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Operate_SceneObj : MonoBehaviour
{
    private bool isFinish = false;

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
	    if (Physics.Raycast(inputRay, out hit))
	    {
	        this.transform.position = hit.point;
	    }

	    if (Input.GetMouseButtonDown(0))
	    {
	        isFinish = true;
	    }
    }
}
