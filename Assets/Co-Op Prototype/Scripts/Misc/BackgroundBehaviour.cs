using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class BackgroundBehaviour : MonoBehaviour
{
    public bool IsScrolling;
    public bool IsParallaxing;

    [ShowIf("IsParallaxing")]
    [Tooltip("How fast this background moves in relation to cameraPos/Player")]
    public float parallaxSpeed;

    private Camera mainCamera;
    private Transform cameraTransform;
    private Transform[] layers;
    private float heightY;
    private float backgroundSize;
    private float lastCameraX;//X coordinate of camera, on previous frame
    private float deltaX;//Difference of current camera X coordinate, and lastCameraX
    private float viewZone = 10;//HMMM
    private int leftIndex;
    private int rightIndex;
    

    private void Start()
    {
        mainCamera = Camera.main;

        cameraTransform = mainCamera.transform;

        layers = new Transform[transform.childCount];//array of size 3

        //Fill the above array
        for (int i = 0; i < layers.Length; i++)
            layers[i] = transform.GetChild(i);

        //Gets the height, from the first background sprite, not the container/parent
        heightY = layers[0].position.y;

        //The "maximum" left and right
        leftIndex = 0;
        rightIndex = layers.Length - 1;

        backgroundSize = layers[0].GetComponent<SpriteRenderer>().bounds.size.x;

        lastCameraX = cameraTransform.position.x;
    }

    private void Update()
    {
        if (IsParallaxing)
        {
            deltaX = cameraTransform.position.x - lastCameraX;

            //Moves the container/parent
            transform.position = transform.position + Vector3.right * (deltaX * parallaxSpeed);
        }

        lastCameraX = cameraTransform.position.x;

        if (IsScrolling)
        {
            if (lastCameraX < layers[leftIndex].transform.position.x + viewZone)
                ScrollLeft();

            if (lastCameraX > layers[rightIndex].transform.position.x - viewZone)
                ScrollRight();
        }


        //TESTING
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            ScrollLeft();

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            ScrollRight();
    }

    //Called when camera reached the left "bounds" of current sprite/background
    private void ScrollLeft()
    {
        //int lastRight = rightIndex;
        layers[rightIndex].position = Vector3.right * (layers[leftIndex].position.x - backgroundSize) + new Vector3(0, heightY, 0);
        leftIndex = rightIndex;
        rightIndex--;
        if (rightIndex < 0)
            rightIndex = layers.Length - 1;
    }

    //Called when camera reached the right "bounds" of current sprite/background
    private void ScrollRight()
    {
        //int lastLeft = leftIndex;
        layers[leftIndex].position = Vector3.right * (layers[rightIndex].position.x + backgroundSize) + new Vector3(0,heightY,0);
        rightIndex = leftIndex;
        leftIndex++;
        if (leftIndex == layers.Length)
            leftIndex = 0;
    }

    
}
