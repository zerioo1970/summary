# 第 10 章 · 第八步：封装 axios 与 API 层

> 上一章：[09-创建React19前端](09-创建React19前端.md) ｜ 下一章：[11-React-CRUD页面](11-React-CRUD页面.md)

## 10.1 axios 实例与拦截器

`src/api/request.js`——统一 baseURL、统一处理后端的 `Result` 信封：

```javascript
import axios from 'axios';

const request = axios.create({
  baseURL: 'http://localhost:8080/api', // 后端地址前缀
  timeout: 10000,
});

// 响应拦截器：自动剥离外层 Result，出错时统一提示
request.interceptors.response.use(
  (response) => {
    const res = response.data; // { code, message, data }
    if (res.code === 200) {
      return res.data;         // 直接把 data 交给业务代码
    }
    alert(res.message || '请求失败');
    return Promise.reject(new Error(res.message));
  },
  (error) => {
    alert('网络错误：' + error.message);
    return Promise.reject(error);
  }
);

export default request;
```

**讲解**：通过响应拦截器，业务代码里 `await getUsers()` 拿到的直接就是用户数组，不用每次都写 `res.data.data`，非常清爽。

## 10.2 用户 API 层

`src/api/userApi.js`——把每个后端接口封装成一个函数：

```javascript
import request from './request';

// 查询全部
export const getUsers = () => request.get('/users');

// 按 id 查询
export const getUser = (id) => request.get(`/users/${id}`);

// 新增
export const createUser = (user) => request.post('/users', user);

// 修改
export const updateUser = (id, user) => request.put(`/users/${id}`, user);

// 删除
export const deleteUser = (id) => request.delete(`/users/${id}`);
```

**分层的好处**：组件里不直接写 URL，将来后端地址或路径变了，只改这一个文件。

---

> 下一章 👉 [11-React-CRUD页面](11-React-CRUD页面.md)
