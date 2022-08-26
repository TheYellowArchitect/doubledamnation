using System.IO;
using UnityEngine;


public class PrintScreenManager : MonoBehaviour
{

	//So every1 can access it, without searching for tags and bs like that.
	public static PrintScreenManager globalInstance { set; get; }

	private string screenshotsFolderPath;
	private string tempScreenshotFileName;
	private int screenshotsFolderCount;

	//Happens at the very start of the game
	private void Awake()
	{
		globalInstance = this;

		//Setting the screenshotFolderPath, tl;dr: the game's folder -> Screenshots folder
		screenshotsFolderPath = Application.persistentDataPath + "/Screenshots";
	}
	
	// Update is called once per frame
	void Update ()
	{
		//If pressed print screen button
		if (Input.GetKeyDown(KeyCode.SysReq) ||Input.GetKeyDown(KeyCode.Print))
			TakePicture();

	}

	public void TakePicture()
	{
	//If Screenshots folder doesnt exist, create that folder
		if (Directory.Exists(screenshotsFolderPath) == false)
			Directory.CreateDirectory(screenshotsFolderPath);


		//Now we got to get proper naming so we don't override the filename by default.
		//To do this, we just enumerate the screenshots inside the folder and give it +1 in naming.

		//So, first we get the number of screenshots inside the folder
		screenshotsFolderCount = Directory.GetFiles(screenshotsFolderPath, "*.jpg").Length;

		//And now we make the name.
		tempScreenshotFileName = "DoubleDamnation_" + (screenshotsFolderCount + 1) + ".jpg";

		//And now we call the Unity API to capture the screen, and put it in the target filepath.
		ScreenCapture.CaptureScreenshot(screenshotsFolderPath + "/" + tempScreenshotFileName);
	}
}
