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

