// code/ch02/Answer2-MinPrice/wwwroot/app.js
const msg = document.getElementById('msg');

// 把产品数组画成表格行；查列表和查一条都用它
function renderRows(products) {
  const tbody = document.getElementById('rows');
  tbody.innerHTML = '';
  for (const p of products) {
    const tr = document.createElement('tr');
    for (const value of [p.id, p.code, p.name, p.price]) {
      const td = document.createElement('td');
      td.textContent = value;
      tr.appendChild(td);
    }
    tbody.appendChild(tr);
  }
}

async function search() {
  const keyword = document.getElementById('keyword').value.trim();
  const minPrice = document.getElementById('minPrice').value;
  const query = new URLSearchParams({ keyword });
  // 没填就不传：传空字符串给 decimal? 会得到 400
  if (minPrice !== '') query.append('minPrice', minPrice);
  msg.textContent = '加载中…';
  const response = await fetch('/api/products?' + query);
  const products = await response.json();
  renderRows(products);
  msg.textContent = products.length === 0 ? '没有符合条件的产品' : `共 ${products.length} 条`;
}

async function getOne() {
  const id = document.getElementById('productId').value;
  if (id === '') {
    msg.textContent = '请先输入 Id';
    return;
  }
  msg.textContent = '加载中…';
  const response = await fetch('/api/products/' + id);
  // 404 的响应没有正文，必须先判断状态码，再决定要不要 .json()
  if (response.status === 404) {
    renderRows([]);
    msg.textContent = `没有 Id 为 ${id} 的产品`;
    return;
  }
  const product = await response.json();
  renderRows([product]); // 单个对象包成数组，复用同一个画表函数
  msg.textContent = '找到 1 条';
}

document.getElementById('btnSearch').addEventListener('click', search);
document.getElementById('btnGetOne').addEventListener('click', getOne);
search(); // 打开页面先显示全部
