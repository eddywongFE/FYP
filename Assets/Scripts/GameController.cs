using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // Unity Related Object
    public GameObject Board; // Reference to game board in Unity
    public GameObject PlayerTokenObj; // Reference to AIToken in Unity
    public GameObject AITokenObj; // Reference to PlayerToken in Unity 
    public GameObject[] spotList; // Reference to spot on the game board in Unity
    public Text Guidence;
    public GameObject PopUp;
    public GameObject MonsterCombat;
    public GameObject BossCombat;
    
    // Game Controll Variable
    private char currentTurn; // Indicate current playing side; 'W' for AI && 'B' for Player
    private int tokenCounter; // Counter for the total number of token placed on board
    private int phase; // Indicate current phase
    private string operataion; // Operation to be taken; "delete" or "move" a token
    private int millCounter; // Counter for the closed mill
    private AIBot AI;
    private GameStateController stateControl;

    // Game Related Logic Variable
    private char[] boardStatus; // The board status; 'N' for empty, 'W' for white token, 'B' for black token
    private GameObject[] tokenObjList; // References to all Game Objects base on board status
    private ArrayList PlayerTokenList; // Reference to White Token
    private ArrayList AITokenList; // Reference to Black Token
    private GameObject currentToken; // Selected token
    private bool[] millStatus = new bool[16]; // List for checking closed mill
    private int[][] possibleMill = new int[16][] {
        new int[3] {0, 1, 2}, new int[3] {5, 6, 7}, new int[3] {0, 3, 5}, new int[3] {2, 4, 7},
        new int[3] {8, 9, 10}, new int[3] {13, 14, 15}, new int[3] {8, 11, 13}, new int[3] {10, 12, 15},
        new int[3] {16, 17, 18}, new int[3] {21, 22, 23}, new int[3] {16, 19, 21}, new int[3] {18, 20, 23},
        new int[3] {1, 9, 17}, new int[3] {6, 14, 22}, new int[3] {3, 11, 19}, new int[3] {4, 12, 20}
    }; // List for possible mill position
    
    // Initiate function when game started
    void Awake() {
        // Constructions
        currentTurn = 'W';
        tokenCounter = 0;
        phase = 1;
        operataion = "none";

        PlayerTokenList = new ArrayList(9) ;
        AITokenList = new ArrayList(9);
        boardStatus = new char[spotList.Length];
        tokenObjList = new GameObject[spotList.Length];

        AI = AIBot.getInstance();
        AI.SetGameControllerReferences(this);

        stateControl = GameObject.FindGameObjectWithTag("state").GetComponent<GameStateController>();
        
        if (stateControl.GetCurrentEvent() == "Monster") {
            MonsterCombat.SetActive(true);
        } else if (stateControl.GetCurrentEvent() == "Boss") {
            BossCombat.SetActive(true);
        }

        for (int i = 0; i < spotList.Length; i++) {
            spotList[i].GetComponent<Spot>().SetGameControllerReferences(this);
            boardStatus[i] = 'N';
        }

        StartGame();
    }

    void StartGame() {
        phaseOne();
    }

    // Placing Phase
    private void phaseOne() {
        currentToken = CreateToken(currentTurn);
        Guidence.text = "Place Your Token on Board";

        GetAvailableSpot();
        if (currentTurn == 'B') {
            bool difficult = (stateControl.GetCurrentEvent() == "Boss") ? true : false;
            AI.SelectPlacingSpot(boardStatus, phase, tokenCounter, currentTurn, difficult);
        }
    }

    // Instantiate Game Object in Unity
    private GameObject CreateToken(char side) {
        GameObject token = Instantiate(side == 'W' ? PlayerTokenObj : AITokenObj);
        token.transform.SetParent(Board.transform);
        token.transform.localPosition = Vector3.zero;
        token.transform.localScale = Vector3.one;
        token.GetComponent<Button>().interactable = false;
        token.GetComponent<Token>().SetGameControllerReferences(this);

        if (side == 'W') {
            PlayerTokenList.Add(token);
        } else {
            AITokenList.Add(token);
        }

        return token;
    }

     // Get Available for token to be placed
    public void GetAvailableSpot() {
        ArrayList available = new ArrayList();

        if (phase == 1) {
            for (int i = 0; i < spotList.Length; i++) {
                if (spotList[i].GetComponent<Spot>().getOccupation() == false) {
                    available.Add(spotList[i]);
                }
            }
        } else if (phase == 2) {
            int index = Array.IndexOf(tokenObjList, currentToken);
            int[] nodeList = GetAdjacentNode(index);

            for (int i = 0; i < nodeList.Length; i++) {
                if (spotList[nodeList[i]].GetComponent<Spot>().getOccupation() == false) {
                    available.Add(spotList[nodeList[i]]);
                }
            }
        } else {
            int index = Array.IndexOf(tokenObjList, currentToken);
            int[] nodeList = GetAdjacentNode(index);
            ArrayList targetList = (boardStatus[index] == 'W') ? PlayerTokenList : AITokenList;

            if (targetList.Count == 3) {
                for (int i = 0; i < spotList.Length; i++) {
                    if (spotList[i].GetComponent<Spot>().getOccupation() == false) {
                        available.Add(spotList[i]);
                    }
                }
            } else {
                for (int i = 0; i < nodeList.Length; i++) {
                    if (spotList[nodeList[i]].GetComponent<Spot>().getOccupation() == false) {
                        available.Add(spotList[nodeList[i]]);
                    }
                }
            }
        }

        foreach (GameObject spot in available) {
            spot.GetComponent<Button>().interactable = true;
        };
    }

    public int[] GetAdjacentNode(int index) {
        int[][] adjacent = new int[24][] {
            new int[2] {1, 3}, new int[3] {0, 2, 9}, new int[2] {1, 4}, new int[3] {0, 5, 11},
            new int[3] {2, 7, 12}, new int[2] {3, 6}, new int[3] {5, 7, 14}, new int[2] {4, 6},
            new int[2] {9, 11}, new int[4] {1, 8, 10, 17}, new int[2] {9, 12}, new int[4] {3, 8, 13, 19},
            new int[4] {4, 10, 15, 20}, new int[2] {11, 14}, new int[4] {6, 13, 15, 22}, new int[2] {12, 14},
            new int[2] {17, 19}, new int[3] {9, 16, 18}, new int[2] {17, 20}, new int[3] {11, 16, 21},
            new int[3] {12, 18, 23}, new int[2] {19, 22}, new int[3] {14, 21, 23}, new int[2] {20, 22}
        };

        return adjacent[index];
    }

    // Set token status
    public void SetTokenList(int prev, int next) {
        if (prev != -1) {
            boardStatus[prev] = 'N';
            tokenObjList[prev] = null;
            spotList[prev].GetComponent<Spot>().setOccupation(false);
        }

        boardStatus[next] = currentTurn;
        tokenObjList[next] = currentToken;
        spotList[next].GetComponent<Spot>().setOccupation(true);
    }

    // Check if any new mill is closed
    public bool CheckChangedClosedMill() {
        bool different = false;
        bool[] temp = new bool[16];
        bool[] sample = (bool[])millStatus.Clone();

        // Update number of mill
        for (int x = 0; x < 16; x++) {
            if (boardStatus[possibleMill[x][0]] != 'N' &&
                boardStatus[possibleMill[x][0]] == boardStatus[possibleMill[x][1]] &&
                boardStatus[possibleMill[x][0]] == boardStatus[possibleMill[x][2]]) {
                if (sample[x] == false) {
                    different = true;
                }

                temp[x] = true;
                tokenObjList[possibleMill[x][0]].GetComponent<Token>().ClosingMill(true);
                tokenObjList[possibleMill[x][1]].GetComponent<Token>().ClosingMill(true);
                tokenObjList[possibleMill[x][2]].GetComponent<Token>().ClosingMill(true);
            }
        }

        millStatus = temp;
        return different;
    }

    // Get token that is allow to remove
    public void SelectedRemoveableToken() {
        SetBoardInteractable(false);
        SetAllTokenInteractable(false);
        Guidence.text = "Select a Token to Remove";

        operataion = "delete";

        if (currentTurn == 'B') {
            int tokenIndex = AI.GetRemovingToken();
            tokenObjList[tokenIndex].GetComponent<Token>().Selected();
        } else {
            ArrayList targetList = (currentTurn == 'W') ? AITokenList : PlayerTokenList;
            char targetSide = getOppositeSide(currentTurn);
        
            if (AllInMill(targetSide) == true) { // Situation 1: all opponent's tokens are in a closed mill
                SetTokenInteractable(targetList, true);
            } else { // Situation 2: not all or none of opponent's tokens are in a closed mill
                foreach (GameObject token in targetList) {
                    if (CheckTokenInMill(token) == false) {
                        token.GetComponent<Button>().interactable = true;
                    }
                }
            }
        }
    }

    // Check if all token are in a closed mill
    private bool AllInMill(char side) {
        ArrayList targetList = (side == 'W') ? PlayerTokenList : AITokenList;
        foreach (GameObject token in targetList) {
            if (CheckTokenInMill(token) == false) {
                return false;
            }
        }

        return true;
    }

    // Check if a token in a closed mill
    private bool CheckTokenInMill(GameObject token) {
        return token.GetComponent<Token>().getClosed();
    }

    // Remove a token object from the board
    public void RemoveToken(GameObject token) {
        int index = Array.IndexOf(tokenObjList, token);
        ArrayList targetList = (currentTurn == 'W') ? AITokenList : PlayerTokenList;

        if (index >= 0) {
            boardStatus[index] = 'N';
            targetList.Remove(token);
            tokenObjList[index] = null;
            spotList[index].GetComponent<Spot>().setOccupation(false);

            GameObject.Destroy(token);

            CheckChangedClosedMill();
        }

        SwitchTurn();
    }

    // Switch turn
    public void SwitchTurn() {
        if (phase > 1 && CheckWinningCondition() != 'N') {
            SetBoardInteractable(false);
            SetAllTokenInteractable(false);
            SetTokenClosedValue(false);
            Guidence.text = (CheckWinningCondition() == 'B') ? "You Lose!" : "You Win!";
            
            if (CheckWinningCondition() == 'W') {
                stateControl.UpdateBossHealth(-1);
            } else {
                stateControl.UpdatePlayerHealth(-1);
            }

            this.MonsterCombat.SetActive(false);
            this.BossCombat.SetActive(false);
            this.PopUp.SetActive(true);
        } else {
            operataion = "none";
            SetBoardInteractable(false);
            SetAllTokenInteractable(false);
            SetTokenClosedValue(false);
            SetBoardOccupation();
            
            currentTurn = getOppositeSide(currentTurn);

            if (phase == 1) {
                tokenCounter++;
                if (tokenCounter >= 18) {
                    phase = 2;
                    currentToken = null;
                    SetBoardInteractable(false);
                    phaseTwoAndThree();
                } else {
                    phaseOne();
                }
            } else if (phase == 2) {
                if (PlayerTokenList.Count == 3 || AITokenList.Count == 3) {
                    phase = 3;
                }
                phaseTwoAndThree();
            } else {
                phaseTwoAndThree();
            }
        }
    }

    // Check whether the game is terminated
    private char CheckWinningCondition() {
        char winner = 'N';
        if (PlayerTokenList.Count <= 2) {
            winner = 'B';
        } else if (AITokenList.Count <= 2) {
            winner = 'W';
        } else if (CheckNoPossibleMove() != 'N') {
            winner = CheckNoPossibleMove();
        }

        return winner;
    }

    // Check if current player has legal move
    private char CheckNoPossibleMove() {
        bool check = true;
        ArrayList targetList = (currentTurn == 'W') ? AITokenList : PlayerTokenList;
        if (phase == 3 && targetList.Count == 3) {
            check = false;
        } else {
            foreach (GameObject token in targetList) {
                int index = Array.IndexOf(tokenObjList, token);
                int[] nodeList = GetAdjacentNode(index);

                for (int i = 0; i < nodeList.Length; i++) {
                    if (spotList[nodeList[i]].GetComponent<Spot>().getOccupation() == false) {
                        check = false;
                        break;
                    }
                }

                if (!check) break;
            }
        }

        return (check) ? currentTurn : 'N';
    }

    // Mid-game and end-game
    private void phaseTwoAndThree() {
        operataion = "move";
        Guidence.text = "Select a Token to Move";
        ArrayList targetList = (currentTurn == 'W') ? PlayerTokenList : AITokenList;

        SetTokenInteractable(targetList, true);
        if (currentTurn == 'B') {
            bool difficult = (stateControl.GetCurrentEvent() == "Boss") ? true : false;
            AI.SelectPlacingSpot(boardStatus, phase, tokenCounter, currentTurn, difficult);
        }
    }

    // Set reference to current selected token
    public void SetCurrentToken(int index) {
        this.currentToken = tokenObjList[index];
    }

    // Move token to the respective spot on board
    public void MoveToken(GameObject token) {
        Guidence.text = "Select Your Destination";
        SetBoardInteractable(false);
        this.currentToken = token;

        GetAvailableSpot();
    }


    // Set the interactable value of a Spot Object on the board
    private void SetBoardInteractable(bool toggle) {
        for (int i = 0; i < spotList.Length; i++) {
            spotList[i].GetComponent<Button>().interactable = false;
        }
    }

    // Set the interactable value of a Token Object on the board
    private void SetTokenInteractable(ArrayList targetList, bool toggle) {
        foreach (GameObject token in targetList) {
            token.GetComponent<Button>().interactable = toggle;
        }
    }

    private void SetAllTokenInteractable(bool toggle) {
        foreach (GameObject token in PlayerTokenList) {
            token.GetComponent<Button>().interactable = toggle;
        }
        foreach (GameObject token in AITokenList) {
            token.GetComponent<Button>().interactable = toggle;
        }
    }
    
    // Set the closed value of all token on the board
    private void SetTokenClosedValue(bool toggle) {
        foreach (GameObject token in PlayerTokenList) {
            token.GetComponent<Token>().ClosingMill(toggle);
        }
        foreach (GameObject token in AITokenList) {
            token.GetComponent<Token>().ClosingMill(toggle);
        }
    }
    
    private void SetBoardOccupation() {
        for (int i = 0; i < boardStatus.Length; i++) {
            spotList[i].GetComponent<Spot>().setOccupation((boardStatus[i] != 'N') ? true : false);
        }
    }

    public char getOppositeSide(char current) { return ((current == 'W') ? 'B' : 'W'); }

    public int getPhase() { return phase; }
    public char getTurn() { return currentTurn; }
    public GameObject getCurrentToken() { return currentToken; }
    public string getOperation() { return operataion; }
    public int getSpotIndex(GameObject item) { return Array.IndexOf(spotList, item); } // Get Index of spot in the spotList
    public int getTokenIndex(GameObject token) { return Array.IndexOf(tokenObjList, token); } // Get Index of token on the board
    public int[][] getPossibleMill() { return possibleMill; }

    // Return to the map scene
    public void BackToMap() {
        stateControl.SetCurrentEvent(-1, "");
        SceneManager.LoadScene(1);
    }
}