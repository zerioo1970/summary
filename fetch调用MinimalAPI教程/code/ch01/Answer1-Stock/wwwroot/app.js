// code/ch01/Answer1-Stock/wwwroot/app.js
async function loadProducts() {
  const response = await fetch('/api/products');
  const products = await response.json();

  const tbody = document.getElementById('rows');
  tbody.innerHTML = '';
  for (const p of products) {
    const tr = document.createElement('tr');
    // 多取一个 p.stock（小写 s）
    for (const value of [p.id, p.code, p.name, p.price, p.stock]) {
      const td = document.createElement('td');
      td.textContent = value;
      tr.appendChild(td);
    }
    tbody.appendChild(tr);
  }
}

document.getElementById('btnLoad').addEventListener('click', loadProducts);
