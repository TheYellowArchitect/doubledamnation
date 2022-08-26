using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class ScrollTextureFluctuateManager : MonoBehaviour
{
    [Header("Fluctuating Values")]

    [Tooltip("The maximum fluctuation will always be between -rangeX and rangeX")]
    public float rangeX = 0;
    [Tooltip("The maximum fluctuation will always be between -rangeY and rangeY")]
    public float rangeY = 0;

    [Range(0,1)]
    [Tooltip("How much/often does this fluctuate?")]
    public float fluctuateXRatio = 0;
    [Range(0, 1)]
    [Tooltip("How much/often does this fluctuate?")]
    public float fluctuateYRatio = 0;

    [Header("Misc")]
    public float ratioFrequency = 0.1f;
    [Tooltip("Ignores the above, and runs every frame, aka max frequency possible")]
    public bool ratioFrequencyMax;

    [Header("ScrollTexture Array")]
    public ScrollTexture[] allScrollTextures;

    private float defaultOffsetX;
    private float defaultOffsetY;

    private float fluctuateXCurrentRatio = 0.5f;
    private float fluctuateYCurrentRatio = 0.5f;

    private static float fluctuateXRatioMax = 1;
    private static float fluctuateYRatioMax = 1;

    private bool fluctuateXRising = true;
    private bool fluctuateYRising = true;

    private ScrollTexture commonScrollTexture;

	// Use this for initialization
	void Start ()
    {
        if (rangeX == 0 && rangeY == 0)
        {
            Destroy(this);
            return;
        }

        //No need to check all children and if any differences, since if different, the visuals will be glitchy without this anyway.
        defaultOffsetX = allScrollTextures[0].offsetX;
        defaultOffsetY = allScrollTextures[0].offsetY;

        for (int i = 0; i < allScrollTextures.Length; i++)
            allScrollTextures[i].useSharedMaterial = true;

        StartCoroutine(Fluctuate());
	}
	

	public IEnumerator Fluctuate()
    {

        //True -> Destroy isn't called
        while(Input.GetKey(KeyCode.PageUp) == false)
        {
            ProcessFluctuationX();

            ProcessFluctuationY();

            for (int i = 0; i < allScrollTextures.Length; i++)
            {
                if (allScrollTextures[i].GetInitialized())
                    allScrollTextures[i].OffsetSharedMaterial();
            }
            

            if (ratioFrequencyMax == false)
            {
                if (ratioFrequency > 0)
                    yield return new WaitForSeconds(ratioFrequency);
                else
                {
                    Debug.LogError("ScrollTextureFluctuate has a frequency of less than 0!!!");
                    yield return new WaitForEndOfFrame();
                }
            }
            else
                yield return new WaitForEndOfFrame();
        }
        
	}

    public void ProcessFluctuationX()
    {
        if (fluctuateXRising)
        {
            fluctuateXCurrentRatio = fluctuateXCurrentRatio + fluctuateXRatio;
        }
        else
        {
            fluctuateXCurrentRatio = fluctuateXCurrentRatio - fluctuateXRatio;
        }

        //Change the offset for every texture at once.
        for (int i = 0; i < allScrollTextures.Length; i++)
            allScrollTextures[i].offsetX = defaultOffsetX + Mathf.Lerp(-rangeX, rangeX, fluctuateXCurrentRatio);

        //Flag
        if (fluctuateXCurrentRatio > fluctuateXRatioMax || fluctuateXCurrentRatio < fluctuateXRatioMax * -1)
        {
            if (fluctuateXCurrentRatio > fluctuateXRatioMax)
                fluctuateXCurrentRatio = fluctuateXRatioMax;//Without this... riperino riperoni, probable infinite looperoni
            else
                fluctuateXCurrentRatio = fluctuateXRatioMax * -1;
            fluctuateXRising = !fluctuateXRising;
        }
    }

    public void ProcessFluctuationY()
    {
        if (fluctuateYRising)
        {
            fluctuateYCurrentRatio = fluctuateYCurrentRatio + fluctuateYRatio;
        }
        else
        {
            fluctuateYCurrentRatio = fluctuateYCurrentRatio - fluctuateYRatio;
        }

        //Change the offset for every texture at once.
        for (int i = 0; i < allScrollTextures.Length; i++)
        {
            allScrollTextures[i].offsetY = defaultOffsetY + Mathf.Lerp(-rangeY, rangeY, fluctuateYCurrentRatio);
        }
            


        //Flag determiner
        if (fluctuateYCurrentRatio > fluctuateYRatioMax || fluctuateYCurrentRatio < fluctuateYRatioMax * -1)
        {
            if (fluctuateYCurrentRatio > fluctuateYRatioMax)
                fluctuateYCurrentRatio = fluctuateYRatioMax;//Without this... riperino riperoni, probable infinite looperoni
            else
                fluctuateYCurrentRatio = fluctuateYRatioMax * -1;
            fluctuateYRising = !fluctuateYRising;
        }
    }

}
