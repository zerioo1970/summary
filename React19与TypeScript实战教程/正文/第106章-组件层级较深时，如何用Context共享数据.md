# 第 106 章：组件层级较深时，如何用 Context 共享数据？

前置：[第 105 章：如何让商品列表和购物车使用同一份数据？](第105章-如何让商品列表和购物车使用同一份数据.md)

## 一、本章要解决的问题

购物车现在有两个问题。第一，把商品加入购物车后点导航中的“新闻”，再回来，购物车又空了：state 在 `ShopPage` 中，离开页面时 `ShopPage` 被移除，state 也跟着没了。第二，第 108 章要在导航栏中显示购物车里有几件商品，可导航栏在 `AppLayout` 中，和 `ShopPage` 隔着好几层，按第 105 章的做法，要把 state 一直提到 `App`，再经过 `AppLayout`、`ShopPage`，一层一层传下去。

本章用 React 的 **Context** 解决这两个问题：把购物车放在整个应用的最外层，任何组件都可以直接取用，不需要经过中间的组件转手。完成后：加入几件商品，切换到别的页面再回来，购物车中的商品还在；`ShopPage` 不再有 state，也不再给子组件传 props。

## 二、必要原理

### 一层一层传 props 的问题

第 105 章的状态提升适合父子相邻的情况。如果使用数据的组件离得很远，中间每一层组件都要接收 props、再原样传给下一层，即使它自己根本用不到。这种情况叫 **props 逐层传递**（prop drilling）：层数一多，加一个字段要改一串文件。

### Context：在上层放好，下层直接取

Context 的做法是：在组件树的上层“放”一份数据，下面任何一层的组件都可以直接“取”，中间的组件什么也不用做。分三步：

| 步骤 | 写法 | 作用 |
| --- | --- | --- |
| 1. 创建 | `createContext(默认值)` | 创建一个 Context 对象，相当于约定好一个“取数据的名字” |
| 2. 提供 | `<CartContext value={...}>…</CartContext>` | 包住一部分组件，这些组件和它们的所有下层组件都能取到 `value` |
| 3. 读取 | `useContext(CartContext)` | 在任意下层组件中取出 `value` |

在 React 19 中，Context 对象本身就可以当作组件来提供数据，写成 `<CartContext value={...}>`。以前的资料中常见 `<CartContext.Provider value={...}>`，作用相同，新代码用前一种写法即可。

`value` 变化时，所有用 `useContext` 读取它的组件都会重新渲染，拿到新值。

### 把 state 和 Context 放进一个“提供者”组件

购物车的数据和修改它的函数，都放进一个新组件 `CartProvider` 中：

```tsx
export default function CartProvider({ children }: CartProviderProps) {
  const [cartItems, setCartItems] = useState<CartItem[]>([]);
  （addToCart、changeQuantity 两个函数）
  return <CartContext value={{ cartItems, addToCart, changeQuantity }}>{children}</CartContext>;
}
```

它接收 `children`（第 16 章），把它们包在 Context 里面。然后在 `App` 中用 `CartProvider` 包住整个 `Routes`。

`CartProvider` 在 `Routes` 外面，换页时不会被移除，所以购物车的 state 一直都在。只有刷新网页时，整个应用重新启动，state 才回到初始值。

### 默认值 null 和 useCart

`createContext` 需要一个默认值，在外面没有提供者时使用。购物车没有合理的默认值，所以写 `null`，类型写成 `CartContextValue | null`。

每次读取都判断一次 `null` 很麻烦，所以写一个自定义 Hook `useCart`（第 83 章），在里面判断：读到 `null`，说明忘了在外面包 `CartProvider`，直接抛出一个看得懂的错误；否则返回购物车。组件中只写 `const { cartItems, addToCart } = useCart();`，拿到的类型一定不是 `null`。

### 三个文件各放一样东西

| 文件 | 内容 |
| --- | --- |
| `src/lib/cartContext.ts` | `CartContext` 对象和它的类型 |
| `src/hooks/useCart.ts` | 读取购物车的 `useCart` |
| `src/components/CartProvider.tsx` | 保存 state、提供 Context 的组件 |

把 Context 对象单独放在一个文件中，`useCart` 和 `CartProvider` 都从这里导入，保证用的是同一个对象。

### 什么时候不用 Context

Context 中的数据一变，所有读取它的组件都会重新渲染，而且组件依赖 Context 后，离开提供者就不能单独使用。所以只有很多地方都要用的数据才放进 Context，例如购物车、当前登录的用户（第 110 章）。只在一两个相邻组件之间共享的数据，仍然用第 105 章的状态提升。

## 三、动手实现

### 第 1 步：创建 Context

新建 `src/lib/cartContext.ts`：

```ts
import { createContext } from 'react';
import type { CartItem } from '../types/cart';

export type CartContextValue = {
  cartItems: CartItem[];
  addToCart: (productId: number) => void;
  changeQuantity: (productId: number, change: number) => void;
};

// 默认值 null 表示“外面没有 CartProvider”，useCart 会据此给出明确的报错
export const CartContext = createContext<CartContextValue | null>(null);
```

### 第 2 步：写读取购物车的 Hook

新建 `src/hooks/useCart.ts`：

```ts
import { useContext } from 'react';
import { CartContext } from '../lib/cartContext';

export function useCart() {
  const value = useContext(CartContext);
  if (value === null) {
    throw new Error('useCart 只能在 CartProvider 里面使用');
  }
  return value;
}
```

### 第 3 步：写提供购物车的组件

新建 `src/components/CartProvider.tsx`：

```tsx
import { useState } from 'react';
import type { ReactNode } from 'react';
import { CartContext } from '../lib/cartContext';
import type { CartItem } from '../types/cart';

type CartProviderProps = {
  children: ReactNode;
};

export default function CartProvider({ children }: CartProviderProps) {
  const [cartItems, setCartItems] = useState<CartItem[]>([]);

  function addToCart(productId: number) {
    const existing = cartItems.find((item) => item.productId === productId);
    if (existing) {
      setCartItems(
        cartItems.map((item) =>
          item.productId === productId ? { ...item, quantity: item.quantity + 1 } : item,
        ),
      );
    } else {
      setCartItems([...cartItems, { productId, quantity: 1 }]);
    }
  }

  // change 是 1 或 -1：加一件或减一件
  function changeQuantity(productId: number, change: number) {
    setCartItems(
      cartItems.map((item) =>
        item.productId === productId ? { ...item, quantity: item.quantity + change } : item,
      ),
    );
  }

  return <CartContext value={{ cartItems, addToCart, changeQuantity }}>{children}</CartContext>;
}
```

两个函数从 `ShopPage` 搬过来，名字从 `handleAdd`、`handleChangeQuantity` 改成了 `addToCart`、`changeQuantity`：它们不再是某个按钮的事件处理函数，而是购物车对外提供的操作。`ReactNode` 是 `children` 的类型，第 16 章用过。

### 第 4 步：在 App 中包上 CartProvider

打开 `src/App.tsx`，在导入 `AppLayout` 的一行下面加入：

```tsx
import CartProvider from './components/CartProvider';
```

用 `<CartProvider>` 包住整个 `<Routes>`，里面的路由各多缩进一级：

```tsx
    <CartProvider>
      <Routes>
        <Route element={<AppLayout />}>
          （各条路由不变）
        </Route>
      </Routes>
    </CartProvider>
```

### 第 5 步：子组件改用 useCart

打开 `src/components/ProductList.tsx`。把导入中 `CartItem` 类型的一行删掉，加入：

```tsx
import { useCart } from '../hooks/useCart';
```

删掉 `ProductListProps` 类型，把组件的开头改为：

```tsx
export default function ProductList() {
  const { cartItems, addToCart } = useCart();
```

把按钮中的 `onAdd(product.id)` 改为 `addToCart(product.id)`。

`src/components/CartPanel.tsx` 做同样的修改：删掉 `CartItem` 的导入和 `CartPanelProps` 类型，导入 `useCart`，组件开头改为：

```tsx
export default function CartPanel() {
  const { cartItems, changeQuantity } = useCart();
```

两个按钮中的 `onChangeQuantity` 改为 `changeQuantity`。

### 第 6 步：ShopPage 只负责摆放

把 `src/pages/ShopPage.tsx` 改为：

```tsx
import CartPanel from '../components/CartPanel';
import ProductList from '../components/ProductList';
import styles from './ShopPage.module.css';

export default function ShopPage() {
  return (
    <main className="page">
      <h1>购物车</h1>
      <div className={styles.layout}>
        <ProductList />
        <CartPanel />
      </div>
    </main>
  );
}
```

## 四、运行与检查

保持开发服务器运行（`npm.cmd run dev`），保存文件后在浏览器中查看。

1. 打开购物车页，加入机械键盘和保温杯，合计“共 2 件，合计 ¥358.00”。
2. 点导航中的“新闻”，再点“购物车”。两件商品还在，数量和合计都没变。
3. 按 F5 刷新，购物车变空。刷新时整个应用重新启动，state 回到初始值；第 15 部分会学习怎样把数据保存在浏览器中。
4. 在 React 开发者工具的 Components 面板中，最上层是 `App`，下面是 `CartProvider`，再往下才是路由和各个页面。选中 `CartProvider`，State 中是 `cartItems`；选中 `ProductList`，它没有 props。
5. 做一个小实验：在 `App.tsx` 中临时去掉 `<CartProvider>` 和 `</CartProvider>` 两行，打开购物车页。页面一片空白，控制台显示“useCart 只能在 CartProvider 里面使用”。实验后恢复。

最后运行检查，应没有错误：

```powershell
npm.cmd run check
```

**小练习**：在商品目录页（`ProductPage`）每件商品的卡片中，显示“已在购物车中”这几个字，只在这件商品已加入购物车时显示。提示：在 `ProductPage` 中调用 `useCart()` 取出 `cartItems`。练习后恢复本章代码。

## 五、常见错误

| 现象 | 原因 | 怎么改 |
| --- | --- | --- |
| 页面空白，控制台显示“useCart 只能在 CartProvider 里面使用” | 使用 `useCart` 的组件不在 `CartProvider` 里面，例如 `App` 中忘了包上它 | 在 `App` 中用 `<CartProvider>` 包住 `<Routes>` |
| 换页后购物车仍然变空 | `CartProvider` 包在了 `ShopPage` 里面，换页时和页面一起被移除 | 放在 `App` 中、`Routes` 的外面 |
| 检查命令报 `Property 'addToCart' does not exist on type 'CartContextValue \| null'` | 在组件中直接用 `useContext(CartContext)`，没有排除 `null` | 改用 `useCart()`，它已经处理了 `null` |
| 两个组件中的购物车互不相干 | 在 `useCart.ts` 和 `CartProvider.tsx` 中各写了一次 `createContext`，得到的是两个不同的 Context | `createContext` 只写在 `cartContext.ts` 中，其他文件都从这里导入 |

## 六、本章完整代码

### `src/lib/cartContext.ts`

同第三节第 1 步。

### `src/hooks/useCart.ts`

同第三节第 2 步。

### `src/components/CartProvider.tsx`

同第三节第 3 步。

### `src/App.tsx`

```tsx
import { Route, Routes } from 'react-router';
import './App.css';
import AppLayout from './components/AppLayout';
import CartProvider from './components/CartProvider';
import ArticlePage from './pages/ArticlePage';
import ContactsPage from './pages/ContactsPage';
import FaqPage from './pages/FaqPage';
import GroupPage from './pages/GroupPage';
import NewsPage from './pages/NewsPage';
import NotFoundPage from './pages/NotFoundPage';
import ProductDetailPage from './pages/ProductDetailPage';
import ProductPage from './pages/ProductPage';
import RegisterPage from './pages/RegisterPage';
import SearchPage from './pages/SearchPage';
import ShopPage from './pages/ShopPage';
import TaskPage from './pages/TaskPage';
import TaskServerPage from './pages/TaskServerPage';

export default function App() {
  return (
    <CartProvider>
      <Routes>
        <Route element={<AppLayout />}>
          <Route path="/" element={<GroupPage />} />
          <Route path="/news" element={<NewsPage />} />
          <Route path="/faq" element={<FaqPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/todos" element={<TaskPage />} />
          <Route path="/contacts" element={<ContactsPage />} />
          <Route path="/products" element={<ProductPage />} />
          <Route path="/products/:id" element={<ProductDetailPage />} />
          <Route path="/articles" element={<ArticlePage />} />
          <Route path="/search" element={<SearchPage />} />
          <Route path="/tasks" element={<TaskServerPage />} />
          <Route path="/shop" element={<ShopPage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Routes>
    </CartProvider>
  );
}
```

### `src/components/ProductList.tsx`

```tsx
import { products } from '../data/products';
import { useCart } from '../hooks/useCart';
import { formatPrice } from '../lib/format';
import styles from './ShopList.module.css';

export default function ProductList() {
  const { cartItems, addToCart } = useCart();

  return (
    <section>
      <h2>商品</h2>
      <ul className={styles.list}>
        {products.map((product) => {
          const cartItem = cartItems.find((item) => item.productId === product.id);
          const quantity = cartItem ? cartItem.quantity : 0;
          let buttonText = '加入购物车';
          if (product.stock === 0) {
            buttonText = '缺货';
          } else if (quantity >= product.stock) {
            buttonText = '已达库存';
          }
          return (
            <li key={product.id} className={styles.row}>
              <span className={styles.name}>{product.name}</span>
              <span className={styles.price}>{formatPrice(product.price)}</span>
              <button
                type="button"
                disabled={quantity >= product.stock}
                onClick={() => addToCart(product.id)}
              >
                {buttonText}
              </button>
            </li>
          );
        })}
      </ul>
    </section>
  );
}
```

### `src/components/CartPanel.tsx`

```tsx
import { products } from '../data/products';
import { useCart } from '../hooks/useCart';
import { formatPrice } from '../lib/format';
import styles from './ShopList.module.css';

export default function CartPanel() {
  const { cartItems, changeQuantity } = useCart();
  let totalQuantity = 0;
  let totalPrice = 0;
  for (const item of cartItems) {
    const product = products.find((candidate) => candidate.id === item.productId);
    if (product) {
      totalQuantity += item.quantity;
      totalPrice += product.price * item.quantity;
    }
  }

  return (
    <section>
      <h2>已选商品</h2>
      {cartItems.length === 0 ? (
        <p className={styles.empty}>购物车是空的。</p>
      ) : (
        <ul className={styles.list}>
          {cartItems.map((item) => {
            const product = products.find((candidate) => candidate.id === item.productId);
            if (!product) {
              return null;
            }
            return (
              <li key={item.productId} className={styles.row}>
                <span className={styles.name}>{product.name}</span>
                <button
                  type="button"
                  aria-label={'减少一件：' + product.name}
                  disabled={item.quantity <= 1}
                  onClick={() => changeQuantity(item.productId, -1)}
                >
                  −
                </button>
                <span className={styles.quantity}>{item.quantity}</span>
                <button
                  type="button"
                  aria-label={'增加一件：' + product.name}
                  disabled={item.quantity >= product.stock}
                  onClick={() => changeQuantity(item.productId, 1)}
                >
                  +
                </button>
                <span className={styles.subtotal}>
                  {formatPrice(product.price * item.quantity)}
                </span>
              </li>
            );
          })}
        </ul>
      )}
      {cartItems.length > 0 && (
        <p className={styles.total}>
          共 {totalQuantity} 件，合计 {formatPrice(totalPrice)}
        </p>
      )}
    </section>
  );
}
```

### `src/pages/ShopPage.tsx`

同第三节第 6 步。

## 七、本章改动小结

新增 `src/lib/cartContext.ts`、`src/hooks/useCart.ts` 和 `src/components/CartProvider.tsx`：购物车的 Context、读取它的 Hook 和提供它的组件；修改 `src/App.tsx`：用 `CartProvider` 包住路由；修改 `src/components/ProductList.tsx` 和 `src/components/CartPanel.tsx`：改用 `useCart` 读取购物车；修改 `src/pages/ShopPage.tsx`：去掉 state 和 props。

---

下一章：[第 107 章：更新操作越来越多时，如何用 reducer 集中管理？](第107章-更新操作越来越多时，如何用reducer集中管理.md)
