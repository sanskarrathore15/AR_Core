using UnityEngine;
using UnityEngine.UI;

public class BirdSpawner : MonoBehaviour
{
    public GameObject arBird; // Reference to the AR Bird
    public GameObject staticBird; // Reference to the Static Bird
    public Button spawnButton; // UI Button Reference

    void Start()
    {
        staticBird.SetActive(true);
        arBird.SetActive(false); // Hide AR Bird initially
        spawnButton.onClick.AddListener(ToggleBird); // Attach function to button
    }

    public void ToggleBird()
    {
        if (!arBird.activeSelf) // If AR bird is hidden
        {
            arBird.SetActive(true);  // Show AR Bird
            staticBird.SetActive(false); // Hide static marker
        }
        else
        {
            arBird.SetActive(false); // Hide AR Bird
            staticBird.SetActive(true); // Show static marker
        }
    }
}
