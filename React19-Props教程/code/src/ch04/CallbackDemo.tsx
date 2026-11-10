/**
 * 第 4 节 · 回调 props 的三个级别
 *
 * 第 1 级 onToggle  : () => void            只通知，不带数据
 * 第 2 级 onDelete  : (id: number) => void  通知 + 带数据上去
 * 第 3 级 onRename  : (title: string) => void  子组件先有自己的 state，加工后再通知
 */

import { useState } from 'react';
import type { ChangeEvent } from 'react';

type Task = {
  id: number;
  title: string;
  done: boolean;
};

type TaskCardProps = {
  task: Task;
  onToggle: () => void;                    // 第 1 级
  onDelete: (id: number) => void;          // 第 2 级
  onRename: (title: string) => void;       // 第 3 级
};

function TaskCard({ task, onToggle, onDelete, onRename }: TaskCardProps) {
  // 「正在编辑」和「输入框里的草稿」是子组件的私有状态，父组件看不到
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState(task.title);

  function handleDraftChange(e: ChangeEvent<HTMLInputElement>): void {
    setDraft(e.target.value);
  }

  // 第 3 级的典型形态：校验 → 通知父组件 → 自己收尾
  function handleSave(): void {
    const next = draft.trim();
    if (next === '') return;
    onRename(next);
    setEditing(false);
  }

  function handleCancel(): void {
    setDraft(task.title);
    setEditing(false);
  }

  if (editing) {
    return (
      <div className="card">
        <input value={draft} onChange={handleDraftChange} aria-label="任务标题" />
        <button type="button" onClick={handleSave}>
          保存
        </button>
        <button type="button" onClick={handleCancel}>
          取消
        </button>
      </div>
    );
  }

  return (
    <div className="card">
      <h3>
        {task.title}　<small>{task.done ? '已完成' : '进行中'}</small>
      </h3>

      {/* 第 1 级：无需加工，直接把回调挂给 onClick */}
      <button type="button" onClick={onToggle}>
        {task.done ? '标记未完成' : '标记完成'}
      </button>

      <button type="button" onClick={() => setEditing(true)}>
        改名
      </button>

      {/* 第 2 级：要传参，所以必须用箭头函数包一层 */}
      <button type="button" onClick={() => onDelete(task.id)}>
        删除
      </button>
    </div>
  );
}

const INITIAL_TASK: Task = { id: 7, title: '写周报', done: false };

export default function CallbackDemo() {
  const [task, setTask] = useState<Task | null>(INITIAL_TASK);
  const [log, setLog] = useState<readonly string[]>([]);

  function handleToggle(): void {
    // 对象也要「返回新的」，不能改原来那个
    setTask((prev) => (prev === null ? prev : { ...prev, done: !prev.done }));
  }

  function handleDelete(id: number): void {
    setLog((prev) => [...prev, `收到子组件传上来的 id：${id}`]);
    setTask(null);
  }

  function handleRename(title: string): void {
    setLog((prev) => [...prev, `收到新标题：${title}`]);
    setTask((prev) => (prev === null ? prev : { ...prev, title }));
  }

  function handleRestore(): void {
    setTask(INITIAL_TASK);
    setLog([]);
  }

  return (
    <section>
      <h2>第 4 节 · 回调 props 的三个级别</h2>

      {task === null ? (
        <p>
          任务已删除。
          <button type="button" onClick={handleRestore}>
            恢复
          </button>
        </p>
      ) : (
        <TaskCard
          task={task}
          onToggle={handleToggle}
          onDelete={handleDelete}
          onRename={handleRename}
        />
      )}

      <h3>父组件收到的消息</h3>
      <ul>
        {log.map((line, i) => (
          <li key={i}>{line}</li>
        ))}
      </ul>
    </section>
  );
}
