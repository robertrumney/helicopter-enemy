using UnityEngine;
using System.Collections;

public class FireHelicopterGun : MonoBehaviour 
{
    	#region Variables
    	public AudioClip[] shots;
	public Transform[] guns;

	[HideInInspector]
	public Transform target;
	public Transform helicopterTransform;

	public GameObject tracer;
	public GameObject fire;
	public GameObject explode;
	public GameObject blades;

	public float health = 10000;
	public float speed = 0.1f;
	public float turnSpeed = 1;
	public float targetHeight = 0;
	public float currentHeight = 15;
	public float offset = 0;
	public float fireRate = 0.01f;
	public float attackDistance;

	private bool dead;
	private bool falling;
	private bool shooting;


	public BoxCollider oldCollider;
	public BoxCollider newCollider;

	private RaycastHit hit;
    	#endregion

    	#region Private Methods
   	private void Start()
	{
		target = Game.instance.playerScript.weaponCameraObj.transform;

		StartCoroutine (Shoot ());
		StartCoroutine (UpdateAI ());
	}

	private void Fire(Vector3 position, Transform origin)
	{
		origin.LookAt(position);

		GameObject GO = (GameObject)Instantiate(tracer, origin.position, origin.rotation);
		Rigidbody instantiatedProjectile = GO.GetComponent<Rigidbody>();

		if (Game.instance.playerScript.hitPoints > 30)
			Game.instance.playerScript.PlayaDamage(1);
		else
			Game.instance.playerScript.PlayaDamage(5);

		instantiatedProjectile.velocity = origin.TransformDirection(new Vector3(0, 0, 500));
	}

	private void Die()
	{
		oldCollider.enabled = false;
		newCollider.enabled = true;

		Game.instance.Achieve("chopper");
		SaveGame.instance.save.xp += 1000;

		RaycastHit hit;
		int layerMask = 1 << 10; // only consider objects on layer 10
		if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, layerMask))
		{
			if (hit.collider.tag == "Ground")
			{
				Vector3 explodePosition = hit.point;
				Instantiate(explode, explodePosition, Quaternion.identity);
				Destroy(this.gameObject);
			}
		}
	}

	private void ApplyDamage(float x)
	{
		health -= x;

		if (health < 1000)
		{
			if (!falling)
			{
				Fall();
				falling = true;
			}
		}
	}

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

	private IEnumerator Shoot()
	{
		yield return new WaitForSeconds (5);

		while (true)
		{
			if (shooting)
			{
				float distance = Vector3.Distance (transform.position, target.position);

				Vector3 pos = Game.instance.player.transform.position + Random.insideUnitSphere * distance / 40;

				int z = Random.Range (0, shots.Length);

				if (distance < (attackDistance / 2))
				{
					if (z == 0)
						Game.instance.playerScript.PlayaDamage (20);
				}

				AudioSource.PlayClipAtPoint (shots [z], pos, 0.1f);

				Fire (pos,guns[0]);
				Fire (pos,guns[1]);
			}

			yield return new WaitForSeconds (fireRate);
		}
	}

	private IEnumerator UpdateAI () 
	{
		yield return new WaitForSeconds (10);

		while (true)
		{
			if (falling)
			{
				shooting = false;
				yield break;
			}

			float distance = Vector3.Distance (transform.position, target.position);

			targetHeight = target.position.y + currentHeight;

			if (Physics.Linecast (transform.position, target.position, out RaycastHit hit2))
			{
				if (hit2.collider.CompareTag ("Calculator"))
				{
					if (distance < attackDistance)
						shooting = true;
					else
						shooting = false;
				} else
					shooting = false;
			} else
				shooting = false;

			Vector3 targetPos = new Vector3 (target.position.x, targetHeight, target.position.z);
			transform.position = Vector3.Lerp (transform.position, targetPos, speed * Time.deltaTime);

			Vector3 relativePos = target.position - transform.position;
			Quaternion LookAtRotation = Quaternion.LookRotation (relativePos);

			float x = LookAtRotation.eulerAngles.x;
			x = Mathf.Clamp (x, 0, 30);

			float z = LookAtRotation.eulerAngles.z;
			x = Mathf.Clamp (x, -30, 30);

			Quaternion LookAtRotationOnly_Y = Quaternion.Euler (x, LookAtRotation.eulerAngles.y + offset, z);

			transform.rotation = Quaternion.Slerp (transform.rotation, LookAtRotationOnly_Y, turnSpeed * Time.deltaTime);

			yield return null;
		}
	}
    #endregion
}
