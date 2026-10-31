// code/ch06/Answer2-FullSave/wwwroot/app.js
const msg = document.getElementById('msg');
const form = document.getElementById('productForm');
let editingId = null; // null = 新增模式；有值 = 正在修改这个 Id

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
    const ops = document.createElement('td');
    // 这里可以用 innerHTML：拼进去的只有后端给的数字 id，没有用户输入的文字
    // data-action、data-id 给文件末尾的统一点击处理用
    ops.innerHTML =
      `<button data-action="edit" data-id="${p.id}">编辑</button> ` +
      `<button data-action="delete" data-id="${p.id}">删除</button>`;
    tr.appendChild(ops);
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

// 回到新增模式
function resetForm() {
  editingId = null;
  form.reset();
  clearErrors();
  document.getElementById('formTitle').textContent = '新增产品';
}

// 点「编辑」：先从后端取最新数据再填表，列表里的可能已经过时
async function startEdit(id) {
  try {
    const p = await api('GET', '/api/products/' + id);
    editingId = id;
    clearErrors();
    document.getElementById('code').value = p.code;
    document.getElementById('name').value = p.name;
    document.getElementById('price').value = p.price;
    document.getElementById('formTitle').textContent = `修改产品（Id ${id}）`;
  } catch (err) {
    msg.textContent = err.message;
  }
}

async function removeProduct(id) {
  // confirm 弹出确认框，点「取消」返回 false
  if (!confirm('确定删除这条产品吗？')) return;
  try {
    await api('DELETE', '/api/products/' + id);
    if (editingId === id) resetForm(); // 正在改的那条被删了，表单也要回到新增模式
    await search();
    msg.textContent = `已删除 Id ${id}`;
  } catch (err) {
    msg.textContent = err.message;
  }
}

async function save(e) {
  e.preventDefault();
  clearErrors();
  const product = {
    code: document.getElementById('code').value.trim(),
    name: document.getElementById('name').value.trim(),
    price: Number(document.getElementById('price').value),
  };
  try {
    let text;
    if (editingId === null) {
      const created = await api('POST', '/api/products', product);
      text = `已新增：${created.code} ${created.name}（Id ${created.id}）`;
    } else {
      const updated = await api('PUT', '/api/products/' + editingId, product);
      text = `已修改：${updated.code} ${updated.name}（Id ${updated.id}）`;
    }
    resetForm();
    await search();
    msg.textContent = text;
  } catch (err) {
    showErrors(err.errors ?? {});
    msg.textContent = err.message;
  }
}

// 整个表格只挂一个点击处理：按钮是每次查询后新画的，挂在按钮上会跟着丢
document.getElementById('rows').addEventListener('click', (e) => {
  const button = e.target.closest('button'); // 点在按钮上才处理
  if (button === null) return;
  const id = Number(button.dataset.id); // data-id 读出来是字符串，转成数字
  if (button.dataset.action === 'edit') startEdit(id);
  if (button.dataset.action === 'delete') removeProduct(id);
});

form.addEventListener('submit', save);
document.getElementById('btnSearch').addEventListener('click', search);
search();
