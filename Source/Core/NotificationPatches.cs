using System;
using System.Collections.Generic;
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
    /// </summary>
    [HarmonyPatch(typeof(LetterMaker), nameof(LetterMaker.MakeLetter))]
    public static class LetterMaker_MakeLetter_Patch
    {
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
