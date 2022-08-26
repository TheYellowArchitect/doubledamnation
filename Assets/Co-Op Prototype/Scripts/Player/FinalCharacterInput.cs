using UnityEngine;

public class FinalCharacterInput : MonoBehaviour
{
	[Header("GameObject References")]
	public GameObject FinalOpponent1;//warrior
	public GameObject FinalOpponent2;//mage

	[Header("Warrior Input")]
	public float movementInputDirectionXWarrior;//LeftJoystickX
	public float movementInputDirectionYWarrior;//LeftJoystickY
	public short attackInputDirectionWarrior;//tfw u assign an int here, apologies, memory.

	[Header("Mage Input")]
	public float movementInputDirectionXMage;
	public float movementInputDirectionYMage;
	public short attackInputDirectionMage;//tfw u assign an int here, apologies, memory.



	private FinalCharacterBehaviour controller1;
	private FinalCharacterBehaviour controller2;

	// Use this for initialization
	void Start ()
	{
		controller1 = FinalOpponent1.GetComponent<FinalCharacterBehaviour>();
		controller2 = FinalOpponent2.GetComponent<FinalCharacterBehaviour>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		GetWarriorMovementInput();
		GetWarriorAttackInput();


		GetMageMovementInput();
		GetMageAttackInput();

		//If online/multiplayer, then filter/update them based on network stuff
		if (NetworkFinalCommunicationController.globalInstance != null)
			NetworkFinalInputSnapshotManager.globalInstance.ProcessFinalInput(ref movementInputDirectionXWarrior, ref movementInputDirectionYWarrior, ref attackInputDirectionWarrior, ref movementInputDirectionXMage, ref movementInputDirectionYMage, ref attackInputDirectionMage);

		controller1.ProcessInput(movementInputDirectionXWarrior, movementInputDirectionYWarrior, attackInputDirectionWarrior);
		controller2.ProcessInput(movementInputDirectionXMage, movementInputDirectionYMage, attackInputDirectionMage);

		/*if (NetworkFinalCommunicationController.globalInstance != null)
		{
			NetworkFinalInputSnapshotManager.globalInstance.UpdateHostInputs(ref movementInputDirectionX, ref movementInputDirectionY, ref attackInputDirectionX);
		}*/
	}

	public void GetMageMovementInput()
	{
		//Get Input from joysticks
		movementInputDirectionXMage = Input.GetAxis("KeyboardHorizontal");

		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow))
			movementInputDirectionYMage = 1;
		else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
			movementInputDirectionYMage = -1;
		else
			movementInputDirectionYMage = 0;

		//Dealing with dead zones, without even getting to movement :D
		if (movementInputDirectionXMage < 0.1f && movementInputDirectionXMage > -0.1f)
			movementInputDirectionXMage = 0;
	}

	public void GetWarriorMovementInput()
	{
		//Get Input from joysticks
		movementInputDirectionXWarrior = Input.GetAxis("LeftJoystickHorizontal");
		if (Input.GetAxis("LeftJoystickVertical") > 0.4f)
			movementInputDirectionYWarrior = 1;
		else if (Input.GetAxis("LeftJoystickVertical") < -0.4f)
			movementInputDirectionYWarrior = -1;
		else
			movementInputDirectionYWarrior = 0;

		//Alterantive jump buttons time!
			//L2/R2 Triggers
			if (Input.GetAxis("Triggers") > 0.1f || Input.GetAxis("Triggers") < -0.1f)
				movementInputDirectionYWarrior = 1;

			//"ABXY" Buttons
			if (Input.GetButton("ButtonA"))
				movementInputDirectionYWarrior = 1;

			if (Input.GetButton("ButtonX"))
				movementInputDirectionYWarrior = 1;

			//This seems like a fullhop, but its actually a shorthop since it happens only once.
			if (Input.GetButtonDown("ButtonY"))
				movementInputDirectionYWarrior = 1;

			//Even if from the others you jump high, this forces a fastfall.
			if (Input.GetButton("ButtonB"))
				movementInputDirectionYWarrior = -1;


		//Dealing with dead zones, without even getting to movement :D
		if (movementInputDirectionXWarrior < 0.1f && movementInputDirectionXWarrior > -0.1f)
			movementInputDirectionXWarrior = 0;

	}

	//Combat(Attack)
	//3 possible cases: 0 -> No attack, -1 -> Left attack, 1 -> Right attack.
	public void GetMageAttackInput()
	{
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftControl))
			attackInputDirectionMage = -1;
		else if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.RightControl))
			attackInputDirectionMage = 1;
		else
			attackInputDirectionMage = 0;
	}

	//Combat(Attack)
	//3 possible cases: 0 -> No attack, -1 -> Left attack, 1 -> Right attack.
	public void GetWarriorAttackInput()
	{
		if (Input.GetAxis("RightJoystickHorizontal") < -0.1f)
			attackInputDirectionWarrior = -1;
		else if (Input.GetAxis("RightJoystickHorizontal") > 0.1f)
			attackInputDirectionWarrior = 1;
		else
			attackInputDirectionWarrior = 0;
	}
}
