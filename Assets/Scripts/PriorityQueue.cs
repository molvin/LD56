using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class PriorityQueue<T>
{
    private List<(T item, double priority)> heap = new();
    public int Count => heap.Count;

    public void Enqueue(T item, double priority)
    {
        heap.Add((item, priority));
        HeapifyUp(heap.Count - 1);
    }

    public T Dequeue()
    {
        T item = heap[0].item;

        heap[0] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);

        if (heap.Count > 0)
        {
            HeapifyDown(0);
        }

        return item;
    }

    public double PeekPriority()
    {
        return heap[0].priority;
    }

    public double PeekBackPriority()
    {
        return heap.Max(x => x.priority);   
    }

    private void HeapifyUp(int index)
    {
        int parent = (index - 1) / 2;
        if (index > 0 && heap[index].priority < heap[parent].priority)
        {
            Swap(index, parent);
            HeapifyUp(parent);
        }
    }

    private void HeapifyDown(int index)
    {
        int left = 2 * index + 1;
        int right = 2 * index + 2;
        int smallest = index;

        if (left < heap.Count && heap[left].priority < heap[smallest].priority)
        {
            smallest = left;
        }
        
        if (right < heap.Count && heap[right].priority < heap[smallest].priority)
        {
            smallest = right;
        }

        if (smallest != index)
        {
            Swap(index, smallest);
            HeapifyDown(smallest);
        }
    }

    private void Swap(int first, int second)
    {
        (heap[first], heap[second]) = (heap[second], heap[first]);
    }
    
    public static void Test()
    {
        PriorityQueue<string> queue = new();
        queue.Enqueue("Task 1", 2);
        queue.Enqueue("Task 2", 1);
        queue.Enqueue("Task 3", 3);
        queue.Enqueue("Task 4", 5);

        Assert.IsTrue(queue.Dequeue() == "Task 2");
        Assert.IsTrue(queue.Dequeue() == "Task 1");
        Assert.IsTrue(queue.Dequeue() == "Task 3");
        Assert.IsTrue(queue.Dequeue() == "Task 4");
    }
}
