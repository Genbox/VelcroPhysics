# Welcome to Velcro Physics (Formerly Farseer Physics Engine)

## Warning: Under construction
It has been years since this code was last touched, and technology has moved a lot since. The code is currently under construction and subject to change. If you need a physics engine right now, please use the [previous release](https://farseerphysics.codeplex.com/releases/view/110074).

## What is this?
Velcro Physics is a high performance 2D collision detection system with realistic physics responses.

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

## Integration
You can run VelcroPhysics in a console application without any dependency on third party game libraries. See VelcroPhysics.sln for an example. We have zero-copy integration with [MonoGame](http://www.monogame.net/), which means if you already use MonoGame for your game, VelcroPhysics uses the same Vector2 clases and you don't have to copy between different vector types. See VelcroPhysics.MonoGame.sln for an example on how to use MonoGame with VelcroPhysics.
