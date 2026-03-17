# 🎮 Survivors Game –  Overview

This Unity project leverages **DOTS** (Data-Oriented Technology Stack) and **ECS** (Entity Component System) to deliver scalable, high-performance gameplay.

---

## 🧩 Authoring Scripts

Authoring scripts define how GameObjects are converted into ECS entities. Each script includes a **Baker** class that sets up entity components and buffers.

- `PlayerAuthoring.cs`
- `CharacterAuthoring.cs`
- `PlasmaBlastAuthoring.cs`
- `EnemySpawnAuthoring.cs` / `EnemyAuthoring.cs`

---

## ⚙️ Systems

Gameplay logic is implemented as **ECS systems**, each responsible for a specific aspect of the game.

### 🎥 Camera System

Manages camera targeting and movement, ensuring the camera follows the player or relevant entities.

### 🧍 Character System

Handles movement, facing direction, hit points, and damage processing. Updates `CharacterMoveDirection`, movement speed, health, and destruction logic.

### 👾 Enemy System

**`EnemyMoveSystem.cs`**
- Moves enemies toward the player using **parallel ECS jobs**
- Calculates direction based on player position
- Updates `CharacterMoveDirection`

**`EnemyAttackSystem.cs`**
- Handles attack logic and cooldowns
- Uses physics events to detect collisions and apply damage via `DamageThisFrame`

### 🧑 Player System

**`PlayerAttackSystem.cs`**
- Handles player input, attack cooldowns, and spawning attack entities
- Updates `PlayerAttackData`

**`PlayerWorldUISystem.cs`**
- Updates world-space UI elements such as health bars and gem counts

**`UpdateGemUISystem.cs`**
- Synchronizes gem collection data with UI for accurate display

---

## 🖥️ UI

**`GameUIController.cs`** manages gem count display, game over screens, and pause/resume functionality.

---

## 🧠 Key Concepts

| Concept | Description |
|---|---|
| **ECS Systems** | All gameplay logic handled by systems implementing `ISystem` or `IJobEntity` |
| **ComponentData** | Data stored in structs implementing `IComponentData` for efficient access |
| **Baker Classes** | Convert MonoBehaviours into ECS entities at build time |
| **Burst Compilation** | Systems and jobs use `[BurstCompile]` for optimized performance |

---

## 🚀 How to Extend

- **Add gameplay features** — create new systems or components
- **Define new entity types** — use authoring scripts and Bakers
- **Extend UI** — add feedback, controls, and interactions

---

## 👤 Author

**Parth Pandya**
- GitHub: [parthpandya008](https://github.com/parthpandya008/)
- LinkedIn: [parthpandya008](http://www.linkedin.com/in/parthpandya008)

---

***Enjoy building Unity games with DOTS!*** 🚀
