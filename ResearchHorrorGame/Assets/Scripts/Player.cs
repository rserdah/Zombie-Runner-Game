using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityStandardAssets.Characters.FirstPerson;


/*

TODO: see this idea

Online/local multiplayer game idea (can be online and local multiplayer so can even have split screen device playing online at the same time)

A teamwork based game where the effect of the individual is changed (not necessarily increased) by the amount of teamwork involved in a certain situation.

Examples:

If one player currently is holding a time changing totem (?) (item that gives an effect while it is being held), then they can control the game time 100% when they are far away from anyone without the time changing effect,
but as they get closer to other players (teammates or enemies), then that effect is averaged with the effects of surrounding players with distance being the weighing factor.

Other effects can have benefits of being close to the totem-weilding player: for example the speed up totem (makes players move faster) can make players around them move faster as well, can also be for a health totem, etc.

Maybe also make it either so players can change how the totem works (e.g. does it become more/less effective around others, what factor is the weighing factor (distance,
something else, etc.))
OR
make it so that it cannot be changed and the players have to figure out how best to use the totems they have found

Also make machines/contraptions that have to be coordinated with players in order to work efficiently (e.g. boat that has to be rowed in an ideal rythm to be fast/effective, etc.)

 */

public class Player : MonoBehaviour
{
    public static Player player;
    public static Vector3 playerStartPosition;
    public static float maxPlayerDistance;
    public static Material selectedMaterial;

    public Dictionary<string, string> activeClues = new Dictionary<string, string>();

    [System.Serializable]
    public class Stats
    {
        public enum StatType
        {
            HEALTH,
            STAMINA,
            ENERGY,
            WATER
        }

        public float health = 100;
        public float stamina = 100;
        public float energy = 100;
        public float water = 100;

        /// <summary>
        /// How fast Player regains (x) stamina and drains (y) stamina
        /// </summary>
        public Vector2 staminaGainDrainSpeed = new Vector2(10, 25);

        /// <summary>
        /// How fast energy drains over time. The default value drains energy completely after 20 minutes based off of 100 energy points (at rest).
        /// (Calculate by dividing the number of points by the number of seconds you want it to drain completely after)
        /// In addition, this stat drains as stamina drains over time.
        /// </summary>
        public float energyDrainSpeed = 100f/1200f;

        /// <summary>
        /// How fast water drains over time. The defualt value drains water completely after 10 minutes based off of 100 water points (at rest).
        /// (Calculate by dividing the number of points by the number of seconds you want it to drain completely after)
        /// In addition, this stat drains as stamina drains over time.
        /// </summary>
        public float waterDrainSpeed = 100f/600f;
    }

    private RigidbodyFirstPersonController controller;
    private Animator anim;
    private Selector selector;

    public Stats stats = new Stats();

    //private IEnumerator zoomCoroutine;
    public Camera cam;
    //public Vector2 zoomLevels = new Vector2(60, 45);
    //public float zoomSpeed = 10f;

    //private float tZoom;
    public UnityEngine.UI.Text text;

    public static UnityAction<Stats> hudCallback;

    [SerializeField]
    private float timeScale = 1f;

    [SerializeField]
    private bool lockMovementDirection = false;
    private bool canLookBack = true; //Player can look back only after a certain amount of time (so they can't spam that feature; hardcoded time)
    private bool canSlideKick = true;

    public AudioMixer masterMixer;


    private void Awake()
    {
        if(player != null)
        {
            gameObject.SetActive(false);
            return;
        }

        player = this;
        playerStartPosition = player.transform.position;

        selectedMaterial = Resources.Load<Material>("Materials/Selected");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        controller = GetComponent<RigidbodyFirstPersonController>();

        anim = GetComponent<Animator>();

        cam = transform.GetComponentInChildren<Camera>();

        controller.lockedForwardDirection = Vector3.forward;
        controller.lockedRightDirection = Vector3.right;

        selector = GetComponent<Selector>();
        selector.Init(this);

        hudCallback += (Stats _) => { HUD.SetDistance(maxPlayerDistance = Mathf.Max(maxPlayerDistance, player.transform.position.z - playerStartPosition.z)); };
        hudCallback += (Stats _) => { HUD.SetTimerSeconds(Time.time); };
        hudCallback += (Stats _) => { HUD.SetSpeedometer((player.controller.rigidbody.velocity.z * Vector3.forward).magnitude); };
    }

    private void Update()
    {
        Time.timeScale = timeScale;
        //Setting Time.fixedDeltaTime sets the fixed time step in settings
        //When setting the timeScale, we must also keep the same ratio between timeScale and time step so there are no choppy frames during slow motion
        Time.fixedDeltaTime = 0.02f * timeScale; //0.02f is the default so multiply that by the ratio (since timeScale is originally 1, the ratio is equal to timeScale)
                                                 //to get consistency

        masterMixer.SetFloat("Pitch", timeScale);

        controller.lockMovementDirection = lockMovementDirection;

        //Attack
        if(canSlideKick && Input.GetButtonDown("Fire1") && controller.rigidbody.velocity.sqrMagnitude > 2.5f)
        {
            anim.Play("SlideKick");

            //10000 force is currently a perfect value for this animation. However, it only works if player has a little momentum currently. FIX THIS ISSUE
            controller.rigidbody.AddForce(transform.forward * 10000f);

            canSlideKick = false;
            Invoke(nameof(ResetSlideKick), 4f);
        }

        //Zoom in
        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            //Zoom(true);
        }

        //Zoom out
        if(Input.GetKeyUp(KeyCode.Mouse1))
        {
            //Zoom(false);
        }

        //Interact
        if(Input.GetKeyDown(KeyCode.E))
        {
            if(selector.Selected != null)
                selector.Selected.triggerable.Invoke();
        }

        //Look back
        if(canLookBack && Input.GetKeyDown(KeyCode.Q) && !lockMovementDirection)
        {
            anim.Play("LookBack");
            canLookBack = false;
        }

        //Look forward
        if(Input.GetKeyUp(KeyCode.Q) && lockMovementDirection)
        {
            anim.Play("LookForward");
            Invoke(nameof(ResetLookBack), 5f);
        }

        CalculateStats();

        //Update HUD
        hudCallback?.Invoke(stats);

        //
        if(Input.GetKeyDown(KeyCode.Alpha0))
            HUD.pointer = HUD.PointerType.NORMAL;

        if(Input.GetKeyDown(KeyCode.Alpha1))
            HUD.pointer = HUD.PointerType.HAND;

        if(Input.GetKeyDown(KeyCode.Alpha2))
            HUD.pointer = HUD.PointerType.UNAVAILABLE;
    }

    private void ResetLookBack()
    {
        canLookBack = true;
    }

    private void ResetSlideKick()
    {
        canSlideKick = true;
    }

    /*private void Zoom(bool zoom)
    {
        if(zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);

        zoomCoroutine = ZoomCoroutine(zoom);
        StartCoroutine(zoomCoroutine);
    }

    private IEnumerator ZoomCoroutine(bool zoom)
    {
        float fov = cam.fieldOfView;
        //Vector3 gunPos = gun.localPosition;
        //Quaternion gunRot = gun.localRotation;

        tZoom = 0;
        while((zoom && cam.fieldOfView > zoomLevels.y) || (!zoom && cam.fieldOfView < zoomLevels.x))
        {
            cam.fieldOfView = Mathf.Lerp(fov, zoom ? zoomLevels.y : zoomLevels.x, tZoom);
            //gun.localPosition = Vector3.Lerp(gunPos, zoom ? gunZoomPos : gunNormalPos, t);
            //gun.localRotation = Quaternion.Lerp(gunRot, zoom ? gunZoomRot : gunNormalRot, t);

            tZoom += Time.deltaTime * zoomSpeed;

            yield return new WaitForFixedUpdate();
        }
    }*/








    //***************************************************************************************************************************************************************************
    //***************************************************************************************************************************************************************************
    //***************************************************************************************************************************************************************************
    //***************************************************************************************************************************************************************************

    private void CalculateStats()
    {
        CalculateHealth();
        CalculateStamina();
        CalculateEnergy();
        CalculateWater();
    }

    private void CalculateHealth()
    {
        if(stats.water <= 0 || stats.energy <= 0)
            stats.health -= ((stats.water <= 0 ? 1 : 0) * 2.5f + (stats.energy <= 0 ? 1 : 0) * 2.5f) * Time.deltaTime;

        stats.health = Mathf.Clamp(stats.health, 0, 100);

        if(stats.health <= 0)
        {
            Debug.LogError("Game Over");
        }
    }

    private void CalculateStamina()
    {
        if(controller.Grounded)
        {
            if(controller.Running && stats.stamina > 0)
                stats.stamina -= stats.staminaGainDrainSpeed.y * Time.deltaTime;
            else if(stats.stamina < 100)
                stats.stamina += stats.staminaGainDrainSpeed.x * Time.deltaTime;
        }

        if(stats.stamina <= 0)
        {
            stats.stamina = 0;

            controller.movementSettings.CanRun = controller.movementSettings.CanJump = false;
        }
        else
        {
            controller.movementSettings.CanRun = true;

            // Prevent Player from jumping until stamina is at least 15
            if(stats.stamina > 15)
                controller.movementSettings.CanJump = true;
        }

        stats.stamina = Mathf.Clamp(stats.stamina, 0, 100);
    }

    private void CalculateEnergy()
    {
        stats.energy -= stats.energyDrainSpeed * Time.deltaTime;
        stats.energy = Mathf.Clamp(stats.energy, 0, 100);
    }

    private void CalculateWater()
    {
        stats.water -= stats.waterDrainSpeed * Time.deltaTime;
        stats.water = Mathf.Clamp(stats.water, 0, 100);
    }

    /// <summary>
    /// Some actions the player performs reveals clues about the world. The key passed in must match an existing entry in Clues.clues.
    /// </summary>
    public void AddClue(string clueKey)
    {
        string val = "";

        if(!Clues.clues.TryGetValue(clueKey, out val))
            return;

        activeClues.Add(clueKey, val);
    }

    /// <summary>
    /// When the player performs the task a given Clue refers to, that clue entry is removed from their activeClues Dictionary.
    /// If player performs the task before gaining the clue in their activeClues, the Clue is removed from Clues.clues so that they should not gain that clue in their activeClues
    /// (or else there would be no way to remove that clue).
    /// </summary>
    /// <param name="clueKey"></param>
    public void UseClue(string clueKey)
    {
        if(activeClues.ContainsKey(clueKey))
            activeClues.Remove(clueKey);
        else
            Clues.clues.Remove(clueKey);
    }
}
