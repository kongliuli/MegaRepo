# xinlingMain 归档说明

日期：2026-07-02
仓库：`D:\Code\githubDown\xinlingMain`

## 结论

先归档，不删除。

这是杏林相关旧主仓/实验仓，README 很薄，但仓库里有 WPF 医疗表单、模板配置、控件库、文档和压缩附件。因为它可能包含 `xinglin` / `xinglin-core` 未覆盖的历史材料，不能直接进入删除队列。

## 分支情况

| 分支 | 判断 |
| --- | --- |
| `origin/main` | 唯一远端主线，旧项目集合 |

## 内容结构

| 路径 | 价值 |
| --- | --- |
| `Xinglin/Xinglin.sln` | 旧解决方案入口 |
| `Xinglin/XingLinMain` | 主 WPF 应用，动态表单/模板编辑 |
| `Xinglin/MedicalFormApp` | 医疗表单应用，JSON 树形配置编辑 |
| `Xinglin/HandyControlsLibrary` | 旧控件库 |
| `Xinglin/testwpf` | 早期表单/模板实验 |
| `Xinglin/XingLinMain/Configs` | 体检、血检、影像、尿检等模板配置 |
| `Xinglin/XingLinMain/Configs_Backup` | 肝功、肾功、流调等备份模板 |
| `Xinglin/markdownVersion` | 沟通记录版本文档 |
| `RePoeDataCatcher.7z` / `TorrentDownloader.rar` | 非杏林附件，后续可单独判断 |
| `可拖拽的报告单编辑.rar` | 可能和报告单编辑器相关，删除前需确认 |

## 可复用点

- 医疗表单 JSON 配置模型。
- 动态表单渲染和模板编辑思路。
- WPF 控件库和布局容器。
- 报告单/打印模板配置。

## 后续处理

保留为归档仓。后续先对比 `xinglin` / `xinglin-core` 是否已经覆盖表单配置、打印模板和控件库，再决定是否拆附件或写删除前复活说明。

