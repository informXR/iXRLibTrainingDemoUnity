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
    private int _totalTargets;
    private int _completedTargets;

    private void Start()
    {
        iXR.LogInfo("Content started (LevelManager)");
        iXR.EventAssessmentStart("stocking_training_unit_1");
        InitializeGame();
    }

    private void InitializeGame()
    {
        _totalTargets = FindObjectsOfType<TargetLocation>().Length;
        _completedTargets = 0;
        score = 0;
    }

    public void CompleteTask(TargetLocation.CompletionData completionData)
    {
        iXR.LogInfo("Placement Attempted");
        Debug.Log("iXRLib - Placement Attempted");

        if (completionData.usedType != completionData.targetType)
        {
            dropper.Replace(completionData.targetType, completionData.usedType);

            completionData.usedTarget.GetComponent<MeshFilter>().sharedMesh = completionData.usedObject.GetComponent<MeshFilter>().sharedMesh;
            string objectId = completionData.usedObject.GetComponent<GrabbableObject>().Id; // Change 'id' to 'Id'
            iXR.EventInteractionComplete($"place_item_{objectId}", "False", "Wrong spot", iXR.InteractionType.Bool, $"placed_fruit={completionData.usedType},intended_fruit={completionData.targetType}");

            StartCoroutine(PlayFailSoundThenRestart());
        }
        else
        {
            string objectId = completionData.usedObject.GetComponent<GrabbableObject>().Id; // Change 'id' to 'Id'

            iXR.EventInteractionComplete($"place_item_{objectId}", "True", "Correct spot", iXR.InteractionType.Bool, $"placed_fruit={completionData.usedType},intended_fruit={completionData.targetType}");

            StartCoroutine(PlaySuccessSoundAndCheckVictory());
        }

        completionData.usedObject.GetComponent<XRGrabInteractable>().colliders.Clear();

        // Disable the collision box of the usedTarget
        Collider targetCollider = completionData.usedTarget.GetComponent<Collider>();
        if (targetCollider != null)
        {
            targetCollider.enabled = false;
        }

        completionData.usedTarget.GetComponent<MeshRenderer>().materials = GrabbableObjectManager.getInstance().getGrabbableObjectData(completionData.usedType).model.GetComponent<MeshRenderer>().sharedMaterials;

        Destroy(completionData.usedObject);
        Destroy(completionData.usedTarget.GetComponent<Outline>());
        Destroy(completionData.usedTarget.GetComponent<TargetLocation>());

        // Calculate Score - later this should be moved out of level manager into its own score manager class that is persistant
        score += (1 / completionData.positionDistance) > 5 ? 5 : 1 / completionData.positionDistance;
        score += 1 - completionData.rotationDistance;
        score *= completionData.targetType == completionData.usedType ? 1 : .25;
    }

    private IEnumerator PlaySuccessSoundAndCheckVictory()
    {
        successAudioSource.Play();
        yield return new WaitForSeconds(successAudioSource.clip.length);

        // Increment completed targets and check for victory
        _completedTargets++;
        CheckForCompletion();
    }

    private void CheckForCompletion()
    {
        if (_completedTargets >= _totalTargets)
        {
            if (score > 70)
            {
                iXR.EventAssessmentComplete("stocking_training_unit_1", $"{score}", result: iXR.ResultOptions.Pass);
                PlaySuccessSound();
            }
            else
            {
                iXR.EventAssessmentComplete("stocking_training_unit_1", $"{score}", result: iXR.ResultOptions.Fail);
                PlayFailSound();
            }
        }
    }

    private void PlaySuccessSound()
    {
        if (victoryAudioSource != null && !victoryAudioSource.isPlaying)
        {
            victoryAudioSource.Play();
            Debug.Log("Level Completed! Success!");

            StartCoroutine(RestartAfterCompletionSound(victoryAudioSource.clip.length));
        }
    }

    private void PlayFailSound()
    {
        if (failureAudioSource != null && !failureAudioSource.isPlaying)
        {
            failureAudioSource.Play();
            Debug.Log("Level Completed! Failure!");

            StartCoroutine(RestartAfterCompletionSound(failureAudioSource.clip.length));
        }
    }

    private IEnumerator RestartAfterCompletionSound(float delay)
    {
        // Wait for the sound to finish playing
        yield return new WaitForSeconds(delay);
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
        _totalTargets = FindObjectsOfType<TargetLocation>().Length;
        _completedTargets = 0;
        score = 0;

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
            iXR.LogInfo("Game components reinitialized successfully");
        }
        catch (Exception e)
        {
            Debug.LogError("Error during InitializeAndReinitializeGame: " + e.Message);
            iXR.LogInfo("Error during InitializeAndReinitializeGame: " + e.Message);
        }
    }
}
