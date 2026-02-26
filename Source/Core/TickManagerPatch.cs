using System;
using HarmonyLib;
using Verse;
using RimWorldAI.Core;

namespace RimWorldAI
{
    /// <summary>
    /// Harmony Patch - 在每帧处理命令队列
    /// 这确保所有命令都在主线程执行
    /// </summary>
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("com.rimworldai.patch");
            harmony.PatchAll(typeof(HarmonyPatches).Assembly);
            Log.Message("[RimWorldAI] Harmony patches applied");
        }
    }

    /// <summary>
    /// Patch TickManager.DoSingleTick - 在每帧末尾处理命令队列
    /// </summary>
    [HarmonyPatch(typeof(TickManager), nameof(TickManager.DoSingleTick))]
    public static class TickManager_DoSingleTick_Patch
    {
        static void Postfix()
        {
            // 在每帧处理待处理的命令
            CommandQueue.ProcessAll();
        }
    }
}
