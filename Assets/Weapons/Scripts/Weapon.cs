using UnityEngine;
using System.Collections.Generic;

/**
 * Weapon activate / deactivate script.
 * 
 * The blade on/off algorithm works by setting the scale of the blade. Therefore the pivot must be at the bottom of the blade. 
 * You can adapt that easily in Blender or simply use the provided model.
 * The scale is applied in z direction.
 * Use space bar to toggle the blade on and off. 
 * 
 * Notes:
 * + light and blade color are initialized using the blade color property, the colors will be overwritten
 * 
 */
public class Weapon : MonoBehaviour {

    [Tooltip("A list of blade game objects, each of them can have a light as child. The light's intensity depends on the blade length.")]
    public List<GameObject> bladeGameObjects;

    [Tooltip("Blade extend speed in seconds.")]
    public float bladeExtendSpeed = 0.1f;

    [Tooltip("Whether the weapon (and all of its blades) is active initially or not.")]
    public bool weaponActive = false;

    [Tooltip("The blade and light color. Will be propagated to the shader and the blade light.")]
    public Color bladeColor;
	
    [Tooltip("The intensity of the blade color, used to create the HDR color which gives the inner white and outer color glow.")]
	public float bladeColorIntensity = 600f; 

    [Tooltip("The intensity of the light the blade emits to the environment. This depends on the light settings (e. g. lum).")]
	public float lightIntensity = 1000f;
	
    public AudioClip soundOn;
    public AudioClip soundOff;
    public AudioClip soundLoop;
    public AudioClip soundSwing;

    public AudioSource AudioSource;
    public AudioSource AudioSourceLoop;
    public AudioSource AudioSourceSwing;
    

    // swinging: used for later, but should work currently anyway
    private float swingSpeed = 0;
    private Vector3 lastSwingPosition = Vector3.zero;

    /// <summary>
    /// The key for toggling the weapon's active state
    /// </summary>
    private KeyCode TOGGLE_KEY_CODE = KeyCode.Space;

    /// <summary>
    /// The color property in the shader. This will receive the color set via this script.
    /// </summary>
    private const string SHADER_PROPERTY_EMISSION_COLOR = "_EmissionColor";

    /// <summary>
    /// Properties of a single blade.
    /// This way you can attach multiple blades to a weapon
    /// </summary>
    private class Blade
    {
        // the blade itself
        public GameObject gameObject;

        // the light attached to the blade
        public Light light;

        // minimum blade length
        private float scaleMin;

        // maximum blade length; initialized with length from scene
        private float scaleMax;

        // current scale, lerped between min and max scale
        private float scaleCurrent;

        public bool active = false;

        // the delta is a lerp value within 1 second. it will be initialized depending on the extend speed
        private float extendDelta;

        private float localScaleX;
        private float localScaleZ;

        public Blade( GameObject gameObject, float extendSpeed, bool active)
        {

            this.gameObject = gameObject;
            this.light = gameObject.GetComponentInChildren<Light>();
            this.active = active;

            // consistency check
            /*
            if (light == null)
            {
                Debug.Log("No light found. Blade should have a light as child");
            }
            */

            // remember initial scale values (non extending part of the blade)
            this.localScaleX = gameObject.transform.localScale.x;
            this.localScaleZ = gameObject.transform.localScale.z;

            // remember initial scale values (extending part of the blade)
            this.scaleMin = 0f;
            this.scaleMax = gameObject.transform.localScale.y;

            // initialize variables
            // the delta is a lerp value within 1 second. depending on the extend speed we have to size it accordingly
            extendDelta = this.scaleMax / extendSpeed;

            if (active)
            {
                // set blade size to maximum
                scaleCurrent = scaleMax;
                extendDelta *= 1;
            }
            else
            {
                // set blade size to minimum
                scaleCurrent = scaleMin;
                extendDelta *= -1;
            }

        }

        public void SetActive( bool active)
        {
            // whether to scale in positive or negative direction
            extendDelta = active ? Mathf.Abs(extendDelta) : -Mathf.Abs(extendDelta);

        }

        public void SetColor( Color color, float intensity)
        {
            if (light != null)
            {
                light.color = color;
            }
			
			Color bladeColor = color * intensity;

			// set the color in the shader. _EmissionColor is a reference which is defined in the property of the graph
            gameObject.GetComponentInChildren<MeshRenderer>().materials[0].SetColor( SHADER_PROPERTY_EMISSION_COLOR, bladeColor);

        }

        public void updateLight( float lightIntensity)
        {
            if (this.light == null)
                return;

            // light intensity depending on blade size
            this.light.intensity = this.scaleCurrent * lightIntensity;
        }

        public void updateSize()
        {

            // consider delta time with blade extension
            scaleCurrent += extendDelta * Time.deltaTime;

            // clamp blade size
            scaleCurrent = Mathf.Clamp(scaleCurrent, scaleMin, scaleMax);

            // scale in z direction
            gameObject.transform.localScale = new Vector3(this.localScaleX, scaleCurrent, this.localScaleZ);

            // whether the blade is active or not
            active = scaleCurrent > 0;
             
            // show / hide the gameobject depending on the blade active state
            if (active && !gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            else if(!active && gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
            
        }
    }

    private List<Blade> blades;

    // Use this for initialization
    void Awake () {

        // consistency check
        if (bladeGameObjects.Count == 0) {
            Debug.LogError("No blades found. Must have at least 1 blade!");
        }

        // store initial attributes
        blades = new List<Blade>();
        foreach (GameObject bladeGameObject in bladeGameObjects)
        {
            blades.Add(new Blade(bladeGameObject, bladeExtendSpeed, weaponActive));
        }


        // initialize audio depending on beam activitiy
        InitializeAudio();

        // light and blade color
        InitializeBladeColor();

        // initially update blade length, so that it isn't set to what we have in unity's visual editor
        UpdateBlades();


    }

    void InitializeAudio()
    {

        // initialize audio depending on beam activitiy
        if (weaponActive)
        {
            AudioSourceLoop.clip = soundLoop;
            AudioSourceLoop.Play();
        }

    }

    // set the color of the light and the blade
    void InitializeBladeColor()
    {
        // update blade color, light color and glow color
        foreach (Blade blade in blades)
        {
            blade.SetColor(bladeColor, bladeColorIntensity);
        }
    }
	
	// Update is called once per frame
	void Update () {
        
        // key pressed
        if (Input.GetKeyDown(TOGGLE_KEY_CODE))
        {

            ToggleWeaponOnOff();

        }

        UpdateBlades();


        // light and blade color
        // only for testing dynamic colors, works.
        // UpdateColor();

        // swing speed
        updateSwingHandler();


    }

    // calculate swing speed
    private void updateSwingHandler()
    {
        // calculate speed
        swingSpeed = (((transform.position - lastSwingPosition).magnitude) / Time.deltaTime);

        // remember last position
        lastSwingPosition = transform.position;

        // swing sound
        // a probably better solution would be to play the swing sound permanently and only fade the volume in and out depending on the swing speed
        if (weaponActive)
        {
            // if certain swing speed is reached, play swing audio sound. if swinging stopped, fade the volume out
            if (swingSpeed > 0.8) // just random swing values; should be more generic
            {
                if (!AudioSourceSwing.isPlaying)
                {
                    AudioSourceSwing.volume = 1f;
                    AudioSourceSwing.PlayOneShot(soundSwing);
                }
            }
            else
            {

                // fade out volume
                if(AudioSourceSwing.isPlaying && AudioSourceSwing.volume > 0)
                {
                    AudioSourceSwing.volume *= 0.9f; // just random swing values; should be more generic
                }
                else
                {
                    AudioSourceSwing.volume = 0;
                    AudioSourceSwing.Stop();
                }

            }
        }
    }

    private void ToggleWeaponOnOff()
    {
        if (weaponActive)
        {
            WeaponOff();
        }
        else
        {
            WeaponOn();
        }

    }

    private void WeaponOn()
    {
        foreach (Blade blade in blades)
        {
            blade.SetActive(true);
        }
            

        AudioSource.PlayOneShot(soundOn);
        AudioSourceLoop.clip = soundLoop;
        AudioSourceLoop.Play();

    }

    private void WeaponOff()
    {
        foreach (Blade blade in blades)
        {
            blade.SetActive(false);
        }

        AudioSource.PlayOneShot(soundOff);
        AudioSourceLoop.Stop();

    }

    private void UpdateBlades()
    {
        foreach (Blade blade in blades)
        {

            blade.updateLight( lightIntensity);
            blade.updateSize();

        }

        // weapon is active if any of the blades is active
        bool active = false;
        foreach (Blade blade in blades)
        {
            if( blade.active)
            {
                active = true;
                break;
            }
           
        }

        this.weaponActive = active;
    }

}
