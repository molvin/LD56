using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class MenuMinion : MonoBehaviour
{

    private string isRunning = "IsRunning";

    private Animator animator;
    private NavMeshAgent navMeshAgent;
    void Start()
    {
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool(isRunning, navMeshAgent.velocity.magnitude >= 0.1f);
    }
}
