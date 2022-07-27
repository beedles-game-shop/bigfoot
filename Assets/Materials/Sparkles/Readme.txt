----------------------------------------
SPARKLE FX
----------------------------------------

1. Introduction
2. Emit effects from mesh
3. Scaling effects
4. Recoloring effects
5. Contact

----------------------------------------
1. INTRODUCTION
----------------------------------------

Effects can be found in the 'Sparkle FX/Prefabs' folder. In most cases you can drag & drop them into the scene to have them play.

Prefabs from v1.0 and v2.0 are separated in different folders. This is how the two versions are different:

v1.0 - Textures are pre-colored with 6 color choices. These effects are only designed to emit from a mesh.

v2.0 - Textures use 6 white/modular textures which are then colored through the Particle System's Vertex Color (Start Color). These effects are designed in 3 types- Explosion, Sphere Emitter and a Trail Emitter.

* Explosions will simply explode once in the form of a sphere emitter
* Sphere Emitters also functions as a base for emitting through a Mesh
* Trails Emitters are similar to the Sphere Emitter, but also emit a specific amount while it's moving

----------------------------------------
2. EMIT EFFECTS FROM MESH
----------------------------------------

This is how you make the sparkle effects to emit from a mesh in the scene:

1. Find the wanted sparkle effect you wish to use. This is any effect from Sparkle v1.0 or the Sphere Emitters from Sparkle v2.0).

2. Drop the effect into the scene and locate and open the Shape settings in the parent of the effect. Set the Shape setting to Mesh Renderer.

3. Drag & drop your wanted mesh into the Mesh field in the Shape settings.

4. Now you can play the particle system and watch it emit from the mesh in the Scene window, or alternatively press play and watch it through the Game window.

If you find that the particles are clipping into the mesh you've selected, you can increase the Normal Offset to spawn the particles further from the mesh.

Particles can also inherit the colors from the mesh, if you find out that the sparkle effect's color is off, disable 'Use Mesh Colors'.

----------------------------------------
3. SCALING
----------------------------------------

To scale an effect in the scene, you can simply use the default Scaling tool (Hotkey 'R'). You can also select the effect and set the Scale in the Hierarchy.

Please remember that some parts of the effects such as Point Lights, Trail Renderers and Audio Sources may have to be manually adjusted afterwards.

----------------------------------------
4. RECOLORING EFFECTS
----------------------------------------

Effects from the 'Prefabs/Sparkle v2.0' folder can be recolored by simply changing the Start Color of the effect.

Here you might use one single Color or use one of the sub-options and make it more unique with a Random Color.

----------------------------------------
5. CONTACT
----------------------------------------

Need help with anything? 

E-Mail : archanor.work@gmail.com
Website: archanor.com

Follow me on Twitter for regular updates and news

Twitter: @Archanor
