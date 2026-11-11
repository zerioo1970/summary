# 第 82 章：如何在 Effect 中读取最新的值，又不让 Effect 重新执行？

前置：[第 81 章：离开页面时，如何清理尚未完成的定时任务？](第081章-离开页面时，如何清理尚未完成的定时任务.md)

## 一、本章要解决的问题

上一章的自动刷新是“悄悄”进行的：刷新时页面没有任何提示，刷新失败也不告诉用户。手动点击“重新加载”的效果则完整得多：显示“正在更新……”，失败时显示错误提示和“重试”，并保留原来的列表。

最直接的想法是让定时任务也调用 `handleReload`。可是这样做会遇到一个两难的问题：不把 `handleReload` 写进依赖，它读到的是旧的值；写进依赖，定时任务又会不断地被清理、重建。本章先看清这两个问题，再用 React 的 `useEffectEvent` 解决：定时任务能读到最新的值，Effect 又只在勾选状态变化时执行。

## 二、必要原理

### 每次渲染都有自己的一份变量和函数

先回顾第 25、28 章接触过的现象，它是理解本章的关键。

组件函数每渲染一次，就从头执行一次。函数体中的 `const articles = …`、`function handleReload() { … }`，每次执行都会**重新创建**一份。可以把每次渲染想成拍一张照片：照片里的 `articles` 是拍照那一刻的值，之后 state 变了，React 会拍一张新照片，但旧照片里的内容不会改变。

在某次渲染中创建的函数，读到的也是**那一次**渲染中的变量。例如第 3 次渲染时列表是空的，这次渲染创建的 `handleReload` 无论什么时候被调用，读到的 `articles` 都是那个空数组，即使后来列表已经有了 4 篇文章。

平时这不是问题：每次点击按钮时，调用的都是最新一次渲染中的 `handleReload`。但如果把某一次渲染中的函数**保存起来，过一会儿再调用**，比如交给 `setInterval`，它读到的就是旧照片里的值。

### 问题一：不写依赖，读到旧的值

`handleReload` 中用到了 `articles`，用来在加载和失败时保留原有列表。每次渲染都会创建一个新的 `handleReload`，它读到的是这一次渲染的 `articles`。

如果 Effect 中直接调用 `handleReload`，却不把它写进依赖，Effect 只在勾选时执行一次，交给 `setInterval` 的就永远是**勾选那一刻**那次渲染中的 `handleReload`，读到的也是那一刻的 `articles`，正是上一节说的“旧照片”。如果勾选时列表还是空的（例如练习服务刚好没启动），之后每次自动刷新，都会以为“没有文章”，列表先消失、显示“正在加载文章……”，再重新出现。

代码检查也会报错：Effect 缺少依赖 `handleReload`。

### 问题二：写进依赖，Effect 不断重新执行

按代码检查的要求把 `handleReload` 写进依赖。可它每次渲染都是一个新函数，于是每次渲染后 Effect 都重新执行：清理旧的定时任务，再开始一个新的。每次刷新本身就会引起两次渲染，定时任务总是在刚开始计时就被重建，行为难以预料。

### useEffectEvent：从 Effect 中分离出来的“事件”

问题的根源在于：定时任务中做的事情分成两部分。

- “每 5 秒触发一次”取决于 `autoRefresh`，它变化时 Effect 应该重新执行。
- “触发时怎样刷新”要用最新的 `articles`，但它变化时不应该重新开始计时。

React 19.2 加入的 `useEffectEvent` 用来写第二部分：

```tsx
const onAutoRefresh = useEffectEvent(() => {
  handleReload();
});
```

它返回一个函数，叫 Effect 事件。调用它时，执行的总是**最新一次渲染**中的那段代码，所以 `handleReload` 和 `articles` 都是最新的。Effect 事件不是依赖，不写进依赖数组，Effect 仍然只依赖 `autoRefresh`。

`useEffectEvent` 有两条规则：只能在 Effect 中调用它返回的函数，不要在事件处理函数或渲染时调用；也不要把它传给子组件。

## 三、动手实现

保持开发服务器和练习服务都在运行（准备篇二）。

### 第 1 步：重现读到旧值的问题

把 `src/pages/ArticlePage.tsx` 中 `setInterval` 的整个函数临时改为：

```tsx
    const timerId = setInterval(() => {
      handleReload();
    }, 5000);
```

停止练习服务，刷新页面，页面显示错误提示，没有文章。勾选“每 5 秒自动刷新”，然后启动练习服务，什么时候启动都可以。启动后的下一次自动刷新，文章出现；再下一次自动刷新时，列表消失，显示“正在加载文章……”，然后又出现。之后每次自动刷新都会这样闪一下：定时任务每次调用的，都是勾选那一刻、列表还是空的那次渲染中的 `handleReload`。

运行 `npm.cmd run check`，提示 Effect 缺少依赖 `handleReload`。

### 第 2 步：用 Effect 事件包装刷新

把第一行的导入改为：

```tsx
import { useEffect, useEffectEvent, useState } from 'react';
```

在自动刷新的 `useEffect` 前面加入：

```tsx
  const onAutoRefresh = useEffectEvent(() => {
    handleReload();
  });
```

把 `setInterval` 中的函数改为调用它：

```tsx
    const timerId = setInterval(() => {
      onAutoRefresh();
    }, 5000);
```

依赖数组保持 `[autoRefresh]`。

## 四、运行与检查

1. 按第三节第 1 步的方式操作：停止练习服务、刷新、勾选、启动练习服务。文章出现后，之后每次自动刷新，列表都保留，上方短暂显示“正在更新……”。
2. 自动刷新期间停止练习服务：5 秒内出现错误提示和“重试”，下面的列表仍然保留。重新启动后，下一次自动刷新时错误提示消失。
3. 取消勾选后，Network（网络）中不再出现新的 `articles` 请求。
4. `npm.cmd run check` 没有错误。

最后运行检查，应没有错误：

```powershell
npm.cmd run check
```

**小练习**：在 `onAutoRefresh` 中、`handleReload()` 的前面加一行 `console.log('当前文章数', articles.length);`，按第 1 项操作，观察打印出的数字是否随列表变化。练习后恢复本章代码。

## 五、常见错误

| 现象 | 原因 | 怎么改 |
| --- | --- | --- |
| 自动刷新时列表总是闪一下 | 定时任务中直接调用了某次渲染中的 `handleReload`，读到的是旧的 `articles` | 用 `useEffectEvent` 包装后再调用 |
| `check` 提示缺少依赖，补上以后定时任务不断重建 | 把每次渲染都会变化的函数写进了依赖 | 把这部分逻辑放进 Effect 事件，依赖只保留 `autoRefresh` |
| 编辑器提示 `react` 没有导出 `useEffectEvent` | React 版本低于 19.2 | 使用准备篇一锁定的 React 19 版本 |
| 把 `onAutoRefresh` 写进了依赖数组 | 误以为它和普通函数一样需要写 | Effect 事件不写进依赖数组 |

## 六、本章完整代码

### `src/pages/ArticlePage.tsx`

```tsx
import { useEffect, useEffectEvent, useState } from 'react';
import ArticleList from '../components/ArticleList';
import { apiUrl } from '../lib/api';
import { isArticle } from '../types/article';
import type { Article } from '../types/article';
import styles from './ArticlePage.module.css';

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
  const [autoRefresh, setAutoRefresh] = useState(false);
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

  const onAutoRefresh = useEffectEvent(() => {
    handleReload();
  });

  useEffect(() => {
    if (!autoRefresh) {
      return;
    }
    const timerId = setInterval(() => {
      onAutoRefresh();
    }, 5000);
    return () => {
      clearInterval(timerId);
    };
  }, [autoRefresh]);

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
      <div className={styles.toolbar}>
        <button type="button" onClick={handleReload} disabled={loadState.status === 'loading'}>
          重新加载
        </button>
        <label>
          <input
            type="checkbox"
            checked={autoRefresh}
            onChange={(event) => setAutoRefresh(event.target.checked)}
          />
          每 5 秒自动刷新
        </label>
      </div>
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

修改 `src/pages/ArticlePage.tsx`：自动刷新改为通过 Effect 事件调用 `handleReload`，显示加载提示并在失败时保留列表。

---

下一章：[第 83 章：如何把防抖逻辑提取成自定义 Hook？](第083章-如何把防抖逻辑提取成自定义Hook.md)
