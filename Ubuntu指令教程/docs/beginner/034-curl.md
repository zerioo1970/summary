# 034 curl：发送 HTTP 请求

> 级别：初级 · 软件包：`curl`（Ubuntu 26.04 自带 8.18 版）

## 1. 作用

`curl` 向网址发送请求，并把服务器返回的内容输出到屏幕。它可以发 GET、POST 等各种请求，带请求头、带 JSON 数据，所以是测试 Web 接口最常用的工具。它也能下载文件。

后端写好一个 REST 接口，在服务器上用 curl 调一下，就能确认“接口本身是好的”，再去排查前端或网络。

## 2. 基本格式

```bash
curl [参数] 网址
```

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-o 文件名` | `--output` | 把返回内容保存到文件 |
| `-O` | `--remote-name` | 按网址里的文件名保存 |
| `-L` | `--location` | 自动跟随跳转（301、302） |
| `-I` | `--head` | 只看响应头 |
| `-s` | `--silent` | 不显示进度信息 |
| `-S` | `--show-error` | 和 `-s` 一起用，出错时仍显示错误 |
| `-f` | `--fail` | HTTP 状态码 ≥ 400 时返回非 0 退出码 |
| `-X 方法` | `--request` | 指定请求方法：POST、PUT、DELETE |
| `-H "头: 值"` | `--header` | 添加请求头 |
| `-d '数据'` | `--data` | 发送请求体（默认用 POST） |
| `-m 秒` | `--max-time` | 整个请求的超时时间 |
| `-w "格式"` | `--write-out` | 请求完成后输出指定信息，如状态码、耗时 |

## 4. 常用示例

### 4.1 获取网页内容

```
$ curl -s https://example.com | grep -o "<title>.*</title>"
<title>Example Domain</title>
```

curl 把内容输出到屏幕，可以直接接 grep 处理。

### 4.2 `-I`：只看响应头

```
$ curl -sI https://example.com
HTTP/2 200
date: Thu, 29 Oct 2026 06:55:39 GMT
content-type: text/html; charset=utf-8
server: cloudflare
last-modified: Sat, 24 Oct 2026 11:34:40 GMT
...
```

第一行的 `200` 是状态码。检查网站是否在线、证书是否正常、走没走 CDN，用 `-I` 就够了。

### 4.3 `-w`：只要状态码和耗时

```
$ curl -s -o /dev/null -w "%{http_code} %{time_total}s\n" https://example.com
200 0.029388s
```

`-o /dev/null` 丢掉网页内容，只输出状态码和总耗时。写健康检查脚本常用。

### 4.4 带参数的 GET 请求

```
$ curl -s "https://httpbin.org/get?item=U001"
{
  "args": {
    "item": "U001"
  },
  "headers": {
    "Accept": "*/*",
    "Host": "httpbin.org",
    "User-Agent": "curl/8.18.0"
  },
  "method": "GET",
  "origin": "（你的公网 IP）",
  "url": "https://httpbin.org/get?item=U001"
}
```

httpbin.org 是一个把收到的请求原样返回的测试网站，适合练习。

### 4.5 POST 一段 JSON

```
$ curl -s -X POST -H "Content-Type: application/json" -d '{"code":"U001","price":12000}' https://httpbin.org/post
{
  "args": {},
  "headers": {
    "Accept": "*/*",
    "Content-Length": "29",
    "Content-Type": "application/json",
    "Host": "httpbin.org",
    "User-Agent": "curl/8.18.0"
  },
  "method": "POST",
  "origin": "（你的公网 IP）",
  "url": "https://httpbin.org/post",
  "data": "{\"code\":\"U001\",\"price\":12000}",
  "files": {},
  "form": {},
  "json": {
    "code": "U001",
    "price": 12000
  }
}
```

测试自己的接口时，把网址换成 `http://localhost:8080/api/products` 这类地址即可。JSON 要用单引号括起来，因为里面有双引号。

### 4.6 下载文件

```
$ curl -sSLO https://cdimage.ubuntu.com/ubuntu-base/releases/26.04.1/release/SHA256SUMS

$ ls -l SHA256SUMS
-rw-r--r-- 1 tom tom 1244 Oct 29 06:55 SHA256SUMS
```

`-L` 跟随跳转，`-O` 按原文件名保存。

### 4.7 常见错误

```
$ curl -sS https://nothere.invalid
curl: (6) Could not resolve host: nothere.invalid

$ curl -sS -m 3 http://10.255.255.1
curl: (28) Connection timed out after 3002 milliseconds

$ curl -fsS https://cdimage.ubuntu.com/nothere.txt; echo "退出码=$?"
curl: (22) The requested URL returned error: 404
退出码=22
```

| 错误码 | 含义 |
|---|---|
| 6 | 域名解析失败 |
| 7 | 连接被拒绝：主机在线，但端口上没有服务 |
| 28 | 超时 |
| 22 | 加了 `-f` 时，服务器返回了 4xx/5xx |

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 返回 404 页面，脚本却当成成功了 | 不加 `-f` 时，只要拿到响应，curl 的退出码就是 0 | 脚本里用 `curl -fsS` |
| 输出里夹着一大段 `% Total % Received` 进度表 | 输出接到管道或文件时，curl 会显示进度 | 加 `-s`；想保留错误信息再加 `-S` |
| 下载下来的文件只有几百字节，内容是 `301 Moved` | 网址发生了跳转，curl 默认不跟随 | 加 `-L` |
| 访问内网 https 地址报 `SSL certificate problem` | 证书是自签的，系统不信任 | 把公司的根证书装进系统；只在测试时临时用 `-k` 跳过校验 |
| `-d` 发的 JSON 后端收不到 | 没加 `Content-Type: application/json`，后端按表单解析 | 加 `-H "Content-Type: application/json"`，或用 `--json '数据'`（curl 7.82 起支持） |

## 6. 一句话总结

测接口用 `curl -s`，看状态用 `-I` 或 `-w`，脚本里加 `-fsS`，下载加 `-LO`。

---

[← 上一个：wget](033-wget.md) · [返回目录](../../README.md) · [下一个：tar →](035-tar.md)
