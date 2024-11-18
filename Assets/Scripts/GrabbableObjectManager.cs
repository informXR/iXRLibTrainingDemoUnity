using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR.Interaction.Toolkit;

//public class GrabbableObject : MonoBehaviour
//{
//    public GrabbableObjectManager.GrabbableObjectType type;
//}

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
        public GameObject model;
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
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            // Add any additional initialization here
        }
    }

    public GrabbableObjectData getGrabbableObjectData(GrabbableObjectType type)
    {
        foreach (GrabbableObjectData data in grabbableObjectList)
            if (data.type == type)
                return data;
        return new GrabbableObjectData();
    }
}
