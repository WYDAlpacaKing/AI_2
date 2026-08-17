# Pacman 死亡机制与确定性来源调查报告

> 调查目的：解释"为什么一个 AI 每次运行都会以完全相同的方式死亡"。
> 调查范围：`Assets/Games/Core/`、`Assets/Games/Pacman/Scripts/`、`Assets/Games/Pacman/AIs/Ghosts/`、`Assets/Games/Pacman/Pacman.unity`、`Assets/AlanZucconi/Scripts/`（Linq 扩展、IntervalList）。

---

## summary（结论先行）

**"每次运行都以完全相同方式死亡"是确定性系统的必然结果——前提是该 AI 在死亡前从未吃过能量豆（power pellet）。**

1. **框架的游戏逻辑几乎完全确定**：回合制协程循环（非 FixedUpdate、非真实时间驱动），每回合按固定顺序执行"所有 Agent 移动 → 碰撞检测 → 状态更新"。回合数（`Game.Turn`）是唯一的时间维度，没有任何 `Time.*`、帧率或墙钟参与决策。
2. **唯一的非确定性来源是"惊吓态（Frightened）鬼的随机目标"**：`GhostAI.cs:223` 的 `.Random()` 调用 `UnityEngine.Random.Range`（`LinqExtension.cs:423`），且本项目**没有任何地方设置种子**（`DeterministicRandom` 结构体存在但被注释禁用，场景/资产中无 `Salt`/`Sequence` 字段）。一旦任何鬼进入 Frightened 状态，后续序列即不可复现。
3. **触发 Frightened 的唯一途径是 Pacman 吃到 power pellet**（`Pacman.cs:123-136` 令所有鬼 `FrightenedTime = PowerPelletTimer`）。因此：
   - **若 AI 死亡前未吃任何 power pellet → 全程零随机数调用 → 100% 确定 → 每次运行必定同样时间、同样位置、同样方式死亡**（这正是用户观察到的现象）；
   - 若死亡前吃过 power pellet，则每次运行结果应该不同；"每次相同"反过来证明该 AI 的死亡发生在确定性阶段。
4. **死亡即游戏结束**：Pacman 被鬼吃掉的当回合（碰撞检测阶段）置 `Eaten=true`，回合末 `IsGameOver()` 返回 true，协程 `StopGame()` 立即终止，无复活、无续关、无状态重置。Automation 的下一轮测试从 `InitialiseGame()` 全新开始。
5. **"半格移动"存在但不影响死亡判定**：`FloatPosition` 是 float（每回合按 `Speed` 0.75/0.8 累积，跨多回合才走完一格），但碰撞判定完全基于整数格 `Position`/`OldPosition`，并带"交换位置"检测，网格上不存在擦肩而过（详见第 2 点）。

---

## evidence（分点证据）

### 1. 确定性来源

#### 1a. `DeterministicRandom.cs` 的实现——存在但**完全未被启用**

- `DeterministicRandom.cs:8-40`：`struct`，含 `Sequence`（`Random`/`Seeded`）与 `Salt` 两个字段；`Get(int x)` 以 `x ^ Salt` 为种子构造 `System.Random`（第 35-40 行）——即"按回合号可预测地取随机"。
- `DeterministicRandom.cs:24-32`：`Initialise()` 在 `Sequence==Random` 时用 `UnityEngine.Random.Range(int.MinValue, int.MaxValue)` 随机化 Salt。
- `DeterministicRandom.cs:47-59`：`DeterministicRandom<T>(source, random, x)` 扩展方法，用 `random.Get(x).Next(count)` 选元素。
- **关键：全项目只有定义本身，没有任何活跃调用**：
  - `GhostAI.cs:62` `//public DeterministicRandom RandomSource;` —— 字段被注释；
  - `GhostAI.cs:116` `//RandomSource.Initialise();` —— 被注释；
  - `GhostAI.cs:222` `//.DeterministicRandom(RandomSource, Game.Turn);` —— 被注释，替换为 `.Random()`。
- 对 `Assets/Games/Pacman` 下所有 `.unity/.asset/.prefab` 搜索 `Salt|SequenceType|RandomSource`：**零命中**。即没有任何种子被序列化/配置。

#### 1b. `Game.cs` 的游戏循环——协程回合制，**不是 FixedUpdate**

- `Game.cs:34-86` `GameLoop_Coroutine()`：
  - `Turn = 0; InitialiseGame(); Running = true;`（41-43 行）；
  - `while (Running) { Turn++; UpdateGame(); if (IsGameOver()) { StopGame(); yield break; } ... }`（53-67 行）；
  - 节奏控制 `Game.cs:72-77`：`waitTime = Mathf.Max(0f, Delay - elapsedSeconds)`，`yield return new WaitForSeconds(waitTime)`——**仅控制回放速度，不参与任何决策**。
- 场景中 `Delay: 0.1`（`Pacman.unity:957`）；Automation 跑分时置 `Delay = 0`（`PacmanAutomation.cs:93`），每回合 `yield return null`（`Game.cs:74-75`）跑满速。
- 全 Pacman 脚本中搜索 `Time.time|Time.deltaTime|Time.fixedDeltaTime|Time.frameCount`：**零命中** → 决策与真实时间完全解耦，只依赖整数 `Turn`。
- 起始方式：`Game.cs:29-33` `StartGame()` 是 `[Button]`；场景无自动启动；Automation 通过 `Game.StartGame()`（`PacmanAutomation.cs:99`）驱动。

#### 1c. 哪些随机数参与决策（完整清单）

| 位置 | 代码 | 是否启用 | 说明 |
|---|---|---|---|
| 惊吓态鬼目标 | `GhostAI.cs:223` → `LinqExtension.cs:409-425` `.Random()` → `UnityEngine.Random.Range(0,count)` | **启用** | 唯一活跃的随机源；无种子；编辑器下每次 Play 的 RNG 状态不重置 → 跨运行不可复现 |
| `DeterministicRandom.Initialise()` | `DeterministicRandom.cs:31` | 未启用（无实例） | — |
| 示例 AI | `PacmanAI_Random.cs:28` `Random.Range` | 不在场景中 | 教学示例，未挂载 |
| Evolution | `EvolutionSystem.cs` 多处 `Random.Range` | 不在 `Pacman.unity` 场景 | 演化专用场景 |
| 学生 AI | `PacmanAI_ywang146.cs`（及 v1/v2） | 无随机 | grep `Random|Time.|Guid|Salt` 零命中，纯效用函数 |

#### 1d. 同一场景重复 Play 是否产出相同序列？

- **是，只要不触发 Frightened**：初始状态每次由 `InitialiseGame()`（`PacmanGame.cs:68-79` → `Agent.Initialise`，`Agent.cs:82-101` 重置 `Position/OldPosition/Action/Eaten/FrightenedTime/PowerPelletTimer`）与 `GhostAI.Initialise()`（`GhostAI.cs:105-117` 重置 `State=Scatter, CurrentDirection=None`）重建；AI 副本由 `CloneAndDestroyOriginal`（`PacmanGame.cs:83-95`）从原始 asset 重新 `Instantiate`，不累积状态。
- 浮点运算（`Agent.cs:162-179`）为 IEEE-754 同机确定，逐回合序列可复现。
- **一旦某回合有任何鬼处于 Frightened**，`GhostAI.cs:218-224` 用未播种的 `UnityEngine.Random` 选目标 → 后续全部不可复现。

---

### 2. 死亡判定（PacmanGame.UpdateGame / UpdateAgentPositions）

#### 2a. 移动与碰撞的顺序：**先全部移动，后统一检测**

- `PacmanGame.cs:173-191` `UpdateGame()`：
  1. `UpdateAgentPositions()`（177 行）——含 AI 决策 + 移动 + 碰撞；
  2. 然后 `foreach agent.UpdateState()`（181-182 行）——吃豆、计时器。
- `PacmanGame.cs:219-285` `UpdateAgentPositions()`：
  1. 第一轮 `foreach (Agent agent in Agents) agent.UpdatePosition();`（222-223 行）——**所有** Agent（Pacman 在前，随后按 `Ghosts` 列表顺序 Blinky→Pinky→Inky→Clyde，见 331-336 行 `Agents = Ghosts.Prepend(Pacman)`）先各自移动；
  2. 第二轮 `foreach (agentB in Ghosts)` 对每个鬼与 Pacman 检测（237-250 行）：
     - `movingIntoSameTile = agentA.Position == agentB.Position`（242-243 行）——同格；
     - `swappingPlaces = agentA.OldPosition == agentB.Position && agentB.OldPosition == agentA.Position`（244-246 行）——互换；
     - 任一成立 → `ResolveCollision(agentA, agentB)`（248-249 行）。
- `PacmanGame.cs:288-305` `ResolveCollision`：先试 `pacman.CanEat(ghost)`（吃鬼），否则试 `ghost.CanEat(pacman)`（被吃）。被吃 → `ghost.OnEating(pacman); pacman.OnEatenBy(ghost)` → `Agent.OnEatenBy` 置 `Eaten = true`（`Agent.cs:123-130`）。
- 判定条件：`Ghost.CanEat(pacman)`（`Ghost.cs:75-82`）= `!IsFrightened() && !Eaten && !pacman.Eaten && !Game.Invincible`；`Pacman.CanEat(ghost)`（`Pacman.cs:166-170`）= `ghost.IsFrightened() && IsPoweredUp() && ...`。

**结论**：Pacman 与鬼"同一回合到达同一格"即死亡；"鬼移动到 Pacman 所在的格子"或"双方互换格子"都会被判死。由于 Pacman 先移动、鬼后移动，鬼的 `Move()`（`Agent.cs:152-182`）能看见 Pacman 本回合的新位置。

#### 2b. "半格移动"是否存在？——存在（仅渲染），但不产生擦肩而过

- `Agent.cs:38` `FloatPosition` 为 float；`Agent.cs:162` `targetFloatPosition = FloatPosition + action.ToV2I() * Speed`；`Speed` 0.75（鬼）/0.8（Pacman）→ 一回合不足 1 格，`Position`（整数格，`Agent.cs:20`）只在 `FloorToInt` 跨格时更新（`Agent.cs:163,176`）。
- `Agent.cs:185-206` `SnapFloatPosition()`：转向/停顿时把 `FloatPosition` 拉回格心 `Position + CENTRE`；反向（`Action.IsReverseOf`，`Action.cs:70-86`）时保留浮点进度。
- **碰撞检测完全基于整数格 + 互换检测**（`PacmanGame.cs:242-246`），与 `FloatPosition` 无关。因为每回合每 Agent 最多移动 1 格，且新旧位置都记录，网格上不存在"擦肩而过"：相向而行要么同时跨格（→ swap 检测），要么一方跨格一方留守（→ 同格检测），总能命中。`FloatPosition` 仅用于 `Draw()` 的平滑插值（`Agent.cs:246-248`）。
- 注意：浮点累积影响**第几回合**跨格，但同一机子上逐回合确定。

#### 2c. 死亡之后：当回合即结束，无重置

- `PacmanGame.cs:193-209` `IsGameOver()`：`Turn >= MaxTurns`（场景 10000，`Pacman.unity:959`）|| `Pacman.IsEaten() && !Invincible`（场景 `Invincible: 0`，`Pacman.unity:970`）|| `Level.IsCleared()`。
- 死亡回合结束后 `Game.cs:63-67` 检查到 `IsGameOver()` → `StopGame()` → `yield break`：**死亡回合的"状态更新"仍会执行完，然后立即结束，不会进入下一回合**。
- 无死亡动画/续命/状态重置；Automation 下一条目测试由 `InitialiseGame()` 全新建档（`PacmanAutomation.cs:99-101`）。

---

### 3. 鬼的行为

#### 3a. 状态机（`GhostAI.cs:53-59, 119-248`）

- 状态：`Scatter / Chase / Frightened / Eaten`；初始 `Scatter`（`GhostAI.cs:110`）。
- 每回合 `UpdateStateAndTarget()`（`Move()` 第一步，`GhostAI.cs:253`）按序执行转换：
  - `[Scatter|Chase|Frightened → Eaten]`：`Ghost.IsEaten()`（`GhostAI.cs:136-141`）；
  - `[Scatter|Chase → Frightened]`：`Ghost.IsFrightened()` → **强制掉头** `CurrentDirection.Reverse()`（`GhostAI.cs:155-159`）；
  - `[Scatter|Chase → Scatter/Chase]`：`Ghost.Timer[Game.Turn] != State` → 掉头 + 切状态（`GhostAI.cs:163-167`）——由 GhostTimer 时间表驱动；
  - `[Frightened → Scatter/Chase]`：`!IsFrightened()` → 掉头 + 按时间表（`GhostAI.cs:174-178`）；
  - `[Eaten → Chase]`：回到 `GhostHousePosition` → `Resurrect()`，状态按时间表（`GhostAI.cs:186-193`）。
- 目标（`GhostAI.cs:201-230`）：Scatter→`ScatterTarget`；Chase→`FindTarget()`（子类实现）；Frightened→`AvailablePositions(Position).Random()`（**非确定**）；Eaten→`GhostHousePosition`。
- 速度（`GhostAI.cs:236-244`）：Scatter/Chase 0.75、Frightened 0.5、Eaten 1.0。
- `Move()`（`GhostAI.cs:250-281`）：过滤"禁止掉头"（`Action.cs:70-86`）与"回到上一不同格"（`LastDifferentPosition`，`GhostAI.cs:264-265,274`），`MinBy(Vector2Int.Distance(..., Target))`（277 行）选最近动作；**平局由 `AvailableActions` 的枚举顺序 Up→Left→Down→Right 打破**（`PacmanLevel.cs:455-476`），完全确定。

#### 3b. `GhostTimer.cs`——Scatter/Chase 时间表

- `GhostTimer.cs:23` `Intervals`（场景值见第 4 点）；`GhostTimer.cs:37-49` `Start()` 将"秒 × FPS=10"转换为帧数构建 `IntervalList<GhostState>`；`LoopTime = true` 硬编码（45 行）。
- `IntervalList.cs:16-56`：`Schedule` 有序表 + `GetAt(time)` 二分归属；`GhostAI` 以 `Game.Turn`（帧号）索引：`Ghost.Timer[Game.Turn]`（`GhostAI.cs:163,166,177,191`）。
- 场景时间表（`Pacman.unity:863-880`）：`7S, 20C, 7S, 20C, 5S, 20C, 5S, ∞C`（10 FPS → 切换帧 70/270/340/540/590/790/840，之后永久 Chase）——经典 Pac-Man 时间表，**纯确定性**。

#### 3c. Blinky/Pinky/Inky/Clyde 决策差异（`AIs/Ghosts/`）

| 鬼 | `FindTarget()` | 随机源 |
|---|---|---|
| Blinky | `Game.Pacman.Position`（`GhostAI_Blinky.cs:15`）——直追 | 无 |
| Pinky | `Pacman.Position + Pacman.Action.ToV2I() * LookAhead(4)`（`GhostAI_Pinky.cs:18-20`）——堵截 | 无 |
| Inky | `Pacman.Position + (Pacman.Position - Blinky.Position)`；Blinky 缺失时退化为追 Pacman（`GhostAI_Inky.cs:17-28`） | 无 |
| Clyde | 距离 > 8 → `ScatterTarget`，否则追 Pacman（`GhostAI_Clyde.cs:19-30`，`Distance=8`） | 无 |

- **Ghost 的 Move 不使用 `DeterministicRandom`**：字段被注释（`GhostAI.cs:62`），唯一随机是 Frightened 目标选择里的 `UnityEngine.Random`（`GhostAI.cs:223`）。
- `Ghost.cs`：`IsFrightened() = FrightenedTime>0 && !IsEaten()`（35-37 行）；`OnPacmanPoweredUp` 令 `FrightenedTime = pacman.PowerPelletTimer`（40-43 行）；`UpdateState` 每回合 `FrightenedTime = max(FrightenedTime-1, 0)`（53-60 行）。

---

### 4. 场景配置（`Pacman.unity`）

#### 4a. 组件挂载

| GameObject | fileID | 组件（脚本 guid） | 关键字段 |
|---|---|---|---|
| PacmanGame | 1398103652 | PacmanGame（`46c462b0` = PacmanGame.cs，953 行）+ PacmanLevel（`ba36ed71` = PacmanLevel.cs，999 行） | `Delay: 0.1`、`MaxTurns: 10000`、`Rendering: 1`、`MakeClonesOfAgentAIs: 1`、`Invincible: 0`、`DrawAI: 0`（956-971 行） |
| Ghosts | 948761380 | **GhostTimer**（`e0e90e52` = GhostTimer.cs，860 行） | `Intervals: 7S,20C,7S,20C,5S,20C,5S,∞C`、`LoopTime: 0`（863-880 行） |
| Automation | 220792551 | **PacmanAutomation**（`57c7aa51` = PacmanAutomation.cs，381 行） | `Game→1398103653`、`Delay: 0`、`TestsPerAI: 1000`、`Rendering: 0`、`ClearData: 1`、`AIs: []`（384-396 行） |
| APSP（Prefab 实例） | 570696022 | **PacmanAPSP**（prefab guid `6b0b0206` = `APSP.prefab`，内含脚本 `70321b33` = PacmanAPSP.cs） | 场景根（1215-1221 行）；`PacmanAPSP.cs:19-22` 仅注册单例，数据构建 `PacmanAPSPData.Build` 无随机 |
| 场景根 | — | 另含 Main Camera/Light（79971073） | — |

#### 4b. AI 挂载与初始位置（PacmanGame 组件 956-971 行）

- **PacmanAI** = `PacmanAI_Keyboard.asset`（guid `308db7c35fc9c8c4aaa2deaa68562ac7`，969 行）——方向键输入 AI（`PacmanAI_Keyboard.cs:17-36`），**不按方向键则 Pacman 永远不动**。
- **GhostAI**（各鬼组件 `Timer` 均指向 GhostTimer 948761382）：
  - Blinky（514277151）：`AI = GhostAI_Blinky.asset`（`402628d7`）、`InitialPosition (26,29)`、`Speed 0.75`（726-746 行）
  - Pinky（1752200041）：`AI = GhostAI_Pinky.asset`（`b7ae5c71`）、`InitialPosition (1,29)`（1125-1145 行）
  - Inky（1461518958）：`AI = GhostAI_Inky.asset`（`2d308b78`）、`InitialPosition (26,1)`（1060-1080 行）
  - Clyde（163432251）：`AI = GhostAI_Clyde.asset`（`302e67c2`）、`InitialPosition (1,1)`（319-339 行）
- **Pacman**（2034548730）：`AI = PacmanAI_Keyboard`、`InitialPosition (14,7)`、`Speed 0.8`、`PowerPelletTime 100`（1190-1213 行）。
- **种子字段：不存在。** 场景/资产中无 `Salt`/`Sequence`/`DeterministicRandom` 序列化数据。
- Ghost asset 内部参数（`AIs/Ghosts/*.asset`）：Blinky `GhostHouse(12,19) Scatter(27,30)`；Pinky `GhostHouse(13,19) Scatter(0,30) LookAhead 4`；Inky `GhostHouse(14,19) Scatter(27,0)`；Clyde `GhostHouse(15,19) Scatter(0,0) Distance 8`——**鬼的出生点即各自角落，与 Scatter 角一致**。
- 迷宫：28×31，`Data[c, (lines.Length-1)-r]` 翻转（`PacmanLevel.cs:309-321`）；`w`墙/`.`豆/`o`能量豆/`H`鬼屋/空格地面/`-`虚空（`PacmanLevel.cs:371-382`）。Pacman (14,7) 在中间走廊地面；四鬼均在角落豆格。

---

### 5. Pacman 状态（`Pacman.cs`）

- 能量豆：`PowerPelletTime = 100` 帧（17 行）、`PowerPelletTimer`（21 行）、`DefaultSpeed 0.8 / PoweredSpeed 0.9`（26-28 行）。
- `UpdateState`（79-120 行）：站在 `Pellet/PowerPellet` 上即吃（88-106 行）；`PowerPelletTimer = max(timer-1, 0)`（110 行），归零调 `PowerDown()`（111-112 行）；速度按 `IsPoweredUp()` 取 0.9/0.8（116-119 行）。
- `PowerUp()`（123-136 行）：`PowerPelletTimer = PowerPelletTime`；清零 `GhostsEatenCounter`；**通知所有鬼** `OnPacmanPoweredUp`（134-135 行）→ 所有鬼进入 Frightened（`Ghost.cs:40-43`）→ 触发全局随机（见第 1 点）。
- `IsPoweredUp() = PowerPelletTimer > 0 && !IsEaten()`（149-151 行）。
- 吃鬼：`CanEat(ghost)`（166-170 行）；`OnEating` 得分 `200 × 2^GhostsEatenCounter`（187-188 行）。
- **被吃后**：无自定义逻辑，走 `Agent.OnEatenBy` → `Eaten = true`（`Agent.cs:123-130`）；本回合结束即 `IsGameOver()`（`PacmanGame.cs:200-201`），运行终止。

---

## artifacts（产物）

- 本报告：`Pacman_DeathDeterminism_Report.md`
- 关键代码文件（行号见上）：
  - `Assets/Games/Core/DeterministicRandom.cs`（61 行）
  - `Assets/Games/Core/Game.cs`（105 行）
  - `Assets/Games/Pacman/Scripts/PacmanGame.cs`（493 行）
  - `Assets/Games/Pacman/Scripts/Agent.cs`（280 行）、`Ghost.cs`（162 行）、`Pacman.cs`（197 行）、`GhostAI.cs`（442 行）、`GhostTimer.cs`（54 行）
  - `Assets/Games/Pacman/AIs/Ghosts/GhostAI_{Blinky,Pinky,Inky,Clyde}.cs` + 4 个 `.asset`
  - `Assets/Games/Pacman/Pacman.unity`（1221 行，文本 YAML）
  - `Assets/AlanZucconi/Scripts/Linq/LinqExtension.cs:409-425`（`.Random()` 扩展，`UnityEngine.Random.Range`）
  - `Assets/AlanZucconi/Scripts/Collections/IntervalList.cs`
  - `Assets/Games/Pacman/AIs/Goldsmiths/2025-26/ywang146/PacmanAI_ywang146.cs`（无随机源）

## open_questions（待确认）

1. 用户测试的是**哪个 AI**、通过**哪种方式**运行（手动 Play + StartGame 按钮，还是 Automation Run）？若为默认场景（Keyboard AI 不动），死亡回合数应为某个固定值（鬼从四角按 0.75 速收敛到 (14,7)），可验证是否与观测一致。
2. 该 AI 死亡前是否确实**从未吃过 power pellet**？（可通过 `Pacman.Score_ThingsEaten` 或是否出现过 Frightened 状态判断；若吃过却仍每次相同，需进一步排查编辑器 RNG 状态是否被外部固定。）
3. 若未来需要"确定性复现惊吓态"，应恢复 `GhostAI.cs:62` 的 `DeterministicRandom RandomSource` 并在 asset 中配置固定 `Salt`（`DeterministicRandom.cs:22`），取消 `GhostAI.cs:222` 的注释。
4. `PacmanAutomation` 的 `AIs` 列表为空（`Pacman.unity:389-390`）——用户跑批量评分时需自行填入目标 AI asset。

## risks（风险）

1. **"每次相同死亡"可能给用户错觉"AI 卡死/有 bug"**——实际是确定性系统的正常表现；惊吓态才是唯一随机源，若希望 AI 表现多样化，需吃能量豆或给惊吓态加种子。
2. **`UnityEngine.Random` 在编辑器下跨 Play 会话不重置**：若死亡路径确实经过 Frightened，不同会话结果必然不同；不能把"本次相同"推广为"任何条件下都相同"。
3. `GhostTimer.Start()` 依赖 Unity `Start()` 先于游戏循环执行（`GhostTimer.cs:37-49`）；若未来改为无场景的纯批处理调用顺序不当，`Timer` 可能为 null 而抛 NRE（`GhostAI.cs:163`）。
4. 浮点移动（`Agent.cs:162-179`）同机确定但**跨机器/跨编译配置可能不完全一致**（IEEE-754 基本一致，但 `FloorToInt` 对精确边界值的处理依赖编译器），批量评分应在同一环境跑。
5. 修改本框架行为（如启用 DeterministicRandom）会改变所有学生 AI 的评测基准，需谨慎。
