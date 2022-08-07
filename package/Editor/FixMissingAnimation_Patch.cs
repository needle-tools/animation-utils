using System;
using System.Collections;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace Needle
{
	internal static class FixMissingAnimationPatch
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			var instance = new Harmony("com.needle.fix-missing-animation");
			var type = Type.GetType("UnityEditorInternal.AnimationWindowHierarchyGUI, UnityEditor.CoreModule");
			var menuMethod = AccessTools.Method(type, "GenerateMenu");

			instance.Patch(menuMethod, null, new HarmonyMethod(AccessTools.Method(typeof(FixMissingAnimationPatch), nameof(GenerateMenu_PostFix))));
		}

		// Patch for:
		// https://github.dev/Unity-Technologies/UnityCsReference/blob/c84064be69f20dcf21ebe4a7bbc176d48e2f289c/Editor/Mono/Animation/AnimationWindow/AnimationWindowUtility.cs @GenerateMenu
		private static void GenerateMenu_PostFix(ref GenericMenu __result, IList interactedNodes)
		{
			for (var index = 0; index < interactedNodes.Count; index++)
			{
				var node = interactedNodes[index];
				var isMissing = AnimationWindowHierarchyNodeAccess.IsMissing(node, out var maybeBinding);
				if (isMissing && maybeBinding.HasValue)
				{
					var binding = maybeBinding.Value;
					__result.AddItem(new GUIContent("Fix " + binding.path), false, () => { SelectedFix(binding); });
				}

				// check if next one is the same actually
				// because there seems to be a bug in unity's menu code where the same items are multiple times in the same list
				if (index + 1 < interactedNodes.Count)
				{
					if (interactedNodes[index + 1] == node)
					{
						index++;
					}
				}
			}
		}

		private static void SelectedFix(EditorCurveBinding binding)
		{
			Debug.Log(binding.path + ", " + binding.propertyName + ", " + binding.type);
		}

		// private static EditorCurveBinding GetBinding(object node)
		// {
		//     var t = node.GetType();
		//     var bindingField = t.GetField("binding", BindingFlags.Instance | BindingFlags.Public);
		//     var binding = bindingField?.GetValue(node);
		//     return binding is EditorCurveBinding curveBinding ? curveBinding : default;
		// }
	}
}