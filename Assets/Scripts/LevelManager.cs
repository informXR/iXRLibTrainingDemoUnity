using System.Collections;
using System.Collections.Generic;
using iXRLib;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public Dropper dropper;
    public AudioSource successAudioSource;
    public AudioSource failureAudioSource;
    public AudioSource victoryAudioSource; // Assign this in the Unity Inspector
    public double score;
    private int totalTargets;
    private int completedTargets;

    void Start()
    {
        // Count the total number of targets at the start
        totalTargets = FindObjectsOfType<TargetLocation>().Length;
        completedTargets = 0;
    }
    
    public void CompleteTask(TargetLocation.CompletionData completionData)
    {
        iXRSend.LogInfo("Placement Attempted");
        Debug.Log("iXRLib - Placement Attempted");

        if (completionData.usedType != completionData.targetType)
        {
            dropper.Replace(completionData.targetType, completionData.usedType);
            completionData.usedTarget.GetComponent<MeshFilter>().sharedMesh = completionData.usedObject.GetComponent<MeshFilter>().sharedMesh;
            iXRSend.AddEvent("Debug", "Placement Failed", "event", "env", $"fruit,{completionData.usedType}");
            failureAudioSource.Play();

            StartCoroutine(RestartAfterFailSound());

        }  else {
            iXRSend.AddEvent("Debug", "Placement Completed", "event", "env", $"fruit,{completionData.usedType}");
            successAudioSource.Play();
            // Increment completed targets and check for victory
            completedTargets++;
            CheckForVictory();
        }

        completionData.usedObject.GetComponent<XRGrabInteractable>().colliders.Clear();
        //completionData.usedTarget.GetComponent<BoxCollider>().isTrigger = false;

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

    private string GetFruitName(GrabbableObjectManager.GrabbableObjectType fruitType)
    {
        return fruitType.ToString().ToLower();
    }
    private void CheckForVictory()
    {
        if (completedTargets >= totalTargets)
        {
            PlayVictorySound();
            // You can add more victory actions here, like showing a UI panel, etc.
        }
    }

    private void PlayVictorySound()
    {
        if (victoryAudioSource != null && !victoryAudioSource.isPlaying)
        {
            victoryAudioSource.Play();
            iXRSend.AddEvent("Debug", "Level Completed", "event", "env", "victory,true");
            Debug.Log("Level Completed! Victory!");

            StartCoroutine(RestartAfterVictorySound());
        }
    }

    private IEnumerator RestartAfterVictorySound()
    {
        // Wait for the victory sound to finish playing
        yield return new WaitForSeconds(victoryAudioSource.clip.length);
        
        // Call the restart method
        RestartExperience();
    }

    private IEnumerator RestartAfterFailSound()
    {
        // Wait for the victory sound to finish playing
        yield return new WaitForSeconds(victoryAudioSource.clip.length);
        
        // Call the restart method
        RestartExperience();
    }

    private void RestartExperience()
    {
        // Reset any necessary game state variables
        score = 0;
        completedTargets = 0;

        // Reload the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}