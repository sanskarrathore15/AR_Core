using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class PlaceObjectAndFlowers : MonoBehaviour
{
    public GameObject gameObjectToInstantiate; // Main object
    public GameObject[] flowerPrefabs; // Flower prefabs array
    public int flowerCount = 5; // Number of flowers per plane
    public float spawnRadius = 0.2f; // Random placement radius
    public float objectHeightOffset = 0.05f; // Height offset for main object

    public Button spawnFlowersButton; // Button to spawn flowers
    public Toggle togglePlanesButton; // Toggle button for planes
    public Toggle planeSelectionToggle; // Toggle for selecting planes to spawn flowers
    public Button resetObjectButton; // Button to reset the spawned object

    private GameObject spawnedObject;
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private ARPlane lastSelectedPlane = null; // Track last clicked plane
    private bool arePlanesVisible = true; // Track plane visibility
    private bool isPlaneSelectionEnabled = false; // Track if plane selection is enabled

    private float initialPinchDistance;
    private Vector3 initialScale;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();

        // Add button event listeners
        spawnFlowersButton.onClick.AddListener(SpawnFlowersOnSelectedPlane);
        togglePlanesButton.onValueChanged.AddListener(TogglePlanesVisibility);
        planeSelectionToggle.onValueChanged.AddListener(TogglePlaneSelection);
        resetObjectButton.onClick.AddListener(ResetSpawnedObject);
    }

    void Update()
    {
        // Ignore touch input if over UI elements
        if (IsTouchOverUI()) return;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            TryPlaceObjectOrSelectPlane();
        }

        DetectPinchGesture();
    }

    private void TryPlaceObjectOrSelectPlane()
    {
        Vector2 touchPosition = Input.GetTouch(0).position;

        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            ARPlane hitPlane = planeManager.GetPlane(hits[0].trackableId);

            if (isPlaneSelectionEnabled) // Selecting plane for flowers
            {
                lastSelectedPlane = hitPlane;
            }
            else // Placing the main object (bird)
            {
                if (spawnedObject == null)
                {
                    spawnedObject = Instantiate(gameObjectToInstantiate, hitPose.position + Vector3.up * objectHeightOffset, Quaternion.identity);
                }
                else
                {
                    spawnedObject.transform.position = hitPose.position + Vector3.up * objectHeightOffset;
                }
            }
        }
    }

    private void SpawnFlowersOnSelectedPlane()
    {
        ARPlane targetPlane = lastSelectedPlane; // Default to the last selected plane

        if (!isPlaneSelectionEnabled)
        {
            targetPlane = GetLowestPlane(); // Find the lowest plane when selection is OFF
        }

        if (targetPlane == null || !targetPlane.gameObject.activeSelf) return;

        for (int i = 0; i < flowerCount; i++)
        {
            Vector3 randomPosition = targetPlane.transform.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0,
                Random.Range(-spawnRadius, spawnRadius)
            );

            GameObject flowerPrefab = flowerPrefabs[Random.Range(0, flowerPrefabs.Length)];
            Instantiate(flowerPrefab, randomPosition, Quaternion.identity);
        }
    }

    private ARPlane GetLowestPlane()
    {
        ARPlane lowestPlane = null;
        float lowestY = float.MaxValue;

        foreach (var plane in planeManager.trackables)
        {
            if (plane.gameObject.activeSelf && plane.center.y < lowestY)
            {
                lowestY = plane.center.y;
                lowestPlane = plane;
            }
        }

        return lowestPlane;
    }


    private void TogglePlanesVisibility(bool isOn)
    {
        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(isOn);
        }
    }

    private void TogglePlaneSelection(bool isOn)
    {
        isPlaneSelectionEnabled = isOn;
    }

    private void ResetSpawnedObject()
    {
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
            lastSelectedPlane = null;
        }
    }

    private void DetectPinchGesture()
    {
        if (Input.touchCount == 2 && spawnedObject != null)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            float currentPinchDistance = Vector2.Distance(touch1.position, touch2.position);

            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                initialPinchDistance = currentPinchDistance;
                initialScale = spawnedObject.transform.localScale;
            }
            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                float scaleFactor = currentPinchDistance / initialPinchDistance;
                spawnedObject.transform.localScale = initialScale * scaleFactor;
            }
        }
    }

    private bool IsTouchOverUI()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
        }
        return false;
    }
}
