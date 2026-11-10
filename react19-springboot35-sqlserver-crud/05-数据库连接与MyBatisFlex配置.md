# 第 5 章 · 第三步：配置数据库连接与 MyBatis-Flex

> 上一章：[04-创建SpringBoot后端](04-创建SpringBoot后端.md) ｜ 下一章：[06-后端分层代码](06-后端分层代码.md)

## 5.1 application.yml

把 `src/main/resources/application.properties` 重命名为 `application.yml`，写入：

```yaml
server:
  port: 8080

spring:
  datasource:
    driver-class-name: com.microsoft.sqlserver.jdbc.SQLServerDriver
    url: jdbc:sqlserver://localhost:1433;databaseName=demo_db;encrypt=true;trustServerCertificate=true
    username: sa
    password: 你的密码
    # HikariCP 连接池参数（可选）
    hikari:
      maximum-pool-size: 10
      minimum-idle: 5

# MyBatis-Flex 配置
mybatis-flex:
  # Mapper XML 位置（本教程用注解/QueryWrapper，一般不需要 XML，可留空）
  mapper-locations: classpath*:/mapper/**/*.xml
  configuration:
    # 控制台打印执行的 SQL，方便调试
    log-impl: org.apache.ibatis.logging.stdout.StdOutImpl
```

**连接串（url）逐段讲解**——SQL Server 的 JDBC URL 是初学者最容易踩坑的地方：

| 片段 | 含义 |
|------|------|
| `jdbc:sqlserver://localhost:1433` | 协议 + 主机 + 端口（1433 是 SQL Server 默认端口） |
| `databaseName=demo_db` | 连接哪个数据库 |
| `encrypt=true` | mssql-jdbc 12.x 起**默认开启加密** |
| `trustServerCertificate=true` | 本地开发没有正式证书时必须加，否则会报 `SSL/TLS` 证书错误 |

> ⚠️ 常见报错：不加 `trustServerCertificate=true` 会出现 `The driver could not establish a secure connection ... PKIX path building failed`。本地开发直接信任即可。

## 5.2 在启动类上开启 Mapper 扫描

打开启动类 `CrudBackendApplication.java`，加上 `@MapperScan`：

```java
package com.example.crudbackend;

import org.mybatis.spring.annotation.MapperScan;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

@SpringBootApplication
@MapperScan("com.example.crudbackend.mapper")  // 扫描 Mapper 接口所在包
public class CrudBackendApplication {
    public static void main(String[] args) {
        SpringApplication.run(CrudBackendApplication.class, args);
    }
}
```

`@MapperScan` 告诉 MyBatis-Flex 去哪个包找 Mapper 接口并生成实现类（代理对象）。

---

> 下一章 👉 [06-后端分层代码](06-后端分层代码.md)
