using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Remake this script, using only offsetX and offsetY with the Update. Then, make the fluctuation an "plug-in" which simply tweaks these values, so as to save performance from scripts that dont use fluctuation!!
/// </summary>
public class ScrollTexture : MonoBehaviour
{
    [Header("Main Values")]
    public float offsetX = 0;
    public float offsetY = 0;

    [HideInInspector]
    public bool useSharedMaterial = false;

    private Renderer spriteRenderer;

    private bool hasInitialized = false;

    private void Start()
    {
        spriteRenderer = GetComponent<Renderer>();

        hasInitialized = true;
    }

    // Update is called once per frame
    void Update ()
    {
        //Offset should be always within 0 and -1? Big float numbers over time will fuck up computation as it scales btw, but spagghetti 1 line ftw i guess...
        if (useSharedMaterial == false)
            OffsetMaterial();
    }

    public void OffsetMaterial()
    {
        spriteRenderer.material.mainTextureOffset = new Vector2(offsetX * Time.time, offsetY * Time.time);
    }

    public void OffsetSharedMaterial()
    {
        spriteRenderer.sharedMaterial.mainTextureOffset = new Vector2(offsetX * Time.time, offsetY * Time.time);
    }

    public bool GetInitialized()
    {
        return hasInitialized;
    }

    /* Example implementation to make the above slightly better for memory and for extended gametimes.
    public float moveTextureSpeedX;
    public float moveTextureSpeedY;
    float moveX;
    float moveY;

    void Update () 
    {
        moveX = (moveTextureSpeedX * Time.deltaTime) + moveX;
        moveY = (moveTextureSpeedY * Time.deltaTime) + moveY;
        if (moveX > 1) moveX = 0;
        if (moveY > 1) moveY = 0;
        GetComponent<Renderer>().material.mainTextureOffset = new Vector2(moveX, moveY);
     }
      
    */
}
