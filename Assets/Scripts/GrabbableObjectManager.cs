using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class GrabbableObjectManager : MonoBehaviour
{

    //I COULDNT FIND A CLEANER SOLUTION THAT WOULDN'T TAKE CRAZY AMOUNTS OF WORK, ITS MESSY AND PAINFUL BUT IT WORKS AND HEY ATLEAST I DONT NEED A PREFAB FOR EVERY TYPE

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
    }

    public GameObject CreateGrabbableObject(GrabbableObjectType grabbableObjectType, Transform transform)
    {

        return CreateGrabbableObject(getGrabbableObjectData(grabbableObjectType), transform);
    }

    public GameObject CreateGrabbableObject(GrabbableObjectData grabbableObjectData, Transform transform)
    {
        GameObject obj = Instantiate(grabbableObjectPrefab, transform);
        obj.GetComponent<MeshFilter>().mesh = grabbableObjectData.mesh;
        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
        meshRenderer.materials = grabbableObjectData.materials.ToArray();
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
