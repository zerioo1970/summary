// code/ch04/Answer2-Error500/wwwroot/app.js
const msg = document.getElementById('msg');

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
  const query = new URLSearchParams({ keyword });
  try {
    const products = await api('GET', '/api/products?' + query);
    renderRows(products);
    msg.textContent = products.length === 0 ? '没有符合条件的产品' : `共 ${products.length} 条`;
  } catch (err) {
    msg.textContent = err.message;
  }
}

function clearErrors() {
  for (const span of document.querySelectorAll('.err')) span.textContent = '';
}

function showErrors(errors) {
  for (const [field, messages] of Object.entries(errors)) {
    document.getElementById('err-' + field).textContent = messages.join('；');
  }
}

async function addProduct(e) {
  e.preventDefault();
  clearErrors();
  const product = {
    code: document.getElementById('code').value.trim(),
    name: document.getElementById('name').value.trim(),
    price: Number(document.getElementById('price').value),
  };
  try {
    const created = await api('POST', '/api/products', product);
    document.getElementById('addForm').reset();
    await search();
    msg.textContent = `已新增：${created.code} ${created.name}（Id ${created.id}）`;
  } catch (err) {
    showErrors(err.errors ?? {});
    msg.textContent = err.message;
  }
}

async function deleteProduct() {
  const id = document.getElementById('deleteId').value;
  if (id === '') {
    msg.textContent = '请先输入要删除的 Id';
    return;
  }
  // confirm 弹出确认框，点「取消」返回 false
  if (!confirm(`确定删除 Id ${id} 吗？`)) return;
  try {
    await api('DELETE', '/api/products/' + id); // 成功时是 204，api 返回 null，不用接
    await search();
    msg.textContent = `已删除 Id ${id}`;
  } catch (err) {
    msg.textContent = err.message;
  }
}

document.getElementById('addForm').addEventListener('submit', addProduct);
document.getElementById('btnDelete').addEventListener('click', deleteProduct);
// 不用新写错误处理：api 里"其他状态码"那条分支已经覆盖了 500
document.getElementById('btnBoom').addEventListener('click', async () => {
  try {
    await api('GET', '/api/boom');
  } catch (err) {
    msg.textContent = err.message;
  }
});
document.getElementById('btnSearch').addEventListener('click', search);
search();
