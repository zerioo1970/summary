# GitLab 完整使用指南 - 从入门到精通（新版）

> **本版改动说明（Changelog）**
> 1. 全部 ASCII 流程图 / 分支模型图 / 结构树已替换为 **Mermaid**（在 GitLab / GitHub 中直接渲染成图）。
> 2. IntelliJ IDEA 的 **对话框 / 界面示意** 已改为 **表格** 或 Mermaid，便于阅读与维护。
> 3. 修正了若干技术错误：IDEA Fetch 快捷键、HTTPS 认证方式、Windows 凭证助手、CI/CD `only/except` 旧语法提示等（正文中以 “✅ 修订” 标注）。
> 4. 兼容性提示：若 Mermaid 渲染器较旧、`gitGraph` 中带 `/` 的分支名报错，把 `/` 换成 `-` 即可。

## 📚 目录
1. [GitLab 简介](#gitlab-简介)
2. [GitLab 安装与配置](#gitlab-安装与配置)
3. [基础操作](#基础操作)
4. [Git 命令详解](#git-命令详解)
5. [分支管理](#分支管理)
6. [协作开发](#协作开发)
7. [高级功能](#高级功能)
8. [CI/CD 持续集成](#cicd-持续集成)
9. [最佳实践](#最佳实践)
10. [常见问题与解决方案](#常见问题与解决方案)
11. [IntelliJ IDEA 中使用 Git 和 GitLab](#intellij-idea-2026-中使用-git-和-gitlab)

---

## GitLab 简介

### 什么是 GitLab？

GitLab 是一个基于 Git 的完整 DevOps 平台，提供：
- 📦 **代码托管** - Git 仓库管理
- 🔄 **版本控制** - 完整的版本历史
- 👥 **协作开发** - Merge Request（MR）、Code Review
- 🚀 **CI/CD** - 自动化构建、测试、部署
- 📊 **项目管理** - Issue、看板、里程碑
- 📈 **监控分析** - 代码质量、性能监控

**GitLab 平台模块关系图：**

```mermaid
flowchart TD
    GL["GitLab DevOps 平台"]
    GL --> Repo["📦 代码托管 / 版本控制"]
    GL --> Collab["👥 协作开发<br/>Merge Request / Code Review"]
    GL --> CI["🚀 CI/CD<br/>构建 / 测试 / 部署"]
    GL --> PM["📊 项目管理<br/>Issue / 看板 / 里程碑"]
    GL --> Mon["📈 监控分析<br/>代码质量 / 性能"]
```

### GitLab vs GitHub vs Bitbucket

| 特性 | GitLab | GitHub | Bitbucket |
|------|--------|--------|-----------|
| **部署方式** | 自托管 + 云端 | 主要云端 | 云端 + 自托管 |
| **CI/CD** | 内置强大 | GitHub Actions | Pipelines |
| **私有仓库** | 免费无限制 | 免费无限制 | 有限制 |
| **代码审查** | Merge Request | Pull Request | Pull Request |
| **集成程度** | 一体化平台 | 需要第三方 | Atlassian 生态 |
| **价格** | 免费版功能多 | 企业版较贵 | 中等 |

---

## GitLab 安装与配置

### 1. 注册 GitLab 账号（云端版）

#### 步骤 1: 访问 GitLab 官网
访问：https://gitlab.com

**配图说明：** GitLab 首页截图，标注注册按钮位置

#### 步骤 2: 填写注册信息
```
姓名: 张三
用户名: zhangsan
邮箱: zhangsan@example.com
密码: ******** (至少8位，包含大小写字母和数字)
```

**配图说明：** 注册表单界面，标注各个字段

#### 步骤 3: 验证邮箱
检查邮箱，点击验证链接激活账号。

**配图说明：** 邮箱验证邮件示例

---

### 2. 安装 Git 客户端

#### Windows 安装

**步骤 1:** 下载 Git for Windows
- 访问：https://git-scm.com/download/win
- 下载最新版本安装包

**配图说明：** Git 官网下载页面

**步骤 2:** 运行安装程序
```
推荐配置：
☑ Git Bash Here (右键菜单)
☑ Git GUI Here
☑ Git LFS (Large File Storage)
编辑器：Visual Studio Code (或您喜欢的编辑器)
PATH 环境：Git from the command line and also from 3rd-party software
HTTPS 传输：Use the OpenSSL library
行尾转换：Checkout Windows-style, commit Unix-style
终端模拟器：Use MinTTY
凭证管理：Git Credential Manager  ✅ 修订（新版安装器默认项，替代旧的 wincred）
```

**配图说明：** Git 安装向导的关键步骤截图（至少3张）

#### macOS 安装
```bash
# 使用 Homebrew 安装
brew install git

# 验证安装
git --version
```

#### Linux 安装
```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get install git

# CentOS/RHEL
sudo yum install git

# 验证安装
git --version
```

---

### 3. 配置 Git

#### 全局配置

打开终端（Windows 用 Git Bash），执行以下命令：

```bash
# 配置用户名
git config --global user.name "张三"

# 配置邮箱（与 GitLab 账号邮箱一致）
git config --global user.email "zhangsan@example.com"

# 配置默认编辑器
git config --global core.editor "code --wait"  # VS Code
# 或
git config --global core.editor "vim"  # Vim

# 配置换行符处理（Windows 推荐）
git config --global core.autocrlf true

# 配置换行符处理（Mac/Linux 推荐）
git config --global core.autocrlf input

# 显示中文文件名
git config --global core.quotepath false

# 查看所有配置
git config --global --list
```

**配图说明：** 终端执行配置命令的截图

#### 配置 SSH 密钥（推荐）

SSH 密钥可以让您无需每次输入密码即可推送代码。

**步骤 1: 生成 SSH 密钥**
```bash
# 生成新的 SSH 密钥对
ssh-keygen -t ed25519 -C "zhangsan@example.com"

# 如果系统不支持 ed25519，使用 RSA
ssh-keygen -t rsa -b 4096 -C "zhangsan@example.com"

# 提示输入保存位置，直接回车使用默认位置
# 提示输入密码，可以留空或设置密码（推荐设置）
```

**配图说明：** SSH 密钥生成过程的终端输出

**步骤 2: 查看公钥**
```bash
# Windows (Git Bash) / Mac / Linux
cat ~/.ssh/id_ed25519.pub

# 输出类似：
# ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIGl... zhangsan@example.com
```

复制整个公钥内容。

**配图说明：** 终端显示公钥的截图

**步骤 3: 添加 SSH 密钥到 GitLab**

1. 登录 GitLab
2. 点击右上角头像 → Settings（设置）
3. 左侧菜单选择 **SSH Keys**
4. 将公钥粘贴到 "Key" 文本框
5. 设置 Title（如：我的笔记本）
6. 设置过期时间（可选）
7. 点击 **Add key** 按钮

**配图说明：** GitLab SSH Keys 设置页面，标注各个步骤

**步骤 4: 测试 SSH 连接**
```bash
ssh -T git@gitlab.com

# 首次连接会提示：
# The authenticity of host 'gitlab.com' can't be established.
# Are you sure you want to continue connecting (yes/no)?
# 输入 yes

# 成功输出：
# Welcome to GitLab, @zhangsan!
```

**配图说明：** SSH 连接测试的终端输出

> ✅ **修订提示（HTTPS 认证）：** GitLab.com 已停用“账号密码”进行 Git 操作。若使用 HTTPS，需要用 **Personal Access Token（PAT）** 代替密码，或使用 SSH。生成 PAT：右上角头像 → Settings → Access Tokens。

---

### 4. 账号安全：2FA 与 Access Token 管理

> 🆕 **新增章节。** 现代 GitLab 的认证已全面 Token 化，账号密码不再用于 Git 操作，理解各类 Token 的区别很重要。

#### 4.1 开启双因素认证（2FA）

强烈建议为账号开启 2FA，尤其是拥有 Maintainer / Owner 权限的成员。

**步骤：**
1. 右上角头像 → **Edit profile → Account**
2. 找到 **Two-Factor Authentication**，点击 **Enable two-factor authentication**
3. 用手机 App（如 Google Authenticator、1Password、Authy）扫描二维码
4. 输入 6 位验证码确认
5. **务必保存恢复码（Recovery codes）**，用于手机丢失时登录

> ⚠️ 开启 2FA 后，命令行 **不能** 再用账号密码进行 HTTPS 操作，必须改用 Personal Access Token 或 SSH。

#### 4.2 各类 Access Token 对比

| Token 类型 | 归属 | 典型用途 | 说明 |
|-----------|------|----------|------|
| **Personal Access Token（PAT）** | 个人账号 | 本地 Git 操作、调用 API、IDE 登录 | 以“个人身份”行动，权限等同该用户；可设 scope 和过期时间 |
| **Project Access Token** | 单个项目 | 项目级 CI 脚本、自动化 | 会创建一个项目专属的机器人用户；权限限于该项目 |
| **Group Access Token** | 群组 | 跨项目的群组级自动化 | 权限覆盖群组下所有项目 |
| **Deploy Token** | 项目/群组 | 只读拉取代码、拉取容器镜像 | 适合 CI/服务器 **只读** 部署，比 PAT 更受限、更安全 |
| **Deploy Key（SSH）** | 项目 | 服务器免密拉取代码 | 一对 SSH 公私钥，可设为只读或读写 |

**常用 PAT scope（权限范围）：**

| scope | 作用 |
|-------|------|
| `api` | 完整 API 读写（权限最大，谨慎授予） |
| `read_api` | 只读 API |
| `read_repository` | 只读拉取代码（clone / pull） |
| `write_repository` | 读写代码（push） |
| `read_registry` / `write_registry` | 读写容器镜像仓库 |

**最小权限原则：** 只给 Token 授予实际需要的 scope，并设置合理的过期时间（如 90 天），到期轮换。

#### 4.3 在 CI 中优先用受限 Token

CI 拉取代码 / 镜像时，优先用 **Deploy Token** 或内置的 `CI_JOB_TOKEN`，而不是把个人 PAT 写进变量：

```bash
# 用 CI_JOB_TOKEN 克隆同实例的其他仓库（无需额外配置）
git clone https://gitlab-ci-token:${CI_JOB_TOKEN}@gitlab.com/group/other-project.git
```

---

## 基础操作

### 1. 创建新项目

#### 在 GitLab 创建仓库

**步骤 1:** 登录 GitLab，点击 **New project** 按钮

**配图说明：** GitLab 首页，标注 New project 按钮位置

**步骤 2:** 选择创建方式
- **Create blank project** - 创建空白项目
- **Create from template** - 使用模板创建
- **Import project** - 导入现有项目

选择 "Create blank project"

**配图说明：** 项目创建方式选择界面

**步骤 3:** 填写项目信息
```
Project name: 我的第一个项目
Project URL: gitlab.com/zhangsan/my-first-project
Project slug: my-first-project (自动生成)
Visibility Level:
  ○ Private (私有，仅团队成员可见)
  ● Internal (内部，所有登录用户可见)
  ○ Public (公开，所有人可见)
☑ Initialize repository with a README
```

点击 **Create project** 按钮

**配图说明：** 项目信息填写界面，标注各个字段

---

### 2. 克隆项目到本地

**步骤 1:** 获取仓库地址

在项目页面，点击右上角蓝色 **Clone** 按钮，复制 SSH 或 HTTPS 地址：
```
SSH: git@gitlab.com:zhangsan/my-first-project.git
HTTPS: https://gitlab.com/zhangsan/my-first-project.git
```

**配图说明：** Clone 按钮和地址复制界面

**步骤 2:** 在本地克隆
```bash
# 使用 SSH（推荐，已配置密钥）
cd ~/Documents  # 进入您想存放项目的目录
git clone git@gitlab.com:zhangsan/my-first-project.git

# 使用 HTTPS（首次需输入用户名 + Personal Access Token）  ✅ 修订
git clone https://gitlab.com/zhangsan/my-first-project.git

# 克隆后进入项目目录
cd my-first-project

# 查看项目内容
ls -la
```

**配图说明：** 终端执行 clone 命令的过程和输出

---

### 3. 基本 Git 工作流

#### 完整工作流程图

```mermaid
flowchart LR
    A["工作区<br/>Working Directory"] -->|git add| B["暂存区<br/>Staging Area"]
    B -->|git commit| C["本地仓库<br/>Local Repository"]
    C -->|git push| D["远程仓库<br/>GitLab"]
    D -->|git fetch / pull| A
```

#### 实践示例：创建并提交文件

**步骤 1: 创建新文件**
```bash
# 创建一个新文件
echo "# 我的第一个项目" > index.html
echo "<h1>Hello GitLab!</h1>" >> index.html

# 或使用编辑器创建
code index.html  # VS Code
```

**步骤 2: 查看状态**
```bash
git status

# 输出：
# On branch main
# Your branch is up to date with 'origin/main'.
#
# Untracked files:
#   (use "git add <file>..." to include in what will be committed)
#         index.html
#
# nothing added to commit but untracked files present
```

**配图说明：** git status 输出，标注 Untracked files

**步骤 3: 添加到暂存区**
```bash
# 添加单个文件
git add index.html

# 或添加所有文件
git add .

# 再次查看状态
git status
```

**配图说明：** git add 后的 status 输出，标注 Changes to be committed

**步骤 4: 提交到本地仓库**
```bash
git commit -m "添加首页文件"

# 输出：
# [main 5f8a9b2] 添加首页文件
#  1 file changed, 2 insertions(+)
#  create mode 100644 index.html
```

**提交信息最佳实践：**
```bash
# 好的提交信息
git commit -m "feat: 添加用户登录功能"
git commit -m "fix: 修复购物车计算错误"
git commit -m "docs: 更新 API 文档"

# 不好的提交信息
git commit -m "更新"
git commit -m "修改bug"
git commit -m "111"
```

**步骤 5: 推送到远程仓库**
```bash
git push origin main

# 或简写（如果已经设置上游分支）
git push
```

**步骤 6: 在 GitLab 查看**

刷新 GitLab 项目页面，您会看到新文件已经出现。

**配图说明：** GitLab 项目文件列表，展示新添加的文件

---

### 4. 文件修改与更新

#### 修改已有文件

```bash
# 修改文件
echo "<p>这是一个段落</p>" >> index.html

# 查看修改内容
git diff
```

**配图说明：** git diff 输出，标注添加的行（绿色+）和删除的行（红色-）

#### 查看修改历史

```bash
# 查看提交历史
git log

# 单行显示
git log --oneline

# 图形化显示分支
git log --oneline --graph --all

# 查看最近 3 条提交
git log -3

# 查看某个文件的历史
git log index.html
```

**配图说明：** git log 的不同输出格式对比


---

## Git 命令详解

### 1. 配置命令

```bash
# 查看配置
git config --list                    # 查看所有配置
git config user.name                 # 查看用户名
git config user.email                # 查看邮箱

# 三个配置级别
git config --system   # 系统级（所有用户）/etc/gitconfig
git config --global   # 用户级（当前用户）~/.gitconfig
git config --local    # 仓库级（当前仓库）.git/config

# 删除配置
git config --global --unset user.name
```

### 2. 仓库初始化与克隆

```bash
# 初始化新仓库
git init                             # 当前目录初始化
git init my-project                  # 创建新目录并初始化

# 克隆仓库
git clone <url>                      # 克隆到当前目录
git clone <url> my-folder            # 克隆到指定目录
git clone -b develop <url>           # 克隆指定分支
git clone --depth 1 <url>            # 浅克隆（只克隆最新提交）
```

### 3. 文件操作命令

```bash
# 查看状态
git status                           # 查看完整状态
git status -s                        # 简短状态
git status -sb                       # 简短状态 + 分支信息

# 添加文件到暂存区
git add file.txt                     # 添加单个文件
git add *.js                         # 添加所有 js 文件
git add .                            # 添加所有文件
git add -A                           # 添加所有文件（包括删除）
git add -p                           # 交互式添加（选择部分修改）

# 移除文件
git rm file.txt                      # 删除文件并暂存
git rm --cached file.txt             # 从 Git 移除但保留文件
git rm -r folder/                    # 递归删除文件夹

# 移动/重命名文件
git mv old.txt new.txt               # 重命名文件
```

### 4. 提交命令

```bash
# 提交
git commit -m "message"              # 提交暂存的文件
git commit -am "message"             # 添加并提交（仅跟踪的文件）
git commit --amend                   # 修改最后一次提交
git commit --amend -m "new message"  # 修改提交信息

# 查看提交历史
git log                              # 详细历史
git log --oneline                    # 单行显示
git log --graph                      # 图形化显示
git log --author="张三"              # 查看某人的提交
git log --since="2026-01-01"         # 查看指定日期后的提交
git log --until="2026-12-31"         # 查看指定日期前的提交
git log -p                           # 显示每次提交的差异
git log --stat                       # 显示提交的统计信息

# 查看某次提交
git show <commit-hash>               # 查看提交详情
git show HEAD                        # 查看最新提交
git show HEAD~1                      # 查看上一次提交
```

### 5. 差异比较命令

```bash
# 查看差异
git diff                             # 工作区 vs 暂存区
git diff --staged                    # 暂存区 vs 本地仓库
git diff HEAD                        # 工作区 vs 本地仓库
git diff <commit1> <commit2>         # 比较两次提交
git diff <branch1> <branch2>         # 比较两个分支
git diff --stat                      # 显示差异统计

# 查看文件差异
git diff file.txt                    # 查看文件的修改
git diff HEAD -- file.txt            # 查看文件与 HEAD 的差异
```

### 6. 撤销命令

```bash
# 撤销工作区修改
git checkout -- file.txt             # 恢复文件到最后提交状态
git restore file.txt                 # 新命令（推荐）

# 撤销暂存区
git reset HEAD file.txt              # 取消暂存
git restore --staged file.txt        # 新命令（推荐）

# 版本回退
git reset --soft HEAD~1              # 回退一个版本，保留修改在暂存区
git reset --mixed HEAD~1             # 回退一个版本，保留修改在工作区（默认）
git reset --hard HEAD~1              # 回退一个版本，丢弃所有修改（危险！）
git reset --hard <commit-hash>       # 回退到指定提交

# 撤销提交（创建新提交）
git revert <commit-hash>             # 撤销指定提交（推荐）
git revert HEAD                      # 撤销最后一次提交
```

> 💡 **补充：`git switch` / `git restore`** 是 Git 2.23+ 引入的新命令，用于替代职责过重的 `git checkout`：
> - 切换分支用 `git switch`，恢复文件用 `git restore`，语义更清晰。

**reset 三种模式的区别：**

```mermaid
flowchart TD
    subgraph HEAD 回退影响范围
        soft["--soft<br/>只移动 HEAD<br/>修改保留在暂存区"]
        mixed["--mixed（默认）<br/>移动 HEAD + 清空暂存区<br/>修改保留在工作区"]
        hard["--hard<br/>移动 HEAD + 清空暂存区 + 工作区<br/>⚠️ 丢弃所有修改"]
    end
```

### 7. 远程仓库命令

```bash
# 查看远程仓库
git remote                           # 查看远程仓库名称
git remote -v                        # 查看远程仓库地址
git remote show origin               # 查看远程仓库详细信息

# 添加远程仓库
git remote add origin <url>          # 添加远程仓库
git remote add upstream <url>        # 添加上游仓库

# 修改远程仓库
git remote rename origin new-origin  # 重命名
git remote set-url origin <new-url>  # 修改地址
git remote remove origin             # 删除远程仓库

# 拉取和推送
git fetch origin                     # 从远程获取（不合并）
git pull origin main                 # 拉取并合并
git pull --rebase origin main        # 拉取并变基
git push origin main                 # 推送到远程
git push -u origin main              # 推送并设置上游分支
git push --force-with-lease          # 更安全的强制推送（推荐替代 --force）  ✅ 修订
git push --all                       # 推送所有分支
git push --tags                      # 推送所有标签
```

---

## 分支管理

### 1. 分支概念

分支是 Git 最强大的特性之一，允许您并行开发多个功能。

**分支模型图：**

```mermaid
flowchart TD
    main["main（生产分支）"]
    develop["develop（开发分支）"]
    f1["feature/login（功能分支）"]
    f2["feature/payment（功能分支）"]
    b1["bugfix/cart-error（修复分支）"]
    hotfix["hotfix/security-patch（紧急修复）"]

    main --> develop
    main --> hotfix
    develop --> f1
    develop --> f2
    develop --> b1
```

### 2. 分支基本操作

```bash
# 查看分支
git branch                           # 查看本地分支
git branch -r                        # 查看远程分支
git branch -a                        # 查看所有分支
git branch -v                        # 查看分支及最后提交

# 创建分支
git branch feature/login             # 创建分支
git checkout -b feature/login        # 创建并切换分支
git switch -c feature/login          # 新命令（推荐）

# 切换分支
git checkout main                    # 切换到 main 分支
git switch main                      # 新命令（推荐）

# 删除分支
git branch -d feature/login          # 删除已合并的分支
git branch -D feature/login          # 强制删除分支
git push origin --delete feature/login  # 删除远程分支

# 重命名分支
git branch -m old-name new-name      # 重命名分支
```

### 3. 分支合并

#### Fast-forward 合并（快进合并）

```bash
# 场景：main 分支没有新提交
git checkout main
git merge feature/login

# 输出：
# Updating 3d4e5f6..7a8b9c0
# Fast-forward
#  login.html | 20 ++++++++++++++++++++
#  1 file changed, 20 insertions(+)
```

#### 三方合并（Three-way merge）

```bash
# 场景：main 分支有新提交
git checkout main
git merge feature/payment

# 会打开编辑器输入合并提交信息，或直接指定
git merge feature/payment -m "合并支付功能"
```

#### 解决冲突

当同一文件的同一位置被不同分支修改时，会产生冲突：

```bash
git merge feature/login

# 输出：
# Auto-merging index.html
# CONFLICT (content): Merge conflict in index.html
# Automatic merge failed; fix conflicts and then commit the result.

# 查看冲突文件
git status
```

冲突文件内容：
```
<<<<<<< HEAD
<h1>欢迎来到我的网站</h1>
=======
<h1>Welcome to My Site</h1>
>>>>>>> feature/login
```

**步骤 1: 编辑冲突文件**
```html
<!-- 手动选择保留哪个版本，或合并两者 -->
<h1>欢迎来到我的网站 - Welcome to My Site</h1>
```

**步骤 2: 标记为已解决**
```bash
git add index.html
```

**步骤 3: 完成合并**
```bash
git commit -m "解决合并冲突"
```

### 4. 变基（Rebase）

Rebase 可以让提交历史更清晰，但会改写历史。

```bash
# 基本变基
git checkout feature/login
git rebase main

# 交互式变基（整理提交历史）
git rebase -i HEAD~3

# 交互式界面：
# pick 1a2b3c4 提交1
# squash 2b3c4d5 提交2  # 合并到上一个提交
# reword 3c4d5e6 提交3  # 修改提交信息
# drop 4d5e6f7 提交4    # 删除提交

# 解决冲突后
git add .
git rebase --continue

# 放弃变基
git rebase --abort
```

**Merge vs Rebase 对比：**

```mermaid
flowchart LR
    subgraph Merge["Merge（保留分叉历史）"]
        m1["main"] --> mm["合并提交<br/>merge commit"]
        f["feature"] --> mm
    end
    subgraph Rebase["Rebase（线性历史）"]
        r1["main"] --> r2["feature 的提交<br/>被重放到 main 之上"]
    end
```

### 5. GitLab 分支保护

在 GitLab 项目中设置分支保护规则：

**步骤：**
1. 进入项目 → Settings → Repository
2. 展开 "Protected branches"
3. 选择要保护的分支（如 main）
4. 设置规则：
   - Allowed to merge: Maintainers
   - Allowed to push: No one
   - Allowed to force push: ☐ 禁用

**配图说明：** GitLab 分支保护设置界面


---

## 协作开发

### 1. Fork 工作流

Fork 工作流适合开源项目或大型团队。

**流程图：**

```mermaid
flowchart TD
    A["原始仓库 upstream"] -->|Fork| B["您的仓库 origin"]
    B -->|clone| C["本地仓库 local"]
    C -->|push| B
    B -.->|Merge Request| A
    A -.->|fetch upstream（保持同步）| C
```

#### 步骤 1: Fork 项目

在 GitLab 项目页面点击右上角 **Fork** 按钮。

**配图说明：** Fork 按钮位置和 Fork 对话框

#### 步骤 2: 克隆您的 Fork

```bash
git clone git@gitlab.com:zhangsan/forked-project.git
cd forked-project
```

#### 步骤 3: 添加上游仓库

```bash
git remote add upstream git@gitlab.com:original-owner/project.git
git remote -v

# 输出：
# origin    git@gitlab.com:zhangsan/forked-project.git (fetch)
# origin    git@gitlab.com:zhangsan/forked-project.git (push)
# upstream  git@gitlab.com:original-owner/project.git (fetch)
# upstream  git@gitlab.com:original-owner/project.git (push)
```

#### 步骤 4: 保持同步

```bash
# 获取上游更新
git fetch upstream

# 合并到本地
git checkout main
git merge upstream/main

# 推送到您的 Fork
git push origin main
```

### 2. Merge Request（合并请求）

Merge Request（MR）是 GitLab 的代码审查和协作核心功能。

#### 创建 Merge Request

**步骤 1: 创建功能分支并推送**
```bash
git checkout -b feature/new-dashboard
# ... 开发功能 ...
git add .
git commit -m "feat: 添加新的仪表板"
git push -u origin feature/new-dashboard
```

**步骤 2: 在 GitLab 创建 MR**

方式 1: 推送后自动提示
```bash
# 推送后，GitLab 会显示创建 MR 的链接
# remote:
# remote: To create a merge request for feature/new-dashboard, visit:
# remote:   https://gitlab.com/zhangsan/project/-/merge_requests/new?...
```

方式 2: 手动创建
1. 进入项目页面
2. 点击左侧 **Merge requests**
3. 点击 **New merge request**
4. 选择源分支和目标分支
5. 点击 **Compare branches and continue**

**步骤 3: 填写 MR 信息**

```markdown
标题: feat: 添加新的仪表板功能

描述:
## 功能说明
- 添加了用户仪表板页面
- 实现了数据可视化图表
- 支持自定义布局

## 测试
- [x] 单元测试通过
- [x] 集成测试通过
- [x] 手动测试完成

## 截图
![仪表板预览](screenshots/dashboard.png)

## 相关 Issue
Closes #123

审查者: @lisi
标签: feature, frontend
里程碑: v2.0
```

#### MR 最佳实践

**MR 标题规范：**
```
feat: 添加新功能
fix: 修复问题
docs: 文档更新
style: 代码格式调整
refactor: 代码重构
test: 测试相关
chore: 构建/工具配置
perf: 性能优化
```

**MR 大小建议：**
- ✅ 小型 MR: 100-300 行改动（最佳）
- ⚠️ 中型 MR: 300-500 行改动
- ❌ 大型 MR: 500+ 行改动（应拆分）

### 3. 代码审查（Code Review）

#### 审查者职责

**步骤 1: 接收审查通知** — 被指定为审查者时会收到邮件和 GitLab 通知。

**步骤 2: 审查代码**
1. 进入 MR 页面
2. 点击 **Changes** 标签查看修改
3. 在代码行上点击 + 按钮添加评论

**审查要点：**
```
✓ 代码功能
  - 是否实现了需求？
  - 是否有明显的 bug？
  - 边界情况是否考虑？

✓ 代码质量
  - 命名是否清晰？
  - 逻辑是否简洁？
  - 是否有重复代码？
  - 是否遵循团队规范？

✓ 测试
  - 是否有单元测试？
  - 测试覆盖率如何？
  - 是否有集成测试？

✓ 文档
  - 注释是否充分？
  - 文档是否更新？
  - API 文档是否完整？

✓ 安全
  - 是否有安全漏洞？
  - 输入是否验证？
  - 敏感数据是否加密？
```

**步骤 3: 提交审查意见**
```markdown
# 一般性评论
总体来说代码质量不错！有几个小建议：

# 代码行评论
第 45 行：建议使用 const 而不是 let
第 78 行：这个函数可以提取出来复用
第 102 行：需要添加错误处理

# 审查结论
- ✅ Approve（批准）
- 💬 Comment（评论）
- ❌ Request changes（请求修改）
```

### 4. MR 工作流程

#### 完整流程图

```mermaid
sequenceDiagram
    participant D as 开发者
    participant G as GitLab
    participant R as 审查者
    D->>D: 创建分支 / 提交代码
    D->>G: 推送到远程
    D->>G: 创建 MR
    G->>R: 通知审查
    R->>R: 审查代码 / 提出意见
    R-->>D: 请求修改
    D->>D: 修改代码
    D->>G: 推送更新 / 回复评论
    R->>R: 再次审查
    R->>D: 批准 MR
    D->>G: 合并到主分支
    D->>D: 删除功能分支 / 部署
```

#### MR 配置选项

在 MR 页面，可以配置以下选项：

| 选项 | 说明 | 推荐 |
|------|------|------|
| Delete source branch when merge request is accepted | 合并后删除源分支 | ☑ 推荐 |
| Squash commits when merge request is accepted | 合并时压缩提交，保持历史清晰 | ☑ 推荐 |
| Allow commits from members who can merge to the target branch | 允许其他成员推送到此分支 | 视需要 |

**Merge 策略：**
- ● Merge commit（创建合并提交）
- ○ Merge when pipeline succeeds（流水线成功后合并）
- ○ Rebase（变基合并）

### 5. 团队协作最佳实践

#### 分支命名规范

```bash
# 功能分支
feature/user-authentication
feature/payment-gateway
feature/dashboard-redesign

# 修复分支
bugfix/login-error
bugfix/cart-calculation
hotfix/security-vulnerability

# 发布分支
release/v1.2.0
release/v2.0.0-beta

# 个人分支（探索性工作）
zhangsan/experiment-new-ui
lisi/poc-redis-cache
```

#### 提交信息规范（Conventional Commits）

```bash
# 格式
<type>(<scope>): <subject>

<body>

<footer>

# 示例
feat(auth): 添加 OAuth 2.0 登录支持

实现了 GitHub 和 Google 的第三方登录功能。
用户可以选择使用社交账号快速登录。

Closes #234
BREAKING CHANGE: 旧的登录 API 已废弃

# Type 类型
feat:     新功能
fix:      修复 bug
docs:     文档更新
style:    代码格式（不影响代码运行）
refactor: 重构
test:     测试
chore:    构建过程或辅助工具变动
perf:     性能优化
ci:       CI 配置修改
build:    构建系统修改
revert:   回滚提交
```

### 6. GitLab 合并策略与 Merge Trains

> 🆕 **新增章节。** 除了上面 MR 页面的三种基础合并方式，GitLab 在 **Settings → Merge requests** 里还提供项目级的合并方式，直接影响主分支的历史形态。

#### 6.1 三种项目级合并方式

| 合并方式 | 主分支历史 | 说明 |
|----------|-----------|------|
| **Merge commit** | 保留分叉 + 合并提交 | 默认方式，完整保留分支拓扑 |
| **Merge commit with semi-linear history** | 半线性 | 要求源分支先 rebase 到最新目标分支，再创建合并提交；历史清晰又保留合并点 |
| **Fast-forward merge** | 完全线性 | 不产生合并提交；要求源分支必须能快进，否则需先 rebase |

**搭配选项：**
- **Squash commits**：把 MR 里的多个提交压成一个再合入，主分支更干净。可设为 Require（强制）/ Encourage（默认勾选）等。
- **Pipelines must succeed**：流水线通过才允许合并。
- **All threads must be resolved**：所有评论线程解决后才允许合并。

#### 6.2 Merge Trains（合并列车）

> 适用场景：多个 MR 频繁并发合入同一分支时，避免“各自基于旧代码通过 CI、合并后却相互破坏”的问题。（Premium/Ultimate 特性）

**原理：** 当多个 MR 排队合并时，GitLab 把它们排成一列“列车”，**依次把每个 MR 叠加在前一个的预期结果之上运行流水线**，只有全部通过才真正合入。这样能保证合入后的主分支一定是绿色的。

```mermaid
flowchart LR
    A["MR #1<br/>基于 main"] --> B["MR #2<br/>基于 (main + #1)"]
    B --> C["MR #3<br/>基于 (main + #1 + #2)"]
    C --> D["全部流水线通过<br/>→ 依次合入 main"]
```

**启用：** Settings → Merge requests → 勾选 **Enable merged results pipelines** 与 **Enable merge trains**。

---

## 高级功能

### 1. Issues（问题追踪）

Issues 用于跟踪功能需求、bug、任务等。

#### 创建 Issue

**步骤：**
1. 进入项目 → Issues
2. 点击 **New issue**
3. 填写 Issue 信息

**Issue 模板示例：**
```markdown
## Bug 报告

**描述**
登录页面在 Safari 浏览器中无法正常显示

**重现步骤**
1. 打开 Safari 浏览器
2. 访问登录页面
3. 观察布局问题

**期望行为**
登录表单应该居中显示

**实际行为**
表单偏移到左侧

**环境**
- 浏览器: Safari 16.1
- 操作系统: macOS 13.0
- 应用版本: v1.2.3

**截图**
![问题截图](screenshots/bug.png)

**优先级**
- [x] High
- [ ] Medium
- [ ] Low

**标签**
bug, frontend, safari
```

#### Issue 管理功能

| 功能 | 说明 |
|------|------|
| 标签（Labels） | `bug`、`feature`、`documentation`、`enhancement`；优先级 P0-P3；状态 todo/in-progress/review/done |
| 里程碑（Milestones） | v1.0.0、v2.0.0、Sprint 23 |
| 指派人（Assignees） | 可指派多人 |
| 截止日期（Due date） | 2026-12-31 |
| 时间跟踪（Time tracking） | `/estimate 2h` 预估时间；`/spend 1h 30m` 实际花费 |
| 关联 MR | `Closes #123` 合并后自动关闭；`Related to #456` 相关 Issue |

### 2. 看板（Board）

看板视图帮助团队可视化工作流程。

**步骤：**
1. 进入项目 → Issues → Boards
2. 创建新看板或使用默认看板
3. 添加列表（如：To Do, Doing, Review, Done）

**看板列表类型：** Open / Closed / Label / Assignee / Milestone。

**典型看板布局：**

```mermaid
flowchart LR
    subgraph Backlog["Backlog"]
        i1["Issue #1"]
        i2["Issue #2"]
    end
    subgraph ToDo["To Do"]
        i3["Issue #3"]
        i4["Issue #4"]
    end
    subgraph Doing["Doing"]
        i5["Issue #5"]
        i6["Issue #6"]
    end
    subgraph Done["Done"]
        i7["Issue #7"]
        i8["Issue #8"]
    end
    Backlog --> ToDo --> Doing --> Done
```

> 补充：Mermaid v11.3+ 已支持原生 `kanban` 图类型，渲染环境支持时可改用以获得更贴近看板的效果。

### 3. Wiki（项目文档）

Wiki 用于编写项目文档、指南、规范等。

**步骤：**
1. 进入项目 → Wiki
2. 点击 **Create your first page**
3. 使用 Markdown 编写文档

**Wiki 文档结构示例：**

```mermaid
flowchart TD
    Home["Home"]
    Home --> GS["Getting Started"]
    Home --> Dev["Development"]
    Home --> API["API Documentation"]
    Home --> Dep["Deployment"]

    GS --> Inst["Installation"]
    GS --> QS["Quick Start"]

    Dev --> CS["Code Style Guide"]
    Dev --> GW["Git Workflow"]
    Dev --> TG["Testing Guide"]

    API --> REST["REST API"]
    API --> GQL["GraphQL API"]

    Dep --> DE["Development Environment"]
    Dep --> SE["Staging Environment"]
    Dep --> PE["Production Environment"]
```

### 4. Snippets（代码片段）

Snippets 用于共享代码片段、配置文件等。

**步骤：**
1. 点击右上角 + → New snippet
2. 填写标题和描述
3. 添加文件内容
4. 设置可见性

**使用场景：** Docker Compose 配置、Nginx 配置模板、Git Hooks 脚本、常用工具函数、SQL 查询示例等。

### 5. 标签（Tags）和发布（Releases）

#### 创建标签

```bash
# 轻量标签
git tag v1.0.0

# 附注标签（推荐）
git tag -a v1.0.0 -m "发布版本 1.0.0"

# 为历史提交打标签
git tag -a v0.9.0 9fceb02 -m "版本 0.9.0"

# 查看标签
git tag
git tag -l "v1.*"

# 查看标签详情
git show v1.0.0

# 推送标签
git push origin v1.0.0
git push origin --tags

# 删除标签
git tag -d v1.0.0
git push origin --delete v1.0.0
```

#### 在 GitLab 创建 Release

**步骤：**
1. 进入项目 → Deployments → Releases
2. 点击 **New release**
3. 选择标签或创建新标签
4. 填写发布信息

**Release 信息示例：**
```markdown
# v1.0.0 - 重大版本发布

## 🎉 新功能
- 用户认证系统
- 支付网关集成
- 实时通知

## 🐛 Bug 修复
- 修复购物车计算错误
- 修复移动端显示问题

## 📝 文档
- 更新 API 文档
- 添加部署指南

## 💥 破坏性更改
- API v1 已废弃，请迁移到 v2

## 📦 资产
- application-v1.0.0.zip
- database-migration.sql
```

---

## CI/CD 持续集成

### 1. GitLab CI/CD 简介

GitLab CI/CD 是内置的持续集成和部署工具。

**CI/CD 流程图：**

```mermaid
flowchart LR
    A[代码提交] --> B[自动测试]
    B --> C[构建]
    C --> D[部署到测试环境]
    D --> E{手动审批}
    E -->|通过| F[部署到生产环境]
```

> ✅ **修订提示（现代 CI 语法）：** 下文示例中的 `only:` / `except:` 属于 GitLab **旧语法（legacy）**，仍可运行但官方已推荐改用 `rules:`。示例：
> ```yaml
> deploy_prod:
>   rules:
>     - if: '$CI_COMMIT_TAG'
>       when: manual
> ```
> 现代流水线还常用 `workflow:`（控制流水线是否创建）、`needs:`（DAG 并行）、`include:`（复用模板 / CI Components）。

### 2. .gitlab-ci.yml 配置文件

在项目根目录创建 `.gitlab-ci.yml` 文件。

#### 基础示例

```yaml
# .gitlab-ci.yml

# 定义阶段
stages:
  - test
  - build
  - deploy

# 全局变量
variables:
  NODE_VERSION: "18"
  APP_NAME: "my-app"

# 测试任务
test_job:
  stage: test
  image: node:18
  script:
    - npm install
    - npm run test
  artifacts:
    reports:
      coverage_report:
        coverage_format: cobertura
        path: coverage/cobertura-coverage.xml

# 构建任务
build_job:
  stage: build
  image: node:18
  script:
    - npm install
    - npm run build
  artifacts:
    paths:
      - dist/
    expire_in: 1 week
  rules:                       # ✅ 修订：由 only 改为 rules
    - if: '$CI_COMMIT_BRANCH == "main" || $CI_COMMIT_BRANCH == "develop"'

# 部署到测试环境
deploy_staging:
  stage: deploy
  script:
    - echo "部署到测试环境"
    - scp -r dist/* user@staging-server:/var/www/app/
  environment:
    name: staging
    url: https://staging.example.com
  rules:
    - if: '$CI_COMMIT_BRANCH == "develop"'

# 部署到生产环境
deploy_production:
  stage: deploy
  script:
    - echo "部署到生产环境"
    - scp -r dist/* user@prod-server:/var/www/app/
  environment:
    name: production
    url: https://example.com
  rules:
    - if: '$CI_COMMIT_BRANCH == "main"'
      when: manual            # 需要手动触发
```

#### Node.js 项目完整示例

```yaml
# Node.js 应用 CI/CD 配置
image: node:18

stages:
  - install
  - lint
  - test
  - build
  - deploy

# 缓存依赖
cache:
  key: ${CI_COMMIT_REF_SLUG}
  paths:
    - node_modules/
    - .npm/

install_dependencies:
  stage: install
  script:
    - npm ci --cache .npm --prefer-offline
  artifacts:
    paths:
      - node_modules/
    expire_in: 1 day

lint_code:
  stage: lint
  script:
    - npm run lint
    - npm run format:check
  needs: ["install_dependencies"]   # ✅ 修订：dependencies → needs（DAG）

unit_tests:
  stage: test
  script:
    - npm run test:unit
  coverage: '/Lines\s*:\s*(\d+\.\d+)%/'
  artifacts:
    reports:
      junit: junit.xml
      coverage_report:
        coverage_format: cobertura
        path: coverage/cobertura-coverage.xml
  needs: ["install_dependencies"]

integration_tests:
  stage: test
  services:
    - postgres:14
    - redis:7
  variables:
    POSTGRES_DB: test_db
    POSTGRES_USER: test_user
    POSTGRES_PASSWORD: test_password
    REDIS_URL: redis://redis:6379
  script:
    - npm run test:integration
  needs: ["install_dependencies"]

build_app:
  stage: build
  script:
    - npm run build
  artifacts:
    paths:
      - dist/
      - package.json
      - package-lock.json
    expire_in: 1 week
  needs: ["install_dependencies"]
  rules:
    - if: '$CI_COMMIT_BRANCH == "main" || $CI_COMMIT_BRANCH == "develop"'
    - if: '$CI_COMMIT_BRANCH =~ /^release\/.*$/'

build_docker:
  stage: build
  image: docker:24
  services:
    - docker:24-dind
  variables:
    DOCKER_DRIVER: overlay2
    DOCKER_TLS_CERTDIR: "/certs"
  before_script:
    - docker login -u $CI_REGISTRY_USER -p $CI_REGISTRY_PASSWORD $CI_REGISTRY
  script:
    - docker build -t $CI_REGISTRY_IMAGE:$CI_COMMIT_SHORT_SHA .
    - docker tag $CI_REGISTRY_IMAGE:$CI_COMMIT_SHORT_SHA $CI_REGISTRY_IMAGE:latest
    - docker push $CI_REGISTRY_IMAGE:$CI_COMMIT_SHORT_SHA
    - docker push $CI_REGISTRY_IMAGE:latest
  rules:
    - if: '$CI_COMMIT_BRANCH == "main"'
```

#### Python 项目示例

```yaml
# Python 应用 CI/CD 配置
image: python:3.11

stages:
  - test
  - build
  - deploy

variables:
  PIP_CACHE_DIR: "$CI_PROJECT_DIR/.cache/pip"

cache:
  paths:
    - .cache/pip
    - venv/

before_script:
  - python -V
  - pip install virtualenv
  - virtualenv venv
  - source venv/bin/activate
  - pip install -r requirements.txt

code_quality:
  stage: test
  script:
    - pip install flake8 black mypy
    - flake8 src/
    - black --check src/
    - mypy src/
  allow_failure: true

run_tests:
  stage: test
  script:
    - pip install pytest pytest-cov
    - pytest --cov=src tests/
  coverage: '/TOTAL.*\s+(\d+%)$/'
  artifacts:
    reports:
      junit: report.xml
      coverage_report:
        coverage_format: cobertura
        path: coverage.xml

build_docker:
  stage: build
  image: docker:24
  services:
    - docker:24-dind
  script:
    - docker build -t $CI_REGISTRY_IMAGE:$CI_COMMIT_TAG .
    - docker push $CI_REGISTRY_IMAGE:$CI_COMMIT_TAG
  rules:
    - if: '$CI_COMMIT_TAG'
```

### 3. CI/CD 关键概念

#### Pipeline（流水线）

流水线是一组按顺序执行的任务。

```mermaid
flowchart TD
    P["Pipeline #12345"]
    P --> test
    test --> build
    build --> deploy

    subgraph test["Stage: test（2 分钟）"]
        t1["lint_code ✓"]
        t2["unit_tests ✓"]
    end
    subgraph build["Stage: build（5 分钟）"]
        b1["build_app ✓"]
    end
    subgraph deploy["Stage: deploy（3 分钟）"]
        d1["deploy_staging ⏸️（手动）"]
    end
```

#### Job（任务）

Job 是 Pipeline 中的单个执行单元。

```yaml
job_name:
  stage: test
  image: node:18
  services:
    - postgres:14
  variables:
    NODE_ENV: test
  before_script:
    - npm install
  script:
    - npm test
  after_script:
    - echo "清理工作"
  artifacts:
    paths:
      - coverage/
  cache:
    paths:
      - node_modules/
  rules:                        # ✅ 修订：由 only/except 改为 rules
    - if: '$CI_COMMIT_BRANCH == "main"'
  when: on_success              # always, on_success, on_failure, manual
  allow_failure: false
  retry: 2
  timeout: 1h
  tags:
    - docker
    - linux
```

#### Runners（执行器）

Runners 是执行 CI/CD 任务的服务器。

| 类型 | 说明 |
|------|------|
| Shared Runners | GitLab 提供的共享执行器 |
| Group Runners | 群组级别的执行器 |
| Specific Runners | 项目专用执行器 |

#### 安装自己的 Runner

```bash
# Linux 安装
sudo curl -L --output /usr/local/bin/gitlab-runner https://gitlab-runner-downloads.s3.amazonaws.com/latest/binaries/gitlab-runner-linux-amd64
sudo chmod +x /usr/local/bin/gitlab-runner
sudo useradd --comment 'GitLab Runner' --create-home gitlab-runner --shell /bin/bash
sudo gitlab-runner install --user=gitlab-runner --working-directory=/home/gitlab-runner
sudo gitlab-runner start

# 注册 Runner
sudo gitlab-runner register
# URL: https://gitlab.com/
# Token: 从项目 Settings > CI/CD > Runners 获取
# Description: my-runner
# Tags: docker,linux
# Executor: docker
# Default image: alpine:latest

# 查看状态
sudo gitlab-runner status

# 验证
sudo gitlab-runner verify
```

### 4. 环境变量和密钥

**步骤：** 进入项目 → Settings → CI/CD → 展开 "Variables" → "Add variable"。

| 变量类型 | 说明 |
|----------|------|
| Variable | 普通文本变量，可在 CI/CD 脚本中直接使用 |
| File | 将变量值写入临时文件，适合存储证书、密钥文件等 |

**变量选项：**

| 选项 | 作用 |
|------|------|
| Protected | 仅在受保护的分支/标签上可用 |
| Masked | 在日志中隐藏变量值 |
| Expand variable reference | 展开变量引用 |

```yaml
deploy_job:
  script:
    - echo "API Key: $API_KEY"
    - echo "Database URL: $DATABASE_URL"
    - scp -i $SSH_KEY_FILE app.tar.gz user@server:/app/
```

#### 预定义变量

```bash
$CI_COMMIT_SHA          # 完整的 commit SHA
$CI_COMMIT_SHORT_SHA    # 短 commit SHA
$CI_COMMIT_REF_NAME     # 分支或标签名
$CI_COMMIT_MESSAGE      # 提交信息
$CI_COMMIT_AUTHOR       # 提交作者
$CI_PIPELINE_ID         # Pipeline ID
$CI_JOB_ID              # Job ID
$CI_JOB_NAME            # Job 名称
$CI_PROJECT_NAME        # 项目名称
$CI_PROJECT_PATH        # 项目路径
$CI_REGISTRY            # 容器镜像仓库地址
$CI_REGISTRY_IMAGE      # 项目的镜像地址
```

### 5. 实际应用场景

#### 场景 1: 自动化测试和代码质量检查

```yaml
stages:
  - quality
  - test

eslint:
  stage: quality
  script:
    - npm run lint
  allow_failure: false

prettier:
  stage: quality
  script:
    - npm run format:check
  allow_failure: true

unit_test:
  stage: test
  script:
    - npm run test:unit
  coverage: '/Lines\s*:\s*(\d+\.\d+)%/'

e2e_test:
  stage: test
  script:
    - npm run test:e2e
  artifacts:
    when: on_failure
    paths:
      - cypress/screenshots/
      - cypress/videos/
```

#### 场景 2: 多环境部署

```yaml
deploy_dev:
  stage: deploy
  script:
    - ./deploy.sh dev
  environment:
    name: dev
    url: https://dev.example.com
  rules:
    - if: '$CI_COMMIT_BRANCH == "develop"'

deploy_staging:
  stage: deploy
  script:
    - ./deploy.sh staging
  environment:
    name: staging
    url: https://staging.example.com
  rules:
    - if: '$CI_COMMIT_BRANCH == "main"'

deploy_production:
  stage: deploy
  script:
    - ./deploy.sh production
  environment:
    name: production
    url: https://example.com
  rules:
    - if: '$CI_COMMIT_TAG'
      when: manual
```

#### 场景 3: 依赖更新检查（定时任务）

```yaml
check_dependencies:
  image: node:18
  script:
    - npm outdated || true
    - npm audit
  rules:
    - if: '$CI_PIPELINE_SOURCE == "schedule"'
  allow_failure: true
```

**设置定时任务：** 进入项目 → CI/CD → Schedules → New schedule，设置 Cron 表达式（如 `0 9 * * 1`）。

### 6. 现代 .gitlab-ci.yml 语法（推荐）

> 🆕 **新增章节。** 旧的 `only` / `except` 已被官方标记为 legacy，现代流水线应改用下面这些关键字。

#### 6.1 `rules:` —— 替代 `only/except`

`rules:` 按顺序匹配，命中即决定该 Job 是否创建以及 `when` 行为，表达力远强于 `only/except`：

```yaml
deploy_prod:
  script: ./deploy.sh production
  rules:
    - if: '$CI_COMMIT_TAG'          # 打 tag 时
      when: manual                   # 手动触发
    - if: '$CI_COMMIT_BRANCH == "main"'
      changes:                       # 且指定文件有改动时才跑
        - src/**/*
    - when: never                    # 其余情况不创建该 Job
```

#### 6.2 `workflow:` —— 控制“整条流水线”是否创建

避免重复流水线（如 push 和 MR 各触发一次）：

```yaml
workflow:
  rules:
    - if: '$CI_PIPELINE_SOURCE == "merge_request_event"'   # MR 流水线
    - if: '$CI_COMMIT_BRANCH == "main"'                     # main 分支
    - when: never                                           # 其余不创建
```

#### 6.3 `needs:` —— DAG 并行流水线

让 Job 不必死等整个上一个 stage，只依赖它真正需要的 Job，缩短流水线时间：

```yaml
build_frontend:
  stage: build
  script: npm run build:web

build_backend:
  stage: build
  script: npm run build:api

deploy:
  stage: deploy
  needs: ["build_frontend", "build_backend"]   # 两者一好就开始，不等其他 build
  script: ./deploy.sh
```

#### 6.4 `include:` —— 复用模板 / CI Components

把公共配置抽出来复用，避免每个项目重复维护：

```yaml
include:
  - local: '/ci/templates/test.yml'                       # 本仓库文件
  - project: 'my-group/ci-templates'                      # 其他项目
    ref: main
    file: '/deploy.yml'
  - template: 'Security/SAST.gitlab-ci.yml'               # GitLab 官方模板
  - component: 'gitlab.com/my-group/my-component@1.0.0'   # CI/CD Component（新一代复用方式）
```

### 7. 安全扫描（DevSecOps）

> 🆕 **新增章节。** GitLab 内置多种安全扫描模板，通过 `include` 一行即可接入，扫描结果会展示在 MR 的 **Security** 面板。（部分特性需 Ultimate 版）

| 扫描类型 | 作用 | 官方模板 |
|----------|------|----------|
| **SAST**（静态应用安全测试） | 扫描源码中的安全漏洞 | `Security/SAST.gitlab-ci.yml` |
| **Dependency Scanning** | 扫描第三方依赖的已知漏洞（CVE） | `Security/Dependency-Scanning.gitlab-ci.yml` |
| **Secret Detection** | 检测误提交的密钥 / Token / 密码 | `Security/Secret-Detection.gitlab-ci.yml` |
| **Container Scanning** | 扫描构建出的容器镜像 | `Security/Container-Scanning.gitlab-ci.yml` |
| **DAST**（动态应用安全测试） | 对运行中的应用做黑盒扫描 | `DAST.gitlab-ci.yml` |

**接入示例：**
```yaml
include:
  - template: Security/SAST.gitlab-ci.yml
  - template: Security/Secret-Detection.gitlab-ci.yml
  - template: Security/Dependency-Scanning.gitlab-ci.yml

stages:
  - test
  - build
  - deploy
```

> 💡 Secret Detection 属于免费可用范围，是性价比很高的一道防线；扫描发现的问题会在 MR 中以安全告警形式提示，避免密钥泄露进历史。

---

## 最佳实践

### 1. 分支策略

#### Git Flow

```mermaid
gitGraph
    commit id: "init"
    branch develop
    checkout develop
    commit
    branch "feature/xxx"
    checkout "feature/xxx"
    commit
    commit
    checkout develop
    merge "feature/xxx"
    branch "release/v1.0"
    checkout "release/v1.0"
    commit id: "bugfix & 版本号"
    checkout main
    merge "release/v1.0" tag: "v1.0.0"
    checkout develop
    merge "release/v1.0"
    checkout main
    branch "hotfix/xxx"
    checkout "hotfix/xxx"
    commit
    checkout main
    merge "hotfix/xxx" tag: "v1.0.1"
    checkout develop
    merge "hotfix/xxx"
```

**实施：**
```bash
# 开始新功能
git checkout develop
git checkout -b feature/user-profile

# 完成功能
git checkout develop
git merge --no-ff feature/user-profile
git branch -d feature/user-profile
git push origin develop

# 创建发布分支
git checkout -b release/1.0.0 develop
# 修复 bug，更新版本号

# 完成发布
git checkout main
git merge --no-ff release/1.0.0
git tag -a v1.0.0
git checkout develop
git merge --no-ff release/1.0.0
git branch -d release/1.0.0
```

#### GitHub Flow（简化版）

```mermaid
gitGraph
    commit
    branch "feature/xxx"
    checkout "feature/xxx"
    commit
    commit
    checkout main
    merge "feature/xxx" tag: "自动部署"
    branch "bugfix/yyy"
    checkout "bugfix/yyy"
    commit
    checkout main
    merge "bugfix/yyy" tag: "自动部署"
```

**适合：** 持续部署、快速迭代、小型团队。

### 2. Commit 最佳实践

```bash
# ✅ 好的提交
git commit -m "feat(auth): 添加 JWT 认证中间件"
git commit -m "fix(cart): 修复商品数量计算错误"
git commit -m "docs: 更新 API 文档和使用示例"

# ❌ 不好的提交
git commit -m "修改"
git commit -m "fix bug"
git commit -m "aaa"
git commit -m "临时提交，稍后修改"
```

**提交原则：**
- 🎯 **原子性**: 一个提交只做一件事
- 📝 **清晰性**: 提交信息准确描述修改
- 🔍 **可追溯**: 关联 Issue 或任务号
- ⚡ **频繁提交**: 完成一个小功能就提交

### 3. .gitignore 配置

```gitignore
# .gitignore

# 依赖目录
node_modules/
vendor/
venv/
__pycache__/

# 构建产物
dist/
build/
*.exe
*.dll
*.so
*.dylib

# 日志文件
*.log
logs/
npm-debug.log*

# 环境配置
.env
.env.local
.env.*.local
config/local.js

# IDE 配置
.vscode/
.idea/
*.swp
*.swo
*~
.DS_Store

# 测试覆盖率
coverage/
.nyc_output/
*.lcov

# 临时文件
*.tmp
*.bak
*.cache
.sass-cache/

# 密钥文件
*.pem
*.key
*.cert
secrets/
```

### 4. Pull/Push 策略

```bash
# 推荐工作流程
# 1. 开始工作前，先拉取最新代码
git checkout main
git pull origin main

# 2. 创建功能分支
git checkout -b feature/new-feature

# 3. 定期从主分支更新
git fetch origin
git rebase origin/main
# 或
git merge origin/main

# 4. 推送前先拉取
git pull --rebase origin feature/new-feature
git push origin feature/new-feature

# 避免使用 force push（除非必要）
# ❌ git push --force
# ✅ git push --force-with-lease  # 更安全
```

### 5. 代码审查清单

```
功能性
  ☐ 代码实现了需求吗？
  ☐ 边界情况处理了吗？
  ☐ 错误处理充分吗？
  ☐ 有潜在的 bug 吗？

可读性
  ☐ 命名清晰易懂吗？
  ☐ 逻辑简洁明了吗？
  ☐ 注释充分吗？
  ☐ 代码结构合理吗？

性能
  ☐ 有性能问题吗？
  ☐ 数据库查询优化了吗？
  ☐ 有内存泄漏风险吗？

安全
  ☐ 输入验证了吗？
  ☐ SQL 注入防护了吗？
  ☐ XSS 攻击防护了吗？
  ☐ 敏感数据加密了吗？

测试
  ☐ 单元测试覆盖了吗？
  ☐ 集成测试通过了吗？
  ☐ 边界测试充分吗？

文档
  ☐ API 文档更新了吗？
  ☐ README 更新了吗？
  ☐ 变更日志记录了吗？
```

### 6. 团队协作规范

#### 命名规范

```bash
# 分支命名
feature/login-page
feature/payment-gateway
bugfix/cart-calculation
hotfix/security-patch
release/v1.2.0

# 标签命名
v1.0.0        # 正式版本
v1.0.0-beta.1 # Beta 版本
v1.0.0-rc.1   # 候选版本

# 提交前缀
feat:     新功能
fix:      修复
docs:     文档
style:    格式
refactor: 重构
test:     测试
chore:    杂务
```

#### 工作流程

```mermaid
flowchart TD
    A["1. 认领任务<br/>Issue 指派 / 移到 In Progress"] --> B["2. 创建分支<br/>从最新 develop 拉出"]
    B --> C["3. 开发<br/>频繁提交 / 定期推送"]
    C --> D["4. 自测<br/>本地测试 / lint / 测试套件"]
    D --> E["5. 创建 MR<br/>详细描述 / 关联 Issue / 指定审查者"]
    E --> F["6. 代码审查<br/>回复评论 / 修改后重新请求"]
    F --> G["7. 合并<br/>批准后合并 / 删除分支 / 关闭 Issue"]
    G --> H["8. 验证<br/>测试环境验证功能"]
```

### 7. 提交签名与验证（Signed Commits）

> 🆕 **新增章节。** 提交里的 `user.name` / `user.email` 可任意伪造，签名能证明提交确实出自你本人。GitLab 会给通过验证的提交显示绿色 **Verified** 徽章。

#### 7.1 用 SSH key 签名（最简单，推荐）

复用已有的 SSH 密钥即可，无需额外生成：

```bash
# 指定用 SSH 方式签名
git config --global gpg.format ssh
git config --global user.signingkey ~/.ssh/id_ed25519.pub

# 开启自动签名
git config --global commit.gpgsign true

# 单次手动签名
git commit -S -m "feat: 带签名的提交"
```

然后到 GitLab：右上角头像 → **Preferences → SSH Keys**，把这把公钥的用途设为 **Signing**（或单独添加一个 Signing key）。

#### 7.2 用 GPG key 签名

```bash
# 生成 GPG 密钥
gpg --full-generate-key

# 查看密钥 ID
gpg --list-secret-keys --keyid-format=long

# 配置 Git 使用该密钥
git config --global user.signingkey <KEY_ID>
git config --global commit.gpgsign true

# 导出公钥，粘贴到 GitLab → Preferences → GPG Keys
gpg --armor --export <KEY_ID>
```

#### 7.3 验证与团队约定

```bash
# 查看本地提交的签名状态
git log --show-signature
```

- GitLab 上验证通过的提交会显示 **Verified** 徽章。
- 团队可在受保护分支开启 **Reject unsigned commits**（拒绝未签名提交），强制所有合入的提交都经过签名。

---

## 常见问题与解决方案

### 1. 认证问题

#### 问题：推送时要求输入密码

```bash
# 原因：使用 HTTPS 克隆
# 解决方案 1: 切换到 SSH
git remote set-url origin git@gitlab.com:username/repo.git

# 解决方案 2: 使用 Git Credential Manager 缓存 PAT（✅ 修订，替代旧的 wincred）
git config --global credential.helper manager        # Windows / 跨平台 GCM
git config --global credential.helper osxkeychain     # macOS Keychain
git config --global credential.helper 'cache --timeout=3600'  # 临时缓存
```

> 注意：GitLab.com 已停用账号密码，HTTPS 推送时“密码”处应输入 **Personal Access Token**。

#### 问题：SSH 连接失败

```bash
# 检查 SSH 密钥
ls -la ~/.ssh

# 测试连接
ssh -T git@gitlab.com
ssh -Tv git@gitlab.com  # 详细模式

# 检查 SSH 配置
cat ~/.ssh/config

# 添加 SSH 配置
cat >> ~/.ssh/config << EOF
Host gitlab.com
  PreferredAuthentications publickey
  IdentityFile ~/.ssh/id_ed25519
EOF

# 修正权限
chmod 700 ~/.ssh
chmod 600 ~/.ssh/id_ed25519
chmod 644 ~/.ssh/id_ed25519.pub
```

### 2. 合并冲突

#### 场景 1: Merge 冲突

```bash
git pull origin main
# CONFLICT (content): Merge conflict in file.txt
git status
# 编辑文件，删除冲突标记 <<<<<<<, =======, >>>>>>>
git add file.txt
git commit -m "解决合并冲突"
```

#### 场景 2: Rebase 冲突

```bash
git rebase main
# CONFLICT (content): Merge conflict in file.txt
git add file.txt
git rebase --continue    # 继续
git rebase --skip        # 跳过当前提交
git rebase --abort       # 放弃变基
```

### 3. 回滚和撤销

```bash
# 撤销工作区的修改
git checkout -- file.txt
git restore file.txt
git restore .

# 撤销暂存区（保留修改）
git reset HEAD file.txt
git restore --staged file.txt

# 撤销暂存区和工作区（丢弃修改）
git reset --hard HEAD

# 撤销已提交的修改
git reset --soft HEAD~1          # 保留修改
git reset --hard HEAD~1          # 丢弃修改
git revert <commit-hash>         # 创建反向提交
git revert <commit1>..<commit2>  # 撤销多个提交
```

#### 恢复删除的分支

```bash
git reflog
# 3d4e5f6 HEAD@{0}: commit: 最后的提交
# 1b2c3d4 HEAD@{2}: commit: 功能分支的提交
git checkout -b feature/recovered 1b2c3d4
```

### 4. 大文件处理

#### Git LFS（Large File Storage）

```bash
# 安装 Git LFS
# Windows: 下载安装 https://git-lfs.github.com/
# macOS: brew install git-lfs
# Linux: apt-get install git-lfs

git lfs install
git lfs track "*.psd"
git lfs track "*.zip"
git lfs track "*.mp4"
git lfs ls-files

git add .gitattributes
git commit -m "配置 Git LFS"
git add large-file.zip
git commit -m "添加大文件"
git push
```

#### 清理历史中的大文件

```bash
# 使用 BFG Repo-Cleaner（https://rtyley.github.io/bfg-repo-cleaner/）
java -jar bfg.jar --strip-blobs-bigger-than 10M repo.git
cd repo.git
git reflog expire --expire=now --all
git gc --prune=now --aggressive
git push --force
```

### 5. 性能优化

```bash
# 浅克隆
git clone --depth 1 <url>
git clone --single-branch --branch main <url>
git fetch --unshallow

# 清理本地仓库
git clean -n   # 预览
git clean -f   # 删除文件
git clean -fd  # 删除文件和目录
git gc --aggressive --prune=now
git repack -a -d --depth=250 --window=250
```

### 6. 权限问题

#### GitLab 权限级别

| 角色 | 主要权限 |
|------|----------|
| Guest（访客） | 查看 Issues、评论 |
| Reporter（报告者） | 克隆代码、下载 Artifacts、创建 Issues |
| Developer（开发者） | 推送到非保护分支、创建 MR、分配 Issues |
| Maintainer（维护者） | 推送到保护分支、管理 MR、管理 CI/CD |
| Owner（所有者） | 删除项目、管理成员、转移项目 |

#### 添加团队成员

进入项目 → Members → Invite members → 输入用户名/邮箱 → 选择角色 → 设置过期时间（可选）→ Invite。

### 7. 迁移和备份

```bash
# 克隆裸仓库（完整备份）
git clone --mirror <url> backup.git

# 恢复
cd backup.git
git remote set-url origin <new-url>
git push --mirror

# 归档包
git bundle create repo.bundle --all
git clone repo.bundle repo
```

导出/导入项目：项目 → Settings → General → Advanced → Export project；新建项目时选择 Import project。

---

## 实用技巧

### 1. Git Aliases（命令别名）

```bash
git config --global alias.co checkout
git config --global alias.br branch
git config --global alias.ci commit
git config --global alias.st status
git config --global alias.unstage 'reset HEAD --'
git config --global alias.last 'log -1 HEAD'
git config --global alias.visual 'log --oneline --graph --all'
git config --global alias.amend 'commit --amend --no-edit'
```

### 2. 搜索和查找

```bash
# 在提交历史中搜索
git log --grep="关键词"
git log --author="张三"
git log --since="2026-01-01" --until="2026-12-31"

# 在代码中搜索
git grep "function" $(git rev-list --all)
git grep -n "TODO"
git grep -c "TODO"

# 二分查找定位问题提交
git bisect start
git bisect bad
git bisect good <commit>
git bisect good  # 或 git bisect bad
git bisect reset
```

### 3. 储藏（Stash）

```bash
git stash
git stash save "工作进度描述"
git stash list
git stash apply
git stash pop
git stash apply stash@{1}
git stash drop stash@{0}
git stash clear
git stash branch feature/from-stash
```

### 4. Cherry-pick（挑选提交）

```bash
git cherry-pick <commit-hash>
git cherry-pick <commit1> <commit2>
git cherry-pick <commit1>..<commit2>
git cherry-pick --continue
git cherry-pick --abort
```

### 5. 子模块（Submodules）

```bash
git submodule add <url> path/to/submodule
git clone --recursive <url>
git submodule init
git submodule update
git submodule update --remote
git submodule deinit path/to/submodule
git rm path/to/submodule
rm -rf .git/modules/path/to/submodule
```

### 6. 工作树（Worktree）

```bash
git worktree add ../project-feature feature/new
git worktree list
git worktree remove ../project-feature
git worktree prune
```

---

## 附录

### A. Git 速查表

```bash
# 配置
git config --global user.name "name"
git config --global user.email "email"

# 初始化
git init
git clone <url>

# 基本操作
git status
git add <file>
git add .
git commit -m "message"
git push
git pull

# 分支
git branch
git branch <name>
git switch <branch>
git switch -c <branch>
git merge <branch>
git branch -d <branch>

# 历史
git log
git log --oneline
git log --graph
git show <commit>

# 撤销
git restore <file>
git restore --staged <file>
git reset --soft HEAD~1
git reset --hard HEAD~1
git revert <commit>

# 远程
git remote -v
git remote add origin <url>
git fetch
git pull
git push
```

### B. 常用 GitLab CI/CD 模板

```yaml
# Docker 应用
build:
  image: docker:24
  services:
    - docker:24-dind
  script:
    - docker build -t $CI_REGISTRY_IMAGE:$CI_COMMIT_SHA .
    - docker push $CI_REGISTRY_IMAGE:$CI_COMMIT_SHA
```

```yaml
# 前端应用
test:
  image: node:18
  script:
    - npm ci
    - npm run lint
    - npm run test
    - npm run build
```

```yaml
# Kubernetes 部署
deploy:
  image: bitnami/kubectl:latest
  script:
    - kubectl set image deployment/app app=$CI_REGISTRY_IMAGE:$CI_COMMIT_SHA
```

### C. 学习资源

**官方文档：**
- Git 官方文档：https://git-scm.com/doc
- GitLab 文档：https://docs.gitlab.com/
- Git Book：https://git-scm.com/book/zh/v2

**在线教程：**
- Learn Git Branching：https://learngitbranching.js.org/
- GitLab Learn：https://about.gitlab.com/learn/

**练习平台：**
- GitLab Demo：https://gitlab.com/gitlab-org/gitlab-foss

---

## 总结

### GitLab 核心优势

✅ **一体化平台** - 代码托管、CI/CD、项目管理全包含
✅ **强大的 CI/CD** - 内置完整的持续集成和部署
✅ **自托管选项** - 可部署在私有服务器
✅ **免费功能丰富** - 免费版本功能已非常强大
✅ **活跃的社区** - 持续更新和改进

### 学习路径建议

```mermaid
flowchart LR
    W1["第 1 周<br/>Git 基础"] --> W2["第 2-3 周<br/>协作开发"]
    W2 --> W3["第 4-5 周<br/>CI/CD"]
    W3 --> W4["第 6 周及以后<br/>高级特性"]
```

---

**文档版本：** 2.1（Mermaid 图 + 表格化 + 技术修订）
**创建日期：** 2026-07-09
**最后更新：** 2026-07-09
**作者：** AI Assistant

**注意：**
1. Mermaid 图会在 GitLab / GitHub 中直接渲染；`配图说明` 处仍需自行截取界面。
2. 在实际项目中练习所有命令，并根据团队需求调整工作流程。
3. 定期查阅官方文档获取最新信息。


---

# IntelliJ IDEA 2026 中使用 Git 和 GitLab

### 目录
1. [IDEA Git 集成概述](#idea-git-集成概述)
2. [初始配置](#初始配置)
3. [克隆 GitLab 项目](#克隆-gitlab-项目)
4. [日常 Git 操作](#日常-git-操作)
5. [分支管理](#分支管理-1)
6. [GitLab 集成功能](#gitlab-集成功能)
7. [高级功能](#高级功能-1)
8. [IDEA 2026 现代特性与进阶操作](#idea-2026-现代特性与进阶操作)
9. [实用技巧](#实用技巧-1)

---

## IDEA Git 集成概述

IntelliJ IDEA 提供了业界领先的 Git 集成，让您无需离开 IDE 即可完成所有版本控制操作。

### 核心优势

✅ **可视化界面** - 图形化的 Git 操作，降低学习曲线
✅ **智能提示** - 冲突检测、代码审查建议
✅ **无缝集成** - 与 GitLab、GitHub 等平台深度集成
✅ **历史追踪** - 强大的文件历史和差异查看
✅ **快捷操作** - 丰富的快捷键和右键菜单

---

## 初始配置

### 1. 安装和配置 Git

#### 步骤 1: 检查 Git 是否已安装

打开 IDEA，按 `Ctrl+Alt+S`（Windows/Linux）或 `Cmd+,`（macOS）打开设置，导航到 **Version Control → Git**。系统会自动检测 Git 可执行文件路径：

```
Windows: C:\Program Files\Git\bin\git.exe
macOS:   /usr/local/bin/git
Linux:   /usr/bin/git
```

#### 步骤 2: 测试 Git 配置

点击 **Test** 按钮，确认 Git 版本（如 `Git version 2.43.0`）。

#### 步骤 3: 配置 Git 用户信息

打开 IDEA 内置终端（`Alt+F12`）：
```bash
git config --global user.name "张三"
git config --global user.email "zhangsan@example.com"
```

### 2. 配置 GitLab 账号

**步骤 1:** 导航到 **Settings → Version Control → GitLab**。

**步骤 2:** 点击 **+** 添加账号（推荐使用 Token 登录）：

| 字段 | 值 |
|------|-----|
| Server | https://gitlab.com |
| Token | 点击 Generate 按钮生成 |

**步骤 3: 生成 Personal Access Token**

点击 **Generate** 后会跳转到 GitLab 网站：

| 配置项 | 说明 |
|--------|------|
| Token 名称 | 自动填充 `IntelliJ IDEA` |
| 权限 `api` | 完整 API 访问 ☑ |
| 权限 `read_repository` | 读取仓库 ☑ |
| 权限 `write_repository` | 写入仓库 ☑ |
| 过期时间 | 建议 90 天 |

点击 **Create personal access token** 并复制。

**步骤 4:** 将 Token 粘贴到 IDEA，点击 **Add Account**，成功后显示 GitLab 用户名。

---

## 克隆 GitLab 项目

### 方法 1: 从欢迎界面克隆

1. 欢迎界面点击 **Get from VCS** 按钮（或 **Git → Clone...**）
2. 左侧选择 **GitLab**，右侧显示项目列表
3. 选择项目 → 选择本地目录 → 点击 **Clone**

```
URL: git@gitlab.com:username/my-project.git
Directory: D:\Projects\my-project
```

### 方法 2: 从菜单克隆（已有项目打开时）

**Git → Clone...** 或 **VCS → Get from Version Control**

### 方法 3: 直接使用 URL

若项目不在列表中，选择 **Repository URL**，粘贴地址（SSH 或 HTTPS）：
```
SSH:   git@gitlab.com:username/project.git
HTTPS: https://gitlab.com/username/project.git
```

---

## 日常 Git 操作

### 1. 查看文件状态

打开 Git 工具窗口：**View → Tool Windows → Git** 或 `Alt+9`。

**文件颜色标识：**

| 颜色 | 状态 |
|------|------|
| 🟦 蓝色 | 已修改（Modified） |
| 🟩 绿色 | 新添加（Added） |
| 🟥 红色 | 冲突（Conflict） |
| ⚪ 灰色 | 已删除（Deleted） |
| ⚪ 无颜色 | 未修改（Unmodified） |

### 2. 提交更改（Commit）

**快捷键：** `Ctrl+K`（Windows/Linux）或 `Cmd+K`（macOS）；或 **Git → Commit**。

**Commit 窗口结构说明：**

| 区域 | 内容 |
|------|------|
| Changes（待提交文件） | 勾选要提交的文件，标注 (M)修改 / (A)新增 / (D)删除 |
| Commit Message | 提交信息，如 `feat: 添加用户登录功能` + 详细说明 |
| Author | 提交作者，如 `张三 <zhangsan@example.com>` |
| 选项 | ☐ Amend（修改上次提交） / ☑ Run Git hooks / ☐ Sign-off / ☐ Skip CI |
| 操作按钮 | `Commit` / `Commit and Push...` / `Cancel` |

**提交前检查（可配置）：** 代码检查、TODO 检查、格式化代码、优化导入、更新版权信息。

**提交选项对比：**

| 选项 | 行为 |
|------|------|
| Commit | 仅提交到本地仓库，不推送 |
| Commit and Push... | 提交到本地并立即推送到远程（推荐用于频繁同步） |

### 3. 查看差异（Diff）

- 单文件差异：编辑器右键 → **Git → Show Diff** 或 `Ctrl+D`
- 与分支比较：项目树右键文件 → **Git → Compare with Branch...**

**差异查看器常用操作：**

| 按钮/功能 | 作用 |
|-----------|------|
| `<` / `>` | 应用/回退单个更改 |
| `<<` / `>>` | 应用/回退所有更改 |
| 忽略空格 | 忽略空白差异 |
| 高亮单词差异 | 逐词高亮 |

**三方合并工具（解决冲突时）：**

```mermaid
flowchart TD
    L["左侧：本地版本<br/>Your Version"] --> R["结果版本<br/>Result"]
    B["中间：基础版本<br/>Base"] --> R
    S["右侧：远程版本<br/>Server Version"] --> R
```

### 4. 推送（Push）

**快捷键：** `Ctrl+Shift+K`（Windows/Linux）或 `Cmd+Shift+K`（macOS）；或 **Git → Push**。

**Push 对话框内容说明：**

| 区域 | 内容 |
|------|------|
| Branch | 当前分支 → 目标远程分支，如 `main → origin/main` |
| Commits to Push | 待推送提交列表 |
| Files Changed | 涉及的文件列表 |
| 选项 | ☐ Force Push（危险！建议用 --force-with-lease） / ☐ Push Tags / ☐ Skip Hooks |
| 操作 | `Push` / `Cancel` |

### 5. 拉取（Pull）和更新（Update）

**Update Project（推荐）：** `Ctrl+T`（Windows/Linux）或 `Cmd+T`（macOS）；或 **Git → Update Project**。

**Update 选项：**

| 选项 | 说明 |
|------|------|
| ● Merge（合并） | 创建合并提交，保留完整历史 |
| ○ Rebase（变基） | 线性历史，更清晰的提交记录 |
| ○ Branch Default | 使用分支配置的默认方式 |
| ☑ Clean working tree before update | 更新前暂存本地修改 |
| ☑ Update submodules | 同时更新子模块 |

- **Git → Pull** — 直接从远程拉取并合并
- **Git → Fetch** — 只获取远程更新，不合并（等同 `git fetch origin`）

### 6. 查看历史记录

打开 Git Log：**Git → Show Git Log** 或 `Alt+9`（Log 标签）；文件右键 → **Git → Show History**。

**Log 界面功能：**

| 区域 | 功能 |
|------|------|
| 顶部工具栏 | 搜索提交、按作者/日期/分支/路径筛选 |
| 主界面 | 图形化分支树：提交哈希、信息、作者、日期、分支/标签 |
| 底部面板 | 提交详情、修改文件列表、差异预览 |

**文件历史（Annotate / Blame）：** 编辑器右键 → **Git → Annotate with Git Blame**，显示每行的最后修改者、修改时间、提交哈希与信息。

---

## 分支管理

### 1. 查看分支

**快捷键：** `Ctrl+Shift+`（反引号）或 `Cmd+Shift+`（macOS）；或点击右下角状态栏的分支名称。

**分支列表结构：**

```mermaid
flowchart TD
    cur["★ 当前分支：main"]
    subgraph local["Local Branches"]
        l1["develop"]
        l2["feature/login"]
        l3["feature/payment"]
    end
    subgraph remote["Remote Branches"]
        r1["origin/main"]
        r2["origin/develop"]
        r3["origin/feature/dashboard"]
    end
```

### 2. 创建分支

- **方法 1（Branches 弹窗）：** `Ctrl+Shift+`（反引号）→ **+ New Branch** → 输入 `feature/new-feature` → ☑ Checkout branch
- **方法 2（菜单）：** **Git → New Branch**
- **方法 3（从提交创建）：** Git Log 中右键提交 → **New Branch from Selected...**

### 3. 切换分支（Checkout）

- 状态栏点击分支名 → 选择目标分支
- `Ctrl+Shift+`（反引号）→ 双击分支名
- Git 工具窗口 → 右键分支 → **Checkout**

**有未提交更改时的切换选项：**

| 选项 | 行为 |
|------|------|
| Smart Checkout（智能切换） | 尝试保留更改，若冲突则提示 |
| Force Checkout（强制切换） | 放弃所有更改 |
| Stash Changes（暂存更改） | 保存到 stash，切换后可恢复 |

### 4. 合并分支（Merge）

1. 切换到目标分支（如 `main`）
2. Branches 弹窗右键要合并的分支（如 `feature/login`）→ **Merge into Current**（或 **Git → Merge Changes...**）

**合并选项：**

| 选项 | 说明 |
|------|------|
| Merge Strategy | Default / Recursive / Resolve / Octopus / Ours / Subtree |
| ☐ --no-ff | 总是创建合并提交（No fast-forward） |
| ☐ --squash | 压缩所有提交 |
| ☐ --no-commit | 不自动提交 |

### 5. 变基（Rebase）

1. 切换到要变基的分支（如 `feature/login`）
2. 右键目标分支（如 `main`）→ **Rebase Current onto Selected**（或 **Git → Rebase...**）

**交互式 Rebase 操作（Git → Rebase... → Interactive Rebase）：**

| 操作 | 说明 |
|------|------|
| pick | 保留提交 |
| squash | 合并到上一个提交 |
| reword | 修改提交信息 |
| edit | 编辑提交 |
| drop | 删除提交 |

### 6. 删除分支

- 删除本地分支：Branches 弹窗右键分支 → **Delete**
- 删除远程分支：右键远程分支 → **Delete Remote Branch**
- 批量删除：**Git → Branches → Delete Branches...**（可批量选择已合并分支）

---

## GitLab 集成功能

### 1. Merge Request（合并请求）

**创建方式：**
- **方法 1：** 推送后在结果对话框点击 **Create Merge Request**
- **方法 2：** **Git → GitLab → Create Merge Request**

**MR 创建对话框字段：**

| 字段 | 示例 |
|------|------|
| Title | `feat: 添加用户登录功能` |
| Description | 功能说明（支持 Markdown） |
| Source Branch | `feature/login` |
| Target Branch | `main` |
| Assignee | `@lisi` |
| Reviewer | `@wangwu` |
| Labels | `feature`, `backend` |
| 选项 | ☐ Delete source branch after merge / ☐ Squash commits / ☑ Mark as Draft |

### 2. 查看和管理 Merge Requests

打开 MR 工具窗口：**Git → GitLab → Show Merge Requests**。

**列表筛选：** All / Opened / Merged / Closed / Assigned to me / Created by me。

**MR 列表示例：**

| MR | 标题 | 分支 | 作者 | 状态 |
|----|------|------|------|------|
| !123 | feat: 添加用户登录 | feature/login → main | 张三 | 💬 3 评论 · ✓ CI Passed |
| !122 | fix: 修复购物车bug | bugfix/cart → develop | 李四 | 💬 1 评论 · ⚠️ CI Failed |

**MR 详情标签页：**

| 标签页 | 内容 |
|--------|------|
| Overview（概览） | 描述、流水线状态、审查者 |
| Changes（更改） | 文件差异、内联评论、代码建议 |
| Commits（提交） | 提交列表与详情 |
| Discussions（讨论） | 评论线程、待办事项 |

**操作按钮：** `Approve` / `Comment` / `Merge` / `Close`。

### 3. 代码审查

- 在 Changes 标签点击代码行号旁的 **+** 添加评论：**Add Comment**（单条）或 **Start Review**（批量）
- 代码建议：使用 ```` ```suggestion ```` 语法直接建议修改
- 审查结论：**Approve** / **Request Changes** / **Comment**

### 4. Issue 管理

- 查看：**Git → GitLab → Show Issues**
- 创建：**Git → GitLab → Create Issue**

**Issue 创建对话框字段：**

| 字段 | 示例 |
|------|------|
| Title | 登录页面在 Safari 无法显示 |
| Description | Bug 描述 + 重现步骤（Markdown） |
| Type | Bug |
| Assignee | `@zhangsan` |
| Labels | `bug`, `frontend` |
| Milestone | v1.0.0 |

在提交信息中用 `Closes #123` / `Related to #124` 可自动关联 Issue。

### 5. CI/CD Pipeline 查看

Git Log 中每个提交旁显示 CI 状态图标：

| 图标 | 含义 |
|------|------|
| ✓ | 成功 |
| ✗ | 失败 |
| ⏳ | 运行中 |
| ○ | 待处理 |

打开 Pipeline 详情：右键提交 → **GitLab → View Pipeline**，或点击 CI 状态图标（在浏览器中打开）。

---

## 高级功能

### 1. 储藏更改（Stash）

创建：**Git → Uncommitted Changes → Stash Changes**，或 Commit 窗口的 **Stash** 按钮。

**Stash 选项：**

| 选项 | 说明 |
|------|------|
| Message | 描述，如 `WIP: 登录功能开发中` |
| ☑ Include untracked files | 包含未跟踪的文件 |
| ☐ Keep index | 保留暂存区状态 |

查看/应用：**Git → Uncommitted Changes → Unstash Changes...**

| 操作 | 说明 |
|------|------|
| Apply | 应用 stash（保留） |
| Pop | 应用并删除 |
| Drop | 删除 stash |
| ☑ Remove stash after applying | 应用后删除 |
| ☐ Restore index | 恢复暂存区状态 |

### 2. Cherry-pick

- 单个：Git Log 中右键提交 → **Cherry-Pick**
- 多个：按 `Ctrl`/`Cmd` 多选提交 → 右键 → **Cherry-Pick**

### 3. 重置和还原

- **Revert（撤销提交）：** Git Log 右键提交 → **Revert Commit**（创建反向提交）
- **Reset（重置到指定提交）：** 右键目标提交 → **Reset Current Branch to Here...**

**Reset 类型：**

| 类型 | 行为 |
|------|------|
| Soft | 保留所有更改在暂存区（最安全） |
| Mixed（默认） | 保留更改在工作区，取消暂存 |
| Hard | 丢弃所有更改（⚠️ 危险操作！） |

### 4. 标签管理

- 创建：Git Log 右键提交 → **New Tag...**（如 `v1.0.0`）；或 **Git → New Tag...**
- 推送：**Git → Push Tags...**，或在 Push 对话框勾选 **Push Tags**

### 5. 子模块（Submodules）

- 添加：**Git → Submodule → Add Submodule**（填写仓库 URL 与目录）
- 更新：**Git → Submodule → Update Submodules**

### 6. 解决冲突

合并/拉取有冲突时，IDEA 弹出 **Conflicts Detected** 对话框，列出冲突文件，点击 **Merge** 打开三方合并工具（见上文"日常 Git 操作 → 三方合并工具"图）。

**三方合并工具栏按钮：**

| 按钮 | 作用 |
|------|------|
| `<<` | 接受左侧（您的更改） |
| `>>` | 接受右侧（服务器更改） |
| `<< >>` | 接受两者 |
| `X` | 拒绝两者 |

也可在 Conflicts 对话框逐文件选择 **Accept Yours** / **Accept Theirs** / **Merge**。

---

## IDEA 2026 现代特性与进阶操作

> 🆕 **新增章节。** 以下是 2024→2026 版 IntelliJ IDEA 在 Git / GitLab 协作上引入或强化的能力，很多操作无需再切到命令行。

### 1. AI Assistant 与 Git 集成

IDEA 内置的 **AI Assistant**（需订阅 AI Pro / 企业授权）深度接入了版本控制流程。

| 场景 | 操作 | 说明 |
|------|------|------|
| 生成提交信息 | Commit 窗口信息框旁的 ✨ 图标 | 根据当前**暂存的 diff** 自动生成规范的 commit message |
| 生成 MR / PR 描述 | 创建 MR 时的 AI 生成按钮 | 汇总本次改动，生成结构化描述 |
| 解释提交 | Git Log 右键提交 → **Explain Commit** | 用自然语言说明这次改动做了什么 |
| 提交前审查 | Commit 检查阶段 | AI 给出潜在问题 / 改进建议 |

**典型流程：**

```mermaid
flowchart LR
    A["git add<br/>暂存改动"] --> B["点击 ✨<br/>AI 生成提交信息"]
    B --> C["人工确认 / 微调"]
    C --> D["Commit / Commit and Push"]
```

> 💡 建议：AI 生成的信息仍需人工核对，确保符合团队的 Conventional Commits 规范（如 `feat:` / `fix:` 前缀）。

### 2. Git 暂存区（Staging Area）与变更列表（Changelists）

IDEA 现在支持原生 Git **Staging Area**，可像命令行一样精细控制 `git add`。

**启用：** Settings → Version Control → Git → ☑ **Enable staging area**。

| 特性 | Staging Area（暂存区） | Changelists（变更列表） |
|------|------------------------|--------------------------|
| 本质 | Git 原生 `git add` 机制 | IDEA 自有的改动分组（不影响 Git） |
| 能否只提交部分行 | ✅ 支持（按 hunk / 按行 stage） | ✅ 支持 |
| 与命令行一致 | ✅ 完全一致 | ❌ 仅 IDEA 内可见 |
| 是否互斥 | 二者**互斥**，启用暂存区后 Changelists 隐藏 | 关闭暂存区时可用 |

- 启用暂存区后，Commit 窗口的文件会出现 **Stage/Unstage** 复选状态，可只暂存某个文件甚至某几行。
- 老习惯用 Changelists 分组改动（如把“调试代码”单独分一组不提交）的团队，可保留关闭状态。

### 3. Git Log 里的“无命令行”提交编辑

在 **Git 工具窗口 → Log** 标签中右键某个提交，即可完成过去必须靠命令行的历史整理操作：

| 菜单项 | 等价命令 | 作用 |
|--------|----------|------|
| **Undo Commit** | `git reset --soft HEAD~1` | 撤销最近提交，改动退回暂存区 |
| **Edit Commit Message**（Reword） | `git rebase -i` → reword | 修改历史提交的信息 |
| **Fixup...** | `git commit --fixup` + autosquash | 把改动并入指定历史提交 |
| **Squash...** | `git rebase -i` → squash | 合并多个提交为一个 |
| **Drop Commit** | `git rebase -i` → drop | 删除某个提交 |
| **Interactively Rebase from Here** | `git rebase -i <commit>` | 打开图形化交互式变基面板 |

> ⚠️ 这些操作会**改写历史**，只对未推送 / 个人分支使用；已共享的分支慎用。

### 4. 补丁：Create Patch / Apply Patch

不推送也能分享或应用改动，适合评审、跨环境搬运、离线协作。

| 操作 | 入口 | 说明 |
|------|------|------|
| 创建补丁 | 选中改动/提交右键 → **Git → Create Patch...** | 导出 `.patch` 文件 |
| 应用补丁 | **Git → Apply Patch...** | 应用他人给的 `.patch` |
| 从剪贴板应用 | **Git → Apply Patch from Clipboard** | 直接粘贴补丁内容应用 |

```mermaid
flowchart LR
    A["本地改动 / 提交"] -->|Create Patch| B["changes.patch 文件"]
    B -->|发送给同事 / 存档| C["另一台机器 / 他人"]
    C -->|Apply Patch| D["还原相同改动"]
```

### 5. `.gitlab-ci.yml` 智能编辑

IDEA 对 GitLab CI 配置文件提供专门支持：

| 能力 | 说明 |
|------|------|
| Schema 校验 | 实时标出非法关键字 / 缩进错误 |
| 关键字补全 | `stages` / `rules` / `needs` / `artifacts` 等自动补全 |
| `include` 跳转 | 可跳转到被 include 的模板文件 |
| 锚点 & 变量提示 | YAML 锚点（`&`/`*`）、预定义变量提示 |
| 流水线查看 | 结合 GitLab 集成，可查看该配置触发的 Pipeline 状态 |

> 与前文「CI/CD → 现代 .gitlab-ci.yml 语法」章节配合使用：在 IDEA 里写 `rules:` / `needs:` / `include:` 时会有补全和校验，降低出错率。

### 6. 新 UI 的 Git 入口与受保护分支提醒

**新 UI（2024+ 默认）中 Git 操作的位置：**

| 位置 | 功能 |
|------|------|
| 顶部工具栏 **VCS 组件**（显示当前分支名） | 点击弹出分支列表、切换/新建分支 |
| 顶部工具栏 Git 图标组 | Update / Commit / Push 快捷按钮 |
| 左侧 **Commit** 竖排工具窗口 | 常驻的提交面板（新 UI 默认） |
| 底部 **Git** 工具窗口（`Alt+9`） | Log、Console、分支树 |

**受保护分支提醒：** 当你试图直接向 GitLab 的**受保护分支**（如 `main`）提交或推送时，IDEA 会弹出警告，提示改用功能分支 + MR 流程，避免误操作。

### 7. 多账号与多远程

**多账号：** Settings → Version Control → GitLab（及 GitHub）可添加多个账号（如公司实例 + gitlab.com），克隆/创建 MR 时按需选择。

**多远程推送：**

```bash
# 为同一仓库配置多个远程
git remote add origin   git@gitlab.com:team/app.git
git remote add backup   git@github.com:team/app-mirror.git
```

在 IDEA 的 **Push** 对话框中，可分别选择目标 remote 逐个推送；也可用别名一次推送到多个远程（需在 `.git/config` 配置多个 pushurl）。

```mermaid
flowchart LR
    L["本地仓库"] -->|push| O["origin<br/>gitlab.com"]
    L -->|push| B["backup<br/>github.com（镜像）"]
```

---

## 实用技巧

### 1. 快捷键速查

> ✅ **修订：** 原文将 Fetch 标为 `Ctrl+Shift+F`（该键位实为 Find in Files），Fetch 默认无快捷键，需从菜单 **Git → Fetch** 触发；Annotate / 提交详情等键位随版本与键盘布局不同，建议以右键菜单为准。

| 操作 | 快捷键（Windows/Linux） |
|------|--------------------------|
| 提交更改 | `Ctrl+K` |
| 更新项目 | `Ctrl+T` |
| 推送 | `Ctrl+Shift+K` |
| 回滚更改 | `Ctrl+Alt+Z` |
| 分支操作弹窗 | `` Alt+` `` （反引号） |
| Git 工具窗口 | `Alt+9` |
| 查看差异 | `Ctrl+D` |
| Fetch | 无默认键位（菜单 Git → Fetch）✅ 修订 |
| Annotate / Blame | 右键菜单（键位随版本变化） |

### 2. 配置推荐设置

**Settings → Version Control → Git：**

| 设置 | 作用 |
|------|------|
| ☑ Auto-update if push of the current branch was rejected | 推送被拒时自动更新 |
| ☑ Warn if CRLF line separators are about to be committed | 提交 CRLF 时警告 |
| ● Automatically set branch tracking | 自动设置分支跟踪 |

**Settings → Version Control → Commit（提交前检查）：**

| 选项 | 作用 |
|------|------|
| ☑ Reformat code | 格式化代码 |
| ☑ Optimize imports | 优化导入 |
| ☑ Perform code analysis | 代码分析 |
| ☑ Check TODO | 检查 TODO |
| ☑ Update copyright | 更新版权 |

### 3. 其他实用功能

- **自定义文件颜色：** Settings → Version Control → File Status Colors
- **命令行终端集成：** `Alt+F12` 打开内置终端，可直接执行任意 Git 命令
- **Local History（本地历史）：** 右键文件 → **Local History → Show History**，即使未提交也能查看/恢复
- **Shelf（搁置更改）：** **Git → Uncommitted Changes → Shelf Changes**，可命名、长期保存、跨分支使用、部分文件搁置
- **快速操作面板：** `Ctrl+Shift+A`（Windows/Linux）或 `Cmd+Shift+A`（macOS），输入 `git commit`、`git push` 等快速执行

---

## 常见问题

### 1. IDEA 无法识别 Git 仓库
检查项目根目录是否有 `.git` 文件夹；**VCS → Enable Version Control Integration** → 选择 **Git**。

### 2. 推送时认证失败
- SSH：Settings → Version Control → Git → SSH executable，选择 Built-in 或 Native
- HTTPS：使用 GitLab Personal Access Token 代替密码 ✅ 修订

### 3. 合并冲突无法解决
在 IDEA 终端使用 `git status` / `git mergetool` 辅助，或使用 **Abort Merge** 取消合并。

### 4. Git 操作很慢
启用 Git 索引缓存；排除不必要目录（`node_modules`）；用 `.gitignore` 忽略大文件。

### 5. 无法看到远程分支
执行 Fetch：**Git → Fetch**（✅ 修订：Fetch 无默认快捷键，通过菜单触发）。

---

## 总结

### IDEA Git 集成优势

✅ **可视化操作** - 无需记忆复杂命令
✅ **智能提示** - 自动检测冲突和问题
✅ **一站式** - 所有操作不离开 IDE
✅ **GitLab 深度集成** - MR、Issue、CI/CD 全支持
✅ **强大的差异工具** - 业界最好的 Diff/Merge 工具

### 学习建议

```mermaid
flowchart LR
    A["第 1 周<br/>基本操作<br/>Commit/Push/Pull"] --> B["第 2 周<br/>分支管理"]
    B --> C["第 3 周<br/>GitLab 集成<br/>MR/Issue"]
    C --> D["第 4 周<br/>高级功能<br/>Rebase/Cherry-pick"]
```

### 最佳实践

1. ✅ 频繁提交，保持提交原子性
2. ✅ 写清晰的提交信息
3. ✅ 提交前进行代码检查
4. ✅ 使用 MR 进行代码审查
5. ✅ 保持分支命名规范
6. ✅ 定期同步远程分支

---

**IntelliJ IDEA 版本：** 最新版本（Git 集成功能随版本更新而改进）  ✅ 修订
**更新日期：** 2026-07-09
**作者：** AI Assistant

**更多资源：**
- [IntelliJ IDEA 官方文档](https://www.jetbrains.com/help/idea/)
- [Git 集成教程](https://www.jetbrains.com/help/idea/working-with-git-tutorial.html)
- [GitLab 集成文档](https://www.jetbrains.com/help/idea/gitlab.html)
