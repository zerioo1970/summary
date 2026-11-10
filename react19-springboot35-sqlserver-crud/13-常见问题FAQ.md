# 第 13 章 · 常见问题排查（FAQ）

> 上一章：[12-前后端联调与数据流](12-前后端联调与数据流.md) ｜ 回到：[README 目录](README.md)

| 现象 | 原因 | 解决 |
|------|------|------|
| `The driver could not establish a secure connection ... PKIX` | mssql-jdbc 12+ 默认加密但无证书 | URL 加 `encrypt=true;trustServerCertificate=true` |
| `Login failed for user 'sa'` | 未开混合验证 / 密码错 / sa 被禁用 | SSMS 开启混合模式，启用并重设 sa 密码 |
| `TCP/IP connection to host localhost failed` | SQL Server TCP/IP 未开或端口不对 | 配置管理器开 TCP/IP，端口设 1433，重启服务 |
| 前端报 `blocked by CORS policy` | 后端未放行跨域 | 检查 `CorsConfig` 的 `allowedOrigins` 是否等于前端实际地址 |
| 找不到 `UserTableDef` / `table` 包 | APT 处理器还没跑 | 先 `Build Project` 或 `./gradlew compileJava`，并确认 `annotationProcessor` 依赖已加 |
| 中文乱码 | 表字段不是 `NVARCHAR` 或未加 `N'...'` | 用 `NVARCHAR`，连接串可加 `sendStringParametersAsUnicode=true`（驱动默认已是） |
| 时间字段报错 | 类型不匹配 | 实体用 `LocalDateTime`，表用 `DATETIME` |
| 端口被占用 `Port 8080 already in use` | 已有进程占用 | 改 `server.port` 或结束占用进程 |

---

## 🎉 结语

到这里你已经拥有一条完整、可运行的全栈 CRUD 链路：

> React 页面 → axios → Spring Boot REST 接口 → Service → MyBatis-Flex Mapper → SQL Server → 原路返回渲染。

**后续可扩展方向**：分页查询（MyBatis-Flex 的 `paginate`）、参数校验（`@Valid`）、登录鉴权（Spring Security + JWT）、前端换 Ant Design 美化界面、Docker 部署等。

> 回到 👉 [README 目录](README.md)
