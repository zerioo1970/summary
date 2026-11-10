/**
 * 第 1 节 · props 的基本使用
 *
 * 覆盖要点：类型定义、父组件传值、子组件解构接收、
 *          string / number / boolean 三种传法、可选 props 与默认值、props 只读。
 */

// props 的类型：一份父子之间的契约。每个字段就是一个「可以传什么」的约定。
type TaskCardProps = {
  title: string;     // 必填
  days: number;      // 必填
  done: boolean;     // 必填
  owner?: string;    // 可选，问号表示父组件可以不传
};

// 参数位置直接解构，owner 用默认参数兜底（React 19 不用 defaultProps）
function TaskCard({ title, days, done, owner = '未指派' }: TaskCardProps) {
  return (
    <div className="card">
      <h3>{title}</h3>
      <p>负责人：{owner}</p>
      <p>剩余 {days} 天</p>
      <p>状态：{done ? '已完成' : '进行中'}</p>
    </div>
  );
}

export default function TaskCardDemo() {
  return (
    <section>
      <h2>第 1 节 · props 的基本使用</h2>

      {/* 字符串可以用引号，其余类型一律用花括号 */}
      <TaskCard title="写周报" days={3} done={false} />

      {/* done 是布尔简写，等价于 done={true}；owner 传了就覆盖默认值 */}
      <TaskCard title="改预算表" days={1} done owner="老王" />

      {/* owner 不传，显示默认的「未指派」 */}
      <TaskCard title="核对发票" days={7} done={false} />
    </section>
  );
}
