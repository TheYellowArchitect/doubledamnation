using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using UnityEngine;
using NaughtyAttributes;


//Bless this vid https://youtube.com/watch?v=Y8XCoEt7zTU
public class JSONDataManager : MonoBehaviour
{

    public string playerCoreStatsFilename = "PlayerCoreStats";
    public string playerPathStatsFilename = "PlayerPathStats";

    [ReadOnly]
    public string jsonAppend = ".json";

    [ReadOnly]
    public int playerStatsSaveID = 1;
    private bool readAllPlayerStats = false;//used as a flag, to increase playerStatsSaveID.

    [ReadOnly]
    public string playerCoreStatsFilepath = "";
    [ReadOnly]
    public string playerPathStatsFilepath = "";

	// Use this for initialization
	void Start ()
    {
        /* Unauthorized access on program files, so i gave up.
        //Check this out: https://docs.microsoft.com/en-us/dotnet/api/system.io.directory.createdirectory?view=netframework-4.8
        //Interesting stuff to know https://en.wikipedia.org/wiki/User_Account_Control#Requesting_elevation
        #if UNITY_STANDALONE_WIN
                Debug.Log("Stand Alone Windows");

                DirectorySecurity securityRules = new DirectorySecurity();
                securityRules.AddAccessRule(new FileSystemAccessRule(@"Domain\account1", FileSystemRights.FullControl, AccessControlType.Allow));
                //^Pass this to 2nd argument.

                //Create folder of "Double Damnation", if it doesnt exist already!
                DirectoryInfo folderInfo = Directory.CreateDirectory("C:\\Program Files\\Double Damnation");

                //Create the string filenames!
                playerCoreStatsFilepath = "C:/Program Files/Double Damnation" + "/" + playerCoreStatsFilename;
                playerPathStatsFilepath = "C:/Program Files/Double Damnation" + "/" + playerPathStatsFilename;
        #else
                Debug.log("Not standalone windows");
                playerCoreStatsFilepath = Application.persistentDataPath + "/" + playerCoreStatsFilename;
                playerPathStatsFilepath = Application.persistentDataPath + "/" + playerPathStatsFilename;
        #endif
        */

        //This gets all the sentences in the folder
        string[] filePathsArray = Directory.GetFiles(Application.persistentDataPath, "*.json");//This will throw an error if the filepath doesn't exist (categorypath most often)
        List<string> pathfiles = new List<string>(filePathsArray);

        /*
        //Display all files in the folder
        Debug.Log("--- Files: ---");
        foreach (string name in pathfiles)
            Debug.Log(name);
        */

        //Find which is the latest/biggest ID
        if (pathfiles.Count > 0)
            foreach (string name in pathfiles)
            {
                //The enumeration to be accurate no matter what (aka what if a player copies his saves from another pc)
                //and to be fast (aka when 100 save files happen, rip cuz i have to get dat read access to all 100 in the same frame)
                //happens simply, by counting the amount of files. For example, if there are only 4 files, then that means, there are only 2 runs!
                //Aka, enumerate by 2.

                if (readAllPlayerStats == false)
                    readAllPlayerStats = true;
                else
                {
                    readAllPlayerStats = false;

                    playerStatsSaveID++;
                }

            }

        //Save the final path for the playerStats
        //C:\Users\Balroth\AppData\LocalLow\MadeByYellowArchitect+
        playerCoreStatsFilepath = Application.persistentDataPath + "\\" + playerCoreStatsFilename + playerStatsSaveID.ToString() + jsonAppend;
        playerPathStatsFilepath = Application.persistentDataPath + "\\" + playerPathStatsFilename + playerStatsSaveID.ToString() + jsonAppend;

        //Debug.Log(playerCoreStatsFilepath);
        //Debug.Log(playerPathStatsFilepath);

        PlayerStatsManager.globalInstance.SetPlayerStatsSaveID(playerStatsSaveID);
    }

    /// <summary>
    /// Takes PlayerCoreStats as a parameter, and converts it to JSON and creates the JSON file in the proper filepath.
    /// </summary>
    /// <param name="playerCoreStatsData">This is the gameobject that will be converted to JSON.</param>
    public void SaveCoreStatsData(PlayerCoreStats playerCoreStatsData)
    {
        string coreStatsJSONString = JsonUtility.ToJson(playerCoreStatsData, true);

        File.WriteAllText(playerCoreStatsFilepath, coreStatsJSONString);

        Debug.Log("Saved PlayerCoreStats");
    }

    /// <summary>
    /// Takes PlayerPathStats as a parameter, and converts it to JSON and creates the JSON file in the proper filepath.
    /// </summary>
    /// <param name="playerPathStatsData">This is the gameobject that will be converted to JSON.</param>
    public void SavePathStatsData(PlayerPathStats playerPathStatsData)
    {
        string pathStatsJSONString = JsonUtility.ToJson(playerPathStatsData, false);

        File.WriteAllText(playerPathStatsFilepath, pathStatsJSONString);

        Debug.Log("Saved PlayerPathStats");

        //For 1 hour, a file of 4.32 mb should be made (excluding InputData!!!)
    }


    /// <summary>
    /// Returns a new PlayerCoreStats instance, which is made from converting the JSON string to it.
    /// </summary>
    /// <returns></returns>
    public PlayerCoreStats GetCoreStatsData()
    {
        string coreStatsJSONString = File.ReadAllText(playerCoreStatsFilepath);

        PlayerCoreStats temp = JsonUtility.FromJson<PlayerCoreStats>(coreStatsJSONString);

        Debug.Log("LastPlayedTime is: " + temp.lastPlayedTime);

        return temp;
    }

    /// <summary>
    /// Returns a new PlayerPathStats instance, which is made from converting the JSON string to it.
    /// </summary>
    /// <returns></returns>
    public PlayerPathStats GetPathStatsData()
    {
        if (File.Exists(playerPathStatsFilepath))
        {
            string pathStatsJSONString = File.ReadAllText(playerPathStatsFilepath);

            return JsonUtility.FromJson<PlayerPathStats>(pathStatsJSONString);
        }
        else
        {
            Debug.Log("You tried to read PlayerPathStats, but it doesn't exist!");
            return default(PlayerPathStats);
        }
        
    }

    /// <summary>
    /// Returns a new PlayerCoreStats instance, which is made from converting the JSON string to it.
    /// It is offset by parameter. For example, if current is 5, and you put -2, then it will return the 3rd run
    /// Could use this with 0 or default parameter to get current playerstatssaveID
    /// </summary>
    /// <returns></returns>
    public PlayerCoreStats GetOffsetCoreStatsData(int offset = 0)
    {
        //Get the filepath of the offset
        string previousCoreStatsFilepath = Application.persistentDataPath + "\\" + playerCoreStatsFilename + (playerStatsSaveID + offset).ToString() + jsonAppend;

        if (File.Exists(previousCoreStatsFilepath))
        {
            //Get the content string from the JSON file
            string coreStatsJSONString = File.ReadAllText(previousCoreStatsFilepath);

            return JsonUtility.FromJson<PlayerCoreStats>(coreStatsJSONString);
        }
        else
        {
            Debug.Log("Offset run doesn't exist lmao");
            return default(PlayerCoreStats);
        }
    }

    //Used to see if first playthrough of the game.
    public int GetMaxStatsSaveID()
    {
        return playerStatsSaveID;
    }


    /// <summary>
    /// Detect if player has finished the game before
    /// Simply by checking if a file exists, instead of parsing all .json corestats and searching for ending or timetobeatlevel4!
    /// </summary>
    public bool GetPlayerClear()
    {
        string clearPath = Application.persistentDataPath + "\\" + "PlayerClear" + jsonAppend;

        if (File.Exists(clearPath))
            return true;
        else
            return false;
    }

    /// <summary>
    /// Indicator of having finished the game
    /// Simply by checking if a file exists, instead of parsing all .json corestats and searching for ending or timetobeatlevel4!
    /// </summary>
    public void SetPlayerClear()
    {
        string clearPath = Application.persistentDataPath + "\\" + "PlayerClear" + jsonAppend;

        //If has finished before, don't even bother re-writing!
        if (GetPlayerClear())
            return;

        //Write the PlayerClear.json onto the hard drive, and drop a wholesome message to the player who willed through the end <3 (((but also to keep him playing hehehe)))
        File.WriteAllText(clearPath, "Thank you for playing the game, I hope you had fun playing to the End! <3\n\nIf you are still not satisfied (hopefully xD), there are alternative endings and speedrun mode!\nThe Endless sea awaits~");
    }

    //Random knowledge for PlayerPrefs
    //https://answers.unity.com/questions/1137965/using-jsonutility.html
}
