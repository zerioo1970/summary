// React 19 + TypeScript 实战教程：配套练习服务
// 只使用 Node.js 内置模块，不需要安装任何依赖。读者不需要读懂本文件，教程正文只讲前端如何调用这些接口。
//
// 启动：  node server/practice-server.mjs
// 重置：  node server/practice-server.mjs --reset
// 端口：  默认 3001，可用环境变量 API_PORT 修改。
//
// 所有 /api 接口都支持两个练习参数：
//   ?delay=毫秒   改变响应等待时间（默认 500 毫秒，便于看到“加载中”）
//   ?fail=状态码  直接返回这个错误状态，例如 ?fail=500、?fail=404，用来练习失败处理

import { createServer } from 'node:http';
import { randomUUID } from 'node:crypto';
import { mkdir, readFile, writeFile, rm } from 'node:fs/promises';
import { dirname, extname, join } from 'node:path';
import { Readable } from 'node:stream';
import { fileURLToPath } from 'node:url';

const serverDir = dirname(fileURLToPath(import.meta.url));
const dataDir = join(serverDir, 'data');
const dataFile = join(dataDir, 'practice-data.json');
const uploadDir = join(dataDir, 'uploads');
const port = Number(process.env.API_PORT ?? 3001);
const defaultDelay = 500;
const allowedOrigins = ['http://127.0.0.1:5173', 'http://localhost:5173', 'http://127.0.0.1:4173', 'http://localhost:4173'];

// ---------- 初始数据 ----------

function createSeed() {
  return {
    tasks: [
      { id: 't1', title: '阅读准备篇', completed: true },
      { id: 't2', title: '完成第一个页面', completed: false },
    ],
    people: Array.from({ length: 12 }, (_, index) => ({
      id: 'p' + (index + 1),
      name: ['林晓', '王磊', '陈静', '赵阳', '孙悦', '周宁', '吴迪', '郑欣', '冯涛', '何琳', '许诺', '唐果'][index],
      email: 'user' + (index + 1) + '@example.com',
      role: index % 4 === 0 ? '管理员' : '成员',
      avatarUrl: null,
    })),
    accounts: {
      admin: { avatarUrl: null },
      member: { avatarUrl: null },
    },
  };
}

const articles = [
  { id: 1, title: 'React 组件入门', summary: '用函数描述一块界面。', publishedAt: '2026-11-01T02:00:00Z', content: '组件是返回界面描述的函数。把页面拆成组件，可以分别编写和复用。' },
  { id: 2, title: 'TypeScript 类型检查', summary: '在运行前发现数据约定错误。', publishedAt: '2026-11-02T03:30:00Z', content: '类型标注描述数据应有的形状，编辑器和检查命令据此提示错误。' },
  { id: 3, title: '用 state 更新页面', summary: '让数据变化带动界面变化。', publishedAt: '2026-11-03T08:00:00Z', content: '调用 set 函数后，React 会重新运行组件并更新页面。' },
  { id: 4, title: '从接口读取数据', summary: 'fetch、加载状态与错误处理。', publishedAt: '2026-11-04T09:15:00Z', content: '读取接口要同时考虑等待、成功和失败三种情况。' },
];

const products = [
  { id: 1, name: 'React 入门书', category: '图书', price: 38, stock: 12, description: '适合第一次接触 React 的读者。' },
  { id: 2, name: '机械键盘', category: '数码', price: 299, stock: 5, description: '87 键布局，适合长时间打字。' },
  { id: 3, name: 'TypeScript 手册', category: '图书', price: 45, stock: 0, description: '常用类型写法速查。' },
  { id: 4, name: '无线鼠标', category: '数码', price: 89, stock: 20, description: '静音按键，蓝牙连接。' },
  { id: 5, name: '保温杯', category: '生活', price: 59, stock: 8, description: '500 毫升，保温 12 小时。' },
  { id: 6, name: '笔记本', category: '生活', price: 12, stock: 30, description: 'A5 方格内页。' },
];

const users = {
  admin: { username: 'admin', password: 'learn1234', displayName: '管理员小林', role: 'admin', joinedAt: '2026-01-15' },
  member: { username: 'member', password: 'learn1234', displayName: '成员小王', role: 'member', joinedAt: '2026-03-02' },
};

// ---------- 数据读写 ----------

async function loadData() {
  try {
    return JSON.parse(await readFile(dataFile, 'utf8'));
  } catch {
    const seed = createSeed();
    await saveData(seed);
    return seed;
  }
}

async function saveData(data) {
  await mkdir(dataDir, { recursive: true });
  await writeFile(dataFile, JSON.stringify(data, null, 2), 'utf8');
}

async function resetData() {
  await rm(dataDir, { recursive: true, force: true });
  await saveData(createSeed());
}

// 登录会话只保存在内存中：重启练习服务后需要重新登录。
const sessions = new Map();
const sessionMaxAge = 30 * 60;

// ---------- 工具函数 ----------

class HttpError extends Error {
  constructor(status, message, fieldErrors) {
    super(message);
    this.status = status;
    this.fieldErrors = fieldErrors;
  }
}

const statusText = { 400: '请求格式不正确', 401: '请先登录', 403: '没有权限', 404: '找不到资源', 409: '数据冲突', 413: '文件太大', 422: '提交的数据不符合要求', 500: '服务器内部错误', 503: '服务暂时不可用' };

function corsHeaders(request, url) {
  const origin = request.headers.origin;
  if (!origin || url.searchParams.get('cors') === 'off' || !allowedOrigins.includes(origin)) return {};
  return {
    'Access-Control-Allow-Origin': origin,
    'Access-Control-Allow-Credentials': 'true',
    'Access-Control-Allow-Methods': 'GET, POST, PATCH, DELETE, OPTIONS',
    'Access-Control-Allow-Headers': 'Content-Type',
    Vary: 'Origin',
  };
}

function send(response, status, data, headers = {}) {
  if (data === undefined) {
    response.writeHead(status, headers);
    response.end();
    return;
  }
  response.writeHead(status, { 'Content-Type': 'application/json; charset=utf-8', ...headers });
  response.end(JSON.stringify(data));
}

async function readJson(request) {
  let body = '';
  request.setEncoding('utf8');
  for await (const chunk of request) {
    body += chunk;
    if (body.length > 100_000) throw new HttpError(413, '请求内容太大');
  }
  try {
    return body === '' ? {} : JSON.parse(body);
  } catch {
    throw new HttpError(400, '请求正文不是有效的 JSON');
  }
}

function getSessionUser(request) {
  const cookie = request.headers.cookie ?? '';
  const match = cookie.match(/(?:^|;\s*)practice_session=([^;]+)/);
  if (!match) return null;
  const session = sessions.get(match[1]);
  if (!session || session.expiresAt < Date.now()) {
    sessions.delete(match[1]);
    return null;
  }
  return users[session.username];
}

function requireUser(request) {
  const user = getSessionUser(request);
  if (!user) throw new HttpError(401, '登录已失效，请重新登录');
  return user;
}

function requireAdmin(request) {
  const user = requireUser(request);
  if (user.role !== 'admin') throw new HttpError(403, '只有管理员可以执行这个操作');
  return user;
}

function publicUser(user, data) {
  return { username: user.username, displayName: user.displayName, role: user.role, avatarUrl: data.accounts[user.username]?.avatarUrl ?? null };
}

function validateTitle(value) {
  const title = typeof value === 'string' ? value.trim() : '';
  if (title === '') throw new HttpError(422, '任务名称不能为空', { title: '请填写任务名称' });
  if (title.length > 30) throw new HttpError(422, '任务名称太长', { title: '任务名称最多 30 个字' });
  return title;
}

async function readAvatar(request) {
  const length = Number(request.headers['content-length'] ?? 0);
  if (length > 1_200_000) throw new HttpError(413, '图片不能超过 1MB');
  const webRequest = new Request('http://practice.local' + request.url, {
    method: 'POST',
    headers: Object.entries(request.headers).filter(([, value]) => typeof value === 'string'),
    body: Readable.toWeb(request),
    duplex: 'half',
  });
  let form;
  try {
    form = await webRequest.formData();
  } catch {
    throw new HttpError(400, '请用表单格式上传文件');
  }
  const file = form.get('avatar');
  if (!(file instanceof File)) throw new HttpError(422, '没有收到图片', { avatar: '请选择一张图片' });
  const types = { 'image/png': '.png', 'image/jpeg': '.jpg', 'image/webp': '.webp' };
  if (!types[file.type]) throw new HttpError(422, '只支持 PNG、JPEG、WebP 图片', { avatar: '只支持 PNG、JPEG、WebP 图片' });
  if (file.size > 1024 * 1024) throw new HttpError(413, '图片不能超过 1MB', { avatar: '图片不能超过 1MB' });
  await mkdir(uploadDir, { recursive: true });
  const fileName = randomUUID() + types[file.type];
  await writeFile(join(uploadDir, fileName), Buffer.from(await file.arrayBuffer()));
  return '/uploads/' + fileName;
}

const wait = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

// ---------- 路由 ----------

async function handleApi(request, response, url) {
  const method = request.method;
  const path = url.pathname;
  const parts = path.split('/').filter(Boolean); // ['api', 'tasks', 'id']

  if (method === 'GET' && path === '/api/health') return send(response, 200, { ok: true });

  // 文章（公开）
  if (method === 'GET' && path === '/api/articles') {
    const mode = url.searchParams.get('mode');
    if (mode === 'empty') return send(response, 200, []);
    if (mode === 'bad') return send(response, 200, [{ id: 'one', title: 123 }]);
    return send(response, 200, articles.map(({ content, ...item }) => item));
  }
  if (method === 'GET' && parts[1] === 'articles' && parts.length === 3) {
    const article = articles.find((item) => item.id === Number(parts[2]));
    if (!article) throw new HttpError(404, '没有这篇文章');
    return send(response, 200, article);
  }

  // 搜索：关键词越短响应越慢，用来重现“先发的请求后返回”的问题
  if (method === 'GET' && path === '/api/search') {
    const q = (url.searchParams.get('q') ?? '').trim().toLowerCase();
    const result = articles.filter((item) => item.title.toLowerCase().includes(q)).map(({ content, ...item }) => item);
    return send(response, 200, { query: q, items: result });
  }

  // 商品（公开）
  if (method === 'GET' && path === '/api/products') return send(response, 200, products);
  if (method === 'GET' && parts[1] === 'products' && parts.length === 3) {
    const product = products.find((item) => item.id === Number(parts[2]));
    if (!product) throw new HttpError(404, '没有这件商品');
    return send(response, 200, product);
  }

  // 任务（公开，保存到 practice-data.json）
  if (parts[1] === 'tasks') {
    const data = await loadData();
    if (method === 'GET' && parts.length === 2) return send(response, 200, data.tasks);
    if (method === 'POST' && parts.length === 2) {
      const body = await readJson(request);
      const task = { id: randomUUID(), title: validateTitle(body.title), completed: false };
      data.tasks.push(task);
      await saveData(data);
      return send(response, 201, task);
    }
    if (parts.length === 3) {
      const index = data.tasks.findIndex((item) => item.id === parts[2]);
      if (index === -1) throw new HttpError(404, '任务不存在，可能已被删除');
      if (method === 'PATCH') {
        const body = await readJson(request);
        const next = { ...data.tasks[index] };
        if ('title' in body) next.title = validateTitle(body.title);
        if ('completed' in body) {
          if (typeof body.completed !== 'boolean') throw new HttpError(422, 'completed 必须是 true 或 false');
          next.completed = body.completed;
        }
        data.tasks[index] = next;
        await saveData(data);
        return send(response, 200, next);
      }
      if (method === 'DELETE') {
        data.tasks.splice(index, 1);
        await saveData(data);
        return send(response, 204);
      }
    }
  }

  // 登录与会话
  if (method === 'POST' && path === '/api/auth/login') {
    const body = await readJson(request);
    const user = users[body.username];
    if (!user || user.password !== body.password) throw new HttpError(401, '用户名或密码不正确');
    const token = randomUUID();
    sessions.set(token, { username: user.username, expiresAt: Date.now() + sessionMaxAge * 1000 });
    const data = await loadData();
    return send(response, 200, publicUser(user, data), {
      'Set-Cookie': `practice_session=${token}; HttpOnly; SameSite=Lax; Path=/; Max-Age=${sessionMaxAge}`,
    });
  }
  if (method === 'GET' && path === '/api/auth/me') {
    const user = requireUser(request);
    return send(response, 200, publicUser(user, await loadData()));
  }
  if (method === 'POST' && path === '/api/auth/logout') {
    const match = (request.headers.cookie ?? '').match(/(?:^|;\s*)practice_session=([^;]+)/);
    if (match) sessions.delete(match[1]);
    return send(response, 204, undefined, { 'Set-Cookie': 'practice_session=; HttpOnly; SameSite=Lax; Path=/; Max-Age=0' });
  }
  // 练习“登录失效”：让所有已登录会话立即过期
  if (method === 'POST' && path === '/api/dev/expire-sessions') {
    sessions.clear();
    return send(response, 204);
  }

  // 当前用户资料与头像
  if (method === 'GET' && path === '/api/account') {
    const user = requireUser(request);
    const data = await loadData();
    return send(response, 200, { ...publicUser(user, data), joinedAt: user.joinedAt });
  }
  if (method === 'POST' && path === '/api/account/avatar') {
    const user = requireUser(request);
    const avatarUrl = await readAvatar(request);
    const data = await loadData();
    data.accounts[user.username] = { avatarUrl };
    await saveData(data);
    return send(response, 200, publicUser(user, data));
  }

  // 人员管理（仅管理员）
  if (method === 'GET' && path === '/api/people') {
    requireAdmin(request);
    const data = await loadData();
    const pageSize = Number(url.searchParams.get('pageSize') ?? 5);
    const page = Number(url.searchParams.get('page') ?? 1);
    if (!Number.isInteger(pageSize) || pageSize < 1 || pageSize > 20) throw new HttpError(400, 'pageSize 必须是 1 到 20 的整数');
    if (!Number.isInteger(page) || page < 1) throw new HttpError(400, 'page 必须是大于 0 的整数');
    const total = data.people.length;
    const totalPages = Math.max(1, Math.ceil(total / pageSize));
    const items = data.people.slice((page - 1) * pageSize, page * pageSize);
    return send(response, 200, { items, page, pageSize, total, totalPages });
  }
  if (method === 'POST' && path === '/api/people/batch-delete') {
    requireAdmin(request);
    const body = await readJson(request);
    if (!Array.isArray(body.ids) || body.ids.length === 0 || body.ids.some((id) => typeof id !== 'string')) {
      throw new HttpError(422, '请提供要删除的人员编号');
    }
    const data = await loadData();
    const before = data.people.length;
    data.people = data.people.filter((person) => !body.ids.includes(person.id));
    await saveData(data);
    return send(response, 200, { deleted: before - data.people.length });
  }
  if (parts[1] === 'people' && parts.length >= 3) {
    requireAdmin(request);
    const data = await loadData();
    const person = data.people.find((item) => item.id === parts[2]);
    if (!person) throw new HttpError(404, '这位人员不存在，可能已被删除');
    if (method === 'PATCH' && parts.length === 3) {
      const body = await readJson(request);
      const name = typeof body.name === 'string' ? body.name.trim() : '';
      if (name === '' || name.length > 20) throw new HttpError(422, '姓名须为 1 到 20 个字', { name: '姓名须为 1 到 20 个字' });
      person.name = name;
      await saveData(data);
      return send(response, 200, person);
    }
    if (method === 'POST' && parts[3] === 'avatar') {
      person.avatarUrl = await readAvatar(request);
      await saveData(data);
      return send(response, 200, person);
    }
  }

  throw new HttpError(404, '没有这个接口：' + method + ' ' + path);
}

async function serveUpload(response, url) {
  const name = url.pathname.slice('/uploads/'.length);
  if (!/^[0-9a-f-]+\.(png|jpg|webp)$/.test(name)) return send(response, 404, { message: '找不到文件' });
  try {
    const types = { '.png': 'image/png', '.jpg': 'image/jpeg', '.webp': 'image/webp' };
    const file = await readFile(join(uploadDir, name));
    response.writeHead(200, { 'Content-Type': types[extname(name)], 'Cache-Control': 'no-cache' });
    response.end(file);
  } catch {
    send(response, 404, { message: '找不到文件' });
  }
}

const server = createServer(async (request, response) => {
  const url = new URL(request.url ?? '/', 'http://127.0.0.1');
  const cors = corsHeaders(request, url);
  for (const [key, value] of Object.entries(cors)) response.setHeader(key, value);
  response.on('finish', () => console.log(`${request.method} ${request.url} → ${response.statusCode}`));

  if (request.method === 'OPTIONS') return send(response, 204);
  if (request.method === 'GET' && url.pathname.startsWith('/uploads/')) return serveUpload(response, url);
  if (!url.pathname.startsWith('/api/')) return send(response, 404, { message: '练习服务只提供 /api 和 /uploads 地址' });

  try {
    const delayParam = url.searchParams.get('delay');
    let delay = delayParam === null ? defaultDelay : Math.min(Math.max(Number(delayParam) || 0, 0), 10_000);
    if (delayParam === null && url.pathname === '/api/search') {
      const q = (url.searchParams.get('q') ?? '').trim();
      delay = q.length <= 2 ? 1500 : 300;
    }
    await wait(delay);
    const fail = Number(url.searchParams.get('fail'));
    if (statusText[fail]) throw new HttpError(fail, '练习用的模拟错误：' + statusText[fail]);
    await handleApi(request, response, url);
  } catch (error) {
    if (response.headersSent) return;
    if (error instanceof HttpError) {
      send(response, error.status, { message: error.message, ...(error.fieldErrors ? { fieldErrors: error.fieldErrors } : {}) });
    } else {
      console.error(error);
      send(response, 500, { message: '服务器内部错误' });
    }
  }
});

if (process.argv.includes('--reset')) {
  await resetData();
  console.log('练习数据已恢复为初始状态：' + dataFile);
} else {
  server.listen(port, '127.0.0.1', () => {
    console.log(`练习服务已启动：http://127.0.0.1:${port}/api/health`);
    console.log('按 Ctrl+C 停止。数据文件：' + dataFile);
  });
}
