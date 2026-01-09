using UnityEngine;

/// <summary>
/// プレイヤー
/// </summary>
public class Player : MonoBehaviour
{
    [Header("壁掴む判定"),SerializeField]
    private CatchWall _catchWall;

    /// <summary>
    /// タイルサイズ
    /// </summary>
    private static readonly float TileSize = 2.56f;

    /// <summary>
    /// 方向情報構造体
    /// </summary>
    private struct DirectionInfo
    {
        public Vector3 direction;   // 方向ベクトル
        public KeyCode pushKey;     // 押すキー
        public KeyCode pullKey;     // 引くキー
        public string name;         // 方向名（デバッグ用）

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="direction"> 方向ベクトル </param>
        /// <param name="pushKey"> 押すキー </param>
        /// <param name="pullKey"> 引くキー </param>
        /// <param name="name"> 方向名（デバッグ用） </param>
        public DirectionInfo(Vector3 direction, KeyCode pushKey, KeyCode pullKey, string name)
        {
            this.direction = direction;
            this.pushKey = pushKey;
            this.pullKey = pullKey;
            this.name = name;
        }
    }

    /// <summary>
    /// 状態
    /// </summary>
    private enum State
    {
        Stanby,
        Move,
        CatchWall,
        Push,
        Pull
    }
    private State _state = State.Stanby;

    [Header("リジッドボディ"), SerializeField]
    private Rigidbody _rigidbody;

    [Header("アニメーター"), SerializeField]
    private Animator _animator;

    [Header("ゲームマネージャー"), SerializeField]
    private GameManager _gameManager;

    [Header("爆弾プレハブ"), SerializeField]
    private GameObject _bombPrefab;

    /// <summary>
    /// 方向情報配列
    /// </summary>
    private DirectionInfo[] _directionInfos;

    /// <summary>
    /// 現在操作中の方向情報
    /// </summary>
    private DirectionInfo _currentPlayerToAbleBreakObjDirectionInfo;

    /// <summary>
    /// 横方向の入力
    /// </summary>
    private float _inputHorizontal;

    /// <summary>
    /// 縦方向の入力
    /// </summary>
    private float _inputVertical;

    /// <summary>
    /// 死亡フラグ
    /// </summary>
    private bool isDead = false;

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        // 方向とキーの対応を初期化
        _directionInfos = new DirectionInfo[]
        {
            new DirectionInfo(Vector3.forward, KeyCode.W, KeyCode.S, "Forward"),
            new DirectionInfo(Vector3.back,    KeyCode.S, KeyCode.W, "Back"),
            new DirectionInfo(Vector3.right,   KeyCode.D, KeyCode.A, "Right"),
            new DirectionInfo(Vector3.left,    KeyCode.A, KeyCode.D, "Left")
        };
    }

    /// <summary>
    /// Update
    /// </summary>
    public void Update()
    {
        if (isDead)
        {
            return;   // ← 死んでたら操作無効
        }

        switch (_state)
        {
            // 待機
            case State.Stanby:
                // 操作不可
                // 地面に接触したら移動可能に。Groundタグとの接触判定
                if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 0.2f)
                    && hit.collider.CompareTag("Ground"))
                {
                    ChangeState(State.Move);
                }
                break;

            // 移動
            case State.Move:
                // 入力取得
                _inputHorizontal = Input.GetAxisRaw("Horizontal");
                _inputVertical = Input.GetAxisRaw("Vertical");

                // 移動
                Move();

                // 壁をつかむかどうか
                if (CatchWall())
                {
                    _animator.SetBool("StanbyMode", true);
                    ControlRunAnim(false);
                    ChangeState(State.CatchWall);
                }

                // 爆弾設置
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    PlaceBomb();
                }
                break;

            // 壁キャッチ
            case State.CatchWall:
                if (Input.GetKeyDown(_currentPlayerToAbleBreakObjDirectionInfo.pushKey))
                {
                    ChangeState(State.Push);
                }
                else if (Input.GetKeyDown(_currentPlayerToAbleBreakObjDirectionInfo.pullKey))
                {
                    ChangeState(State.Pull);
                }

                // Jキーが離されたら移動状態に戻る
                if (Input.GetKeyUp(KeyCode.J))
                {
                    _animator.SetBool("StanbyMode", false);
                    ChangeState(State.Move);
                }
                break;

            // 押す
            case State.Push:
                // 壁を移動させる
                _catchWall.OnPushPullAction(_currentPlayerToAbleBreakObjDirectionInfo.direction);
                // TODO: 移動させる壁にプレイヤーを追従させる
                _animator.SetBool("Push", true);
                break;

            // 引く
            case State.Pull:
                // 壁を移動させる
                _catchWall.OnPushPullAction(_currentPlayerToAbleBreakObjDirectionInfo.direction*-1);
                // TODO: 移動させる壁にプレイヤーを追従させる
                _animator.SetBool("Pull", true);
                break;
        }

        // 4方向敵チェック
        CheckEnemyHit();
    }

    // --- 4方向に Raycast を飛ばして敵ヒットを判定 ---
    private void CheckEnemyHit()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        float checkDistance = 0.5f;

        // チェックする4方向
        Vector3[] directions =
        {
        transform.forward,
        -transform.forward,
        transform.right,
        -transform.right
    };

        foreach (var dir in directions)
        {
            if (Physics.Raycast(origin, dir, out RaycastHit hit, checkDistance)
                && hit.collider.CompareTag("Enemy"))
            {
                // --- 敵に当たった！死亡処理 ---
                isDead = true;
                // 物理停止
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
                _rigidbody.isKinematic = true;  // ← 完全停止させる
                _animator.SetBool("Death", true);
                Invoke(nameof(Death), 1.8f);

                return; // 1つ当たれば終了
            }
        }
    }

    void Move()
    {
        // 移動方向ベクトル取得
        var moveDirection = new Vector3(_inputHorizontal, 0, _inputVertical).normalized;

        if (moveDirection.magnitude > 0)
        {
            // プレイヤー移動
            transform.Translate(moveDirection * 6 * Time.deltaTime, Space.World);

            // 向きを移動方向に合わせる
            transform.rotation = Quaternion.LookRotation(moveDirection);

            ControlRunAnim(true);
        }
        else
        {
            ControlRunAnim(false);
        }
    }

    /// <summary>
    /// 壁をつかむか
    /// </summary>
    private bool CatchWall()
    {
        if (_catchWall.AbleCatchWall && Input.GetKeyDown(KeyCode.J))
        {
            // つかむ方向を取得し、どの方向か判定
            var info = new DirectionInfo();
            foreach (var dirInfo in _directionInfos)
            {
                // 内積で方向を比較（ほぼ同じ方向なら1に近い値になる）
                float dot = Vector3.Dot(_catchWall.WallDirection, dirInfo.direction);
                if (dot > 0.9f)
                {
                    info = dirInfo;
                    break;
                }
            }

            // 動かせる壁をつかむ
            _currentPlayerToAbleBreakObjDirectionInfo = info;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 爆弾を置く
    /// </summary>
    private void PlaceBomb()
    {
        // プレイヤーの座標をタイル番号（整数）に変換
        int tileX = Mathf.RoundToInt(transform.position.x / TileSize);
        int tileZ = Mathf.RoundToInt(transform.position.z / TileSize);
        // タイル中心のワールド座標を計算
        float centerX = tileX * TileSize;
        float centerZ = tileZ * TileSize;

        var tilePos = new Vector2Int(tileX, tileZ);


        // すでに同じタイルに爆弾があるなら置かない
        if (_gameManager.IsBombAtPosition(tilePos))
        {
            return;
        }

        Vector3 placePos = new Vector3(centerX, 1f, centerZ);

        // 爆弾生成
        Instantiate(_bombPrefab, placePos, Quaternion.identity);

        _gameManager.AddBombAtPosition(tilePos);
    }

    /// <summary>
    /// 走るアニメーションの制御
    /// </summary>
    private void ControlRunAnim(bool isRun)
    {
        _animator.SetBool("Run", isRun);
    }

    /// <summary>
    /// 死亡処理
    /// </summary>
    void Death()
    {
        Destroy(gameObject);
    }

    /// <summary>
    ///  ステートチェンジ
    /// </summary>
    private void ChangeState(State nextState)
    {
        Debug.Log($"ステート変更：{nextState}");
        _state = nextState;
    }
}
