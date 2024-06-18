# BUGSKILL

## What It Does
Set in a dynamically mapped real-world environment, BUGSKILL challenges players to eliminate all flies in sight using their hands and quick reflexes. Players earn money by killing flies, and they can do so by slapping against surfaces, clapping them in air, and purchasing a variety of power-ups to snap them up. This immersive experience is enhanced with spatial audio cues and realistic interactions facilitated by Meta’s advanced hand tracking.

## How We Built It
Built with Unity and leveraging Meta Quest SDKs, BUGSKILL utilizes a wide range of tools, including Passthrough, Scene API, Depth API, Mixed Reality Utility Kit, and Audio SDK.

Through the use of Scene API and Mixed Reality Utility Toolkit, we were able to procedurally calculate the flight path of the flies and spawn them near specific areas within a space. By adding colliders to the scene volumes, the flies can check for any possible obstacles in their paths and maneuver around physical objects in the space. The Mixed Reality Utility Toolkit also provides a convenient method for generating random points on any vertical or upward-facing surfaces. By utilizing the semantic labels and bounds provided by the scene model, we can ensure that the flies only spawn near windows or doors.

We intentionally designed the game with hand tracking only to evoke the feeling of catching flies in real life. Wide Motion Mode significantly enhances hand tracking accuracy and reduces lag, enabling us to maintain high-paced gameplay without compromising tracking quality. The hand physics capsules also handle collisions with precision, inspiring the implementation of various hand-based mechanics, such as clapping and slapping against surfaces.

We leveraged the capabilities of spatial audio to provide users with a better sense of the flies' locations. Using Meta's Audio SDK, we implemented audio cues to easily track where the flies are relative to the player's head. Additionally, we added the Doppler effect to flies that are moving toward or away to heighten players' sensitivity to targets.

Passthrough is key to the immersive gameplay of BUGSKILL. Players feel safer moving and navigating in Passthrough mode, allowing us to take advantage of a larger play area and provide them with a sense of agency during gameplay. Furthermore, occlusion by the Depth API truly elevates the game to the next level, as flies can easily hide behind physical objects, encouraging players to strategize carefully. By intentionally prompting users to apply everyday hand gestures and interact with physical space within the headset, the game enhances players' spatial awareness and sense of safety with mixed reality technology.

