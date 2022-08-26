using System.Diagnostics;
using System.IO;
using System.Threading;
using System.ComponentModel;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
//Any class using UnityEditor cannot be built btw.

//Followed the series "Unity Editor Scripting" to get this. Smooth series to get how to make editor windows.
//All I knew was how to make just tools aka 1 button editor functions.

//TODO: Split UI and Git functionality in different classes. UI stuff should simply call the Git functions
public class ToolGit : EditorWindow
{
	Texture2D headerSectionTexture;
	Texture2D versionSectionTexture;
	Texture2D titleSectionTexture;
	Texture2D detailsSectionTexture;

	Color headerSectionColor = new Color(1, 230f/255f, 128f/255f, 1f);
	Color versionSectionColor = new Color(141f/255f, 211f/255f, 95f/255f, 1f);
	Color titleSectionColor = new Color(255f/255f, 128f/255f, 128f/255f, 1f);
	Color detailsSectionColor = new Color(128f/255f, 179f/255f, 1, 1f);

	Rect headerSectionRect;
	Rect versionSectionRect;
	Rect titleSectionRect;
	Rect detailsSectionRect;

	Rect middleLabelRect;
	Rect finalGitButtonRect;
	Rect gitLogButtonRect;

	//public static LabelGUI versionLabel;
	//public static LabelGUI titleLabel;
	//public static LabelGUI detailsLabel;


	//The below 3 are the fields that will be gotten
	public static string versionString;
	public static string titleString;
	public static string detailsString; //=EditorGUILayout.Popup
	public static string titleSplitString;//Used to put ===== below titleString, as many = as characters it has.


	//No Version Number or Version Number Duplicate
	public static bool versionValid = false;

	//No title
	public static bool titleValid = false;

	[MenuItem("Window/Git #&s")]
	static void OpenWindow()
	{
		//This seems kinda complicated but here is how it goes.
		//ToolGit is an extension of EditorWindow so it has GetWindow function.
		//We downcast it here.
		ToolGit newWindow = (ToolGit)GetWindow(typeof(ToolGit));

		//We never want the window to be very small
		newWindow.minSize = new Vector2(300, 400);
	}

	/// <summary>
	/// Similar to Start()/Awake() of Unity
	/// </summary>
	void OnEnable()
	{
		InitTextures();
	}

	/// <summary>
	/// Initialize Texture2D values
	/// </summary>
	void InitTextures()
	{
		headerSectionTexture = new Texture2D(1,1);
		headerSectionTexture.SetPixel(0,1, headerSectionColor);
		headerSectionTexture.Apply();

		versionSectionTexture = new Texture2D(1,1);
		versionSectionTexture.SetPixel(0,1, versionSectionColor);
		versionSectionTexture.Apply();

		titleSectionTexture = new Texture2D(1,1);
		titleSectionTexture.SetPixel(0,1, titleSectionColor);
		titleSectionTexture.Apply();

		detailsSectionTexture = new Texture2D(1,1);
		detailsSectionTexture.SetPixel(0,1, detailsSectionColor);
		detailsSectionTexture.Apply();
	}

	/// <summary>
	/// Similar to Update() of Unity
	/// Not called once per frame, but per mouse interaction
	/// </summary>
	void OnGUI()
	{
		DrawLayouts();
		DrawHeader();
		DrawVersion();
		DrawTitle();
		DrawDetails();

		//GUI.skin.button.stretchHeight = true;
		//GUI.skin.textField.stretchHeight = true;
		GUI.skin.textField.wordWrap = true;
	}

	/// <summary>
	///	Defines Rect Values and paints textures based on those rects
	/// </summary>
	void DrawLayouts()
	{
		headerSectionRect.x = 0;
		headerSectionRect.y = 0;
		//^Both 0 means its top window all times
		headerSectionRect.width = Screen.width;
		headerSectionRect.height = Screen.height/4f;


		versionSectionRect.x = 0;
		versionSectionRect.y = Screen.height / 4f;
		versionSectionRect.width = Screen.width;
		versionSectionRect.height = Screen.height / 4f;


		titleSectionRect.x = 0;
		titleSectionRect.y = Screen.height / 4f + Screen.height / 4f;
		titleSectionRect.width = Screen.width;
		titleSectionRect.height = Screen.height / 4f;


		detailsSectionRect.x = 0;
		detailsSectionRect.y = Screen.height / 4f + Screen.height / 4f + Screen.height / 4f;
		detailsSectionRect.width = Screen.width;
		detailsSectionRect.height = Screen.height / 4f;



		GUI.DrawTexture(headerSectionRect, headerSectionTexture);
		GUI.DrawTexture(versionSectionRect, versionSectionTexture);
		GUI.DrawTexture(titleSectionRect, titleSectionTexture);
		GUI.DrawTexture(detailsSectionRect, detailsSectionTexture);



		//Label and Button Rects below
		middleLabelRect.x = Screen.width /2;
		middleLabelRect.y = 0;
		middleLabelRect.width = Screen.width;
		middleLabelRect.height = 50;

		finalGitButtonRect.x = Screen.width / 2;
		finalGitButtonRect.y = 0;
		finalGitButtonRect.width = Screen.width / 4;
		finalGitButtonRect.height = Screen.height / 8;

		gitLogButtonRect.x = Screen.width - 100;
		gitLogButtonRect.y = 50;
		gitLogButtonRect.width = 80;
		gitLogButtonRect.height = 50;
	}

	/// <summary>
	/// Draw Contents of header
	/// </summary>
	void DrawHeader()
	{
		GUILayout.BeginArea(headerSectionRect);

		if (versionValid && titleValid)
		{
			//If you click the button, then do brackets
			if (GUI.Button (finalGitButtonRect, "GIT GUD"))
			{
				//Save open scenes (only if its ActiveGameScene and it's the only game scene!)
				if (EditorSceneManager.GetActiveScene() == EditorSceneManager.GetSceneAt(0) && EditorSceneManager.loadedSceneCount == 1)
					GitButtonAction();
				else
					UnityEngine.Debug.LogError("You tried to save, while ActiveGameScene is not open and exclusive!");


			}
		}

		if (GUI.Button(gitLogButtonRect, "GIT LOG"))
			GitLog();

		GUILayout.EndArea();
	}

	/// <summary>
	/// Inside are the Git button actions
	/// </summary>
	void GitButtonAction()
	{
		EditorSceneManager.SaveOpenScenes();
		UnityEngine.Debug.Log("Saving ActiveGameScene || Complete!");

		EditorApplication.ExecuteMenuItem("File/Save Project");
		UnityEngine.Debug.Log("Saving Project || Complete!");
		//EditorApplication.ExecuteMenuItem("Edit/Play");//This works, so ofc the above does too ;)

		//====GIT GUD!====
		//You need to open GitBash, get into the proper directory, then put git add, then git commit and the messages ofc.

		//This is how you start any process, wherever it is.
		//Process bashProcess = Process.Start("git-bash.exe",
		//Process bashProcess = new Process();
		//bashProcess.StartInfo.FileName = "C:\\Program Files (x86)\\Git\\git-bash.exe";

		//So you, the user won't be spooked, but you want to see it anyway.
		//bashProcess.StartInfo.CreateNoWindow = true;

		//bashProcess.StartInfo.Arguments = "cd \"C:\\Users\\Balroth\\Desktop\\Unity\\DevBuilds\\Co-Op Prototype\\Co-Op Prototype\\46e\\Co-Op Prototype\"";
		//bashProcess.StartInfo.Arguments = "cd \"C:/Users/Balroth/Desktop/Unity/DevBuilds/Co-Op Prototype/Co-Op Prototype/46e/Co-Op Prototype\"";
		//bashProcess.StartInfo.Arguments = "cd " + Application.dataPath;
		//bashProcess.StartInfo.Arguments = "git status";
		//bashProcess.StartInfo.Arguments = "cd /";

		//UnityEngine.Debug.Log("Current path is" + Application.dataPath);

		//bashProcess.Start();

		//====

		//If you open literally any other .exe in Git folder, you will get fucked.
		//So don't do it. Feel mercy there is bin\\git.exe
		string fullPath = "C:\\Program Files\\Git\\bin\\git.exe";

		//Caching the process so we can wait for it and exit, no timer/threadsleep bs
		Process gitProcess;

		//Give the filepath we open, including the .exe
		ProcessStartInfo startInfo = new ProcessStartInfo(fullPath);
		//We want it to pop up in a normal window, no fullscreen or hidden
		startInfo.WindowStyle = ProcessWindowStyle.Normal;
		//If false... may crasherino. Not sure tho.
		startInfo.UseShellExecute = true;

		if (startInfo.UseShellExecute == false)
		{
			//Display stuff on Debug.Log
			startInfo.RedirectStandardError = true;
			startInfo.RedirectStandardOutput = true;
		}

		//Take the administrator permision to run process
		//startInfo.Verb="runas";

		//startInfo.Arguments = "--login unknown";
		//startInfo.Arguments = "-c \"" + "git log";
		//startInfo.Arguments = @"/c -git log -git log -git add . -git commit -m";
		//startInfo.Arguments = "log";

		//Argument to open it aside of .exe
		//Since it is git.exe, it automatically puts git in front of it
		startInfo.Arguments = "add .";

		//Process.Start is how you start any process, open any file, no matter what it is.
		//Data-based function feelsgoodman, so it takes all the data input in startInfo to function
		//Hence it knows the path, the arguments, etc etc
		gitProcess = Process.Start(startInfo);

		//Don't move next frame since this takes time. Wait for finish/exit and then close since Unity freezes by that time
		gitProcess.WaitForExit();
		gitProcess.Close();

		UnityEngine.Debug.Log("git add || Complete!");

		//titleSplitString should have as many = characters as titleString, so it is put right below titleString
		//so as to have beautiful formating. So it is readable.
			//Empty the titleSplitString, but give it \n so as it starts right below it.
			titleSplitString = "\n";

			//Iterate for each character, and add 1 =
			for (int i = 0; i < titleString.Length; i++)
				titleSplitString = titleSplitString + "=";

		//The arguments line
		//startInfo.Arguments = "commit -m " + "\"" + versionString + "\"" + " -m " + "\"" + titleString + "\"" + " -m " + "\"" + detailsString + "\"";
		startInfo.Arguments = "commit -m " + "\"" + versionString + "\"" + " -m " + "\"" + titleString + titleSplitString + "\"" + " -m " + "\"" + detailsString + "\"";

		gitProcess = Process.Start(startInfo);

		gitProcess.WaitForExit();
		gitProcess.Close();

		UnityEngine.Debug.Log("git commit || Complete!");
		UnityEngine.Debug.Log("Version: " + versionString);
		UnityEngine.Debug.Log("Title: " + titleString);
		UnityEngine.Debug.Log("Details: " + detailsString);

		//And display the result on the screen of the user
		GitLog();

		UnityEngine.Debug.Log("git log || Complete!");

		UnityEngine.Debug.Log("ALL STEPS || Complete!");

		//====
		/*
                        string fullPath = "C:\\Windows\\System32\\cmd.exe";

                        ProcessStartInfo startInfo = new ProcessStartInfo(fullPath);
                        startInfo.WindowStyle = ProcessWindowStyle.Normal;
                        startInfo.Arguments = "cd ..";

                        // take the administrator permision to run process
                        startInfo.Verb="runas";

                        //startInfo.UseShellExecute = true;

                        Process.Start(startInfo);

                        GitLog();
                        */

		/*
		Still no idea as to how to put multiple arguments in a single line...
		However, I would like to put all the links, because if git.exe didn't start on the perfect file location
		I would indeed be very fucked and would be forced to use them...
			https://stackoverflow.com/questions/15061854/how-to-pass-multiple-arguments-in-processstartinfo
			https://www.c-sharpcorner.com/forums/how-to-pass-multiples-arguments-in-processstartinfo
			https://stackoverflow.com/questions/3456228/c-sharp-launch-application-with-multiple-arguments

			Misc:
				https://www.codeproject.com/Questions/5280194/How-to-use-git-bash-command-in-Csharp-application
				https://stackoverflow.com/questions/5343068/is-there-a-way-to-cache-github-credentials-for-pushing-commits/1874522#1874522
				https://stackoverflow.com/questions/34669377/c-sharp-git-command-line-process

			And the ultimate solution, the wild card of desperation...
			https://github.com/github-for-unity/Unity
			No, not use it.
			But see the code, how it invokes git.exe, how it gives those damn arguments.
			Because after all, for 3 .exe arguments I did spend around 12 hours of active development...
		*/

	}

	/// <summary>
	/// Via Process, runs git log and displays the results at Debug.Log
	/// </summary>
	void GitLog()
	{
		string fullPath = "C:\\Program Files\\Git\\bin\\git.exe";

		Process gitProcess;

		ProcessStartInfo startInfo = new ProcessStartInfo(fullPath);
		startInfo.WindowStyle = ProcessWindowStyle.Normal;
		startInfo.UseShellExecute = true;//Making this false doesnt make it redirect to debug.log feelsbadman

		if (startInfo.UseShellExecute == false)
		{
			startInfo.RedirectStandardError = true;
			startInfo.RedirectStandardOutput = true;
		}

		//Take the administrator permision to run process
		//startInfo.Verb="runas";

		//Give it the argument "git log" so it shows git changes
		startInfo.Arguments = "log";

		//Multi-threading so as not to freeze Unity while the git log window is open.
		//https://docs.microsoft.com/en-us/dotnet/api/system.threading.thread.start?view=netcore-3.1
		//Godsent example.
		//There is also this https://www.c-sharpcorner.com/article/passing-data-to-a-worker-thread-in-c-sharp-and-net/
		//but it makes you think you have to make a seperate object/window, and we static af here!
		//
		//Tried Tasks, tasks are not included in Unity's library LMAO
		//Tried backgroundWorked, too complicated5me, I remember I made it work once, but fuck me, so many functions and objects for 2 lines.
		//
		//All I want is to just spawn a thread and let it do whatever it wants.
		//Damn. Got it all working, even with multi-threading... In a single day (and a half for that multithreading)
		//(haven't multi-threaded coding since that horrible skype GUI spinoff 2 years ago...)
		//2 lines of code below, but 9999 lines read to get there.
		Thread gitLogThread = new Thread(ToolGit.BootGitAndGitLog);
		gitLogThread.Start(startInfo);

		//Below ofc works but without the new thread above, it freezes the unity window.
		//gitProcess = Process.Start(startInfo);
	}

	//Git Merge Development to Master
	public static void GitMergeDevelopmentToMaster()
	{
		//Fast-Forward Merge of going from Master to Development, and back
		//	git add .
		//	git commit -m "[Merging to Master]"
		//	git checkout Master
		//	git merge Development
		//then coming back
		//	git checkout Development
	}

	//This must exclude Library folder, soooo good luck changing the .txt then changing it back? xD
	//Find the commands I had used on discord, its some wack retarded shit.
	//And gotta improve it since only you and other contributors should be able to push...
	public static void GitPushToGithub()
	{

	}

	//You have to take it as object otherwise Unity compiler will screech
	//So instead of taking it outright as ProcessStartInfo, take it as object
	//Then cast it again to ProcessStartInfo ;)
	public static void BootGitAndGitLog(object logInfo)
	{
		Process.Start((ProcessStartInfo)logInfo);
	}

	/// <summary>
	/// Draw Contents of Version
	/// </summary>
	void DrawVersion()
	{
		GUILayout.BeginArea(versionSectionRect);

		EditorGUILayout.BeginVertical();

		if (versionString == null)
		{
			//Putting this, when it goes away the selection is gone, so you gotta detect this and via code select the textarea...
			//EditorGUILayout.HelpBox("Please insert the next version number.", MessageType.Info);
			versionValid = false;
		}
			//else if (versionString == duplicate)  EditorGUILayout.HelpBox("This version already exists!", MessageType.Warning);
		else
			versionValid = true;

		GUI.Label(middleLabelRect, "Version Number");

		EditorGUILayout.Space();
		EditorGUILayout.Space();

		versionString = EditorGUILayout.TextField(versionString);


		EditorGUILayout.EndVertical();

		GUILayout.EndArea();
	}

	/// <summary>
	/// Draw Contents of Title
	/// </summary>
	void DrawTitle()
	{
		GUILayout.BeginArea(titleSectionRect);

		EditorGUILayout.BeginVertical();

		if (titleString == null)
		{
			//Putting this, when it goes away the selection is gone, so you gotta detect this and via code select the textarea...
			//EditorGUILayout.HelpBox("Please insert the main changes.", MessageType.Info);
			titleValid = false;
		}
		else
			titleValid = true;

		GUI.Label(middleLabelRect, "Main Changes");

		EditorGUILayout.Space();
		EditorGUILayout.Space();

		titleString = EditorGUILayout.TextField(titleString);


		EditorGUILayout.EndVertical();

		GUILayout.EndArea();
	}

	/// <summary>
	/// Draw Contents of Description
	/// </summary>
	void DrawDetails()
	{
		GUILayout.BeginArea(detailsSectionRect);

		EditorGUILayout.BeginVertical();

		GUI.Label(middleLabelRect, "In Detail...");

		EditorGUILayout.Space();
		EditorGUILayout.Space();

		detailsString = EditorGUILayout.TextArea(detailsString);

		EditorGUILayout.EndVertical();

		GUILayout.EndArea();

		//Doesn't work. What a shame.
		//if (detailsSectionRect.Contains(GUIUtility.ScreenToGUIPoint(Input.mousePosition)))
		//UnityEngine.Debug.Log("Make the text field big only here!");
	}

}

//Improvements that could be done, enough to even release it for other ppl
//	1. Have Github Icon on top, and when clicking it, darken it
//	2. Have all the text areas except last one, be one line (see GUI.skin line!!!)
//	3. Truly center them. This means instead of `Screen.Width / 2`, you should also do `- UIElementSize.width / 2`
//	4. Duplicate Version Number also disallows button. Also for helpbox to appear...