# 第五章 · State 与事件

> 本文是《React 18 & 19 系统教程》的第 5 章。完整目录见 [README](README.md)。

> **State（状态）是什么？** state 是组件"自己的、会变化的数据"。props 是父组件传进来的（只读），而 state 是组件内部管理、可以随用户交互而改变的数据（比如计数器的数字、输入框的内容、开关的开/关）。
>
> **核心机制**：当你用 React 提供的方法更新 state 时，React 会**自动重新渲染**这个组件，让界面反映最新的数据。这就是 React"数据驱动界面"的精髓——你只管改数据，界面自动更新。
>
> **事件（Event）**：用户的点击、输入、按键等操作。React 用 `onClick`、`onChange` 等属性来绑定事件处理函数。state 通常在事件处理函数里被更新。
>
> 本章从"最简单的计数器"讲到"完整的交互组件"，共 28 个示例。

### （A）State 基础

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 58：最简单的 state（计数器）</h3>

```jsx
import { useState } from 'react';

function Counter() {
  const [count, setCount] = useState(0);
  return (
    <button onClick={() => setCount(count + 1)}>
      点击了 {count} 次
    </button>
  );
}
```

**详解**：`useState` 是最常用的 Hook，用来给组件添加一个状态。`useState(0)` 表示"创建一个初始值为 0 的状态"。它返回一个包含两项的数组：第一项 `count` 是当前状态值，第二项 `setCount` 是"更新它的函数"。点击按钮时调用 `setCount(count + 1)`，React 就会把 `count` 加 1 并**重新渲染**，界面上的数字随之更新。这就是一个完整的交互闭环。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 59：为什么要用 state，而不是普通变量</h3>

```jsx
// ❌ 用普通变量：点击时变量确实变了，但界面不会更新
function Broken() {
  let count = 0;
  return <button onClick={() => { count++; console.log(count); }}>{count}</button>;
}

// ✅ 用 state：更新会触发重新渲染，界面才会变
function Works() {
  const [count, setCount] = useState(0);
  return <button onClick={() => setCount(count + 1)}>{count}</button>;
}
```

**详解**：这是理解 state 的关键。普通变量 `let count` 改了之后，① 界面不会重新渲染（React 不知道数据变了）；② 而且组件每次重新渲染时，普通变量都会被重新初始化为 0，无法"记住"上次的值。而 state 有两个特殊能力：**更新它会触发重新渲染**，且 **React 会在多次渲染之间"记住"它的值**。所以凡是"变化后需要反映到界面上"的数据，都必须用 state。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 60：useState 语法详解</h3>

```jsx
const [count, setCount] = useState(0);
//     ↑当前值  ↑更新函数        ↑初始值

// 命名约定：更新函数用 set + 状态名（驼峰）
const [name, setName] = useState('');
const [isOpen, setIsOpen] = useState(false);
```

**详解**：`useState` 用到了 JS 的"数组解构"语法。它返回的其实是一个两元素数组 `[值, 更新函数]`，用 `[count, setCount]` 把它们取出来。名字可以随便起，但社区有强约定：**更新函数用 `set` + 状态名的驼峰形式**（`count` → `setCount`，`isOpen` → `setIsOpen`）。初始值作为 `useState()` 的参数传入，只在**第一次渲染**时用一次。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 61：state 可以是任意类型</h3>

```jsx
function Types() {
  const [count, setCount] = useState(0);        // 数字
  const [name, setName] = useState('张三');      // 字符串
  const [isOn, setIsOn] = useState(false);      // 布尔
  const [list, setList] = useState([]);         // 数组
  const [user, setUser] = useState({ id: 1 });  // 对象
  return <p>{count}-{name}-{String(isOn)}</p>;
}
```

**详解**：state 的值可以是任何 JavaScript 类型：数字、字符串、布尔、数组、对象，甚至 `null`。你根据要存的数据选择合适的类型。简单值（数字、字符串、布尔）更新起来最直接；数组和对象因为是"引用类型"，更新时有讲究（见后面的示例 66–73）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 62：使用多个 state</h3>

```jsx
function Form() {
  const [name, setName] = useState('');
  const [age, setAge] = useState(0);
  return (
    <>
      <input value={name} onChange={e => setName(e.target.value)} />
      <input value={age} onChange={e => setAge(Number(e.target.value))} />
      <p>{name}，{age} 岁</p>
    </>
  );
}
```

**详解**：一个组件里可以调用多次 `useState`，声明多个互相独立的状态。这里 `name` 和 `age` 各管各的，更新其中一个不影响另一个。**建议按"关注点"拆分 state**——把不相关的数据分开放，而不是硬塞进一个大对象。（当多个 state 关系紧密、更新逻辑复杂时，可考虑用第八章的 `useReducer`。）

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 63：布尔 state 与切换（toggle）</h3>

```jsx
function Toggle() {
  const [isOpen, setIsOpen] = useState(false);
  return (
    <div>
      <button onClick={() => setIsOpen(!isOpen)}>
        {isOpen ? '收起' : '展开'}
      </button>
      {isOpen && <p>这是展开的内容</p>}
    </div>
  );
}
```

**详解**：布尔 state 用于表示"开/关"类状态（展开/收起、显示/隐藏、选中/未选中）。切换时用 `setIsOpen(!isOpen)` 取反当前值。配合条件渲染 `{isOpen && ...}`，就能实现"点击切换显示"的常见交互。这是最基础也最高频的 state 用法之一。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 64：惰性初始化 state（初始值来自昂贵计算）</h3>

```jsx
function Expensive() {
  // 传"函数"而不是"值"，这个函数只在首次渲染执行一次
  const [value, setValue] = useState(() => {
    console.log('只计算一次');
    return computeExpensiveValue();
  });
  return <p>{value}</p>;
}
function computeExpensiveValue() { return 42; }
```

**详解**：`useState` 的初始值如果需要一次昂贵的计算（比如读 localStorage、大量运算），不要直接写 `useState(computeExpensiveValue())`——那样**每次渲染都会调用它**（虽然结果只有第一次被采用，但计算白白执行了）。正确做法是**传一个函数** `useState(() => computeExpensiveValue())`，React 只在首次渲染时调用它一次。这叫"惰性初始化"。

### （B）正确地更新 state

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 65：更新 state 会触发重新渲染</h3>

```jsx
function Clock() {
  const [time, setTime] = useState(new Date().toLocaleTimeString());
  return (
    <div>
      <p>当前时间：{time}</p>
      <button onClick={() => setTime(new Date().toLocaleTimeString())}>
        刷新
      </button>
    </div>
  );
}
```

**详解**：每次调用 `setTime(...)`，React 都会做两件事：① 把 state 更新成新值；② **重新执行整个组件函数**（重新渲染），用新的 state 值生成新界面。理解"更新 state → 组件重跑 → 界面更新"这条链路，是理解 React 的核心。组件函数会被反复调用，所以别在函数体里写会产生副作用的代码（那属于 `useEffect` 的活，见第八章）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 66：不要直接修改 state（不可变原则）</h3>

```jsx
function Demo() {
  const [user, setUser] = useState({ name: '张三', age: 20 });

  // ❌ 错误：直接改 state，React 检测不到变化，界面不更新
  const wrong = () => { user.age = 21; };

  // ✅ 正确：用 setUser 传入一个新对象
  const right = () => setUser({ ...user, age: 21 });

  return <button onClick={right}>{user.name}: {user.age}</button>;
}
```

**详解**：这是最重要的 state 规则——**永远不要直接修改 state，而要用更新函数传入新值**。原因：React 靠比较"新旧值的引用是否相同"来判断要不要重新渲染。直接改 `user.age`，对象引用没变，React 以为没变化，界面不更新。必须用 `{ ...user, age: 21 }` 创建一个**新对象**再 `setUser`。这条原则对对象和数组尤其关键（数字/字符串是原始值，本身不可变，不受影响）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 67：函数式更新（依赖旧值时）</h3>

```jsx
function Counter() {
  const [count, setCount] = useState(0);
  // 连续加 3 次，必须用函数式更新
  const addThree = () => {
    setCount(c => c + 1);
    setCount(c => c + 1);
    setCount(c => c + 1);
  };
  return <button onClick={addThree}>{count}</button>;
}
```

**详解**：当新值依赖旧值时，推荐用**函数式更新** `setCount(c => c + 1)`——参数 `c` 是 React 保证的"最新的 state 值"。为什么这里必须用它？因为下一个示例会讲：在同一个事件里连续调用 `setCount(count + 1)` 三次，`count` 都是同一个旧值（比如 0），三次都算成 0+1=1，结果只加了 1。而用 `c => c + 1`，React 会依次拿上一次的结果，最终正确地加到 3。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 68：为什么连续 setCount(count+1) 不生效</h3>

```jsx
function Counter() {
  const [count, setCount] = useState(0);
  const wrong = () => {
    setCount(count + 1); // count 是 0 → 设成 1
    setCount(count + 1); // count 还是 0 → 又设成 1
    setCount(count + 1); // count 还是 0 → 又设成 1，最终只 +1
  };
  return <button onClick={wrong}>{count}</button>;
}
```

**详解**：这解释了示例 67 的原因。在一次事件处理中，`count` 的值是"这次渲染时被冻结的快照"，整个函数里它都是同一个值（0）。所以三次 `setCount(count + 1)` 都是 `setCount(0 + 1)`，等于设了三次 1。而且 React 会把同一事件里的多次更新**批处理**（合并成一次渲染），最终 `count` 只变成 1。要连续基于最新值更新，就必须用函数式更新 `c => c + 1`（示例 67）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 69：state 为对象（更新要展开旧值）</h3>

```jsx
function Profile() {
  const [user, setUser] = useState({ name: '张三', age: 20, city: '北京' });
  // 只想改 age，但要把其它字段一起带上
  const grow = () => setUser({ ...user, age: user.age + 1 });
  return <button onClick={grow}>{user.name} {user.age} {user.city}</button>;
}
```

**详解**：更新对象型 state 时，因为要遵守"不可变原则"（示例 66），得创建新对象。用展开运算符 `{ ...user }` 先复制旧对象的所有字段，再覆盖要改的字段（`age: user.age + 1`）。如果只写 `setUser({ age: 21 })`，会丢掉 `name` 和 `city`！记住这个模式：**`{ ...旧对象, 要改的字段: 新值 }`**。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 70：更新嵌套对象</h3>

```jsx
function Settings() {
  const [config, setConfig] = useState({
    theme: 'light',
    notify: { email: true, sms: false },
  });
  // 修改嵌套的 notify.sms，每一层都要展开
  const toggleSms = () =>
    setConfig({
      ...config,
      notify: { ...config.notify, sms: !config.notify.sms },
    });
  return <button onClick={toggleSms}>短信：{String(config.notify.sms)}</button>;
}
```

**详解**：嵌套对象更新时，**每一层都要展开复制**。修改 `config.notify.sms`，既要 `{ ...config }` 复制外层，又要 `{ ...config.notify }` 复制内层，再改 `sms`。层级太深时这会很啰嗦——这也是为什么 state 结构不宜嵌套过深，或可借助 Immer 这类库来简化。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 71：state 为数组（添加元素）</h3>

```jsx
function TodoList() {
  const [items, setItems] = useState(['学习']);
  const add = () => setItems([...items, '新任务']); // 展开旧数组 + 新元素
  return (
    <div>
      <button onClick={add}>添加</button>
      <ul>{items.map((t, i) => <li key={i}>{t}</li>)}</ul>
    </div>
  );
}
```

**详解**：给数组型 state 添加元素，同样要遵守不可变原则——**不要用 `items.push()`**（那是原地修改）。正确做法是用展开语法创建新数组：`[...items, 新元素]`（加到末尾）或 `[新元素, ...items]`（加到开头）。React 看到是新数组引用，才会重新渲染。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 72：state 为数组（删除元素）</h3>

```jsx
function List() {
  const [items, setItems] = useState(['a', 'b', 'c']);
  const remove = (index) => setItems(items.filter((_, i) => i !== index));
  return (
    <ul>
      {items.map((t, i) => (
        <li key={i}>{t} <button onClick={() => remove(i)}>删除</button></li>
      ))}
    </ul>
  );
}
```

**详解**：删除数组元素用 `filter`——它返回一个**新数组**，只保留满足条件的项。这里保留"下标不等于要删除下标"的所有项。`filter` 天然符合不可变原则（不改原数组），是删除的首选。若按 id 删，则写 `items.filter(item => item.id !== targetId)`。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 73：state 为数组（修改某一项）</h3>

```jsx
function TodoList() {
  const [todos, setTodos] = useState([
    { id: 1, text: '学习', done: false },
    { id: 2, text: '运动', done: false },
  ]);
  const toggle = (id) =>
    setTodos(todos.map(t => t.id === id ? { ...t, done: !t.done } : t));
  return (
    <ul>
      {todos.map(t => (
        <li key={t.id} onClick={() => toggle(t.id)}
            style={{ textDecoration: t.done ? 'line-through' : 'none' }}>
          {t.text}
        </li>
      ))}
    </ul>
  );
}
```

**详解**：修改数组里的某一项用 `map`——遍历每一项，命中目标（`t.id === id`）就返回一个"改过的新对象"（`{ ...t, done: !t.done }`），其余原样返回。`map` 返回新数组，且被改的那一项也是新对象，完全符合不可变原则。**增删改分别对应 `[...arr, x]` / `filter` / `map`**，记住这三板斧就能应对绝大多数数组 state 操作。

### （C）事件处理

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 74：事件处理基础（onClick）</h3>

```jsx
function Button() {
  const handleClick = () => {
    alert('按钮被点击了');
  };
  return <button onClick={handleClick}>点我</button>;
}
```

**详解**：React 用 `onClick`、`onChange` 这类**驼峰命名**的属性来绑定事件（注意不是 HTML 的全小写 `onclick`）。属性值是一个函数——事件发生时 React 会调用它。这里把 `handleClick` 函数传给 `onClick`。事件处理函数常写成箭头函数，命名习惯是 `handle + 事件名`。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 75：传递函数 vs 调用函数（高频错误）</h3>

```jsx
function Demo() {
  const handleClick = () => alert('点击');

  return (
    <>
      <button onClick={handleClick}>✅ 正确：传函数引用</button>
      {/* ❌ 错误：加了括号 = 渲染时立即执行，而不是点击时 */}
      {/* <button onClick={handleClick()}>错误</button> */}
      {/* ✅ 需要传参时用箭头函数包一层 */}
      <button onClick={() => handleClick()}>✅ 正确：箭头函数包裹</button>
    </>
  );
}
```

**详解**：这是新手最常犯的错误。`onClick={handleClick}` 传的是"函数本身"（点击时才调用），正确；而 `onClick={handleClick()}` 带了括号，意思是"**立即执行** `handleClick`，把它的返回值给 onClick"——这会在渲染时就弹窗，且行为错误。**规则**：绑定事件传函数引用（不加括号）；如果需要传参，用箭头函数包一层 `() => handleClick(参数)`（见示例 77）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 76：事件对象 event</h3>

```jsx
function Link() {
  const handle = (e) => {
    console.log('事件类型：', e.type);       // 'click'
    console.log('目标元素：', e.target);      // 被点击的 DOM
    console.log('坐标：', e.clientX, e.clientY);
  };
  return <a href="/" onClick={handle}>点我</a>;
}
```

**详解**：事件处理函数会自动收到一个"事件对象" `e`（习惯命名为 `e` 或 `event`），里面包含这次事件的详细信息：`e.type`（事件类型）、`e.target`（触发事件的元素）、鼠标坐标、按键信息等。React 的事件对象是"合成事件"（SyntheticEvent），对各浏览器做了统一封装，用法和原生基本一致。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 77：传参给事件处理函数</h3>

```jsx
function Buttons() {
  const handle = (id) => alert('点了按钮 ' + id);
  return (
    <>
      <button onClick={() => handle(1)}>按钮 1</button>
      <button onClick={() => handle(2)}>按钮 2</button>
    </>
  );
}
```

**详解**：想给事件处理函数传自定义参数时，用箭头函数包一层：`onClick={() => handle(1)}`。这样传给 `onClick` 的是"一个点击时才会执行 `handle(1)` 的新函数"，符合示例 75 的规则。如果既要传参又要用事件对象，写成 `onClick={(e) => handle(1, e)}`。列表里给每一项绑事件时，这个模式极其常用。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 78：阻止默认行为（preventDefault）</h3>

```jsx
function Form() {
  const handleSubmit = (e) => {
    e.preventDefault(); // 阻止表单提交导致的页面刷新
    console.log('用 JS 处理提交，不刷新页面');
  };
  return (
    <form onSubmit={handleSubmit}>
      <button type="submit">提交</button>
    </form>
  );
}
```

**详解**：某些元素有"默认行为"——表单提交会刷新页面、点链接会跳转、右键会弹出菜单。调用 `e.preventDefault()` 可以阻止这些默认行为，改由你的 JS 代码接管。表单场景最典型：不加它，点提交按钮页面会整个刷新，破坏单页应用体验。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 79：阻止事件冒泡（stopPropagation）</h3>

```jsx
function Box() {
  return (
    <div onClick={() => console.log('外层 div 被点击')}>
      <button onClick={(e) => {
        e.stopPropagation(); // 阻止事件向上冒泡到外层 div
        console.log('只有按钮被点击');
      }}>
        点我不触发外层
      </button>
    </div>
  );
}
```

**详解**：事件默认会"冒泡"——点击内层按钮，事件会依次向上传播到外层 `div`，导致两个 `onClick` 都触发。调用 `e.stopPropagation()` 能阻止事件继续向上冒泡，这样点按钮时只执行按钮自己的处理函数。常用于弹窗（点内容区不关闭，点遮罩才关闭）等场景。注意区分：`preventDefault` 阻止默认行为，`stopPropagation` 阻止冒泡传播，两者不同。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 80：键盘事件</h3>

```jsx
function SearchBox() {
  const onKeyDown = (e) => {
    if (e.key === 'Enter') {
      alert('搜索：' + e.target.value);
    }
    if (e.key === 'Escape') {
      e.target.value = '';
    }
  };
  return <input onKeyDown={onKeyDown} placeholder="回车搜索，Esc 清空" />;
}
```

**详解**：键盘事件（`onKeyDown`、`onKeyUp`）的事件对象里，`e.key` 表示按下的键名（`'Enter'`、`'Escape'`、`'a'`、`'ArrowUp'` 等）。通过判断 `e.key` 实现快捷键、回车提交等交互。此外 `e.ctrlKey`、`e.shiftKey` 可判断是否同时按了组合键。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 81：常见事件类型一览</h3>

```jsx
function EventsDemo() {
  return (
    <div
      onClick={() => console.log('点击')}
      onDoubleClick={() => console.log('双击')}
      onMouseEnter={() => console.log('鼠标移入')}
      onMouseLeave={() => console.log('鼠标移出')}
    >
      <input
        onChange={(e) => console.log('输入变化', e.target.value)}
        onFocus={() => console.log('获得焦点')}
        onBlur={() => console.log('失去焦点')}
      />
    </div>
  );
}
```

**详解**：React 支持大量事件，都是驼峰命名。常见的有：鼠标类 `onClick`/`onDoubleClick`/`onMouseEnter`/`onMouseLeave`；表单类 `onChange`（输入变化）/`onFocus`（聚焦）/`onBlur`（失焦）/`onSubmit`（提交）；键盘类 `onKeyDown`/`onKeyUp`。其中 `onChange` 是表单开发的核心（下一节和第七章详讲）。

### （D）State + 事件综合与 React 18 特性

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 82：输入框与 state 联动（受控组件基础）</h3>

```jsx
function NameInput() {
  const [name, setName] = useState('');
  return (
    <div>
      <input value={name} onChange={(e) => setName(e.target.value)} />
      <p>你好，{name || '陌生人'}</p>
    </div>
  );
}
```

**详解**：这是 state 和事件结合的经典模式，也是"受控组件"的雏形。输入框的 `value` 绑定到 state（`value={name}`），用户每次输入触发 `onChange`，从 `e.target.value` 拿到最新输入值再 `setName` 更新 state，state 一变界面就刷新。数据流形成闭环：**state 决定输入框显示什么，输入又更新 state**。（表单的完整用法见第七章。）

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 83：自动批处理（React 18 新行为）</h3>

```jsx
function Batching() {
  const [a, setA] = useState(0);
  const [b, setB] = useState(0);
  const handle = () => {
    setTimeout(() => {
      setA(x => x + 1);
      setB(x => x + 1); // React 18：这两次更新合并成一次重新渲染
    }, 100);
  };
  console.log('渲染');
  return <button onClick={handle}>{a}-{b}</button>;
}
```

**详解**："批处理"指 React 把同一时机的多次 state 更新**合并成一次重新渲染**，以提升性能。在 React 17，只有事件处理函数内的更新才会批处理；而在 `setTimeout`、Promise、原生事件里则不会（会渲染多次）。**React 18 的改进**：无论更新发生在哪里（包括 `setTimeout`、异步回调），都会自动批处理。上面点击后控制台只打印一次"渲染"，而不是两次。这是 React 18 开箱即用的性能优化。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 84：退出批处理（flushSync）</h3>

```jsx
import { flushSync } from 'react-dom';

function Demo() {
  const [count, setCount] = useState(0);
  const handle = () => {
    flushSync(() => setCount(c => c + 1)); // 强制立即同步更新并重渲染
    console.log('此时 DOM 已经更新了');
  };
  return <button onClick={handle}>{count}</button>;
}
```

**详解**：极少数情况下，你需要"立即"更新 DOM，而不想等批处理结束（比如更新后马上要读取新的 DOM 尺寸、或控制滚动位置）。`flushSync(() => {...})` 会强制里面的更新同步执行并立刻重新渲染 DOM。**注意**：它会打断批处理、影响性能，属于"逃生舱"，只在确有需要时使用，绝大多数场景不需要它。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 85：综合实战——带增删的任务清单</h3>

```jsx
import { useState } from 'react';

function TodoApp() {
  const [todos, setTodos] = useState([]);
  const [text, setText] = useState('');

  const add = () => {
    if (!text.trim()) return;                 // 空输入不添加
    setTodos([...todos, { id: Date.now(), text }]); // 添加（示例 71）
    setText('');                              // 清空输入框
  };
  const remove = (id) =>
    setTodos(todos.filter(t => t.id !== id)); // 删除（示例 72）

  return (
    <div>
      <input
        value={text}
        onChange={(e) => setText(e.target.value)}      // 受控输入（示例 82）
        onKeyDown={(e) => e.key === 'Enter' && add()}  // 回车添加（示例 80）
      />
      <button onClick={add}>添加</button>
      <ul>
        {todos.map(t => (                              // 列表渲染
          <li key={t.id}>
            {t.text}
            <button onClick={() => remove(t.id)}>删除</button>
          </li>
        ))}
      </ul>
      <p>共 {todos.length} 项</p>
    </div>
  );
}
```

**详解**：这个小应用综合了本章的核心知识：
1. **两个 state**：任务列表 `todos`（数组）和输入框内容 `text`（字符串）；
2. **受控输入**：`value` + `onChange` 双向联动（示例 82）；
3. **数组不可变更新**：添加用 `[...todos, 新项]`（示例 71），删除用 `filter`（示例 72）；
4. **事件处理**：点击添加、回车添加（示例 80）、点击删除并传参（示例 77）；
5. **边界处理**：空输入不添加、添加后清空输入框。

把这个例子亲手敲一遍并理解每一行，就真正掌握了 state 与事件的配合——这是几乎所有 React 交互功能的基础。

---

---
[← 上一章](04-组件与Props.md) · [📖 目录](README.md) · [下一章 →](06-条件渲染与列表.md)
