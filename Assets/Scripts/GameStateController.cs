using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameState // Singleton class containing game state
{
    private static GameState instance; // Instance
    private bool Started; // Check if the event is newly started
    private int PlayerHealth; // Player's health point
    private int BossHealth; // Boss's health point
    private Vector3 PlayerPos; // Player position on map
    private string ItemInUse; // Used item
    private string[] Events; // Event list on map
    private ArrayList Completed; // Completed event
    
    // Constructor
    private GameState() {
        this.Started = false;
        this.PlayerHealth = 5;
        this.BossHealth = 8;
        this.PlayerPos = Vector3.zero;
        this.ItemInUse = "empty";
        this.Events = new string[9];
        this.Completed = new ArrayList();
    }
    
    // Get instance to the singleton class
    public static GameState GetInstance() {
        if (GameState.instance == null) {
            GameState.instance = new GameState();
        }

        return GameState.instance;
    }

    public void SetStarted(bool started) {
        this.Started = started;
    }

    public void SetPlayerHealth(int health) {
        if (health > 5) {
            health = 5;
        }

        this.PlayerHealth = health;
    }

    public void SetBossHealth(int health) {
        this.BossHealth = health;
    }

    public void SetPlayerPosition(Vector3 pos) {
        this.PlayerPos = pos;
    }

    public void SetItemInUse(string item) {
        this.ItemInUse = item;
    }

    public void SetEventList(string[] events) {
        this.Events = events;
    }

    public void AddCompletedEvent(int index) {
        this.Completed.Add(index);
    }

    // Reset GameState
    public void ResetState() {
        this.Started = false;
        this.PlayerHealth = 5;
        this.BossHealth = 8;
        this.PlayerPos = Vector3.zero;
        this.ItemInUse = "empty";
        this.Events = new string[9];
        this.Completed = new ArrayList();
    }

    public bool GetStarted() { return this.Started; }
    public int GetPlayerHealth() { return this.PlayerHealth; }
    public int GetBossHealth() { return this.BossHealth; }
    public Vector3 GetPlayerPos() { return this.PlayerPos; }
    public string GetItemInUse() { return this.ItemInUse; }
    public string[] GetEventList() { return this.Events; }
    public ArrayList GetCompleted() { return this.Completed; }
}

public class GameStateController : MonoBehaviour
{   
    public GameObject Map; // Reference to map object in Unity
    public GameObject Player; // Reference to player object in Unity
    public GameObject Monster; // Reference to "Fight" event in Unity
    public GameObject Potion; // Reference to "Heal" event in Unity
    public GameObject Treasure; // Reference to "Treasure" event in Unity
    public GameObject Boss; // Reference to "Boss" event in Unity
    public Sprite[] PossibleItemList; // Reference to item generate by "Treasure" event
    public GameObject[] SpawnList; // Event spawn positions
    public HealthBarControl PlayerHealth; // Reference to player health bar
    public HealthBarControl BossHealth; // Reference to boss health bar

    // References to popup dialogue for different events
    public GameObject completedPopup;
    public GameObject monsterConfirmation;
    public GameObject potionConfirmation;
    public GameObject treasureConfirmation;
    public GameObject treasureResult;

    private GameState state = GameState.GetInstance(); // Reference to GameState singleton
    private GameObject[] EventObjList = new GameObject[9]; // List of randomized event
    private int currentEventIndex = -1; // Current event index on EventObjList
    private string currentEventName = ""; // Current event name
    private string[] itemName = new string[2] { // Possible items of "Treasure"
        "Sword", "Bomb"
    };
    private string[] itemDescription = new string[2] { // Description for items
        "Double the damage in Next Combat",
        "Deduct 1 Health Point from the boss"
    };

    // Init function when game started
    void Awake() {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("state");
        if (objs.Length > 1) {
            Destroy(objs[0]);
        }

        DontDestroyOnLoad(this.gameObject);
        StartGame();
    }

    // Update game state when start game
    private void StartGame() {
        if (this.state.GetBossHealth() <= 0) {
            this.completedPopup.GetComponentsInChildren<Text>()[0].text = "YOU WIN!!";
            this.completedPopup.SetActive(true);
        } else if (this.state.GetPlayerHealth() <= 0) {
            this.completedPopup.GetComponentsInChildren<Text>()[0].text = "YOU LOSE!!";
            this.completedPopup.SetActive(true);
        }

        if (this.state.GetStarted() == false) {
            this.RandomizeEvent();
        } else {
            Vector3 pos = this.state.GetPlayerPos();
            this.Player.transform.position = pos;

            this.PlayerHealth.SetHealth(this.state.GetPlayerHealth());
            this.BossHealth.SetHealth(this.state.GetBossHealth());
        }

        this.Boss.GetComponent<BoxCollider2D>().isTrigger = true;
        string[] eventList = this.state.GetEventList();
        ArrayList completed = this.state.GetCompleted();
        for (int i = 0; i < this.SpawnList.Length; i++) {
            if (!completed.Contains(i)) {
                GameObject obj = this.CreateEvent(eventList[i], i);
                Vector3 pos = this.SpawnList[i].transform.position;

                obj.transform.SetParent(this.Map.transform);
                obj.transform.position = pos;

                this.EventObjList[i] = obj;
            } else {
                SpawnList[i].GetComponent<BoxCollider2D>().isTrigger = true;
            }
        }

        this.Boss.GetComponent<BoxCollider2D>().isTrigger = false;
        this.Boss.GetComponent<Event>().SetGameStateController(this);
        this.Boss.GetComponent<Event>().SetEventNumber(9);
    }

    // Create event
    private GameObject CreateEvent(string eventType, int index) {
        GameObject obj;
        if (eventType == "Monster") {
            obj = Instantiate(Monster);
        } else if (eventType == "Potion") {
            obj = Instantiate(Potion);
        } else {
            obj = Instantiate(Treasure);
        }

        obj.GetComponent<Event>().SetGameStateController(this);
        obj.GetComponent<Event>().SetEventNumber(index);

        return obj;
    }

    // Random event list
    private void RandomizeEvent() {
        string[] eventList = new string[9] {
            "Monster", "Potion", "Treasure", "Monster", "Monster", "Monster", "Potion", "Treasure", "Treasure"
        };

        System.Random rand = new System.Random();
        for (int i = 1; i < eventList.Length - 1; i++) {
            int j = rand.Next(i, eventList.Length);
            string temp = eventList[i];
            eventList[i] = eventList[j];
            eventList[j] = temp;
        }

        this.state.SetEventList(eventList);
        this.state.SetStarted(true);
    }

    // Set current triggering event
    public void SetCurrentEvent(int index, string name) {
        this.currentEventIndex = index;
        this.currentEventName = name;
    }
    
    // Control popup dialogue
    public void popup(string name) {
        if (name == "Boss" && this.state.GetCompleted().Contains(8)) {
            this.monsterConfirmation.SetActive(true);
        }
        if (name == "Monster") {
            this.monsterConfirmation.SetActive(true);
        } else if (name == "Potion") {
            this.potionConfirmation.SetActive(true);
        } else if (name == "Treasure") {
            this.treasureConfirmation.SetActive(true);
        }
    }

    // Control the function triggering when confirmed event popup
    public void confirmDialogue() {
        if (currentEventName == "Boss") {
            this.UpdatePlayerPosition();
            SceneManager.LoadScene(2);
        } else if (currentEventName == "Monster") {
            this.UpdateCompleted(this.currentEventIndex);
            SceneManager.LoadScene(2);
        } else if (currentEventName == "Potion") {
            this.UpdateCompleted(this.currentEventIndex);
            this.UpdatePlayerHealth(1);
            this.SetCurrentEvent(-1, "");
        } else if (currentEventName == "Treasure") {
            this.UpdateCompleted(this.currentEventIndex);
            this.GetRandomItem();
        }

        this.dismissDialogue();
    }

    // Dismiss dialogue without callback return
    public void dismissDialogue() {
        this.monsterConfirmation.SetActive(false);
        this.potionConfirmation.SetActive(false);
        this.treasureConfirmation.SetActive(false);
    }

    // Generate random item when "Treasure" event trigger
    private void GetRandomItem() {
        this.treasureResult.SetActive(true);
        System.Random rand = new System.Random();
        int itemNum = rand.Next(0, this.PossibleItemList.Length);

        this.treasureResult.GetComponentsInChildren<Image>()[1].sprite = this.PossibleItemList[itemNum];
        this.treasureResult.GetComponentsInChildren<Text>()[1].text = this.itemDescription[itemNum];

        this.state.SetItemInUse(this.itemName[itemNum]);
        this.SetCurrentEvent(-1, "");
    }

    // Use item
    public void UseItem() {
        this.treasureResult.SetActive(false);
        string item = this.state.GetItemInUse();

        if (item == "Bomb") {
            this.UpdateBossHealth(-1);
            this.state.SetItemInUse("empty");
        }
    }

    // Add the current triggered event to completed event list
    public void UpdateCompleted(int index) {
        this.state.AddCompletedEvent(index);
        this.UpdatePlayerPosition();

        this.SpawnList[index].GetComponent<BoxCollider2D>().isTrigger = true;
        Destroy(this.EventObjList[index]);
    }

    // Update player position on map
    public void UpdatePlayerPosition() {
        Vector3 pos = this.Player.transform.position;
        this.state.SetPlayerPosition(pos);
    }

    // Update player's health bar
    public void UpdatePlayerHealth(int change) {
        int health = this.state.GetPlayerHealth() + change;

        health = (health > 5) ? 5 : health;
        this.state.SetPlayerHealth(health);
        this.PlayerHealth.SetHealth(health);
    }

    // Update Boss's health bar
    public void UpdateBossHealth(int change) {
        if (this.state.GetItemInUse() == "Sword") {
            change *= 2;
            this.state.SetItemInUse("empty");
        }

        int health = this.state.GetBossHealth() + change;
        this.state.SetBossHealth(health);
        this.BossHealth.SetHealth(health);
    }

    // Return to main menu
    public void exitGame() {
        SceneManager.LoadScene(0);
        this.state.ResetState();
        GameObject storedData = GameObject.FindGameObjectWithTag("state");
        Destroy(storedData);
    }

    public string GetCurrentEvent() { return this.currentEventName; }
}
