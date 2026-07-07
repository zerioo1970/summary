# 第 11 章 · 第九步：编写 React CRUD 页面

> 上一章：[10-封装axios与API层](10-封装axios与API层.md) ｜ 下一章：[12-前后端联调与数据流](12-前后端联调与数据流.md)

## 11.1 完整的用户管理组件

`src/components/UserManager.jsx`——一个包含「列表 + 新增 + 编辑 + 删除」的完整页面。用 React 19 的 Hooks（`useState`、`useEffect`）实现：

```jsx
import { useEffect, useState } from 'react';
import {
  getUsers, createUser, updateUser, deleteUser,
} from '../api/userApi';

const emptyForm = { username: '', age: '', email: '' };

export default function UserManager() {
  const [users, setUsers] = useState([]);   // 用户列表
  const [form, setForm] = useState(emptyForm); // 表单数据
  const [editingId, setEditingId] = useState(null); // 正在编辑的 id（null=新增）
  const [loading, setLoading] = useState(false);

  // 加载列表
  const loadUsers = async () => {
    setLoading(true);
    try {
      const data = await getUsers();
      setUsers(data);
    } finally {
      setLoading(false);
    }
  };

  // 组件首次挂载时加载一次
  useEffect(() => {
    loadUsers();
  }, []);

  // 表单输入变化
  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  // 提交（新增 或 修改）
  const handleSubmit = async (e) => {
    e.preventDefault();
    const payload = { ...form, age: Number(form.age) || null };
    if (editingId) {
      await updateUser(editingId, payload); // 修改
    } else {
      await createUser(payload);            // 新增
    }
    setForm(emptyForm);
    setEditingId(null);
    loadUsers(); // 重新拉列表
  };

  // 点击「编辑」：把该行数据填进表单
  const handleEdit = (user) => {
    setEditingId(user.id);
    setForm({ username: user.username, age: user.age ?? '', email: user.email ?? '' });
  };

  // 删除
  const handleDelete = async (id) => {
    if (!window.confirm('确定删除这条数据吗？')) return;
    await deleteUser(id);
    loadUsers();
  };

  return (
    <div style={{ maxWidth: 800, margin: '40px auto', fontFamily: 'sans-serif' }}>
      <h2>用户管理（React 19 + Spring Boot 3.5 + SQL Server）</h2>

      {/* 表单区 */}
      <form onSubmit={handleSubmit} style={{ marginBottom: 20 }}>
        <input name="username" placeholder="用户名" value={form.username}
               onChange={handleChange} required />
        <input name="age" placeholder="年龄" type="number" value={form.age}
               onChange={handleChange} style={{ marginLeft: 8 }} />
        <input name="email" placeholder="邮箱" value={form.email}
               onChange={handleChange} style={{ marginLeft: 8 }} />
        <button type="submit" style={{ marginLeft: 8 }}>
          {editingId ? '保存修改' : '新增'}
        </button>
        {editingId && (
          <button type="button" onClick={() => { setEditingId(null); setForm(emptyForm); }}
                  style={{ marginLeft: 8 }}>
            取消
          </button>
        )}
      </form>

      {/* 列表区 */}
      {loading ? <p>加载中...</p> : (
        <table border="1" cellPadding="8" style={{ borderCollapse: 'collapse', width: '100%' }}>
          <thead>
            <tr>
              <th>ID</th><th>用户名</th><th>年龄</th><th>邮箱</th><th>操作</th>
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id}>
                <td>{u.id}</td>
                <td>{u.username}</td>
                <td>{u.age}</td>
                <td>{u.email}</td>
                <td>
                  <button onClick={() => handleEdit(u)}>编辑</button>
                  <button onClick={() => handleDelete(u.id)} style={{ marginLeft: 8 }}>删除</button>
                </td>
              </tr>
            ))}
            {users.length === 0 && (
              <tr><td colSpan="5" style={{ textAlign: 'center' }}>暂无数据</td></tr>
            )}
          </tbody>
        </table>
      )}
    </div>
  );
}
```

## 11.2 挂载到 App

`src/App.jsx`：

```jsx
import UserManager from './components/UserManager';

export default function App() {
  return <UserManager />;
}
```

**核心逻辑讲解**：
- `useState` 管理三份状态：`users`（列表）、`form`（表单）、`editingId`（区分新增/编辑）。
- `useEffect(() => loadUsers(), [])`：空依赖数组表示「只在组件首次渲染后执行一次」，用来初始化加载列表。
- **每次增删改成功后都调用 `loadUsers()` 重新拉取列表**，保证界面和数据库一致（最简单可靠的刷新策略）。
- 表单复用：`editingId` 为 `null` 时是新增，有值时是编辑，提交时据此选择调用 `createUser` 还是 `updateUser`。

> **React 19 进阶（可选）**：也可以用 `useActionState` + `<form action={fn}>` 的新写法来管理提交状态，或用 `useOptimistic` 做乐观更新。本教程用经典 Hooks，便于初学者理解数据流。

---

> 下一章 👉 [12-前后端联调与数据流](12-前后端联调与数据流.md)
