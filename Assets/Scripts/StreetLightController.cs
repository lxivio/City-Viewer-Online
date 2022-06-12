using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class StreetLightController : MonoBehaviour
{
    public IntersectionCollider iC;
    public SignStopLightView sSLV;
    public StreetLightModel sLM;
    public Allow allowPassage = Allow.None;
    public Allow[] posPassAllowed = new Allow[2];
    public Allow[] enterFromAllowed = new Allow[2];
    [Tooltip("delay to switch the lights")]
    private Vector2 delayMinMax = new Vector2(4.0F,10.0F);
    private float yellowLightDelay = 2.0F;
    private Color[] sLColors = new Color[4];
    public Material bulbMat;
    private CubeCity cubeCity;
    private float timeOfLastSwitch = float.PositiveInfinity;
    private float waitingDur = 0.0F;
    private int carsWaitingHere = 0;
    private int flipAxis = 0;
    string[] posPassAllowedS = new string[2];
    public List<string> blocked = new List<string>();
    public bool freewayEntrance = false;

    public void SetUp(IntersectionCollider setIC, SignStopLightView setSSLV, StreetLightModel setSLM, Color[] setSLColors, Material setBulbMat, CubeCity setCubeCity, bool[] flip, List<string> setBlocked)
    {
        for (int i = 0; i < setBlocked.Count; i++)
        {
            blocked.Add(setBlocked[i]);
        }
        iC = setIC;
        iC.myController = this;
        sSLV = setSSLV;
        sLM = setSLM;
        sLColors = setSLColors;
        bulbMat = setBulbMat;
        cubeCity = setCubeCity;


        if (sLM.iD == 17)
        {
            freewayEntrance = true;
        }


        posPassAllowed[0] = Allow.East;
        posPassAllowed[1] = Allow.North;


        if (flip[0])
        {
            //  Debug.Log("flip 0");
            posPassAllowed[0] = Allow.West;
            setSSLV.xLights[0].transform.parent.localEulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
        }

        if (flip[1])
        {
            //Debug.Log("flip 1");
            posPassAllowed[1] = Allow.South;
            setSSLV.zLights[0].transform.parent.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        }

        // actual drive in entrances
        enterFromAllowed[0] = posPassAllowed[0];
        enterFromAllowed[1] = posPassAllowed[1];

        // removed blocked from options
        if (isBlocked(posPassAllowed[0].ToString()))
        {
            posPassAllowed[0] = posPassAllowed[1];
        }
        else if (isBlocked(posPassAllowed[1].ToString()))
        {
            posPassAllowed[1] = posPassAllowed[0];

        }

        posPassAllowedS = new string[2];
        posPassAllowedS[0] = posPassAllowed[0].ToString();
        posPassAllowedS[1] = posPassAllowed[1].ToString();

        allowPassage = enterFromAllowed[0];
        float roll = UnityEngine.Random.Range(0, 2);
        if (roll == 1)
        {
            allowPassage = enterFromAllowed[1];
        }


        for (int i = 0; i < 3; i++)
        {
            SetLightBulbColor(Allow.North, i, 0);
            SetLightBulbColor(Allow.West, i, 0);
        }
        if (allowPassage == Allow.North || allowPassage == Allow.South)
        {
            SetLightBulbColor(Allow.North, 2, 3);
            SetLightBulbColor(Allow.West, 0, 1);
        }
        else
        {
            SetLightBulbColor(Allow.West, 2, 3);
            SetLightBulbColor(Allow.North, 0, 1);
        }



        float setWait = UnityEngine.Random.Range(delayMinMax.x, delayMinMax.y);
        waitingDur = setWait;
        timeOfLastSwitch = Time.time;
        StartCoroutine(WaitToSwitchLightsAgain(setWait));
    }
    bool isBlocked(string desc)
    {
        for (int i = 0; i < blocked.Count; i++)
        {
            if (desc == blocked[i])
            {
                return true;
            }
        }
        return false;
    }
    public enum Allow { None, North, East, South, West };
    public IEnumerator SwitchLights()
    {
        yield return 0;

        if (enterFromAllowed[0] != enterFromAllowed[1])
        {
            Allow setA = enterFromAllowed[1];
            if (allowPassage == enterFromAllowed[0])
            {
                setA = enterFromAllowed[1];
            }
            else
            {
                setA = enterFromAllowed[0];
            }

            if (setA == Allow.West || setA == Allow.East)
            {
                SetLightBulbColor(Allow.North, 2, 0);
                SetLightBulbColor(Allow.North, 1, 2);
                yield return new WaitForSeconds(yellowLightDelay);
                SetLightBulbColor(Allow.North, 1, 0);
                SetLightBulbColor(Allow.North, 0, 1);
                SetLightBulbColor(Allow.West, 0, 0);
                SetLightBulbColor(Allow.West, 2, 3);
            }
            else
            {
                SetLightBulbColor(Allow.West, 2, 0);
                SetLightBulbColor(Allow.West, 1, 2);
                yield return new WaitForSeconds(yellowLightDelay);
                SetLightBulbColor(Allow.West, 1, 0);
                SetLightBulbColor(Allow.West, 0, 1);
                SetLightBulbColor(Allow.North, 0, 0);
                SetLightBulbColor(Allow.North, 2, 3);
            }

            allowPassage = setA;
            carsWaitingHere = 0;
            float setWait = UnityEngine.Random.Range(delayMinMax.x, delayMinMax.y);
            waitingDur = setWait;
            timeOfLastSwitch = Time.time;
            StartCoroutine(WaitToSwitchLightsAgain(setWait));
        }
    }
    private IEnumerator WaitToSwitchLightsAgain(float thisWait)
    {
        yield return new WaitForSeconds(thisWait);
        StartCoroutine("SwitchLights");
    }
    private void SetLightBulbColor(Allow allowAxis, int setBulb, int setColor)
    {
        Material instanceMat = new Material(bulbMat);
        instanceMat.color = sLColors[setColor];
        if (allowAxis == Allow.West || allowAxis == Allow.East)
        {
            sSLV.xLights[setBulb].transform.GetComponent<MeshRenderer>().material = instanceMat;
        }
        else
        {
            sSLV.zLights[setBulb].transform.GetComponent<MeshRenderer>().material = instanceMat;
        }
    }
    public void CarApproaching(string carName)
    {
        if (carName.Substring(0, 3) == "car")
        {
            string travelOK = allowPassage.ToString();
            int carID = int.Parse(carName.Substring(4));
            //float possibleWaitDur = waitingDur - (Time.time - timeOfLastSwitch) + yellowLightDelay + carsWaitingHere * 1.2f;
            float possibleWaitDur = waitingDur - (Time.time - timeOfLastSwitch) + yellowLightDelay;

            cubeCity.carControllers[carID].ApproachIntersection(sLM.iD, travelOK, possibleWaitDur, posPassAllowedS, new Vector3(transform.position.x, 0.05f, transform.position.z), freewayEntrance);
        }
        carsWaitingHere++;
    }
}