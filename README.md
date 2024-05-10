# Catch dem flies (tentative)

You can find all the scripts in Assets/Scripts.

---


**Fly Movement**

This handles all fly behavior, e.g. direction and speed of movement, finding landing surface and rest etc. It's attached to the Fly prefab in Assets/Prefabs.

Jasmine: I believe the logic here will need to be redone if we want to use the MRUK function call to get a random optimal  point for landing. I'm down to discuss with you if it's helpful!

---

**Environment Setup**

This adds the walls and ceiling and floor to the Landing Surface layer (hardcoded). This gets called in the MRUK's OnSceneLoaded event.

---

**Game Manager**

This manages where the fly gets spawned and spawn flies at a random time. It also contains a list of blood splat objects that can be accessed easily.

---


**Hand Controller**

This script is attached to the hands - under the OVRCameraRig > HandAnchor > (Left/Right)HandCollider. Basically each hand has a box collider, roughly mapped to fit the size of the hand.

This script simply checks if the hand is touching both the fly and a landing surface at the same time. The layer number is hardcoded so make sure this is updated if you tinker with layers.

The commented out parts is supposed to detect hand clap and catch fly mechanism. Feel free to uncomment to try it out :) This should also be useful for Nwalahnjie for the fly slowing down stuff.
