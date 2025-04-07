# Arcade Idle Game Case Study

This project is an Arcade Idle game case study developed using Unity and C#. The main mechanic revolves around collecting, processing, and discarding raw materials.

## Game Features

- **Character Control System**: Easy-to-use movement and interaction mechanics
- **Object Collection and Transport**: Vertical stacking mechanism with the stack system
- **Resource Management**: Collection, processing, and transformation cycle of raw materials
- **AI Characters**: AI system that automatically completes tasks
- **Production Cycle**: Raw Material → Processing → Waste cycle

## Technical Features

- **Object Pool**: Usage of an object pool for performance optimization
- **Event-Driven Architecture**: Loose-coupled components with an EventBus system
- **Dependency Injection**: Use of the Zenject framework
- **Data-Oriented Design**: Separation of data and code with ScriptableObjects
- **SOLID Principles**: Design adhering to clean code principles
- **State Machine**: Behavior control for AI
- **Modular Structure**: Reusable and extendable codebase

## Gameplay

By controlling the player character:
1. Collect raw materials from the spawner
2. Transport these materials to the Asset Transformer machine
3. Take the processed products and discard them in the Trash Can

AI characters automatically perform the same tasks and act intelligently based on warehouse capacity.

## Development Notes

This project focuses on the following principles:
- Modular and extensible code architecture
- Performance optimization
- Clean and readable code
- Event-driven approach with loosely coupled systems
- Reusable components
