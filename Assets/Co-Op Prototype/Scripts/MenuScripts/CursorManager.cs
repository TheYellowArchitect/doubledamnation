using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour 
{
	public static CursorManager globalInstance;

	public bool showCursor = false;
	public GameObject cursorGameObject;

	[Header("Cursor Arrow Sprites per level")]
	public Sprite level1Sprite;
	public Sprite level2Sprite;
	public Sprite level3Sprite;

	private RectTransform levelEditorCursorTransform;
	private Image cursorSprite;

	// Use this for initialization
	void Start () 
	{
		globalInstance = this;

		levelEditorCursorTransform = cursorGameObject.GetComponent<RectTransform>();
		cursorSprite = cursorGameObject.GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (showCursor == true)
			levelEditorCursorTransform.position = Input.mousePosition;
	}

	//Activate on settings menu, on darkwind distortion, and on level editor!
	public void Activate()
	{
		showCursor = true;
		cursorSprite.enabled = true;
	}

	public void Disable()
	{
		//Debug.Log("About to disable!");

		//If in level editor, shouldnt ever hide mouse.
		if (LevelManager.currentLevel == 7)
			return;

		//Debug.Log("DISABLEDD");
		
		showCursor = false;
		cursorSprite.enabled = false;
	}

	public void OnLevelLoad()
	{
		//Should do the same on VFXManager, oh well.
        if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
			cursorSprite.sprite = level1Sprite;
        else if (LevelManager.currentLevel == 2)
            cursorSprite.sprite = level2Sprite;
        else if (LevelManager.currentLevel > 3)
            cursorSprite.sprite = level3Sprite;
		
		if (LevelManager.currentLevel == 7)
			Activate();
	}
}
