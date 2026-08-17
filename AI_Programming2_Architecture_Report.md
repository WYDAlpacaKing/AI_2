# Pacman 项目架构梳理报告（E:\AI_Programming2）

> 生成时间：2026-08-18 | 方式：全量代码勘察（very thorough）

## 1. 整体项目定位

- **类型**：用于游戏 AI 教学的 **Pacman AI 框架**（Goldsmiths 大学课程项目，命名空间 `AlanZucconi.*`）。
- **Unity 版本**：`ProjectSettings/ProjectVersion.txt` → `m_EditorVersion: 6000.5.7f1`（Unity 6）。
- **三个场景**：
  - `Assets/Scenes/SampleScene.unity`：仅含 Main Camera + Directional Light 的默认空模板场景，未挂载任何游戏组件。
  - `Assets/Games/Pacman/Pacman.unity`：**主游戏场景**。根节点 `PacmanGame`（挂 `PacmanGame` + `PacmanLevel` 组件），子节点 Pacman、Ghosts（Blinky/Pinky/Inky/Clyde 四鬼 + 共享 `GhostTimer`）、Grid/Tilemap；另有 `Automation` 节点（`PacmanAutomation` 批量测试，TestsPerAI=1000）。场景默认 PacmanAI = `PacmanAI_Keyboard.asset`。
  - `Assets/Games/Pacman/Scripts/Evolution/PacmanEvolution.unity`：**演化场景**。`Evolution System`（EvolutionSystem 组件：5000 代、每基因组 25 次测试、Tournament 选择）+ `PacmanWorldBatch`（10×10=100 个世界，WorldPrefab=`PacmanGame.prefab`）+ APSP 预制体实例。

## 2. Pacman 游戏核心（Assets/Games/Pacman/Scripts/）

### 类层次
```
Game (abstract, AlanZucconi.Core)  ←  PacmanGame (MonoBehaviour, 游戏主控)
Agent (MonoBehaviour)              ←  Pacman（玩家）、Ghost（鬼）
AgentAI (abstract ScriptableObject) ←  PacmanAI ← PacmanAIEvo / 各示例 AI
                                   ←  GhostAI ← GhostAI_Blinky/Pinky/Inky/Clyde
PacmanLevel (MonoBehaviour)        关卡网格
PacmanPathfinding (struct, IPathfindingCost) 实时 Dijkstra
PacmanAPSP / PacmanAPSPData        预计算全对最短路径
GhostTimer / PacmanAutomation      时间表 / 批量测试
```

### 主循环与状态管理
- **Game.cs**（`Games/Core/Game.cs`）：抽象基类，协程 `GameLoop_Coroutine()` 驱动：`Turn++ → UpdateGame() → IsGameOver()? → 按 Delay 等待`。字段 `Running/Turn/MaxTurns/Delay`。抽象方法 `InitialiseGame/UpdateGame/IsGameOver/Score`。
- **PacmanGame.cs**：实现四个抽象方法。
  - `InitialiseGame()`：重建关卡（`Level.Build()`+`Draw()`）→ `InitialiseAgents()`（把每个 AI ScriptableObject **浅拷贝克隆**一份，保证多实例并行不共享状态；首次运行时缓存 `GhostAIs` 原始引用，之后每次从原始克隆，避免"副本的副本"累积）→ `DrawAgents()`。
  - `UpdateGame()`：先 `UpdateAgentPositions()`（所有 Agent 先移动，再检测 Pacman 与每个 Ghost 的碰撞：同格 or 交换位置，`ResolveCollision()` 依 `CanEat` 双向判定），再对每个 Agent 调 `UpdateState()`（吃豆、计时器），最后按需重绘。
  - `IsGameOver()`：超时（Turn≥MaxTurns）/ Pacman 被吃（除非 Invincible）/ 关卡清空。
  - `Score()` = `Pacman.Score`。
- **ValidateAIPath()**：用正则校验学生 AI 是否放在 `Assets/Games/Pacman/AIs/Goldsmiths/20xx-xx/学号/PacmanAI_学号.asset`（或 Game AI 目录），防止交到错误位置。

### 实体
- **Agent.cs**：所有可移动体基类。`Position/OldPosition`（整格）、`FloatPosition`（平滑插值）、`Action/OldAction`、`Speed`、`Eaten`、`Game/AI` 引用、Tile/颜色（按方向选朝向 Tile）。`UpdatePosition()`：取 `AI.Move()` → 校验 `Level.IsFree` → 移动（支持**世界循环** `Level.Loop`）→ `SnapFloatPosition()`（转向时对齐到格心，避免穿墙视觉）。吃与被吃：`CanEat/CanBeEaten/OnEatenBy/OnEating/Resurrect` 虚方法。
- **Pacman.cs**：吃豆人。`UpdateState()` 吃 Pellet(+10)/PowerPellet(+50, `PowerUp()` 通知所有鬼)，递减 `PowerPelletTimer`；`CanEat(ghost)` = 鬼 frightened 且自己 powered；吃到鬼得分 200×2^n（`GhostsEatenCounter`）。
- **Ghost.cs**：鬼。`FrightenedTime`（吃能量豆时同步 Pacman 计时器）；`CanEat(pacman)` = 自己不 frightened、双方没被吃、且非 Invincible；被吃后回巢 `Resurrect`；颜色状态（frightened 蓝 / eaten 白）。

### AI 基类
- **AgentAI.cs**（abstract ScriptableObject）：`Move()` 抽象、`Initialise()`/`Draw()` 虚方法、持有 `Game/Agent` 引用与 `Position` 便捷属性；含学生信息字段。
- **PacmanAI.cs**：`Pacman` 便捷属性 + 测试数据收集（`ScoresData`、四个 PlotData 图表 + `UpdatePlots()`）。
- **GhostAI.cs**：四态状态机 `GhostState {Scatter, Chase, Frightened, Eaten}`。
  - `UpdateStateAndTarget()`：根据 Ghost 的 frightened/eaten/`Ghost.Timer[Turn]` 做状态迁移（scatter↔chase 与进入 frightened 时**强制反向**，与原始 Pacman 行为一致）；按状态定 Target（Scatter→ScatterTarget；Chase→`FindTarget()` 抽象；Frightened→随机邻居；Eaten→鬼屋）；按状态设速度（0.75/0.5/1.0）。
  - `Move()`：在 `AvailableActions` 中过滤掉反向与 `LastDifferentPosition`，选离 Target 欧氏距离最近的动作（贪心）。
  - `FindTarget()` 抽象 → 四个鬼各自实现。

### Action.cs
枚举 `None/Up/Down/Left/Right` + 扩展：`ToV2I()`（方向↔向量）、`Reverse()`、`IsReverseOf()`（鬼禁止掉头）。

### PacmanLevel.cs 关卡表示
- `Item {Void, Wall, Ground, Pellet, PowerPellet}`；`Data` 为 `Item[,]` 二维数组（宽=列、高=行，y 翻转）。
- 关卡是 ASCII 字符串：`w`墙、`.`豆、`o`能量豆、`H`鬼屋墙、空格地面、`-`虚空（不可达墙）。`Build()` 解析字符串填数组并统计 `ItemsLeft`；`IsCleared()` = ItemsLeft≤0。
- 支持**环世界**：`Loop()` 取模包裹；`EuclideanLoopDistance/ManhatanLoopDistance` 考虑环绕的启发式距离。
- 关键查询：`IsFree/IsObstacle`、`AvailableActions`（上下左右自由方向）、`AvailableNeighbours`（返回 `(位置, Edge<Action>)`）、`Edibles/Pellets/PowerPellets`、`FreePositions`。
- 索引器 `this[Vector2Int]` 在"可食物被替换"时自动递减 `ItemsLeft`。

### 寻路：PacmanPathfinding 与 APSP
- **PacmanPathfinding**（struct）：实现 `IPathfindingCost<Vector2Int, Edge<Action>>`，`Outgoing` 直接委托 `Game.Level.AvailableNeighbours`（**以 Action 为边**，因为环世界坐标不足以来判断方向）。扩展方法 `MoveTowards/DistanceFrom/IsReachable`（Agent 版与 Game 版），内部跑 **Dijkstra**，返回路径的第一条边作为动作。
- **PacmanAPSP/Data**：预计算**全对最短路径**。`Build()` 对 `FreePositions` 的所有有序对跑 Dijkstra，把 `(首动作, 距离)` 存入 `PFResult2DArray`（Flat2DArray 展平，避免字典 9 万条目开销），`Index`（SerializableDictionary<Vector2Int,int>）做坐标→下标映射。查询 O(1)：`MoveTowards_APSP/DistanceFrom_APSP/IsReachable_APSP`。`PacmanAPSP` 是单例 MonoBehaviour（`PacmanAPSP.S`），场景里以 prefab 形式放置；`Pacman Level.asset`（18 万行）是已构建好的数据资产。

### GhostTimer.cs / PacmanAutomation.cs
- **GhostTimer**：把 `Intervals`（7/20/7/20/5/20/5/∞ 秒 × FPS=10）转成 `IntervalList<GhostState>` 时间表，`this[Game.Turn]` 查询当前应处于 Scatter 还是 Chase。四个鬼共享一个实例（挂在 Ghosts 父节点）。
- **PacmanAutomation**：批量无头测试。协程对 `AIs` 列表逐个跑 `TestsPerAI` 局：设 `Game.PacmanAI=ai` → `StartGame()` → `WaitWhile(Running)` → `CollectStats`（把 `(Turn, Score, ThingsEaten)` 存入 `ai.ScoresData` 并 `UpdatePlots()`）。配套 ProgressBar 显示进度；`ClearData` 清空旧数据。

## 3. AI 接口与实例（Assets/Games/Pacman/AIs/）

### Examples（策略对比）
| AI | 策略 |
|---|---|
| PacmanAI_Random | 每帧从 `AvailableActions` 均匀随机选一个方向 |
| PacmanAI_Greedy | 每帧找**欧氏距离最近**的可食物，`MoveTowards` 寻路过去；忽略鬼（会因贪心在两个等距食物间振荡） |
| PacmanAI_GreedyFix | 同上但**锁定目标**直到吃到或到达，再重选（修复振荡） |
| PacmanAI_Flee | 找最近鬼，选使其**距离最大化**的动作（贪心逃跑） |
| PacmanAI_Idle | 恒返回 `Action.None`（不动） |
| PacmanAI_Keyboard | 读方向键输入（人工游玩） |
| PacmanAIEvo_Idle | 继承 `PacmanAIEvo`，打印 `Weights[]` 后不动——展示演化权重如何接入 AI 的最小示例 |

### Ghosts（四个鬼，共享 GhostAI 状态机，仅 `FindTarget` 不同）
- **Blinky**：目标 = Pacman 当前位置（直接追击）。
- **Pinky**：目标 = Pacman 位置 + 当前方向 × `LookAhead=4`（拦截前方）。
- **Inky**：目标 = Pacman 位置 + (Pacman位置 − Blinky位置)（以 Blinky 为参照的"镜像包围"；无 Blinky 时退化为追击）。
- **Clyde**：距 Pacman 超过 `Distance=8` 时转向 ScatterTarget，否则追击（近处胆怯）。

### Goldsmiths/2025-26/ywang146（学生提交，三版）
- **PacmanAI_ywang146.cs（v3/最终）**：**效用理论**（继承 PacmanAIEvo）。对每个可用邻居计算加权和 `Σ wi×heuristic`（w0 FoodProximity 靠近食物、w1 GhostSafety 远离危险鬼、w2 PowerProximity 靠近能量豆、w3 HuntProximity 追逐可吃鬼（仅 powered 时且按剩余时间缩放）、w4 DeadEndAvoidance 避开死胡同、w5 DirectionPersistence 保持方向/惩罚掉头），启发式归一化 `1/(1+d)`，距离优先用 APSP O(1)，缺失时回退环欧氏距离。权重可在 Inspector 调，也可交给演化系统优化（初始 `[1, 3, 0.8, 1.2, 0.5, 0.4]`）。
- **PacmanAI_ywang146_v1.cs**：**贪心吃豆基线**——每帧重选最近可食物（欧氏距离）+ 寻路，无视鬼（注释自述三个局限：振荡、无视墙、无鬼意识）。
- **PacmanAI_ywang146_v2.cs**：**三态有限状态机** `EatFood/Flee/Hunt`。EatFood = v1 贪心但**锁定目标**且用真实路径距离；Flee = 危险鬼在 `FleeDistance=12` 内时选最远离方向；Hunt = powered 且可吃鬼在 `HuntDistance=30` 内时追击赚分。每帧按 `dangerous/eatable` 判定状态迁移。
- 演变脉络：v1 基线 → v2 加鬼意识（FSM）→ v3 连续效用加权（可演化）。

## 4. 演化计算系统（AlanZucconi/AI/Evo + Pacman/Scripts/Evolution）

### 核心接口
- **IGenome**：`Copy()`、`Mutate()`、`Mutate(int)`（多次变异）；配套 `IGenomeFactory<T>.Instantiate()` 造随机基因组。
- **IWorld<T>**：`ResetSimulation/SetGenome/StartSimulation/IsDone/GetScore`——"世界"即一个可被指派基因组并评估的模拟环境。
- **ArrayGenome**（struct）：`float[] Params` + `MutationRate`。`Mutate()` 随机挑一参数：90% 概率 ±MutationRate 微调（clamp [-1,1]），10% 概率整体重随机。`RMSE` 用于比较两个基因组。
- **GenomeWorld**（abstract）：同时实现 `IWorld<ArrayGenome>` + `IGenomeFactory<ArrayGenome>`，子类给出 `GetGenomeSize/GetMutationRate`，`Instantiate()` 产出随机 `ArrayGenome`。
- **EvolutionSystem<T> / EvolutionSystem**：
  - 启动时通过 `FindObjectsOfType<MonoBehaviour>().OfType<IWorld<T>>()` **自动发现所有世界**（无需手动登记），用第一个 `IGenomeFactory` 生成初始种群。
  - 每代：给每个世界分配基因组 → 每世界用协程 `BatchScore(TestsPerGenome)` 并行跑多局（用 IQR 中位数作为鲁棒适应度）→ 按分数降序排序 → 精英保留 `TopK` 份拷贝 + `RandomGenomes` 个随机新个体 + 按选择策略（Truncation 截断 / FitnessProportionate 轮盘赌 / Tournament 锦标赛）补满种群 → `MutateGenomes`（变异数 = 基础 Mutations + 自适应：每 10 代无改进 +1，上限 20）。
  - 记录 `BestScoreSoFar/GenerationsWithoutImprovement`，每代画 LinePlot（带 IQR 误差棒）。

### Pacman 侧集成
- **PacmanAIEvo**（abstract : PacmanAI）：`float[] Weights` + `GetWeightsSize/SetWeights`——把 AI 参数暴露给演化。
- **PacmanWorld**（: GenomeWorld）：持 `Game`(PacmanGame) 与 `AI`(PacmanAIEvo)。`ResetSimulation` 克隆一份 AICopy（清空 PlotData 减负）；`SetGenome` 把 `genome.Params` 写入 `AICopy.SetWeights` 并赋给 `Game.PacmanAI`；`StartSimulation→Game.StartGame()`；`IsDone→!Game.Running`；`GetScore` 可选 `ScoreType {Time, Score, ThingsEaten, Mix(50/50), Mix30x70}`（Mix 用 260 豆 / 14600 分归一化）。
- **PacmanWorldBatch**：按 `Size×Size` 实例化 `WorldPrefab`（= `PacmanGame.prefab`，内部含 PacmanWorld+PacmanGame+PacmanLevel，Rendering=0 无头运行）批量并行评估；`SetWeights` 广播、`GetScore` 取各世界分数的中位数。被演化场景中的 `EvolutionSystem.StartEvent → InstantiateWorlds` 调用。

### 演化↔游戏数据流
```
EvolutionSystem (每代)
  → 为 N 个 PacmanWorld 各分配一个 ArrayGenome（weights）
  → 每个 World: SetWeights → Game.PacmanAI = AICopy
  → Game.StartGame() 跑完整局（协程；PacmanGame 内部再克隆一份 AI 副本，隔离状态）
  → IsDone → GetScore（按 ScoreType）
  → BatchScore 跑 TestsPerGenome 局取 IQR 中位 → 排序 → 选择+变异 → 下一代
```

## 5. 寻路库（Assets/AlanZucconi/AI/PF/）

- **IPathfinding<N>**：仅 `Outgoing(N)`——无权重图接口。
- **Pathfinding**（static partial）：`BreadthFirstSearch`（Queue 前沿 + `Null<N>` 包装解决 struct 不能为 null 的问题）、`ReachableNodes`。返回 null=不可达、`[start]=已在目标`、否则含起终点的路径。
- **IPathfindingCost<N,E> / IEdge**：带权重接口，`Outgoing` 返回 `(N,E)`。`Edge`（纯 cost）、`Edge<T>`（包装任意内容为边，默认 cost=1，如 `Edge<Action>`）、`UnitCostGraph`（把无权重图包成单位代价，扩展方法 `ToWeightedGraph`）。
- **Dijkstra**：`PriorityQueue<N,float>`（内部是 FibonacciHeap）开放前沿；`visitedFrom/fromEdge/costSoFar` 三个字典记录前驱/来边/代价；支持 `isGoal` 谓词版（GOAP 用）与具体 goal 版；返回 `List<(N,E)>` 路径。
- **AStar**：与 Dijkstra 相同骨架，仅 `frontier.Insert(next, newCost + heuristic(next))` 加入启发式；同样双版本。
- **Graph<N>** / **WeightedGraph<N,E>**：通用内存图（Dictionary<节点, HashSet<邻居>>），`Connect/Disconnect/Outgoing`。
- **Grid2D**：`bool[,] Wall` + 四方向 `Outgoing`，越界视为墙。
- **PriorityQueue/FibonacciHeap**：`PriorityQueue<TElement,TPriority>` 包装 `FibonacciHeap`（Insert O(1) 摊还、RemoveMin O(log n) 摊还、DecreaseKey/Union 等标准实现，`FibonacciHeapNode` 双向循环链表+child/parent/mark/degree）。

## 6. 工具库（Assets/AlanZucconi/ 其余部分）

- **PlotData**：可序列化数据容器（`List<Vector2> Data` + 可选 BarsData 误差棒），`CalculateStatistics` 算 Min/Max/IQR 四分位，Dirty 标记懒重算。配套 Inspector 属性：`[LinePlot]`（折线）、`[ScatterPlot]`（散点）、`[HistogramPlot]`（直方图）、`[GridPlot]`（网格热图），各自带 LabelX/Y、网格密度、颜色等参数，Editor 内 `PropertyDrawer`（折叠 + 自绘 GL 图）。
- **自定义 Attribute/编辑器扩展**：
  - `[ReadOnly]`：Inspector 只读显示（追踪字段如 Position/Score/Turn）。
  - `[ShowIf("EnumField", EnumValue)]`：按枚举位与显示/隐藏字段（如 Selection 策略参数）。
  - `[Button(Editor=bool)]`：在 Inspector 给方法渲染按钮（运行时可点/仅编辑器模式）。
  - `[ProgressBar]`：把 ProgressBar 字段渲染为进度条；`ProgressBar.Loop()` 迭代器顺带更新进度与剩余时间估算。
  - `[Monospaced]`：关卡 ASCII 文本等宽显示（`[Monospaced(31,31)]`）。
  - `[EditorOnly]`：仅编辑器序列化的字段（如 Tilemap 引用，运行时/演化场景不加载）。
- **Collections**：`Flat2DArray<T>`（可序列化二维数组，APSP 用它）、`SerializableDictionary<TK,TV>`（ISerializationCallbackReceiver 序列化字典，APSP Index 用它）、`IntervalList<T>`（SortedList 分段时间表，GhostTimer 用它）、`Cached<T>`（惰性缓存）、`Tuple/ArrayUtils`。
- **Linq 扩展**：`MinBy/MaxBy`（MoreLINQ 移植）、`Median/IQR/Percentile`、`Random/RandomProbability`（按概率抽样）、`DistinctPairs/AllDistinctPairs/AllPairs`、`ToArrayOrNull`、`Zip`（元组版）、`IsEmpty`、`StandardDeviation` 等。
- **Debug**：`DebugDraw`（Arrow/DashedLine/Circle/Rectangle 等 Gizmos 风格调试绘制，DrawAI 时可视化鬼的目标）、`SegmentFont`（七段数码字体）。
- **UnityExtensions**：Color/Vector3/VectorInt/Renderer 扩展、`GlobalMonoBehaviour`、UnityEvents 工具。
- **Utils**：通用数学/几何工具（椭圆、角度、射线投影、时间格式化 `FormatTime` 等）。

## 7. Games/Core

- **Game.cs**：抽象游戏基类（见第 2 节），注释 `// TODO: not used yet!`——实际已被 PacmanGame 使用。协程循环、Turn 计数、Start/Stop/Pause 控制。
- **DeterministicRandom**：可序列化结构体，`Salt` + `Get(x)` 以 `x ^ Salt` 为种子构造 `System.Random`（同 turn 同结果，可复现实验）；`RandomSourceExtension.DeterministicRandom<T>()` 从序列按该随机源取元素。注：当前 GhostAI 的 Frightened 用普通 `Random()`，DeterministicRandom 在代码中有被注释掉的引用。

## 关键数据流总结

```
[Pacman.unity 场景]                        [PacmanEvolution.unity 场景]
PacmanGame(Game 协程主循环)                EvolutionSystem
 ├─ Level.Build/Draw (ASCII→Item[,]→Tilemap)  └─ 发现全部 IWorld<PacmanWorld>
 ├─ InitialiseAgents (克隆 AI 资产)             └─ 每代: SetWeights(ArrayGenome)
 └─ UpdateGame:                                 └─ PacmanWorld.StartSimulation
     ├─ Agent.UpdatePosition                      └─ PacmanGame.StartGame()  (无头 Rendering=0)
     │   └─ AI.Move()                             └─ GetScore (中位数) → 选择/变异
     │       ├─ 学生 AI: 启发式/FSM/效用
     │       └─ GhostAI: 状态机+贪心最近目标
     ├─ 碰撞 ResolveCollision (吃/被吃, 计分)
     └─ Agent.UpdateState (吃豆/计时器)
路径查询: MoveTowards(APSP O(1)) ── 回退 ── Dijkstra(实时)  ←  PacmanLevel.AvailableNeighbours
```

## 主要风险 / 注意点（代码中发现）

1. `PacmanAI_Greedy.cs` 等示例在 `Edibles()` 为空时会抛异常（`.MinBy` 对空序列抛 InvalidOperationException）；GreedyFix 未处理无食物。
2. `PacmanWorldBatch` 场景中 `AI: {fileID: 0}` 为空、`EvolutionSystem.FirstGenome.Params` 长度 5 而 ywang146 权重为 6——演化运行前必须手动指派 `PacmanAIEvo` 资产。
3. GhostAI 注释承认 Frightened 时 `Target = 随机邻居` 在速度 0.5 下可能原地徘徊/效率问题（旧代码中曾有 `Game.Turn % 2` 停帧补偿，已删除）。
4. `Game.StopGame()` 后 `TogglePause()` 会误取消暂停（Game.cs 内 FIXME 注释）。
5. `PacmanPathfinding.Dijkstra` 每帧新建实例并全量寻路，实时 AI 大量调用时开销大——APSP 是官方推荐优化路径（学生代码已采用）。
6. 自动化测试 asset（如 PacmanAI_Greedy.asset）内含 1000+ 条序列化测试数据，体积大、易污染版本库。
