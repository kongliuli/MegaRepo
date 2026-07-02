# autoCrafter 复活说明

日期：2026-07-02
仓库：`D:\Code\githubDown\autoCrafter`
结论：计划删除；保留本文档用于未来复活。

## 项目定位

`autoCrafter` 是 Path of Exile 自动做装工具。

它按五层结构组织：

- L0 UI
- L1 数据
- L2 感知
- L3 决策
- L4 执行

项目是真实代码，不是空壳，但领域非常窄，只在继续做 PoE 自动化时值得恢复。

## 当前组成

| 路径 | 价值 |
| --- | --- |
| `main.py` | 主入口 |
| `autocrafter/layers/l0_ui` | PyQt6 UI，包含普通、势力、瓦尔、地心、批量做装 tab |
| `autocrafter/layers/l1_data` | 词缀、基底、通货、势力等数据加载 |
| `autocrafter/layers/l2_perception` | 剪贴板读取、物品解析、势力/瓦尔/地心识别 |
| `autocrafter/layers/l3_decision` | 策略、状态机、通货选择、批量引擎 |
| `autocrafter/layers/l4_execution` | 输入模拟、SendInput、虚拟桌面、执行引擎 |
| `autocrafter/utils` | 安全限制、方案校验、方案管理 |
| `autocrafter/tests` | 分层测试和集成测试 |
| `autocrafter/data/*.json` | 精简后的做装数据 |

## 技术栈

- Python
- PyQt6
- lxml
- python-docx
- Windows 输入模拟 / SendInput 思路

## 可复用点

未来如果要复活，只取这些：

- 安全限制：`autocrafter/utils/safety_manager.py`
- 方案管理和校验：`craft_scheme_manager.py`、`craft_scheme_validator.py`
- 物品解析：`layers/l2_perception/item_parser.py`
- 决策策略：`layers/l3_decision/*strategy.py`、`state_machine.py`
- 输入执行：`layers/l4_execution/send_input_controller.py`、`input_simulator.py`
- 分层测试：`autocrafter/tests`

## 不建议整仓恢复的原因

1. 项目领域过窄，只服务 PoE 自动做装。
2. `autocrafter/data/PyPoE-master` 和 `repoe-master` 像第三方/数据镜像，体积和维护成本高。
3. UI、数据、感知、执行都耦合具体游戏场景。
4. 自动化操作需要重新确认安全边界，不能盲目继续跑。

## 复活路径

如果后续继续做 PoE 自动化：

1. 新建干净 Python 项目。
2. 只恢复 `utils`、`l2_perception`、`l3_decision`、`l4_execution`。
3. 数据只保留精简 JSON，不直接带回 `PyPoE-master` / `repoe-master`。
4. 先跑现有测试，再接 UI。
