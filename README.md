# Lightsabers-Tutorial

### Foreword

This is an educational tutorial, this free asset is for training purposes only.

### Introduction

Two years ago, when I started with Unity, I created lightsaber (#1) like weapons to toy around in VR. Those have always been free for personal usage. Today I thought I'd try lightsaber like weapons in HDRP and share them as well. You can find the source code on github:

### Preview

Here's a preview video of what this is about:

[![How to create Lightsaber like Weapons in Unity](https://img.youtube.com/vi/SJ0ZCCjO5aQ/0.jpg)](https://www.youtube.com/watch?v=SJ0ZCCjO5aQ)

*Note: I kept the HDRP standard demo scene that everyone is familiar with.*

### Quick Setup

Here are the quick setup steps:

* create a new project "Weapon Tutorial" using "High-Defintion RP" in Unity 2019.1.5f1

* enable Realtime Global Illumination in Window -> Rendering -> Lighting Settings

* add Bloom: Rendering Settings -> Add Override -> Post-processing -> Bloom

  set Intensity 0.491, Scatter 0.487 (whatever you prefer for the glow effect)

* import the [source code from github](https://github.com/Roland09/Lightsabers-Tutorial) or the [weapons_tutorial.unitypackage](https://github.com/Roland09/Lightsabers-Tutorial/blob/master/Release/weapons_tutorial.unitypackage)

* drag the "Weapon (Single)" or "Weapon (Double)" or "Weapon (Cross)" prefab into the scene

* hit play

* press space to toggle the weapon on and off

### Additional Info

* the shader graph is a modified shader graph conversion of the one I posted in the [Amplify Shader Editor thread](https://forum.unity.com/threads/best-tool-asset-store-award-amplify-shader-editor-node-based-shader-creation-tool.430959/page-32#post-3147421)

  (Credits to "K Re" who did the glow effect graph tutorial for the unreal engine)
 
* you might want to add audio for the weapon on / off / loop / swing sound in the Weapon script settings; unfortunately I'm not allowed to provide them

* regarding light darkening when the weapons activate: if you don't want that, set "Default Post-process" -> Exposure -> Mode: Fixed and adjust Fixed Exposure value. I kept it with the slight darkening, because I like how it adds to the atmosphere



### How it works

... to be done...


### Addendum

#1) Please note that Lightsaber is a registered trademark owned by LUCASFILM ENTERTAINMENT COMPANY LTD. The word is being used as reference because it's common knowledge.


