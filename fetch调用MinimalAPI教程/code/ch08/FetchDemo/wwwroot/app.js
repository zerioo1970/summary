// code/ch08/FetchDemo/wwwroot/app.js
const msg = document.getElementById('msg');
const form = document.getElementById('productForm');
let editingId = null; // null = 新增模式；有值 = 正在修改这个 Id
const MAX_VIDEO_BYTES = 100 * 1024 * 1024; // 和后端一致：100MB

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
    // 有视频才显示「播放」
    if (p.videoUrl) {
      ops.innerHTML += ` <button data-action="play" data-id="${p.id}">播放</button>`;
    }
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
    document.getElementById('video').value = '';
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

// 显示上传进度；percent 为 null 时隐藏进度条
function showProgress(percent) {
  const box = document.getElementById('progressBox');
  if (percent === null) {
    box.hidden = true;
    return;
  }
  box.hidden = false;
  document.getElementById('progress').value = percent;
  document.getElementById('progressText').textContent = `${percent}%`;
}

// 第二步：依次上传选中的图片和视频。任何一个失败都抛出 Error，由 save 统一处理
async function uploadFiles(id) {
  const image = document.getElementById('image').files[0];
  const video = document.getElementById('video').files[0];

  if (image !== undefined) {
    const formData = new FormData();
    formData.append('image', image);
    await api('POST', `/api/products/${id}/image`, formData);
  }

  if (video !== undefined) {
    // 先在浏览器里检查大小：不合格的文件不用等它传完 100MB 才被后端拒绝
    if (video.size > MAX_VIDEO_BYTES) {
      const error = new Error('输入有误，请看红字提示');
      error.errors = { video: ['视频不能超过 100MB'] };
      throw error;
    }
    const formData = new FormData();
    formData.append('video', video);
    showProgress(0);
    try {
      await upload(`/api/products/${id}/video`, formData, showProgress);
    } finally {
      showProgress(null); // 成功失败都要把进度条收起来
    }
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

  // 保存按钮在上传期间禁用：视频要传好一会儿，防止用户着急连点
  const button = form.querySelector('button[type="submit"]');
  button.disabled = true;
  try {
    await uploadFiles(saved.id);
  } catch (err) {
    // 文字已经存好了，只是文件没传上去：转到修改模式，重选文件再点保存即可
    await search();
    await startEdit(saved.id);
    showErrors(err.errors ?? {});
    msg.textContent = `产品已保存，但文件上传失败：${err.message}`;
    return;
  } finally {
    button.disabled = false;
  }

  resetForm();
  await search();
  msg.textContent = text;
}

// 点「播放」：取最新数据，把视频地址交给 video 元素
async function play(id) {
  try {
    const p = await api('GET', '/api/products/' + id);
    const player = document.getElementById('player');
    player.src = p.videoUrl; // 换 src 后浏览器自动开始加载，用户点播放键即可
    document.getElementById('playerTitle').textContent = `${p.code} ${p.name}`;
    document.getElementById('playerBox').hidden = false;
  } catch (err) {
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
  if (button.dataset.action === 'play') play(id);
});

form.addEventListener('submit', save);
document.getElementById('btnSearch').addEventListener('click', search);
search();
