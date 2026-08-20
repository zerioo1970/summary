import { useState } from 'react';
import TaskCardDemo from './ch01/TaskCardDemo';
import TypedPropsDemo from './ch02/TypedPropsDemo';
import SlotsDemo from './ch03/SlotsDemo';
import TaskPanel from './ch04/TaskPanel';
import CallbackDemo from './ch04/CallbackDemo';

// 联合类型当「枚举」用：写错页面名编译就报错
type Page = 'ch01' | 'ch02' | 'ch03' | 'ch04min' | 'ch04';

const PAGES: readonly { id: Page; label: string }[] = [
  { id: 'ch01', label: '1 · props 基础' },
  { id: 'ch02', label: '2 · 类型' },
  { id: 'ch03', label: '3 · children 与插槽' },
  { id: 'ch04min', label: '4A · 回调最小形态' },
  { id: 'ch04', label: '4B · 回调三级' },
];

export default function App() {
  const [page, setPage] = useState<Page>('ch01');

  return (
    <div className="app">
      <h1>React 19 props 教程 · 示例</h1>

      <nav className="nav">
        {PAGES.map((item) => (
          <button
            key={item.id}
            type="button"
            onClick={() => setPage(item.id)}
            aria-pressed={page === item.id}
            className={page === item.id ? 'active' : ''}
          >
            {item.label}
          </button>
        ))}
      </nav>

      {page === 'ch01' && <TaskCardDemo />}
      {page === 'ch02' && <TypedPropsDemo />}
      {page === 'ch03' && <SlotsDemo />}
      {page === 'ch04min' && (
        <section>
          <h2>第 4 节 · 回调最小形态（图 2 / 图 3 用的就是这份代码）</h2>
          <TaskPanel />
        </section>
      )}
      {page === 'ch04' && <CallbackDemo />}
    </div>
  );
}
