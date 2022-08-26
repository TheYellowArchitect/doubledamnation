using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using UnityEngine;
using NaughtyAttributes;

//Once you hit the sphere, resync happens.
//But needs 4 mana.
//tfw I wanted to increase the intensity or color based on mana but that's polish -_-

//Once desynced, subscribes by event and detects the gameobject, and spawns it if not created already.
//Each Update(), it takes the gameobject and simply interpolates it.

//TODO: Remove interpolation. Just moving it works since particles are not local, what an overkill.
public class NetworkDesyncSphereInterpolation : MonoBehaviour
{
	public static NetworkDesyncSphereInterpolation globalInstance;

	[ReadOnly]
	public bool activated = false;

	public WarriorMovement warriorBehaviour;

	[Header("Position Snapshot")]

	[Tooltip("0.02 seconds is 50 milliseconds")]
	public double timePerSnapshot = 0.020d;

	[Tooltip("[0] is current, [1] is the one we interpolate towards, [2] and [3] are the 2 next ones")]
	public List<PositionSnapshot> snapshots = new List<PositionSnapshot>();

	private double timePassedSinceLastPositionReceived = 0d;
	private double timePassedSinceLastPositionSent = 0d;
	private double timePassedSinceLastSentPositionTotal = 0d;
	public Vector2 lerpResultPosition;
	public double lerpRate;

	[Header("DesyncSphere Data")]
	public GameObject desyncSphereGameObject = null;
	public ParticleSystem desyncSphereVFX;

	private BoxCollider2D boxPlayerCollider;       //Body(Top&Mid)
	private CircleCollider2D circlePlayerCollider; //Feet(Bottom)

	// Use this for initialization
	void Start ()
	{
		globalInstance = this;

		boxPlayerCollider = warriorBehaviour.GetComponent<BoxCollider2D>();
		circlePlayerCollider = warriorBehaviour.GetComponent<CircleCollider2D>();
	}

	//This Activate/DeActivate is not like the others. It's not triggered by NetworkCommunicationController, and can't really turn off...
	//How is it invoked? Perhaps I could refactor this to be triggered by some network entity alongside NetworkLevelEditor, instead of LevelManager activating NetworkLevelEditor lol
	public void Activate()
	{
		if (activated == false)
			activated = true;
		else
			return;

		//If not subscribed before to it
		if (NetworkCommunicationController.globalInstance.ReceiveDesyncedPositionEvent == null)
			NetworkCommunicationController.globalInstance.ReceiveDesyncedPositionEvent += UpdateDesyncedPosition;

		//If desyncsphere doesn't exist, create it
		//Making it a parent to each level is a good idea, but not "brazier" ffs, what spagghetti.
		//Once loading a new level btw, it recreates it.
		if (desyncSphereGameObject == null)
		{
			desyncSphereGameObject = VFXManager.globalInstance.SpawnVFX(VFXManager.VFXName.DesyncLocationSphere, warriorBehaviour.transform.position, GameObject.FindGameObjectWithTag("Brazier"));
			desyncSphereVFX = desyncSphereGameObject.GetComponent<ParticleSystem>();
		}
		else
			desyncSphereGameObject.transform.position = warriorBehaviour.transform.position;

		if (desyncSphereVFX.isPlaying == false)
			desyncSphereVFX.Play();

		//Make it not collide with player
		Physics2D.IgnoreCollision(desyncSphereGameObject.GetComponent<CircleCollider2D>(), boxPlayerCollider, true);
		Physics2D.IgnoreCollision(desyncSphereGameObject.GetComponent<CircleCollider2D>(), circlePlayerCollider, true);
	}

	public void DeActivate()
	{
		activated = false;

		//If subscribed before to it
		if (NetworkCommunicationController.globalInstance.ReceiveDesyncedPositionEvent != null)
			NetworkCommunicationController.globalInstance.ReceiveDesyncedPositionEvent -= UpdateDesyncedPosition;

		//GOT ERROR ON MONSTERSYNC PLAYTESTING: UnassignedReferenceException (desyncSphereVFX)
		desyncSphereVFX.Stop();

		ClearSnapshots();
	}

	[Button("Clear Snapshots")]
	public void ClearSnapshots()
	{
		timePassedSinceLastPositionReceived = 0d;
		timePassedSinceLastPositionSent = 0d;
		timePassedSinceLastSentPositionTotal = 0d;
		snapshots.Clear();
	}

	public void UpdateDesyncedPosition(RpcArgs args)
	{
		double receivedSnapshotTime = args.GetNext<double>();
		Vector2 receivedWarriorPosition = args.GetNext<Vector2>();

		snapshots.Add(new PositionSnapshot { timestamp = receivedSnapshotTime, warriorPosition = receivedWarriorPosition});

		//If 2nd snapshot, put the timestamp to start properly!
		if (snapshots.Count == 2)
			timePassedSinceLastPositionReceived = snapshots[0].timestamp;//TODO: Used to be timePassedSinceLastPositionSent. Maybe it will bug?
	}

	public void Update()
	{
		if (activated == false)
			return;

		//Run only if desynced.
		if (NetworkDamageShare.globalInstance.IsSynchronized() == false)
		{
			//Update the timestamps below, and send by timer.
			timePassedSinceLastPositionReceived = timePassedSinceLastPositionReceived + Time.unscaledDeltaTime;
			timePassedSinceLastPositionSent = timePassedSinceLastPositionSent + Time.unscaledDeltaTime;
			timePassedSinceLastSentPositionTotal = timePassedSinceLastSentPositionTotal + Time.unscaledDeltaTime;
			if (timePassedSinceLastPositionSent >= timePerSnapshot || timePassedSinceLastSentPositionTotal == 0d)
			{
				timePassedSinceLastPositionSent = 0;
				NetworkCommunicationController.globalInstance.SendDesyncedPosition(warriorBehaviour.transform.position.ToVector2(), timePassedSinceLastSentPositionTotal);
			}

			InterpolateDesyncedSphere();
		}
	}

	public void InterpolateDesyncedSphere()
	{
		if (snapshots.Count < 2)
		{
			Debug.LogWarning("Not enough snapshots, to do any interpolation.");
			return;
		}

		while (snapshots.Count > 2 && timePassedSinceLastPositionReceived > snapshots[1].timestamp)
			snapshots.RemoveAt(0);

		//Debug.Log("Snapshots: " +snapshots.Count);
		//Debug.Log("Final Snapshot: " + snapshots[snapshots.Count - 1].timestamp + " X " + snapshots[snapshots.Count - 1].warriorPosition.x);

		lerpRate = (timePassedSinceLastPositionReceived - snapshots[0].timestamp) / (snapshots[1].timestamp - snapshots[0].timestamp);
		lerpResultPosition = Vector2.Lerp(snapshots[0].warriorPosition, snapshots[1].warriorPosition, (float)lerpRate);

		//This is for a VERY rare bug (only on high pings) where lerpResultPosition is NaN...
		if (float.IsNaN(lerpResultPosition.x))
			return;

		//Take the interpolated position and put it onto the sphere
		desyncSphereGameObject.transform.position = new Vector3(lerpResultPosition.x, lerpResultPosition.y, 0);
	}

}
