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
    public bool isInEnd = false;
    void Start()
    {
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isInEnd && navMeshAgent.velocity.magnitude <= 0.1f)
        {
            navMeshAgent.enabled = false;
        }
        animator.SetBool(isRunning, navMeshAgent.velocity.magnitude >= 0.1f);
    }
}
