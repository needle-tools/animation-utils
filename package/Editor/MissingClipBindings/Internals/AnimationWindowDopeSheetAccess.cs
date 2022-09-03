using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;

namespace Needle.AnimationUtils
{
	public class AnimationWindowDopeSheetAccess
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			// DopeSheetEditor.
			// AnimEditor
			// AnimationWindowState
		}
		
		public static Type GetDopeSheetType() => typeof(DopeSheetEditor);
		public static Type GetAnimEditorType() => typeof(AnimEditor);
		public static Type GetAnimationWindowCurveType() => typeof(AnimationWindowCurve);

		
		
		
		private static readonly Dictionary<string, string[]> splitPaths = new Dictionary<string, string[]>();

		public static bool CompareAnimWindowCurvePaths(object animWindowCurve, string otherPath, out int res)
		{
			if (animWindowCurve is AnimationWindowCurve curve)
			{
				if (!splitPaths.TryGetValue(curve.path, out var thisPath)) 
					thisPath = splitPaths[curve.path] = curve.path.Split('/');
				if (!splitPaths.TryGetValue(otherPath, out var objPath)) 
					objPath = splitPaths[otherPath] = otherPath.Split('/');
				res = OriginalCompare(thisPath, objPath);
				return true;
			}

			res = 0;
			return false;

		}

		private static int OriginalCompare(string[] thisPath, string[] objPath)
		{
			int smallerLength = Math.Min(thisPath.Length, objPath.Length);
			for (int i = 0; i < smallerLength; ++i)
			{
				int compare = string.Compare(thisPath[i], objPath[i], StringComparison.Ordinal);
				if (compare == 0)
				{
					continue;
				}

				return compare;
			}

			if (thisPath.Length < objPath.Length)
			{
				return -1;
			}

			return 1;
		}
	}
}