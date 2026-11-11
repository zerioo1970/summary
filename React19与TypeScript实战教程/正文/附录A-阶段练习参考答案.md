# 附录 A：阶段练习参考答案

本附录收录各阶段练习的参考答案。请先独立完成练习，再对照答案。参考答案只是一种写法，只要满足验收标准，你的写法不同也没有关系。

## 第 23 章：课程列表页

### 思路

1. **数据**：新建 `src/data/courses.ts`，定义 `Course` 类型。授课老师可能没有，所以写成可选字段 `teacher?: string`（第 20 章）。
2. **布局**：复用第 16 章的 `Card` 组件和 `App.css` 中的 `.card-list`，卡片自动并排和换行（第 12、13 章）。
3. **列表**：用 `map` 生成卡片，`key` 使用课程编号（第 17、18 章）。
4. **默认值**：没有老师时用 `??` 显示“待定”（第 20 章）。
5. **日期**：用 `Intl.DateTimeFormat` 按北京时间显示（第 22 章）。
6. **空状态**：用条件运算符在提示和列表之间选择（第 19 章）。

### `src/data/courses.ts`

```ts
export type Course = {
  id: number;
  name: string;
  teacher?: string;
  lessons: number;
  startsAt: string;
};

export const courses: Course[] = [
  { id: 1, name: 'React 入门', teacher: '林老师', lessons: 12, startsAt: '2026-11-10T01:00:00Z' },
  { id: 2, name: 'TypeScript 基础', lessons: 8, startsAt: '2026-11-17T01:00:00Z' },
  { id: 3, name: '网页布局实战', teacher: '陈老师', lessons: 10, startsAt: '2026-11-24T06:00:00Z' },
];
```

### `src/pages/CoursePage.tsx`

```tsx
import Card from '../components/Card';
import { courses } from '../data/courses';

const dateFormatter = new Intl.DateTimeFormat('zh-CN', {
  year: 'numeric',
  month: 'long',
  day: 'numeric',
  timeZone: 'Asia/Shanghai',
});

export default function CoursePage() {
  return (
    <main className="page">
      <h1>课程列表</h1>
      {courses.length === 0 ? (
        <p>暂时没有开放的课程。</p>
      ) : (
        <section className="card-list">
          {courses.map((course) => (
            <Card key={course.id}>
              <h2>{course.name}</h2>
              <p>授课：{course.teacher ?? '待定'}</p>
              <p>共 {course.lessons} 课时</p>
              <p>
                开课日期：
                <time dateTime={course.startsAt}>
                  {dateFormatter.format(new Date(course.startsAt))}
                </time>
              </p>
            </Card>
          ))}
        </section>
      )}
    </main>
  );
}
```

`key` 写在 `map` 直接返回的 `Card` 上，而不是写在 `Card` 组件内部的 `article` 上（第 18 章）。

### `src/App.tsx`（练习期间）

```tsx
import './App.css';
import CoursePage from './pages/CoursePage';

export default function App() {
  return <CoursePage />;
}
```

### 对照验收标准

1. “TypeScript 基础”没有 `teacher`，显示“授课：待定”；其余两门显示老师姓名。
2. 宽度 375 时三张卡片上下排列。`Card` 中已有第 14 章的 `overflow-wrap: anywhere`，课程名再长也不会撑出屏幕。
3. 把 `courses` 改成 `[]`，页面显示“暂时没有开放的课程。”。
4. `npm.cmd run check` 没有错误。

练习完成后，把 `App.tsx` 恢复为第 23 章的内容（显示 `NewsPage`）。`courses.ts` 和 `CoursePage.tsx` 可以保留，不影响后续章节。

## 第 54 章：购物清单

### 思路

1. **数据**：每件物品是 `{ id, name, bought }`，所有物品放在一个 state 数组中（第 45 章）。
2. **添加**：受控输入框加表单提交，名称为空时禁用按钮；新物品用 `crypto.randomUUID()` 生成 `id`，用展开语法创建新数组（第 45 章）。
3. **删除**：用 `filter` 按 `id` 删除；删除的正好是正在编辑的物品时，关闭编辑框（第 46、47 章）。
4. **勾选**：复选框写在 `label` 中，用 `map` 加对象展开切换 `bought`（第 48 章）。
5. **修改与取消**：`editingId` 记住正在编辑哪一件，`editDraft` 保存草稿；保存时才写回数组，取消只需把 `editingId` 设为 `null`（第 47、52 章）。
6. **样式**：直接复用 `TaskPage.module.css`，类名的用法与待办事项页相同。

### `src/pages/ShoppingPage.tsx`

```tsx
import { useState } from 'react';
import type { SubmitEvent } from 'react';
import styles from './TaskPage.module.css';

type ShoppingItem = {
  id: string;
  name: string;
  bought: boolean;
};

export default function ShoppingPage() {
  const [items, setItems] = useState<ShoppingItem[]>([
    { id: 'item-1', name: '牛奶', bought: false },
    { id: 'item-2', name: '面包', bought: true },
  ]);
  const [newName, setNewName] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editDraft, setEditDraft] = useState('');

  function handleAdd(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    const name = newName.trim();
    if (name === '') {
      return;
    }
    setItems([...items, { id: crypto.randomUUID(), name, bought: false }]);
    setNewName('');
  }

  function handleDelete(id: string) {
    setItems(items.filter((item) => item.id !== id));
    if (id === editingId) {
      setEditingId(null);
    }
  }

  function handleToggle(id: string) {
    setItems(items.map((item) => (item.id === id ? { ...item, bought: !item.bought } : item)));
  }

  function handleStartEdit(item: ShoppingItem) {
    setEditingId(item.id);
    setEditDraft(item.name);
  }

  function handleSaveEdit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    const name = editDraft.trim();
    if (name === '') {
      return;
    }
    setItems(items.map((item) => (item.id === editingId ? { ...item, name } : item)));
    setEditingId(null);
  }

  return (
    <main className="page">
      <h1>购物清单</h1>
      <form className={styles.addForm} onSubmit={handleAdd}>
        <label htmlFor="new-item">要买的东西</label>
        <input id="new-item" value={newName} onChange={(event) => setNewName(event.target.value)} />
        <button type="submit" disabled={newName.trim() === ''}>
          添加
        </button>
      </form>
      <ul className={styles.list}>
        {items.map((item) => (
          <li key={item.id} className={styles.item}>
            <label className={item.bought ? styles.title + ' ' + styles.done : styles.title}>
              <input
                type="checkbox"
                checked={item.bought}
                onChange={() => handleToggle(item.id)}
              />
              {item.name}
            </label>
            <button type="button" onClick={() => handleStartEdit(item)}>
              编辑
            </button>
            <button
              type="button"
              aria-label={'删除：' + item.name}
              onClick={() => handleDelete(item.id)}
            >
              删除
            </button>
          </li>
        ))}
      </ul>
      {editingId !== null && (
        <form className={styles.addForm} onSubmit={handleSaveEdit}>
          <label htmlFor="edit-item">修改名称</label>
          <input
            id="edit-item"
            value={editDraft}
            onChange={(event) => setEditDraft(event.target.value)}
          />
          <button type="submit" disabled={editDraft.trim() === ''}>
            保存
          </button>
          <button type="button" onClick={() => setEditingId(null)}>
            取消
          </button>
        </form>
      )}
    </main>
  );
}
```

### `src/App.tsx`（练习期间）

```tsx
import './App.css';
import ShoppingPage from './pages/ShoppingPage';

export default function App() {
  return <ShoppingPage />;
}
```

### 对照验收标准

1. 添加、删除、勾选、修改都只影响操作的那一件物品，因为每次都按 `id` 判断。
2. 输入时只修改 `editDraft`，取消时物品名称没有被动过；再次编辑时 `handleStartEdit` 用当前名称覆盖草稿。
3. `handleDelete` 中判断 `id === editingId`，删除正在编辑的物品时关闭编辑框。
4. 所有更新都用展开语法、`filter`、`map` 创建新数组和新对象。
5. `npm.cmd run check` 没有错误。

练习完成后，把 `App.tsx` 恢复为第 54 章的内容（显示 `ContactsPage`）。`ShoppingPage.tsx` 可以保留，不影响后续章节。

## 第 93 章：任务进度页

### 思路

1. **读取**：`fetchTasks` 写在组件外面，打开页面时在 Effect 中调用，用 `ignore` 处理开发环境中执行两次的情况（第 74、75、85 章）。读取期间、读取失败分别显示提示，`finally` 中结束“读取中”（第 64 章）。
2. **保存完成状态**：勾选时发送 `PATCH`，正文只有 `completed`（第 87 章）。成功后用服务器返回的任务替换列表中的这一条，不重新读取整个列表（第 92 章）。
3. **保存期间禁用**：`savingIds` 记住正在保存的任务编号，复选框按编号判断是否禁用（第 89 章）。用数组而不是单个编号，是因为用户可能在第一条还没保存完时又勾选了第二条：只记一个编号的话，第一条保存结束时会把第二条的禁用也解除。添加和去掉编号都用更新函数，基于最新的数组计算（第 28、92 章）。
4. **失败时保持原状**：复选框是受控的，`checked` 来自 `task.completed`（第 38 章）。保存失败时没有更新 `tasks`，复选框自然显示原来的状态，不需要另外“恢复”。
5. **只看未完成**：`hideCompleted` 是一个 state，列表由 `tasks` 计算出来，不另外保存筛选结果（第 32、49 章）。
6. **样式**：复用 `TaskPage.module.css` 中的 `toolbar`、`list`、`item`、`title`、`done`。
7. **另一种写法**：如果做了第 92A 章的小练习，在 `taskApi.ts` 中写了 `updateTaskCompleted`，也可以直接导入 `fetchTasks` 和 `updateTaskCompleted`，页面中就不用自己写请求了。下面的参考答案没有依赖它，是一个完全独立的页面。

### `src/pages/TaskStatusPage.tsx`

```tsx
import { useEffect, useState } from 'react';
import { apiUrl } from '../lib/api';
import { isTask } from '../types/task';
import type { Task } from '../types/task';
import styles from './TaskPage.module.css';

async function fetchTasks(): Promise<Task[]> {
  let response: Response;
  try {
    response = await fetch(apiUrl('/tasks'));
  } catch {
    throw new Error('无法连接练习服务，请确认它已经启动。');
  }
  if (!response.ok) {
    throw new Error('任务读取失败：服务器返回了状态 ' + response.status + '。');
  }
  const data: unknown = await response.json();
  if (!Array.isArray(data) || !data.every(isTask)) {
    throw new Error('任务数据格式不正确，暂时无法显示。');
  }
  return data;
}

export default function TaskStatusPage() {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState('');
  const [hideCompleted, setHideCompleted] = useState(false);
  const [savingIds, setSavingIds] = useState<string[]>([]);
  const [saveError, setSaveError] = useState('');
  const visibleTasks = hideCompleted ? tasks.filter((task) => !task.completed) : tasks;

  useEffect(() => {
    let ignore = false;

    async function loadOnOpen() {
      try {
        const data = await fetchTasks();
        if (!ignore) {
          setTasks(data);
        }
      } catch (error) {
        if (!ignore) {
          setLoadError(error instanceof Error ? error.message : '任务读取失败。');
        }
      } finally {
        if (!ignore) {
          setIsLoading(false);
        }
      }
    }

    loadOnOpen();
    return () => {
      ignore = true;
    };
  }, []);

  async function handleToggle(task: Task) {
    setSaveError('');
    setSavingIds((current) => [...current, task.id]);
    try {
      await saveCompleted(task.id, !task.completed);
    } finally {
      setSavingIds((current) => current.filter((id) => id !== task.id));
    }
  }

  async function saveCompleted(id: string, completed: boolean) {
    let response: Response;
    try {
      response = await fetch(apiUrl('/tasks/' + id), {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ completed }),
      });
    } catch {
      setSaveError('无法连接练习服务，完成状态没有保存。');
      return;
    }
    if (!response.ok) {
      setSaveError('保存失败：服务器返回了状态 ' + response.status + '。');
      return;
    }
    const data: unknown = await response.json();
    if (!isTask(data)) {
      setSaveError('已保存，但返回的数据格式不正确，请刷新页面查看。');
      return;
    }
    setTasks((current) => current.map((task) => (task.id === data.id ? data : task)));
  }

  return (
    <main className="page">
      <h1>任务进度</h1>
      <div className={styles.toolbar}>
        <label>
          <input
            type="checkbox"
            checked={hideCompleted}
            onChange={(event) => setHideCompleted(event.target.checked)}
          />
          只看未完成
        </label>
      </div>
      {saveError && <p role="alert">{saveError}</p>}
      {isLoading && <p role="status">正在读取任务……</p>}
      {loadError && <p role="alert">{loadError}</p>}
      {!isLoading && !loadError && visibleTasks.length === 0 && (
        <p className={styles.empty}>没有需要显示的任务。</p>
      )}
      {visibleTasks.length > 0 && (
        <ul className={styles.list}>
          {visibleTasks.map((task) => (
            <li key={task.id} className={styles.item}>
              <label className={task.completed ? styles.title + ' ' + styles.done : styles.title}>
                <input
                  type="checkbox"
                  checked={task.completed}
                  disabled={savingIds.includes(task.id)}
                  onChange={() => handleToggle(task)}
                />
                {task.title}
              </label>
            </li>
          ))}
        </ul>
      )}
    </main>
  );
}
```

### `src/App.tsx`（练习期间）

```tsx
import './App.css';
import TaskStatusPage from './pages/TaskStatusPage';

export default function App() {
  return <TaskStatusPage />;
}
```

### 对照验收标准

1. 勾选时发送 `PATCH`，完成状态保存在服务器上，刷新后重新读取，结果不变。
2. 保存期间 `savingIds` 中有这条任务的编号，它的复选框 `disabled`。
3. 失败时 `saveCompleted` 在更新 `tasks` 之前就 `return` 了，`task.completed` 没变，受控的复选框保持原来的状态，页面显示 `saveError`。
4. `visibleTasks` 每次渲染时根据最新的 `tasks` 计算，任务变成已完成后，就不在“只看未完成”的结果中了。
5. 成功后用 `setTasks((current) => current.map(…))` 替换这一条，没有调用 `fetchTasks`。
6. `npm.cmd run check` 没有错误。

练习完成后，把 `App.tsx` 恢复为第 93 章的内容（显示 `TaskServerPage`）。`TaskStatusPage.tsx` 可以保留，不影响后续章节。

## 第 115 章：我的收藏

### 思路

1. **只有登录后才能打开**：路由用第 112 章的 `RequireAuth` 包住页面。守卫跳转时记下原来的地址，登录后自动回到 `/favorites`（第 113 章），这部分不用另写代码。
2. **导航栏中的链接**：在 `AppLayout` 中按 `!checking && user` 决定是否显示“我的收藏”，和用户名、“退出”的写法相同（第 111、114 章）。
3. **标题**：用 `useAuth()` 读取当前用户，标题和标签页标题用同一个变量 `title`，标签页标题用 React 19 的 `<title>`（第 100 章）。`<title>` 中只能有一段文字，所以先拼好再放进去。
4. **收藏**：用一个编号数组 `favoriteIds` 记住收藏了哪些商品，和第 60 章记录选中的商品相同。切换时用更新函数，收藏过的用 `filter` 去掉，没收藏的用展开语法加上（第 45、46 章）。
5. **已收藏件数**：就是 `favoriteIds.length`，现算即可，不另外保存（第 32 章）。
6. **样式**：复用待办事项页的 `TaskPage.module.css` 中的 `list`、`item`、`title`（第 85 章）。按钮加上 `aria-pressed`，读屏软件能读出它是否处于“已收藏”状态。

### `src/pages/FavoritesPage.tsx`

```tsx
import { useState } from 'react';
import { products } from '../data/products';
import { useAuth } from '../hooks/useAuth';
import styles from './TaskPage.module.css';

export default function FavoritesPage() {
  const { user } = useAuth();
  const [favoriteIds, setFavoriteIds] = useState<number[]>([]);

  function handleToggle(productId: number) {
    setFavoriteIds((current) =>
      current.includes(productId)
        ? current.filter((id) => id !== productId)
        : [...current, productId],
    );
  }

  // RequireAuth 保证只有登录后才显示本页，这里只是让 TypeScript 放心
  if (!user) {
    return null;
  }

  const title = user.displayName + ' 的收藏';

  return (
    <main className="page">
      <title>{title}</title>
      <h1>{title}</h1>
      <p>已收藏 {favoriteIds.length} 件</p>
      <ul className={styles.list}>
        {products.map((product) => {
          const isFavorite = favoriteIds.includes(product.id);
          return (
            <li key={product.id} className={styles.item}>
              <span className={styles.title}>{product.name}</span>
              <button
                type="button"
                aria-pressed={isFavorite}
                onClick={() => handleToggle(product.id)}
              >
                {isFavorite ? '取消收藏' : '收藏'}
              </button>
            </li>
          );
        })}
      </ul>
    </main>
  );
}
```

### `src/App.tsx`（练习期间）

在导入 `FaqPage` 的一行下面加入：

```tsx
import FavoritesPage from './pages/FavoritesPage';
```

在布局路由中 `/account` 那条路由的上面加入：

```tsx
            <Route
              path="/favorites"
              element={
                <RequireAuth>
                  <FavoritesPage />
                </RequireAuth>
              }
            />
```

### `src/components/AppLayout.tsx`（练习期间）

在用户名链接的上面加入：

```tsx
        {!checking && user && <NavLink to="/favorites">我的收藏</NavLink>}
```

### 对照验收标准

1. 没有登录时 `user` 是 `null`，导航栏不显示链接；`RequireAuth` 把人送去登录页，并在 `state` 中带上 `/favorites`，登录后回到这里。
2. `title` 由 `user.displayName` 拼出，`<h1>` 和 `<title>` 用的是同一个变量。
3. `favoriteIds` 中有这件商品的编号时，按钮显示“取消收藏”；件数是数组的长度，随之变化。
4. 点“退出”后 `user` 变成 `null`，`RequireAuth` 把人送去登录页（第 114 章）。
5. `npm.cmd run check` 没有错误。

练习完成后，把 `App.tsx` 和 `AppLayout.tsx` 恢复为第 115 章的内容。`FavoritesPage.tsx` 可以保留，不影响后续章节。

## 第 141 章：上线自检

### 思路

1. **只有登录后才能打开**：和第 115 章的“我的收藏”一样，路由用 `RequireAuth` 包住页面（第 112 章），导航链接按 `!checking && user` 显示（第 111 章）。
2. **三步请求**：直接使用已有的请求函数：`authApi.ts` 中的 `fetchCurrentUser`（第 111 章）、`taskApi.ts` 中的 `createTask` 和 `fetchTasks`（第 92A 章）。三步写在一个异步函数 `runSteps` 中，按顺序 `await`，某一步失败就 `return`，后面的步骤不会执行。
3. **每完成一步显示一行**：结果保存在数组 state `results` 中，每一步用更新函数追加一项（第 45 章）。`runSteps` 中途多次更新 state，用更新函数才不会读到旧的数组（第 126 章）。
4. **请求出错**：请求函数在连不上服务器或状态不正常时会抛出错误。`handleStart` 用 `try`/`catch` 接住，把原因作为最后一行显示（第 136 章）；`finally` 中恢复按钮。
5. **确认任务保存在服务器上**：新增成功后记下服务器返回的任务，重新读取列表后用数组的 `some` 按编号查找：`some` 对每一项调用传入的函数，只要有一项返回 `true`，结果就是 `true`，写法和 `filter` 相同（第 46 章），只是结果是真假而不是新数组。比较编号而不是标题，因为可能已经有同名的任务。

### `src/pages/OnlineCheckPage.tsx`

```tsx
import { useState } from 'react';
import { fetchCurrentUser } from '../lib/authApi';
import { createTask, fetchTasks } from '../lib/taskApi';

type StepResult = {
  name: string;
  passed: boolean;
  message: string;
};

function getErrorMessage(error: unknown) {
  return error instanceof Error ? error.message : '出现了未知错误。';
}

export default function OnlineCheckPage() {
  const [results, setResults] = useState<StepResult[]>([]);
  const [isRunning, setIsRunning] = useState(false);

  function addResult(result: StepResult) {
    // 每完成一步就显示一步，用更新函数追加，避免读到旧的 results
    setResults((current) => [...current, result]);
  }

  async function runSteps() {
    // 第 1 步：确认登录。浏览器带上了登录 Cookie，服务器才会返回当前用户
    const user = await fetchCurrentUser();
    if (!user) {
      addResult({ name: '确认登录', passed: false, message: '服务器认为没有登录。' });
      return;
    }
    addResult({ name: '确认登录', passed: true, message: '已登录：' + user.displayName + '。' });

    // 第 2 步：新增一个任务，写入练习服务的数据
    const saveResult = await createTask({ title: '上线自检' });
    if (saveResult.status === 'invalid') {
      addResult({ name: '新增任务', passed: false, message: saveResult.titleError });
      return;
    }
    const newTask = saveResult.task;
    addResult({ name: '新增任务', passed: true, message: '已新增任务“' + newTask.title + '”。' });

    // 第 3 步：重新读取任务列表，确认刚才的任务确实保存在服务器上
    const tasks = await fetchTasks();
    const found = tasks.some((task) => task.id === newTask.id);
    addResult({
      name: '重新读取',
      passed: found,
      message: found
        ? '在 ' + tasks.length + ' 个任务中找到了刚才新增的任务。'
        : '任务列表中没有刚才新增的任务。',
    });
  }

  async function handleStart() {
    setResults([]);
    setIsRunning(true);
    try {
      await runSteps();
    } catch (error) {
      // 请求失败时，把原因作为最后一步显示，后面的步骤不再执行
      addResult({ name: '请求失败', passed: false, message: getErrorMessage(error) });
    } finally {
      setIsRunning(false);
    }
  }

  return (
    <main className="page">
      <title>上线自检</title>
      <h1>上线自检</h1>
      <button type="button" disabled={isRunning} onClick={handleStart}>
        {isRunning ? '正在自检……' : '开始自检'}
      </button>
      <ol>
        {results.map((result) => (
          <li key={result.name}>
            {result.passed ? '通过' : '失败'}：{result.name}。{result.message}
          </li>
        ))}
      </ol>
    </main>
  );
}
```

### `src/App.tsx`（练习期间）

在导入 `NotesPage` 的一行下面加入：

```tsx
import OnlineCheckPage from './pages/OnlineCheckPage';
```

在 `/status` 那条路由的下面加入：

```tsx
            <Route
              path="/online-check"
              element={
                <RequireAuth>
                  <OnlineCheckPage />
                </RequireAuth>
              }
            />
```

### `src/components/AppLayout.tsx`（练习期间）

在“运行状态”链接的下面加入：

```tsx
        {!checking && user && <NavLink to="/online-check">上线自检</NavLink>}
```

改完后运行 `npm.cmd run build`，把新的 `dist` 拖到 Netlify 上这个网站的“Deploys”页面（第 140 章），再在线上地址中验收。

### 对照验收标准

1. 没有登录时 `user` 是 `null`，导航栏不显示链接；直接打开 `/online-check` 时，Netlify 按改写规则返回 `index.html`（第 140 章），`RequireAuth` 再把人送去登录页，登录后回到这里（第 113 章）。
2. 登录 Cookie 由 Netlify 转交给练习服务（第 139 章），`fetchCurrentUser` 得到当前用户，第一行显示“成员小王”；新增和重新读取都成功，三行都是“通过”。
3. 任务保存在 Render 上的练习服务中，刷新“任务管理”页时重新读取，能看到“上线自检”；刷新后 `AuthProvider` 向服务器确认登录状态（第 111 章），导航栏中仍是“成员小王”。
4. `npm.cmd run check` 没有错误。

练习完成后，把 `App.tsx` 和 `AppLayout.tsx` 恢复为第 141 章的内容。`OnlineCheckPage.tsx` 可以保留，不影响其他页面。
