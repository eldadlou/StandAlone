using UnityEngine;
using UnityEngine.UI;
using MyGame.Core.Units;

namespace MyGame.Presentation
{
    /// <summary>
    /// Displays unit health above the unit in the game world
    /// </summary>
    public class HealthDisplay : MonoBehaviour
    {
        [Header("Health Display Settings")]
        [SerializeField] private bool showHealthBar = true;
        [SerializeField] private bool showHealthText = true;
        [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);
        [SerializeField] private float maxDistance = 50f; // Only show when camera is within this distance
        
        [Header("Health Bar")]
        [SerializeField] private GameObject healthBarPrefab;
        [SerializeField] private Color fullHealthColor = Color.green;
        [SerializeField] private Color lowHealthColor = Color.red;
        
        [Header("Health Text")]
        [SerializeField] private GameObject healthTextPrefab;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private int fontSize = 14;
        
        // Components
        private Unit unit;
        private Camera mainCamera;
        private GameObject healthBarObject;
        private GameObject healthTextObject;
        private Slider healthBarSlider;
        private Text healthText;
        
        // Health tracking
        private float lastHealth = -1f;
        private float maxHealth = 150f;
        
        private void Start()
        {
            // Get components
            unit = GetComponent<Unit>();
            mainCamera = Camera.main;
            
            if (unit == null)
            {
                Debug.LogError($"HealthDisplay: No Unit component found on {gameObject.name}");
                return;
            }
            
            // Get max health from unit
            maxHealth = unit.Health;
            
            // Create health display objects
            CreateHealthDisplay();
        }
        
        private void CreateHealthDisplay()
        {
            if (showHealthBar && healthBarPrefab != null)
            {
                healthBarObject = Instantiate(healthBarPrefab, transform.position + offset, Quaternion.identity);
                healthBarObject.transform.SetParent(transform);
                healthBarSlider = healthBarObject.GetComponentInChildren<Slider>();
                
                if (healthBarSlider != null)
                {
                    healthBarSlider.minValue = 0f;
                    healthBarSlider.maxValue = maxHealth;
                    healthBarSlider.value = maxHealth;
                }
            }
            
            if (showHealthText && healthTextPrefab != null)
            {
                healthTextObject = Instantiate(healthTextPrefab, transform.position + offset, Quaternion.identity);
                healthTextObject.transform.SetParent(transform);
                healthText = healthTextObject.GetComponentInChildren<Text>();
                
                if (healthText != null)
                {
                    healthText.color = textColor;
                    healthText.fontSize = fontSize;
                    healthText.text = $"{maxHealth:F0}/{maxHealth:F0}";
                }
            }
        }
        
        private void Update()
        {
            if (unit == null || mainCamera == null) return;
            
            // Check distance to camera
            float distanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);
            bool shouldShow = distanceToCamera <= maxDistance;
            
            // Update health display
            UpdateHealthDisplay(shouldShow);
            
            // Update position to face camera
            if (shouldShow)
            {
                UpdateDisplayPosition();
            }
        }
        
        private void UpdateHealthDisplay(bool shouldShow)
        {
            float currentHealth = unit.Health;
            
            // Only update if health changed
            if (currentHealth != lastHealth)
            {
                lastHealth = currentHealth;
                
                // Update health bar
                if (healthBarSlider != null)
                {
                    healthBarSlider.value = currentHealth;
                    
                    // Update color based on health percentage
                    float healthPercent = currentHealth / maxHealth;
                    Color barColor = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent);
                    healthBarSlider.fillRect.GetComponent<Image>().color = barColor;
                }
                
                // Update health text
                if (healthText != null)
                {
                    healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
                }
            }
            
            // Show/hide based on distance
            if (healthBarObject != null)
                healthBarObject.SetActive(shouldShow && showHealthBar);
            
            if (healthTextObject != null)
                healthTextObject.SetActive(shouldShow && showHealthText);
        }
        
        private void UpdateDisplayPosition()
        {
            // Make health display face the camera
            if (healthBarObject != null)
            {
                healthBarObject.transform.position = transform.position + offset;
                healthBarObject.transform.LookAt(mainCamera.transform);
                healthBarObject.transform.Rotate(0, 180, 0); // Flip to face camera properly
            }
            
            if (healthTextObject != null)
            {
                healthTextObject.transform.position = transform.position + offset;
                healthTextObject.transform.LookAt(mainCamera.transform);
                healthTextObject.transform.Rotate(0, 180, 0); // Flip to face camera properly
            }
        }
        
        private void OnDestroy()
        {
            // Clean up health display objects
            if (healthBarObject != null)
                Destroy(healthBarObject);
            
            if (healthTextObject != null)
                Destroy(healthTextObject);
        }
        
        // Public methods for external control
        public void SetShowHealthBar(bool show)
        {
            showHealthBar = show;
            if (healthBarObject != null)
                healthBarObject.SetActive(show);
        }
        
        public void SetShowHealthText(bool show)
        {
            showHealthText = show;
            if (healthTextObject != null)
                healthTextObject.SetActive(show);
        }
        
        public void SetMaxDistance(float distance)
        {
            maxDistance = distance;
        }
    }
}
