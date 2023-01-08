using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LilSkeleton : MonoBehaviour, IDamagable
{
    NavMeshAgent agent;
    StateTree stateTree;

    [SerializeField] float AttackPower = 0;

    [SerializeField] HealthSubsystem healthSubsystem;


    float timer = .51f;
    bool isTimerRun;
    const float limitTimer = .5f; 

    public void Damage(float damage, Vector2 dir)
    {
        healthSubsystem.Damage(damage);
        PushSelf(damage, dir);
    }

    private void Start()
    {
        stateTree = LilSkeletonStateTree.Init(gameObject);
        healthSubsystem.Start(gameObject);
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Update()
    {
        stateTree.SetData("EnemyFind", Physics2D.OverlapCircle(transform.position,2.5f, LayerMask.GetMask("Enemy")) != null);

        stateTree.Execution();

        timer += Time.deltaTime;

        var target = Physics2D.OverlapCircle(transform.position, .7f, LayerMask.GetMask("Enemy"));
        if (target != null)
        {
            if(timer > limitTimer)
            {
                Vector2 dir;
                switch ((LilSkeletonStateTree.States)stateTree.GetCurrState())
                {
                    case LilSkeletonStateTree.States.kCombat:
                        dir = target.transform.position - transform.position;
                        dir = dir.normalized;
                        target.gameObject.GetComponent<Enemy>().Damage(AttackPower, dir);
                        stateTree.SetData("IsAttack", true);
                        PushSelf(.2f, -dir);
                        break;
                    case LilSkeletonStateTree.States.kIdle:
                        dir = transform.position - target.transform.position;
                        dir = dir.normalized;
                        Damage(target.gameObject.GetComponent<Enemy>().attackPower, dir);
                        target.gameObject.GetComponent<Enemy>().PushSelf(.2f, -dir);
                        break;

                }
                timer = 0;
            }
        }
        

        
    }
    

    public void PushSelf(float amount, Vector2 dir)
    {
        agent.ResetPath();
        agent.velocity = amount * dir;
    }
}
