using Verse;
using RimWorldAI.Core;

namespace RimWorldAI
{
    /// <summary>
    /// Mod初始化入口
    /// 使用StaticConstructorOnStartup确保在游戏启动完成后执行
    /// </summary>
    [StaticConstructorOnStartup]
    public static class RimWorldAIInit
    {
        static RimWorldAIInit()
        {
            // 启动HTTP服务器 (支持 MCP 协议)
            HttpServer.Instance.Start();
            Log.Message("[RimWorldAI] HTTP server started, MCP protocol enabled");
        }
    }

    /// <summary>
    /// Mod主类，提供设置界面
    /// </summary>
    public class RimWorldAIMod : Mod
    {
        public static RimWorldAIMod Instance { get; private set; }
        public static RimWorldAISettings Settings { get; private set; }

        public RimWorldAIMod(ModContentPack content) : base(content)
        {
            Instance = this;
            Settings = GetSettings<RimWorldAISettings>();
            Log.Message("[RimWorldAI] Mod constructor called");
        }

        public override string SettingsCategory()
        {
            return "RimWorld AI";
        }
    }

    /// <summary>
    /// Mod设置
    /// </summary>
    public class RimWorldAISettings : ModSettings
    {
        public int port = 8080;
        public float updateInterval = 1.0f;
        public bool autoStart = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref port, "port", 8080);
            Scribe_Values.Look(ref updateInterval, "updateInterval", 1.0f);
            Scribe_Values.Look(ref autoStart, "autoStart", true);
        }
    }
}
