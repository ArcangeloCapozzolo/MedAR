using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainLogic : MonoBehaviour
{
    private bool debug = true;

    public GameObject sphereA;
    public GameObject sphereB;
    public GameObject sphereC;
    private GameObject[] spheres = new GameObject[3];
    private bool[] sphereDone = {false, false, false};

    public Material unselectedColor;
    public Material selectedColor;
    public Material doneColor;

    public GameObject tooltip;

    private float countsAsHitDistance = 0.02f; //range in which contact is seen as close enough to complete target
    private float currentHitTimer = 0.0f;
    private float goalHitTimer = 3.0f; //in seconds
    private float rangeForSoundAid = 0.2f; //range (around target) in which distance-sounds are played

    public Sound soundManager;

    private bool isPatientFound = false;
    private bool isToolFound = false;

    public GameObject toolIndicator;
    private float rangeForVisualAid = 0.2f; //range (around target) in which scalpel's indicator reflects distance as color

    //public GameObject displayTextHolder;
    public TMP_Text displayText;
    
    // Start is called before the first frame update
    void Start()
    {
        spheres[0] = sphereA;
        spheres[1] = sphereB;
        spheres[2] = sphereC;

        //displayText = displayTextHolder.GetComponent<>;
    }

    // Update is called once per frame
    void Update()
    {
        checkSpheres();
        if (debug)
            printAllPositions();
        checkDistances();
        updateDisplayText();
    }

    private void checkDistances()
    {
        if (!isPatientFound || ! isToolFound)
        {
            print($"MainLogic checkDistances() skipped due to loss in tracking. (isPatientFound={isPatientFound}, isToolFound={isToolFound})");
            return;
        }
        
        int closest = -1; //holds index of current "best" matching sphere, ie with closest distance to tooltip
        float closestDistance = -1.0f; //holds the actual distance of this best match

        for (int i = 0; i < 3; i++)
        {
            Vector3 offset = spheres[i].transform.position - tooltip.transform.position;
            float new_distance = offset.magnitude;
            if (!sphereDone[i] && (closestDistance == -1.0f || new_distance < closestDistance))
            {
                closest = i;
                closestDistance = new_distance;
            }
        }
        
        if (closest < 0 || closestDistance < 0)
        {
            print($"MainLogic - Sanity check failed for finding closest sphere. (clostest={closest}, closestDistance={closestDistance})");
            return;
        }

        if (debug)
            print($"MainLogic - checkDistances() chose index {closest} with distance {closestDistance}.");

        for (int i = 0; i < 3; i++)
        {
            if (closest == i)
            {
                //set selection color to indicate which sphere is currently targeted
                spheres[i].GetComponent<Renderer>().material = selectedColor;

                //play sound for in-progress sphere
                if (closestDistance <= rangeForSoundAid)
                    soundManager.playDistanceSound(i, closestDistance, debug);

                //check distance, update timer, set sphere to done if necessary
                if (closestDistance <= countsAsHitDistance)
                {
                    currentHitTimer += Time.deltaTime;
                    if (currentHitTimer >= goalHitTimer)
                    {
                        //sphere is done
                        currentHitTimer = 0.0f;
                        sphereDone[i] = true;
                        spheres[i].GetComponent<Renderer>().material = doneColor;

                        //play sound for finished sphere
                        soundManager.playFinishedSound(i);

                        print($"Sphere at index {i} is done.");
                    }
                } else {
                    //reset timer of current attempt, ie not (/no longer) close enough
                    currentHitTimer = 0.0f;
                }
            } else {
                //reset color of other spheres, unless they are already finished
                if (!sphereDone[i])
                    spheres[i].GetComponent<Renderer>().material = unselectedColor;
            }
        }

        updateVisualIndicator(closestDistance);
    }

    private void updateVisualIndicator(float closestDistance)
    {
        //check if all spheres are done, if so, set indicator to green
        if (sphereDone[0] && sphereDone[1] && sphereDone[2])
        {
            toolIndicator.GetComponent<Renderer>().material.color = new Color(0, 1, 0);
            return;
        }

        //normal distance indicator
        if (closestDistance <= rangeForVisualAid)
        {
            // increasingly bright pink with closer distance, starting from black ie 0 0 0 at closestDistance == rangeForVisualAid
            float redBlueStrength = (rangeForVisualAid - closestDistance) * (1/rangeForVisualAid);
            if (debug)
                print($"MainLogic - updateVisualIndicator() is in range and chose redBlueStrength={redBlueStrength}.");
            toolIndicator.GetComponent<Renderer>().material.color = new Color(redBlueStrength, 0, redBlueStrength);
        } else {
            //reset color
            toolIndicator.GetComponent<Renderer>().material.color = new Color(0, 0, 0);
        }
    }


    private void printAllPositions()
    {
        print($"SphereA pos: {sphereA.transform.position}\tSphereB pos: {sphereB.transform.position}\tSphereC pos: {sphereC.transform.position}\nTooltip pos: {tooltip.transform.position}");
    }

    private void checkSpheres()
    {
        if(sphereA == null || sphereB == null || sphereC == null || tooltip == null)
            print($"MainLogic - Error in spheres not being null ({sphereA == null}, {sphereB == null}, {sphereC == null}, {tooltip == null}).");
        if (spheres[0] != sphereA || spheres[1] != sphereB || spheres[2] != sphereC)
            print($"MainLogic - Error in match to sphere array, separate checks (should all be false) are: {spheres[0] != sphereA}, {spheres[1] != sphereB}, {spheres[2] != sphereC}.");
        
    }

    private void updateDisplayText()
    {
        int completed = 0;
        for (int i = 0; i < sphereDone.Length; i++)
            if (sphereDone[i])
                completed++;
        displayText.text = $"Targets completed:\n{completed}/{sphereDone.Length}";
    }


    public void patientTrackingFound()
    {
        isPatientFound = true;
    }

    public void patientTrackingLost()
    {
        isPatientFound = false;
    }

    public void toolTrackingFound()
    {
        isToolFound = true;
    }

    public void toolTrackingLost()
    {
        isToolFound = false;
    }
}
