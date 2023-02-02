# Overview
This script is an AI system for controlling enemy behavior in a video game. It handles the shooting & movement mechanics for a helicopter enemy.

## Usage
1. Attach the script to an empty GameObject in your Unity scene
2. Assign the required variables in the Inspector:
  - `target`: the Transform component of the object the helicopter should follow and shoot at
  - `guns`: an array of Transforms, representing the positions of the guns on the helicopter
  - `shots`: an array of AudioClips, representing the different shooting sounds
  - `tracer`: the prefab of the tracer object that should be instantiated when shooting
  - `explode`: the prefab of the explosion object that should be instantiated when the helicopter is destroyed
  - `attackDistance`: the distance at which the helicopter should start shooting at the player
  - `fireRate`: the frequency at which the helicopter should shoot
  - `oldCollider`: the Collider component of the helicopter that should be disabled when it is destroyed
  - `newCollider`: the Collider component that should be enabled when the helicopter is destroyed
  - `health`: the starting health of the helicopter
3. Ensure the player has a `PlayerDamage` method that reduces the player's health, and a `hitPoints` property that returns the player's current health

The script should be attached to the helicopter GameObject in the scene. It uses several other components, such as the `Game` and `SaveGame` instances, the player's `PlayerDamage` function, and `AudioSource` and `Rigidbody` components. The script also requires the `target` transform, an array of `shots` audio clips, an `explode` prefab, and `guns` transforms to be assigned in the inspector.

## Weapons
The fireRate variable determines how frequently the helicopter will fire its weapons, and the attackDistance variable determines the distance at which the helicopter will start firing. The helicopter will follow the player until it reaches the attack distance, at which point it will start firing its weapons and continue to pursue the player until either it or the player is destroyed.

## Movement
The helicopter AI in this script is designed to follow the player's position on the x and z axis while maintaining its own y axis elevation. This is achieved through the implementation of a number of variables that control the behavior of the helicopter.

The health variable is the amount of damage the helicopter can sustain before it is destroyed. The speed variable determines how quickly the helicopter moves towards the player, while the turnSpeed variable determines how quickly the helicopter can change direction. The targetHeight and currentHeight variables determine the desired and current heights of the helicopter, respectively, while the offset variable is used to calculate the height offset between the two.

To achieve the desired movement, the helicopter's position is continuously updated based on the player's position, using the x and z axis only. The helicopter's y axis elevation is maintained by continuously adjusting the currentHeight variable to match the targetHeight. This allows the helicopter to follow the player while maintaining its own altitude.

## Coroutines
This script implements two coroutines:
1. `Shoot`: handles the shooting behavior of the helicopter
2. `UpdateAI`: handles the following behavior of the helicopter

## Methods
1. `Start`: initializes the target and starts the `Shoot` and `UpdateAI` coroutines
2. `Fire`: instantiates a tracer object and calls the player's `PlayerDamage` method
3. `Die`: handles the death behavior of the helicopter, including calling the `Achieve` method and disabling/enabling the correct colliders
4. `Fall`: starts the `Falling` coroutine
5. `ApplyDamage`: reduces the health of the helicopter and calls the `Fall` method if the health falls below a certain threshold

# Copyright
Copyright (c) Robert Rumney. Code released under the MIT license. Feel free to use as you please!
