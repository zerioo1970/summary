/**
 * 第 3 节 · children 与插槽
 *
 * 覆盖要点：children、多个 ReactNode 插槽、插槽与 children 的选择、
 *          render prop（把「怎么渲染」交给父组件）。
 */

import type { ReactNode } from 'react';

type Task = {
  id: number;
  title: string;
  days: number;
  done: boolean;
};

// icon / footer / children 都是 ReactNode：能装文字、数字、JSX、数组、null
type CardProps = {
  title: string;
  icon?: ReactNode;
  footer?: ReactNode;
  children?: ReactNode;
};

export function Card({ title, icon, footer, children }: CardProps) {
  return (
    <div className="card">
      <header className="card-head">
        {icon}
        <h3>{title}</h3>
      </header>

      {/* children 就是写在 <Card> ... </Card> 之间的内容 */}
      <div className="card-body">{children}</div>

      {/* 用 !== undefined 判断，避免 footer 传 0 或 '' 时整块消失 */}
      {footer !== undefined && <footer className="card-foot">{footer}</footer>}
    </div>
  );
}

type TaskCardProps = {
  task: Task;
  // render prop：父组件决定标签长什么样，但需要用到子组件手里的 task
  renderTag?: (task: Task) => ReactNode;
  children?: ReactNode;
};

export function TaskCard({ task, renderTag, children }: TaskCardProps) {
  return (
    <Card
      title={task.title}
      icon={<span aria-hidden="true">★</span>}
      footer={<small>剩余 {task.days} 天</small>}
    >
      {/* 可选调用：父组件没传 renderTag 时什么都不渲染 */}
      {renderTag?.(task)}
      {children}
    </Card>
  );
}

const TASK: Task = { id: 1, title: '写周报', days: 1, done: false };

export default function SlotsDemo() {
  return (
    <section>
      <h2>第 3 节 · children 与插槽</h2>

      <TaskCard
        task={TASK}
        renderTag={(task) => (
          <span className="tag">{task.days <= 1 ? '紧急' : '正常'}</span>
        )}
      >
        <p>今天下班前给我</p>
      </TaskCard>

      {/* 同一个 Card，父组件换个内容就换了个用途 */}
      <Card title="本周统计" icon={<span aria-hidden="true">📊</span>}>
        <p>已完成 2 项，进行中 1 项</p>
      </Card>
    </section>
  );
}
