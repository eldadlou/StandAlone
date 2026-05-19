using UnityEngine;
using MyGame.Core.Units;

namespace MyGame.Presentation
{
    /// <summary>
    /// Handles all Unity-specific visual logic for units
    /// Separated from pure logic to allow for better testing and flexibility
    /// </summary>
    public class UnitVisual : MonoBehaviour
    {
        [Header("Selection & Destination Visuals")]
        public GameObject selectionCirclePrefab;
        public GameObject destinationMarkerPrefab;
        public Vector3 VisualOffset;
        private UnitData unitData;
        private bool isSelected = false;
        
        // Instantiated visual objects
        private GameObject selectionCircleInstance;
        private GameObject destinationMarkerInstance;

        public void Initialize(UnitData data)
        {
            unitData = data;
            
            // Subscribe to unit events for visual updates
            if (unitData is not null)
            {
                unitData.OnDeath += HandleUnitDeath;
                unitData.OnAnimationEvent += HandleAnimationEvent;
            }
            
            // Create visual instances
            CreateVisualInstances();
        }

        private void CreateVisualInstances()
        {
          //  Debug.Log($"Creating visual instances for {gameObject.name}");
            
            // Create selection circle instance at neutral position
            if (selectionCirclePrefab is not null)
            {
                // Create at origin, will be positioned when needed
                selectionCircleInstance = Instantiate(selectionCirclePrefab, Vector3.zero, Quaternion.identity);
                selectionCircleInstance.transform.SetParent(transform);
                selectionCircleInstance.SetActive(false); // Start hidden
         //       Debug.Log($"Created selection circle instance for {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"Selection circle prefab not assigned for {gameObject.name}");
            }
            
            // Create destination marker instance
            if (destinationMarkerPrefab is not null)
            {
                destinationMarkerInstance = Instantiate(destinationMarkerPrefab, Vector3.zero, Quaternion.Euler(Vector3.up));
                destinationMarkerInstance.SetActive(false); // Start hidden
                Debug.Log($"Created destination marker instance for {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"Destination marker prefab not assigned for {gameObject.name}");
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            if (unitData is not null)
            {
                unitData.OnDeath -= HandleUnitDeath;
                unitData.OnAnimationEvent -= HandleAnimationEvent;
            }
            
            // Clean up instantiated objects
            if (selectionCircleInstance is not null)
                Destroy(selectionCircleInstance);
            if (destinationMarkerInstance is not null)
                Destroy(destinationMarkerInstance);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
   //         Debug.Log($"Setting selection for {gameObject.name} to: {selected}");
            
            if (selectionCircleInstance is not null)
            {
                selectionCircleInstance.SetActive(selected);
                
                // Update position to follow the unit
                if (selected)
                {
                    selectionCircleInstance.transform.position = transform.position +VisualOffset;
 //                   Debug.Log($"Selection circle positioned at {transform.position} for {gameObject.name}");
                }
            }
            else
            {
                Debug.LogWarning($"Selection circle instance is null for {gameObject.name}");
            }
        }

        public void UpdatePosition(Vector3 newPosition)
        {
            // Unity-specific position update
            transform.position = newPosition;
            
            // Update selection circle position if selected
            if (isSelected && selectionCircleInstance is not null)
            {
                selectionCircleInstance.transform.position = newPosition+VisualOffset;
            }
            
            // Update the unit data (pure logic)
            unitData?.UpdatePosition(newPosition);
        }

        public void ShowDestinationMarker(Vector3 destination)
        {
      //      Debug.Log($"Showing destination marker at {destination} for {gameObject.name}");
            
            if (destinationMarkerInstance is not null)
            {
                destinationMarkerInstance.transform.position = destination;
                destinationMarkerInstance.SetActive(true);
   //             Debug.Log($"Destination marker activated for {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"Destination marker instance is null for {gameObject.name}");
            }
        }

        public void HideDestinationMarker()
        {
            destinationMarkerInstance?.SetActive(false);
        }

        private void HandleUnitDeath(UnitData unit)
        {
            // Visual death effects
            // Could play death animation, particle effects, etc.
            Debug.Log($"Unit {unit.Type} died!");
        }

        private void HandleAnimationEvent(string eventName)
        {
            // Handle animation events
            // Could trigger animator parameters, play sounds, etc.
            Debug.Log($"Animation event: {eventName}");
        }
    }
} 