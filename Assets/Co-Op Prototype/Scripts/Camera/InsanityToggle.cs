using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsanityToggle : MonoBehaviour
{
    public ushort midCutsceneDialogueIndex = 19;//"Finally, you opened your eyes..."
    [Header("Plug the Background here")]
    public SpriteRenderer clay;
    public SpriteRenderer flatColor;

    

    private bool active = false;
    private Camera cameraComponent;
    private CameraClearFlags defaultCameraFlags;
    private InsanityIntensityManager commonInsanityIntensity;

    private bool hasActivatedOnce = false;

    private void Start()
    {
        //Get Camera component
        //if (transform.parent != null && transform.parent.GetComponent<Camera>() != null)
            cameraComponent = transform.parent.GetComponent<Camera>();
        //else
            //cameraComponent = Camera.main;
        defaultCameraFlags = cameraComponent.clearFlags;

        commonInsanityIntensity = GetComponent<InsanityIntensityManager>();
    }

    /// <summary>
    /// Tinkers with the camera's flags, to "simulate" insanity since it doesn't refresh the screen every frame.
    /// To be called from "Insanity" spellword or pressing End (yes even in final vers).
    /// </summary>
    public void ToggleInsanity()
    {
        //Trigger voiceline
        if (hasActivatedOnce == false && active == false)
        {
            hasActivatedOnce = true;

            GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayerEnteredMidCutscene(midCutsceneDialogueIndex);
        }

        //Deactivate
        if (active)
        {
            active = false;

            //Reactivates the background
            clay.gameObject.SetActive(true);
            flatColor.gameObject.SetActive(true);

            //Set camera flags to normal/default
            cameraComponent.clearFlags = defaultCameraFlags;

            //De-Activate Insanity Intensity
            commonInsanityIntensity.ToggleIntensity(false);
        }
        else//Activate
        {
            active = true;

            //Disables the background, because otherwise clearing the flags by themselves does nothing
            clay.gameObject.SetActive(false);
            flatColor.gameObject.SetActive(false);

            //Empty the camera flags
            cameraComponent.clearFlags = CameraClearFlags.Nothing;

            //Activate Insanity Intensity
            commonInsanityIntensity.ToggleIntensity(true);

        }
    }

    public bool GetInsanityActive()
    {
        return active;
    }

}
