using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤー
/// </summary>
public class Player : MonoBehaviour
{
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
    /// 破壊可能な壁を押す/引く際の接触判定Rayの距離
    /// </summary>
    private static readonly float RayDistance = 2.0f;

    [Header("リジッドボディ"),SerializeField]
    private Rigidbody _rigidbody;

    [Header("アニメーター"),SerializeField]
    private Animator _animator;

    [Header("敵たち"), SerializeField]
    private GameObject[] _enemies;

    [Header("ゲームマネージャー"), SerializeField]
    private GameManager _gameManager;

    [Header("破壊可能な壁オブジェクトたち"),SerializeField]
    private GameObject[] _ableBreakWallObjs;

    [Header("破壊可能な壁のリジッドボディたち"),SerializeField]
    private Rigidbody[] _ableBreakWallRigidbodys;

    [Header("破壊不可な壁オブジェクトたち"),SerializeField]
    private GameObject[] _dontBreakWallObjs;

    [Header("爆弾プレハブ"),SerializeField]
    private GameObject _bombPrefab;

    /// <summary>
    /// 方向情報配列
    /// </summary>
    private DirectionInfo[] _directionInfos;

    /// <summary>
    /// 横方向の入力
    /// </summary>
    private float _inputHorizontal;

    /// <summary>
    /// 縦方向の入力
    /// </summary>
    private float _inputVertical;

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
        // 入力取得
        _inputHorizontal = Input.GetAxisRaw("Horizontal");
        _inputVertical = Input.GetAxisRaw("Vertical");

        // 移動
        Move();

        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        int count = Mathf.Min(_ableBreakWallObjs.Length, _dontBreakWallObjs.Length);

        //横方向と縦方向の入力を取得
        
        bool holdingJ = Input.GetKey(KeyCode.J);

        int layerMask = 1 << 8;

        layerMask = ~layerMask;

        Vector3 rayPosition = transform.position + new Vector3(0, 1f, 0);
        Debug.DrawRay(rayPosition, transform.TransformDirection(Vector3.forward) * 1.0f, Color.yellow);
        Ray rayforward = new Ray(rayPosition, Vector3.forward);
        Ray rayright = new Ray(rayPosition, Vector3.right);
        Ray rayleft = new Ray(rayPosition, Vector3.left);
        Ray rayback = new Ray(rayPosition, Vector3.back);
        RaycastHit hit;

        //前後左右方向
        if (Physics.Raycast(rayforward, out hit, 1.5f) ||
            Physics.Raycast(rayright, out hit, 1.5f) ||
            Physics.Raycast(rayleft, out hit, 1.5f) ||
            Physics.Raycast(rayback, out hit, 1.5f)
            )
        {
            var hitColliderObj = hit.collider.gameObject;
            ChangeAnim(hitColliderObj, Vector3.Distance(hitColliderObj.transform.position, transform.position));
        }

        // --- 爆弾を置く ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlaceBomb();
        }

        // Cube全体に対して同じ処理を回す
        for (int i = 0; i < _ableBreakWallObjs.Length; i++)
        {
            var distance = Vector3.Distance(_ableBreakWallObjs[i].transform.position, _dontBreakWallObjs[i].transform.position);

            if (distance < TileSize &&
                Physics.Raycast(rayforward, out hit, RayDistance) &&
                hit.collider.gameObject == _ableBreakWallObjs[i])
            {
                _animator.SetBool("Push", false);
                _animator.SetBool("Pull", false);

                _ableBreakWallRigidbodys[i].constraints = RigidbodyConstraints.FreezeRotation;

                // 該当のCubeが見つかったら、残りのCubeはチェックしなくてよい
                break;
            }

            // --- 前方向へのRay ---
            if (Input.GetKey(KeyCode.J) &&
                Input.GetKeyDown(KeyCode.W) &&
                Physics.Raycast(rayforward, out hit, 1.5f) &&
                hit.collider.CompareTag("Cube"))
            {
                Rigidbody cubeRb = hit.collider.GetComponent<Rigidbody>();

                cubeRb.constraints = RigidbodyConstraints.FreezeRotation;
                cubeRb.isKinematic = false;
            }
            else if (Input.GetKey(KeyCode.J) &&
                Input.GetKey(KeyCode.S) &&
                Physics.Raycast(rayforward, out hit, 1.5f) &&
                hit.collider.CompareTag("Cube"))
            {
                Rigidbody cubeRb = hit.collider.GetComponent<Rigidbody>();

                // --- プレイヤーを Cube の方向に回転させる ---
                Vector3 dir = hit.collider.transform.position - transform.position;
                dir.y = 0;                     // 上下方向は無視
                transform.rotation = Quaternion.LookRotation(dir);

                cubeRb.constraints = RigidbodyConstraints.FreezeRotation;
                cubeRb.isKinematic = false;
            }


            foreach (var info in _directionInfos)
            {
                Ray ray = new Ray(transform.position, info.direction);

                // 押す（Push）
                if (Input.GetKey(KeyCode.J) &&
                    Input.GetKeyDown(info.pushKey) &&
                    Physics.Raycast(ray, out hit, RayDistance) &&
                    hit.collider.CompareTag("Cube") &&
                    hit.collider.gameObject == _ableBreakWallObjs[i])
                {
                    _animator.SetBool("Push", true);
                    _ableBreakWallRigidbodys[i].constraints = RigidbodyConstraints.FreezeRotation;
                    _ableBreakWallRigidbodys[i].transform.position += info.direction * 3 * Time.deltaTime;
                }
                // 引く（Pull）
                else if (Input.GetKey(KeyCode.J) &&
                    Input.GetKey(info.pullKey) &&
                    Physics.Raycast(ray, out hit, RayDistance) &&
                    hit.collider.CompareTag("Cube") &&
                    hit.collider.gameObject == _ableBreakWallObjs[i])
                {
                    _animator.SetBool("Pull", true);
                    _ableBreakWallRigidbodys[i].constraints = RigidbodyConstraints.FreezeRotation;
                    _ableBreakWallRigidbodys[i].transform.position += -info.direction * 5 * Time.deltaTime;
                    // --- プレイヤーを Cube の方向に回転させる ---
                    Vector3 dir = hit.collider.transform.position - transform.position;
                    dir.y = 0;                     // 上下方向は無視
                    transform.rotation = Quaternion.LookRotation(dir);
                }
                else if (_inputVertical != 0 || _inputHorizontal != 0)
                {
                    _animator.SetBool("Run", true);
                    _ableBreakWallRigidbodys[i].constraints = RigidbodyConstraints.FreezeAll;
                }

                // Rayを発射してCubeを検知
                if (Physics.Raycast(rayOrigin, transform.forward, out hit, 2f))
                {
                    if (hit.collider.gameObject == _ableBreakWallObjs[i])
                    {
                        // J + W or S or A or D で移動
                        if (Input.GetKey(KeyCode.J))
                        {
                            _ableBreakWallRigidbodys[i].constraints = RigidbodyConstraints.FreezeRotation;

                            if (Input.GetKey(KeyCode.W))
                            {
                                _ableBreakWallRigidbodys[i].MovePosition(_ableBreakWallRigidbodys[i].position + Vector3.forward * 3 * Time.deltaTime);
                            }
                            else if (Input.GetKey(KeyCode.S))
                            {
                                _ableBreakWallRigidbodys[i].MovePosition(_ableBreakWallRigidbodys[i].position + Vector3.back * 6 * Time.deltaTime);
                            }
                            else if (Input.GetKey(KeyCode.A))
                            {
                                _ableBreakWallRigidbodys[i].MovePosition(_ableBreakWallRigidbodys[i].position + Vector3.left * 3 * Time.deltaTime);
                            }
                            else if (Input.GetKey(KeyCode.D))
                            {
                                _ableBreakWallRigidbodys[i].MovePosition(_ableBreakWallRigidbodys[i].position + Vector3.right * 3 * Time.deltaTime);
                            }
                            else
                            {
                                _animator.SetBool("Run", true);
                                _animator.SetBool("StanbyMode", true);
                                _ableBreakWallRigidbodys[i].constraints = RigidbodyConstraints.FreezeAll;
                            }
                        }
                    }
                }

                // ✅ Jボタンだけ押して、方向キーが押されていない場合は待機（Idle）にする
                if (Input.GetKey(KeyCode.J) && _inputVertical == 0 && _inputHorizontal == 0)
                {
                    _animator.SetBool("Push", false);
                    _animator.SetBool("Pull", false);
                    _animator.SetBool("Run", false);
                    _animator.SetBool("StanbyMode", true);
                }
                else if (_inputVertical != 0 || _inputHorizontal != 0)
                {
                    _animator.SetBool("Run", true);

                }
                else
                {
                    _animator.SetBool("Idle", true);
                }
            }

            // Enemyに当たったら死亡処理
            if (Physics.Raycast(rayforward, out hit, 0.5f) && hit.collider.tag == "Enemy")
            {
                // TODO: 本来はEnemyを非アクティブにしたくない。
                _enemies[0].SetActive(false);
                _rigidbody.velocity = Vector3.zero;
                _animator.SetBool("Death", true);
                Invoke(nameof(Death), 1.8f);
            }

            if (Physics.Raycast(rayright, out hit, 0.5f) && hit.collider.tag == "Enemy")
            {
                // TODO: 本来はEnemyを非アクティブにしたくない。
                _enemies[1].SetActive(false);
                _rigidbody.velocity = Vector3.zero;
                _animator.SetBool("Death", true);
                Invoke(nameof(Death), 1.8f);
            }

            if (Physics.Raycast(rayleft, out hit, 0.5f) && hit.collider.tag == "Enemy")
            {
                // TODO: 本来はEnemyを非アクティブにしたくない。
                _enemies[2].SetActive(false);
                _rigidbody.velocity = Vector3.zero;
                _animator.SetBool("Death", true);
                Invoke(nameof(Death), 1.8f);
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

            _animator.SetBool("Run", true);
            _animator.SetBool("Push", false);
            _animator.SetBool("Pull", false);
            _animator.SetBool("StanbyMode", false);
        }
        else
        {
            _animator.SetBool("Run", false);
            _animator.SetTrigger("Idle");
        }
    }

    /// <summary>
    /// 爆弾を置く
    /// </summary>
    private void PlaceBomb()
    {
        Vector3 pos = transform.position;

        // プレイヤーの座標をタイル番号（整数）に変換
        int tileX = Mathf.RoundToInt(pos.x / TileSize);
        int tileZ = Mathf.RoundToInt(pos.z / TileSize);

        Vector2Int tilePos = new Vector2Int(tileX, tileZ);

        // すでに同じタイルに爆弾があるなら置かない
        if (_gameManager.IsBombAtPosition(tilePos))
        {
            return;
        }

        // タイル中心のワールド座標を計算
        float centerX = tileX * TileSize;
        float centerZ = tileZ * TileSize;

        Vector3 placePos = new Vector3(centerX, 1f, centerZ);

        // 爆弾生成
        var bombObj = Instantiate(_bombPrefab, placePos, Quaternion.identity);

        // 必要なら参照を取る
        var bomb = bombObj.GetComponent<Explode>();

        _gameManager.AddBombAtPosition(tilePos);
    }

    /// <summary>
    /// アニメーション遷移
    /// </summary>
    private void ChangeAnim(GameObject obj, float distance)
    {
        if (obj.tag != "Cube")
        {
            return;
        }

        //移動アニメーションの制御
        if (Input.GetKey(KeyCode.J) && distance < 2.8f)
        {
            _animator.SetBool("StanbyMode", true);
            _animator.SetBool("Run", false);
        }
        else
        {
            _animator.SetBool("StanbyMode", false);
            _animator.SetBool("Push", false);
            _animator.SetBool("Pull", false);
            _animator.SetBool("Kick", false);
        }
    }

    /// <summary>
    /// 死亡処理
    /// </summary>
    void Death()
    {
        Destroy(gameObject);
    }
}
