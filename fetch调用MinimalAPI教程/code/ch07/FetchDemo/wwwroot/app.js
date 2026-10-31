// code/ch07/FetchDemo/wwwroot/app.js
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
    const imgCell = document.createElement('td');
    if (p.imageUrl) {
      // 用 createElement 而不是拼 innerHTML：alt 里放的是用户填的名称
      const img = document.createElement('img');
      img.src = p.imageUrl; // 形如 /uploads/xxx.png，就是 wwwroot 里的静态文件
      img.alt = p.name;
      img.className = 'thumb';
      imgCell.appendChild(img);
    }
    tr.appendChild(imgCell);
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
    document.getElementById('image').value = ''; // 清掉上一次选的文件，改文字时不会误传旧图
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
  // files 是用户选中的文件列表；没选时 files[0] 是 undefined
  const file = document.getElementById('image').files[0];

  // 第一步：照旧保存文字信息，拿到产品 Id
  let saved;
  let text;
  try {
    if (editingId === null) {
      saved = await api('POST', '/api/products', product);
      text = `已新增：${saved.code} ${saved.name}（Id ${saved.id}）`;
    } else {
      saved = await api('PUT', '/api/products/' + editingId, product);
      text = `已修改：${saved.code} ${saved.name}（Id ${saved.id}）`;
    }
  } catch (err) {
    showErrors(err.errors ?? {});
    msg.textContent = err.message;
    return;
  }

  // 第二步：选了图片才上传。新增时第一步才有 Id，所以必须分两步
  if (file !== undefined) {
    const formData = new FormData();
    formData.append('image', file); // 名字 image 必须和后端参数名一致
    try {
      await api('POST', `/api/products/${saved.id}/image`, formData);
    } catch (err) {
      // 文字已经存好了，只是图片没传上去：转到修改模式，重选图片再点保存即可
      await search();
      await startEdit(saved.id);
      showErrors(err.errors ?? {});
      msg.textContent = `产品已保存，但图片上传失败：${err.message}`;
      return;
    }
  }

  resetForm();
  await search();
  msg.textContent = text;
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
