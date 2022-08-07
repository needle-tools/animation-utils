using UnityEditor;
using UnityEditorInternal;

namespace Needle
{
    public static class AnimationWindowHierarchyNodeAccess
    {
        public static bool IsMissing(object obj, out EditorCurveBinding? binding)
        {
            if (obj is AnimationWindowHierarchyNode node)
            {
                binding = node.binding;
                return AnimationWindowUtility.IsNodeLeftOverCurve(node);
            }
            binding = null;
            return false;
        }
    }
}
