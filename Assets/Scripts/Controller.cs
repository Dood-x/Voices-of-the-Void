using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Controller : MonoBehaviour
{

    [System.Serializable]
    public struct SoundSource
    {
        public int horizontalCoordiante;
        public int verticalCoordiante;
        public float soundLevel;
    }

    public bool flipMovement = true;

    public int horizontalTicks = 12;
    public int verticalTicks = 10;

    public Slider fuelGauge; 

    public float maxFuel;
    public int noTries;

    public GameObject verticalKnob;
    public GameObject horizontalKnob;
    public GameObject confirmButton;

    public GameObject noiseSource;

    //public GameObject reticle;
    public RectTransform reticle;

    public GameObject soundGauge;

    public Sprite cursor;

    private float horizontalDegrees;
    private float verticalDegrees;

    public float imageHorizontalRes = 12.0f;
    public float imageVerticalRes = 6.0f;

    private float soundLevel;
    private float fuel;
    private float fuelDepleteAmount;

    public int soundFalloffSpaces = 5;
    private float soundFallofFAmount;

    public SoundSource[] soundSources;

    private float[,] soundMap;

    private float currentSoundLevel;

    private bool pressedButton = false;

    private AudioSource audios;

    public enum Knob {
        Vertical,
        Horizontal,
        Confirm
    }

    public struct IntPair {
        public int i;
        public int j;

        public IntPair(int a, int b)
        {
            i = a;
            j = b;
        }
    }

    public enum ReticleDir
    {
        Up,
        Down,
        Left,
        Right
    }

    private IntPair reticlePos;
    private Knob selectedKnob;

    private Vector3 retPosStart;


    [Header("Audio")]
    public AudioClip signal;
    public AudioClip noise;
    public AudioClip dialTurn;
    public AudioClip dialError;
    public AudioClip buttonPress;
    public AudioClip buttonUnpress;
    public AudioClip chooseCorrect;
    public AudioClip choseWrong;
    public AudioClip changeDial;

    // Start is called before the first frame update
    void Start()
    {
        horizontalDegrees = 360f / horizontalTicks;
        verticalDegrees = 360f / verticalTicks;
        fuel = maxFuel;
        fuelDepleteAmount = maxFuel / noTries;

        //generate the grid
        soundMap = new float[verticalTicks,horizontalTicks];

        soundFallofFAmount = 1.0f / soundFalloffSpaces;

        foreach (SoundSource s in soundSources)
        {
            AssignSoundLevelRecursion(s.verticalCoordiante, s.horizontalCoordiante, s.soundLevel);
        }


        //fade the sound by soundLevel


        reticlePos = new IntPair(0, 0);

        selectedKnob = Knob.Horizontal;

        //retPosStart = reticle.transform.position;

        SetReticleToPos(0, 0);

        audios = GetComponent<AudioSource>();
    }

    void AssignSoundLevelRecursion(int row, int collumn, float soundLevel)
    {
        if (row >= verticalTicks || collumn >= horizontalTicks || row < 0 || collumn < 0)
            return;
        
        if(soundMap[row, collumn] < soundLevel)
        {
            soundMap[row, collumn] = soundLevel;
        }


        soundLevel -= soundFallofFAmount;

        if (soundLevel <= 0.0f)
            return;

        AssignSoundLevelRecursion(row-1, collumn, soundLevel);
        AssignSoundLevelRecursion(row, collumn-1, soundLevel);
        AssignSoundLevelRecursion(row+1, collumn, soundLevel);
        AssignSoundLevelRecursion(row, collumn+1, soundLevel);
    }

    // Update is called once per frame
    void Update()
    {
        // a/d - select knobs
        // q/e - rotate knobs
        // Enter - press button

        
        

        if (selectedKnob != Knob.Confirm)
        {
            if (!TurnKnob())
                SelectKnob();

        }
        else {
            if(!PressButton())
                SelectKnob();
        }

    }

    void ResetFuel()
    {
        fuel = maxFuel;
        fuelGauge.value = fuel;
    }
    void FuelDeplete()
    {
        fuel -= fuelDepleteAmount;
        //fuelGauge.value = fuel / maxFuel;
        Debug.Log("Fuel: " + fuel);
    }
    void SetSoundLevel()
    {
        currentSoundLevel = soundMap[reticlePos.i, reticlePos.j];
        Debug.Log("Sound: " + currentSoundLevel);
        //TODO change guagues and play sound
        if (currentSoundLevel == 0)
        {
            noiseSource.GetComponent<AudioSource>().volume = 0.1f;
            reticle.GetComponent<AudioSource>().volume = 0;
        }
        else if (currentSoundLevel <= 0.1f) {
            noiseSource.GetComponent<AudioSource>().volume = 0.1f;
            reticle.GetComponent<AudioSource>().volume  = currentSoundLevel;
        }
        else
        {
            noiseSource.GetComponent<AudioSource>().volume = 1.0f - currentSoundLevel;
            reticle.GetComponent<AudioSource>().volume = currentSoundLevel;
        }
    }

    bool SelectKnob()
    {
        bool movedLeft = Input.GetKeyDown(KeyCode.A);
        bool movedRight = Input.GetKeyDown(KeyCode.D);

        switch (selectedKnob)
        {
            case Knob.Vertical:
                {
                    if (movedRight)
                    {
                        ChangeKnob(Knob.Horizontal);
                        return true;
                    }

                    break;
                }
            case Knob.Horizontal:
                {
                    if(movedRight)
                    {
                        ChangeKnob(Knob.Confirm);
                        return true;
                    }
                    if (movedLeft)
                    {
                        ChangeKnob(Knob.Vertical);
                        return true;

                    }
                    break;
                }
            case Knob.Confirm:
                {
                    if (movedLeft)
                    {
                        ChangeKnob(Knob.Horizontal);
                        return true;
                    }
                    break;
                }


        }
        return false;
    }

    bool TurnKnob()
    {
        if (selectedKnob == Knob.Confirm)
            return false;

        bool turned = false;

        bool turnedClockWise = Input.GetKeyDown(KeyCode.E);
        bool turnedAntiClockWise = Input.GetKeyDown(KeyCode.Q);

        if (selectedKnob == Knob.Horizontal)
        {
            if (turnedClockWise)
            {
                if (MoveReticle(ReticleDir.Right))
                {
                    TurnKnob(true);
                    turned = true;
                }
            }
            else if (turnedAntiClockWise)
            {
                if (MoveReticle(ReticleDir.Left))
                {
                    TurnKnob(false);
                    turned = true;
                }
            }
        }

        if (selectedKnob == Knob.Vertical)
        {
            if (turnedClockWise)
            {
                if (MoveReticle(ReticleDir.Down))
                {
                    TurnKnob(true);
                    turned = true;
                }
            }
            else if (turnedAntiClockWise)
            {
                if (MoveReticle(ReticleDir.Up))
                {
                    TurnKnob(false);
                    turned = true;
                }
            }
        }

        if (turned)
        {
            SetSoundLevel();
            FuelDeplete();

            
        }
        else
        {
            if (turnedAntiClockWise || turnedClockWise)
            {
                if (selectedKnob == Knob.Vertical)
                    verticalKnob.GetComponent<AudioSource>().PlayOneShot(dialError, 0.2f);
                if (selectedKnob == Knob.Horizontal)
                    horizontalKnob.GetComponent<AudioSource>().PlayOneShot(dialError, 0.2f);
            }
                
        }

        return turned;
    }

    void TurnKnob(bool clockWise)
    {
        float degreesToTurn = 0;
        if(selectedKnob == Knob.Horizontal)
        {
            degreesToTurn = clockWise ? horizontalDegrees : -horizontalDegrees;

            horizontalKnob.transform.Rotate(Vector3.up * degreesToTurn);

            horizontalKnob.GetComponent<AudioSource>().PlayOneShot(dialTurn);

        }
        else if (selectedKnob == Knob.Vertical)
        {
            degreesToTurn = clockWise ? verticalDegrees : -verticalDegrees;
            verticalKnob.transform.Rotate(Vector3.up * degreesToTurn);
            verticalKnob.GetComponent<AudioSource>().PlayOneShot(dialTurn);
        }



        return;
    }

    bool MoveReticle(ReticleDir dir)
    {
        IntPair oldRetPos = reticlePos;
       switch (dir)
        {
            case ReticleDir.Down:
                {
                    if(reticlePos.i > 0)
                        reticlePos.i -= 1;
                    break;
                }

            case ReticleDir.Up:
                {
                    if (reticlePos.i < verticalTicks-1)
                        reticlePos.i += 1;
                    break;
                }

            case ReticleDir.Left:
                {
                    if (reticlePos.j > 0)
                        reticlePos.j -= 1;
                    break;
                }

            case ReticleDir.Right:
                {
                    if (reticlePos.j < horizontalTicks - 1)
                        reticlePos.j += 1;
                    break;
                }
        }

        if (oldRetPos.i == reticlePos.i && oldRetPos.j == reticlePos.j)
            return false;


        Debug.Log("Ret Pos: " + reticlePos.i + "," + reticlePos.j);


        //TODO render reticle
        //float imageHorizontalSpace = imageHorizontalRes / horizontalTicks;
        // float imageVerticalSpace = imageVerticalRes / verticalTicks;

        //calculate position on image
        SetReticleToPos(reticlePos.j, reticlePos.i);

        return true;
    }

    void SetReticleToPos(int h, int v)
    {
        float canvasHorizontalPos = (imageHorizontalRes / horizontalTicks) * h /*+ (imageHorizontalRes / horizontalTicks)*/;
        float canvasVerticalPos = (imageVerticalRes / verticalTicks) * v/* + (imageVerticalRes / verticalTicks)*/;

        //reticle
        if (flipMovement)
        {
            reticle.anchoredPosition = new Vector2(-canvasHorizontalPos, canvasVerticalPos);

        }
        else
        {
            reticle.anchoredPosition = /*retPosStart +*/ new Vector2(canvasHorizontalPos, canvasVerticalPos);

        }
    }

    bool PressButton()
    {
        bool pressedConfirm = Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.C);
        if (!pressedConfirm || pressedButton)
            return false;

        bool pressedCorrect = false;
        foreach(SoundSource s in soundSources)
        {
            if(s.soundLevel == 1.0f)
            {
                if (reticlePos.j == s.horizontalCoordiante && reticlePos.i == s.verticalCoordiante)
                {
                    //RIGHT LOCATION
                    ResetFuel();
                    pressedCorrect = true;
                    confirmButton.GetComponent<AudioSource>().PlayOneShot(chooseCorrect);
                }


            }
        }
        //check if right location

        StartCoroutine("PressedButton");

        Debug.Log("PressedConfirm");

        if (!pressedCorrect)
        {
            StartCoroutine("PressedWrong");
        }
        //TODO change image


        return true;
    }

    IEnumerator PressedWrong()
    {
        yield return new WaitForSeconds(0.1f);
        confirmButton.GetComponent<AudioSource>().PlayOneShot(choseWrong);
    }


    IEnumerator PressedButton()
    {
        confirmButton.GetComponent<AudioSource>().PlayOneShot(buttonPress);
        pressedButton = true;
        confirmButton.transform.Translate(Vector3.back * 0.3f);
        yield return new WaitForSeconds(0.5f);
        confirmButton.transform.Translate(Vector3.forward * 0.3f);
        pressedButton = false;
        confirmButton.GetComponent<AudioSource>().PlayOneShot(buttonUnpress);

    }

    void ChangeKnob(Knob knob)
    {
        selectedKnob = knob;
        Debug.Log("ChangedKnob " + knob);
        GetComponent<AudioSource>().PlayOneShot(changeDial, 0.2f);
        //TODO highlight knob!
    }


}
