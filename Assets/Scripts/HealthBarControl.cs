using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarControl : MonoBehaviour
{
    public Slider slider; // Slider reference
    
    public void SetHealth(int health) { // Update health bar
        slider.value = health;
    }
}
