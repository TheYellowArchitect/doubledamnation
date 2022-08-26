using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSDisplayManager : MonoBehaviour
{
    //Singleton
    public static FPSDisplayManager globalInstance;

    //public string formatedString = "{NewFPSValue} (not decimal) ~ {OldFPSValue} FPS ({millisecondsValue} ms)";

    public GameObject fpsUI;

    [Header("The FPS Calculators")]
    public NewFPSCalculator newFPSCalculator;
    public OldFPSCalculator oldFPSCalculator;

    private TextMeshProUGUI fpsUIText;

    //Should be only readable
    public bool active = true;

    // Use this for initialization
    void Start ()
    {
        globalInstance = this;

        fpsUIText = fpsUI.GetComponent<TextMeshProUGUI>();

        if (GameManager.testing == false)
            SetFPSText(false);
	}
	
	// Update is called once per frame
	void LateUpdate ()
    {
        if (active)
            //Sorry for chain-replace spaghetti.
            //Use https://docs.microsoft.com/en-us/dotnet/api/system.string.format?view=netframework-4.8 next time.
            //fpsUIText.text = formatedString.Replace("{NewFPSValue}", newFPSCalculator.GetFPS().ToString("0")).Replace("{OldFPSValue}", oldFPSCalculator.GetFPS().ToString("0")).Replace("{millisecondsValue}", oldFPSCalculator.GetMilliseconds().ToString("0"));
            //fpsUIText.text = newFPSCalculator.GetFPS().ToString("0") + " FPS (" + oldFPSCalculator.GetMilliseconds().ToString("0") + " ms)";
            fpsUIText.text = newFPSCalculator.GetFPS().ToString("0") + " FPS (" + oldFPSCalculator.GetMilliseconds().ToString("0") + " ms)";
            //Both suck. I dont think either is accurate. Solid 60 FPS should happen at level 1 start, as uncapped i reach 100+ FPS constantly.
            //Anyway, better than having 2 of them, since 57 ~ 60 shows fluctuating FPS which is bad, especially since the player doesnt know which is true (NEITHER LOL)
    }

    public void SetFPSText(bool activateFPSText)
    {
        active = activateFPSText;

        newFPSCalculator.SetCalculation(activateFPSText);
        oldFPSCalculator.SetCalculation(activateFPSText);

        fpsUI.SetActive(activateFPSText);
    }

    //Also, reminder that Unity Editor's "Stats" profiler, is horribly wrong.
    //Hell, it is locked in 60 FPS and almost always it shows 70+ FPS.
    //And then I saw this that more or less confirms its inaccurate asf: https://forum.unity.com/threads/is-fps-in-the-editor-accurate.195549/

}
