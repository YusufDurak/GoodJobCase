Developed for: Good Job Games Summer Internship Case Engine: Unity 2022.3 LTS Focus: Performance, Algorithm Design, Game Feel

##Overview

This project is a technical implementation of the core "Blast" mechanic found in top-tier puzzle games. The primary goal was to build a robust, scalable foundation that prioritizes mobile performance and memory management while delivering a satisfying "juicy" gameplay experience.

Technical Architecture & Decisions
1. Zero-Allocation Object Pooling
The case documentation emphasized CPU/GPU optimization. To avoid the performance cost of Unity's Instantiate and Destroy calls during gameplay: I implemented a custom BlockPool system. Blocks are pre-allocated at initialization. During gameplay, blocks are recycled (deactivated/activated), ensuring zero Garbage Collection allocation during the core loop.

2. Matching Algorithm: BFS over Recursion
For the color matching logic (Flood Fill), I chose Breadth-First Search (BFS) using a Queue instead of a recursive Depth-First Search (DFS). Why? Recursion can lead to Stack Overflow errors on larger grids (e.g., 10x10 or larger custom sizes). BFS is iterative, safer for memory, and more performant on mobile devices.

3. "Smart" Deadlock Shuffle
To solve the "No Moves" situation, I avoided the naive approach of "blindly shuffling until a match is found" as it relies on luck and can cause infinite loops. 

My Solution (Deterministic):

Detect Deadlock (scan grid for any group < 2).

Collect all active blocks and shuffle their data.

Force a Match: Manually place a pair of matching colors at a random coordinate to guarantee at least one valid move.

Update the visual state.

4. Visual Feedback (The "Juice")

To elevate the prototype to a polished feel:

DOTween Integration: Used for smooth falling animations (DOLocalMove) and spawn punches (DOScale).

Dynamic Frame: Implemented a 9-Slice scalable frame that automatically adjusts to the M x N grid dimensions.

Responsive Camera: The camera automatically calculates the required orthographic size to center the grid regardless of the aspect ratio.

##Features

Configurable Grid: M (Rows), N (Cols), and K (Colors) are fully adjustable via Inspector.

Dynamic Icons: Blocks change their sprites (Default, Icon1, Icon2, Icon3) based on the connected group size (Thresholds A, B, C).

Input System: Utilized the legacy Input Manager for simplicity and reliability in touch/click detection.

Particle Effects: Optimized particle system that simulates explosions using the block's color data.

##Project Structure

Scripts/Core:

BoardManager.cs: Handles the grid logic, input, and game state.

BlockPool.cs: Manages memory and object recycling.

Scripts/Entities:

Block.cs: Handles individual block visuals and animations.

Scripts/Utils:

AudioManager.cs: Manages SFX with pitch randomization for variety.

VFXManager.cs: Handles particle spawning without instantiation spam.

##How to Test

Open the GameScene.

Select the GameBoard object.

Adjust Rows, Columns, and Colors in the Inspector to test edge cases (e.g., 2x2 or 10x10).

Press Play.

Notes: This project was a great opportunity to demonstrate Data-Oriented thinking in Unity. By separating the logic (Board) from the view (Block), the system remains modular and easy to extend (e.g., adding Power-ups or Level goals).