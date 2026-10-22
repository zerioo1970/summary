// code/ch04/Frontend/src/main.ts
// 第 4 章：与第 3 章相同。手机适配改的是 index.html 和 src/style.css。

// ---------- 和后端约定的数据形状 ----------
// 对应后端 record Todo(int Id, string Title, bool Done)，序列化后是小驼峰
interface Todo {
  id: number;
  title: string;
  done: boolean;
}

// 对应后端 record ErrorBody(string Message)
interface ErrorBody {
  message: string;
}

// 相对路径：页面和接口由同一个后端发出（同源），换服务器部署不用改
const API = "/api/todos";

// ---------- 统一的请求函数 ----------
// T 由调用方指定：getTodos 要 Todo[]，addTodo 要 Todo
async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(url, init);

  // fetch 只在"网络不通"时抛异常；后端返回 400/404/500 时它照样正常返回，
  // 不检查 res.ok，错误信息会被当成正常数据往下传
  if (!res.ok) {
    const body = (await res.json().catch(() => null)) as ErrorBody | null;
    throw new Error(body?.message ?? `请求失败：HTTP ${res.status}`);
  }

  // as T 只是告诉编译器"相信我"，运行时并不会检查后端真的返回了这个形状
  return (await res.json()) as T;
}

function getTodos(): Promise<Todo[]> {
  return request<Todo[]>(API);
}

function addTodo(title: string): Promise<Todo> {
  return request<Todo>(API, {
    method: "POST",
    // 后端按 JSON 解析请求体，必须声明类型；不写的话浏览器默认发 text/plain，后端回 415
    headers: { "Content-Type": "application/json" },
    // 请求体只能是字符串，对象要先序列化
    body: JSON.stringify({ title }),
  });
}

// ---------- 页面元素 ----------
// 泛型参数决定返回类型：拿到 HTMLInputElement 才有 .value，HTMLButtonElement 才有 .disabled
function getById<T extends HTMLElement>(id: string): T {
  const el = document.getElementById(id);
  if (el === null) {
    throw new Error(`页面缺少 #${id} 元素`);
  }
  return el as T;
}

const form = getById<HTMLFormElement>("form");
const titleInput = getById<HTMLInputElement>("title");
const addBtn = getById<HTMLButtonElement>("addBtn");
const msg = getById<HTMLParagraphElement>("msg");
const list = getById<HTMLUListElement>("list");

// ---------- 把数据画到页面上 ----------
function render(todos: Todo[]): void {
  list.innerHTML = "";
  for (const t of todos) {
    const li = document.createElement("li");
    // 用 textContent 不用 innerHTML：有人输入 <img src=x onerror=alert(1)>，也只会当文字显示
    li.textContent = t.done ? `✔ ${t.title}` : t.title;
    list.appendChild(li);
  }
}

// catch 拿到的是 unknown：抛出来的不一定是 Error 对象，先判断再取 message
function showError(e: unknown): void {
  msg.textContent = e instanceof Error ? e.message : String(e);
}

async function reload(): Promise<void> {
  try {
    render(await getTodos());
    msg.textContent = "";
  } catch (e) {
    showError(e);
  }
}

// ---------- 提交表单 ----------
form.addEventListener("submit", async (e: SubmitEvent) => {
  // 阻止浏览器默认的整页提交（那会跳转、刷新页面），改由 fetch 在后台发请求
  e.preventDefault();

  // 请求没回来之前禁用按钮：手机网络慢时用户会连点，不禁用就会提交两条
  addBtn.disabled = true;
  try {
    await addTodo(titleInput.value);
    titleInput.value = "";
    await reload();
  } catch (err) {
    showError(err);
  } finally {
    addBtn.disabled = false;
  }
});

// 页面打开时先拉一次列表
void reload();
