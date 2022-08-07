# Animation Utils

This package contains some utilities that help working with Unity's timeline and animation tools.

### Missing Animation Clip Binding: Drag & Drop to Rebind
If your animation clip is missing bindings, you can just drag & drop objects from your hierarchy to fix them.  
Hold the <kbd>**ALT**</kbd> key to re-assign existing bindings in your animation clip.  

![video2](https://user-images.githubusercontent.com/5083203/183306599-d8ad8696-4527-46a0-8edd-d719eebf14cf.gif)

### Copying Timeline Assets: copy PlayableDirector bindings as well
`PlayableDirector` stores bindings per `PlayableAsset`. This makes runtime switching easy, but editor usage hard: when you duplicate a timeline asset and assign it to the director, all your bindings are lost and need to be manually re-assigned. Timelines can be complex (hundreds of objects), so this is very error-prone.  

This package adds a simple context menu to `PlayableDirector` with two entries:
- Copy Bindings
- Paste Bindings

![video3](https://user-images.githubusercontent.com/2693840/183313252-bb213864-462c-4fd4-ad6f-f08fd25c00a8.gif)

So the following flow now works:
- select your PlayableDirector
- right click > <kbd>Copy Bindings</kbd>
- duplicate the timeline asset in the Hierarchy
- assign it as playable asset
- right click again > <kbd>Paste Bindings</kbd>

## Contact ✒️
<b>[needle — tools for unity](https://needle.tools)</b> • 
[Discord Community](https://discord.needle.tools) • 
[@NeedleTools](https://twitter.com/NeedleTools) • 
[@marcel_wiessler](https://twitter.com/marcel_wiessler) • 
[@hybridherbst](https://twitter.com/hybridherbst)
