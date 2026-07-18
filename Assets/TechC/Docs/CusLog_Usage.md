# CusLog 使用例

## 名前空間のインポート
（名前空間によって変わるため注意）
```csharp
using ProjectName.Log;
```

## 基本ログ

```csharp
// 通常ログ（白色）
CusLog.Log("これは通常のログです");

// 警告ログ（黄色）
CusLog.Warning("これは警告ログです");

// エラーログ（赤色）
CusLog.Error("これはエラーログです");
```

## カテゴリ付きログ

```csharp
// Playerカテゴリのログ
CusLog.Log("Player", "プレイヤーが初期化されました");

// Enemyカテゴリの警告
CusLog.Warning("Enemy", "敵のAIが応答していません");

// UIカテゴリのエラー
CusLog.Error("UI", "UIパネルの読み込みに失敗しました");

// その他のカテゴリ
CusLog.Log("Audio", "BGMの再生を開始しました");
CusLog.Log("Network", "サーバーに接続しました");
CusLog.Log("System", "ゲームデータをセーブしました");
```

## 文字列補間

```csharp
int playerHealth = 100;
float playerSpeed = 5.5f;
string playerName = "Hero";

// $"{}" による文字列補間
CusLog.Log($"プレイヤー名: {playerName}, HP: {playerHealth}, 速度: {playerSpeed}");

// カテゴリ付きで文字列補間
CusLog.Log("Player", $"{playerName} がダメージを受けました。残りHP: {playerHealth - 20}");

// フォーマット指定
Vector3 position = transform.position;
CusLog.Log("Player", $"プレイヤー位置: ({position.x:F2}, {position.y:F2}, {position.z:F2})");
```

## 実際の使用例

```csharp
using UnityEngine;
using ProjectName.Log;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        CusLog.Log("System", "ゲームマネージャーを初期化しました");
    }
    
    private void Update()
    {
        // nullチェック
        if (targetTransform == null)
        {
            CusLog.Warning("Player", "PlayerInputManager がnullです");
        }
        
        // 条件付きログ
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CusLog.Log("Player", $"スペースキーが押されました (フレーム: {Time.frameCount})");
        }
    }
}
```

## デバッグフラグの制御

```csharp
// ログを有効化
CusLog.isDebug = true;

// ログを無効化（ビルド時に出力されない）
CusLog.isDebug = false;
```