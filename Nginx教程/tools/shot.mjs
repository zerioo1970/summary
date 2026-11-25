// 用无头 Chromium 截取页面：node shot.mjs 网址 输出文件 [宽] [高]
import { chromium } from '/projects/sandbox/.tools/pw/node_modules/playwright-core/index.mjs';
const [url, out, w = '900', h = '420'] = process.argv.slice(2);
const browser = await chromium.launch({ executablePath: '/inner_container/opt/playwright/chromium_headless_shell-1232/chrome-headless-shell-linux64/chrome-headless-shell' });
const page = await browser.newPage({ viewport: { width: +w, height: +h } });
await page.goto(url);
await page.screenshot({ path: out });
await browser.close();
