using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class PlaceObjectsRandomly : MonoBehaviour
{
    public GameObject[] flowerPrefabs; // Array of flower prefabs to spawn
    public int flowerCount = 5; // Number of flowers to spawn per plane
    public float spawnRadius = 0.2f; // Radius within which flowers will be randomly placed

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private bool isTrackingEnabled = true;

    private float initialPinchDistance;
    private Vector3 initialScale;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
        planeManager.planesChanged += OnPlanesChanged;
    }

    public void TogglePlaneTracking()
    {
        isTrackingEnabled = !isTrackingEnabled;
        planeManager.enabled = isTrackingEnabled;

        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(isTrackingEnabled);
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (!isTrackingEnabled) return;

        foreach (var plane in args.added)
        {
            SpawnFlowersOnPlane(plane);
        }
    }

    private void SpawnFlowersOnPlane(ARPlane plane)
    {
        for (int i = 0; i < flowerCount; i++)
        {
            Vector3 randomPosition = plane.transform.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0,
                Random.Range(-spawnRadius, spawnRadius)
            );

            GameObject flowerPrefab = flowerPrefabs[Random.Range(0, flowerPrefabs.Length)]; // Pick a random flower
            Instantiate(flowerPrefab, randomPosition, Quaternion.identity);
        }
    }

    void Update()
    {
        DetectPinchGesture();
    }

    private void DetectPinchGesture()
    {
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            float currentPinchDistance = Vector2.Distance(touch1.position, touch2.position);

            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                initialPinchDistance = currentPinchDistance;
                initialScale = flowerPrefabs[0].transform.localScale;
            }
            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                float scaleFactor = currentPinchDistance / initialPinchDistance;

                foreach (GameObject flower in flowerPrefabs)
                {
                    flower.transform.localScale = initialScale * scaleFactor;
                }
            }
        }
    }
}
