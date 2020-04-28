using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spot : MonoBehaviour
{
    public GameObject button;
    private bool occupied;

    private GameController gameController;

    public void SetGameControllerReferences(GameController controller) {
        gameController = controller;
    }

    // Place a token on the spot
    public void SetToken() {
        GameObject token = gameController.getCurrentToken();
        Vector3 pos = transform.position;
        token.transform.position = pos;
        token.transform.localScale = Vector3.one;

        var prev_I = gameController.getTokenIndex(token);
        var next_I = gameController.getSpotIndex(button);
        gameController.SetTokenList(prev_I, next_I);

        if (gameController.CheckChangedClosedMill() == true) {
            gameController.SelectedRemoveableToken();
        } else {
            gameController.SwitchTurn();
        }
    }

    // Set spot occupation
    public void setOccupation(bool occupied) {
        this.occupied = occupied;
    }

    public bool getOccupation() { return this.occupied; }
}
