using System;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace Needle
{
	internal static class AnimationWindowHierarchyGUI_Patch
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			var animationWindowHierarchy = Type.GetType("UnityEditorInternal.AnimationWindowHierarchyGUI, UnityEditor.CoreModule");

			if (animationWindowHierarchy != null)
			{
				var instance = new Harmony("com.needle.animation-utils");

				// var menuMethod = AccessTools.Method(animationWindowHierarchy, "GenerateMenu");
				// instance.Patch(menuMethod, null, new HarmonyMethod(AccessTools.Method(typeof(FixMissingAnimationPatch), nameof(GenerateMenu_PostFix))));

				var nodeGui = AccessTools.Method(animationWindowHierarchy, "DoNodeGUI");
				if (nodeGui != null)
					instance.Patch(nodeGui, new HarmonyMethod(AccessTools.Method(typeof(AnimationWindowHierarchyGUI_Patch), nameof(AnimationItemGUI_Prefix))));
			}
		}

		private static bool AnimationItemGUI_Prefix(object __instance, Rect rect, object node, bool selected, bool focused, int row)
		{
			if (AnimationWindowHierarchyNodeAccess.DrawNodeRow(rect, node, __instance)) return false;
			return true;
		}

		// Patch for:
		// https://github.dev/Unity-Technologies/UnityCsReference/blob/c84064be69f20dcf21ebe4a7bbc176d48e2f289c/Editor/Mono/Animation/AnimationWindow/AnimationWindowUtility.cs @GenerateMenu
		// private static void GenerateMenu_PostFix(ref GenericMenu __result, IList interactedNodes)
		// {
		// 	for (var index = 0; index < interactedNodes.Count; index++)
		// 	{
		// 		var node = interactedNodes[index];
		// 		var isMissing = AnimationWindowHierarchyNodeAccess.IsMissing(node, out var binding);
		// 		if (isMissing)
		// 		{
		// 			__result.AddItem(new GUIContent("Fix " + binding.path), false, () => { SelectedFix(binding); });
		// 		}
		//
		// 		// check if next one is the same actually
		// 		// because there seems to be a bug in unity's menu code where the same items are multiple times in the same list
		// 		if (index + 1 < interactedNodes.Count)
		// 		{
		// 			if (interactedNodes[index + 1] == node)
		// 			{
		// 				index++;
		// 			}
		// 		}
		// 	}
		// }
		//
		// private static void SelectedFix(EditorCurveBinding binding)
		// {
		// 	Debug.Log(binding.path + ", " + binding.propertyName + ", " + binding.type);
		// }

		// private static EditorCurveBinding GetBinding(object node)
		// {
		//     var t = node.GetType();
		//     var bindingField = t.GetField("binding", BindingFlags.Instance | BindingFlags.Public);
		//     var binding = bindingField?.GetValue(node);
		//     return binding is EditorCurveBinding curveBinding ? curveBinding : default;
		// }
	}
}