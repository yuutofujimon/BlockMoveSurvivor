using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// プレイヤー
/// </summary>
public class Player : MonoBehaviour
{
    private Rigidbody rigidBody; //Rigidbody コンポーネント
    private Animator animator; //アニメーターコンポーネント
    private float inputHorizontal; //横方向の入力
    private float inputVertical; //縦方向の入力
    private string answerTag = "Cube";
    public GameObject gameobject;
    public float pushDistance = 2.5f; //押す距離の値
    [SerializeField] private float rayDistance = 1.5f;
    public float basePushForce = 10f;   //通常の押す力
    public float enhancedPushForce = 1000f;
    [SerializeField] GameObject player;
    [SerializeField] GameObject[] cubes;
    [SerializeField] Rigidbody[] cubeRigidbodys;
    [SerializeField] GameObject[] StageWalls;
    [SerializeField] private float detectDistance = 2.8f;
    [Tooltip("X軸方向に移動する振幅(0にすると移動しない)")]
    private float amplitudeX = 4.0f;
    [SerializeField]
    [Tooltip("Z軸方向に移動する振幅(0にすると移動しない)")]
    private float amplitudeZ = 4.0f;
    Rigidbody playerRigidbody;
    Player player1;
    public Transform Cube;
    public float kickForce = 10f;

    // 方向と対応キーをまとめた構造体
    private struct DirectionInfo
    {
        public Vector3 dir;
        public KeyCode pushKey;
        public KeyCode pullKey;
        public string name;

        public DirectionInfo(Vector3 dir, KeyCode pushKey, KeyCode pullKey, string name)
        {
            this.dir = dir;
            this.pushKey = pushKey;
            this.pullKey = pullKey;
            this.name = name;
        }
    }

    // 前・後・右・左の4方向と対応するキーを登録
    private DirectionInfo[] directions;

    // Start is called before the first frame update
    void Start()
    { 
        //コンポーネントを取得
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>(); //アニメーターのコンポーネントの取得

        //プレイヤーのRigidBodyを取得
        Rigidbody playerRigidbody = this.GetComponent<Rigidbody>();

        // 各CubeのRigidbodyを一度だけ取得しておく
        cubeRigidbodys = new Rigidbody[cubes.Length];

        cubes = GameObject.FindGameObjectsWithTag("Cube");
        int count = Mathf.Min(cubes.Length, StageWalls.Length);

        for (int i = 0; i < count; i++)
        {
            if (cubes[i]) cubeRigidbodys[i] = cubes[i].GetComponent<Rigidbody>();
        }

        // 方向とキーの対応を初期化
        directions = new DirectionInfo[]
        {
            new DirectionInfo(Vector3.forward, KeyCode.W, KeyCode.S, "Forward"),
            new DirectionInfo(Vector3.back,    KeyCode.S, KeyCode.W, "Back"),
            new DirectionInfo(Vector3.right,   KeyCode.D, KeyCode.A, "Right"),
            new DirectionInfo(Vector3.left,    KeyCode.A, KeyCode.D, "Left")
        };
    }


    // 距離判定（dis1, dis2…の代わりに共通化）
    bool IsNear(int idx)
    {
        if (!cubes[idx]) return false;
        float d = Vector3.Distance(transform.position, cubes[idx].transform.position);
        return d < pushDistance;
    }

    // 共通処理（Push/Pull解除、回転固定、ログ）
    void ApplyCommon(int idx)
    {
        if (animator)
        {
            animator.SetBool("Push", false);
            animator.SetBool("Pull", false);
        }

        if (cubeRigidbodys[idx])
        {
            cubeRigidbodys[idx].constraints = RigidbodyConstraints.FreezeRotation;
        }

        Debug.Log("Did not Pull");
    }

    void HandlePlayerMovement(float inputVertical, float inputHorizontal)
    {

        Vector3 moveDir = new Vector3(inputHorizontal, 0, inputVertical).normalized;

        if (moveDir.magnitude > 0)
        {
            // プレイヤーを移動
            transform.Translate(moveDir * 6 * Time.deltaTime, Space.World);

            // 向きを移動方向に合わせる（任意）
            transform.rotation = Quaternion.LookRotation(moveDir);

            animator.SetBool("Run", true);
            animator.SetBool("Push", false);
            animator.SetBool("Pull", false);
        }
        else
        {
            animator.SetBool("Run", false);
            animator.SetTrigger("Idle");
        }

        Debug.Log("実装可能③");
    }

    // Update is called once per frame
    public void Update()
    {
        Debug.Log("Update呼ばれてる");
        Debug.Log($"cubes配列の長さ: {cubes.Length}");
        Debug.Log($"StageWalls配列の長さ: {StageWalls.Length}");
        HandlePlayerMovement(inputVertical, inputHorizontal);
        int count = Mathf.Min(cubes.Length, StageWalls.Length);
        for (int i = 0; i < count; i++)
        {
            float distance = Vector3.Distance(cubes[i].transform.position, StageWalls[i].transform.position);
            Debug.Log($"Cube{i + 1} との距離: {distance}");
            Debug.Log($"For文の中に入っている: {i}");
        }
        Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();

        //横方向と縦方向の入力を取得
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");

        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
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
            ChangeAnim(hitColliderObj, Vector3.Distance(hitColliderObj.transform.position, player.transform.position));
            Debug.Log("ここまで実装出来ている");
        }

        // Cube全体に対して同じ処理を回す
        for (int i = 0; i < count; i++)
        {
            GameObject cube = cubes[i];
            Rigidbody rb = cubeRigidbodys[i];
            float distance = Vector3.Distance(cubes[i].transform.position, StageWalls[i].transform.position);
            Debug.Log($"Cube{i + 1} との距離: {distance}");
            Debug.Log("For文の中に入っている");
            Debug.Log($"cubes配列の要素数: {cubes?.Length}");

            if (distance < pushDistance &&
                Physics.Raycast(rayforward, out hit, rayDistance) &&
                hit.collider.gameObject == cubes[i])
            {
                animator.SetBool("Push", false);
                animator.SetBool("Pull", false);

                cubeRigidbodys[i].constraints = RigidbodyConstraints.FreezeRotation;
                Debug.Log($"Did not Pull: {cubes[i].name}");

                // 該当のCubeが見つかったら、残りのCubeはチェックしなくてよい
                break;
            }

            Debug.Log("実装出来ている②");

            // --- 前方向へのRay ---
            if (Input.GetKey(KeyCode.J) && Input.GetKeyDown(KeyCode.W) &&
                Physics.Raycast(rayforward, out hit, rayDistance) &&
                hit.collider.CompareTag("Cube") && hit.collider.gameObject == cube)
            {
                animator.SetBool("Push", true);
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.transform.position += Vector3.forward * 3 * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.J) && Input.GetKeyDown(KeyCode.S) &&
                Physics.Raycast(rayforward, out hit, rayDistance) &&
                hit.collider.CompareTag("Cube") && hit.collider.gameObject == cube)
            {
                animator.SetBool("Pull", true);
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.transform.position += Vector3.back * 3 * Time.deltaTime;
                Debug.Log($"Did Pull: {cube.name}");
            }

            // --- 後ろ方向へのRay ---
            else if (Input.GetKey(KeyCode.J) && Input.GetKeyDown(KeyCode.S) &&
                Physics.Raycast(rayback, out hit, rayDistance) &&
                hit.collider.CompareTag("Cube") && hit.collider.gameObject == cube)
            {
                animator.SetBool("Push", true);
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.transform.position += Vector3.back * 3 * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.J) && Input.GetKeyDown(KeyCode.W) &&
                Physics.Raycast(rayback, out hit, rayDistance) &&
                hit.collider.CompareTag("Cube") && hit.collider.gameObject == cube)
            {
                animator.SetBool("Pull", true);
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.transform.position += Vector3.forward * 3 * Time.deltaTime;
                Debug.Log($"Did Pull: {cube.name}");
            }
            else if (inputVertical != 0)
            {
                animator.SetBool("Run", true);
                rb.constraints = RigidbodyConstraints.FreezeAll;
                Debug.Log("押している");
            }

            // --- 右方向へのRay ---
            if (Input.GetKey(KeyCode.J) && Input.GetKeyDown(KeyCode.D) &&
                Physics.Raycast(rayright, out hit, rayDistance) &&
                hit.collider.CompareTag("Cube") && hit.collider.gameObject == cube)
            {
                animator.SetBool("Push", true);
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.transform.position += Vector3.right * 3 * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.J) && Input.GetKeyDown(KeyCode.A) &&
                Physics.Raycast(rayright, out hit, rayDistance) &&
                hit.collider.CompareTag("Cube") && hit.collider.gameObject == cube)
            {
                animator.SetBool("Pull", true);
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.transform.position += Vector3.left * 3 * Time.deltaTime;
                Debug.Log($"Did Pull: {cube.name}");
            }

            // ---左方向へのRay ---
            else if (Input.GetKey(KeyCode.J) && Input.GetKeyDown(KeyCode.A) &&
                Physics.Raycast(rayleft, out hit, rayDistance) &&
                hit.collider.CompareTag("Cube") && hit.collider.gameObject == cube)
            {
                animator.SetBool("Push", true);
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.transform.position += Vector3.left * 3 * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.J) && Input.GetKeyDown(KeyCode.D) &&
                Physics.Raycast(rayback, out hit, rayDistance) &&
                hit.collider.CompareTag("Cube") && hit.collider.gameObject == cube)
            {
                animator.SetBool("Pull", true);
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.transform.position += Vector3.right * 3 * Time.deltaTime;
                Debug.Log($"Did Pull: {cube.name}");
            }
            else if (inputHorizontal != 0)
            {
                animator.SetBool("Run", true);
                rb.constraints = RigidbodyConstraints.FreezeAll;
                Debug.Log("押している");
            

            


                foreach (var info in directions)
                {
                    Ray ray = new Ray(transform.position, info.dir);

                    // 押す（Push）
                    if (Input.GetKey(KeyCode.J) &&
                        Input.GetKeyDown(info.pushKey) &&
                        Physics.Raycast(ray, out hit, rayDistance) &&
                        hit.collider.CompareTag("Cube") &&
                        hit.collider.gameObject == cube)
                    {
                        animator.SetBool("Push", true);
                        rb.constraints = RigidbodyConstraints.FreezeRotation;
                        rb.transform.position += info.dir * 3 * Time.deltaTime;
                    }
                    // 引く（Pull）
                    else if (Input.GetKey(KeyCode.J) &&
                             Input.GetKeyDown(info.pullKey) &&
                             Physics.Raycast(ray, out hit, rayDistance) &&
                             hit.collider.CompareTag("Cube") &&
                             hit.collider.gameObject == cube)
                    {
                        animator.SetBool("Pull", true);
                        rb.constraints = RigidbodyConstraints.FreezeRotation;
                        rb.transform.position += -info.dir * 3 * Time.deltaTime;
                        Debug.Log($"Did Pull: {cube.name} ({info.name})");
                    }
                    else if (Physics.Raycast(ray, out hit, rayDistance))
                    {
                        animator.SetBool("Run", true);
                        rb.constraints = RigidbodyConstraints.FreezeAll;
                    }
                }

                // どの方向のキーも押していない場合
                if (inputVertical != 0 || inputHorizontal != 0)
                {
                    animator.SetBool("Run", true);
                    rb.constraints = RigidbodyConstraints.FreezeAll;
                    // ✅ Jボタンだけ押して、方向キーが押されていない場合は待機（Idle）にする
                    if (Input.GetKey(KeyCode.J) && inputVertical == 0 && inputHorizontal == 0)
                    {
                        animator.SetBool("Push", false);
                        animator.SetBool("Pull", false);
                        animator.SetBool("Run", false);
                        animator.SetTrigger("Idle");   // Idle用のTriggerまたはBoolを使う
                    }
                }
                Debug.Log("実装可能③");
            }

            //DummyModel
            if (Physics.Raycast(rayforward, out hit, 0.5f) && hit.collider.name == "DummyModel")
            {
                player1 = gameObject.GetComponent<Player>();
                player1.enabled = false;
                playerRigidbody.velocity = Vector3.zero;
                animator.SetBool("Death", true);
                Invoke(nameof(doDestroy), 1.8f);
            }

            if (Physics.Raycast(rayright, out hit, 0.5f) && hit.collider.name == "DummyModel")
            {
                player1 = gameObject.GetComponent<Player>();
                player1.enabled = false;
                playerRigidbody.velocity = Vector3.zero;
                animator.SetBool("Death", true);
                Invoke(nameof(doDestroy), 1.8f);
            }

            if (Physics.Raycast(rayleft, out hit, 0.5f) && hit.collider.name == "DummyModel")
            {
                player1 = gameObject.GetComponent<Player>();
                player1.enabled = false;
                playerRigidbody.velocity = Vector3.zero;
                animator.SetBool("Death", true);
                Invoke(nameof(doDestroy), 1.8f);
            }

            if (Physics.Raycast(rayback, out hit, 0.5f) && hit.collider.name == "DummyModel")
            {
                player1 = gameObject.GetComponent<Player>();
                player1.enabled = false;
                playerRigidbody.velocity = Vector3.zero;
                animator.SetBool("Death", true);
                Invoke(nameof(doDestroy), 1.8f);
            }


            //DummyModel2
            if (Physics.Raycast(rayforward, out hit, 0.5f) && hit.collider.name == "DummyModel2")
            {
                player1 = gameObject.GetComponent<Player>();
                player1.enabled = false;
                playerRigidbody.velocity = Vector3.zero;
                animator.SetBool("Death", true);
                Invoke(nameof(doDestroy), 1.8f);
            }

            if (Physics.Raycast(rayright, out hit, 0.5f) && hit.collider.name == "DummyModel2")
            {
                player1 = gameObject.GetComponent<Player>();
                player1.enabled = false;
                playerRigidbody.velocity = Vector3.zero;
                animator.SetBool("Death", true);
                Invoke(nameof(doDestroy), 1.8f);
            }

            if (Physics.Raycast(rayleft, out hit, 0.5f) && hit.collider.name == "DummyModel2")
            {
                player1 = gameObject.GetComponent<Player>();
                player1.enabled = false;
                playerRigidbody.velocity = Vector3.zero;
                animator.SetBool("Death", true);
                Invoke(nameof(doDestroy), 1.8f);
            }

            if (Physics.Raycast(rayback, out hit, 0.5f) && hit.collider.name == "DummyModel2")
            {
                player1 = gameObject.GetComponent<Player>();
                player1.enabled = false;
                playerRigidbody.velocity = Vector3.zero;
                animator.SetBool("Death", true);
                Invoke(nameof(doDestroy), 1.8f);
            }

            if (Physics.Raycast(rayforward, out hit, 0.5f) && hit.collider.name == "StageWall2")
            {
                Invoke(nameof(OnCollisionEnter), 1.8f);
            }

        }
    }

    /// <summary>
    /// アニメーション遷移
    /// </summary>
    private void ChangeAnim(GameObject obj, float distance)
    {
        if(obj.tag != "Cube")
        {
            return;
        }

        //移動アニメーションの制御
        if (Input.GetKey(KeyCode.J) && distance < 2.8f)
        {
            animator.SetBool("StanbyMode", true);
            animator.SetBool("Run", false);
        }
        else
        {
            animator.SetBool("StanbyMode", false);
            animator.SetBool("Push", false);
            animator.SetBool("Pull", false);
            animator.SetBool("Kick", false);
        }
        Debug.Log("Hit to Cube Forward");
    }

    void doDestroy()
    {
        Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision col)
    {

        Vector3 rayPosition = transform.position + new Vector3(0, 1f, 0);
        Debug.DrawRay(rayPosition, transform.TransformDirection(Vector3.forward) * 1.0f, Color.yellow);
        Ray rayforward = new Ray(rayPosition, Vector3.forward);
        Ray rayright = new Ray(rayPosition, Vector3.right);
        Ray rayleft = new Ray(rayPosition, Vector3.left);
        Ray rayback = new Ray(rayPosition, Vector3.back);
        RaycastHit hit;

        
        if (animator.GetBool("Push"))
        {

        }

        if(animator.GetBool("Pull"))
        {
            
        }

        Debug.Log("Hit");
        //Debug.Log(col.transform.position);
    }

    void OnCollisionExit(Collision col)
    {
        if(col.gameObject.CompareTag("Cube"))
        {
            animator.SetBool("Kick", false);
        }
        if (animator.GetBool("Push"))
        {
            
        }

        if (animator.GetBool("Pull"))
        {
            
        }
    }      
}
