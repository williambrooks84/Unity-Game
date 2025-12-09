using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorMech : MonoBehaviour 
{

	public Vector3 OpenRotation, CloseRotation;

	public float rotSpeed = 1f;

	public bool doorBool;

	[Header("Proximity")]
	[Tooltip("If true the door will auto-open when the player is within `proximityDistance` units (Unity units, not pixels).")]
	public bool proximityOpen = false;
	[Tooltip("Distance in Unity units at which the door will open automatically")]
	public float proximityDistance = 10f;
	[Tooltip("Tag used to find the player for proximity checks")]
	public string playerTag = "Player";

	Transform _playerTransform;


	void Start()
	{
		doorBool = false;
		if (proximityOpen)
		{
			var p = GameObject.FindWithTag(playerTag);
			if (p != null) _playerTransform = p.transform;
		}
	}
		
	// Open when player enters trigger, close when they exit.
	void OnTriggerEnter(Collider col)
	{
		if (col.CompareTag("Player"))
		{
			doorBool = true;
		}
	}

	void OnTriggerExit(Collider col)
	{
		if (col.CompareTag("Player"))
		{
			doorBool = false;
		}
	}

	void Update()
	{
		// Proximity override: if enabled, open when player is within proximityDistance
		if (proximityOpen)
		{
			if (_playerTransform == null)
			{
				var p = GameObject.FindWithTag(playerTag);
				if (p != null) _playerTransform = p.transform;
			}

			if (_playerTransform != null)
			{
				float sqr = (transform.position - _playerTransform.position).sqrMagnitude;
				bool prox = sqr <= (proximityDistance * proximityDistance);
				doorBool = prox; // auto-open when near, auto-close when far
			}
		}

		if (doorBool)
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler (OpenRotation), rotSpeed * Time.deltaTime);
		else
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler (CloseRotation), rotSpeed * Time.deltaTime);
	}

}

