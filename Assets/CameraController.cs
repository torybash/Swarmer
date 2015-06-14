using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	
	const float maxCamSize = 10f;
	const float minCamSize = 1f;
	const float camZoomSpeed = 10f;

	const float camMoveSpeed = 10f;

	
	// Update is called once per frame
	void Update () 
	{
		//Zoom	
		float scrollIn =  Input.GetAxis("Mouse ScrollWheel");
		float newCamSize = Camera.main.orthographicSize;
		newCamSize += scrollIn * camZoomSpeed * Time.deltaTime;
		newCamSize = Mathf.Clamp(newCamSize, minCamSize, maxCamSize);
		Camera.main.orthographicSize = newCamSize;


		//Movement
		Vector2 moveIn = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		Vector3 camMove = moveIn * Time.deltaTime * camMoveSpeed;
		Camera.main.transform.position += camMove;
	}
}
