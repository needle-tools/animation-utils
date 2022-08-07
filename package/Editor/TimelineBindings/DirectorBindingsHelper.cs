#if TIMELINE_INSTALLED

using System.Linq;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine.Playables;

namespace Needle.AnimationUtils
{
    public static class DirectorBindingsHelper
    {
        /// <summary>
        /// Copies the current binding state from the PlayableDirector, sets the new timeline and pastes the bindings.
        /// </summary>
        public static void ReplaceTimelineAndCopyBindings(PlayableDirector director, PlayableAsset targetTimeline)
        {
            var clipboard = GetCurrentBindings(director);
            director.playableAsset = targetTimeline;
            SetBindingsFromClipboard(director, clipboard);
        }
        
        private static (PlayableDirector director, PlayableAsset asset) bindingClipboard;
        private const int BasePriority = 50000;
        
        [MenuItem("CONTEXT/PlayableDirector/Copy Bindings", false, BasePriority)]
        private static void CopyBindings(MenuCommand cmd) => bindingClipboard = GetCurrentBindings(cmd.context as PlayableDirector);

        [MenuItem("CONTEXT/PlayableDirector/Paste Bindings", true, BasePriority + 1)]
        private static bool PasteBindingsValidate(MenuCommand cmd) => bindingClipboard.director;
        
        [MenuItem("CONTEXT/PlayableDirector/Paste Bindings", false, BasePriority + 1)]
        private static void PasteBindings(MenuCommand cmd) => SetBindingsFromClipboard(cmd.context as PlayableDirector, bindingClipboard);

        private static (PlayableDirector director, PlayableAsset playableAsset) GetCurrentBindings(PlayableDirector director)
        {
            return (director, director.playableAsset);
        }

        private static void SetBindingsFromClipboard(PlayableDirector director, (PlayableDirector director, PlayableAsset asset) clipboard)
        {
            if (!director) return;
            var outputs = director.playableAsset.outputs.ToList();
            var origOutputs = clipboard.asset.outputs.ToList();
            Undo.RegisterCompleteObjectUndo(director, "Paste Bindings");
            for (int i = 0; i < outputs.Count; i++)
            {
                if (!outputs[i].sourceObject) continue;
                director.SetGenericBinding(outputs[i].sourceObject, clipboard.director.GetGenericBinding(origOutputs[i].sourceObject));
            }
            EditorUtility.SetDirty(director);
            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved | RefreshReason.WindowNeedsRedraw);
        }
    }
}
#endif