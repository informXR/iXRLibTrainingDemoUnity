# Unity Project Troubleshooting Guide

## Common Issues and Solutions

### 1. Missing Script References

**Problem**: "The referenced script (Unknown) on this Behaviour is missing!"

**Cause**: This typically occurs during variant switching when Unity's asset database gets corrupted or when scripts are moved/renamed.

**Solutions**:

1. **Use the Script Reference Fixer**:
   - Go to `Tools > Fix Missing Script References` in the Unity menu
   - This will identify all missing script references in the scene

2. **Re-import All Scripts**:
   - Go to `Tools > Re-import All Scripts` in the Unity menu
   - This forces Unity to re-import all script files

3. **Manual Fix**:
   - In the Inspector, look for components showing "Missing Script"
   - Click the gear icon and select "Remove Component"
   - Re-add the component if needed

### 2. Collider Registration Conflicts

**Problem**: "A collider used by an Interactable object is already registered with another Interactable object."

**Cause**: Multiple XR interactable components are trying to use the same collider.

**Solutions**:

1. **Use Scene Validator**:
   - Add the `SceneValidator` component to any GameObject in your scene
   - Right-click the component and select "Validate Scene"
   - Use "Fix ExitCube Issues" to resolve ExitCube-specific conflicts

2. **Manual Fix for ExitCube**:
   - Select the ExitCube GameObject
   - Remove the `XRSimpleInteractable` component (keep the `ExitButton` component)
   - The `ExitButton` already inherits from `XRBaseInteractable`, so it doesn't need `XRSimpleInteractable`

### 3. Variant Switching Issues

**Problem**: Asset import errors during variant switching.

**Solutions**:

1. **Clean Project Settings**:
   - Go to `Tools > Clean Project Settings` in the Unity menu
   - This will force Unity to reload project settings cleanly

2. **Manual Asset Refresh**:
   - Go to `Assets > Refresh` in the Unity menu
   - Or press `Ctrl+R` (Windows) / `Cmd+R` (Mac)

3. **Restart Unity**:
   - Close Unity completely
   - Reopen the project
   - This often resolves persistent asset import issues

### 4. Preventative Measures

1. **Add Scene Validator**:
   - Add the `SceneValidator` component to a GameObject in your scene
   - Enable "Validate On Start" and "Auto Fix Issues"
   - This will automatically check for and fix common issues when the scene starts

2. **Use the Fixed MouseInteractionController**:
   - The updated `MouseInteractionController` now properly checks for existing components before adding new ones
   - This prevents collider conflicts with ExitCube

3. **Improved Variant Manager**:
   - The updated `VariantManager` now has better error handling
   - It will log warnings instead of crashing when asset operations fail

## Quick Fix Commands

Run these in order if you encounter issues:

1. `Tools > Fix Missing Script References`
2. `Tools > Re-import All Scripts`
3. `Tools > Clean Project Settings`
4. `Assets > Refresh`

## Scene Setup Checklist

Before running your scene, ensure:

- [ ] GrabbableObjectManager exists in the scene
- [ ] LevelManager exists in the scene
- [ ] ExitCube has only one interactable component (ExitButton OR XRSimpleInteractable, not both)
- [ ] All required scripts are properly attached to GameObjects
- [ ] No missing script references in the Inspector

## Debug Information

The `Dropper` script logs the number of target locations found and their types. This is normal behavior and helps verify that the scene is set up correctly.

If you see:
- "11" - This is the number of target locations found
- Food names like "Apple", "Beet", "Avocado" - These are the target types being added to the dropper queue

This indicates the scene is loading correctly. 