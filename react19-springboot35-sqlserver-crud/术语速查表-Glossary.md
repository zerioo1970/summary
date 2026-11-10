# 术语速查表（Glossary）

> 回到：[README 目录](README.md)
>
> 本表汇总全教程出现的术语，每条一句话解释 + 跳转到详细讲解处。**看到不认识的词就来这里查。** 按主题分类，方便定位。

---

## 一、整体与架构

| 术语 | 英文 | 一句话解释 | 详见 |
|------|------|-----------|------|
| 前端 | Frontend | 运行在浏览器、负责界面和交互的部分（本教程 React） | [01](01-架构与技术选型.md) |
| 后端 | Backend | 运行在服务器、负责业务和数据的部分（本教程 Spring Boot） | [01](01-架构与技术选型.md) |
| 全栈 | Full-stack | 前端 + 后端都做 | [01](01-架构与技术选型.md) |
| 前后端分离 | — | 前后端各自独立开发部署，通过 HTTP+JSON 通信 | [01](01-架构与技术选型.md) |
| CRUD | Create/Read/Update/Delete | 增、查、改、删四种基本数据操作 | [06](06-后端分层代码.md) |
| 分层架构 | Layered Architecture | 把后端拆成 Controller/Service/Mapper 各司其职 | [06](06-后端分层代码.md) |

## 二、前端（React 生态）

| 术语 | 英文 | 一句话解释 | 详见 |
|------|------|-----------|------|
| React | — | 构建用户界面的 JS 库 | [09](09-创建React19前端.md) |
| Vite | — | 前端构建/开发工具，启动快 | [09](09-创建React19前端.md) |
| 组件 | Component | 可复用的 UI 单元，通常是返回 JSX 的函数 | [附录B.1](附录B-前端概念详解.md) |
| JSX | — | 在 JS 里写的类 HTML 语法 | [附录B.1](附录B-前端概念详解.md) |
| 状态 | State | 组件内会变化的数据，改它界面自动刷新 | [附录B.2](附录B-前端概念详解.md) |
| Hooks | — | 以 `use` 开头、给函数组件加能力的函数 | [附录B.3](附录B-前端概念详解.md) |
| useState | — | 声明和管理状态的 Hook | [附录B.2](附录B-前端概念详解.md) |
| useEffect | — | 处理副作用（如加载数据）的 Hook | [附录B.3](附录B-前端概念详解.md) |
| 虚拟 DOM | Virtual DOM | 用 JS 对象描述页面结构的副本，用于高效更新 | [附录B.4](附录B-前端概念详解.md) |
| Diff | — | 对比新旧虚拟 DOM，算出最小改动 | [附录B.4](附录B-前端概念详解.md) |
| Promise | — | 代表"将来才有结果"的异步操作对象 | [附录B.5](附录B-前端概念详解.md) |
| async/await | — | 让异步代码写得像同步的语法糖 | [附录B.6](附录B-前端概念详解.md) |
| axios | — | 前端发 HTTP 请求的库 | [10](10-封装axios与API层.md) |
| 拦截器 | Interceptor | axios 里统一处理请求/响应的钩子 | [10](10-封装axios与API层.md) |

## 三、后端（Spring 生态）

| 术语 | 英文 | 一句话解释 | 详见 |
|------|------|-----------|------|
| Spring Boot | — | 简化 Spring 开发的框架，内置服务器、自动配置 | [04](04-创建SpringBoot后端.md) |
| IoC | 控制反转 | 创建对象的控制权交给框架 | [附录A.2](附录A-Spring核心概念详解.md) |
| IoC 容器 | — | 帮你创建和管理所有对象的"大管家" | [附录A.2](附录A-Spring核心概念详解.md) |
| Bean | — | 被 Spring 容器管理的对象 | [附录A.3](附录A-Spring核心概念详解.md) |
| 依赖注入 | DI | 容器自动把依赖"喂"给你，不用自己 new | [附录A.4](附录A-Spring核心概念详解.md) |
| 动态代理 | Dynamic Proxy | 运行时凭空生成实现某接口的对象 | [附录A.5](附录A-Spring核心概念详解.md) |
| 反射 | Reflection | 运行时读取类/方法信息的能力 | [附录A.5](附录A-Spring核心概念详解.md) |
| 单例 | Singleton | 容器里同种 Bean 只有一个实例，共用 | [附录A.3](附录A-Spring核心概念详解.md) |
| 注解 | Annotation | 贴在代码上的标签，供框架读取以改变行为 | [附录F](附录F-注解详解与速查.md) |
| 注释 | Comment | 给人看的说明，程序运行时忽略 | [附录F.0](附录F-注解详解与速查.md) |
| APT | 注解处理器 | 编译期读注解并生成代码（如 Lombok） | [附录F.2](附录F-注解详解与速查.md) |
| Lombok | — | 编译期自动生成 getter/setter 等的工具 | [06](06-后端分层代码.md) |

## 四、持久层与数据库

| 术语 | 英文 | 一句话解释 | 详见 |
|------|------|-----------|------|
| MyBatis-Flex | — | 基于 MyBatis 的增强 ORM 框架 | [04](04-创建SpringBoot后端.md) |
| ORM | 对象关系映射 | 把"数据库表↔Java 对象"互相映射 | [06](06-后端分层代码.md) |
| Entity | 实体类 | 对应一张表的 Java 类，一行=一个对象 | [06](06-后端分层代码.md) |
| Mapper / DAO | — | 专门读写数据库的接口层 | [06](06-后端分层代码.md) |
| BaseMapper | — | MyBatis-Flex 内置的一套通用 CRUD 方法 | [06](06-后端分层代码.md) |
| QueryWrapper | — | 链式构造查询条件的对象 | [06](06-后端分层代码.md) |
| Service | 业务层 | 承载业务逻辑、事务的一层 | [06](06-后端分层代码.md) |
| Controller | 控制层 | 接收 HTTP 请求、返回 JSON 的一层 | [06](06-后端分层代码.md) |
| JDBC | — | Java 连接数据库的标准接口 | [05](05-数据库连接与MyBatisFlex配置.md) |
| SQL Server | — | 微软的关系型数据库 | [03](03-SQLServer建库建表.md) |
| 事务 | Transaction | 把多个操作捆成"要么全成要么全败" | [附录D.1](附录D-数据库与事务.md) |
| ACID | — | 事务四特性：原子/一致/隔离/持久 | [附录D.2](附录D-数据库与事务.md) |
| 连接池 | Connection Pool | 预建连接反复借还，避免反复建连接 | [附录D.4](附录D-数据库与事务.md) |
| HikariCP | — | Spring Boot 默认的高性能连接池 | [附录D.4](附录D-数据库与事务.md) |
| 主键 | Primary Key | 唯一标识一行的字段（本教程 id） | [03](03-SQLServer建库建表.md) |
| 自增 | Auto Increment / IDENTITY | 主键由数据库自动递增生成 | [03](03-SQLServer建库建表.md) |

## 五、通信（HTTP / JSON）

| 术语 | 英文 | 一句话解释 | 详见 |
|------|------|-----------|------|
| HTTP | — | 浏览器与服务器通信的协议 | [附录C.1](附录C-HTTP与JSON基础.md) |
| 请求方法 | HTTP Method | GET/POST/PUT/DELETE 等，表达操作意图 | [附录C.2](附录C-HTTP与JSON基础.md) |
| 幂等 | Idempotent | 同一请求发多次结果一样 | [附录C.2](附录C-HTTP与JSON基础.md) |
| 状态码 | Status Code | 2xx成功/4xx客户端错/5xx服务端错 | [附录C.3](附录C-HTTP与JSON基础.md) |
| REST | — | 用 HTTP 方法+URL 表达资源操作的风格 | [06](06-后端分层代码.md) |
| JSON | — | 前后端交换数据的轻量文本格式 | [附录C.4](附录C-HTTP与JSON基础.md) |
| 序列化 | Serialization | 对象 → JSON 字符串 | [附录C.4](附录C-HTTP与JSON基础.md) |
| 反序列化 | Deserialization | JSON 字符串 → 对象 | [附录C.4](附录C-HTTP与JSON基础.md) |
| 跨域 | CORS | 不同源之间的请求，需后端放行 | [附录C.5](附录C-HTTP与JSON基础.md) |
| 同源策略 | Same-Origin Policy | 浏览器安全规则：协议+域名+端口须相同 | [附录C.5](附录C-HTTP与JSON基础.md) |
| 预检请求 | Preflight | 复杂跨域请求前自动发的 OPTIONS 探问 | [附录C.5](附录C-HTTP与JSON基础.md) |

## 六、构建与部署（Gradle / 上线）

| 术语 | 英文 | 一句话解释 | 详见 |
|------|------|-----------|------|
| Gradle | — | 构建工具：下载依赖、编译、测试、打包 | [附录E](附录E-Gradle构建原理.md) |
| Gradle Wrapper | gradlew | 自动用项目指定版本 Gradle 的启动器 | [附录E.1](附录E-Gradle构建原理.md) |
| 依赖坐标 | GAV | `group:artifact:version` 三段式标识 | [附录E.2](附录E-Gradle构建原理.md) |
| 传递依赖 | Transitive | 依赖自动带上它自己依赖的东西 | [附录E.2](附录E-Gradle构建原理.md) |
| implementation | — | 默认依赖配置，不向上游传递 | [附录E.3](附录E-Gradle构建原理.md) |
| api | — | 会把依赖暴露给使用方（写库时用） | [附录E.3](附录E-Gradle构建原理.md) |
| jar | — | Java 打包产物 | [附录E.4](附录E-Gradle构建原理.md) |
| bootJar | — | Spring Boot 可执行 jar | [附录G](附录G-项目部署上线.md) |
| Profile | — | 环境配置（dev/prod）切换机制 | [附录G](附录G-项目部署上线.md) |
| Nginx | — | 高性能 Web 服务器/反向代理 | [附录G](附录G-项目部署上线.md) |
| 反向代理 | Reverse Proxy | 由中间服务器转发请求到后端 | [附录G](附录G-项目部署上线.md) |
| Docker | — | 把应用连环境打包成容器 | [附录G](附录G-项目部署上线.md) |

---

> 回到 👉 [README 目录](README.md)
