using NaughtyAttributes;
using UnityEngine;

public class SpellObserve : Spell
{

	[Header("Unique Spell Values")]//This header should be a standard for every spell!
	[MinValue(0)]
	[Tooltip("How far away does it zoom out?")]
	public int observeZoomOut;//40

	[MinValue(0f)]
	[Tooltip("How briefly does it zoom out? Like, in how many seconds does it fully zoom out.")]
	public float observeZoomOutDuration;//0.8

	[MinValue(0f)]
	[Tooltip("How briefly does it zoom back in? Like, in how many seconds does it go from fully zoomed out, to normal")]
	public float observeZoomInDuration;//0.3

	[MinValue(0f)]
	[Tooltip("When the camera is fully zoomed out from observe spellword, in how many seconds should it start going back to normal?")]
	public float observeMaxZoomHaltInterval;//0.4


	private MageBehaviour mageBehaviour;

	// Use this for initialization
	void Start ()
	{
		mageBehaviour = GetComponent<MageBehaviour>();
	}
	
	public override void Cast()
	{
		//I could cache this, but to cache even the camera in this "god-object"... One day, the tech debt will be eventually paid, and this should ease it. The performance hit in searching a tag is so minor, after all. (then again, all camera scripts should be singletons! u fkin retard)
		GameObject.FindGameObjectWithTag("CameraHolder").GetComponent<MultipleTargetCamera>().ZoomBoth(observeZoomOut, observeZoomOutDuration, observeZoomInDuration, true, observeMaxZoomHaltInterval);

		//Player 2 animation!//jojo pose, like X hands, seems perfect for this spell
		mageBehaviour.CastSpellAnimation(MageBehaviour.SpellAnimation.Observe);

		//VFX?

		//SFX
	}
}
