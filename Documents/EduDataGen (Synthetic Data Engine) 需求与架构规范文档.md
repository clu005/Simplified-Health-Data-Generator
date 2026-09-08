# **EduDataGen (Synthetic Data Engine) 需求与架构规范文档**

## **1\. 项目概述与设计愿景**

### **1.1 项目定位**

**EduDataGen** 是一个面向 K-12（小学高年级至初中阶段）数据科学与 AI 启蒙教育的开源合成数据生成引擎。

### **1.2 核心设计哲学**

* **编译与执行分离（Compiler-Runtime Separation）**：  
  * **LLM（Gemini API）作为“领域架构师 / 编译器”**：负责理解自然语言需求，调动医学与跨学科知识库，输出严谨合规的 **Simulation Blueprint（仿真蓝图 JSON）**。  
  * **C\# (.NET 10) 作为“确定性数学执行引擎 / 运行时”**：纯本地离线解析蓝图，执行高斯采样、多层随机发病判定、非线性衰减算法、生理边界截断（Clamping）与教学埋雷注入，极速输出 CSV/JSON。
* **多层级随机发病与严重度模型（Multi-Tier Stochastic Pathogenesis）**：  
  * 并非人人皆病：样本按独立随机概率划分为“完全健康基准人群”与“患病人群”。  
  * 个体差异化表型：每个病人患有的病症组合不同，且每种病症的严重程度（Severity）通过独立随机数确定并线性/非线性缩放生理偏移量。  
* **双表解耦导出机制（Dual-Table Architecture: Observables vs. Ground Truth）**：  
  * **观测特征表（Features Table）**：仅包含体征、生化检测指标与噪音数据，面向学生，作为数据挖掘、模式识别与聚类的分析靶场。  
  * **真值答案表（Ground Truth Table）**：单列存储病人的真实患病状态、具体病症列表、各病症严重度评分及主诊断标签，作为教师备课、模型验证与数据挖掘竞赛的“标准答案”。  
* **以磁盘为中心的工作台（Disk-Centric Workspace）**：每个功能模块独立解耦，操作产物即时持久化到本地目录，任何菜单随时可返回主菜单并互相调用中间产物。  
* **零手工配置负担**：通过自然语言或参数化输入动态构建场景，无需人工计算医学偏移量。  
* **杜绝初等加法失真**：内置非线性衰减与生理极限算法，确保多并发状态下的生理/物理真实度。  
* **教学工具零摩擦接入**：严格适配 CODAP、Google Colab、Excel 等免代码/轻代码教学分析工具。  
* **全公制单位强制规范**：所有物理及生理指标一律强制采用公制单位（如摄氏度 °C、千克 kg、厘米 cm、千米/小时 km/h 等）。

## **2\. 系统分层架构**

│              前端 / 静态 HTML 手动测试页面 (Static Tester Web UI)       │
│               (REST API 接口调用 / 蓝图创建 / 数据生成与预览)            │
                               │    
                               ▼  

│                      Web API 层 (EduDataGen.WebAPI)                    │
│   • REST Controllers (BlueprintsController, DatasetsController 等)     │
│   • 静态文件服务 (Hosting index.html 手动测试页面)                    │

                               │
                               ▼

│                 1\. ConnectedService 层 (Gemini API 等)               │
│   • 语义解析与领域逻辑推理 (Gemini Structured Outputs)                  │
│   • 调用 LLM 编译生成 Simulation Blueprint JSON                        │
   
                               │  通过 DAL 持久化落盘
                               ▼    
│          Simulation Blueprint JSON (./blueprints/\*.json)     │    
                               │  引擎载入
                               ▼  

│               2\. Engine 算术与业务逻辑层 (EduDataGen.Engine)          │
│   • Seed 伪随机数发生器 (PRNG)                               │    
│   • 随机分流：健康 vs 患病概率投掷 (Health Status Roll)       │    
│   • 条件激活与严重度采样 (Severity Factor Generator)          │    
│   • 状态修饰器与衰减累加器 (Attenuated Accumulator)           │    
│   • 生理上下限约束器 (Physiological Clamp)                   │    
│   • 教学脏数据注入器 (Pedagogical Traps Injector)            │  

                               │    
                               ▼    
│                   3\. DAL 数据访问层 (EduDataGen.DAL)                 │
│   • 双表 CSV 导出与读取 (Features CSV & Ground Truth CSV)              │
│   • 本地磁盘工作区 IO & Blueprint 存储管理                           │
│                                                             │    
│   \[学生数据靶场\]                  \[教师真值答案\]              │    
│   ./datasets/\*\_features.csv      ./datasets/\*\_ground\_truth.csv  │    
3\. Simulation Blueprint 规范标准  
Simulation Blueprint 是连接 LLM 语义层与 C\# 算术层的核心契约。更新后的标准强化了人群发病率、各病症发生概率与严重度分布的定义。

### **3.1 JSON Schema 核心字段定义**

{    
  "$schema": "https://json-schema.org/draft/2020-12/schema",    
  "title": "SimulationBlueprint",    
  "type": "object",    
  "required": \["Scenario", "Seed", "TotalRecords", "CohortSettings", "BaselineMetrics", "Conditions", "Anomalies"\],    
  "properties": {    
    "Scenario": { "type": "string" },    
    "Description": { "type": "string" },    
    "Seed": { "type": "integer" },    
    "TotalRecords": { "type": "integer" },    
    "UnitSystem": { "type": "string", "enum": \["Metric"\] },    
        
    "CohortSettings": {  
      "type": "object",  
      "required": \["HealthyRatio"\],  
      "properties": {  
        "HealthyRatio": {   
          "type": "number",   
          "minimum": 0,   
          "maximum": 1,  
          "description": "完全健康个体的基础比例（不患任何并发症）"   
        }  
      }  
    },  
        
    "BaselineMetrics": {    
      "type": "object",    
      "additionalProperties": {    
        "type": "object",    
        "required": \["Distribution", "Mean", "StdDev", "Limits"\],    
        "properties": {    
          "Distribution": { "type": "string", "enum": \["Gaussian", "Uniform", "IntegerUniform"\] },    
          "Mean": { "type": "number" },    
          "StdDev": { "type": "number" },    
          "Min": { "type": "number" },    
          "Max": { "type": "number" },    
          "Unit": { "type": "string" },    
          "Limits": {    
            "type": "object",    
            "required": \["HardMin", "HardMax"\],    
            "properties": {    
              "HardMin": { "type": "number" },    
              "HardMax": { "type": "number" },    
              "SoftSaturationThreshold": { "type": "number" }    
            }    
          }    
        }    
      }    
    },    
    
    "Conditions": {    
      "type": "object",    
      "additionalProperties": {    
        "type": "object",    
        "required": \["OccurrenceProbability", "SeverityDistribution", "Effects"\],    
        "properties": {    
          "OccurrenceProbability": {   
            "type": "number",   
            "minimum": 0,   
            "maximum": 1,  
            "description": "在非纯健康人群中，患有该病症的独立概率"  
          },    
          "Priority": { "type": "integer", "default": 0 },  
          "SeverityDistribution": {  
            "type": "object",  
            "required": \["Distribution", "MinFactor", "MaxFactor"\],  
            "properties": {  
              "Distribution": { "type": "string", "enum": \["Uniform", "Beta", "Discrete"\] },  
              "MinFactor": { "type": "number", "minimum": 0.1, "default": 0.2 },  
              "MaxFactor": { "type": "number", "maximum": 2.0, "default": 1.0 },  
              "DiscreteGrades": {  
                "type": "array",  
                "items": {  
                  "type": "object",  
                  "properties": {  
                    "GradeName": { "type": "string" },  
                    "Factor": { "type": "number" },  
                    "Weight": { "type": "number" }  
                  }  
                }  
              }  
            }  
          },  
          "Effects": {    
            "type": "object",    
            "additionalProperties": {    
              "type": "object",    
              "required": \["DeltaMean", "Mode"\],    
              "properties": {    
                "DeltaMean": { "type": "number" },    
                "DeltaStdDev": { "type": "number", "default": 0 },    
                "Mode": { "type": "string", "enum": \["Attenuated", "Linear", "Override"\] },    
                "Weight": { "type": "number", "default": 1.0 }    
              }    
            }    
          }    
        }    
      }    
    },    
    
    "Anomalies": {    
      "type": "object",    
      "properties": {    
        "MissingValueRate": { "type": "number", "minimum": 0, "maximum": 1 },    
        "Outliers": {    
          "type": "array",    
          "items": {    
            "type": "object",    
            "required": \["Field", "Value", "Count"\],    
            "properties": {    
              "Field": { "type": "string" },    
              "Value": { "type": \["number", "string"\] },    
              "Count": { "type": "integer" }    
            }    
          }    
        },    
        "TypoInconsistencies": {    
          "type": "array",    
          "items": {    
            "type": "object",    
            "required": \["Field", "OriginalValue", "CorruptedValues"\],    
            "properties": {    
              "Field": { "type": "string" },    
              "OriginalValue": { "type": "string" },    
              "CorruptedValues": { "type": "array", "items": { "type": "string" } }    
            }    
          }    
        }    
      }    
    }    
  }    
}

## **4\. C\# 执行引擎核心功能模块**

### **4.1 确定性与可复现性（PRNG & Seed）**

* 引擎统一采用 System.Random(Seed) 或 MathNet.Numerics.Random.MersenneTwister。  
* 在配置相同、种子相同的情况下，所有个体的健康状态判断、并发病症抽取、严重度随机采样、正态分布采样及异常值注入位置均完全确定。

### **4.2 三层随机发病生成流水线（Stochastic Pathogenesis Pipeline）**

每个生成样本依次经过三层随机检验：

* **第一层（群体分流）：健康 vs 患病抽取**  
  * 引擎生成均匀随机数 $R\_1 \\in \[0, 1)$。  
  * 若 $R\_1 \< \\text{HealthyRatio}$，该个体被标记为纯健康样本（Is\_Patient \= false），直接跳过后续疾病修饰器，各项生理指标纯净采自 Baseline。  
* **第二层（表型分流）：并发病症组合抽取**  
  * 对于进入患病池的样本，依次遍历蓝图中的 Conditions。  
  * 对每个病症生成随机数 $R\_2 \\in \[0, 1)$。若 $R\_2 \< \\text{OccurrenceProbability}$，则该病症在该样本上被激活（如同时命中糖尿病与高血压）。  
  * 若遍历结束未命中任何病症，强制指派一个最低优先级的单发病症，确保患病池样本至少具备一个临床异常。  
* **第三层（严重度分流）：个体严重度因子采样**  
  * 对每个激活的病症，独立抽取该样本的严重度系数 $S \\in \[\\text{MinFactor}, \\text{MaxFactor}\]$。  
  * 实际病理偏移量由基础偏移乘以严重度系数调节：  
    $$\\Delta\_{\\text{actual}} \= (\\Delta\_{\\text{mean}} \\times S) \+ \\mathcal{N}(0, \\Delta\_{\\text{stddev}} \\times S)$$

### **4.3 统计分布采样模块（Distribution Samplers）**

* **正态分布（Box-Muller Transform / Ziggurat Algorithm）**：  
  $$Z \= \\mu \+ \\sigma \\cdot \\sqrt{-2 \\ln U\_1} \\cos(2\\pi U\_2)$$

* **均匀分布与离散整数分布**。  
* **加权离散分布（Weighted Roulette Sampling）**：用于严重度离散等级（轻度/中度/重度）的选择。

### **4.4 非线性累加与生理约束运算（Attenuated Math & Clamping）**

* **边际递减累加算法（Diminishing Returns）**： 当样本同时命中 $N$ 个病症修饰器时，对同一生理指标的有效 Delta 进行降序排列并加权累加：  
  $$\\Delta\_{\\text{total}} \= \\Delta\_{(1)} \+ \\sum\_{i=2}^{N} \\left( \\Delta\_{(i)} \\cdot \\gamma^{i-1} \\right)$$

  其中 $\\gamma$ 为衰减因子（默认 $\\gamma \= 0.35$）。  
* **优先级覆盖机制（Override Mode）**： 高优先级状态（如心源性休克，Priority: 10）直接覆盖低优先级修饰器。  
* **生理边界截断（Clamping）**：  
  $$Value\_{\\text{final}} \= \\min(\\max(Value\_{\\text{calculated}}, \\text{HardMin}), \\text{HardMax})$$

### **4.5 教学埋雷引擎（Pedagogical Anomaly Injector）**

* **离群点注入（Outliers）**：强制将特定行的指定字段替换为物理违规值（如体温 $99.0^\\circ\\text{C}$、年龄 $-5$）。  
* **缺失值注入（Missing Values）**：按概率将数据槽位置空或填入 NaN / 空字符串。  
* **文本一致性破坏（Inconsistent Typos）**：随机替换文本类别为带空格、不同大小写或错别字（如 "Mild "、"mild"、"Mlid"）。  
* **注入边界保护**：埋雷操作仅作用于导出的**观测特征表（Features Table）**，绝不破坏真值答案表（Ground Truth Table）中的客观事实记录。

## **5\. 双表导出规范（Dual-Table Specification）**

每次渲染任务均同步生成并落盘两份严格对应的 CSV 文件。

### **5.1 观测特征表（{Scenario}\_{Timestamp}\_features.csv）**

供学生在 CODAP、Google Colab 中分析挖掘，字段完全模拟真实医疗检验报告，不包含直接答案。

Patient\_ID,Age,Body\_Temp\_C,Heart\_Rate,Systolic\_BP,Blood\_Glucose\_mg\_dL,Cough\_Days  
PAT-0001,13,36.8,76,112,92,0  
PAT-0002,14,37.1,84,138,168,1  
PAT-0003,11,39.4,115,108,98,4  
PAT-0004,12,36.6,72,110,88,0  
PAT-0005,15,36.9,80,99.0,94,2  
*(注：PAT-0005 包含体温 99.0°C 的教学脏数据)*

### **5.2 真值答案表（{Scenario}\_{Timestamp}\_ground\_truth.csv）**

供教师备课、评估学生分析模型准确度、或在数据挖掘任务中充当标准检验集（Test Ground Truth）。

Patient\_ID,Is\_Patient,Condition\_Count,Active\_Conditions,Severity\_Details,Primary\_Diagnosis  
PAT-0001,False,0,None,None,Healthy  
PAT-0002,True,2,Hypertension;Diabetes,Hypertension:0.65;Diabetes:0.82,Type2\_Diabetes  
PAT-0003,True,1,Acute\_Infection,Acute\_Infection:0.95,Acute\_Infection  
PAT-0004,False,0,None,None,Healthy  
PAT-0005,True,1,Mild\_Viral,Mild\_Viral:0.35,Mild\_Viral

## **6\. Gemini 蓝图生成器（LLM Blueprint Generator）**

### **6.1 交互与生成机制**

> 1. 用户在控制台子菜单输入自然语言需求（例如："10-15岁青少年急救体检数据 包含中暑与哮喘"）。  
> 2. C\# 工具调用 Gemini API（采用 Structured Outputs 结构化输出模式）。  
> 3. 注入系统提示词（System Instruction），强制 Gemini 充当生理学与数据科学架构师，推导合理的人群健康比例、各病症独立概率及其严重度分布区间，返回严格符合 Schema 的 Simulation Blueprint。  
> 4. 引擎将生成的 Blueprint 自动持久化保存至本地 ./blueprints/{Scenario}\_{Seed}.json。

### **6.2 Gemini 架构系统提示词规范**

You are an expert Clinical Physiologist and Senior Data Architect.  
Your task is to generate a deterministic "Simulation Blueprint" JSON based on the user's scenario request.

CRITICAL CONSTRAINTS:  
1\. Always use the METRIC system (e.g., Celsius for temperature, kg for weight, cm/m for height, km/h for speed). Never use American imperial units under any circumstances.  
2\. Establish a realistic baseline (Mean, StdDev, Hard Limits) for the target demographic.  
3\. Configure stochastic cohort parameters: Specify a reasonable HealthyRatio (e.g., 0.3-0.5 for triage, 0.8 for general screenings).  
4\. For each condition, define its OccurrenceProbability among the diseased population, and define its SeverityDistribution (MinFactor, MaxFactor) to reflect individual disease variance.  
5\. For diseased or altered conditions, specify non-linear Delta shifts and attenuation factors to prevent biological impossibilities (1+1 does not equal 2 in physiology).  
6\. Define pedagogically valuable anomalies for middle-school data cleaning exercises.  
7\. Output ONLY valid JSON matching the provided JSON Schema.

## **7\. 控制台菜单交互系统与持久化规范**

系统采用状态机交互控制台菜单（Interactive TUI），配合本地磁盘工作区管理。所有功能执行完毕后自动持久化落盘，并支持随时返回主菜单。

### **7.1 工作区与磁盘持久化规范**

程序首次运行时在工作目录下自动初始化标准工作区：

./EduDataGen\_Workspace/    
├── blueprints/       \# 存储由 Gemini 生成或用户自定义的蓝图 (.json)  
├── presets/          \# 内置开箱即用的官方教学预置蓝图 (.json)  
├── datasets/         \# 离线渲染输出的双表数据集 (\*\_features.csv, \*\_ground\_truth.csv)  
└── configs/          \# 系统运行配置文件 (settings.json, API Key 等)

### **7.2 状态机主菜单设计**

\======================================================    
         EduDataGen \- 教学合成数据工作台 v1.0   
\======================================================    
 \[1\] 🌟 新建蓝图 (AI 架构师 / 调用 Gemini API 生成 Blueprint)   
 \[2\] ⚙️ 渲染数据 (选择已有 Blueprint，离线生成 特征表 \+ 真值表)   
 \[3\] 🧪 数据埋雷与调优 (载入已有特征表，二次注入教学脏数据)   
 \[4\] 📂 资产管理器 (浏览 / 预览本地 Blueprint、特征表与真值表)   
 \[5\] 🛠️ 系统设置 (配置 API Key、默认路径、全局种子)   
 \[0\] 🚪 退出程序   
\======================================================

### **7.3 各功能子菜单流转逻辑**

**\[1\] 新建蓝图（Gemini Blueprint Builder）**

* **Step 1（需求输入）**：提示输入场景自然语言（如："10-15岁青少年运动体能与脱水，公制单位"）。  
* **Step 2（参数确认）**：确认/修改建议样本量（默认 100）、健康人群比例（默认 0.4）、随机种子（默认 2026）、异常率（默认 0.05）。  
* **Step 3（API 生成）**：展示 Spinner 加载动画，调用 Gemini API 生成结构化 JSON。  
* **Step 4（落盘与分支）**：  
  * 文件保存至 ./blueprints/{Scenario}\_{Seed}.json。  
  * 提示选择：\[A\] 立即使用此蓝图渲染数据 或 \[B\] 保存并返回主菜单。

**\[2\] 渲染数据（Deterministic Dual-Table Renderer）**

* **Step 1（选择蓝图）**：自动扫描 ./blueprints/ 与 ./presets/ 目录，以列表形式展示所有可用 .json 供光标选择。  
* **Step 2（生成设置）**：输入生成记录行数（回车默认继承蓝图设置）。  
* **Step 3（执行渲染）**：离线完成人群健康分流、并发病症抽取、个体严重度缩放、高斯采样、衰减累加、生理 Clamping 与异常注入。  
* **Step 4（双表落盘与预览）**：  
  * 生成并输出：  
    * ./datasets/{Scenario}\_{Timestamp}\_features.csv  
    * ./datasets/{Scenario}\_{Timestamp}\_ground\_truth.csv  
  * 终端使用分屏或双表格形式展示前 5 行对比（特征表 vs 答案表）。  
  * 按任意键返回主菜单。

**\[3\] 数据埋雷与调优（Data Trap Studio）**

* 用于教师备课时对现有干净特征数据集进行二次注入（绝不改动 Ground Truth）：  
* **Step 1**：从 ./datasets/ 列表中选择目标 \*\_features.csv。  
* **Step 2**：选择埋雷动作（1. 极端离群值、2. 批量制造空值、3. 文本类别错别字）。  
* **Step 3**：另存为新特征文件（如 medical\_triage\_corrupted\_features.csv）并返回主菜单。

**\[4\] 资产管理器（Asset Explorer）**

* 列表查看当前所有 Blueprint 与 CSV 的文件配对情况、文件大小与生成时间。  
* 支持一键对比检查：查看某特征表对应的真值表分布（患病率、疾病频次统计）。

**\[5\] 系统设置（System Settings）**

* 配置与保存 Gemini API Key（安全存储于本地 configs/settings.json）。  
* 配置默认导出目录与全局默认 Seed。

## **8\. 预置场景与教学套件（Built-in Presets）**

仓库在 ./presets/ 目录下默认附带经过医学与教学验证的开箱即用 Blueprint 模板：

| 预置模板文件  | 教学场景  | 包含的核心特征  | 真值表（Ground Truth）提供的答案维度 |
| :---- | :---- | :---- | :---- |
| medical-triage.json  | 救护车体征筛查 | 年龄、体温（°C）、心率、收缩压、血糖、咳嗽天数 | 是否患病、高血压程度、糖尿病程度、急性感染程度 |
| youth-soccer.json  | 青少年足球体能 | 跑动距离（km）、冲刺极速（km/h）、平均心率、疲劳指数 | 球员状态（充沛 / 肌肉疲劳 / 轻度脱水 / 严重透支） |
| pediatric-vitals.json  | 儿科生长曲线 | 月龄、身高（cm）、体重（kg）、头围（cm） | 发育评估（健康正常 / 营养不良 / 生长迟缓 / 肥胖） |

## **9\. 技术栈与依赖选型**

* **运行时**：.NET 10 (C\# 12 / 13 / 14\)
* **控制台 TUI & 状态机**：Spectre.Console（用于交互式选择菜单、动态表格渲染、状态机循环与加载动画）  
* **LLM 集成**：Google\_GenerativeAI 官方 SDK 或轻量 REST HttpClient（用于与 Gemini 1.5 Pro / Flash 进行 Structured Outputs 交互）  
* **JSON 序列化与 Schema 验证**：System.Text.Json \+ Json.NET.Schema

* **双表 CSV 导出**：CsvHelper（严格遵循 RFC 4180 标准，UTF-8 with BOM 以无缝兼容 Excel 与 CODAP）  
* **数学扩展**：MathNet.Numerics（用于精确正态分布采样与随机数生成）

## **10\. 演进路线图（Roadmap）**

* **Phase 1（核心 MVP）**：  
  * 构建以本地磁盘为中心的文件目录结构与 Spectre.Console 主菜单状态机。  
  * 实现 Blueprint JSON 解析与本地确定性 C\# 渲染引擎\[cite: 2\]。  
  * 实现健康分流、多病症独立随机激活、严重度缩放算法。  
  * 实现双表同步导出（Features CSV \+ Ground Truth CSV）\[cite: 2\]。  
* **Phase 2（Gemini 菜单集成与数据工作室）**\[cite: 2\]：  
  * 实现菜单项 \[1\] 新建蓝图 对接 Gemini API，支持自动生成人群发病概率与严重度配置\[cite: 2\]。  
  * 实现菜单项 \[3\] 数据埋雷与调优 与 \[4\] 资产管理器（支持双表对齐查验）\[cite: 2\]。  
* **Phase 3（开源与教学套件发布）**\[cite: 2\]：  
  * 完善 GitHub 仓库开源工程规范与 CI/CD 构建脚本\[cite: 2\]。  
  * 发布 3 套标准教学 Blueprint 及配套 CODAP 数据挖掘验证指南（指导学生如何用挖掘结果比对 Ground Truth）。

# **EduDataGen (Synthetic Data Engine) Requirements and Architecture Specification**

## **1\. Project Overview and Design Vision**

### **1.1 Project Positioning**

**EduDataGen** is an open-source synthetic data generation engine for data science and AI introductory education at the K-12 level (upper elementary to middle school).

### **1.2 Core Design Philosophy**

* **Compiler-Runtime Separation**:  
  * **LLM (Gemini API) as "Domain Architect / Compiler"**: Responsible for understanding natural language requirements, leveraging medical and interdisciplinary knowledge bases, and outputting rigorous, compliant **Simulation Blueprints (JSON)**.  
  * **C\# (.NET 10) as "Deterministic Mathematical Execution Engine / Runtime"**: Performs local, offline parsing of blueprints, executing Gaussian sampling, multi-layer stochastic pathogenesis determination, non-linear attenuation algorithms, physiological boundary clamping (Clamping), and pedagogical trap injection, outputting CSV/JSON at high speed.
* **Multi-Tier Stochastic Pathogenesis Model**:  
  * Populations are divided into "Baseline Healthy Population" and "Patient Population" based on independent random probabilities.  
  * Individualized phenotypes: Each patient has a unique combination of conditions, and the severity of each condition is determined by independent random numbers, linearly/non-linearly scaling physiological offsets.  
* **Dual-Table Architecture (Observables vs. Ground Truth)**:  
  * **Features Table**: Contains only vitals, biochemical indicators, and noise data, designed for students as an analytical field for data mining, pattern recognition, and clustering.  
  * **Ground Truth Table**: Stores the patient's actual health status, specific condition list, severity scores for each condition, and primary diagnosis label in a single column, serving as an "answer key" for teacher preparation, model validation, and data mining competitions.  
* **Disk-Centric Workspace**: Each functional module is decoupled; output artifacts are immediately persisted to local directories, with the ability to return to the main menu and cross-invoke intermediate artifacts at any time.  
* **Zero Manual Configuration Burden**: Dynamically build scenarios through natural language or parameterized input; no manual calculation of medical offsets required.  
* **Elimination of Elementary Addition Distortion**: Built-in non-linear attenuation and physiological limit algorithms ensure physiological/physical realism under multi-morbidity conditions.  
* **Frictionless Integration with Teaching Tools**: Strictly adapted for no-code/low-code analysis tools like CODAP, Google Colab, and Excel.  
* **Forced Metric System Standardization**: All physical and physiological indicators are strictly forced to use metric units (e.g., degrees Celsius °C, kilograms kg, centimeters cm, kilometers per hour km/h, etc.).

## **2\. System Layered Architecture**

│                 Interactive Control Console (Interactive TUI)            │    
│   (Main Menu State Machine: New Blueprint / Offline Rendering / Data Traps / Asset Mgmt)       │    
                               │    
                               ▼  

│            1\. Blueprint Generator (Gemini API)              │    
│   • Semantic parsing and domain logic reasoning                                    │    
│   • Establish physiological baselines and pathological offsets (Delta Shifts)          │    
│   • Define disease occurrence probability distributions and severity ranges (Severity Models)     │    
│   • Calculate non-linear attenuation weights and physical boundaries (Limits)                     │    
│   • Plan pedagogical trap strategies (Anomalies)                             │    
   
                               │  Output and Persist to Disk    
                               ▼    
│          Simulation Blueprint JSON (./blueprints/\*.json)     │    
                               │  Load Engine    
                               ▼  

│               2\. C\# Math Engine Runtime (.NET)              │    
│   • Seeded Pseudo-Random Number Generator (PRNG)                               │    
│   • Stochastic diversion: Health Status Roll (Healthy vs. Diseased)       │    
│   • Conditional activation and severity sampling (Severity Factor Generator)          │    
│   • State modifiers and attenuation accumulators (Attenuated Accumulator)           │    
│   • Physiological constraint clamping (Physiological Clamp)                   │    
│   • Pedagogical dirty data injector (Pedagogical Traps Injector)            │  

                               │    
                               ▼    
│                      3\. Dual-Table Export Layer (Exporters)               │    
│                                                             │    
│   \[Student Data Range\]                  \[Teacher Ground Truth\]              │    
│   ./datasets/\*\_features.csv      ./datasets/\*\_ground\_truth.csv  │    
│   • Patient\_ID                   • Patient\_ID               │    
│   • Age / Gender                 • Is\_Patient (Boolean)       │    
│   • Observed Vitals (Temp, BP, HR)  • Active\_Conditions (Condition Name)│    
│   • Biochemical Indicators (Glucose, etc.)            • Severity\_Scores (Severity)  │    
│   • Dirty Data Traps                   • Primary\_Diagnosis (Main Diagnosis)│    
│   (Drag into CODAP / Colab)           (Used for auto-alignment, grading, & validation) │  

## **3\. Simulation Blueprint Specification**

The Simulation Blueprint is the core contract connecting the LLM semantic layer and the C\# arithmetic layer. The updated standard reinforces definitions for population prevalence, individual condition occurrence probabilities, and severity distributions.

### **3.1 JSON Schema Core Field Definitions**

{    
  "$schema": "https://json-schema.org/draft/2020-12/schema",    
  "title": "SimulationBlueprint",    
  "type": "object",    
  "required": \["Scenario", "Seed", "TotalRecords", "CohortSettings", "BaselineMetrics", "Conditions", "Anomalies"\],    
  "properties": {    
    "Scenario": { "type": "string" },    
    "Description": { "type": "string" },    
    "Seed": { "type": "integer" },    
    "TotalRecords": { "type": "integer" },    
    "UnitSystem": { "type": "string", "enum": \["Metric"\] },    
        
    "CohortSettings": {  
      "type": "object",  
      "required": \["HealthyRatio"\],  
      "properties": {  
        "HealthyRatio": {   
          "type": "number",   
          "minimum": 0,   
          "maximum": 1,  
          "description": "Base ratio of completely healthy individuals (no complications)"   
        }  
      }  
    },  
        
    "BaselineMetrics": {    
      "type": "object",    
      "additionalProperties": {    
        "type": "object",    
        "required": \["Distribution", "Mean", "StdDev", "Limits"\],    
        "properties": {    
          "Distribution": { "type": "string", "enum": \["Gaussian", "Uniform", "IntegerUniform"\] },    
          "Mean": { "type": "number" },    
          "StdDev": { "type": "number" },    
          "Min": { "type": "number" },    
          "Max": { "type": "number" },    
          "Unit": { "type": "string" },    
          "Limits": {    
            "type": "object",    
            "required": \["HardMin", "HardMax"\],    
            "properties": {    
              "HardMin": { "type": "number" },    
              "HardMax": { "type": "number" },    
              "SoftSaturationThreshold": { "type": "number" }    
            }    
          }    
        }    
      }    
    },    
    
    "Conditions": {    
      "type": "object",    
      "additionalProperties": {    
        "type": "object",    
        "required": \["OccurrenceProbability", "SeverityDistribution", "Effects"\],    
        "properties": {    
          "OccurrenceProbability": {   
            "type": "number",   
            "minimum": 0,   
            "maximum": 1,  
            "description": "Independent probability of having this condition within the non-healthy population"  
          },    
          "Priority": { "type": "integer", "default": 0 },  
          "SeverityDistribution": {  
            "type": "object",  
            "required": \["Distribution", "MinFactor", "MaxFactor"\],  
            "properties": {  
              "Distribution": { "type": "string", "enum": \["Uniform", "Beta", "Discrete"\] },  
              "MinFactor": { "type": "number", "minimum": 0.1, "default": 0.2 },  
              "MaxFactor": { "type": "number", "maximum": 2.0, "default": 1.0 },  
              "DiscreteGrades": {  
                "type": "array",  
                "items": {  
                  "type": "object",  
                  "properties": {  
                    "GradeName": { "type": "string" },  
                    "Factor": { "type": "number" },  
                    "Weight": { "type": "number" }  
                  }  
                }  
              }  
            }  
          },  
          "Effects": {    
            "type": "object",    
            "additionalProperties": {    
              "type": "object",    
              "required": \["DeltaMean", "Mode"\],    
              "properties": {    
                "DeltaMean": { "type": "number" },    
                "DeltaStdDev": { "type": "number", "default": 0 },    
                "Mode": { "type": "string", "enum": \["Attenuated", "Linear", "Override"\] },    
                "Weight": { "type": "number", "default": 1.0 }    
              }    
            }    
          }    
        }    
      }    
    },    
    
    "Anomalies": {    
      "type": "object",    
      "properties": {    
        "MissingValueRate": { "type": "number", "minimum": 0, "maximum": 1 },    
        "Outliers": {    
          "type": "array",    
          "items": {    
            "type": "object",    
            "required": \["Field", "Value", "Count"\],    
            "properties": {    
              "Field": { "type": "string" },    
              "Value": { "type": \["number", "string"\] },    
              "Count": { "type": "integer" }    
            }    
          }    
        },    
        "TypoInconsistencies": {    
          "type": "array",    
          "items": {    
            "type": "object",    
            "required": \["Field", "OriginalValue", "CorruptedValues"\],    
            "properties": {    
              "Field": { "type": "string" },    
              "OriginalValue": { "type": "string" },    
              "CorruptedValues": { "type": "array", "items": { "type": "string" } }    
            }    
          }    
        }    
      }    
    }    
  }    
}

## **4\. C\# Execution Engine Core Functional Modules**

### **4.1 Determinism and Reproducibility (PRNG & Seed)**

* The engine uniformly uses System.Random(Seed) or MathNet.Numerics.Random.MersenneTwister.  
* With identical configurations and seeds, all outcomes (health status determination, concurrent condition selection, severity random sampling, normal distribution sampling, and anomaly injection locations) are completely deterministic.

### **4.2 Three-Tier Stochastic Pathogenesis Pipeline**

Each generated sample passes through three layers of random checks:

* **Tier 1 (Population Diversion): Healthy vs. Diseased Roll**  
  * The engine generates a uniform random number $R\_1 \\in \[0, 1)$.  
  * If $R\_1 \< \\text{HealthyRatio}$, the individual is marked as a purely healthy sample (Is\_Patient \= false), bypassing subsequent condition modifiers; all physiological indicators are sampled purely from the Baseline.  
* **Tier 2 (Phenotypic Diversion): Concurrent Condition Combination Selection**  
  * For samples entering the diseased pool, traverse the Conditions in the blueprint sequentially.  
  * For each condition, generate a random number $R\_2 \\in \[0, 1)$. If $R\_2 \< \\text{OccurrenceProbability}$, the condition is activated on that sample (e.g., hitting both Diabetes and Hypertension).  
  * If no conditions are hit by the end of the traversal, a lowest-priority, single condition is mandatorily assigned to ensure the diseased pool contains at least one clinical anomaly.  
* **Tier 3 (Severity Diversion): Individual Severity Factor Sampling**  
  * For each activated condition, independently sample the severity factor $S \\in \[\\text{MinFactor}, \\text{MaxFactor}\]$ for that sample.  
  * The actual pathological offset is adjusted by the base offset multiplied by the severity factor:  
    $$\\Delta\_{\\text{actual}} \= (\\Delta\_{\\text{mean}} \\times S) \+ \\mathcal{N}(0, \\Delta\_{\\text{stddev}} \\times S)$$

### **4.3 Distribution Sampling Modules**

* **Normal Distribution (Box-Muller Transform / Ziggurat Algorithm)**:  
  $$Z \= \\mu \+ \\sigma \\cdot \\sqrt{-2 \\ln U\_1} \\cos(2\\pi U\_2)$$

* **Uniform Distribution and Discrete Integer Distribution**.  
* **Weighted Roulette Sampling**: Used for selecting severity grades (Mild/Moderate/Severe).

### **4.4 Non-linear Accumulation and Physiological Constraint Operations (Attenuated Math & Clamping)**

* **Diminishing Returns Accumulation**: When a sample hits $N$ condition modifiers simultaneously, the effective Deltas for the same physiological indicator are sorted in descending order and accumulated using weights:  
  $$\\Delta\_{\\text{total}} \= \\Delta\_{(1)} \+ \\sum\_{i=2}^{N} \\left( \\Delta\_{(i)} \\cdot \\gamma^{i-1} \\right)$$

  Where $\\gamma$ is the attenuation factor (default $\\gamma \= 0.35$).  
* **Priority Override Mechanism**: High-priority conditions (e.g., Cardiogenic Shock, Priority: 10\) directly override low-priority modifiers.  
* **Physiological Clamping**:  
  $$Value\_{\\text{final}} \= \\min(\\max(Value\_{\\text{calculated}}, \\text{HardMin}), \\text{HardMax})$$

### **4.5 Pedagogical Anomaly Injector**

* **Outlier Injection**: Forces replacement of specific fields in specific rows with physically impossible values (e.g., body temperature $99.0^\\circ\\text{C}$, age $-5$).  
* **Missing Value Injection**: Sets data slots to empty or NaN/empty string based on probability.  
* **Inconsistent Typo Injection**: Randomly replaces text categories with variants containing spaces, different casing, or typos (e.g., "Mild ", "mild", "Mlid").  
* **Injection Boundary Protection**: Anomaly injection operations only act on the exported **Features Table** and never corrupt the objective factual records in the **Ground Truth Table**.

## **5\. Dual-Table Export Specification**

Each rendering task synchronously generates and persists two strictly corresponding CSV files.

### **5.1 Features Table ({Scenario}\_{Timestamp}\_features.csv)**

Designed for student analysis and mining in CODAP/Google Colab; fields perfectly simulate real medical reports without containing direct answers.  
Patient\_ID,Age,Body\_Temp\_C,Heart\_Rate,Systolic\_BP,Blood\_Glucose\_mg\_dL,Cough\_Days  
PAT-0001,13,36.8,76,112,92,0  
PAT-0002,14,37.1,84,138,168,1  
PAT-0003,11,39.4,115,108,98,4  
PAT-0004,12,36.6,72,110,88,0  
PAT-0005,15,36.9,80,99.0,94,2  
*(Note: PAT-0005 contains pedagogical dirty data: 99.0°C)*

### **5.2 Ground Truth Table ({Scenario}\_{Timestamp}\_ground\_truth.csv)**

For teacher lesson preparation, evaluating student analysis model accuracy, or acting as a standard test set for data mining tasks.  
Patient\_ID,Is\_Patient,Condition\_Count,Active\_Conditions,Severity\_Details,Primary\_Diagnosis  
PAT-0001,False,0,None,None,Healthy  
PAT-0002,True,2,Hypertension;Diabetes,Hypertension:0.65;Diabetes:0.82,Type2\_Diabetes  
PAT-0003,True,1,Acute\_Infection,Acute\_Infection:0.95,Acute\_Infection  
PAT-0004,False,0,None,None,Healthy  
PAT-0005,True,1,Mild\_Viral,Mild\_Viral:0.35,Mild\_Viral

## **6\. Gemini Blueprint Generator (LLM Blueprint Generator)**

### **6.1 Interaction and Generation Mechanism**

> 5. User enters natural language requirements in the console sub-menu (e.g., "10-15 year old teenager emergency triage data including heatstroke and asthma").  
> 6. C\# tool calls the Gemini API (using Structured Outputs mode).  
> 7. Inject system instructions, forcing Gemini to act as a physiologist and data scientist architect, deducing reasonable population health ratios, independent condition probabilities, and severity ranges, returning a Simulation Blueprint that strictly complies with the Schema.  
> 8. The engine automatically persists the generated Blueprint to local ./blueprints/{Scenario}\_{Seed}.json.

### **6.2 Gemini Architecture System Prompt Specification**

You are an expert Clinical Physiologist and Senior Data Architect.  
Your task is to generate a deterministic "Simulation Blueprint" JSON based on the user's scenario request.

CRITICAL CONSTRAINTS:  
1\. Always use the METRIC system (e.g., Celsius for temperature, kg for weight, cm/m for height, km/h for speed). Never use American imperial units under any circumstances.  
2\. Establish a realistic baseline (Mean, StdDev, Hard Limits) for the target demographic.  
3\. Configure stochastic cohort parameters: Specify a reasonable HealthyRatio (e.g., 0.3-0.5 for triage, 0.8 for general screenings).  
4\. For each condition, define its OccurrenceProbability among the diseased population, and define its SeverityDistribution (MinFactor, MaxFactor) to reflect individual disease variance.  
5\. For diseased or altered conditions, specify non-linear Delta shifts and attenuation factors to prevent biological impossibilities (1+1 does not equal 2 in physiology).  
6\. Define pedagogically valuable anomalies for middle-school data cleaning exercises.  
7\. Output ONLY valid JSON matching the provided JSON Schema.

## **7\. Console Menu Interaction System and Persistence Specification**

The system uses an Interactive TUI with local disk workspace management. All actions automatically persist upon completion, with support for returning to the main menu at any time.

### **7.1 Workspace and Persistence Specification**

Upon first run, the program initializes a standard workspace:  
./EduDataGen\_Workspace/    
├── blueprints/       \# Stores blueprints (.json) generated by Gemini or user  
├── presets/          \# Built-in ready-to-use official pedagogical blueprints (.json)  
├── datasets/         \# Dual-table datasets generated by offline rendering (\*\_features.csv, \*\_ground\_truth.csv)  
└── configs/          \# System configuration files (settings.json, API Key, etc.)

### **7.2 Main Menu Design**

\======================================================    
         EduDataGen \- Teaching Synthetic Data Workbench v1.0   
\======================================================    
 \[1\] 🌟 New Blueprint (AI Architect / Call Gemini API)   
 \[2\] ⚙️ Render Data (Select existing Blueprint, generate Features \+ Truth tables)   
 \[3\] 🧪 Data Traps & Optimization (Inject pedagogical dirty data)   
 \[4\] 📂 Asset Manager (Browse/Preview blueprints, tables)   
 \[5\] 🛠️ System Settings (API Key, Paths, Seeds)   
 \[0\] 🚪 Exit   
\======================================================

### **7.3 Function Flow Logic**

* **\[1\] New Blueprint (Gemini Blueprint Builder)**: User inputs requirements, adjusts cohort parameters, Gemini generates JSON, file is saved to ./blueprints/.  
* **\[2\] Render Data (Deterministic Dual-Table Renderer)**: Selects a blueprint, enters record count, engine generates two CSV files (Features & Ground Truth), and displays a preview in the terminal.  
* **\[3\] Data Traps (Data Trap Studio)**: Injects anomalies into existing Feature tables (never touches Ground Truth).  
* **\[4\] Asset Manager (Asset Explorer)**: Views file pairings and checks distribution statistics.  
* **\[5\] System Settings**: Manages API Keys and path defaults.

## **8\. Built-in Presets and Teaching Kits**

| Preset Template | Teaching Scenario | Core Features Included | Answer Dimensions (Ground Truth) |
| :---- | :---- | :---- | :---- |
| medical-triage.json | Ambulance Triage | Age, Temp (°C), HR, SysBP, Glucose, Cough Days | Illness status, Hypertension severity, Diabetes severity, Infection severity |
| youth-soccer.json | Youth Soccer Fitness | Distance (km), Sprint speed (km/h), HR, Fatigue index | Player state (Energized / Muscle fatigue / Dehydrated / Exhausted) |
| pediatric-vitals.json | Pediatric Growth Curves | Age, Height (cm), Weight (kg), Head Circumference (cm) | Development status (Normal / Malnourished / Stunted / Obese) |

## **9\. Technology Stack and Dependencies**

* **Runtime**: .NET 10 (C\# 12 / 13 / 14\)
* **TUI & State Machine**: Spectre.Console  
* **LLM Integration**: Google\_GenerativeAI SDK  
* **JSON Serialization**: System.Text.Json \+ Json.NET.Schema  
* **CSV Export**: CsvHelper  
* **Math Library**: MathNet.Numerics

## **10\. Roadmap**

* **Phase 1 (Core MVP)**: Local disk file structure, Spectre.Console menu, Blueprint JSON parsing, deterministic C\# engine, multi-condition stochastic activation, dual-table export.  
* **Phase 2 (Gemini & Data Studio)**: Gemini API integration for blueprint generation, Data Trap Studio, Asset Manager.

**Phase 3 (Open Source & Kits)**: GitHub repository standards, CI/CD, 3 standard teaching blueprints, and CODAP guide.