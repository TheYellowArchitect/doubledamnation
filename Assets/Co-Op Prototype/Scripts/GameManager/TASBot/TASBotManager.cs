using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEditor;

/*
In the future, it needs a way to import/export TASBot Inputs
and ofc reading inputs from player's json!
	^replay system should be a thing by going to that gate
	then Player2/Mage shows a list of all clears (if only one, it shows just that)
	And then replay happens. No menu list or whatever, you must finish the level to see its replay.

Also, make this go onto MasterInputManager instead!
So you don't have to disable him, and you can play alongside your TAS input!!!!
^You currently can't, since both inputs, bug warrior (animation bug and likely physics too)
*/
[System.Serializable]
public class TASBotManager : MonoBehaviour
{
	[Header("New Warrior Input")]
		[MinValue(0f)]
		public float warriorTimeToPress;

		[MinValue(-1f)]
		[MaxValue(1f)]
		public float leftJoystickX;
		[MinValue(-1f)]
		[MaxValue(1f)]
		public float leftJoystickY;

		[MinValue(-1f)]
		[MaxValue(1f)]
		public float rightJoystickX;
		[MinValue(-1f)]
		[MaxValue(1f)]
		public float rightJoystickY;

	[Header("New Mage Input")]
		[MinValue(0f)]
		public float mageTimeToPress;
		public MageInputData createdMageInput;


	[Header("Warrior Input List")]
		public List<WarriorTASInput> warriorTimeSortedInputList = new List<WarriorTASInput>();
	[Header("Mage Input List")]
		public List<MageTASInput> mageTimeSortedInputList = new List<MageTASInput>();


	//Below should be special for each level!
	[Header("Change this if you want to start somewhere in the level")]
		public Vector3 whereToSpawn = Vector3.zero;



	public WarriorInputData defaultWarriorInput;
	public MageInputData defaultMageInput;

	private WarriorMovement warriorBehaviour;
	private MageBehaviour mageBehaviour;
	private WordManager wordManager;

	public bool deleteInputs = false;

	[Header("Read-Only")]
		public bool hasStartedTAS;
		public float timeSinceTASStarted = 0;

		public int warriorInputCurrentIndex;
		public int mageInputCurrentIndex;

		public bool isWarriorInputEmpty;
		public bool isMageInputEmpty;


	// Use this for initialization
	void Start ()
	{
		CreateDefaultInputs();

		ResetNewWarriorInput();
		ResetNewMageInput();

		warriorBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>();
		mageBehaviour = GameObject.FindGameObjectWithTag("Mage").GetComponent<MageBehaviour>();
		wordManager = GameObject.FindObjectOfType<WordManager>();

		//EditorUtility.SetDirty(warriorTimeSortedInputList);
		//EditorUtility.SetDirty(mageTimeSortedInputList);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (hasStartedTAS)
		{
			//======================================
			//===Done at the start of every frame===
			//======================================
			isWarriorInputEmpty = true;
			isMageInputEmpty = true;

			timeSinceTASStarted = timeSinceTASStarted + Time.unscaledDeltaTime;

			//==============================================
			//===Determine if warrior/mage input happened===
			//==============================================

			//If there are any warrior inputs left
			if (warriorInputCurrentIndex + 1 < warriorTimeSortedInputList.Count)
			{
				//If the next input, has less time to be pressed, than current update/framerate
				if (warriorTimeSortedInputList[warriorInputCurrentIndex + 1].timeToActivate <= timeSinceTASStarted)
				{
					warriorInputCurrentIndex++;
					isWarriorInputEmpty = false;

					//Cache the left joystick, so, leaving it empty, it applies for all future frames
					//Otherwise, to just move to the right/left, you would have to put (1,0) MANUALLY. FOR. EACH. FRAME.
					defaultWarriorInput.movementInputDirection = warriorTimeSortedInputList[warriorInputCurrentIndex].warriorInput.movementInputDirection;
				}
			}

			//If there are any mage inputs left
			if (mageInputCurrentIndex + 1 < mageTimeSortedInputList.Count)
			{
				//If the next input, has less time to be pressed, than current update/framerate
				if (mageTimeSortedInputList[mageInputCurrentIndex + 1].timeToActivate <= timeSinceTASStarted)
				{
					mageInputCurrentIndex++;
					isMageInputEmpty = false;

					//Cache the spacebar jump, so, leaving it empty, it applies for all future frames
					//Otherwise, for extended jumps, you would have to put jump MANUALLY. FOR. EACH. FRAME.
					defaultMageInput.aboutToJump = mageTimeSortedInputList[mageInputCurrentIndex].mageInput.aboutToJump;
				}
			}

			//=====================================
			//===Apply inputs, whatever they are===
			//=====================================

			//Determine activated flags, and using that, put proper input
			if (isWarriorInputEmpty && isMageInputEmpty)
				warriorBehaviour.Movement(defaultWarriorInput, defaultMageInput);
			else if (isWarriorInputEmpty == false && isMageInputEmpty == true)
				warriorBehaviour.Movement(warriorTimeSortedInputList[warriorInputCurrentIndex].warriorInput, defaultMageInput);
			else if (isWarriorInputEmpty == true && isMageInputEmpty == false)
				warriorBehaviour.Movement(defaultWarriorInput, mageTimeSortedInputList[mageInputCurrentIndex].mageInput);
			else//if (isWarriorInputEmpty == false && isMageInputEmpty == false)
				warriorBehaviour.Movement(warriorTimeSortedInputList[warriorInputCurrentIndex].warriorInput, mageTimeSortedInputList[mageInputCurrentIndex].mageInput);

			if (isMageInputEmpty == false)
			{
				//Iterate the string into individual characters to push onto TypeLetter(char)
				//This sadly only works visually... Spellword would ideally need a full remake, also allowing for many spells on same letter root (e.g. help/health), but whatever, i have already delayed release for years
				for (int i = 0; i < mageTimeSortedInputList[mageInputCurrentIndex].mageInput.finishedSpellwordString.Length; i++)
					wordManager.TypeLetter(mageTimeSortedInputList[mageInputCurrentIndex].mageInput.finishedSpellwordString[i]);

				//This is used for the final spellword
				if (wordManager.IsStringSpellWord(mageTimeSortedInputList[mageInputCurrentIndex].mageInput.finishedSpellwordString))
				{
					wordManager.ExpireWord();//Without this, it types the above and this, so "fir" to "fire" somehow does fire, while also writing "re" of rewind -_-
					mageBehaviour.DetermineSpell(mageTimeSortedInputList[mageInputCurrentIndex].mageInput.finishedSpellwordString);	
				}
				
			}
				
		}
	}

	[Button("Add Warrior Input")]
	public void AddWarriorInput()
	{
		//Create new warriorTASinput then copy the values
		WarriorTASInput warriorInputToAdd = new WarriorTASInput();
		warriorInputToAdd.warriorInput.movementInputDirection.x = leftJoystickX;
		warriorInputToAdd.warriorInput.movementInputDirection.y = leftJoystickY;
		warriorInputToAdd.warriorInput.combatInputDirection.x = rightJoystickX;
		warriorInputToAdd.warriorInput.combatInputDirection.y = rightJoystickY;

		//Put the time!
		warriorInputToAdd.timeToActivate = warriorTimeToPress;

		//Add the new warrior input, onto the list!
		warriorTimeSortedInputList.Add(warriorInputToAdd);

		ResetNewWarriorInput();

		warriorTimeToPress = 0;
	}

	public void ResetNewWarriorInput()
	{
		leftJoystickX = 0;
		leftJoystickY = 0;
		rightJoystickX = 0;
		rightJoystickY = 0;
	}

	[Button("Add Mage Input")]
	public void AddMageInput()
	{
		//Create new warriorTASinput then copy the values
		MageTASInput mageInputToAdd = new MageTASInput();
		mageInputToAdd.mageInput.aboutToDodgeroll = createdMageInput.aboutToDodgeroll;
		mageInputToAdd.mageInput.dodgerollRight = createdMageInput.dodgerollRight;
		mageInputToAdd.mageInput.aboutToJump = createdMageInput.aboutToJump;
		mageInputToAdd.mageInput.finishedSpellwordString = createdMageInput.finishedSpellwordString;

		//Put the time!
		mageInputToAdd.timeToActivate = mageTimeToPress;

		//Add the new warrior input, onto the list!
		mageTimeSortedInputList.Add(mageInputToAdd);

		ResetNewMageInput();

		mageTimeToPress = 0;
	}

	public void ResetNewMageInput()
	{
		createdMageInput.aboutToDodgeroll = false;
		createdMageInput.dodgerollRight = false;
		createdMageInput.aboutToJump = false;
		createdMageInput.finishedSpellwordString = "";
	}

	public void CreateDefaultInputs()
	{
		defaultWarriorInput = new WarriorInputData();
		defaultWarriorInput.movementInputDirection = Vector2.zero;
		defaultWarriorInput.combatInputDirection = Vector2.zero;

		defaultMageInput = new MageInputData();
		defaultMageInput.aboutToDodgeroll = false;
		defaultMageInput.dodgerollRight = false;
		defaultMageInput.aboutToJump = false;
		defaultMageInput.finishedSpellwordString = "";
	}

	[Button("Sort Inputs")]
	public void SortBothLists()
	{
		//Sort the list by time!
		warriorTimeSortedInputList.Sort((x,y) => x.timeToActivate.CompareTo(y.timeToActivate));
		mageTimeSortedInputList.Sort((x,y) => x.timeToActivate.CompareTo(y.timeToActivate));
	}
	
	[Button("Spawn at Current Location")]
	public void SpawnAtCurrentLocation()
	{
		whereToSpawn = GameObject.FindGameObjectWithTag("Player").transform.position;
	}


	[Button("BEGIN")]
	public void StartTAS()
	{
		StartCoroutine(StartTASCoroutine());
	}

	public IEnumerator StartTASCoroutine()
	{
		//if (whereToSpawn != Vector3.zero)
			MoveToSpawnLocation();

		hasStartedTAS = false;

		timeSinceTASStarted = 0;
		warriorInputCurrentIndex = -1;
		mageInputCurrentIndex = -1;

		warriorBehaviour.ResetInfluences();
		warriorBehaviour.Movement(defaultWarriorInput, defaultMageInput);

		isWarriorInputEmpty = true;
		isMageInputEmpty = true;

		SortBothLists();
		
		//For gravity to do its work, just in case, e.g. spawn is midair, OR, gate is mispositioned
		yield return new WaitForSeconds(1.5f);

		//Do note, that we make .enabled = false, instead of .disableInput.
		//Because, we REPLACE warriorBehaviour.Movement(), and if we disabled input, it would still send to Movement() default inputs! (e.g. for warrior joysticks, vector2.zero)
		GameObject.FindGameObjectWithTag("GameManager").transform.GetChild(2).gameObject.GetComponent<MasterInputManager>().enabled = false;

		

		hasStartedTAS = true;
	}

	[Button("STOP")]
	public void StopTASCoroutine()
	{
		if (hasStartedTAS == false)
			return;

		hasStartedTAS = false;

		GameObject.FindGameObjectWithTag("GameManager").transform.GetChild(2).gameObject.GetComponent<MasterInputManager>().enabled = true;
	}

	//[Button("Delete ALL Inputs")]
	public void ClearLists()
	{
		warriorTimeSortedInputList.Clear();
		mageTimeSortedInputList.Clear();
	}

	public void MoveToSpawnLocation()
	{
		GameObject.FindGameObjectWithTag("Player").transform.position = whereToSpawn;
	}

}