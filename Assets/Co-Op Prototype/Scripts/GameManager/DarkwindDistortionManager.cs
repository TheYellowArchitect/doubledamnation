using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Player Choice: The Class. (bless that day with Zee)
public class DarkwindDistortionManager : MonoBehaviour
{
    public static DarkwindDistortionManager globalInstance;

    //Remind yourself... The greater the power, the lesser the glory!
    public int newMaxHealth;

    public bool noManaLimitOnSpellwords = false;

    public enum Chaindash { Normal, Disabled, Infinite };
    public Chaindash playerChaindash = Chaindash.Normal;

    [Tooltip("If you have any influenceX, should it get reset to 0, if you reach idle state? "
        + "\nBy default yes, otherwise, you will slide off the last-built momentum which is more natural but also seems janky")]
    public bool allowIdleSlide = false;

    [Tooltip("Determines if you can stack momentum on a ground wall (aka cheese it by getting as much speed you want from a ground wall)")]
    public bool allowGroundWallSpeedCheese = false;

    [Tooltip("No matter how fast you go, the game allows you to go up to 200 InfluenceX. True if you want to exceed the 200.")]
    public bool allowInfiniteSpeedMomentum = false;


    //If true, you can jump only with buttons and triggers.
    //public bool disableJumpFromJoystick = false;//Disabled because it needs a lot of refactoring on jump and its input to trigger it.

    /// <summary>
    /// Accepts only inputs that are different than previous inputs.
    /// </summary>
    public bool ignoreSameAttackJoystickInput = false;

    /// <summary>
    /// Accepts only inputs that have a radius of 1 aka the whole perimeter.
    /// In other words, act as a tilt stick, and take only the edges.
    /// </summary>
    public bool acceptAttackInputOnlyOnJoystickEdges = true;

    /// <summary>
    /// When P2 dashes, and P1 runs on opposite direction, after he stops running and influenceX still pushes him opposite to running
    /// Should it pull him backwards (with dat unfitting animation) properly, or just influenceX = 0 and ggwp?
    /// </summary>
    public bool allowReverseDarkwindPull = false;

    //When to regain walljump? Touching horizontal ground ofc, and when else? On kill enemy? On taking damage/hit?
    public enum WallJumpReset { GroundOnly, Hit, Kill, KillAndHit};
    public WallJumpReset playerWalljumpReset = WallJumpReset.KillAndHit;

    public enum CrouchDashRestore { Perfect, Lazy };
    public CrouchDashRestore playerCrouchDashRestore = CrouchDashRestore.Perfect;

    public enum WallJumpingLimit { Never, Once, Twice, Infinite};
    public WallJumpingLimit playerWallJumpingLimit = WallJumpingLimit.Once;

    //Unused.
    public enum InversedCameraOptions { None, FlipX, FlipY, FlipBoth};
    public InversedCameraOptions mainCameraInversedOptions = InversedCameraOptions.None;

    /// <summary>
    /// Making this just in case a controller is broken or sensitivity is wrong, so it can be tweaked.
    /// Obviously, the default is 1.
    /// </summary>
    public float RightJoystickRadius = 1f;

    private WarriorHealth warriorHealthScript;

    private void Awake()
    {
        globalInstance = this;

        warriorHealthScript = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>();
    }

    public void AddMaxHealth(int newMaxHealth)
    {
        warriorHealthScript.AddMaxHealth(newMaxHealth);
    }

    public void ResetMaxHealth()
    {
        warriorHealthScript.MaxHealth = warriorHealthScript.defaultMaxHealth;
    }

    public int GetCurrentMaxHealth()
    {
        return warriorHealthScript.MaxHealth;
    }

}
