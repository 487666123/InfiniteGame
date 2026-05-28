using System;
using System.IO;
using System.Text;
using Godot;

namespace DesktopAssistant;

/// <summary>
/// 数据文件管理器
/// </summary>
public class DataManager
{
    public string SavePath { get; }

    public DataManager()
    {
        SavePath = Path.Combine(OS.GetUserDataDir(), "todo.yaml");

        GD.Print($"SavePath: {SavePath}");
    }

    public bool LoadData(out string data)
    {
        if (!File.Exists(SavePath))
        {
            data = string.Empty;
            GD.Print($"不存在配置文件");
            return false;
        }

        data = File.ReadAllText(SavePath, Encoding.UTF8);
        GD.Print($"Data: {data}");
        return true;
    }
}

/// <summary>
/// 待办项
/// </summary>
public class TodoItem
{
    /// <summary>
    /// 待办名称
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 是否完成
    /// </summary>
    public bool IsDone { get; set; }

    /// <summary>
    /// 创建日期
    /// </summary>
    public DateTime CreationDateTime { get; set; }

    /// <summary>
    /// 完成日期
    /// </summary>
    public DateTime CompletionDateTime { get; set; }
}