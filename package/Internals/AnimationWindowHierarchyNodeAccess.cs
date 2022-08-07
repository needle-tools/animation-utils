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
				if (IsMissing(node, out var binding) && DragAndDrop.objectReferences.Length > 0)
				{
					// GUI.Label(rect, binding.propertyName);
					var obj = EditorGUI.ObjectField(rect, null, binding.type, true);
					if (obj is Component comp) obj = comp.gameObject;
					if (obj && obj is GameObject go)
					{
						Debug.Log("Update binding with: " + obj);
						foreach (var curve in node.curves)
						{
							Undo.RegisterCompleteObjectUndo(curve.clip, "Replace curve");
							var path = AnimationUtility.CalculateTransformPath(go.transform, curve.rootGameObject.transform);
							curve.clip.SetCurve(path, curve.type, curve.propertyName, curve.ToAnimationCurve());
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

			if (hierarchyNode.parent is AnimationWindowHierarchyPropertyGroupNode && hierarchyNode.binding != null && AnimationWindowUtility.ForceGrouping((EditorCurveBinding)hierarchyNode.binding))
				hierarchyNode = (AnimationWindowHierarchyNode)hierarchyNode.parent;

			if (hierarchyNode.curves == null)
				return;

			List<AnimationWindowCurve> curves = null;

			// Property or propertygroup
			if (hierarchyNode is AnimationWindowHierarchyPropertyGroupNode || hierarchyNode is AnimationWindowHierarchyPropertyNode)
				curves = AnimationWindowUtility.FilterCurves(hierarchyNode.curves.ToArray(), hierarchyNode.path, hierarchyNode.animatableObjectType, hierarchyNode.propertyName);
			else
				curves = AnimationWindowUtility.FilterCurves(hierarchyNode.curves.ToArray(), hierarchyNode.path, hierarchyNode.animatableObjectType);

			foreach (AnimationWindowCurve animationWindowCurve in curves)
				state.RemoveCurve(animationWindowCurve, "Remove AnimationCurve");
		}
	}
}