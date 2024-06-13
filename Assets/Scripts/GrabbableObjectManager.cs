using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR.Interaction.Toolkit;
public class GrabbableObjectManager : MonoBehaviour
{
    public GameObject grabbableObjectPrefab;
    [SerializeField]
    public List<GrabbableObjectData> grabbableObjectList;
    private static GrabbableObjectManager instance;

    [Serializable]
    public struct GrabbableObjectData
    {
        public GrabbableObjectType type;
        public Mesh mesh;
        public List<Material> materials;
    }

    public enum GrabbableObjectType
    {
        Apple, Avocado, Bacon, Bag, Banana,
        Barrel, Beet, Ketchup, Mustard, Oil,
        Bowl, Bread, Broccoli, Burger, Cabbage,
        Cake, CakeSlicer, Can, CandyBar,
        SmallCan, Carrot, Carton, SmallCarton, Cauliflower,
        Celery, Cheese, CheseSlicer, Cherries, Chinese,
        Chocolate, Chopstick, Cocktail, Coconut, Cookie,
        Fork, Knife, Spatula, Spoon, Corn,
        Croissant, Cup, Cupcake, Saucer, Tea,
        Frappe, Fish, Fries, Loaf, Baguette,
        IceCream, RoundLoaf, Mug, Mortar, LollyPop,
        Plate, PizzaCutter, PizzaBox, Pineapple, Pan,
        Pancakes, SodaCan, SodaBottle, Whisk, Sundae
    }

    public static GrabbableObjectManager getInstance()
    {
        return instance;
    }

    public void Start()
    {
        if (instance != null && instance != this) Destroy(this.gameObject);
        else instance = this;

        CreateGrabbableObject(GrabbableObjectType.Apple);
    }

    public GameObject CreateGrabbableObject(GrabbableObjectData grabbableObjectData) { return CreateGrabbableObject(grabbableObjectData, transform); }
    public GameObject CreateGrabbableObject(GrabbableObjectType grabbableObjectType) { return CreateGrabbableObject(grabbableObjectType, transform); }
    public GameObject CreateGrabbableObject(GrabbableObjectType grabbableObjectType, Transform transform) { return CreateGrabbableObject(getGrabbableObjectData(grabbableObjectType), transform); }
    public GameObject CreateGrabbableObject(GrabbableObjectData grabbableObjectData, Transform transform)
    {




        GameObject obj = Instantiate(grabbableObjectPrefab, transform);
        obj.GetComponent<MeshFilter>().mesh = grabbableObjectData.mesh;
        obj.GetComponent<GrabbableObject>().type = grabbableObjectData.type;
        obj.GetComponent<MeshCollider>().sharedMesh = grabbableObjectData.mesh;
        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
        meshRenderer.materials = grabbableObjectData.materials.ToArray();

        // Get All Targets
        TargetLocation[] targetLocations = FindObjectsOfType(typeof(TargetLocation)) as TargetLocation[];
        foreach (TargetLocation targetLocation in targetLocations)
        {
            obj.GetComponent<XRGrabInteractable>().selectExited.AddListener(interactable => targetLocation.OnRelease());
        }

        return obj;
    }

    public GrabbableObjectData getGrabbableObjectData(GrabbableObjectType type)
    {
        foreach (GrabbableObjectData data in grabbableObjectList)
            if (data.type == type)
                return data;
        return new GrabbableObjectData();
    }
}
