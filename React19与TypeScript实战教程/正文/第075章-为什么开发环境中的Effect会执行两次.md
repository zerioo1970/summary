# 第 75 章：为什么开发环境中的 Effect 会执行两次？

前置：[第 74 章：如何在打开页面时自动读取数据？](第074章-如何在打开页面时自动读取数据.md)

## 一、本章要解决的问题

上一章在 Network（网络）面板中看到，刷新文章页时 `articles` 请求出现了两次。依赖数组明明是 `[]`，说好的“只执行一次”呢？是代码写错了，还是需要想办法阻止？

本章先确认 Effect 确实执行了两次，再弄清这是 React 在开发环境中**故意**做的检查。然后给 Effect 加上**清理函数**，让第一次请求的结果被丢弃。这个写法以后的每个读取数据的 Effect 都会用到。

## 二、必要原理

### StrictMode 会故意多执行一次

打开 `src/main.tsx`，`<App />` 外面包着 `<StrictMode>`，这是准备篇一创建项目时就写好的。在开发环境中，StrictMode 会在组件第一次显示到页面上时，把 Effect 依次执行三步：

1. 执行 Effect；
2. 执行 Effect 的**清理函数**，模拟组件离开页面；
3. 再执行一次 Effect，模拟组件重新回到页面。

所以请求发了两次。打包后的正式版本中，StrictMode 不做这个检查，Effect 只执行一次。

### 为什么要故意这样做

真实使用中，组件确实会离开页面又回来，例如切换到别的页面再切回来。如果 Effect 开始的事情没有收尾，就会留下问题：请求还没完成组件就离开了，结果回来时却更新了状态；定时器（第 81 章）在离开后仍然不停地执行……

StrictMode 在开发时就模拟一次“离开再回来”，让这类问题尽早暴露。正确的做法不是想办法阻止 Effect 执行两次，而是让 Effect 能被正确地清理：执行两次和执行一次，最后看到的结果应该完全一样。

### 清理函数

Effect 的函数可以**返回一个函数**，这个函数就是清理函数。React 在组件离开页面时调用它，在 Effect 重新执行之前也会先调用它：

```tsx
useEffect(() => {
  let ignore = false;
  // ……开始读取，读完后先检查 ignore
  return () => {
    ignore = true;
  };
}, []);
```

读取数据的 Effect 通常这样清理：用一个变量 `ignore` 记录“这次的结果还要不要”。清理时把它设为 `true`，请求回来后先检查，`ignore` 为 `true` 就什么都不做。

每次执行 Effect 都会创建一个新的 `ignore` 变量。第一次执行的 `ignore` 被它自己的清理函数设成了 `true`，所以第一个请求的结果被丢弃；第二次执行的 `ignore` 仍是 `false`，它的结果正常显示。

请求本身仍然发了两次。读取数据重复一次没有坏处，第 79 章会介绍怎样真正取消请求。

## 三、动手实现

### 第 1 步：确认 Effect 执行了两次

在 `src/pages/ArticlePage.tsx` 的 Effect 中，`async function loadOnOpen()` 的上面临时加一行：

```tsx
    console.log('读取文章的 Effect 执行了');
```

刷新页面，控制台中这句话出现两次。确认后删掉这一行。

### 第 2 步：加入清理函数

把整个 `useEffect(…)` 改为：

```tsx
  useEffect(() => {
    let ignore = false;

    async function loadOnOpen() {
      try {
        const data = await fetchArticles();
        if (!ignore) {
          setLoadState({ status: 'success', articles: data });
        }
      } catch (error) {
        if (!ignore) {
          setLoadState({ status: 'error', message: getErrorMessage(error), articles: [] });
        }
      }
    }

    loadOnOpen();
    return () => {
      ignore = true;
    };
  }, []);
```

成功和失败两处更新 state 之前都要检查 `ignore`。

## 四、运行与检查

保持开发服务器和练习服务都在运行（准备篇二）。

1. 刷新页面，文章正常显示。Network 中仍然有两个 `articles` 请求，这是开发环境的正常现象。
2. 在 `loadOnOpen` 中、`const data = await fetchArticles();` 的下一行临时加一行，注意要加在 `if (!ignore)` 的**外面**：

   ```tsx
           const data = await fetchArticles();
           console.log('显示结果', ignore);
           if (!ignore) {
   ```

   刷新页面，控制台先后出现 `显示结果 true` 和 `显示结果 false`：第一个请求的结果被丢弃了。如果加在了 `if (!ignore)` 的里面，只会看到一次 `显示结果 false`，因为 `ignore` 为 `true` 的那一次根本进不去。确认后删掉这一行。
3. 开发环境中出现两次，是 StrictMode 故意多执行的一次。上线后的正式版本不会这样，请求只有一个，第 137 章打包上线时可以验证。

最后运行检查，应没有错误：

```powershell
npm.cmd run check
```

**小练习**：暂时把 `src/main.tsx` 中的 `<StrictMode>` 和 `</StrictMode>` 删掉，刷新页面，看看请求还有几个。练习后恢复本章代码，以后不要删掉 StrictMode。

## 五、常见错误

| 现象 | 原因 | 怎么改 |
| --- | --- | --- |
| 为了只发一次请求，删掉了 StrictMode | 把检查当成了问题 | 保留 StrictMode，让 Effect 能被正确清理 |
| 加了 `ignore` 后页面一直显示“正在加载” | `ignore` 写在了 Effect 外面（组件函数中），两次执行共用一个变量，第二次的结果也被丢弃 | `let ignore = false;` 写在 Effect 的函数里面 |
| 编辑器提示 Effect 返回值的类型不对 | 写成了 `return ignore = true;`，返回的是布尔值而不是函数 | 写成 `return () => { ignore = true; };` |

## 六、本章完整代码

### `src/pages/ArticlePage.tsx`

```tsx
import { useEffect, useState } from 'react';
import ArticleList from '../components/ArticleList';
import { apiUrl } from '../lib/api';
import { isArticle } from '../types/article';
import type { Article } from '../types/article';

type LoadState =
  | { status: 'loading'; articles: Article[] }
  | { status: 'success'; articles: Article[] }
  | { status: 'error'; message: string; articles: Article[] };

async function fetchArticles(): Promise<Article[]> {
  let response: Response;
  try {
    response = await fetch(apiUrl('/articles'));
  } catch {
    throw new Error('无法连接练习服务，请确认它已经启动。');
  }
  if (!response.ok) {
    throw new Error('文章读取失败：服务器返回了状态 ' + response.status + '。');
  }
  const data: unknown = await response.json();
  if (!Array.isArray(data) || !data.every(isArticle)) {
    throw new Error('文章数据格式不正确，暂时无法显示。');
  }
  return data;
}

function getErrorMessage(error: unknown) {
  return error instanceof Error ? error.message : '文章读取失败。';
}

export default function ArticlePage() {
  const [loadState, setLoadState] = useState<LoadState>({ status: 'loading', articles: [] });
  const articles = loadState.articles;

  useEffect(() => {
    let ignore = false;

    async function loadOnOpen() {
      try {
        const data = await fetchArticles();
        if (!ignore) {
          setLoadState({ status: 'success', articles: data });
        }
      } catch (error) {
        if (!ignore) {
          setLoadState({ status: 'error', message: getErrorMessage(error), articles: [] });
        }
      }
    }

    loadOnOpen();
    return () => {
      ignore = true;
    };
  }, []);

  async function handleReload() {
    setLoadState({ status: 'loading', articles });
    try {
      const data = await fetchArticles();
      setLoadState({ status: 'success', articles: data });
    } catch (error) {
      setLoadState({ status: 'error', message: getErrorMessage(error), articles });
    }
  }

  return (
    <main className="page">
      <h1>文章列表</h1>
      <button type="button" onClick={handleReload} disabled={loadState.status === 'loading'}>
        重新加载
      </button>
      {loadState.status === 'loading' && (
        <p role="status">{articles.length === 0 ? '正在加载文章……' : '正在更新……'}</p>
      )}
      {loadState.status === 'error' && (
        <div>
          <p role="alert">{loadState.message}</p>
          <button type="button" onClick={handleReload}>
            重试
          </button>
        </div>
      )}
      {loadState.status === 'success' && articles.length === 0 && <p>暂时还没有文章。</p>}
      {articles.length > 0 && <ArticleList articles={articles} />}
    </main>
  );
}
```

## 七、本章改动小结

修改 `src/pages/ArticlePage.tsx`：读取文章的 Effect 加入清理函数，丢弃已被清理的那次请求的结果。

---

下一章：[第 76 章：如何在搜索关键词变化后重新请求数据？](第076章-如何在搜索关键词变化后重新请求数据.md)
