# 第 65 章：为什么接口返回 404 时，fetch 不会进入 catch？

前置：[第 64 章：请求尚未完成时，如何显示加载提示？](第064章-请求尚未完成时，如何显示加载提示.md)

## 一、本章要解决的问题

第 63 章在 Network（网络）面板中看到：练习服务返回 500 时，页面提示的却是“请确认练习服务已经启动”。服务明明在运行，这个提示会让人往错误的方向排查。

本章先做一个实验，看看去掉 `response.ok` 的检查会发生什么，弄清 `fetch` 什么时候才算“失败”；然后把两种失败分开：连不上服务器时提示检查练习服务，服务器返回错误状态时说明状态码。

下图都是服务器返回状态 500 时的页面，左边是现在的提示，右边是本章完成后的提示：

![错误提示前后对比](images/第065章-错误提示前后对比.png)

## 二、必要原理

### fetch 只在“没有收到响应”时失败

`fetch` 返回的 Promise 只在**没有收到任何响应**时失败，例如服务器没有启动、网络断开、地址的域名不存在。这时 `await fetch(...)` 抛出错误，程序进入 `catch`。

只要服务器回了响应，不管状态码是 200、404 还是 500，`fetch` 都算成功完成。404 表示“服务器收到了请求，告诉你找不到”，这本身也是一个正常的回答。所以 404、500 都**不会**自动进入 `catch`。

### 用 response.ok 判断状态码

响应对象的 `response.ok` 在状态码是 200—299 时为 `true`，其他情况为 `false`；`response.status` 是具体的状态码。第 62 章就是用 `!response.ok` 判断后主动 `throw`，把错误状态也交给 `catch` 处理。

如果漏了这个判断会怎样？服务器出错时通常也返回一段 JSON，练习服务返回的是 `{ "message": "……" }`。`response.json()` 能正常读出这个对象，`as Article[]` 又不做任何检查，于是这个对象被当作文章数组存进 state。渲染时调用 `articles.map`，对象没有 `map` 方法，页面整个变成空白。

### 两种失败，两种提示

现在两种失败都进入 `catch`，只能显示同一句话。更好的做法是在各自发生的地方处理：

- 状态码不正常：在 `if (!response.ok)` 中直接设置错误提示，写明状态码，然后 `return` 结束函数。
- 连不上服务器：仍然由 `catch` 处理，提示检查练习服务。

`return` 写在 `try` 中时，`finally` 仍然会执行，所以加载提示照样会消失。

## 三、动手实现

### 第 1 步：实验，去掉状态检查

把 `src/pages/ArticlePage.tsx` 中的请求地址临时改为 `'http://127.0.0.1:3001/api/articles?fail=404'`，并临时删掉 `if (!response.ok) { … }` 这三行。

保存后点击“加载文章”：页面变成一片空白，控制台中有 `articles.map is not a function`。在 Network 面板中能看到这次请求的状态是 404，`fetch` 却没有进入 `catch`。

恢复删掉的三行，保留 `?fail=404`，继续下一步。

### 第 2 步：分别处理两种失败

把 `try` 和 `catch` 中的这几行：

```tsx
      if (!response.ok) {
        throw new Error('接口返回状态 ' + response.status);
      }
      const data = (await response.json()) as Article[];
      setArticles(data);
    } catch {
      setError('文章读取失败，请确认练习服务已经启动。');
```

改为：

```tsx
      if (!response.ok) {
        setError('文章读取失败：服务器返回了状态 ' + response.status + '。');
        return;
      }
      const data = (await response.json()) as Article[];
      setArticles(data);
    } catch {
      setError('无法连接练习服务，请确认它已经启动。');
```

保存后点击“加载文章”，页面显示“文章读取失败：服务器返回了状态 404。”。

最后把地址中的 `?fail=404` 删掉。

## 四、运行与检查

保持开发服务器和练习服务都在运行（准备篇二）。

1. 正常点击“加载文章”，文章显示正常。
2. 地址临时加上 `?fail=500`，提示“文章读取失败：服务器返回了状态 500。”，加载提示正常消失。看完后去掉。
3. 停止练习服务，点击加载，提示“无法连接练习服务，请确认它已经启动。”。
4. 重新启动练习服务，点击加载，错误提示消失，文章出现。

最后运行检查，应没有错误：

```powershell
npm.cmd run check
```

**小练习**：状态码是 404 时显示“找不到文章接口，请检查请求地址。”，其他状态码仍显示原来的提示。练习后恢复本章代码。

## 五、常见错误

| 现象 | 原因 | 怎么改 |
| --- | --- | --- |
| 服务器返回 404 后页面空白，控制台报 `articles.map is not a function` | 没有检查 `response.ok`，错误信息对象被当成文章数组 | 先判断 `response.ok`，不正常时不读取文章 |
| 状态码错误时，错误提示一闪而过，页面随即变成空白 | 设置错误提示后没有 `return`，继续把错误信息当作文章读取 | 设置提示后立即 `return` |
| 以为 `catch` 会处理 404，没有写状态检查 | 误以为 `fetch` 收到错误状态码就会失败 | 记住：只有收不到响应时 `fetch` 才失败 |
| 加了 `return` 后担心加载提示不消失 | `return` 会跳过后面的代码 | `finally` 在 `return` 之后仍会执行，不需要另外处理 |
| 学完本章后，页面一直报错、一直显示空数据或加载很慢 | 步骤中临时加在请求地址上的 `?mode=`、`?fail=`、`?delay=`、`?cors=` 等参数忘了删 | 运行 `git diff` 查看本章改动，删掉地址中的临时参数 |

## 六、本章完整代码

### `src/pages/ArticlePage.tsx`

```tsx
import { useState } from 'react';

type Article = {
  id: number;
  title: string;
};

export default function ArticlePage() {
  const [articles, setArticles] = useState<Article[]>([]);
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  async function handleLoad() {
    setError('');
    setIsLoading(true);
    try {
      const response = await fetch('http://127.0.0.1:3001/api/articles');
      if (!response.ok) {
        setError('文章读取失败：服务器返回了状态 ' + response.status + '。');
        return;
      }
      const data = (await response.json()) as Article[];
      setArticles(data);
    } catch {
      setError('无法连接练习服务，请确认它已经启动。');
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <main className="page">
      <h1>文章列表</h1>
      <button type="button" onClick={handleLoad} disabled={isLoading}>
        加载文章
      </button>
      {isLoading && <p role="status">正在加载文章……</p>}
      {error && <p role="alert">{error}</p>}
      <ul>
        {articles.map((article) => (
          <li key={article.id}>{article.title}</li>
        ))}
      </ul>
    </main>
  );
}
```

## 七、本章改动小结

修改 `src/pages/ArticlePage.tsx`：状态码不正常和连不上服务器分别显示不同的提示。

---

下一章：[第 66 章：请求失败后，如何让用户点击重试？](第066章-请求失败后，如何让用户点击重试.md)
