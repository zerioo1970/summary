// code/ch01/Answer2-AutoLoad/wwwroot/app.js
async function loadProducts() {
  const response = await fetch('/api/products');
  const products = await response.json();

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

document.getElementById('btnLoad').addEventListener('click', loadProducts);
// 脚本在 body 最后执行，表格已经存在，直接调一次就是"打开即加载"
loadProducts();
