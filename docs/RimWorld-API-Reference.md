# RimWorld API 参考

基于 ILSpy 反编译 Assembly-CSharp.dll 的整理。

## 目录

- [地图相关](#地图相关)
- [MapPawns - 角色管理](#mappawns---角色管理)
- [Pawn - 角色属性](#pawn---角色属性)
- [移动与任务系统](#移动与任务系统)
- [Job 系统](#job-系统)
- [健康系统](#健康系统)

---

## 地图相关

### 获取当前地图

```csharp
Map map = Find.CurrentMap;           // 当前激活的地图
List<Map> maps = Find.Maps;          // 所有地图
```

### 地图属性

```csharp
map.Size           // IntVec3: x=250, y=1, z=250 (地图大小)
map.Tile           // int: 世界地图上的瓦片ID
map.mapTemperature // MapTemperature: 温度相关
map.mapPawns       // MapPawns: 角色管理器
```

---

## MapPawns - 角色管理

**位置**: `Verse.MapPawns`

### 获取角色列表

```csharp
// 所有角色
IReadOnlyList<Pawn> AllPawnsSpawned     // 已生成的所有角色
List<Pawn> AllPawns                      // 所有角色（包括未生成）

// 殖民者
List<Pawn> FreeColonists                 // 自由殖民者
List<Pawn> FreeColonistsSpawned          // 已生成的自由殖民者
List<Pawn> FreeAdultColonistsSpawned     // 已生成的成年殖民者

// 囚犯和奴隶
List<Pawn> PrisonersOfColony             // 殖民地的囚犯
List<Pawn> PrisonersOfColonySpawned      // 已生成的囚犯
List<Pawn> SlavesOfColonySpawned         // 已生成的奴隶

// 组合
List<Pawn> FreeColonistsAndPrisoners          // 殖民者和囚犯
List<Pawn> FreeColonistsAndPrisonersSpawned   // 已生成的殖民者和囚犯

// 分类
List<Pawn> AllHumanlike              // 所有人形生物
List<Pawn> AllHumanlikeSpawned       // 已生成的人形生物
List<Pawn> ColonyAnimals             // 殖民地动物
List<Pawn> SpawnedColonyAnimals      // 已生成的殖民地动物
List<Pawn> SpawnedColonyMechs        // 已生成的殖民地机甲

// 特殊状态
List<Pawn> SpawnedDownedPawns        // 已生成的倒地角色
List<Pawn> SpawnedHungryPawns        // 已生成的饥饿角色

// 按阵营
List<Pawn> PawnsInFaction(Faction faction)           // 某阵营的所有角色
List<Pawn> SpawnedPawnsInFaction(Faction faction)    // 某阵营已生成的角色
```

### 计数属性

```csharp
int ColonistCount           // 殖民者数量
int AllPawnsSpawnedCount    // 已生成角色总数
int FreeColonistsCount      // 自由殖民者数量
bool AnyColonistSpawned     // 是否有殖民者已生成
```

### 调试方法

```csharp
void LogListedPawns()       // 打印所有角色列表（调试用）
```

---

## Pawn - 角色属性

### 基本属性

```csharp
int thingIDNumber                    // 唯一ID
string LabelShort                    // 短名称
IntVec3 Position                     // 位置 (x, y, z)
Faction Faction                      // 所属阵营
    string Name                      // 阵营名称

// 布尔属性
bool IsColonist                      // 是否是殖民者
bool IsPrisoner                      // 是否是囚犯
bool IsSlave                         // 是否是奴隶
bool Downed                          // 是否倒地
bool Dead                            // 是否死亡
```

### 种族属性

```csharp
RaceProperties RaceProps
    bool Animal                      // 是否是动物
    bool Humanlike                   // 是否是人形生物

ThingDef def                         // 角色定义
    string label                     // 种族名称
    string defName                   // 定义名称
```

### 当前任务

```csharp
Job CurJob                           // 当前任务
    JobDef def                       // 任务定义
        string defName               // 任务名称 (如 "GotoWander", "Wait_Wander")
```

### 子系统

```csharp
Pawn_HealthTracker health            // 健康系统
Pawn_NeedsTracker needs              // 需求系统
Pawn_SkillTracker skills             // 技能系统
Pawn_EquipmentTracker equipment      // 装备系统
    Thing Primary                    // 主武器
Pawn_MindState mindState             // 心理状态
    bool Active                      // 是否活跃
Pawn_JobTracker jobs                 // 任务系统
PawnPather pather                    // 寻路系统
```

---

## 移动与任务系统

### 底层寻路 (Pather)

```csharp
// 直接移动到目标位置
pawn.pather.StartPath(LocalTargetInfo target, PathEndMode peMode);

// PathEndMode 选项:
// - ClosestTouch    // 最近可接触点
// - OnCell          // 目标格子
// - TouchPathEdge   // 触碰边缘
// - InteractionCell // 交互点

// 检查可达性
bool canReach = pawn.CanReach(LocalTargetInfo target, PathEndMode peMode, Danger danger);
```

### Job 系统移动

```csharp
// 创建移动任务
using RimWorld;

Job job = JobMaker.MakeJob(JobDefOf.Goto, targetCell);

// 启动任务
pawn.jobs.StartJob(job, JobCondition.InterruptOptional);

// JobCondition 选项:
// - InterruptOptional      // 可选中断
// - InterruptForced        // 强制中断
// - Suspend                // 暂停当前任务
```

### 目标信息

```csharp
// 创建位置目标
LocalTargetInfo target = new LocalTargetInfo(IntVec3 position);
LocalTargetInfo target = new LocalTargetInfo(Thing thing);

// IntVec3 位置
IntVec3 pos = new IntVec3(x, y, z);  // 通常 y=0
int x = pos.x;
int z = pos.z;
```

---

## Job 系统

### JobMaker - 任务工厂

**位置**: `Verse.JobMaker` (需要 `using Verse;`)

```csharp
// 创建简单任务（只有目标A）
Job job = JobMaker.MakeJob(JobDef def, LocalTargetInfo targetA);

// 创建双目标任务
Job job = JobMaker.MakeJob(JobDef def, LocalTargetInfo targetA, LocalTargetInfo targetB);

// 创建三目标任务
Job job = JobMaker.MakeJob(JobDef def, LocalTargetInfo targetA, LocalTargetInfo targetB, LocalTargetInfo targetC);

// 创建带数量的任务（如搬运数量）
Job job = JobMaker.MakeJob(JobDef def, LocalTargetInfo targetA, int count);

// 完整参数
Job job = JobMaker.MakeJob(JobDef def, LocalTargetInfo targetA, LocalTargetInfo targetB,
                           LocalTargetInfo targetC, int count, bool suspended = false,
                           LocalTargetInfo? overrideBodyTarget = null);
```

**示例**:
```csharp
// 移动到指定位置
Job gotoJob = JobMaker.MakeJob(JobDefOf.Goto, new LocalTargetInfo(targetCell));

// 搬运物品到存放区
Job haulJob = JobMaker.MakeJob(JobDefOf.Haul, new LocalTargetInfo(item), new LocalTargetInfo(destination));

// 近战攻击敌人
Job attackJob = JobMaker.MakeJob(JobDefOf.AttackMelee, new LocalTargetInfo(enemy));
```

### Job - 任务实例

**位置**: `Verse.AI.Job` (需要 `using Verse.AI;`)

```csharp
class Job : IExposable, ILoadReferenceable
{
    // 核心属性
    public JobDef def;
    public LocalTargetInfo targetA = LocalTargetInfo.Invalid;
    public LocalTargetInfo targetB = LocalTargetInfo.Invalid;
    public LocalTargetInfo targetC = LocalTargetInfo.Invalid;
    public int count = -1;

    // 移动相关
    public LocomotionUrgency locomotionUrgency = LocomotionUrgency.Jog;
    public bool playerForced;
    public HaulMode haulMode;

    // 其他常用属性
    public int expiryInterval = -1;
    public bool ignoreForbidden;
    public bool canBashDoors;
    public bool canBashFences;

    // 方法
    public JobDriver MakeDriver(Pawn driverPawn);
    public bool CanBeginNow(Pawn pawn, bool whileLyingDown = false);
    public Job Clone();
}
```

### LocomotionUrgency - 移动紧迫性

**位置**: `Verse.AI.LocomotionUrgency`

```csharp
enum LocomotionUrgency
{
    None,           // 无
    Amble,          // 漫步
    Walk,           // 行走
    Jog,            // 慢跑 (默认)
    Sprint          // 冲刺
}
```

### Pawn_JobTracker - 角色任务管理器

**位置**: `Verse.Pawn_JobTracker`

```csharp
// 访问
Pawn_JobTracker tracker = pawn.jobs;

// 属性
Job curJob                              // 当前任务
JobDef curJobDef                        // 当前任务定义
PawnPosture curPosture                  // 当前姿势
bool curDriver != null                  // 是否有任务驱动器
bool startingNewJob                     // 是否正在开始新任务

// 任务队列
JobQueue jobQueue                       // 待执行的任务队列
```

#### StartJob - 启动任务

```csharp
void StartJob(Job newJob,
              JobCondition lastJobEndCondition = JobCondition.None,
              ThinkNode jobGiver = null,
              bool resumeCurJobAfterwards = false,
              bool cancelBusyStances = true,
              ThinkTreeDef thinkTreeDef = null,
              JobTag? tag = null,
              bool fromQueue = false);
```

**参数说明**:
- `newJob`: 要启动的新任务
- `lastJobEndCondition`: 结束当前任务的条件
- `resumeCurJobAfterwards`: 是否在完成后恢复当前任务
- `cancelBusyStances`: 是否取消繁忙姿态

**示例**:
```csharp
// 简单启动（中断当前任务）
pawn.jobs.StartJob(newJob, JobCondition.InterruptOptional);

// 强制中断
pawn.jobs.StartJob(newJob, JobCondition.InterruptForced);

// 暂停当前任务，完成后恢复
pawn.jobs.StartJob(newJob, JobCondition.Suspend, resumeCurJobAfterwards: true);
```

#### EndCurrentJob - 结束当前任务

```csharp
void EndCurrentJob(JobCondition condition, bool startNewJob = true, bool canReturnToPool = true);
```

**示例**:
```csharp
// 正常完成
pawn.jobs.EndCurrentJob(JobCondition.Succeeded);

// 中断
pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
```

#### 其他方法

```csharp
// 检查是否可以执行某任务
bool CanTakeJob(Job job);

// 清空任务队列
void ClearQueuedJobs();

// 尝试获取下一个队列任务
bool TryTakeOrderedJob(Job job, JobTag tag = JobTag.Misc);

// 通知任务完成
void Notify_JobCompleted(Job job, JobCondition condition);
```

### JobCondition - 任务结束条件

**位置**: `Verse.AI.JobCondition` (需要 `using Verse.AI;`)

```csharp
[Flags]
enum JobCondition : byte
{
    None = 0,
    Ongoing = 1,
    Succeeded = 2,
    Incompletable = 4,
    InterruptOptional = 8,
    InterruptForced = 0x10,
    QueuedNoLongerValid = 0x20,
    Errored = 0x40,
    ErroredPather = 0x80
}
```

### JobDef - 任务定义属性

```csharp
class JobDef : Def
{
    bool playerInterruptible;     // 是否可被玩家中断
    bool suspendable;             // 是否可暂停
    bool alwaysShowWeapon;        // 是否总是显示武器
    bool canDeadline;             // 是否有截止时间
    JoyDuration joyDuration;      // 娱乐持续时间类型
    int tendDuration;             // 照料持续时间
    bool makeOnlyPrisoners;       // 是否仅限囚犯
    bool countingAsWork;          // 是否算作工作
    bool neverShowWeapon;         // 是否从不显示武器
    bool allowOpportunisticPrefix;// 允许机会主义前缀
    bool casual;                  // 是否随意
    bool sendStandardAnalytics;   // 发送标准分析
}
```

---

## 健康系统

### Pawn_HealthTracker

```csharp
// 位置: Verse.Pawn_HealthTracker
Pawn_HealthTracker health = pawn.health;

// 属性
bool Downed                           // 是否倒地
bool Dead                             // 是否死亡
SummaryHealthHandler summaryHealth    // 健康摘要
```

### SummaryHealthHandler

```csharp
// 位置: Verse.SummaryHealthHandler
SummaryHealthHandler summary = pawn.health.summaryHealth;

// 属性
float SummaryHealthPercent            // 健康百分比 (0.0 - 1.0)
```

---

## 阵营相关

### 获取玩家阵营

```csharp
Faction playerFaction = Faction.OfPlayer;
```

### 敌对判断

```csharp
bool isHostile = pawn.HostileTo(Faction.OfPlayer);
bool isHostile = pawn.HostileTo(otherPawn);
```

---

## 常用 JobDef

| defName | 用途 |
|---------|------|
| `Goto` | 移动到位置 |
| `GotoWander` | 漫游移动 |
| `Wait` | 等待 |
| `Wait_Wander` | 等待漫游 |
| `Wait_MaintainPosture` | 保持姿势 |
| `AttackMelee` | 近战攻击 |
| `AttackStatic` | 静态攻击 |
| `Haul` | 搬运 |
| `Mine` | 挖掘 |
| `ConstructFinishFrame` | 建造 |
| `PlantCut` | 收割 |
| `Sow` | 播种 |
| `Repair` | 修理 |

---

## 注意事项

1. **线程安全**: 所有游戏数据访问必须在主线程进行
2. **空检查**: 许多属性可能为 null，使用 `?.` 和 `??` 进行安全访问
3. **API 版本**: 基于 RimWorld 1.6.9438

## 命名空间参考

| 类型 | 命名空间 | 说明 |
|------|----------|------|
| `Job` | `Verse.AI` | 任务实例 |
| `JobMaker` | `Verse` | 任务工厂 |
| `JobCondition` | `Verse.AI` | 任务结束条件枚举 |
| `LocomotionUrgency` | `Verse.AI` | 移动紧迫性枚举 |

```csharp
// 使用示例
using Verse;
using Verse.AI;

Job job = JobMaker.MakeJob(JobDefOf.Goto, target);
job.locomotionUrgency = LocomotionUrgency.Jog;
pawn.jobs.StartJob(job, JobCondition.InterruptOptional);
```

---

## 控制命令示例

### 移动角色到指定位置

```csharp
using Verse;
using Verse.AI;

public static bool MovePawnTo(Pawn pawn, IntVec3 targetPos)
{
    // 创建移动任务
    Job gotoJob = JobMaker.MakeJob(JobDefOf.Goto, new LocalTargetInfo(targetPos));
    gotoJob.locomotionUrgency = LocomotionUrgency.Jog;

    // 启动任务
    pawn.jobs.StartJob(gotoJob, JobCondition.InterruptOptional);
    return true;
}
```

### 搬运物品

```csharp
public static bool HaulItem(Pawn pawn, Thing item, IntVec3 destination)
{
    if (item == null || pawn == null) return false;

    // 创建搬运任务
    Job haulJob = JobMaker.MakeJob(
        JobDefOf.Haul,
        new LocalTargetInfo(item),
        new LocalTargetInfo(destination)
    );
    haulJob.count = item.stackCount;

    pawn.jobs.StartJob(haulJob, JobCondition.InterruptOptional);
    return true;
}
```

### 攻击目标

```csharp
public static bool AttackTarget(Pawn pawn, Thing target)
{
    if (pawn == null || target == null) return false;

    // 根据武器选择攻击方式
    JobDef attackDef = pawn.equipment.Primary?.def.IsRangedWeapon == true
        ? JobDefOf.AttackStatic
        : JobDefOf.AttackMelee;

    Job attackJob = JobMaker.MakeJob(attackDef, new LocalTargetInfo(target));
    pawn.jobs.StartJob(attackJob, JobCondition.InterruptOptional);
    return true;
}
```

### 暂停当前任务

```csharp
public static void PauseCurrentJob(Pawn pawn)
{
    if (pawn.jobs.curJob != null)
    {
        pawn.jobs.curJob.suspended = true;
    }
}
```

---

## 更新日志

- **2026-02-21**: 验证并确认 Job 系统 API 命名空间 (Verse.AI, Verse)，实现控制命令
- **2026-02-21**: 添加详细 Job 系统 API（JobMaker, Job, Pawn_JobTracker, JobDef），添加控制命令示例
- **2026-02-21**: 初始版本，整理 MapPawns、Pawn、健康系统 API
