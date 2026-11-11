# 第 36 章：如何为输入框事件写 TypeScript 类型？

前置：[第 35 章：如何让输入框的内容与 state 保持同步？](第035章-如何让输入框的内容与state保持同步.md)

## 一、本章要解决的问题

注册表单要增加“密码”输入框。表单中的输入框越来越多，把处理函数都写在 `onChange` 的箭头函数里，JSX 会越来越乱，所以通常把它们提取成有名字的函数，例如 `handleUsernameChange`。

但提取出来以后，编辑器立刻在参数 `event` 下面画了红线：TypeScript 不知道 `event` 是什么。本章学习怎样给事件参数写类型。完成后，表单有用户名和密码两个输入框，处理函数都单独命名，类型检查通过。

## 二、必要原理

### 写在 JSX 中时，类型是自动推断的

第 35 章的写法：

```tsx
onChange={(event) => setUsername(event.target.value)}
```

箭头函数直接写在 `<input>` 的 `onChange` 中，TypeScript 知道“输入框的 `onChange` 会收到什么样的事件”，所以自动推断出了 `event` 的类型，不需要手写。

### 单独写成函数后，需要手写类型

```tsx
function handleUsernameChange(event) {
  setUsername(event.target.value);
}
```

这个函数是单独定义的，TypeScript 看不出它将来会交给谁使用，只能把 `event` 当作 `any`（任意类型）。项目开启了严格检查，不允许参数隐含 `any`，所以报错。

### React 提供的事件类型

React 为各种事件准备了类型。输入框内容变化的事件是 `ChangeEvent`，尖括号中写触发事件的元素类型：

```tsx
import type { ChangeEvent } from 'react';

function handleUsernameChange(event: ChangeEvent<HTMLInputElement>) {
  setUsername(event.target.value);
}
```

- `ChangeEvent`：内容变化的事件。
- `HTMLInputElement`：浏览器中 `<input>` 元素的类型。写明它以后，TypeScript 就知道 `event.target` 是一个输入框，有 `value` 属性。

以后遇到下拉框，会写成 `ChangeEvent<HTMLSelectElement>`（第 39 章）。

### 不知道类型时怎么查

不用记住所有事件类型。先把箭头函数写在 JSX 中，把鼠标移到参数 `event` 上，VS Code 会显示 TypeScript 推断出的类型，照着写到单独的函数上即可。

### 用 import type 导入

`ChangeEvent` 只是类型，运行时并不存在，用 `import type` 导入，与第 9 章导入 `Profile` 的方式相同。

## 三、动手实现

### 第 1 步：导入事件类型

打开 `src/pages/RegisterPage.tsx`，在第一行下面加入：

```tsx
import { useState } from 'react';
import type { ChangeEvent } from 'react';
```

### 第 2 步：增加密码的 state 和两个处理函数

在 `username` 那一行的下面加入：

```tsx
  const [password, setPassword] = useState('');

  function handleUsernameChange(event: ChangeEvent<HTMLInputElement>) {
    setUsername(event.target.value);
  }

  function handlePasswordChange(event: ChangeEvent<HTMLInputElement>) {
    setPassword(event.target.value);
  }
```

### 第 3 步：使用命名的处理函数，并增加密码输入框

把用户名输入框的 `onChange` 改为：

```tsx
            onChange={handleUsernameChange}
```

在用户名所在的 `<div className={styles.field}>` 后面，加入密码输入框：

```tsx
        <div className={styles.field}>
          <label htmlFor="password">密码</label>
          <input id="password" type="password" value={password} onChange={handlePasswordChange} />
        </div>
```

`type="password"` 让输入的内容显示为圆点，旁边的人看不到密码。

## 四、运行与检查

保持开发服务器运行（`npm.cmd run dev`），保存文件后在浏览器中查看。

1. 表单有“用户名”和“密码”两个输入框，用户名下方仍有字数提示。
2. 在密码框中输入，显示为圆点。
3. 打开 React 开发者工具，`RegisterPage` 有两个 `State`，分别是用户名和密码的内容（密码在这里是明文）。
4. 把 `handlePasswordChange` 参数中的 `: ChangeEvent<HTMLInputElement>` 临时删掉，编辑器在 `event` 下面画红线，提示参数隐式具有 `any` 类型。检查后恢复。
5. 把鼠标移到 `handleUsernameChange` 的 `event` 上，提示中显示的就是它的类型。

最后运行检查，应没有错误：

```powershell
npm.cmd run check
```

**小练习**：把两个处理函数合并成一个 `handleChange`，根据 `event.target.id` 是 `'username'` 还是 `'password'` 决定修改哪个 state。写完后比较一下，哪种写法更容易读懂？练习后恢复本章代码。

## 五、常见错误

| 现象 | 原因 | 怎么改 |
| --- | --- | --- |
| 编辑器提示参数 `event` 隐式具有 `any` 类型 | 单独定义的函数没有写参数类型 | 写 `event: ChangeEvent<HTMLInputElement>` |
| 编辑器提示 `ChangeEvent` 找不到 | 没有导入，或从别的地方导入了同名的类型 | `import type { ChangeEvent } from 'react';` |
| 编辑器提示 `event.target` 上没有 `value` | 尖括号中写成了 `HTMLElement` 等没有 `value` 的元素类型 | 输入框写 `HTMLInputElement` |
| 写成 `onChange={handleUsernameChange()}` 后报错 | 加了括号，变成了渲染时调用函数，而且没有传入事件 | 只写函数名 |
| 密码框中能看到明文 | 漏写了 `type="password"` | 密码输入框加上 `type="password"` |

## 六、本章完整代码

### `src/pages/RegisterPage.tsx`

```tsx
import { useState } from 'react';
import type { ChangeEvent } from 'react';
import styles from './RegisterPage.module.css';

export default function RegisterPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  function handleUsernameChange(event: ChangeEvent<HTMLInputElement>) {
    setUsername(event.target.value);
  }

  function handlePasswordChange(event: ChangeEvent<HTMLInputElement>) {
    setPassword(event.target.value);
  }

  return (
    <main className="page">
      <h1>注册账号</h1>
      <div className={styles.form}>
        <div className={styles.field}>
          <label htmlFor="username">用户名</label>
          <input
            id="username"
            value={username}
            onChange={handleUsernameChange}
          />
          <p className={styles.hint}>已输入 {username.length} / 12 个字</p>
        </div>
        <div className={styles.field}>
          <label htmlFor="password">密码</label>
          <input id="password" type="password" value={password} onChange={handlePasswordChange} />
        </div>
      </div>
    </main>
  );
}
```

## 七、本章改动小结

修改 `src/pages/RegisterPage.tsx`：导入 `ChangeEvent` 类型，新增密码的 state、两个命名的处理函数和密码输入框。

---

下一章：[第 37 章：如何切换密码的显示与隐藏？](第037章-如何切换密码的显示与隐藏.md)
