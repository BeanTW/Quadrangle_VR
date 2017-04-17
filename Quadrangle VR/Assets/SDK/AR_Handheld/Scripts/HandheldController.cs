using UnityEngine;
using System.Collections;

public class HandheldController : MonoBehaviour {

	public LayerMask visibleLayers;

	void Awake () 
	{
		Camera.main.cullingMask &= ~(visibleLayers);
	}
	
}
