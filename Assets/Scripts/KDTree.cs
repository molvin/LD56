using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KDNode<T>
{
    public T Value;
    public Vector3 Position;
    public KDNode<T> Left = null;
    public KDNode<T> Right = null;

    public KDNode((T value, Vector3 position) _)
    {
        Value = _.value;
        Position = _.position;
    }
}

public class KDTree<T>
{
    public const int K = 3;

    private KDNode<T> root;
    private PriorityQueue<KDNode<T>> nearestNeighbours;

    public KDTree(List<(T, Vector3)> points)
    {
        root = BuildTree(points);
    }

    public List<T> NearestNeighbours(Vector3 target, int num)
    {
        nearestNeighbours = new();
        NeighbourSearch(root, target, 0, num);

        List<T> neighbours = new();
        while (nearestNeighbours.Count > 0)
        {
            neighbours.Add(nearestNeighbours.Dequeue().Value);
        }
        return neighbours;
    }

    private void NeighbourSearch(KDNode<T> node, Vector3 target, int depth, int num)
    {
        if (node == null) return;

        int axis = depth % K;
        double distance = Vector3.Distance(target, node.Position);

        if (nearestNeighbours.Count < num)
        {
            nearestNeighbours.Enqueue(node, -distance);
        }
        else if (distance < -nearestNeighbours.PeekPriority())
        {
            nearestNeighbours.Dequeue();
            nearestNeighbours.Enqueue(node, -distance);
        }

        var next = target[axis] < node.Position[axis] ? node.Left : node.Right;
        var other = target[axis] < node.Position[axis] ? node.Right : node.Left;

        NeighbourSearch(next, target, depth + 1, num);

        if (Mathf.Abs(target[axis] - node.Position[axis]) < -nearestNeighbours.PeekPriority())
        {
            NeighbourSearch(other, target, depth + 1, num);
        }
    }
    
    private KDNode<T> BuildTree(List<(T, Vector3)> points, int depth = 0)
    {
        if (points == null || points.Count == 0) return null;

        int axis = depth % K;
        points = points.OrderBy(x => x.Item2[axis]).ToList();

        int median = points.Count / 2;
        var node = new KDNode<T>(points[median]);
        node.Left = BuildTree(points.Take(median).ToList(), depth + 1);
        node.Right = BuildTree(points.Skip(median + 1).ToList(), depth + 1);

        return node;
    }

    public static void Test()
    {
        List<(string, Vector3)> value = new()
        {
            ("A", new(2.1f, -3.5f, 1.2f)),
            ("B", new(5.4f, 4.1f, 3.3f)),
            ("C", new(-9.2f, 6.3f, -4.1f)),
            ("D", new(4.0f, 7.0f, -5.6f)),
            ("E", new(8.1f, -1.3f, -2.4f)),
            ("F", new(7.4f, 2.5f, 6.5f))
        };

        int numPoints = 5000;
        for (int i = 0; i < numPoints; i++)
        {
            Vector3 vec = Random.insideUnitSphere * Random.value * 10.0f;
            value.Add(($"{i}", vec));
        }

        System.Diagnostics.Stopwatch sw = new();
        sw.Start();

        for (int i = 0; i < numPoints; i++)
        {
            Vector3 vec = value[i].Item2;
            List<(string, Vector3)> nearest = value.OrderBy(x => Vector3.Distance(vec, x.Item2)).ToList();
        }

        sw.Stop();
        System.TimeSpan ts = sw.Elapsed;
        Debug.Log($"Sorting: {ts.TotalMilliseconds}");
        for (int i = 0; i < 3; i++)
        {
            //Debug.Log(nearest[i].Item1);
        }

        sw = new();
        sw.Start();

        KDTree<string> tree = new(value);
        for (int i = 0; i < numPoints; i++)
        {
            Vector3 vec = value[i].Item2;
            List<string> knn = tree.NearestNeighbours(vec, 3);
        }

        sw.Stop();
        ts = sw.Elapsed;
        Debug.Log($"Tree: {ts.TotalMilliseconds}");
        //foreach (var s in knn)
        {
            //Debug.Log(s);
        }
    }
}
