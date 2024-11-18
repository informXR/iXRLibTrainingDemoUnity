using System;
using System.Collections;
using System.Collections.Generic;
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
        //iXR.LogInfo("Content started (LevelManager)");
        //iXR.EventAssessmentStart("stocking_training_unit_1", "scriptName=LevelManager");
        InitializeGame();
    }

    private void InitializeGame()
    {
        completedTargets = 0;
        score = 0;
        startTime = Time.time; // Initialize start time when the game begins
    }

    private IEnumerator PlaySuccessSoundAndCheckVictory()
    {
        successAudioSource.Play();
        yield return new WaitForSeconds(successAudioSource.clip.length);
        
        // Increment completed targets and check for victory
        completedTargets++;
        CheckForVictory();
    }

    private void CheckForVictory()
    {
        if (completedTargets >= totalTargets)
        {
            float elapsedTime = Time.time - startTime; // Calculate elapsed time
            //iXR.EventAssessmentComplete("stocking_training_unit_1", $"{score}", "success=true");

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

    private IEnumerator PlayFailSoundThenRestart()
    {
        if (failureAudioSource != null && !failureAudioSource.isPlaying)
        {
            failureAudioSource.Play();
            yield return new WaitForSeconds(failureAudioSource.clip.length);
        }
        RestartExperience();
    }

    private void RestartExperience()
    {
        InitializeAndReinitializeGame();
    }

    private void InitializeAndReinitializeGame()
    {
        // Initialize game state
        completedTargets = 0;
        score = 0;
        startTime = Time.time;

        try
        {
        //     // Reset the Dropper
        //     if (dropper != null)
        //     {
        //         dropper.ResetDropper();
        //         TargetLocation[] targetLocations = FindObjectsOfType<TargetLocation>();
        //         foreach (TargetLocation targetLocation in targetLocations)
        //         {
        //             dropper.Add(targetLocation.targetType);
        //         }
        //     }
        //     else
        //     {
        //         Debug.LogWarning("Dropper not found during reinitialization");
        //     }

        //     // Reinitialize GrabbableObjectManager
        //     GrabbableObjectManager.getInstance().Start();

        //     // Reset all TargetLocations
        //     TargetLocation[] allTargetLocations = FindObjectsOfType<TargetLocation>();
        //     foreach (TargetLocation targetLocation in allTargetLocations)
        //     {
        //         targetLocation.ResetState();
        //     }

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
            //iXR.LogInfo("Game components reinitialized successfully");
        }
        catch (Exception e)
        {
            Debug.LogError("Error during InitializeAndReinitializeGame: " + e.Message);
            //iXR.LogInfo("Error during InitializeAndReinitializeGame: " + e.Message);
        }
    }
}
