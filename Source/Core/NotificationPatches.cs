using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;
using RimWorldAI.Core;

namespace RimWorldAI.Core
{
    /// <summary>
    /// 通知系统 Harmony Patches
    /// </summary>

    /// <summary>
    /// 拦截 LetterMaker.MakeLetter - 当信件被创建时触发
    /// MakeLetter 是创建所有信件的统一入口点
    /// 使用 MethodType 获取所有重载，在 Postfix 中过滤
    /// </summary>
    [HarmonyPatch(typeof(LetterMaker))]
    public static class LetterMaker_MakeLetter_Patch
    {
        // 使用 HarmonyTargetMethods 指定所有 MakeLetter 重载
        static IEnumerable<MethodBase> TargetMethods()
        {
            // 获取所有名为 MakeLetter 的方法
            foreach (var method in typeof(LetterMaker).GetMethods(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            {
                if (method.Name == "MakeLetter")
                {
                    yield return method;
                }
            }
        }

        static void Postfix(Letter __result)
        {
            try
            {
                if (__result != null)
                {
                    NotificationManager.Instance.AddLetter(__result);
                }
            }
            catch (Exception ex)
            {
                Verse.Log.Warning($"[RimWorldAI] Failed to capture letter: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 拦截 Messages.Message (string, LookTargets, MessageTypeDef, bool)
    /// </summary>
    [HarmonyPatch(typeof(Messages), nameof(Messages.Message),
        new Type[] { typeof(string), typeof(LookTargets), typeof(MessageTypeDef), typeof(bool) })]
    public static class Messages_Message_Patch1
    {
        static void Postfix(string text, LookTargets lookTargets, MessageTypeDef def, bool historical)
        {
            try
            {
                NotificationManager.Instance.AddMessage(text, def);
            }
            catch (Exception ex)
            {
                Verse.Log.Warning($"[RimWorldAI] Failed to capture message: {ex.Message}");
            }
        }
    }
}
