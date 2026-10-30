// code/ch03/Answer1-NameLength/wwwroot/app.js
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
  const response = await fetch('/api/products?' + query);
  const products = await response.json();
  renderRows(products);
  msg.textContent = products.length === 0 ? '没有符合条件的产品' : `共 ${products.length} 条`;
}

// 清掉上一次的红字，否则改对了旧错误还挂在那里
function clearErrors() {
  for (const span of document.querySelectorAll('.err')) span.textContent = '';
}

// errors 形如 { code: ['编码已存在'], price: ['价格必须大于 0'] }
function showErrors(errors) {
  for (const [field, messages] of Object.entries(errors)) {
    document.getElementById('err-' + field).textContent = messages.join('；');
  }
}

async function addProduct(e) {
  // 阻止浏览器按传统方式提交表单：不阻止的话页面会整页刷新，fetch 白发
  e.preventDefault();
  clearErrors();

  const product = {
    code: document.getElementById('code').value.trim(),
    name: document.getElementById('name').value.trim(),
    // 输入框的值永远是字符串，转成数字再发；空字符串转出来是 0，交给后端校验
    price: Number(document.getElementById('price').value),
  };

  const response = await fetch('/api/products', {
    method: 'POST',
    // 告诉后端正文是 JSON；不写这行后端返回 415
    headers: { 'Content-Type': 'application/json' },
    // 对象必须先转成 JSON 文本，fetch 不会自动转
    body: JSON.stringify(product),
  });

  // response.ok 表示状态码在 200–299 之间
  if (!response.ok) {
    const problem = await response.json();
    showErrors(problem.errors);
    return;
  }

  const created = await response.json();
  document.getElementById('addForm').reset(); // 成功后清空表单
  await search(); // 重新查列表，新数据才会出现
  msg.textContent = `已新增：${created.code} ${created.name}（Id ${created.id}）`;
}

document.getElementById('addForm').addEventListener('submit', addProduct);
document.getElementById('btnSearch').addEventListener('click', search);
search();
