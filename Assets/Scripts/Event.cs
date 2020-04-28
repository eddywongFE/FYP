using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event : MonoBehaviour
{
    public GameObject item; // Event prefabs
    public string eventName; // Event type
    
    private GameStateController stateController; // GameStateController reference
    private int EventNumber; // Event index on path

    public void SetGameStateController(GameStateController controller) { // Set GameStateController reference
        this.stateController = controller;
    }

    public void SetEventNumber(int index) { // Set event index
        this.EventNumber = index;
    }

    void OnMouseDown() { // Trigger when player click event
        stateController.SetCurrentEvent(this.EventNumber, this.eventName);
        stateController.popup(this.eventName);
    }
}
