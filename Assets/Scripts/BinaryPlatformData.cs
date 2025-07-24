using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 二进制平台数据存储系统
/// 使用位标志和紧凑的数据结构来最小化存储空间
/// </summary>
[System.Serializable]
public class BinaryPlatformData
{
    // 使用位标志来压缩布尔值存储
    [Flags]
    public enum OverrideFlags : byte
    {
        None = 0,
        Position = 1 << 0,      // 位置覆盖
        Size = 1 << 1,          // 尺寸覆盖
        Anchors = 1 << 2,       // 锚点覆盖
        Pivot = 1 << 3,         // 轴心覆盖
        Rotation = 1 << 4,      // 旋转覆盖
        Scale = 1 << 5          // 缩放覆盖
    }

    // 紧凑的平台设置结构
    [System.Serializable]
    public struct CompactPlatformSettings
    {
        public OverrideFlags flags;     // 1字节 - 覆盖标志
        public Vector2 position;        // 8字节 - 位置
        public Vector2 size;            // 8字节 - 尺寸
        public Vector4 anchors;         // 16字节 - 锚点(min.x, min.y, max.x, max.y)
        public Vector2 pivot;           // 8字节 - 轴心
        public Vector3 rotation;        // 12字节 - 旋转
        public Vector3 scale;           // 12字节 - 缩放
        
        // 总计: 65字节每个平台设置
        
        public CompactPlatformSettings(PlatformRectSettings original)
        {
            flags = OverrideFlags.None;
            if (original.overrideAnchoredPosition) flags |= OverrideFlags.Position;
            if (original.overrideSizeDelta) flags |= OverrideFlags.Size;
            if (original.overrideAnchors) flags |= OverrideFlags.Anchors;
            if (original.overridePivot) flags |= OverrideFlags.Pivot;
            if (original.overrideRotation) flags |= OverrideFlags.Rotation;
            if (original.overrideScale) flags |= OverrideFlags.Scale;
            
            position = original.anchoredPosition;
            size = original.sizeDelta;
            anchors = new Vector4(original.anchorMin.x, original.anchorMin.y, 
                                original.anchorMax.x, original.anchorMax.y);
            pivot = original.pivot;
            rotation = original.rotation;
            scale = original.scale;
        }
        
        public PlatformRectSettings ToPlatformRectSettings()
        {
            var settings = new PlatformRectSettings();
            
            settings.overrideAnchoredPosition = (flags & OverrideFlags.Position) != 0;
            settings.overrideSizeDelta = (flags & OverrideFlags.Size) != 0;
            settings.overrideAnchors = (flags & OverrideFlags.Anchors) != 0;
            settings.overridePivot = (flags & OverrideFlags.Pivot) != 0;
            settings.overrideRotation = (flags & OverrideFlags.Rotation) != 0;
            settings.overrideScale = (flags & OverrideFlags.Scale) != 0;
            
            settings.anchoredPosition = position;
            settings.sizeDelta = size;
            settings.anchorMin = new Vector2(anchors.x, anchors.y);
            settings.anchorMax = new Vector2(anchors.z, anchors.w);
            settings.pivot = pivot;
            settings.rotation = rotation;
            settings.scale = scale;
            
            return settings;
        }
    }

    // 二进制数据容器
    [System.Serializable]
    public class BinaryPlatformContainer
    {
        [SerializeField] private byte[] binaryData;
        [SerializeField] private int dataVersion = 1; // 版本控制
        
        private Dictionary<Platform, CompactPlatformSettings> cachedData;
        
        public BinaryPlatformContainer()
        {
            cachedData = new Dictionary<Platform, CompactPlatformSettings>();
        }
        
        // 从原始数据转换
        public void FromPlatformDataList(List<PlatformDataEntry> platformData)
        {
            cachedData.Clear();
            
            foreach (var entry in platformData)
            {
                cachedData[entry.platform] = new CompactPlatformSettings(entry.settings);
            }
            
            SerializeToBinary();
        }
        
        // 转换回原始数据
        public List<PlatformDataEntry> ToPlatformDataList()
        {
            DeserializeFromBinary();
            
            var result = new List<PlatformDataEntry>();
            foreach (var kvp in cachedData)
            {
                var entry = new PlatformDataEntry(kvp.Key);
                entry.settings = kvp.Value.ToPlatformRectSettings();
                result.Add(entry);
            }
            
            return result;
        }
        
        // 获取特定平台的设置
        public CompactPlatformSettings GetPlatformSettings(Platform platform)
        {
            DeserializeFromBinary();
            
            if (cachedData.TryGetValue(platform, out var settings))
            {
                return settings;
            }
            
            return new CompactPlatformSettings();
        }
        
        // 设置特定平台的数据
        public void SetPlatformSettings(Platform platform, CompactPlatformSettings settings)
        {
            DeserializeFromBinary();
            cachedData[platform] = settings;
            SerializeToBinary();
        }
        
        // 序列化到二进制
        private void SerializeToBinary()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                // 写入版本号
                writer.Write(dataVersion);
                
                // 写入平台数量
                writer.Write(cachedData.Count);
                
                // 写入每个平台的数据
                foreach (var kvp in cachedData)
                {
                    // 平台枚举 (1字节)
                    writer.Write((byte)kvp.Key);
                    
                    // 覆盖标志 (1字节)
                    writer.Write((byte)kvp.Value.flags);
                    
                    // 位置 (8字节)
                    writer.Write(kvp.Value.position.x);
                    writer.Write(kvp.Value.position.y);
                    
                    // 尺寸 (8字节)
                    writer.Write(kvp.Value.size.x);
                    writer.Write(kvp.Value.size.y);
                    
                    // 锚点 (16字节)
                    writer.Write(kvp.Value.anchors.x);
                    writer.Write(kvp.Value.anchors.y);
                    writer.Write(kvp.Value.anchors.z);
                    writer.Write(kvp.Value.anchors.w);
                    
                    // 轴心 (8字节)
                    writer.Write(kvp.Value.pivot.x);
                    writer.Write(kvp.Value.pivot.y);
                    
                    // 旋转 (12字节)
                    writer.Write(kvp.Value.rotation.x);
                    writer.Write(kvp.Value.rotation.y);
                    writer.Write(kvp.Value.rotation.z);
                    
                    // 缩放 (12字节)
                    writer.Write(kvp.Value.scale.x);
                    writer.Write(kvp.Value.scale.y);
                    writer.Write(kvp.Value.scale.z);
                }
                
                binaryData = stream.ToArray();
            }
        }
        
        // 从二进制反序列化
        private void DeserializeFromBinary()
        {
            if (binaryData == null || binaryData.Length == 0)
            {
                cachedData.Clear();
                return;
            }
            
            if (cachedData.Count > 0) return; // 已经缓存
            
            using (var stream = new MemoryStream(binaryData))
            using (var reader = new BinaryReader(stream))
            {
                // 读取版本号
                int version = reader.ReadInt32();
                if (version != dataVersion)
                {
                    Debug.LogWarning($"Binary data version mismatch: {version} vs {dataVersion}");
                }
                
                // 读取平台数量
                int platformCount = reader.ReadInt32();
                
                cachedData.Clear();
                
                // 读取每个平台的数据
                for (int i = 0; i < platformCount; i++)
                {
                    var platform = (Platform)reader.ReadByte();
                    
                    var settings = new CompactPlatformSettings();
                    settings.flags = (OverrideFlags)reader.ReadByte();
                    
                    // 位置
                    settings.position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    
                    // 尺寸
                    settings.size = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    
                    // 锚点
                    settings.anchors = new Vector4(reader.ReadSingle(), reader.ReadSingle(), 
                                                 reader.ReadSingle(), reader.ReadSingle());
                    
                    // 轴心
                    settings.pivot = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    
                    // 旋转
                    settings.rotation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    
                    // 缩放
                    settings.scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    
                    cachedData[platform] = settings;
                }
            }
        }
        
        // 获取存储大小信息
        public int GetStorageSize()
        {
            return binaryData?.Length ?? 0;
        }
        
        // 清理缓存
        public void ClearCache()
        {
            cachedData.Clear();
        }
    }
}