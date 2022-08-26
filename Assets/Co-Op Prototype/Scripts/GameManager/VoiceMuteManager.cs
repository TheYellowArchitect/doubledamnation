using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is made so as not to expose my shitty voice-acting when some1 irl asks to play.
/// Because holy fuck that intro is horrible, and tbh, even if the rest is good (it is)
/// It will be super awkward, playing with someone who has voice-acted 2 characters and talks with himself, really.
/// It's like watching yugioh next to the marik voiceactor, and there is a scene where marik talks with odion/rashido.
/// It is peak awkwardness, especially since voiceacting here is not just a commercial contract, a gig
/// but a personal expression, in a way, and it will be judged. Judged when on release/finished ftw, not earlier!
/// </summary>
public class VoiceMuteManager : MonoBehaviour
{
	public static VoiceMuteManager globalInstance;

	public bool muteVoiceOnStart = false;

	/// <summary>
	/// This is checked when setting the volume slider
	/// </summary>
	public bool muteVoicePermanent = false;

	// Use this for initialization
	void Start ()
	{
		globalInstance = this;

		if (muteVoicePermanent)
			muteVoiceOnStart = true;

		if (muteVoiceOnStart)
			SettingsMenu.globalInstance.SetVoiceVolume(0.0001f);
	}

}
