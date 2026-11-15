using System.Collections;
using UnityEngine;
using TMPro; // Required for TextMeshPro

[RequireComponent(typeof(LineDrawer))]
public class PullBack : MonoBehaviour
{
    [Header("Settings")]
    public float countdownTime = 10f;
    public float pullSpeed = 15f;

    [Header("UI")]
    public TMP_Text countdownText;

    private LineDrawer lineDrawer;
    private float timer;
    private bool isCountingDown = false;

    private void Awake()
    {
        lineDrawer = GetComponent<LineDrawer>();
        timer = countdownTime;
        UpdateTimerText();
    }

    private void Update()
    {
        // Start countdown automatically once tethered
        if (lineDrawer.basePoint != null && !isCountingDown && !lineDrawer.isPullingBack)
        {
            ResetTimer();
        }

        if (isCountingDown)
        {
            timer -= Time.deltaTime;
            UpdateTimerText();

            if (timer <= 0f)
            {
                isCountingDown = false;
                StartCoroutine(PullBackRoutine());
            }
        }
    }

    public void ResetTimer()
    {
        timer = countdownTime;
        isCountingDown = true;
        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (countdownText != null)
        {
            int roundedTime = Mathf.CeilToInt(Mathf.Max(0f, timer));
            countdownText.text = "TimeTill Pull: " + roundedTime;
        }
    }

    private IEnumerator PullBackRoutine()
    {
        Vector3[] points = lineDrawer.currentCorners;
        if (points == null || points.Length < 2) yield break;

        lineDrawer.isPullingBack = true;

        for (int i = points.Length - 2; i >= 0; i--)
        {
            Vector3 targetPos = new Vector3(points[i].x, transform.position.y, points[i].z);

            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, pullSpeed * Time.deltaTime);
                yield return null;
            }

            if (lineDrawer.line.positionCount > 2)
            {
                lineDrawer.line.positionCount--;
            }
        }

        transform.position = new Vector3(lineDrawer.basePoint.position.x, transform.position.y, lineDrawer.basePoint.position.z);
        lineDrawer.isPullingBack = false;

        // Reset the timer so the line continues drawing and counting down again
        ResetTimer();
    }
}
