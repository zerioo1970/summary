# 第 83 章：如何把防抖逻辑提取成自定义 Hook？

前置：[第 82 章：如何在 Effect 中读取最新的值，又不让 Effect 重新执行？](第082章-如何在Effect中读取最新的值，又不让Effect重新执行.md)

## 一、本章要解决的问题

搜索页中的防抖用了一个 state 和一个 Effect，共 9 行。以后商品搜索、人员搜索都需要同样的防抖，每次都复制这 9 行，既啰嗦，也容易在某一处漏掉清理函数。

组件可以把一段界面提取出来复用（第 8 章：把一段界面提取成组件），Hook 也一样。本章把防抖提取成自定义 Hook `useDebouncedValue`，搜索页只需要写一行：

```tsx
const debouncedKeyword = useDebouncedValue(keyword, 500);
```

## 二、必要原理

### 自定义 Hook 就是一个调用了 Hook 的函数

自定义 Hook 是一个普通的函数，特别之处只有两点：

- 名称以 `use` 开头，例如 `useDebouncedValue`。React 和代码检查据此判断它是一个 Hook，并按 Hook 的规则检查它。
- 函数中可以调用 `useState`、`useEffect` 等其他 Hook。

原来写在组件中的 state 和 Effect，原样搬进这个函数，再把组件需要的结果 `return` 出来。

### 参数和返回值

防抖需要知道两件事：要防抖的值、等待的毫秒数，作为参数传进来。它给出的是防抖后的值，作为返回值。

```ts
export function useDebouncedValue(value: string, delay: number) {
  const [debouncedValue, setDebouncedValue] = useState(value);
  // ……原来的 Effect，依赖变成 [value, delay]
  return debouncedValue;
}
```

`delay` 也在 Effect 中用到了，所以要写进依赖数组。

初始值从原来的 `''` 改成了 `value`：页面打开时，防抖后的值和传入的值一致，不需要等 0.5 秒。搜索页的关键词一开始就是空字符串，效果和原来相同。

### 每次调用都有自己的 state

自定义 Hook 复用的是**逻辑**，不是数据。两个组件各自调用 `useDebouncedValue`，或者同一个组件调用两次，每次调用都有自己独立的 state 和 Effect，互不影响。这和在组件中写两次 `useState` 一样。

### Hook 的调用规则同样适用

自定义 Hook 中调用了 Hook，所以它自己也要遵守 Hook 的规则：只在组件或其他自定义 Hook 的最上层调用，不能写在条件判断、循环或事件处理函数中。代码检查的 `react/rules-of-hooks` 规则会检查这一点。

### 放在 src/hooks 中

多个组件共用的自定义 Hook 放在 `src/hooks` 文件夹中，一个 Hook 一个文件，文件名和 Hook 同名。

## 三、动手实现

### 第 1 步：新建自定义 Hook

新建文件夹 `src/hooks`，在其中新建 `useDebouncedValue.ts`：

```ts
import { useEffect, useState } from 'react';

export function useDebouncedValue(value: string, delay: number) {
  const [debouncedValue, setDebouncedValue] = useState(value);

  useEffect(() => {
    const timerId = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);
    return () => {
      clearTimeout(timerId);
    };
  }, [value, delay]);

  return debouncedValue;
}
```

### 第 2 步：搜索页改用自定义 Hook

在 `src/pages/SearchPage.tsx` 中，在 `ArticleList` 的导入下面加入：

```tsx
import { useDebouncedValue } from '../hooks/useDebouncedValue';
```

删除 `debouncedKeyword` 的 `useState` 那一行，以及整个防抖的 `useEffect`。在 `searchState` 的下面加入：

```tsx
  const debouncedKeyword = useDebouncedValue(keyword, 500);
```

搜索的 Effect 不需要任何修改，它仍然依赖 `debouncedKeyword`。

### 第 3 步：让 App 显示搜索页

第 81、82 章 `App` 显示的是文章页。把 `src/App.tsx` 改为：

```tsx
import './App.css';
import SearchPage from './pages/SearchPage';

export default function App() {
  return <SearchPage />;
}
```

## 四、运行与检查

保持开发服务器和练习服务都在运行（准备篇二）。

1. 快速输入 `type`，Network（网络）中只有一个 `search` 请求，结果正常，和第 79 章完全一样。
2. 按第 78 章的方式输入 `t`、停顿、再输入 `ype`，旧请求被取消，最后只显示“TypeScript 类型检查”。
3. 在 React 开发者工具中选中 `SearchPage`，hooks 列表中出现 `DebouncedValue`，展开能看到它里面的 state。
4. `SearchPage.tsx` 中不再直接使用 `setTimeout`。

最后运行检查，应没有错误：

```powershell
npm.cmd run check
```

**小练习**：在搜索页中再调用一次 `useDebouncedValue(keyword, 1500)`，把结果显示在输入框下方，例如“1.5 秒防抖后的值：……”，观察两个值各自跟上输入的时间。练习后恢复本章代码。

## 五、常见错误

| 现象 | 原因 | 怎么改 |
| --- | --- | --- |
| `check` 提示不能在这里调用 Hook | 函数名没有以 `use` 开头，例如写成了 `debouncedValue` | 改名为 `useDebouncedValue` |
| `check` 提示 Hook 不能在条件中调用 | 写成了 `if (keyword) { useDebouncedValue(…) }` | 在组件最上层无条件调用 |
| 修改等待时间后没有效果 | Hook 的 Effect 依赖中漏了 `delay` | 依赖写 `[value, delay]` |
| 编辑器提示找不到模块 | 导入路径写错，或文件不在 `src/hooks` 中 | 路径是 `../hooks/useDebouncedValue`，不写 `.ts` |
| 删掉防抖的 Effect 后，`setDebouncedKeyword` 报未定义 | 只删了一部分，旧的代码还在调用它 | 删除原来的 `useState` 和整个防抖 Effect |

## 六、本章完整代码

### `src/hooks/useDebouncedValue.ts`

内容同第三节第 1 步。

### `src/pages/SearchPage.tsx`

```tsx
import { useEffect, useState } from 'react';
import type { ChangeEvent } from 'react';
import ArticleList from '../components/ArticleList';
import { useDebouncedValue } from '../hooks/useDebouncedValue';
import { apiUrl } from '../lib/api';
import { isArticle } from '../types/article';
import type { Article } from '../types/article';
import styles from './SearchPage.module.css';

type SearchState =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; items: Article[] }
  | { status: 'error'; message: string };

async function fetchSearchResults(query: string, signal: AbortSignal): Promise<Article[]> {
  let response: Response;
  try {
    response = await fetch(apiUrl('/search?q=' + encodeURIComponent(query)), { signal });
  } catch {
    throw new Error('无法连接练习服务，请确认它已经启动。');
  }
  if (!response.ok) {
    throw new Error('搜索失败：服务器返回了状态 ' + response.status + '。');
  }
  const data: unknown = await response.json();
  if (
    typeof data !== 'object' ||
    data === null ||
    !('items' in data) ||
    !Array.isArray(data.items) ||
    !data.items.every(isArticle)
  ) {
    throw new Error('搜索结果格式不正确，暂时无法显示。');
  }
  return data.items;
}

export default function SearchPage() {
  const [keyword, setKeyword] = useState('');
  const [searchState, setSearchState] = useState<SearchState>({ status: 'idle' });
  const debouncedKeyword = useDebouncedValue(keyword, 500);

  useEffect(() => {
    const query = debouncedKeyword.trim();
    if (query === '') {
      setSearchState({ status: 'idle' });
      return;
    }
    setSearchState({ status: 'loading' });
    const controller = new AbortController();

    async function search() {
      try {
        const items = await fetchSearchResults(query, controller.signal);
        setSearchState({ status: 'success', items });
      } catch (error) {
        if (controller.signal.aborted) {
          return;
        }
        const message = error instanceof Error ? error.message : '搜索失败。';
        setSearchState({ status: 'error', message });
      }
    }

    search();
    return () => {
      controller.abort();
    };
  }, [debouncedKeyword]);

  function handleKeywordChange(event: ChangeEvent<HTMLInputElement>) {
    setKeyword(event.target.value);
  }

  return (
    <main className="page">
      <h1>搜索文章</h1>
      <div className={styles.searchBox}>
        <label htmlFor="search-keyword">关键词</label>
        <input
          id="search-keyword"
          type="search"
          value={keyword}
          onChange={handleKeywordChange}
        />
      </div>
      {searchState.status === 'idle' && <p className={styles.hint}>输入关键词，搜索文章标题。</p>}
      {searchState.status === 'loading' && <p role="status">正在搜索……</p>}
      {searchState.status === 'error' && <p role="alert">{searchState.message}</p>}
      {searchState.status === 'success' &&
        (searchState.items.length === 0 ? (
          <p className={styles.hint}>没有找到相关文章。</p>
        ) : (
          <ArticleList articles={searchState.items} />
        ))}
    </main>
  );
}
```

### `src/App.tsx`

内容同第三节第 3 步。

## 七、本章改动小结

新增 `src/hooks/useDebouncedValue.ts`；修改 `src/pages/SearchPage.tsx`：防抖改用自定义 Hook；修改 `src/App.tsx`：改为显示搜索页。

---

下一章：[第 84 章：【组合】如何保证搜索结果始终对应最后输入的关键词？](第084章-如何保证搜索结果始终对应最后输入的关键词.md)
