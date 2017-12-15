using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {
    public float MoveMouseSensitivity = 1.0f;
    public float RotateMouseSensitivity = 1.0f;

    public float OffsetHeight = 0.5f;

    Vector3 lastMousePosition = Vector3.zero;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        bool isDirty = false;
		if(Input.GetKey(KeyCode.W))
        {
            this.transform.localPosition += this.transform.forward * Time.deltaTime * MoveMouseSensitivity; isDirty = true;
        }
        if(Input.GetKey(KeyCode.A))
        {
            this.transform.localPosition -= this.transform.right * Time.deltaTime * MoveMouseSensitivity; isDirty = true;
        }
        if(Input.GetKey(KeyCode.S))
        {
            this.transform.localPosition -= this.transform.forward * Time.deltaTime * MoveMouseSensitivity; isDirty = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            this.transform.localPosition += this.transform.right * Time.deltaTime * MoveMouseSensitivity; isDirty = true;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            this.transform.localPosition += Vector3.up * Time.deltaTime * MoveMouseSensitivity; isDirty = true;
        }
        if (Input.mouseScrollDelta.y != 0)
        {
            this.transform.localPosition += this.transform.forward * Input.mouseScrollDelta.y * Time.deltaTime * MoveMouseSensitivity; isDirty = true;
        }
        //if (Input.GetMouseButton(0))
        //{
        //    if (lastMousePosition == Vector3.zero)
        //    {
        //        lastMousePosition = Input.mousePosition;
        //        return;
        //    }

        //    Vector3 delta = Input.mousePosition - lastMousePosition;
        //    delta *= (Time.deltaTime * MoveMouseSensitivity);
        //    delta = new Vector3(delta.x, 0, delta.y);
        //    delta = this.transform.localRotation * delta;
        //    this.transform.localPosition -= delta;

        //    lastMousePosition = Input.mousePosition;
        //}
        if (Input.GetMouseButton(1))
        {
            if(lastMousePosition == Vector3.zero)
            {
                lastMousePosition = Input.mousePosition;
                return;
            }

            Vector3 delta = Input.mousePosition - lastMousePosition;
            delta *= (Time.deltaTime * RotateMouseSensitivity);
            
            this.transform.Rotate(new Vector3(0, delta.x, 0), Space.World);
            this.transform.Rotate(new Vector3(-delta.y, 0, 0), Space.Self);

            lastMousePosition = Input.mousePosition;
        }
        else
        {
            lastMousePosition = Vector3.zero;
        }

        float height;
        if(isDirty && DetectTerrain(out height))
        {
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, height, this.transform.localPosition.z);
        }
    }

    bool DetectTerrain(out float height)
    {
        height = 0;
        Ray ray = new Ray(this.transform.position + Vector3.up * 10, Vector3.down);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.transform.name.Contains("HexCell"))
            {
                if(this.transform.position.y < (hit.transform.position.y + OffsetHeight))
                {
                    height = hit.transform.position.y + OffsetHeight;
                    return true;
                }
            }
        }
        return false;
    }
}
