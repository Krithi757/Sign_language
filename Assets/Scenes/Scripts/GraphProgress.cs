using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Profiling;


public class GraphProgress : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    public TextMeshProUGUI memoryText;
    public TextMeshProUGUI cpuText;
    public TextMeshProUGUI gcText;
    public TextMeshProUGUI frameTimeText;
    public TextMeshProUGUI loadTimeText;
    public TextMeshProUGUI stressTestText;
    public TextMeshProUGUI gpuText;

    public RawImage graphPanel;
    public Color fpsColor = Color.green;
    public Color memoryColor = Color.blue;
    public Color backgroundColor = Color.black;

    private Texture2D graphTexture;
    private int width = 600;
    private int height = 200;

    private Queue<float> fpsData = new Queue<float>();
    private Queue<float> memoryData = new Queue<float>();
    private int maxPoints = 100;

    private float loadStartTime;
    private bool isStressTesting = false;
    private int stressTestIterations = 0;

    // New variables for the automatic start delay
    private float stressTestStartDelay = 10f;  // Delay in seconds before starting stress test
    private float stressTestTimer = 0f;

    void Start()
    {
        graphTexture = new Texture2D(width, height);
        graphPanel.texture = graphTexture;

        // Start load time testing
        loadStartTime = Time.time;
    }

    void Update()
    {
        // FPS calculation
        float fps = 1.0f / Time.deltaTime;
        // Memory usage in MB
        float memory = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
        // Graphics memory usage in MB
        float gcAlloc = Profiler.GetAllocatedMemoryForGraphicsDriver() / (1024f * 1024f);
        // Frame time in milliseconds
        float frameTime = Time.deltaTime * 1000f;
        // CPU Usage approximation (simulated)
        float cpuUsage = GetCPUUsage();
        // GPU usage (using Profiler GPU API)
        float gpuUsage = GetGPUUsage();

        // Load time calculation
        float loadTime = Time.time - loadStartTime;

        // Stress test simulation (triggered after a certain number of frames)
        if (isStressTesting)
        {
            stressTestIterations++;
            if (stressTestIterations > 500)
            {
                isStressTesting = false;
                stressTestText.text = $"Stress Test Completed. {stressTestIterations} iterations.";
            }
            else
            {
                stressTestText.text = $"Stress Testing: {stressTestIterations} iterations.";
            }
        }

        // Update on-screen texts
        fpsText.text = $"FPS: {fps:F1}";
        memoryText.text = $"Memory: {memory:F1} MB";
        gcText.text = $"GC Alloc: {gcAlloc:F2} MB";
        frameTimeText.text = $"Frame Time: {frameTime:F1} ms";
        cpuText.text = $"CPU Usage: {cpuUsage:F1}%";
        loadTimeText.text = $"Load Time: {loadTime:F1}s";
        gpuText.text = $"GPU Usage: {gpuUsage:F1}%";

        UpdateData(fpsData, fps);
        UpdateData(memoryData, memory);

        DrawGraph();

        // Start stress test after delay
        stressTestTimer += Time.deltaTime;
        if (stressTestTimer >= stressTestStartDelay && !isStressTesting)
        {
            StartStressTest(); // Starts the stress test
        }
    }

    void UpdateData(Queue<float> dataQueue, float newValue)
    {
        if (dataQueue.Count >= maxPoints)
            dataQueue.Dequeue();
        dataQueue.Enqueue(newValue);
    }

    void DrawGraph()
    {
        // Fill the texture with the background color
        Color[] backgroundPixels = new Color[width * height];
        for (int i = 0; i < backgroundPixels.Length; i++)
            backgroundPixels[i] = backgroundColor;
        graphTexture.SetPixels(backgroundPixels);

        DrawLine(fpsData, fpsColor, 200);      // FPS line
        DrawLine(memoryData, memoryColor, 100); // Memory line

        graphTexture.Apply();
    }

    void DrawLine(Queue<float> data, Color color, float scale)
    {
        float[] dataArray = new float[data.Count];
        data.CopyTo(dataArray, 0);

        for (int i = 1; i < dataArray.Length; i++)
        {
            int x1 = (int)(((float)(i - 1) / maxPoints) * width);
            int y1 = (int)((dataArray[i - 1] / scale) * height);
            int x2 = (int)(((float)i / maxPoints) * width);
            int y2 = (int)((dataArray[i] / scale) * height);

            DrawLineOnTexture(x1, y1, x2, y2, color);
        }
    }

    void DrawLineOnTexture(int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x0 >= 0 && x0 < width && y0 >= 0 && y0 < height)
                graphTexture.SetPixel(x0, y0, color);

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    // 🚀 Simulated CPU usage (Unity doesn't provide direct CPU %)
    float GetCPUUsage()
    {
        return Mathf.Clamp((Time.deltaTime / (1f / Application.targetFrameRate)) * 100f, 0f, 100f);
    }

    // 🚀 Simulated GPU usage (using Unity Profiler to get GPU info)
    float GetGPUUsage()
    {
        // Get GPU usage through Profiler API (Unity does not directly provide GPU percentage).
        // Assuming a custom approximation as GPU load is hard to measure in Unity directly.
        return Mathf.Clamp((Time.deltaTime / (1f / Application.targetFrameRate)) * 50f, 0f, 100f); // Custom approximation
    }

    // Method to simulate stress testing
    private void StartStressTest()
    {
        isStressTesting = true;
        stressTestIterations = 0;
        stressTestText.text = "Stress Testing Started";
    }
}
