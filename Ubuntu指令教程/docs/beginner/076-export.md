# 076 export：设置环境变量

> 级别：初级 · 来源：bash 内建命令

## 1. 作用

在 bash 里写 `变量=值`，只是定义了一个 **Shell 变量**，只有当前 Shell 自己能用。`export` 把它变成**环境变量**，之后从这个 Shell 启动的所有程序、脚本都能读到它。

常见用途：设置 `JAVA_HOME`、把自己的程序目录加进 `PATH`、给程序传配置（如 `APP_ENV=prod`）。

## 2. 基本格式

```bash
export 变量名=值
export 已有的变量名
```

等号两边不能有空格。

## 3. 常用参数

| 写法 | 作用 |
|---|---|
| `export 变量=值` | 定义并导出 |
| `export -p` | 列出所有已导出的变量 |
| `export -n 变量` | 取消导出（变量还在，只是子进程看不到了） |
| `unset 变量` | 删除变量 |

## 4. 常用示例

### 4.1 有没有 export 的区别

```
$ APP_ENV=test; bash -c 'echo "子进程看到: [$APP_ENV]"'
子进程看到: []

$ export APP_ENV=test; bash -c 'echo "子进程看到: [$APP_ENV]"'
子进程看到: [test]
```

`bash -c` 启动了一个子进程。没有 export 时，子进程看不到 APP_ENV。

### 4.2 把目录加进 PATH

```
$ export PATH="$HOME/bin:$PATH"; echo $PATH
/home/tom/bin:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin
```

`$HOME/bin` 放在最前面，同名命令会优先使用这里的版本。一定要保留后面的 `:$PATH`。

### 4.3 永久生效

在终端里 export 的变量，关掉终端就没了。要永久生效，写进 `~/.bashrc`：

```bash
echo 'export JAVA_HOME=/usr/lib/jvm/java-21-openjdk-amd64' >> ~/.bashrc
echo 'export PATH="$JAVA_HOME/bin:$PATH"' >> ~/.bashrc
source ~/.bashrc
```

用单引号，是为了让 `$JAVA_HOME`、`$PATH` 原样写进文件，等每次登录时再展开。

| 文件 | 作用范围 |
|---|---|
| `~/.bashrc` | 只对自己，每开一个终端都会读取 |
| `/etc/environment` | 对所有用户，格式是 `变量=值`，不能写 export，也不展开 `$变量` |

> ⚠️ 4.3 未实机验证：JAVA_HOME 的路径以你实际安装的 JDK 为准，用 `ls /usr/lib/jvm` 确认。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `export PATH=/opt/app/bin` 之后，ls、cd 以外的命令全都找不到了 | 把 PATH 整个覆盖了，系统目录丢了 | 写成 `export PATH="/opt/app/bin:$PATH"`；改坏了就关掉终端重开 |
| `export A = 1` 报 ``bash: export: `=': not a valid identifier`` | 等号两边有空格 | 写成 `export A=1` |
| 写进 .bashrc 了，ssh 执行远程命令时还是读不到 | 非交互式的 Shell 可能不读取 .bashrc | 程序的配置用配置文件或 systemd 的 `Environment=`，不要依赖 .bashrc |
| `sudo 命令` 读不到自己 export 的变量 | sudo 默认会重置环境变量 | 写成 `sudo 变量=值 命令`；见 [sudo](004-sudo.md) |

## 6. 一句话总结

子程序要用的变量要 `export`；改 PATH 一定带上 `:$PATH`；永久生效写进 `~/.bashrc`。

---

[← 上一个：env](075-env.md) · [返回目录](../../README.md) · [下一个：source →](077-source.md)
