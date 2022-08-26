using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GrassColliderManager : MonoBehaviour
{
    /// <summary>
    /// Created because player enters the trigger2D() 2 times instead of once, because he has 2 colliders.
    /// </summary>
    private bool recognized = false;

    private Transform child0;
    private Transform child1;

    private void Start()
    {
        child0 = transform.GetChild(0).transform;
        child1 = transform.GetChild(1).transform;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //==============================================================
        recognized = !recognized;

        if (recognized == false)
            return;
        //==============================================================

        //Activate the colliders

            //Child0
            for (int i = 0; i < child0.transform.childCount; i++)
                child0.GetChild(i).gameObject.GetComponent<Collider2D>().enabled = true;

            //Child1
            for (int i = 0; i < child1.transform.childCount; i++)
                child1.GetChild(i).gameObject.GetComponent<Collider2D>().enabled = true;

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //==============================================================
        recognized = !recognized;

        if (recognized == false)
            return;
        //==============================================================

        //Deactivate the colliders

            //Child0
            for (int i = 0; i < child0.transform.childCount; i++)
                child0.GetChild(i).gameObject.GetComponent<Collider2D>().enabled = false;

            //Child1
            for (int i = 0; i < child1.transform.childCount; i++)
                child1.GetChild(i).gameObject.GetComponent<Collider2D>().enabled = false;
    }
}

#region The Previous GrassManager (in case you find an edge case this one doesnt work)
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassColliderManager : MonoBehaviour
{
    /// <summary>
    /// Created because player enters the trigger2D() 2 times instead of once, because he has 2 colliders.
    /// </summary>
    private bool recognized = false;

    private Transform child0;
    private Transform child1;

    private void Start()
    {
        child0 = transform.GetChild(0).transform;
        child1 = transform.GetChild(1).transform;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //==============================================================
        recognized = !recognized;

        if (recognized == false)
            return;
        //==============================================================

        //Activate the colliders

            //Child0
            for (int i = 0; i < child0.transform.childCount; i++)
                child0.GetChild(i).gameObject.GetComponent<Collider2D>().enabled = true;

            //Child1
            for (int i = 0; i < child1.transform.childCount; i++)
                child1.GetChild(i).gameObject.GetComponent<Collider2D>().enabled = true;

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //==============================================================
        recognized = !recognized;

        if (recognized == false)
            return;
        //==============================================================

        //Deactivate the colliders

            //Child0
            for (int i = 0; i < child0.transform.childCount; i++)
                child0.GetChild(i).gameObject.GetComponent<Collider2D>().enabled = false;

            //Child1
            for (int i = 0; i < child1.transform.childCount; i++)
                child1.GetChild(i).gameObject.GetComponent<Collider2D>().enabled = false;
}
}

*/
#endregion