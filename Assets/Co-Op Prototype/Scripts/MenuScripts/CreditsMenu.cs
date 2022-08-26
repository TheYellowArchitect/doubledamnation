using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;

    public GameObject scrollingText;

    private Vector2 originalPosition;
    private RectTransform scrollingTransform;

    [Header("Scrolling Values")]
    public bool isScrolling = false;
    public float defaultScrolling = 50f;
    public float currentScrollingMultiplier = 1f;
    public float defaultScrollingMultiplier = 1f;
    public float buttonAMultiplier = 3f;
    public float buttonXMultiplier = 2f;
    public float buttonYMultiplier = 2f;
    public float buttonSpaceMultiplier = 2f;
    public float buttonDashMultiplier = 2f;

    [Header("Misc Values")]
    public float bottomBoundary = 4288f;
    public float topBoundary = -950f;

    // Use this for initialization
    void Awake()
    {
        scrollingTransform = scrollingText.GetComponent<RectTransform>();
        originalPosition = scrollingTransform.anchoredPosition;
    }

    //No need for level-changing text here as well. Black&White ftw.
    public void RollCredits()
    {
        if (isScrolling == false)
            isScrolling = true;
    }

    private void OnDisable()
    {
        //if (scrollingTransform != null)//cuz at first time it happens, it maybe hasn't gotten them//nvm, Awake instead of Start saves the day
            scrollingTransform.anchoredPosition = originalPosition;

            isScrolling = false;
    }

    private void Update()
    {
        if (isScrolling == false)
            return;

        currentScrollingMultiplier = GetCurrentScrollingMultiplier();

        scrollingTransform.anchoredPosition = new Vector2(scrollingTransform.anchoredPosition.x, scrollingTransform.anchoredPosition.y + (defaultScrolling * currentScrollingMultiplier * Time.unscaledDeltaTime));

        //Debug.Log("Current anchored position Y: " + scrollingTransform.anchoredPosition.y);//Prepare for 6 FPS

        //Boundaries Range
        if (scrollingTransform.anchoredPosition.y > bottomBoundary)
            scrollingTransform.anchoredPosition = new Vector2(scrollingTransform.anchoredPosition.x, bottomBoundary);
        else if (scrollingTransform.anchoredPosition.y < topBoundary)
            scrollingTransform.anchoredPosition = new Vector2(scrollingTransform.anchoredPosition.x, topBoundary);

        //Exit Credits
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonUp("ButtonB"))
            pauseMenuUI.GetComponent<PauseMenu>().Resume();            
    }

    public float GetCurrentScrollingMultiplier()
    {
        defaultScrollingMultiplier = 1f;

        //Joysticks
            defaultScrollingMultiplier = defaultScrollingMultiplier + Input.GetAxis("LeftJoystickVertical") * -1 + Input.GetAxis("RightJoystickVertical") * -1;

        //UpDown Arrows
            if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.DownArrow) == false)
                defaultScrollingMultiplier = defaultScrollingMultiplier - 1f;
            else if (Input.GetKey(KeyCode.UpArrow) == false && Input.GetKey(KeyCode.DownArrow))
                defaultScrollingMultiplier = defaultScrollingMultiplier + 1f;

        //Pure multiplier buttons
            if (Input.GetKey(KeyCode.Space))
                defaultScrollingMultiplier = defaultScrollingMultiplier * buttonSpaceMultiplier;

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                defaultScrollingMultiplier = defaultScrollingMultiplier * buttonDashMultiplier;

            if (Input.GetButton("ButtonA"))
                defaultScrollingMultiplier = defaultScrollingMultiplier * buttonAMultiplier;

            if (Input.GetButton("ButtonX"))
                defaultScrollingMultiplier = defaultScrollingMultiplier * buttonXMultiplier;

            if (Input.GetButton("ButtonY"))
                defaultScrollingMultiplier = defaultScrollingMultiplier * buttonYMultiplier;

        return defaultScrollingMultiplier;
    }
}
