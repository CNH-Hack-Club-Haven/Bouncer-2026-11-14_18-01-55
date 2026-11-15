using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

[RequireComponent(typeof(LineDrawer), typeof(NavMeshAgent), typeof(PlayerMovement))]
public class PullBack : MonoBehaviour
{
    public float countdownTime = 10f;
    public float pullSpeed = 15f;
    public TMP_Text countdownText;

    public float RemainingRatio => countdownTime > 0f ? Mathf.Clamp01(timer / countdownTime) : 1f;

    private LineDrawer lineDrawer;
    private NavMeshAgent agent;
    private PlayerMovement movement;
    private float timer;

    private void Awake()
    {
        lineDrawer = GetComponent<LineDrawer>();
        agent = GetComponent<NavMeshAgent>();
        movement = GetComponent<PlayerMovement>();
        agent.enabled = false;
        ResetTimer();
    }

    private void Update()
    {
        if (!lineDrawer.basePoint || movement.isBeingPulled) return;

        timer -= Time.deltaTime;
        UpdateUI();

        if (timer <= 0f)
            StartCoroutine(PullRoutine());
    }

    public void ResetTimer()
    {
        if (movement.isBeingPulled) return;
        timer = countdownTime;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (countdownText)
            countdownText.text = "TimeTill Pull: " + Mathf.CeilToInt(Mathf.Max(0f, timer));
    }

    private IEnumerator PullRoutine()
    {
        movement.isBeingPulled = true;
        movement.rb.linearVelocity = Vector3.zero;
        movement.rb.isKinematic = true;

        agent.enabled = true;
        agent.speed = pullSpeed;
        agent.acceleration = 100f;
        agent.SetDestination(lineDrawer.basePoint.position);
        yield return null;

        float timeout = 5f;
        while (agent.enabled && timeout > 0f && (agent.pathPending || agent.remainingDistance > 0.5f))
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        agent.enabled = false;
        movement.rb.isKinematic = false;
        movement.isBeingPulled = false;
        ResetTimer();
    }
}
