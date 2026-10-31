// code/ch08/Answer2-ClosePlayer/wwwroot/api.js
// 页面里所有请求共用；先于 app.js 加载
// 所有请求都走这个函数：成功返回数据（没有正文时返回 null），失败抛出 Error
async function api(method, url, data) {
  const options = { method, headers: {} };
  if (data instanceof FormData) {
    // 上传文件：原样交给 fetch。Content-Type 由浏览器自动生成（里面带分隔符 boundary），不能手写
    options.body = data;
  } else if (data !== undefined) {
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

// 上传文件并报告进度。fetch 拿不到"已经发出去多少"，所以这里用浏览器更早的 XMLHttpRequest
// 结果约定和 api() 一样：成功返回数据，失败抛出带 message、errors 的 Error，调用方照样 try/catch
function upload(url, formData, onProgress) {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    xhr.open('POST', url);

    // 每发出去一批数据触发一次：loaded 是已发字节数，total 是总字节数
    xhr.upload.onprogress = (e) => {
      if (e.lengthComputable) onProgress(Math.round((e.loaded / e.total) * 100));
    };

    // 收到响应（不论状态码多少）时触发
    xhr.onload = () => {
      const isJson = (xhr.getResponseHeader('Content-Type') ?? '').includes('json');
      const body = isJson && xhr.responseText ? JSON.parse(xhr.responseText) : null;
      if (xhr.status >= 200 && xhr.status < 300) {
        resolve(body);
        return;
      }
      let message = `服务器出错（状态码 ${xhr.status}）`;
      if (xhr.status === 400) message = '输入有误，请看红字提示';
      if (xhr.status === 404) message = '这条数据不存在，可能已被删除';
      if (xhr.status === 413) message = '文件太大，服务器拒绝接收';
      const error = new Error(message);
      error.errors = body?.errors ?? {};
      reject(error);
    };

    // 网络不通、或者服务器中途断开连接时触发
    xhr.onerror = () => reject(new Error('上传中断：连不上服务器，或者被服务器断开'));

    // 和 fetch 一样，FormData 交给它自己处理，不要设置 Content-Type
    xhr.send(formData);
  });
}
