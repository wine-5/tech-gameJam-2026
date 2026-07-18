# C# 命名規則

## 1. 大文字・小文字スタイルの種類

| スタイル | 例 |
|---|---|
| PascalCase | `MyClassName` |
| camelCase | `myVariableName` |
| SCREAMING_SNAKE_CASE | `MAX_VALUE` |
| _camelCase（アンダースコアプレフィックス） | `_myField` |

---

## 2. 各要素の命名規則

### クラス・構造体・インターフェース・列挙型

```csharp
// クラス: PascalCase
public class CustomerOrder { }

// 構造体: PascalCase
public struct Point { }

// インターフェース: PascalCase + 先頭に "I"
public interface IRepository { }

// 列挙型: PascalCase
public enum OrderStatus
{
    Pending,
    Completed,
    Cancelled
}
```

### メソッド・プロパティ・イベント

```csharp
// メソッド: PascalCase（動詞または動詞句）
public void SaveOrder() { }
public string GetCustomerName() { }

// プロパティ: PascalCase（名詞または名詞句）
public string FirstName { get; set; }
public int TotalCount { get; }

// イベント: PascalCase（動詞または動詞句、過去形も可）
public event EventHandler OrderCompleted;
public event EventHandler<DataEventArgs> DataReceived;
```

### フィールド・変数・パラメーター

```csharp
// privateフィールド: _camelCase（アンダースコアプレフィックス）
private string _customerName;
private readonly IRepository _repository;

// publicフィールド: PascalCase（ただし通常はプロパティを使う）
public string Name;

// ローカル変数: camelCase
int itemCount = 0;
string customerName = "John";

// メソッドパラメーター: camelCase
public void ProcessOrder(int orderId, string customerName) { }
```

### 定数・静的読み取り専用フィールド

```csharp
// 定数: PascalCase（または SCREAMING_SNAKE_CASE は非推奨）
public const int MaxRetryCount = 3;
private const string DefaultPrefix = "USR_";

// static readonly: PascalCase
public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
```

### 型パラメーター（ジェネリクス）

```csharp
// 単一の場合は "T"、複数の場合は意味のある名前 + "T" プレフィックス
public class Repository<T> { }
public class KeyValuePair<TKey, TValue> { }
```

### 名前空間

```csharp
// PascalCase、ドット区切り（会社名.プロジェクト名.機能名）
namespace TechC.Services { }
namespace TechC.Data.Repositories { }
```

---

## 3. 命名規則まとめ表

| 種類 | スタイル | プレフィックス | 例 |
|---|---|---|---|
| クラス | PascalCase | なし | `OrderService` |
| インターフェース | PascalCase | `I` | `IOrderService` |
| 構造体 | PascalCase | なし | `ColorRgb` |
| 列挙型 | PascalCase | なし | `HttpMethod` |
| 列挙値 | PascalCase | なし | `HttpMethod.Get` |
| メソッド | PascalCase | なし | `GetById` |
| プロパティ | PascalCase | なし | `FullName` |
| イベント | PascalCase | なし | `DataLoaded` |
| privateフィールド | camelCase | `_` | `_userCount` |
| publicフィールド | PascalCase | なし | `MaxSize` |
| ローカル変数 | camelCase | なし | `itemCount` |
| パラメーター | camelCase | なし | `orderId` |
| 定数 | PascalCase | なし | `MaxRetryCount` |
| 型パラメーター | PascalCase | `T` | `TEntity` |
| 名前空間 | PascalCase | なし | `MyApp.Services` |

---

## 4. 推奨事項・注意点

- **意味のある名前をつける**: `x`, `temp`, `data` などの曖昧な名前は避ける
- **略語を避ける**: `GetCustOrdr()` より `GetCustomerOrder()`
- **否定形のbool変数は避ける**: `isNotValid` より `isInvalid` や `isValid`
- **型名をそのまま使わない**: `OrderList` より `Orders`（型情報は型で表現する）
- **インターフェースの "I" プレフィックスは必須**: `IDisposable`, `IEnumerable` など
- **非同期メソッドは "Async" サフィックスをつける**:

```csharp
public async Task<Order> GetOrderAsync(int id) { }
public async Task SaveAsync() { }
```

- **Boolean プロパティ・変数は "is", "has", "can" などで始める**:

```csharp
bool isActive;
bool hasPermission;
bool canExecute;
```

---

## 5. 参考

- [Microsoft 公式命名ガイドライン](https://learn.microsoft.com/ja-jp/dotnet/standard/design-guidelines/naming-guidelines)
- [C# コーディング規則 (Microsoft Docs)](https://learn.microsoft.com/ja-jp/dotnet/csharp/fundamentals/coding-style/coding-conventions)