using UnityEngine.Audio;
using UnityEngine;

//I have a dream...
//That every game will have this feature on its main menu...
	//^No, because its hard to implement PROPERLY aka it feedbacks to the player when you scrollwheel with an audio icon, and its also unusable midgame (e.g. on mouse-using games)
	//Bloated, for so minor a feature, so it will never become universal like the settings menu.

public class MouseScrollAudioDeltaManager : MonoBehaviour
{
	//So every1 can access it, without searching for tags and bs like that.
	public static MouseScrollAudioDeltaManager globalInstance { set; get; }

	public AudioMixer mainAudioMixer;

	public float scrollPower = 0.04f;

	//x value is useless, but Unity holds mouseScrollDelta in a Vector2 so whatever
	private Vector2 mouseScrollCurrentPosition;

	void Awake()
	{
		globalInstance = this;

		//Added to erase bugs and user confusion as to why the game is muted on bootup (gray screen lol)
		globalInstance = null;
		Destroy(this);
	}

	public void Update()
	{
		///Debug.Log("Delta is: " + Input.mouseScrollDelta.y);

		//Checks first if there is any change, so it doesn't fuck performance each frame.
		if (Input.mouseScrollDelta.y != 0)
		{
			mouseScrollCurrentPosition.y = mouseScrollCurrentPosition.y + Input.mouseScrollDelta.y * scrollPower;

			if (mouseScrollCurrentPosition.y <= 0.0001f)//min is 0.0001
				mouseScrollCurrentPosition.y = 0.0001f;
			else if (mouseScrollCurrentPosition.y >= 1)//max is 1
				mouseScrollCurrentPosition.y = 1;

			//tl;dr: As long the above is [0.0001,1], and the below multiplies with 20, it goes from -80 dB to 0 dB
			mainAudioMixer.SetFloat("MasterVolume", Mathf.Log10(mouseScrollCurrentPosition.y) * 20);
		}

	}

}

//If you want to read on volume sliders ((because most developers do them wrong)), here is a good read!
//	https://forum.unity.com/threads/changing-audio-mixer-group-volume-with-ui-slider.297884/
//	https://answers.unity.com/questions/1174589/changing-game-volume.html?childToView=1233447#answer-1233447
//	https://stackoverflow.com/questions/46529147/how-to-set-a-mixers-volume-to-a-sliders-volume-in-unity
//	https://gamedevbeginner.com/the-right-way-to-make-a-volume-slider-in-unity-using-logarithmic-conversion/
//	https://www.youtube.com/watch?v=xNHSGMKtlv4