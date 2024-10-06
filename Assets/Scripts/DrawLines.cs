using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLines
{
    private static DrawLines instance;

    public static DrawLines Get
    {
        get
        {
            if (instance == null)
            {
                instance = new();
                GameObject go = new GameObject("Draw Lines");
                instance.holder = go.AddComponent<CoroutineRunner>();
            }

            return instance;
        }
    }

    private CoroutineRunner holder;
    private List<LineRenderer> inactiveRenderers = new();

    public void DrawLine(Vector3 start, Vector3 end, float duration, float size, Color color)
    {
        if (inactiveRenderers.Count == 0)
            inactiveRenderers.Add(holder.gameObject.AddComponent<LineRenderer>());

        LineRenderer ren = inactiveRenderers[inactiveRenderers.Count - 1];
        inactiveRenderers.RemoveAt(inactiveRenderers.Count - 1);

        ren.startWidth = size;
        ren.endWidth = size;
        ren.startColor = color;
        ren.endColor = color;

        holder.StartCoroutine(Draw(ren, start, end, duration));
    }

    public void DrawMultipleLines(List<(Vector3 start, Vector3 end)> lines, float duration)
    {

    }

    public IEnumerator Draw(LineRenderer ren, Vector3 start, Vector3 end, float duration)
    {
        ren.positionCount = 2;
        ren.SetPosition(0, start);
        ren.SetPosition(1, end);

        yield return new WaitForSeconds(duration);

        ren.positionCount = 0;
        inactiveRenderers.Add(ren);
    }
}
