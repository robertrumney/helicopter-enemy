using UnityEngine;
using System.Collections;

public class HelicopterEnemy : MonoBehaviour 
{
	#region Variables
	// array of audio clips for shots
	public AudioClip[] shots; 

	// array of transforms for guns
	public Transform[] guns; 

	// transform of target object
	public Transform target; 
	// transform of helicopter object
	public Transform helicopterTransform; 

	// prefab for tracer effect
	public GameObject tracer; 
	// prefab for fire effect
	public GameObject fire; 
	// prefab for explode effect
	public GameObject explode; 
	// prefab for blades effect
	public GameObject blades; 

	// health value of helicopter
	public float health = 10000; 
	// speed value for helicopter movement
	public float speed = 0.1f; 
	// turn speed for helicopter movement
	public float turnSpeed = 1; 
	// target height for helicopter
	public float targetHeight = 0; 
	// current height of helicopter
	public float currentHeight = 15; 
	// offset value for helicopter movement
	public float offset = 0; 
	// fire rate for shooting
	public float fireRate = 0.01f; 
	// attack distance for shooting
	public float attackDistance; 

	// flag to check if helicopter is dead
	private bool dead; 
	// flag to check if helicopter is falling
	private bool falling; 
	// flag to check if helicopter is shooting
	private bool shooting; 

	// reference to old collider
	public BoxCollider oldCollider; 
	// reference to new collider
	public BoxCollider newCollider; 

	// variable to store the raycast hit information
	private RaycastHit hit;
	#endregion

    	#region Private Methods
   	private void Start()
	{
	    // Get reference to the target (player's weapon camera)
	    target = Game.instance.playerScript.weaponCameraObj.transform;

	    // Start coroutines for shooting and updating AI
	    StartCoroutine (Shoot ());
	    StartCoroutine (UpdateAI ());
	}
	
	// Fire weapon at target
	private void Fire(Vector3 position, Transform origin)
	{
	    // Point the origin transform at the target position
	    origin.LookAt(position);

	    // Create a tracer game object at the origin position and rotation
	    GameObject GO = (GameObject)Instantiate(tracer, origin.position, origin.rotation);
	    Rigidbody instantiatedProjectile = GO.GetComponent<Rigidbody>();

	    // Check the player's health and apply damage
	    if (Game.instance.playerScript.hitPoints > 30)
		Game.instance.playerScript.PlayaDamage(1);
	    else
		Game.instance.playerScript.PlayaDamage(5);

	    // Set the velocity of the instantiatedProjectile
	    instantiatedProjectile.velocity = origin.TransformDirection(new Vector3(0, 0, 500));
	}

	// Handle helicopter death sequence
	private void Die()
	{
		// Disable the old collider and enable the new collider
		oldCollider.enabled = false;
		newCollider.enabled = true;
		
		// Raycast from the helicopter's position towards the ground
		RaycastHit hit;
		int layerMask = 1 << 10; // Only consider objects on layer 10
		if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, layerMask))
		{
			// If the raycast hit a collider with the tag "Ground", instantiate an explosion prefab at the hit point and destroy the helicopter game object
			if (hit.collider.tag == "Ground")
			{
				Vector3 explodePosition = hit.point;
				Instantiate(explode, explodePosition, Quaternion.identity);
				Destroy(this.gameObject);
			}
		}
	}

	// Apply damage to the helicopter
	private void ApplyDamage(float x)
	{
		// Reduce the helicopter's health by x
		health -= x;

		// If the health is below 1000 and the helicopter isn't already falling, call the "Fall" function
		if (health < 1000)
		{
			if (!falling)
			{
				Fall();
				falling = true;
			}
		}
	}

	// Start the "Falling" coroutine
	private void Fall()
	{
		StartCoroutine(Falling());
	}
	#endregion

	#region Coroutines
	private IEnumerator Falling()
	{
		// Enable the fire particle effect
		fire.SetActive(true);

		// Get the helicopter's Rigidbody component
		Rigidbody rb = GetComponent<Rigidbody>();

		// Disable kinematic behavior and set the mass of the Rigidbody
		rb.isKinematic = false;
		rb.mass = 1;

		// Enable collision and disable triggers for all colliders on the helicopter
		foreach (Collider col in GetComponents<Collider>())
		{
			col.isTrigger = false;
			col.enabled = true;
		}

		// Destroy the helicopter's collider components
		Destroy(GetComponent<SphereCollider>());
		Destroy(blades.GetComponent<BoxCollider>());

		// Set the initial rotation and falling speed
		float rotationSpeed = 90;
		float fallingSpeed = 1;

		// Set the gravity and drag constants
		const float gravity = 9.81f;
		const float drag = 0.5f;

		// Set the wind direction and intensity
		Vector3 windDirection = Vector3.left;
		float windIntensity = 1;

		// Set the noise parameters for the rotation and falling speeds
		float noiseFrequency = 0.1f;
		float noiseAmplitude = 10;
		float noiseOffset = Random.value;

		// Enter the falling loop
		while (true)
		{
			// Calculate the current rotation and falling speeds
			rotationSpeed = Mathf.Lerp(rotationSpeed, noiseAmplitude * Mathf.PerlinNoise(Time.time * noiseFrequency, noiseOffset), Time.deltaTime);
			fallingSpeed = Mathf.Lerp(fallingSpeed, noiseAmplitude * Mathf.PerlinNoise(Time.time * noiseFrequency, noiseOffset + 1), Time.deltaTime);

			// Rotate the helicopter around its own position based on the rotation speed
			helicopterTransform.RotateAround(transform.position, transform.up, Time.deltaTime * rotationSpeed);

			// Apply gravity and drag forces to the helicopter's Rigidbody
			rb.AddForce(-Vector3.up * gravity * rb.mass * Time.deltaTime);
			rb.AddForce(-rb.velocity * drag * Time.deltaTime);

			// Apply wind force to the helicopter based on the wind direction and intensity
			rb.AddForce(windDirection * windIntensity * Time.deltaTime);

			// Move the helicopter downward based on the falling speed
			transform.position -= (transform.up) * fallingSpeed * Time.deltaTime;

			// Check for a collision with the ground
			if (Physics.Raycast(transform.position, -Vector3.up, out hit))
			{
				float distance = Vector3.Distance(hit.point, transform.position);

				// If the helicopter is close to the ground, trigger the explosion and destroy the game object
				if (distance < 0.3f)
				{
					if (!dead)
					{
						Die();
						dead = true;
					}
				}
			}

			// Wait for the next frame
			yield return null;
		}
	}

	// Enemy weapon logic
	private IEnumerator Shoot()
	{
		// Wait for 5 seconds before starting the shooting
		yield return new WaitForSeconds (5);

		// Continuously check for shooting
		while (true)
		{
			if (shooting)
			{
				// Calculate the distance between the helicopter and the target
				float distance = Vector3.Distance (transform.position, target.position);

				// Randomize the position of the shot's sound effect
				Vector3 pos = Game.instance.player.transform.position + Random.insideUnitSphere * distance / 40;

				// Choose a random audio clip from the shots array
				int z = Random.Range (0, shots.Length);

				// If the helicopter is within half of the attack distance, play the damage effect
				if (distance < (attackDistance / 2))
				{
					if (z == 0)
						Game.instance.playerScript.PlayaDamage (20);
				}

				// Play the audio clip at the randomized position
				AudioSource.PlayClipAtPoint (shots [z], pos, 0.1f);

				// Call the Fire method for both guns
				Fire (pos, guns[0]);
				Fire (pos, guns[1]);
			}

			// Wait for the fire rate duration before checking for shooting again
			yield return new WaitForSeconds (fireRate);
		}
	}

	
	// Enemy A.I. Loop
	private IEnumerator UpdateAI () 
	{
	    // wait for 10 seconds before starting
	    yield return new WaitForSeconds (10);

	    // repeat indefinitely
	    while (true)
	    {
		// if helicopter is falling, stop shooting and exit coroutine
		if (falling)
		{
		    shooting = false;
		    yield break;
		}

		// calculate the distance between helicopter and target
		float distance = Vector3.Distance (transform.position, target.position);

		// set target height based on target's y position and current height
		targetHeight = target.position.y + currentHeight;

		// perform linecast between helicopter and target
		if (Physics.Linecast (transform.position, target.position, out RaycastHit hit2))
		{
		    // if linecast hits an object with "Calculator" tag and distance is less than attack distance
		    if (hit2.collider.CompareTag ("Calculator"))
		    {
			if (distance < attackDistance)
			    // set shooting flag to true
			    shooting = true;
			else
			    // set shooting flag to false
			    shooting = false;
		    } 
		    else
			// set shooting flag to false
			shooting = false;
		} 
		else
		    // set shooting flag to false
		    shooting = false;

		// set target position based on target's x, y, and z positions and target height
		Vector3 targetPos = new Vector3 (target.position.x, targetHeight, target.position.z);
		// smoothly move helicopter towards target position
		transform.position = Vector3.Lerp (transform.position, targetPos, speed * Time.deltaTime);

		// calculate relative position between helicopter and target
		Vector3 relativePos = target.position - transform.position;
		// get rotation to look at target
		Quaternion LookAtRotation = Quaternion.LookRotation (relativePos);

		// clamp x rotation between 0 and 30 degrees
		float x = LookAtRotation.eulerAngles.x;
		x = Mathf.Clamp (x, 0, 30);

		// clamp z rotation between -30 and 30 degrees
		float z = LookAtRotation.eulerAngles.z;
		x = Mathf.Clamp (x, -30, 30);

		// get rotation only in y axis with clamped x and z rotations
		Quaternion LookAtRotationOnly_Y = Quaternion.Euler (x, LookAtRotation.eulerAngles.y + offset, z);

		// smoothly rotate helicopter towards target
		transform.rotation = Quaternion.Slerp (transform.rotation, LookAtRotationOnly_Y, turnSpeed * Time.deltaTime);

		// wait for next frame before repeating
		yield return null;
	    }
	}
    	#endregion
}
