using NaughtyAttributes;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public enum VFXName { DamagedPlayer, MagicCircleWalljump, MagicCircleGround, MagicCircleMidair, JumpSmokeGround, JumpSmokeMidair, ManaRingGain, ManaRingLoss, TempHPGain, TempHPAura, SpellFail, DamagedEnemy, DyingEnemy, FireRing, PlayerKiller, Dodgeroll, Wavedash, PushGround, PushMidair, Enviroment, EnemyMagicalBurst, DashCooldown, MagicCircleRevive, ReviveWind, MageSummon, MageLink, DesyncLocationSphere};

    [BoxGroup("Level1")]
    [Tooltip("When P2 walljumps")]
    public GameObject MagicCircleWalljump1;
    [BoxGroup("Level1")]
    [Tooltip("When P2 jumps first")]
    public GameObject MagicCircleMidair1;
    [BoxGroup("Level1")]
    [Tooltip("When P2 jumps second")]
    public GameObject MagicCircleGround1;
    [BoxGroup("Level1")]
    [Tooltip("When P1 jumps first")]
    public GameObject JumpSmokeGround1;
    [BoxGroup("Level1")]
    [Tooltip("When P1 jumps second/midair")]
    public GameObject JumpSmokeMidair1;
    [BoxGroup("Level1")]
    [Tooltip("When P1 kills an enemy, and gains one mana, the ring forms")]
    public GameObject ManaRingGain1;//Unused, used via mana manager for single-responsibility (what a hypocritic thing to say after I have written all this mess.)
    [BoxGroup("Level1")]
    [Tooltip("When P1 casts a spell, and loses one mana, the ring forms")]
    public GameObject ManaRingLoss1;//Like the above, it used to be called on WarriorMovement.cs on SetMana() function , right after Removing/Adding Mana();
    [BoxGroup("Level1")]
    [Tooltip("When Player gains TempHP(coin or kills), that VFX")]
    public GameObject TempHPGain1;
    [BoxGroup("Level1")]
    [Tooltip("When Player gains TempHP, the constant VFX until he takes damage")]
    public GameObject TempHPAura1;
    [BoxGroup("Level1")]
    [Tooltip("When Player gets damaged")]
    public GameObject DamagedPlayer1;
    [BoxGroup("Level1")]
    [Tooltip("When Enemy gets damaged")]
    public GameObject DamagedEnemy1;
    [BoxGroup("Level1")]
    [Tooltip("The buff of the playerkiller monster")]
    public GameObject PlayerKiller1;
    [BoxGroup("Level1")]
    [Tooltip("When Player dodgerolls(ground)")]
    public GameObject Dodgeroll1;
    [BoxGroup("Level1")]
    [Tooltip("When player wavedashes (midair)")]
    public GameObject Wavedash1;
    [BoxGroup("Level1")]
    [Tooltip("When player casts push spell, when he is on the ground")]
    public GameObject PushGround1;
    [BoxGroup("Level1")]
    [Tooltip("When player casts push spell, when he is on the air")]
    public GameObject PushMidair1;
    [BoxGroup("Level1")]
    [Tooltip("Enviroment VFX to attach to player, to fake the particles being everywhere.")]
    public GameObject Enviroment1;
    [BoxGroup("Level1")]
    [Tooltip("Currently, the VFX to appear when a hollow dies")]
    public GameObject EnemyMagicalBurst1;
    [BoxGroup("Level1")]
    [Tooltip("The VFX when dash cooldown refreshes")]
    public GameObject DashCooldown1;
    [BoxGroup("Level1")]
    [Tooltip("When revive happens, this is the circle P1 comes through.")]
    public GameObject MagicCircleRevive1;
    [BoxGroup("Level1")]
    [Tooltip("When revive happens, this is the wind VFX that is playing while the circle is spinning")]
    public GameObject ReviveWind1;
    [BoxGroup("Level1")]
    [Tooltip("When P2 spawns")]
    public GameObject MageSummon1;
    [BoxGroup("Level1")]
    [Tooltip("When P2 casts link")]
    public GameObject MageLink1;
    [BoxGroup("Level1")]
    [Tooltip("When desync happens, this spawns and moves")]
    public GameObject DesyncLocationSphere1;

    [BoxGroup("Level2")]
    [Tooltip("When P2 walljumps")]
    public GameObject MagicCircleWalljump2;
    [BoxGroup("Level2")]
    [Tooltip("When P2 jumps first")]
    public GameObject MagicCircleMidair2;
    [BoxGroup("Level2")]
    [Tooltip("When P2 jumps second")]
    public GameObject MagicCircleGround2;
    [BoxGroup("Level2")]
    [Tooltip("When P1 jumps first")]
    public GameObject JumpSmokeGround2;
    [BoxGroup("Level2")]
    [Tooltip("When P1 jumps second/midair")]
    public GameObject JumpSmokeMidair2;
    [BoxGroup("Level2")]
    [Tooltip("When P1 kills an enemy, and gains one mana, the ring forms")]
    public GameObject ManaRingGain2;
    [BoxGroup("Level2")]
    [Tooltip("When P1 casts a spell, and loses one mana, the ring forms")]
    public GameObject ManaRingLoss2;
    [BoxGroup("Level2")]
    [Tooltip("When Player gains TempHP, either by X kills or by an item/coin")]
    public GameObject TempHPGain2;
    [BoxGroup("Level2")]
    [Tooltip("When Player gains TempHP, the constant VFX until he takes damage")]
    public GameObject TempHPAura2;
    [BoxGroup("Level2")]
    [Tooltip("When Player gets damaged")]
    public GameObject DamagedPlayer2;
    [BoxGroup("Level2")]
    [Tooltip("When Enemy gets damaged")]
    public GameObject DamagedEnemy2;
    [BoxGroup("Level2")]
    [Tooltip("The buff of the playerkiller monster")]
    public GameObject PlayerKiller2;
    [BoxGroup("Level2")]
    [Tooltip("When Player dodgerolls")]
    public GameObject Dodgeroll2;
    [BoxGroup("Level2")]
    [Tooltip("When player wavedashes (midair)")]
    public GameObject Wavedash2;
    [BoxGroup("Level2")]
    [Tooltip("When player casts push spell, when he is on the ground")]
    public GameObject PushGround2;
    [BoxGroup("Level2")]
    [Tooltip("When player casts push spell, when he is on the air")]
    public GameObject PushMidair2;
    [BoxGroup("Level2")]
    [Tooltip("Enviroment VFX to attach to player, to fake the particles being everywhere.")]
    public GameObject Enviroment2;
    [BoxGroup("Level2")]
    [Tooltip("Currently, the VFX to appear when a hollow dies")]
    public GameObject EnemyMagicalBurst2;
    [BoxGroup("Level2")]
    [Tooltip("The VFX when dash cooldown refreshes")]
    public GameObject DashCooldown2;
    [BoxGroup("Level2")]
    [Tooltip("When revive happens, this is the circle P1 comes through.")]
    public GameObject MagicCircleRevive2;
    [BoxGroup("Level2")]
    [Tooltip("When revive happens, this is the wind VFX that is playing while the circle is spinning")]
    public GameObject ReviveWind2;
    [BoxGroup("Level2")]
    [Tooltip("When P2 spawns")]
    public GameObject MageSummon2;
    [BoxGroup("Level2")]
    [Tooltip("When P2 casts link")]
    public GameObject MageLink2;
    [BoxGroup("Level2")]
    [Tooltip("When desync happens, this spawns and moves")]
    public GameObject DesyncLocationSphere2;

    [BoxGroup("Level3")]
    [Tooltip("When P2 walljumps")]
    public GameObject MagicCircleWalljump3;
    [BoxGroup("Level3")]
    [Tooltip("When P2 jumps first")]
    public GameObject MagicCircleMidair3;
    [BoxGroup("Level3")]
    [Tooltip("When P2 jumps second")]
    public GameObject MagicCircleGround3;
    [BoxGroup("Level3")]
    [Tooltip("When P1 jumps first")]
    public GameObject JumpSmokeGround3;
    [BoxGroup("Level3")]
    [Tooltip("When P1 jumps second/midair")]
    public GameObject JumpSmokeMidair3;
    [BoxGroup("Level3")]
    [Tooltip("When P1 kills an enemy, and gains one mana, the ring forms")]
    public GameObject ManaRingGain3;
    [BoxGroup("Level3")]
    [Tooltip("When P1 casts a spell, and loses one mana, the ring forms")]
    public GameObject ManaRingLoss3;
    [BoxGroup("Level3")]
    [Tooltip("When Player gains TempHP, either by X kills or by an item/coin")]
    public GameObject TempHPGain3;
    [BoxGroup("Level3")]
    [Tooltip("When Player gains TempHP, the constant VFX until he takes damage")]
    public GameObject TempHPAura3;
    [BoxGroup("Level3")]
    [Tooltip("When Player gets damaged")]
    public GameObject DamagedPlayer3;
    [BoxGroup("Level3")]
    [Tooltip("When Enemy gets damaged")]
    public GameObject DamagedEnemy3;
    [BoxGroup("Level3")]
    [Tooltip("The buff of the playerkiller monster")]
    public GameObject PlayerKiller3;
    [BoxGroup("Level3")]
    [Tooltip("When Player dodgerolls")]
    public GameObject Dodgeroll3;
    [BoxGroup("Level3")]
    [Tooltip("When player wavedashes (midair)")]
    public GameObject Wavedash3;
    [BoxGroup("Level3")]
    [Tooltip("When player casts push spell, when he is on the ground")]
    public GameObject PushGround3;
    [BoxGroup("Level3")]
    [Tooltip("When player casts push spell, when he is on the air")]
    public GameObject PushMidair3;
    [BoxGroup("Level3")]
    [Tooltip("Enviroment VFX to attach to player, to fake the particles being everywhere.")]
    public GameObject Enviroment3;
    [BoxGroup("Level3")]
    [Tooltip("Currently, the VFX to appear when a hollow dies")]
    public GameObject EnemyMagicalBurst3;
    [BoxGroup("Level3")]
    [Tooltip("The VFX when dash cooldown refreshes")]
    public GameObject DashCooldown3;
    [BoxGroup("Level3")]
    [Tooltip("When revive happens, this is the circle P1 comes through.")]
    public GameObject MagicCircleRevive3;
    [BoxGroup("Level3")]
    [Tooltip("When revive happens, this is the wind VFX that is playing while the circle is spinning")]
    public GameObject ReviveWind3;
    [BoxGroup("Level3")]
    [Tooltip("When P2 spawns")]
    public GameObject MageSummon3;
    [BoxGroup("Level3")]
    [Tooltip("When P2 casts link")]
    public GameObject MageLink3;
    [BoxGroup("Level3")]
    [Tooltip("When desync happens, this spawns and moves")]
    public GameObject DesyncLocationSphere3;


    [BoxGroup("LevelIndependent")]
    [Tooltip("When Player casts a spell but 0 mana")]
    public GameObject SpellFail;


    [Header("Monsters")]
    [Tooltip("When Enemy dies\nUseless. It is supposed to be a more powerful dmg sprite, but it seems unnecessary when the death feedback + gaining mana is so obvious.")]
    public GameObject DyingEnemy;

    //Both unused lel
    /* Just fix the orbit to look natural/smooth and don't put VFX when harpy fires. Same for player.
    [Tooltip("When Harpy uses one fire mana, the ring forms, level 2")]
    public GameObject FireRing2;
    [Tooltip("When Harpy uses one fire mana, the ring forms, level 3")]
    public GameObject FireRing3;
    */

    //So every1 can access it, without searching for tags and bs like that.
    public static VFXManager globalInstance { set; get; }

    private GameObject lastCreatedVFX;
    private Vector3 cameraDepthVector;//Used for depth objects, so they won't get fucked by the camera, aka magic circles (currently)

    //Happens at the very start of the game
    private void Awake()
    {
        globalInstance = this;

        cameraDepthVector = new Vector3(0, 0, 7);
    }

    public GameObject SpawnVFX(VFXName vfx, Vector3 spawnLocation, GameObject parent = null)
    {
        //Called by WarriorHealth
        if (vfx == VFXName.DamagedPlayer)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(DamagedPlayer1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(DamagedPlayer2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(DamagedPlayer3, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.MagicCircleWalljump)//Walljump
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(MagicCircleWalljump1, spawnLocation + cameraDepthVector, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(MagicCircleWalljump2, spawnLocation + cameraDepthVector, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(MagicCircleWalljump3, spawnLocation + cameraDepthVector, Quaternion.identity);

            if (GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().GetFacingLeft() == false)
            {
                //Rotating the children instead, because the parent uses rotation for animation.
                lastCreatedVFX.transform.GetChild(0).Rotate(0, 0, 180);
                lastCreatedVFX.transform.GetChild(1).Rotate(0, 0, 180);
            }
            //else
            //lastCreatedVFX.transform.Rotate(0, 0, 0);
        }
        else if (vfx == VFXName.MagicCircleGround)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(MagicCircleGround1, spawnLocation + cameraDepthVector, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(MagicCircleGround2, spawnLocation + cameraDepthVector, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(MagicCircleGround3, spawnLocation + cameraDepthVector, Quaternion.identity);
        }
        else if (vfx == VFXName.MagicCircleMidair)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(MagicCircleMidair1, spawnLocation + cameraDepthVector, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(MagicCircleMidair2, spawnLocation + cameraDepthVector, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(MagicCircleMidair3, spawnLocation + cameraDepthVector, Quaternion.identity);
        }
        else if (vfx == VFXName.JumpSmokeGround)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
            {
                lastCreatedVFX = Instantiate(JumpSmokeGround1, spawnLocation + new Vector3(2.5f, 0, 0), Quaternion.identity);

                lastCreatedVFX = Instantiate(JumpSmokeGround1, spawnLocation + new Vector3(-2.5f, 0, 0), Quaternion.identity);
            }
            else if (LevelManager.currentLevel == 2)
            {
                lastCreatedVFX = Instantiate(JumpSmokeGround2, spawnLocation + new Vector3(2.5f, 0, 0), Quaternion.identity);

                lastCreatedVFX = Instantiate(JumpSmokeGround2, spawnLocation + new Vector3(-2.5f, 0, 0), Quaternion.identity);
            }
            else if (LevelManager.currentLevel > 2)
            {
                lastCreatedVFX = Instantiate(JumpSmokeGround3, spawnLocation + new Vector3(2.5f, 0, 0), Quaternion.identity);

                lastCreatedVFX = Instantiate(JumpSmokeGround3, spawnLocation + new Vector3(-2.5f, 0, 0), Quaternion.identity);
            }

            lastCreatedVFX.GetComponent<SpriteRenderer>().flipX = true;
        }
        else if (vfx == VFXName.JumpSmokeMidair)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(JumpSmokeMidair1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(JumpSmokeMidair2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(JumpSmokeMidair3, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.ManaRingGain)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(ManaRingGain1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(ManaRingGain2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(ManaRingGain3, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.ManaRingLoss)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(ManaRingLoss1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(ManaRingLoss2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(ManaRingLoss3, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.TempHPGain)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(TempHPGain1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(TempHPGain2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(TempHPGain3, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.TempHPAura)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(TempHPAura1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(TempHPAura2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(TempHPAura3, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.SpellFail)
        {
            lastCreatedVFX = Instantiate(SpellFail, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.DamagedEnemy)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(DamagedEnemy1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(DamagedEnemy2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(DamagedEnemy3, spawnLocation, Quaternion.identity);
        }
        /*
        else if (vfx == VFXName.FireRing)
        {
            if (LevelManager.currentLevel < 3)
                lastCreatedVFX = Instantiate(FireRing2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(FireRing3, spawnLocation, Quaternion.identity);
        }
        */
        else if (vfx == VFXName.PlayerKiller)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(PlayerKiller1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(PlayerKiller2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(PlayerKiller3, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.Dodgeroll)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(Dodgeroll1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(Dodgeroll2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(Dodgeroll3, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.Wavedash)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(Wavedash1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(Wavedash2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(Wavedash3, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.PushMidair)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(PushMidair1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(PushMidair2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(PushMidair3, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.PushGround)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(PushGround1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(PushGround2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(PushGround3, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.Enviroment)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(Enviroment1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(Enviroment2, spawnLocation, Quaternion.identity);
            //else if (LevelManager.currentLevel > 2)
            //lastCreatedVFX = Instantiate(Enviroment3, spawnLocation, Quaternion.identity);//nothing, for now. But add some "wind lines" to blow once in a while.
        }
        else if (vfx == VFXName.EnemyMagicalBurst)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(EnemyMagicalBurst1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(EnemyMagicalBurst2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(EnemyMagicalBurst3, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.DashCooldown)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(DashCooldown1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(DashCooldown2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(DashCooldown3, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.MagicCircleRevive)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(MagicCircleRevive1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(MagicCircleRevive2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(MagicCircleRevive3, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.ReviveWind)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(ReviveWind1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(ReviveWind2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(ReviveWind3, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.MageSummon)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(MageSummon1, spawnLocation + new Vector3(0, 0, 30f), Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(MageSummon2, spawnLocation + new Vector3(0, 0, 30f), Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(MageSummon3, spawnLocation + new Vector3(0, 0, 30f), Quaternion.identity);
            //30Z added because joint spirals move beyond the camera's Z!

            //Hacky, idk why Unity is bugged but this fixes it. (tl;dr spawns the prefab on scale Z = 0, and because of that, its position Z is also 0 fml.)
            /*if (lastCreatedVFX.transform.position.z < 30)
            {
                lastCreatedVFX.transform.localScale = new Vector3(lastCreatedVFX.transform.localScale.x, lastCreatedVFX.transform.localScale.y, 1);
                lastCreatedVFX.transform.position = new Vector3(lastCreatedVFX.transform.position.x, lastCreatedVFX.transform.position.y, 30);
            }*/
            //^For some reason it doesnt fucking work. Wtf unity? Fucking engine bugs i cant do shit about REEEEEEEEEEEEE
        }
        else if (vfx == VFXName.MageLink)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(MageLink1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(MageLink2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(MageLink3, spawnLocation, Quaternion.identity);
        }
        else if (vfx == VFXName.DesyncLocationSphere)
        {
            if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
                lastCreatedVFX = Instantiate(DesyncLocationSphere1, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel == 2)
                lastCreatedVFX = Instantiate(DesyncLocationSphere2, spawnLocation, Quaternion.identity);
            else if (LevelManager.currentLevel > 2)
                lastCreatedVFX = Instantiate(DesyncLocationSphere3, spawnLocation, Quaternion.identity);
        }


        if (parent != null && lastCreatedVFX != null)
            lastCreatedVFX.transform.SetParent(parent.transform, true);

        return lastCreatedVFX;
    }

    //I could turn the SpawnVFX into GameObject return type, but I fear breaking stuff.
    public GameObject GetLastCreatedVFX()
    {
        return lastCreatedVFX;
    }

}
