using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}



	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKey (KeyCode.LeftArrow)) {
			transform.Rotate (new Vector3 (0, Time.deltaTime * -60f, 0));
		}

		if (Input.GetKey (KeyCode.RightArrow)) {
			transform.Rotate (new Vector3 (0, Time.deltaTime * 60f, 0));
		}
	}
}
