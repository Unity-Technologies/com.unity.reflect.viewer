# Rays and Cursors

Ray Interaction Renderers provide visuals that automatically update based on raycast results and hover/capture/active states of ray interactors.

    
## Ray Interaction Line
This component creates an XR Line Renderer. The line begins at the raycast origin. 

If the interactor has hit something, the line ends at the hit point of the raycast; otherwise, it extends in the raycast direction for the maximum distance of the raycast.

If the interactor is selecting something, the end point where stick to object it is selecting and the line will bend. The bending can be disabled in the line settings.

### Line Settings

The line settings asset is used to control various aspects of the line such as color, width, etc.

## Ray Interaction Cursor
The game object this component is attached to will automatically be positioned at the hit point of the interactor’s current raycast.

If it hasn’t hit anything, then the cursor’s game object will be positioned along the ray at the raycast’s maximum distance from its origin.
It can also hide itself when not hovering something.

A Ray Interaction Cursor can also:
    * Scale with its distance from the main camera.
    * Align itself to the normal of whatever surface it hits.
    * Align its up vector to that of the ray origin.

