// screenshot #fig of each html file: node shot.mjs out_dir a.html b.html ...
import { chromium } from 'playwright-core';
const [out, ...files] = process.argv.slice(2);
const b = await chromium.launch({ executablePath: '/inner_container/opt/playwright/chromium_headless_shell-1232/chrome-headless-shell-linux64/chrome-headless-shell' });
const p = await b.newPage({ deviceScaleFactor: 1 });
for (const f of files) {
  await p.goto('file://' + f); await p.waitForTimeout(300);
  const name = f.split('/').pop().replace('.html', '.png');
  await (await p.$('#fig')).screenshot({ path: out + '/' + name });
  console.log(name);
}
await b.close();
