using System;
using System.Collections;
using System.Collections.Generic;
using iXRLib;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;
using UnityEngine.Android;

public class LevelManager : MonoBehaviour
{
    public Dropper dropper;
    public AudioSource successAudioSource;
    public AudioSource failureAudioSource;
    public AudioSource victoryAudioSource;
    public double score;
    private int totalTargets;
    private int completedTargets;
    private float startTime; // New variable to track start time

    void Start()
    {
        iXRLog.Info("Content started (LevelManager)");
        //iXRLog.EventLevelStart("1", "scriptName=LevelManager");
        iXRLog.Event("level_start", "level=1,scriptName=LevelManager");

        InitializeGame();
    }

    private void InitializeGame()
    {
        totalTargets = FindObjectsOfType<TargetLocation>().Length;
        completedTargets = 0;
        score = 0;
        startTime = Time.time; // Initialize start time when the game begins
    }

    public void CompleteTask(TargetLocation.CompletionData completionData)
    {
        iXRLog.Info("Placement Attempted");
        Debug.Log("iXRLib - Placement Attempted");

        if (completionData.usedType != completionData.targetType)
        {
            dropper.Replace(completionData.targetType, completionData.usedType);
            completionData.usedTarget.GetComponent<MeshFilter>().sharedMesh = completionData.usedObject.GetComponent<MeshFilter>().sharedMesh;
            iXRLog.Event("task_failed", $"fruit={completionData.usedType}");
            //iXRLog.Event("Placement Failed", $"fruit={completionData.usedType}", obj);
            failureAudioSource.Play();

            StartCoroutine(RestartAfterFailSound());
        }
        else
        {
            iXRLog.Event("task_completed", $"fruit={completionData.usedType}");
            successAudioSource.Play();
            // Increment completed targets and check for victory
            completedTargets++;
            CheckForVictory();
        }

        completionData.usedObject.GetComponent<XRGrabInteractable>().colliders.Clear();

        // Disable the collision box of the usedTarget
        Collider targetCollider = completionData.usedTarget.GetComponent<Collider>();
        if (targetCollider != null)
        {
            targetCollider.enabled = false;
        }

        completionData.usedTarget.GetComponent<MeshRenderer>().materials = GrabbableObjectManager.getInstance().getGrabbableObjectData(completionData.usedType).model.GetComponent<MeshRenderer>().sharedMaterials;

        //iXRLog.Event("Did something cool", "key=val,key2=val",completionData.usedTarget.GetComponent<TargetLocation>());
        //iXRLog.Event("Did something cool", "key=val,key2=val");

        Destroy(completionData.usedObject);
        Destroy(completionData.usedTarget.GetComponent<Outline>());
        Destroy(completionData.usedTarget.GetComponent<TargetLocation>());

        // Calculate Score - later this should be moved out of level manager into its own score manager class that is persistant
        score += (1 / completionData.positionDistance) > 5 ? 5 : 1 / completionData.positionDistance;
        score += 1 - completionData.rotationDistance;
        score *= completionData.targetType == completionData.usedType ? 1 : .25;
    }

    private void CheckForVictory()
    {
        if (completedTargets >= totalTargets)
        {
            float elapsedTime = Time.time - startTime; // Calculate elapsed time
            //iXRLog.EventLevelComplete("1", $"{score}", $"{elapsedTime}", "victory=true");
            iXRLog.Event("level_complete", $"level=1,score={score},duration={elapsedTime},victory=true");

            PlayVictorySound();
            // You can add more victory actions here, like showing a UI panel, etc.
        }
    }

    private void PlayVictorySound()
    {
        if (victoryAudioSource != null && !victoryAudioSource.isPlaying)
        {
            victoryAudioSource.Play();
            Debug.Log("Level Completed! Victory!");

            StartCoroutine(RestartAfterVictorySound());
        }
    }

    private IEnumerator RestartAfterVictorySound()
    {
        // Wait for the victory sound to finish playing
        yield return new WaitForSeconds(victoryAudioSource.clip.length);
        RestartExperience();
    }

    private IEnumerator RestartAfterFailSound()
    {
        // Wait for the victory sound to finish playing
        yield return new WaitForSeconds(failureAudioSource.clip.length);
        RestartExperience();
    }

    private void RestartExperience()
    {
        InitializeGame();
        ReinitializeComponents();
    }

    private void ReinitializeComponents()
    {
        try
        {
            // Reset the Dropper
            if (dropper != null)
            {
                dropper.ResetDropper();
                TargetLocation[] targetLocations = FindObjectsOfType<TargetLocation>();
                foreach (TargetLocation targetLocation in targetLocations)
                {
                    dropper.Add(targetLocation.targetType);
                }
            }
            else
            {
                Debug.LogWarning("Dropper not found during reinitialization");
            }

            // Reinitialize GrabbableObjectManager
            GrabbableObjectManager.getInstance().Start();

            // Reset all TargetLocations
            TargetLocation[] allTargetLocations = FindObjectsOfType<TargetLocation>();
            foreach (TargetLocation targetLocation in allTargetLocations)
            {
                targetLocation.ResetState();
            }

            // Re-enable all XR Interactors and Interactables
            XRBaseInteractor[] interactors = FindObjectsOfType<XRBaseInteractor>();
            foreach (XRBaseInteractor interactor in interactors)
            {
                interactor.enabled = false;
                interactor.enabled = true;
            }

            XRBaseInteractable[] interactables = FindObjectsOfType<XRBaseInteractable>();
            foreach (XRBaseInteractable interactable in interactables)
            {
                interactable.enabled = false;
                interactable.enabled = true;
            }

            // Reset the player's position if necessary
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = Vector3.zero; // Or your desired starting position
                player.transform.rotation = Quaternion.identity;
            }

            // Reinitialize audio sources
            if (successAudioSource != null) successAudioSource.Stop();
            if (failureAudioSource != null) failureAudioSource.Stop();
            if (victoryAudioSource != null) victoryAudioSource.Stop();

            // Clear any ongoing coroutines
            StopAllCoroutines();

            // Ensure all necessary GameObjects are active
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.CompareTag("ResetOnPause"))
                {
                    obj.SetActive(true);
                }
            }

            // Log the reinitialization
            Debug.Log("Game components reinitialized successfully");
            iXRLog.Info("Game components reinitialized successfully");
        }
        catch (Exception e)
        {
            Debug.LogError("Error during ReinitializeComponents: " + e.Message);
            iXRLog.Info("Error during ReinitializeComponents: " + e.Message);
        }
    }

    public class LifecycleManager : MonoBehaviour
    {
        private LevelManager levelManager;

        void Start()
        {
            levelManager = FindObjectOfType<LevelManager>();
        }

        void OnApplicationFocus(bool hasFocus)
        {
            Debug.Log("Focus changed: " + hasFocus);
            iXRLog.Info("Focus changed: " + hasFocus);
        }

        void OnApplicationQuit()
        {
            Debug.Log("App is quitting");
            iXRLog.Info("App is quitting");
            // Perform any cleanup here
        }
    }
}
