using System;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace Needle.AnimationUtils
{
	public class AnimationWindowDopeSheetGUI_Patch
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			var instance = new Harmony("com.needle.animation-utils-dopesheet");
			
			// var dopeLines = AccessTools.Method(AnimationWindowDopeSheetAccess.GetDopeSheetType(), "DopelinesGUI");
			// if (dopeLines != null)
			// 	instance.Patch(dopeLines, new HarmonyMethod(AccessTools.Method(typeof(AnimationWindowDopeSheetGUI_Patch), nameof(DopeLinesGUI_Prefix))));
			//
			// var startLiveEdit = AccessTools.Method(AnimationWindowDopeSheetAccess.GetAnimEditorType(), "OnStartLiveEdit");
			// if (startLiveEdit != null)
			// 	instance.Patch(startLiveEdit, new HarmonyMethod(AccessTools.Method(typeof(AnimationWindowDopeSheetGUI_Patch), nameof(OnStartLiveEdit_Prefix))));
			//
			// var endLiveEdit = AccessTools.Method(AnimationWindowDopeSheetAccess.GetAnimEditorType(), "OnEndLiveEdit");
			// if (endLiveEdit != null)
			// 	instance.Patch(endLiveEdit, new HarmonyMethod(AccessTools.Method(typeof(AnimationWindowDopeSheetGUI_Patch), nameof(OnStartLiveEdit_Prefix))));
			
			var animWindowCurve = AccessTools.Method(AnimationWindowDopeSheetAccess.GetAnimationWindowCurveType(), "ComparePaths");
			if (animWindowCurve != null)
				instance.Patch(animWindowCurve, new HarmonyMethod(AccessTools.Method(typeof(AnimationWindowDopeSheetGUI_Patch), nameof(OnComparePaths))));
		}

		// private static bool DopeLinesGUI_Prefix(object __instance, Rect position, Vector2 scrollPosition)
		// {
		// 	// Debug.Log(__instance);
		// 	// Debug.Log(position);
		// 	// Debug.Log(scrollPosition);
		// 	// return false;
		// 	return true;
		// }
		//
		// private static bool OnStartLiveEdit_Prefix()
		// {
		// 	return true;
		// }

		private static bool OnComparePaths(object __instance, string otherPath, ref int __result)
		{
			if (AnimationWindowDopeSheetAccess.CompareAnimWindowCurvePaths(__instance, otherPath, out var res))
			{
				__result = res;
				return false;
			}
			
			return true;
		}
	}
}