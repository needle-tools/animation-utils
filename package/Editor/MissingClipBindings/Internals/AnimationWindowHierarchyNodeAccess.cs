using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Needle.AnimationUtils
{
	public static class AnimationWindowHierarchyNodeAccess
	{
		public static bool DrawNodeRow(Rect rect, object nodeObj, object hierarchy)
		{
			if (nodeObj != null && nodeObj is AnimationWindowHierarchyNode node && hierarchy is AnimationWindowHierarchyGUI gui)
			{
				var isMissing = IsMissing(node, gui.state);
				var isMissingOrShortcut = isMissing || Event.current.modifiers == EventModifiers.Alt;
				// if the binding is missing and we drag an object
				if (isMissingOrShortcut && DragAndDrop.objectReferences.Length > 0)
				{
					var draggedObject = DragAndDrop.objectReferences.FirstOrDefault();
					if (!IsAllowedToReplace(node))
					{
						return false;
					}

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
					// check if we can actually make an assignment
					var canAssign = IsAssignable(draggedObject, type);
					using var disabled = new EditorGUI.DisabledScope(!canAssign);
					var prevColor = GUI.color;
					GUI.color = isMissing ? Color.yellow : Color.white;
					var obj = EditorGUI.ObjectField(rect, label, null, type, true);
					GUI.color = prevColor;
					
					var assignedObject = obj;
					if (obj is Component comp) obj = comp.gameObject;
					if (obj && obj is GameObject go)
					{
						var transform = go.transform;
						foreach (var curve in node.curves)
						{
							if (!curve.rootGameObject) continue;
							Object currentlyBoundObject = default;
							if (!isMissing)
							{
								currentlyBoundObject = AnimationUtility.GetAnimatedObject(curve.rootGameObject, curve.binding);
								if (currentlyBoundObject == assignedObject)
								{
									Debug.Log("Already bound to " + assignedObject, assignedObject);
									continue;
								}
							}
							var root = curve.rootGameObject.transform;
							if (!IsChild(root, transform))
							{
								Debug.LogError("Can not assign " + transform.name + " because it's no child of " + root.name, root);
								continue;
							}
							// TODO: need to disable timeline animation mode otherwise we have overrides on prefabs if we replace "non missing" bindings
							var isInAnimationMode = AnimationMode.InAnimationMode();
							if (isInAnimationMode)
								AnimationMode.StopAnimationMode();
							// Debug.Log("Update animation target with: " + obj + "\nPrevious binding: " + node.path + "." + node.propertyName + "; " + node.propertyName, obj);
							if (currentlyBoundObject)
								Undo.RegisterCompleteObjectUndo(currentlyBoundObject, "Replace animation target");
							Undo.RegisterCompleteObjectUndo(curve.clip, "Replace curve");
							var objPath = AnimationUtility.CalculateTransformPath(transform, root);
							curve.clip.SetCurve(objPath, curve.type, curve.propertyName, curve.ToAnimationCurve());
							RemoveCurve(gui, node);
							if (isInAnimationMode)
								AnimationMode.StartAnimationMode();
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

		private static bool IsMissing(AnimationWindowHierarchyNode node, AnimationWindowState state = null)
		{
#if !UNITY_2023_1_OR_NEWER
			return AnimationWindowUtility.IsNodeLeftOverCurve(node);
#else
			return AnimationWindowUtility.IsNodeLeftOverCurve(state, node);
#endif
		}

		private static readonly List<Component> _components = new List<Component>();

		/// <summary>
		/// Checks if the dragged object (or any of its components) is assignable to the provided type
		/// </summary>
		private static bool IsAssignable(Object obj, Type type)
		{
			if (obj is GameObject go)
			{
				go.GetComponents(_components);
				foreach (var comp in _components)
				{
					if (type.IsInstanceOfType(comp)) return true;
				}
			}
			else if (obj is Component comp)
			{
				if (type.IsInstanceOfType(comp)) return true;
			}
			return false;
		}

		private static bool IsChild(Transform root, Transform possibleChild)
		{
			var current = possibleChild;
			while (current)
			{
				if (current == root) return true;
				current = current.parent;
			}
			return false;
		}

		private static readonly string[] invalidPropertyNames = new[]
		{
			"m_LocalPosition.",
			"m_LocalRotation.",
			"m_LocalScale.",
		};

		/// <summary>
		/// Dont allow replacing position.x and rotation.x etc individually because it is most likely not intentional (not sure if someone would ever want to replace only one property)
		/// </summary>
		private static bool IsAllowedToReplace(AnimationWindowHierarchyNode node)
		{
			if (node.propertyName != null)
			{
				foreach (var name in invalidPropertyNames)
				{
					if (node.propertyName.StartsWith(name, StringComparison.Ordinal))
						return false;
				}
			}
			return true;
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