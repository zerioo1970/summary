// code/ch02/Frontend/src/main.ts
// 第 2 章只证明一件事：后端发出的页面里，TypeScript 编译出的脚本真的跑起来了。

const status = document.querySelector<HTMLParagraphElement>("#status");

// querySelector 找不到元素时返回 null。strict 模式下不判断就用，tsc 会直接报错
if (status === null) {
  throw new Error("页面缺少 #status 元素");
}

// 每次构建后刷新页面，时间都会变：用来确认看到的是最新产物，不是浏览器缓存
status.textContent = `脚本已运行：${new Date().toLocaleTimeString()}`;
