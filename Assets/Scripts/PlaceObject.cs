using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class PlaceObject : MonoBehaviour
{
    public GameObject gameObjectToInstantiate;

    private GameObject spawnedObject;
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private Vector2 touchPosition;
    private bool isTrackingEnabled = true;

    private float initialPinchDistance; // Store the initial distance between fingers
    private Vector3 initialScale; // Store the initial scale of the object

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }

        touchPosition = default;
        return false;
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

    void Update()
    {
        if (!TryGetTouchPosition(out Vector2 touchPosition) || !isTrackingEnabled)
            return;

        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;

            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(gameObjectToInstantiate, hitPose.position, hitPose.rotation);
            }
            else
            {
                spawnedObject.transform.position = hitPose.position;
            }
        }

        DetectPinchGesture(); // Call the function to detect pinch gesture
    }

    private void DetectPinchGesture()
    {
        if (Input.touchCount == 2) // Detect two-finger gesture
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            // Calculate the distance between two touches
            float currentPinchDistance = Vector2.Distance(touch1.position, touch2.position);

            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                // Store initial distance and scale when pinch starts
                initialPinchDistance = currentPinchDistance;
                if (spawnedObject != null)
                {
                    initialScale = spawnedObject.transform.localScale;
                }
            }
            else if (spawnedObject != null && (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved))
            {
                // Calculate scale factor based on pinch movement
                float scaleFactor = currentPinchDistance / initialPinchDistance;

                // Apply new scale with clamping to avoid extreme values
                spawnedObject.transform.localScale = initialScale * scaleFactor;
            }
        }
    }
}
