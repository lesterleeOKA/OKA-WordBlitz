using UnityEngine;

[RequireComponent((typeof(LineRenderer)))]
public class LineDrawer : MonoBehaviour
{
    public static LineDrawer Instance = null;
    private LineRenderer lineRenderer;
    private Vector3[] positions;
    public float lineWidth = 0.1f;
    private int index = 0;
    public bool started = false;

    private void Awake()
    {
        if(Instance == null) Instance = this;
    }
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = this.lineWidth;
        lineRenderer.endWidth = this.lineWidth;
    }

    public void StartDrawing()
    {
        if(!started) { 
            index = 0;
            lineRenderer.positionCount = 0; // Reset positions
            started = true;
        }
    }

    public void AddPoint(Vector3 position)
    {
        lineRenderer.positionCount = index + 1;
        lineRenderer.SetPosition(index, position);
        index++;
    }

    public void FinishDrawing()
    {
        lineRenderer.positionCount = 0; // Reset position count
        index = 0; // Reset index
        started = false; // Reset started state
    }
}
