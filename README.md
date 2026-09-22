# 🐍 Orecoil

> **Status:** In Development  
> **Engine:** Unity  
> **Developer:** brambach

## 📖 Overview
**Orecoil** takes the classic Snake formula and adds a single, clean strategic twist. You guide a serpent around a grid, eating "Ore" to grow, but with a catch: every piece of Ore fills a meter.

When full, this meter allows you to trigger a **Shield**, forgiving your next collision. This creates a risk-reward loop where you can push your luck for a high score or "cash in" your meter to survive a tight corner. There are no crafting trees or RPG systems, just a familiar arcade feel with an earned safety valve.

---

## 🎮 Controls

| Action | Input |
| :--- | :--- |
| **Movement** | `Arrow Keys` or `WASD` |
| **Activate Shield** | `Space` (Requires full meter) |
| **Restart Game** | `Enter` |
| **UI Interaction** | `Mouse` |

---

## ⚙️ Game Mechanics

### **The Loop**
1.  **Collect:** Ore spawns one at a time. Collecting it increases length, score, and adds one "pip" to the Shield meter.
2.  **Charge:** Collecting three pieces of Ore fully charges the Shield.
3.  **Survive:** Pressing `Space` activates the Shield for a short window. While active, it negates one wall or self-collision.
4.  **Escalate:** Every 60 seconds, the game speed increases slightly to maintain tension.

### **Loss Conditions**
* Colliding with a wall or your own tail while the Shield is **inactive**.

---

## 🎨 Aesthetic & Audio
* **Visuals:** A readable, minimal grid. Ore appears as a bright, distinct pickup. When the Shield is active, the snake head highlights to signal safety.
* **Audio:** A light, steady looping track sits beneath the gameplay. Sound effects are kept punchy but non-intrusive, with distinct cues for Shield activation and Game Over stings.

---

## 🛠️ Development & Technical Challenges

### **Input & Feel**
* **Input Buffering:** To ensure the game feels fair at higher speeds, input buffering is implemented so quick turns register cleanly without being "eaten" by the frame rate.
* **Fair Spawning:** The spawn logic includes validation to ensure Ore never spawns directly on the snake's body segments.

### **Balance Tuning**
* **Shield Pacing:** The core design challenge is tuning the "Pips per Shield" and "Shield Duration." If it charges too fast, the game becomes trivial; too slow, and the player never feels the benefit.
* **Visual Clarity:** As the snake grows, the screen becomes crowded. The art style prioritizes high contrast and simple colors to keep the board readable during late-game runs.

---

## 🚀 Roadmap & Stretch Goals

* **Core Release:** Polished single-arena gameplay with the Shield mechanic.
* **Stretch Goal (Trim Mechanic):** A potential feature allowing players to spend a full Shield charge to pause the game and remove the last three tail segments. It's a "panic button" for impossible situations.

---

## 💻 Tech Stack
* **Engine:** Unity 2022+
* **Languages:** C# (Logic), ShaderLab/HLSL (Visuals)
