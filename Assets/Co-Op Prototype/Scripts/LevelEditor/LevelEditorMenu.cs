using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB; //StandaloneFileBrowser, because Unity cannot open windows explorer file dialog LMAO

public class LevelEditorMenu : MonoBehaviour 
{
	//Singleton
    public static LevelEditorMenu globalInstance;

	[Header("TMP_InputFields")]
	public TMP_InputField titleField;
	public TMP_InputField saveIDField;
	public TMP_InputField loadIDField;
	public TMP_InputField healthField;
	private int latestHealth = 3;

	[Header("Save 2048 Warning")]
	public GameObject saveOverflow1;
	public GameObject saveOverflow2;
	public GameObject saveOverflow3;

	[Header("Border Fill")]
	public Image borderImage;
	public Color defaultBorderFill;
	public Color activeBorderFill;
	public static bool isPlayMode = false;

	[Header("TilePlacementTypeColor")]
	public Color selectedTileColor;//White 255
	public Color unselectedTileColor;//White Fully Transparent
	public Color selectHighlightedTileColor;//White 123 transparency

	[Header("List of PlacementType Buttons")]
	public List<Image> PlacementTypeButtonImages;
	public List<Button> PlacementTypeButtons;
	private int lastSelectedButton = -1;//Ground

	[Header("Title and Health GameObjects")]
	public GameObject healthLabel;
	public GameObject healthInputField;
	public GameObject titleLabel;
	public GameObject titleInputField;

	[Header("The Level Editor UI Panel")]
	public GameObject levelEditorMenuUI;
	public GameObject levelEditorPlayUI;

	[Header("Play Stop Image Buttons")]
	public Image PlayTriangleButton;
	public Image StopBoxButton;
	public Animator circleSpinAnimator;

	private MasterGridManager masterGrid;

	private TextEditor tempTextEditor;
	private Animator swipeLeftAnimator;
	private WarriorHealth warriorHealthScript;
	private WarriorMovement warriorBehaviour;

	private string latestSavedURI;

	//Instead of adding a new bool parameter to each function, this flag turns true when RPC is received, so it won't send it to the other player!
	public bool networkSendBackFlag = true;

	public static Action<bool> toggledPlayModeEvent;

	void Awake()
	{
		swipeLeftAnimator = GameObject.FindGameObjectWithTag("LevelEditorLeftUI").GetComponent<Animator>();
	}

	void Start()
	{
		for (int i = 0; i < PlacementTypeButtons.Count; i++)
		{
			//Fucking unity does it again.
			//If I pass i instead of onto the delegate, it somehow stores i BY REFERENCE
			//And this means all buttons are stored as number 14 (which is the final i value)
			//For fuck sake. Unity doesnt have a way to get the button which calls a function, and if you do try, you must do the below variable hack, cringe.
			int j = i;
			PlacementTypeButtons[i].onClick.AddListener(delegate { CacheButtonColor(j); });
		}

		globalInstance = this;

		warriorHealthScript = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>();

		warriorBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>();

		//Select Ground, visually.
		CacheButtonColor(0);//[0] is Ground
	}

	// Use this for initialization
	void GetMasterGrid() 
	{
		if (GameObject.FindGameObjectWithTag("LevelEditorManager") != null)
			masterGrid = GameObject.FindGameObjectWithTag("LevelEditorManager").GetComponent<MasterGridManager>();
	}
	
	//Saves current level as title.wind
	//Converts .wind to URI and places it on saveInputField
	//Puts URI^ onto clipboard ;)
	public void SaveLevel()
	{
		if (masterGrid == null)
			GetMasterGrid();

		if (masterGrid == null)
		{
			Debug.LogError("Level Editor Menu can't find master grid reeee!");
			return;
		}

		masterGrid.serializationTileManager.LevelSaveWrapper(titleField.text);
		
		latestSavedURI = masterGrid.serializationTileManager.ConvertLatestLevelToBase64String();

		//Copies it to clipboard for convenience :)
		SaveToClipboard();

		//Cannot paste over 2MB on browser, nor on any mainstream chatting platform (e.g. discord or element)
		//Hence, give warning to the user.
		if (latestSavedURI.Length < 2048)
		{
			if (saveOverflow1.active == true)
			{
				saveOverflow1.active = false;
				saveOverflow2.active = false;
				saveOverflow3.active = false;
			}

			Debug.Log("less than 2048");
		}
		else
		{
			saveOverflow1.active = true;
			saveOverflow2.active = true;
			saveOverflow3.active = true;

			Debug.Log("more than 2048");
		}
		
		//Display full URI to box, but only when selected
		saveIDField.text = GetLatestLevelTrimmed();

		//If online, send to the other player
		if (NetworkCommunicationController.globalInstance != null)
		{
			if (networkSendBackFlag == true)
				NetworkCommunicationController.globalInstance.SendLevelEditorSave();
			
			networkSendBackFlag = true;
		}//Kinda sucks that Load is on SerializationTileManager instead of here, but gotta update the UI, so this is the best.
	}

	public void SaveToClipboard()
	{
		if (tempTextEditor == null)
			tempTextEditor = new TextEditor();

		tempTextEditor.content = new GUIContent(latestSavedURI);
		tempTextEditor.SelectAll();
		tempTextEditor.Copy();
	}

	public void SelectedSaveField()
	{
		Debug.Log("LENGTH IS: " + saveIDField.text.Length);
		if (saveIDField.text.Length > 1)
		{
			SaveToClipboard();

			saveIDField.text = latestSavedURI;
	
		}
	}

	public void DeselectedSaveField()
	{
		if (saveIDField.text.Length > 1)
			saveIDField.text = GetLatestLevelTrimmed();
	}

	public string GetLatestLevelTrimmed()
	{
		return latestSavedURI.Substring(0, 36) + "...";
	}

	//=======================================================

	public void LoadLevel()
	{
		if (masterGrid == null)
			GetMasterGrid();

		if (masterGrid == null)
		{
			Debug.LogError("Level Editor Menu can't find master grid reeee!");
			return;
		}

		//Never load on play mode.
		if (isPlayMode)
			TogglePlayMode();

		//If there is no URI, it means the user wants to load a level from his system
		if (loadIDField.text.Length == 0)
		{
			string levelsFolderPath = masterGrid.serializationTileManager.GetLevelsFolderPath();

			Debug.Log("Levels folder path is: " + levelsFolderPath);

			Cursor.visible = true;

			//If levelsFolderPath is deleted, then open at desktop or sth lol
			if (Directory.Exists(levelsFolderPath) == false)
			{
				Debug.LogWarning("Levels folder path is deleted!");
				levelsFolderPath = "";
			}
				

			//var paths = StandaloneFileBrowser.OpenFilePanel("Load a Level", levelsFolderPath, "wind", false);//boolean at the end is multi-select
			//var paths = StandaloneFileBrowser.OpenFilePanel("Load a Level", Application.persistentDataPath, "wind", false);
			var extensions = new [] 
			{
				new ExtensionFilter("Level Files", "wind"),
				new ExtensionFilter("All Files", "*" ),
			};
			var paths = StandaloneFileBrowser.OpenFilePanel("Load a Level", levelsFolderPath.Replace("/", "\\"), extensions, false);
			//paths[0] is the file selected/opened.

			//If user selected a level
			if (paths.Length > 0)
			{
				//Clear/Delete everything
				masterGrid.ClearAllGrid(true);

				//Load the filepath onto deserialization manager
				masterGrid.serializationTileManager.LoadLevel(paths[0]);
			}

			Cursor.visible = false;
		}
		else
		{
			//Clear/Delete everything
			masterGrid.ClearAllGrid(true);

			masterGrid.serializationTileManager.ConvertBase64StringToByteArray(loadIDField.text);
			masterGrid.serializationTileManager.LoadLatestLevel();

			//Delete the URI since it is loaded, and its useless now (intense visual bloat), and removing it on load also gives feedback lol
			//If user needs URI, he can literally press save, and its also on clipboard if he doesnt want to save!
			loadIDField.text = "";
		}

	}

	public void TogglePlayMode()
	{
		Debug.Log("Is playmode: " + isPlayMode);

		isPlayMode = !isPlayMode;			

		if (masterGrid == null)
			GetMasterGrid();

		if (masterGrid == null)
		{
			Debug.LogError("Level Editor Menu can't find master grid reeee!");
			return;
		}

		if (isPlayMode)
		{
			healthLabel.SetActive(false);
			healthInputField.SetActive(false);
			titleLabel.SetActive(false);
			titleInputField.SetActive(false);

			PlayTriangleButton.color = Color.clear;
			StopBoxButton.color = Color.white;

			borderImage.color = activeBorderFill;

			PlaySwipeLeftAnimation();

			if (circleSpinAnimator != null)
				circleSpinAnimator.speed = 0;

			masterGrid.enemyTileManager.ActivateEnemies();

			masterGrid.cameraTileManager.EnableWarriorCamera();

			LevelManager.globalInstance.DeletePillars();

			//romanStringNumerals.NumeralsOneToTwentyFive(healthField.text);
			//warriorHealthScript.SetMaxHealth(romanStringNumerals.NumeralsOneToTwentyFive(healthField.text));
			Debug.Log("Healthfield text is: " + healthField.text);
			SetHealth(healthField.text);
			warriorHealthScript.defaultMaxHealth = romanStringNumerals.NumeralsOneToTwentyFive(healthField.text);
			warriorHealthScript.NormalMaxHealth = romanStringNumerals.NumeralsOneToTwentyFive(healthField.text);

			//Warrior reset values
				warriorHealthScript.SetStartingHealth();

				//Reset position
				warriorBehaviour.ResetAtLevelStart();

			//Find all exit gates, and reset them
			masterGrid.entranceExitTileManager.ResetExits();
			
		}
		else
		{
			healthLabel.SetActive(true);
			healthInputField.SetActive(true);
			titleLabel.SetActive(true);
			titleInputField.SetActive(true);

			StopBoxButton.color = Color.clear;
			PlayTriangleButton.color = Color.white;

			borderImage.color = defaultBorderFill;

			PlaySwipeRightAnimation();

			if (circleSpinAnimator != null)
				circleSpinAnimator.speed = 1;

			masterGrid.enemyTileManager.DisableEnemies();

			masterGrid.cameraTileManager.DisableWarriorCamera();

			LevelManager.globalInstance.DeletePillars();

			masterGrid.platformTileManager.RestartFallingPlatforms();

			//Warrior reset values
				//Resets kills (for playerkiller HP buff)
				PlayerStatsManager.globalInstance.ResetCurrentRunStats();

				GameObject.Find("ManaManager").GetComponent<PlayerManaManager>().ResetMana();

				warriorHealthScript.SetStartingHealth();

				warriorHealthScript.RemoveTempHP();
		}

		if (toggledPlayModeEvent != null)
			toggledPlayModeEvent(isPlayMode);

		//If online, send to the other player
		if (NetworkCommunicationController.globalInstance != null)
		{
			if (NetworkDamageShare.globalInstance.IsSynchronized() == false)
				NetworkDamageShare.globalInstance.SynchronizeFully();

			if (networkSendBackFlag == true)
			{
				if (isPlayMode)
					NetworkCommunicationController.globalInstance.SendLevelEditorPlay();
				else
					NetworkCommunicationController.globalInstance.SendLevelEditorStop();
			}
				
			networkSendBackFlag = true;
		}

	}

	public void SelectPlacementType(int placementEnum)
	{
		if (masterGrid == null)
			GetMasterGrid();

		if (masterGrid == null)
		{
			Debug.LogError("Level Editor Menu can't find master grid reeee!");
			return;
		}

		masterGrid.inputTileManager.DeterminePlacementTypeClick(placementEnum);
	}

	//I can't believe there isn't a straightforward way to get the button which triggers this as a parameter. What bloat.
	public void CacheButtonColor(int justSelectedButtonNumber)
	{
		ColorBlock tempColorBlock;

		if (lastSelectedButton != -1)
		{
			//PlacementTypeButtons[lastSelectedButton].GetComponent<Image>().color = unselectedTileColor;
			tempColorBlock = PlacementTypeButtons[lastSelectedButton].colors;
			tempColorBlock.normalColor = unselectedTileColor;
			tempColorBlock.highlightedColor = selectHighlightedTileColor;
			PlacementTypeButtons[lastSelectedButton].colors = tempColorBlock;
		}
		

		//PlacementTypeButtons[justSelectedButtonNumber].GetComponent<Image>().color = selectedTileColor;
		tempColorBlock = PlacementTypeButtons[justSelectedButtonNumber].colors;
		tempColorBlock.normalColor = selectedTileColor;
		tempColorBlock.highlightedColor = selectedTileColor;
		PlacementTypeButtons[justSelectedButtonNumber].colors = tempColorBlock;

		lastSelectedButton = justSelectedButtonNumber;

		Debug.Log("This played! " + justSelectedButtonNumber);
	}

	public void SetHealth(string finalTextInput)
	{
		//If numerical, convert to latin
		int tempInt;
		if (int.TryParse(finalTextInput, out tempInt))//if number
		{
			//If 0 or less, revert to previous value and return
			if (int.Parse(finalTextInput) < 1)
			{
				healthField.text = romanStringNumerals.romanStringNumeralGenerator(latestHealth);
				return;
			}

			latestHealth = int.Parse(finalTextInput);
			healthField.text = romanStringNumerals.romanStringNumeralGenerator(latestHealth);
			warriorHealthScript.SetMaxHealth(latestHealth);
		}
		else//Check if already latin, otherwise revert
		{
			//If latin
			if (romanStringNumerals.NumeralsOneToTwentyFive(finalTextInput) != -1)
			{
				//If 0 or less, revert to previous value and return
				if (romanStringNumerals.NumeralsOneToTwentyFive(finalTextInput) < 1)
				{
					healthField.text = romanStringNumerals.romanStringNumeralGenerator(latestHealth);
					return;
				}

				latestHealth = romanStringNumerals.NumeralsOneToTwentyFive(finalTextInput);
				warriorHealthScript.SetMaxHealth(latestHealth);
			}
			else//revert to previous value
				healthField.text = romanStringNumerals.romanStringNumeralGenerator(latestHealth);
		}

		//Debug.Log("textInput is: " + finalTextInput);
	}

	//Called by SerializationTileManager.SaveLevel
	public int GetHealth()
	{
		return latestHealth;
	}

	//Called by SerializationTileManager.LoadLevel
	public void SetHealth(int newValue)
	{
		latestHealth = newValue;

		//Update on the UI
		healthField.text = romanStringNumerals.romanStringNumeralGenerator(latestHealth);

		//Update on the actual game mechanics
		warriorHealthScript.SetMaxHealth(latestHealth);
	}

	public void ShowLevelEditorUI(bool show)
	{
		levelEditorMenuUI.SetActive(show);
		levelEditorPlayUI.SetActive(show);
	}

	public void PlaySwipeLeftAnimation()
	{
		if (swipeLeftAnimator == null)
			swipeLeftAnimator = GameObject.FindGameObjectWithTag("LevelEditorLeftUI").GetComponent<Animator>();
		
		swipeLeftAnimator.Play("SwipeLeft");
	}

	//Would go SwipeRight but it must be instantly responsive and have no mouse bugs!
	public void PlaySwipeRightAnimation()
	{
		if (swipeLeftAnimator == null)
			swipeLeftAnimator = GameObject.FindGameObjectWithTag("LevelEditorLeftUI").GetComponent<Animator>();

		swipeLeftAnimator.Play("SwipeRight");
	}
}
