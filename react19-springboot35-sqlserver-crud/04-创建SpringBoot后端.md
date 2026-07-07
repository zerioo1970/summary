# 第 4 章 · 第二步：用 IDEA + Gradle 创建 Spring Boot 3.5 后端

> 上一章：[03-SQLServer建库建表](03-SQLServer建库建表.md) ｜ 下一章：[05-数据库连接与MyBatisFlex配置](05-数据库连接与MyBatisFlex配置.md)

## 4.1 用 Spring Initializr 生成骨架

在 IDEA 2026：**File → New → Project → Spring Boot（Spring Initializr）**，填写：

| 选项 | 值 |
|------|----|
| Language | Java |
| Type（构建工具） | **Gradle - Groovy**（或 Kotlin DSL，本教程用 Groovy） |
| Group | `com.example` |
| Artifact | `crud-backend` |
| Java | 17（或 21） |
| Spring Boot | 3.5.x |

**Dependencies（依赖）** 只勾选：
- **Spring Web**（提供 REST、内嵌 Tomcat）
- **Lombok**（可选，少写 getter/setter）

> MyBatis-Flex 和 SQL Server 驱动 Initializr 里没有内置选项，我们下一节手动加到 `build.gradle`。

点击 **Create**，IDEA 会自动下载 Gradle 依赖并生成如下结构：

```
crud-backend/
├── build.gradle              ← 依赖与构建脚本（重点）
├── settings.gradle
├── gradlew / gradlew.bat     ← Gradle Wrapper，无需本地装 Gradle
├── gradle/wrapper/...
└── src/
    ├── main/
    │   ├── java/com/example/crudbackend/
    │   │   └── CrudBackendApplication.java   ← 启动类
    │   └── resources/
    │       └── application.yml               ← 配置文件（默认是 .properties，可改成 .yml）
    └── test/
```

## 4.2 完整的 build.gradle

打开 `build.gradle`，替换成下面内容（**注意版本号**，MyBatis-Flex 用 `mybatis-flex-spring-boot3-starter`，专为 Spring Boot 3 适配）：

```groovy
plugins {
    id 'java'
    id 'org.springframework.boot' version '3.5.3'
    id 'io.spring.dependency-management' version '1.1.6'
}

group = 'com.example'
version = '0.0.1-SNAPSHOT'

java {
    sourceCompatibility = '17'
}

repositories {
    // 国内建议加阿里云镜像，下载更快
    maven { url 'https://maven.aliyun.com/repository/public' }
    mavenCentral()
}

// 统一管理 MyBatis-Flex 版本
ext {
    mybatisFlexVersion = '1.11.0'
}

dependencies {
    // --- Web ---
    implementation 'org.springframework.boot:spring-boot-starter-web'

    // --- MyBatis-Flex（Spring Boot 3 专用 starter）---
    implementation "com.mybatis-flex:mybatis-flex-spring-boot3-starter:${mybatisFlexVersion}"

    // --- MyBatis-Flex APT 处理器：编译期生成 XxxTableDef 类型安全字段 ---
    annotationProcessor "com.mybatis-flex:mybatis-flex-processor:${mybatisFlexVersion}"

    // --- SQL Server JDBC 驱动 ---
    runtimeOnly 'com.microsoft.sqlserver:mssql-jdbc:12.8.1.jre11'

    // --- 数据库连接池（HikariCP，Spring Boot 默认自带，可省略显式声明）---

    // --- Lombok（可选）---
    compileOnly 'org.projectlombok:lombok'
    annotationProcessor 'org.projectlombok:lombok'

    // --- 测试 ---
    testImplementation 'org.springframework.boot:spring-boot-starter-test'
}

tasks.named('test') {
    useJUnitPlatform()
}
```

**关键依赖讲解**：

| 依赖 | 作用 |
|------|------|
| `mybatis-flex-spring-boot3-starter` | MyBatis-Flex 与 Spring Boot 3 的自动装配，帮你自动配置 `SqlSessionFactory`、扫描 Mapper |
| `mybatis-flex-processor`（annotationProcessor） | **编译期注解处理器**。它会为每个带 `@Table` 的实体生成一个 `实体名Table` 或 `实体名TableDef` 类（如 `UserTableDef.USER`），让你在写 `QueryWrapper` 时用 `USER.USERNAME.eq(...)` 这种类型安全、可重构的写法，而不是手写字符串列名 |
| `mssql-jdbc` | 微软官方 SQL Server JDBC 驱动。`jre11` 后缀表示适配 Java 11+，可用于 JDK 17/21 |
| `runtimeOnly` | 表示这个 jar 只在运行时需要（编译时不引用它的类），驱动类正合适 |

改完后点击 IDEA 右上角的 **Gradle 刷新（大象图标 / Load Gradle Changes）**，等待依赖下载完成。

> **不用死记依赖坐标**：绿色的 Spring/Lombok/Test 部分由 Spring Initializr 自动生成；mybatis-flex、mssql-jdbc 这些专用依赖可在 `dependencies {}` 里输入关键字后按 `Ctrl+Space` 让 IDEA 自动补全坐标和版本，或去 [mvnrepository.com](https://mvnrepository.com) 复制现成的 Gradle 片段。你真正要理解的只是 `implementation / runtimeOnly / annotationProcessor` 这几个关键字的含义。

---

> 下一章 👉 [05-数据库连接与MyBatisFlex配置](05-数据库连接与MyBatisFlex配置.md)
