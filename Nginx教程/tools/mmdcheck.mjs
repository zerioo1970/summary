// 检查 docs/*.md 里每个 mermaid 代码块能否被 Mermaid 解析：node tools/mmdcheck.mjs docs/*.md
import { chromium } from '/projects/sandbox/.tools/pw/node_modules/playwright-core/index.mjs';
import { readFileSync } from 'node:fs';
const browser = await chromium.launch({ executablePath: '/inner_container/opt/playwright/chromium_headless_shell-1232/chrome-headless-shell-linux64/chrome-headless-shell' });
const page = await browser.newPage();
await page.setContent('<html><body></body></html>');
await page.addScriptTag({ path: '/projects/sandbox/.tools/pw/node_modules/mermaid/dist/mermaid.min.js' });
let bad = 0;
for (const f of process.argv.slice(2)) {
  const blocks = [...readFileSync(f, 'utf8').matchAll(/```mermaid\n([\s\S]*?)```/g)].map(m => m[1]);
  for (const [i, code] of blocks.entries()) {
    const err = await page.evaluate(async c => { try { await mermaid.parse(c); return null; } catch (e) { return String(e.message || e); } }, code);
    console.log(`${f} 第 ${i + 1} 个图：${err ? '错误 ' + err : '通过'}`);
    if (err) bad++;
  }
}
await browser.close();
process.exit(bad ? 1 : 0);
