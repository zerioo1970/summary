// code/ch04/Answer1-GetOne/wwwroot/api.js
// 页面里所有请求共用；先于 app.js 加载
// 所有请求都走这个函数：成功返回数据（没有正文时返回 null），失败抛出 Error
async function api(method, url, data) {
  const options = { method, headers: {} };
  if (data !== undefined) {
    options.headers['Content-Type'] = 'application/json';
    options.body = JSON.stringify(data);
  }

  let response;
  try {
    response = await fetch(url, options);
  } catch {
    // fetch 只在网络不通时抛异常；404、500 这些不会进这里
    throw new Error('连不上服务器，请确认后端程序在运行');
  }

  // 先按文本读：204、404 没有正文，开发环境的 400/500 可能是纯文本，都不能直接 .json()
  const text = await response.text();
  const isJson = (response.headers.get('Content-Type') ?? '').includes('json');
  const body = isJson && text ? JSON.parse(text) : null;

  if (response.ok) return body;

  let message = `服务器出错（状态码 ${response.status}）`;
  if (response.status === 400) message = '输入有误，请看红字提示';
  if (response.status === 404) message = '这条数据不存在，可能已被删除';
  const error = new Error(message);
  error.errors = body?.errors ?? {}; // 校验错误按字段带出去，没有就是空对象
  throw error;
}
