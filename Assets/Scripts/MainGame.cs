using System;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class MainGame : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private float timeMinus;
    [SerializeField] private Text scoreTextUI;
    [SerializeField] private Text namenickTextUI;
    [SerializeField] private Image pasteNick;
    [SerializeField] private Button loadData;
    [SerializeField] private Button saveData;
    [SerializeField] private Button saveNick;

    private float lastTime;
    private int currentScore = 0;
    string name;
    DatabaseReference reference;
    bool changeScore = false;

    private void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://androidredclicker.firebaseio.com");
        reference = FirebaseDatabase.DefaultInstance.RootReference;

        lastTime = Time.time;
        camera.backgroundColor = Color.white;
        Invoke("RedMinus", timeMinus);

        saveNick.onClick.AddListener(HandleOnSaveNick);
        loadData.onClick.AddListener(HandleOnLoadData);
        saveData.onClick.AddListener(HandleOnSaveData);
    }

    private void HandleOnSaveNick()
    {
        if (namenickTextUI.text == "")
            return;

        name = namenickTextUI.text;
        pasteNick.gameObject.SetActive(false);
    }

    private void HandleOnSaveData()
    {
        reference.Child("users").Child(name).SetValueAsync(currentScore);
    }

    private void HandleOnLoadData()
    {
        FirebaseDatabase.DefaultInstance.GetReference("users").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("ошибка");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                snapshot = snapshot.Child(name);
                Int32.TryParse(snapshot.GetValue(true).ToString(), out currentScore);
                changeScore = true;
            }
        });
    }

    private void RedMinus()
    {
        var green = camera.backgroundColor.g;
        var blue = camera.backgroundColor.b;

        if (green < 1 && blue < 1)
            camera.backgroundColor = new Color(1, green + 0.01f, blue + 0.01f);

        Invoke("RedMinus", timeMinus);
    }

    private void RedPlus(float force)
    {
        var green = camera.backgroundColor.g;
        var blue = camera.backgroundColor.b;

        if (green > 0.1f && blue > 0.1f)
            camera.backgroundColor = new Color(1, green - force, blue - force);
    }

    private void Update()
    {
        if (changeScore)
        {
            scoreTextUI.text = currentScore.ToString();
            changeScore = false;
        }

        Touch touch = new Touch();
        if(touch.tapCount == 1 || Input.GetMouseButtonDown(0))
        {
            var force = (Time.time - lastTime);
            lastTime = Time.time;
            RedPlus(1/force/70);

            var green = camera.backgroundColor.g;
            int point = (int)((1 - green) * 5);
            currentScore += point;
            scoreTextUI.text = currentScore.ToString();
        }
    }
}
