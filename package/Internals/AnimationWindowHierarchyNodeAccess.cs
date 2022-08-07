using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Needle
{
	public static class AnimationWindowHierarchyNodeAccess
	{
		public static bool DrawNodeRow(Rect rect, object nodeObj, object hierarchy)
		{
			if (nodeObj is AnimationWindowHierarchyNode node && hierarchy is AnimationWindowHierarchyGUI gui)
			{
				var isMissing = IsMissing(node);
				var isMissingOrShortcut = isMissing || Event.current.modifiers == EventModifiers.Alt;
				if (isMissingOrShortcut && DragAndDrop.objectReferences.Length > 0)
				{
					var path = node.path;
					if (path == null) return false;
					var lastPathPartIndex = path.LastIndexOf("/", StringComparison.Ordinal);
					if (lastPathPartIndex > 0)
					{
						// ReSharper disable once ReplaceSubstringWithRangeIndexer
						path = path.Substring(lastPathPartIndex + 1);
					}
					var label = path + " : " + node.displayName;
					var type = node.animatableObjectType;
					var obj = EditorGUI.ObjectField(rect, label, null, type, true);
					if (obj is Component comp) obj = comp.gameObject;
					if (obj && obj is GameObject go)
					{
						Debug.Log("Update binding with: " + obj);
						foreach (var curve in node.curves)
						{
							Undo.RegisterCompleteObjectUndo(curve.clip, "Replace curve");
							var objPath = AnimationUtility.CalculateTransformPath(go.transform, curve.rootGameObject.transform);
							curve.clip.SetCurve(objPath, curve.type, curve.propertyName, curve.ToAnimationCurve());
							if (isMissing)
								RemoveCurve(gui, node);
						}
					}
					return true;
				}
			}
			return false;
		}

		public static bool IsMissing(object obj, out EditorCurveBinding binding)
		{
			if (obj is AnimationWindowHierarchyNode node)
			{
				binding = node.binding.GetValueOrDefault();
				return binding != null && IsMissing(node);
			}
			binding = default;
			return false;
		}

		private static bool IsMissing(AnimationWindowHierarchyNode node)
		{
			return AnimationWindowUtility.IsNodeLeftOverCurve(node);
		}

		private static void RemoveCurve(AnimationWindowHierarchyGUI gui, AnimationWindowHierarchyNode node)
		{
			var state = gui.state;

			AnimationWindowHierarchyNode hierarchyNode = node;

			if (hierarchyNode.parent is AnimationWindowHierarchyPropertyGroupNode && hierarchyNode.binding != null &&
			    AnimationWindowUtility.ForceGrouping((EditorCurveBinding)hierarchyNode.binding))
				hierarchyNode = (AnimationWindowHierarchyNode)hierarchyNode.parent;

			if (hierarchyNode.curves == null)
				return;

			List<AnimationWindowCurve> curves = null;

			// Property or propertygroup
			if (hierarchyNode is AnimationWindowHierarchyPropertyGroupNode || hierarchyNode is AnimationWindowHierarchyPropertyNode)
				curves = AnimationWindowUtility.FilterCurves(hierarchyNode.curves.ToArray(), hierarchyNode.path, hierarchyNode.animatableObjectType,
					hierarchyNode.propertyName);
			else
				curves = AnimationWindowUtility.FilterCurves(hierarchyNode.curves.ToArray(), hierarchyNode.path, hierarchyNode.animatableObjectType);

			foreach (AnimationWindowCurve animationWindowCurve in curves)
				state.RemoveCurve(animationWindowCurve, "Remove AnimationCurve");
		}
	}
}