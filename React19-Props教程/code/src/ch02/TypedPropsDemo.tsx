/**
 * 第 2 节 · 用类型把 props 管住
 *
 * 覆盖要点：联合类型限定取值、Record 映射表、传对象、传只读数组、
 *          判别联合让非法组合无法编译、Omit + ComponentProps + spread 继承原生属性。
 */

import type { ComponentProps } from 'react';

// 联合类型：取值只能是这三个之一，写错第四个值编译就过不去
export type Priority = 'low' | 'normal' | 'high';

export type Task = {
  id: number;
  title: string;
  days: number;
  done: boolean;
  priority: Priority;
};

// Record<Priority, string> 要求三个键一个都不能少，将来给 Priority 加值这里会立刻报错
const PRIORITY_LABEL: Record<Priority, string> = {
  low: '低',
  normal: '中',
  high: '高',
};

// 自己的 props + 原生 <button> 的全部属性（去掉 children，因为内容由 icon 决定）
type IconButtonProps = {
  icon: string;
} & Omit<ComponentProps<'button'>, 'children'>;

export function IconButton({ icon, ...rest }: IconButtonProps) {
  // rest 里可能有 disabled、title、onClick、aria-* …… 全部透传给真正的 button
  // 注意 type 写在 {...rest} 前面：这样父组件传 type 时能覆盖掉默认值
  return (
    <button type="button" {...rest}>
      {icon}
    </button>
  );
}

// 两种模式共有的字段
type TaskCardBase = {
  task: Task;
  owner?: string;
};

// 判别联合：mode 是判别字段。view 模式没有 draft，edit 模式必须有 draft。
export type TaskCardProps =
  | (TaskCardBase & { mode: 'view' })
  | (TaskCardBase & { mode: 'edit'; draft: string });

// 这里不解构 props，因为要靠 props.mode 收窄类型
export function TaskCard(props: TaskCardProps) {
  const { task, owner = '未指派' } = props;

  return (
    <div className="card">
      <h3>{task.title}</h3>
      <p>
        优先级：{PRIORITY_LABEL[task.priority]}｜负责人：{owner}
      </p>

      {props.mode === 'edit' ? (
        // 判断过 mode 之后，TypeScript 才允许访问 props.draft
        <input defaultValue={props.draft} aria-label="标题草稿" />
      ) : (
        <p>
          剩余 {task.days} 天｜{task.done ? '已完成' : '进行中'}
        </p>
      )}

      <IconButton icon="删除" title="删除这条任务" disabled={task.done} />
    </div>
  );
}

// readonly 数组：子组件只能读，push / sort 之类的修改方法直接不存在
type TaskListProps = {
  tasks: readonly Task[];
};

export function TaskList({ tasks }: TaskListProps) {
  return (
    <div>
      {tasks.map((task) => (
        // key 不是 props，子组件里拿不到它，它只给 React 用来识别列表项
        <TaskCard key={task.id} mode="view" task={task} />
      ))}
    </div>
  );
}

const TASKS: readonly Task[] = [
  { id: 1, title: '写周报', days: 3, done: false, priority: 'high' },
  { id: 2, title: '改预算表', days: 1, done: true, priority: 'normal' },
  { id: 3, title: '核对发票', days: 7, done: false, priority: 'low' },
];

export default function TypedPropsDemo() {
  return (
    <section>
      <h2>第 2 节 · 用类型把 props 管住</h2>
      <TaskList tasks={TASKS} />
      <h3>edit 模式（必须传 draft）</h3>
      <TaskCard mode="edit" task={TASKS[0]} draft="写周报（改）" owner="老王" />
    </section>
  );
}
