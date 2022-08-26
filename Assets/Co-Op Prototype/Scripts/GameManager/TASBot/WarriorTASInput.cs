

[System.Serializable]
public struct WarriorTASInput
{
	//This is for the time (not frames, TIME) you want the input to be activated!
	//Given the game is not truly frame independent, its not fully accurate
	//but for a local pc, creating its own TAS, it should be accurate
	public float timeToActivate;

	//This contains the 2 joysticks, aka 2 vector2
	public WarriorInputData warriorInput;
}
