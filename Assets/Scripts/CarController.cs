using UnityEngine;
using PathCreation;
using PathCreation.Examples;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class CarController : MonoBehaviour
{
    public int iD;
    public GameObject car;
    public CarModel carModel;
    public PathCreator carPC;
    public bool crossingIntersection = false;
    public bool waitingIntersection = false;
    public bool lerpingPos = false;
    public bool pathControlled = true;
    public float lSpeed = 1.0f;
    public string[] posPassS = new string[2];

    public BezierPath freewayEntrancePath;


    /// <summary>
    /// set between 0 - 1
    /// </summary>
    public float[] moveDir = new float[]{
        1.0f,0.0f
    };


    Vector3 freewayExitPos = new Vector3(0.32f, 0.05f, -20.0f);
    Quaternion normalCarQ = Quaternion.identity;

    public void SetUp(int setID, GameObject setCar, CarModel setModel, PathCreator setPC, BezierPath setFreeEntr)
    {
        normalCarQ.eulerAngles = new Vector3(0.0f, 90.0f, -90.0f);

        iD = setID;
        car = setCar;
        carModel = setModel;
        carPC = setPC;

        CarFrontCollider carFrontCol = car.transform.Find("InFrontDet").GetComponent<CarFrontCollider>();
        carFrontCol.SetUp(this);

        lSpeed = carModel.speed;
        freewayEntrancePath = setFreeEntr;

    }
    public void ApproachIntersection(int intersectionID, string travelOK, float possibleWait, string[] setPosPassS, Vector3 interPos, bool freewayEntrance)
    {

        if (!crossingIntersection && !waitingIntersection)
        {

            posPassS = setPosPassS;
            float xDiff = car.transform.position.x - car.transform.GetChild(0).transform.position.x;
            float zDiff = car.transform.position.z - car.transform.GetChild(0).transform.position.z;
            string carDir = "";

            if (Math.Abs(zDiff) < Math.Abs(xDiff))
            {
                if (xDiff < 0)
                {
                    carDir = "east";
                }
                else
                {
                    carDir = "west";
                }
            }
            else
            {
                if (zDiff < 0)
                {
                    carDir = "north";
                }
                else
                {
                    carDir = "south";
                }

            }
            if (iD == 0)
            {
                Debug.Log("approach intersection (" + intersectionID + ") car (" + iD + ") carDir " + carDir + " travelOK " + travelOK + " intersec id " + intersectionID + " inter pos " + interPos + " posPassS[0] " + posPassS[0] + " posPassS[1] " + posPassS[1]);
            }
            /*
            bool mustWait = true;
            if (travelOK == "NorthWest")
            {
                if (carDir == "north" || carDir == "west")
                {
                    mustWait = false;
                }
            }
            else
            {
                if (carDir == "south" || carDir == "east")
                {
                    mustWait = false;
                }
            }

            if (mustWait)
            {
                waitingIntersection = true;
                ChangeSpeed(0.0f);
                StartCoroutine(SlowDown(possibleWait));
            }
            else
            {*/
            StartCoroutine(CrossIntersection(interPos, freewayEntrance));
            //}
            // Debug.Log("car approaching intersection: " + carDir + " travelOK " + travelOK);
        }
    }
    public static string FlipDir(string thisDir)
    {
        if (thisDir == "North")
        {
            thisDir = "South";
        }
        else if (thisDir == "South")
        {
            thisDir = "North";
        }
        else if (thisDir == "East")
        {
            thisDir = "West";
        }
        else if (thisDir == "West")
        {
            thisDir = "East";
        }
        return thisDir;
    }
    private IEnumerator CrossIntersection(Vector3 interPos, bool freewayEntrance)
    {
        bool enterFreeway = false;
        if (freewayEntrance)
        {
            int rollForFreeway = UnityEngine.Random.Range(0, 2);
            if (rollForFreeway == 1)
            {
                enterFreeway = true;
                
            }
            yield return 0;
        }

            int rollForDir = UnityEngine.Random.Range(0, 2);
            // decide direction (opposite of passage dir)
            //Debug.Log("car(" + iD + ") posPassS[rollForDir] " + posPassS[rollForDir]);
            Vector3 exitPos = new Vector3();
            float[] newDir = new float[2];
            float[] setDir = getDirFromCDir(posPassS[rollForDir]);
        if (enterFreeway)
        {
            setDir = new float[] { 1,0 };
        }
            bool turning = false;
            bool turnLeft = true;
            if (moveDir[0] != setDir[0] || moveDir[1] != setDir[1])
            {
                turning = true;

                if (moveDir[0] == 1 && setDir[1] == -1)
                {
                    turnLeft = false;
                }
                else if (moveDir[0] == -1 && setDir[1] == 1)
                {
                    turnLeft = false;
                }
                else if (moveDir[1] == 1 && setDir[0] == 1)
                {
                    turnLeft = false;
                }
                else if (moveDir[1] == -1 && setDir[0] == -1)
                {
                    turnLeft = false;
                }


                exitPos = exitIntersectionPos(setDir, interPos);

            }

            waitingIntersection = false;
            crossingIntersection = true;

            yield return new WaitForSeconds(1.0f / carModel.speed);
            if (turning)
                ChangeSpeed(carModel.speed / 5.0f);
            yield return new WaitForSeconds(0.3f / carModel.speed);
            if (turning)
            {
                if (turnLeft)
                {
                    LerpRotFromTo(new Vector3(90.0f, 0.0f, 0.0f));
                }
                else
                {
                    LerpRotFromTo(new Vector3(-90.0f, 0.0f, 0.0f));
                }
               // Debug.Log("turn left: " + turnLeft);
            }
            yield return new WaitForSeconds(0.7f / carModel.speed);
            if (turning)
            {
                moveDir[0] = setDir[0];
                moveDir[1] = setDir[1];

                LerpMoveFromTo(car.transform.position, exitPos);

            }
            yield return new WaitForSeconds(2.0f / carModel.speed);
            if (turning)
                ChangeSpeed(carModel.speed);
            yield return new WaitForSeconds(2.0f / carModel.speed);
            //car.transform.Find("CarBody1").gameObject.SetActive(true);

            crossingIntersection = false;
            if (enterFreeway)
            {
                EnterFreeway();
            }
        }
  
    private IEnumerator SlowDown(float waitAfter)
    {
        yield return new WaitForSeconds(waitAfter);
        StartCoroutine("CrossIntersection");
    }
    public void BlockedInFront()
    {
        if (!crossingIntersection && !waitingIntersection)
        {
            ChangeSpeed(0.0f);
            StopCoroutine("CheckUnblocked");
            StartCoroutine("CheckUnblocked");
        }
    }
    private static float[] getDirFromCDir(string cDir)
    {
        float[] dir = new float[2];
        if (cDir == "North")
        {
            dir[0] = 0.0f;
            dir[1] = 1.0f;
        }
        else if (cDir == "South")
        {
            dir[0] = 0.0f;
            dir[1] = -1.0f;
        }
        else if (cDir == "East")
        {
            dir[0] = 1.0f;
            dir[1] = 0.0f;
        }
        else if (cDir == "West")
        {
            dir[0] = -1.0f;
            dir[1] = 0.0f;
        }

        return dir;
    }
    private IEnumerator CheckUnblocked()
    {
        yield return new WaitForSeconds(0.2f);
        ChangeSpeed(carModel.speed);
    }
    private IEnumerator changeSpeedCR;
    private bool changeSpeedCRRunning = false;

    private void ChangeSpeed(float targetSpeed)
    {
        //Debug.Log("changing car(" + carModel.iD + ") speed to: " + targetSpeed + " max speed is: " + carModel.speed);
        if (changeSpeedCRRunning)
        {
            StopCoroutine(changeSpeedCR);
        }
        changeSpeedCR = ChangeSpeedIE(targetSpeed);
        StartCoroutine(changeSpeedCR);
    }
    private float speedThresh = 0.5f;
    private float speedInc = 2.0f;
    private IEnumerator ChangeSpeedIE(float targetSpeed)
    {
        // Debug.Log("change speed to: " + targetSpeed);
        changeSpeedCRRunning = true;
        bool updated = true;
        float newSpeed = car.GetComponent<PathFollower>().speed;
        while (updated)
        {
            updated = false;
            if (newSpeed < targetSpeed - speedThresh)
            {
                newSpeed += speedInc*Time.deltaTime;
                updated = true;
            }
            else if (newSpeed > targetSpeed + speedThresh)
            {
                newSpeed -= speedInc * Time.deltaTime * 2.0f;
                updated = true;
            }
            newSpeed = Mathf.Clamp(newSpeed, 0.0f, carModel.speed);
            car.GetComponent<PathFollower>().speed = newSpeed;
            lSpeed = newSpeed;
            yield return 0;
        }
        car.GetComponent<PathFollower>().speed = targetSpeed;
        lSpeed = targetSpeed;
        changeSpeedCRRunning = false;
    }
    Vector3 exitStartPos;
    Quaternion exitStartQ;
    public void ExitFreeway()
    {
        car.transform.localRotation = normalCarQ;
        car.GetComponent<PathFollower>().enabled = false;
        pathControlled = false;

        //exitStartPos = car.transform.position;
        //exitStartQ = car.transform.rotation;
        //LerpMoveFromTo(exitStartPos, freewayExitPos);
        //car.transform.localRotation = Quaternion.Lerp(exitStartQ, normalCarQ, p);
        
    }
    public void EnterFreeway()
    {
         crossingIntersection = false;
    waitingIntersection = false;
    lerpingPos = false;
    car.GetComponent<PathFollower>().enabled = true;


        car.GetComponent<PathFollower>().distanceTravelled = 0;
        car.GetComponent<PathFollower>().onFreeway = true;
        //car.GetComponent<PathCreator>().bezierPath = freewayEntrancePath;
        pathControlled = true;
    }


    IEnumerator moveCR;
    void LerpMoveFromTo(Vector3 from, Vector3 to)
    {

        if (moveCR != null)
        {
            StopCoroutine(moveCR);
        }
        float dist = Vector3.Distance(from, to);
        moveCR = LerpMoveFromToCR(dist, from, to);
        StartCoroutine(moveCR);
    }
    IEnumerator rotCR;
    void LerpRotFromTo(Vector3 rotPush)
    {

        if (rotCR != null)
        {
            StopCoroutine(rotCR);
        }
        float dist = 1.0f;
        rotCR = LerpRotFromToCR(rotPush);
        StartCoroutine(rotCR);
    }
    public bool lerpingRot = false;
    private IEnumerator LerpRotFromToCR(Vector3 rotPush)
    {
        Quaternion ogQ = car.transform.localRotation;
        lerpingRot = true;
        float p = 0.0f;
        float damp = 0.1f;
        while (p < 1.0f)
        {
            float incrPush = Time.deltaTime * carModel.speed * damp;
            if (p < 0.3f)
            {
                damp += Time.deltaTime;
                if (damp > 1.0f)
                {
                    damp = 1.0f;
                }
            }
            else if (p > 0.7f)
            {
                damp -= Time.deltaTime;
                if (damp < 0.1f)
                {
                    damp = 0.1f;
                }
            }
            p += incrPush;
            if (p > 1.0f)
            {
                p = 1.0f;
            }
            car.transform.Rotate(new Vector3(rotPush.x*incrPush, rotPush.y, rotPush.z));
            yield return 0;
        }
        car.transform.localRotation = ogQ;
        car.transform.Rotate(new Vector3(rotPush.x, rotPush.y, rotPush.z));
        lerpingRot = false;
    }
    private IEnumerator LerpMoveFromToCR(float dist, Vector3 from, Vector3 to)
    {
        lerpingPos = true;
        float dur = 1.0f / (dist / carModel.speed);
        float p = 0.0f;
        while (p < 1.0f)
        {
            p += Time.deltaTime * dur;
            if (p > 1.0f)
            {
                p = 1.0f;
            }
            car.transform.position = Vector3.Lerp(from, to, p);

                

            yield return 0;
        }

        lerpingPos = false;
    }
    public void PossiblyMove()
    {
        if (!pathControlled && !lerpingPos)
        {
            car.transform.position = new Vector3(car.transform.position.x + lSpeed * Time.deltaTime * moveDir[0], car.transform.position.y, car.transform.position.z + lSpeed * Time.deltaTime * moveDir[1]);
        }
    }
    private Vector3 exitIntersectionPos(float[] moveDir, Vector3 interPos) {
        float distToEscapeInter = 1.5f;
        Vector3 exitPos = new Vector3(interPos.x + moveDir[0] * distToEscapeInter, 0.05f, interPos.z + moveDir[1] * distToEscapeInter);
        return exitPos;
    }
    


    
}
