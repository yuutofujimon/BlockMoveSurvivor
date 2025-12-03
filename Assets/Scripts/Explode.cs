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

        // 2秒後に爆発
        Invoke(nameof(Explotion), 2f);
    }

    /// <summary>
    /// 爆発
    /// </summary>
    void Explotion()
    {
        // 中心の爆発
        CreateExplosion(transform.position);

        // 四方向へ爆発を伸ばす
        SpreadExplosion(Vector3.forward); // ↑
        SpreadExplosion(Vector3.back);    // ↓
        SpreadExplosion(Vector3.left);    // ←
        SpreadExplosion(Vector3.right);   // →
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
            var targetPos = centerPos + direction * (TileSize * i);

            // そのマスに Ray を撃つ
            if (Physics.Raycast(centerPos + direction * (TileSize * (i - 1) + 0.1f),
                                direction,
                                out RaycastHit hit,
                                TileSize
                                )
                )
            {
                // 壊せない壁ならここで止める（手前まで爆発）
                if (hit.collider.CompareTag("StageWall"))
                {
                    CreateExplosion(centerPos + direction * (TileSize * (i - 1)));
                    break;
                }

                // 壊れるブロック
                if (hit.collider != null && hit.collider.CompareTag("Cube"))
                {
                    CreateExplosion(hit.collider.transform.position);

                    Destroy(hit.collider.gameObject);

                    break;
                }
            }

            // 何もなければ通常爆発
            CreateExplosion(targetPos);
        }
    }

    /// <summary>
    /// 爆発エフェクト生成
    /// </summary>
    /// <param name="position"> 位置 </param>
    void CreateExplosion(Vector3 position)
    {
        Instantiate(_explotionEffectPrefab, position, Quaternion.identity);

        // ボム破壊
        Destroy(gameObject);

        // Vector3をVector2Intに変換して爆弾情報を削除
        var pos2D = new Vector2Int(
            Mathf.RoundToInt(position.x / TileSize),
            Mathf.RoundToInt(position.z / TileSize)
        );
        _gameManager.RemoveBombAtPosition(pos2D);
    }
}
