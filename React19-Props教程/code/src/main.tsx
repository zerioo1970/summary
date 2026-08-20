import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';
import './styles.css';

const container = document.getElementById('root');

// container 的类型是 HTMLElement | null，strict 模式下必须先排除 null
if (container === null) {
  throw new Error('找不到 #root 节点，请检查 index.html');
}

createRoot(container).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
