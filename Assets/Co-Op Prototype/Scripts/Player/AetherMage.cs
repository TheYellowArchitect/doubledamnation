using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AetherMage : MonoBehaviour
{

	public GameObject aetherInstance;//Not prefab, but a sprite that is fully black.
	public GameObject spriteMaskInstance;
	public float spriteMaskSpeedLimit;
	public List<GameObject> aetherParticles = new List<GameObject>();
	public float aetherParticlesSpeed = 1f;
	public ParticleSystem aetherParticleBurst;

	private Rigidbody2D spriteMaskRigidbody;
	private SpriteRenderer aetherRenderer;
	private SpriteRenderer warriorRenderer;
	private bool runningAetherCoroutine;
	private List<ParticleSystem.EmissionModule> aetherParticlesEmissionModules = new List<ParticleSystem.EmissionModule>();
	private ParticleSystem.EmissionModule tempEmission;

	// Use this for initialization
	void Start ()
	{
		//Cache the aether's sprite
		aetherRenderer = aetherInstance.GetComponent<SpriteRenderer>();

		warriorRenderer = transform.parent.GetComponent<SpriteRenderer>();

		spriteMaskRigidbody = spriteMaskInstance.GetComponent<Rigidbody2D>();

		runningAetherCoroutine = false;

		//Cache the modules
		SetAetherParticlesEmissionModules();

		//Disable the aether (sprite+mask)
		aetherInstance.SetActive(false);

		//Deactivate the particle effect gameobjects
		SetAetherParticles(false);

		//TODO: set the burst particle timerate to be the same with armor invulnerability's (didn't do cuz i plan to ignore the time, and trigger the burst via script from endaether())
	}
	
	public void CastAether()
	{
		aetherInstance.SetActive(true);

		StartCoroutine(CopyWarriorSprite());

		//Resets position of spritemask, and gives it a speed.
		spriteMaskInstance.transform.localPosition = Vector3.zero;
		spriteMaskRigidbody.velocity = new Vector2(UnityEngine.Random.Range(-spriteMaskSpeedLimit, spriteMaskSpeedLimit), UnityEngine.Random.Range(-spriteMaskSpeedLimit, spriteMaskSpeedLimit));

		//Activates the aetherparticles
		SetAetherParticles(true);


		//SFX
		PlayerSoundManager.globalInstance.PlayAudioSource(PlayerSoundManager.AudioSourceName.ArmorSpell);
	}

	public IEnumerator CopyWarriorSprite()
	{
		runningAetherCoroutine = true;

		do
		{
			if (aetherRenderer.sprite != warriorRenderer.sprite)
				aetherRenderer.sprite = warriorRenderer.sprite;

			yield return new WaitForSeconds(0.05f);


		} while (runningAetherCoroutine);


		yield break;
	}

	public void SetAetherParticlesEmissionModules()
	{
		for (int i = 0; i < aetherParticles.Count; i++)
			aetherParticlesEmissionModules.Add(aetherParticles[i].GetComponent<ParticleSystem>().emission);
	}

	public void SetAetherParticles(bool setActive)
	{

		for (int i = 0; i < aetherParticlesEmissionModules.Count; i++)
		{
			tempEmission = aetherParticlesEmissionModules[i];//retarded unity enforces temp variable...

			if (setActive == true)
				tempEmission.rateOverDistance = aetherParticlesSpeed;//0.8f
			else//Stop particle emission
				tempEmission.rateOverDistance = 0f;
		}

	}

	/// <summary>
	/// Called by WarriorHealth when the invulnerability ends, so as the above coroutine ("VFX") ends.
	/// </summary>
	public void EndAether()
	{
		Debug.Log("Running aether coroutine: " + runningAetherCoroutine);

		if (runningAetherCoroutine == false)
			return;

		runningAetherCoroutine = false;

		aetherInstance.SetActive(false);

		//Deactivate the particle effect gameobjects
		SetAetherParticles(false);

		aetherParticleBurst.Play();
	}
}
