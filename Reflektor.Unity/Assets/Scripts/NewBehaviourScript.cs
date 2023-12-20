using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NewBehaviourScript : MonoBehaviour
{
    ListView l;
    ListView c;
    List<string> list = new();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 100; i++) {
            list.Add(i.ToString());
        }

        var uiDocument = GetComponent<UIDocument>();
        l = uiDocument.rootVisualElement.Q<ListView>(name: "ObjectList");
        l.itemsSource = list;
        l.Rebuild();

        c = uiDocument.rootVisualElement.Q<ListView>(name: "ComponentList");
        c.itemsSource = list;
        c.Rebuild();

        var path = uiDocument.rootVisualElement.Q<TextField>(name: "PathInput");
        path.isDelayed = false;
        path.RegisterValueChangedCallback(evt =>
        {
            float divisor = evt.newValue.Length / 78.0f;
            if (divisor != 0)
                path.style.fontSize = System.Math.Clamp(14 / divisor, 10, 14);
            else
                path.style.fontSize = 14;
        });
    }

    // Update is called once per frame
    void Update()
    {
    }
}
