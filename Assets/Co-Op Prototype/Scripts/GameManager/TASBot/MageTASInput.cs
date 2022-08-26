

[System.Serializable]
public struct MageTASInput
{
	//This is for the time (not frames, TIME) you want the input to be activated!
	//Given the game is not truly frame independent, its not fully accurate
	//but for a local pc, creating its own TAS, it should be accurate
	public float timeToActivate;

	//This contains the spacebar+inputstring
	public MageInputData mageInput;
}
