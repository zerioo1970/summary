// code/ch01/FetchDemo/wwwroot/app.js
async function loadProducts() {
  // 相对地址：页面和接口在同一个网站，不用写 http://localhost:5080
  const response = await fetch('/api/products');
  // 把响应正文从 JSON 文本解析成数组；读正文也要等，所以也要 await
  const products = await response.json();

  const tbody = document.getElementById('rows');
  tbody.innerHTML = ''; // 先清空，否则点两次会出现两份数据
  for (const p of products) {
    const tr = document.createElement('tr');
    // 字段名小写开头：C# 的 Name 到了 JSON 里变成 name
    for (const value of [p.id, p.code, p.name, p.price]) {
      const td = document.createElement('td');
      // 用 textContent 不用 innerHTML：名称里就算有 <b> 也只当文字显示
      td.textContent = value;
      tr.appendChild(td);
    }
    tbody.appendChild(tr);
  }
}

document.getElementById('btnLoad').addEventListener('click', loadProducts);
