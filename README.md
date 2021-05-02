# Velcro Physics (Formerly Farseer Physics)

[![Build](https://img.shields.io/github/workflow/status/Genbox/VelcroPhysics/Generic%20build?label=Build)](https://github.com/Genbox/VelcroPhysics/actions)
[![License](https://img.shields.io/github/license/Genbox/VelcroPhysics)](https://github.com/Genbox/VelcroPhysics/blob/master/LICENSE.txt)

## Warning: Under construction
The project is under development. [Consider donating](https://github.com/sponsors/Genbox) to support the effort.

## What is this?
Velcro Physics is a high performance 2D collision detection system with realistic physics responses. It can be used to create games or real-time robotic simulations.

## What is it good for?
You can create a game, robotic simulatons or even UI feedback systems using this engine and associated tools. Everything from a simple platform game to Marsrover simulations are possible.

## Features
We have tons of features!

* Continuous collision detection (with time of impact solver)
* Contact callbacks: begin, end, pre-solve, post-solve
* Convex and concave polygons and circles.
* Multiple shapes per body
* Dynamic tree and quad tree broadphase
* Fast broadphase AABB queries and raycasts
* Collision groups and categories
* Sleep management
* Friction and restitution
* Stable stacking with a linear-time solver
* Revolute, prismatic, distance, pulley, gear, mouse joint, and other joint types
* Joint limits and joint motors
* Controllers (gravity, force generators)
* Tools to decompose concave polygons, find convex hulls and boolean operations
* Factories to simplify the creation of bodies

## Packages
You can use VelcroPhysics as a normal .NET library. We also have zero-copy integration with [MonoGame](http://www.monogame.net/).

* [![NuGet](https://img.shields.io/nuget/v/Genbox.VelcroPhysics.svg?style=flat-square&label=Library)](https://www.nuget.org/packages/Genbox.VelcroPhysics/)
* [![NuGet](https://img.shields.io/nuget/v/Genbox.VelcroPhysics.MonoGame.svg?style=flat-square&label=MonoGame)](https://www.nuget.org/packages/Genbox.VelcroPhysics.MonoGame/)

## Sponsored Project
This is a [sponsored project](https://github.com/sponsors/Genbox). That means in order to create issues and pull requests, or use the project commercially, you must become a sponsor. Projects like this take up a lot of time and effort, so please help keeping open-source projects alive by becoming a sponsor today. You can stop your sponsorship anytime you want.