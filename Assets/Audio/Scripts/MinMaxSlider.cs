#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class MinMaxSlider : EditorWindow
{
    float minVal = -10;
    float minLimit = -20;
    float maxVal = 10;
    float maxLimit = 20;

    [MenuItem("Examples/Place Object Randomly")]
    static void Init()
    {
        MinMaxSlider window = (MinMaxSlider)GetWindow(typeof(MinMaxSlider));
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Min Val:", minVal.ToString());
        EditorGUILayout.LabelField("Max Val:", maxVal.ToString());
        EditorGUILayout.MinMaxSlider(ref minVal, ref maxVal, minLimit, maxLimit);
        if (GUILayout.Button("Move!"))
            PlaceRandomly();
    }

    void PlaceRandomly()
    {
        if (Selection.activeTransform)
            Selection.activeTransform.position =
                new Vector3(Random.Range(minVal, maxVal),
                    Random.Range(minVal, maxVal),
                    Random.Range(minVal, maxVal));
        else
            Debug.LogError("Select a GameObject to randomize its position.");
    }
}
#endif