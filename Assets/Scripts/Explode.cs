using UnityEngine;

/// <summary>
/// 爆発処理
/// </summary>
public class Explode : MonoBehaviour
{
    /// <summary>
    /// タイルサイズ
    /// </summary>
    private static readonly float TileSize = 2.4f;

    [Header("爆発エフェクトプレハブ"),SerializeField]
    private GameObject _explotionEffectPrefab;

    /// <summary>
    /// 爆発制御フラグ
    /// </summary>
    private bool _isExploded = false;

    /// <summary>
    /// ゲームマネージャー
    /// </summary>
    private GameManager _gameManager;

    /// <summary>
    /// Awake
    /// </summary>
    void Awake()
    {
        // ゲームオブジェクト名で取得
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // 3秒後に爆発
        Invoke(nameof(Explotion), 3f);
    }

    /// <summary>
    /// 爆発
    /// </summary>
    void Explotion()
    {
        if (_isExploded) return; // ★二重呼び出し防止
        _isExploded = true;

        // 中心の爆発
        CreateExplosion(transform.position);

        // 四方向へ爆発を伸ばす
        SpreadExplosion(Vector3.forward); // ↑
        SpreadExplosion(Vector3.back);    // ↓
        SpreadExplosion(Vector3.left);    // ←
        SpreadExplosion(Vector3.right);   // →

        // ❗ 爆弾削除はここで1回だけ実行する
        Destroy(gameObject);

        // 爆弾位置を削除
        var pos2D = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / TileSize),
            Mathf.RoundToInt(transform.position.z / TileSize)
        );
        _gameManager.RemoveBombAtPosition(pos2D);
    }

    /// <summary>
    /// 四方向へ爆発を伸ばす
    /// </summary>
    /// <param name="direction"> 方向 </param>
    void SpreadExplosion(Vector3 direction)
    {
        var centerPos = transform.position;

        for (var i = 1; i <= 3; i++)
        {
            // 爆発が届くタイル座標
            var targetPos = centerPos + direction * (TileSize * i);

            // ================================
            // このタイル位置にあるオブジェクトを取得
            // ================================
            Collider[] hits = Physics.OverlapSphere(targetPos, 0.5f);

            bool blocked = false;

            foreach (var hit in hits)
            {
                if (hit == null) continue;

                // 壊せない壁
                if (hit.CompareTag("DontBreakWall"))
                {
                    // 手前の位置まで爆発
                    CreateExplosion(centerPos + direction * (TileSize * (i - 1)));
                    blocked = true;
                    break;
                }

                // 壊れるブロック
                if (hit.CompareTag("AbleBreakWall"))
                {
                    CreateExplosion(hit.transform.position);
                    Destroy(hit.gameObject);
                    blocked = true;
                    break;
                }

                // 💣 別の爆弾 → 連鎖爆発
                if (hit.CompareTag("Bomb"))
                {
                    var bomb = hit.GetComponent<Explode>();

                    if (bomb != null)
                    {
                        bomb.CancelInvoke();   // タイマー停止
                        bomb.Explotion();      // 即時爆発！
                    }

                    blocked = true;
                    break;
                }

                if (hit.CompareTag("Player"))
                {

                }
            }

            // 壁やブロックで止まった
            if (blocked)
                break;

            // 何にも当たらなければ通常爆発
            CreateExplosion(targetPos);
        }
    }

    /// <summary>
    /// 爆発エフェクト生成
    /// </summary>
    /// <param name="position"> 位置 </param>
    void CreateExplosion(Vector3 position)
    {
       var effect =  Instantiate(_explotionEffectPrefab, position, Quaternion.identity);
        Destroy(effect, 1f);  // 1秒後に消える
    }
}
