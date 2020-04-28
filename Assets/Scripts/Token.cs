using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Token : MonoBehaviour
{
    public GameObject token;
    private bool closed;

    private GameController gameController;

    public void SetGameControllerReferences(GameController controller) {
        gameController = controller;
    }

    // Operation for selecting the token
    public void Selected() { 
        if (gameController.getOperation() == "delete") { // Delete token when closing mill
            gameController.RemoveToken(this.token);
        } else if (gameController.getOperation() == "move") {
            gameController.MoveToken(this.token);
        }
    }

    // Set true when Token is in mill
    public void ClosingMill(bool toggle) {
        this.closed = toggle;
    }

    public bool getClosed() { return this.closed; }
}
