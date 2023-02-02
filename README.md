# Overview
This script is part of an AI system for controlling enemy behavior in a video game. It handles the shooting mechanics for a helicopter enemy.

# Usage
The script should be attached to the helicopter GameObject in the scene. It uses several other components, such as the `Game` and `SaveGame` instances, the player's `PlayaDamage` function, and `AudioSource` and `Rigidbody` components. The script also requires the `target` transform, an array of `shots` audio clips, an `explode` prefab, and `guns` transforms to be assigned in the inspector.

# Functions
- `Start()`: This function is called when the script is first run. It sets the `target` to the player's weapon camera, starts the `Shoot` and `UpdateAI` coroutines.

- `Shoot()`: This function is a coroutine that waits for 5 seconds then repeatedly fires tracer projectiles from the helicopter's guns at the player's position. The projectiles are instantiated from a prefab and given an initial velocity towards the player. The function also plays a random sound from the `shots` audio clips and calls the `PlayaDamage` function on the player to deal damage.

- `Fire(Vector3 position, Transform origin)`: This function takes in a `position` to shoot at and an `origin` transform to fire from. It sets the `origin` to look at the `position` then instantiates a tracer projectile from a prefab. The tracer's velocity is set in the direction of the `origin`'s forward axis. The function also calls the player's `PlayerDamage` function to deal damage.

# Copyright
Copyright (c) Robert Rumney. Code released under the MIT license. Feel free to use as you please!
