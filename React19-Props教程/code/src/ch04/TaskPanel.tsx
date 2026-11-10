import { useState } from 'react';

type TaskCardProps = {
  onToggle: () => void;
};

function TaskCard({ onToggle }: TaskCardProps) {
  return (
    <button type="button" onClick={onToggle}>
      切换完成
    </button>
  );
}

export default function TaskPanel() {
  const [done, setDone] = useState(false);

  function handleToggle(): void {
    setDone((d) => !d);
  }

  return (
    <div>
      <p>{done ? '已完成' : '进行中'}</p>
      <TaskCard onToggle={handleToggle} />
    </div>
  );
}
