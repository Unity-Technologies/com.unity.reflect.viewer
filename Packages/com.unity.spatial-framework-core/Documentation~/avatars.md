# XR Avatars

Avatars and voice chat are crucial to any multiplayer XR app. The Spatial Design Framework's avatars are meant to be modular and customizable to your application.

## Known limitations
​
*_XR Avatars have the following known limitations:_*
* *_If you are planning to update the XR avatars to use the Universal Render Pipeline, the "Avatar Outline.mat" will need to be updated to use the "SpatialFramework/OutlineURP" shader instead of "SpatialFramework/Outline"_*
* *_Avatar tag billboarding follow behavior needs to be fixed to accommodate blink locomotion or other sudden XR rig movements_*

## XR Avatar Features

* Head pose/Controller Representation.

    - The current avatar designs include a head and controller representation to give users the ability to gesture and point at objects in their environment.


* Avatar Tag.

    - The avatar tag displays the avatar's name and initials, along with a customizable color or profile pic. The tag has a basic billboarding behavior, and responds to view distance by changing it's overall scale and by adjusting the amount of information shown. At far distances, the tag only displays the color and initials of the collaborator, without the full name or audio icon.


* Audio Indicator.

    - The avatar audio indicator is used to show when a person is speaking in a collaborative environment. These auditory cues are shown through the avatar tag as well as animating the avatar's mouth.


* View Responsive Material.

     - It is very jarring when avatars intersect and overlap in multiplayer environments. In order to prevent this, each avatar has a collider attached to the head pose that when triggered, adjusts the visuals associated.


* Eye Gaze.

    - Presence is enhanced by providing simple facial features that include a blink cycle and the ability to infer eye direction.

<a name="Workflows"></a>

## Avatar workflows

To use an XR avatar:

Go to Spatial Framework > Runtime > Prefabs > Avatar and drag out the prefab called "Avatar Player" into your Unity Scene. On the root of the prefab, there is the `AvatarControls` component. Since the XR avatar design is generic from the multiplayer solution that you may choose in your application, this component is used to set various properties of your XR avatar. Properties include the avatar name, position of the head/controllers, color, muted state, volume loudness, and any parts of the avatar geometry that will include color accents (i.e. a helmet would be colored the same as the label).    

In order to animate the XR Avatar's mouth and audio label indicator, there are two properties of the `AvatarControls` component that we need to be concerned with; the `muted` and `normalizedMicLevel` properties. By setting the `muted` to true or false, you swap different icons in the avatar label. The `normalizedMicLevel` property is used to animate the mouth and the audio label indicator's particle system. The `normalizedMicLevel` property ranges from 0.0 to 1.0.

The XR Avatar's name and color can be set using the `avatarName` and `color` properties of the `AvatarControls` component. When the name is set, label's name and initials are updated above the avatar. When the color property is set, the label's color changes along with any accent geo that is set in the `m_ColoredGeo` property (this requires that the material attached to the MeshRenderer of the GameObject has a `color` property).   
