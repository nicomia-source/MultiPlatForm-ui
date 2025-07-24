using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public class BinaryStorageTest : MonoBehaviour
{
    [Header("Test Configuration")]
    public MultiPlatformRectData testTarget;
    public int testDataCount = 100;
    public bool runPerformanceTest = false;

    [Header("Test Results")]
    [SerializeField] private string lastTestResult;
    [SerializeField] private float compressionRatio;
    [SerializeField] private int traditionalSize;
    [SerializeField] private int binarySize;

    private void Start()
    {
        if (runPerformanceTest)
        {
            RunPerformanceTest();
        }
    }

    [ContextMenu("Run Binary Storage Test")]
    public void RunBinaryStorageTest()
    {
        if (testTarget == null)
        {
            UnityEngine.Debug.LogError("Test target not assigned!");
            return;
        }

        UnityEngine.Debug.Log("=== Binary Storage Test Started ===");

        // 1. 生成测试数据
        GenerateTestData();

        // 2. 测试传统存储
        var traditionalStopwatch = Stopwatch.StartNew();
        testTarget.UseBinaryStorage = false;
        TestDataAccess();
        traditionalStopwatch.Stop();

        // 3. 测试二进制存储
        var binaryStopwatch = Stopwatch.StartNew();
        testTarget.UseBinaryStorage = true;
        TestDataAccess();
        binaryStopwatch.Stop();

        // 4. 比较结果
        CompareResults(traditionalStopwatch.ElapsedMilliseconds, binaryStopwatch.ElapsedMilliseconds);

        UnityEngine.Debug.Log("=== Binary Storage Test Completed ===");
    }

    private void GenerateTestData()
    {
        UnityEngine.Debug.Log($"Generating {testDataCount} test data entries...");

        // 确保使用传统存储来生成数据
        testTarget.UseBinaryStorage = false;

        var platforms = System.Enum.GetValues(typeof(Platform));
        var random = new System.Random();

        foreach (Platform platform in platforms)
        {
            var settings = new PlatformRectSettings
            {
                overrideAnchoredPosition = true,
                anchoredPosition = new Vector2(random.Next(-500, 500), random.Next(-300, 300)),
                overrideSizeDelta = true,
                sizeDelta = new Vector2(random.Next(50, 200), random.Next(30, 100)),
                overrideAnchors = true,
                anchorMin = new Vector2((float)random.NextDouble(), (float)random.NextDouble()),
                anchorMax = new Vector2((float)random.NextDouble(), (float)random.NextDouble()),
                overridePivot = true,
                pivot = new Vector2((float)random.NextDouble(), (float)random.NextDouble()),
                overrideRotation = true,
                rotation = new Vector3(random.Next(0, 360), random.Next(0, 360), random.Next(0, 360)),
                overrideScale = true,
                scale = new Vector3((float)random.NextDouble() * 2, (float)random.NextDouble() * 2, 1f)
            };

            testTarget.SetSettingsForPlatform(platform, settings);
        }

        UnityEngine.Debug.Log("Test data generation completed");
    }

    private void TestDataAccess()
    {
        var platforms = System.Enum.GetValues(typeof(Platform));
        
        // 测试读取性能
        foreach (Platform platform in platforms)
        {
            var settings = testTarget.GetSettingsForPlatform(platform);
            // 模拟数据使用
            var _ = settings.anchoredPosition + settings.sizeDelta;
        }

        // 测试写入性能
        foreach (Platform platform in platforms)
        {
            var settings = testTarget.GetSettingsForPlatform(platform);
            settings.anchoredPosition += Vector2.one;
            testTarget.SetSettingsForPlatform(platform, settings);
        }
    }

    private void CompareResults(long traditionalTime, long binaryTime)
    {
        string storageInfo = testTarget.GetStorageInfo();
        compressionRatio = testTarget.GetCompressionRatio();

        // 解析存储大小
        if (storageInfo.Contains("Binary Storage:"))
        {
            string sizeStr = storageInfo.Split(':')[1].Trim().Split(' ')[0];
            int.TryParse(sizeStr, out binarySize);
        }

        traditionalSize = System.Enum.GetValues(typeof(Platform)).Length * 200; // 估算

        lastTestResult = $"Traditional: {traditionalTime}ms, Binary: {binaryTime}ms\n" +
                        $"Size - Traditional: ~{traditionalSize}B, Binary: {binarySize}B\n" +
                        $"Compression: {compressionRatio:F2}x\n" +
                        $"Performance: {(binaryTime < traditionalTime ? "Binary faster" : "Traditional faster")}";

        UnityEngine.Debug.Log($"Performance Comparison:\n{lastTestResult}");
    }

    [ContextMenu("Run Performance Test")]
    public void RunPerformanceTest()
    {
        if (testTarget == null) return;

        UnityEngine.Debug.Log("=== Performance Test Started ===");

        const int iterations = 1000;
        var platforms = System.Enum.GetValues(typeof(Platform));

        // 准备测试数据
        GenerateTestData();

        // 测试传统存储性能
        testTarget.UseBinaryStorage = false;
        var traditionalStopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            foreach (Platform platform in platforms)
            {
                var settings = testTarget.GetSettingsForPlatform(platform);
                testTarget.SetSettingsForPlatform(platform, settings);
            }
        }
        
        traditionalStopwatch.Stop();

        // 测试二进制存储性能
        testTarget.UseBinaryStorage = true;
        var binaryStopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            foreach (Platform platform in platforms)
            {
                var settings = testTarget.GetSettingsForPlatform(platform);
                testTarget.SetSettingsForPlatform(platform, settings);
            }
        }
        
        binaryStopwatch.Stop();

        float speedup = (float)traditionalStopwatch.ElapsedMilliseconds / binaryStopwatch.ElapsedMilliseconds;
        
        UnityEngine.Debug.Log($"Performance Test Results ({iterations} iterations):");
        UnityEngine.Debug.Log($"Traditional Storage: {traditionalStopwatch.ElapsedMilliseconds}ms");
        UnityEngine.Debug.Log($"Binary Storage: {binaryStopwatch.ElapsedMilliseconds}ms");
        UnityEngine.Debug.Log($"Speedup: {speedup:F2}x");
        UnityEngine.Debug.Log($"Storage Info: {testTarget.GetStorageInfo()}");

        UnityEngine.Debug.Log("=== Performance Test Completed ===");
    }

    [ContextMenu("Validate Data Integrity")]
    public void ValidateDataIntegrity()
    {
        if (testTarget == null) return;

        UnityEngine.Debug.Log("=== Data Integrity Test Started ===");

        // 生成测试数据
        GenerateTestData();

        // 保存传统存储的数据
        var originalData = new Dictionary<Platform, PlatformRectSettings>();
        var platforms = System.Enum.GetValues(typeof(Platform));
        
        foreach (Platform platform in platforms)
        {
            originalData[platform] = testTarget.GetSettingsForPlatform(platform);
        }

        // 转换到二进制存储
        testTarget.UseBinaryStorage = true;

        // 验证数据完整性
        bool dataIntegrityValid = true;
        foreach (Platform platform in platforms)
        {
            var binaryData = testTarget.GetSettingsForPlatform(platform);
            var originalSettings = originalData[platform];

            if (!CompareSettings(originalSettings, binaryData))
            {
                dataIntegrityValid = false;
                UnityEngine.Debug.LogError($"Data integrity failed for platform: {platform}");
            }
        }

        if (dataIntegrityValid)
        {
            UnityEngine.Debug.Log("✓ Data integrity test PASSED - All data preserved correctly");
        }
        else
        {
            UnityEngine.Debug.LogError("✗ Data integrity test FAILED - Data corruption detected");
        }

        UnityEngine.Debug.Log("=== Data Integrity Test Completed ===");
    }

    private bool CompareSettings(PlatformRectSettings a, PlatformRectSettings b)
    {
        const float tolerance = 0.001f;

        return a.overrideAnchoredPosition == b.overrideAnchoredPosition &&
               Vector2.Distance(a.anchoredPosition, b.anchoredPosition) < tolerance &&
               a.overrideSizeDelta == b.overrideSizeDelta &&
               Vector2.Distance(a.sizeDelta, b.sizeDelta) < tolerance &&
               a.overrideAnchors == b.overrideAnchors &&
               Vector2.Distance(a.anchorMin, b.anchorMin) < tolerance &&
               Vector2.Distance(a.anchorMax, b.anchorMax) < tolerance &&
               a.overridePivot == b.overridePivot &&
               Vector2.Distance(a.pivot, b.pivot) < tolerance &&
               a.overrideRotation == b.overrideRotation &&
               Vector3.Distance(a.rotation, b.rotation) < tolerance &&
               a.overrideScale == b.overrideScale &&
               Vector3.Distance(a.scale, b.scale) < tolerance;
    }
}