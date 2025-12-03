using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMeshを使ってキャラクターを歩行
/// </summary>
public class NavWalk : MonoBehaviour
{
    [Header("ナビメッシュエージェント"),SerializeField]
    private NavMeshAgent _agent;

    [Header("アニメーター"),SerializeField]
    private Animator _animator;

    //目的地の数と場所の設定
    public Transform[] Points;

    //最初の目的地
    private int destPoint_ = 0;

    /// <summary>
    /// Start
    /// </summary>
    void Start()
    {
        GotoNextPoint();
    }

    void GotoNextPoint()
    {
        // 地点がなにも設定されていないときに返します
        if (Points.Length == 0)
        {
            return;
        }

        // エージェントが現在設定された目標地点に行くように設定します
        _agent.destination = Points[destPoint_].position;

        // 配列内の次の位置を目標地点に設定し、
        // 必要ならば出発地点にもどります
        destPoint_ = (destPoint_ + 1) % Points.Length;
    }

    /// <summary>
    /// Update
    /// </summary>
    void Update()
    {
        int layerMask = 1 << 8;
        layerMask = ~layerMask;

        Vector3 rayPosition = transform.position + new Vector3(0, 1f, 0);
        Debug.DrawRay(rayPosition, transform.TransformDirection(Vector3.forward) * 1.0f, Color.yellow);
        Ray rayforward = new Ray(rayPosition, Vector3.forward);
        Ray rayright = new Ray(rayPosition, Vector3.right);
        Ray rayleft = new Ray(rayPosition, Vector3.left);
        Ray rayback = new Ray(rayPosition, Vector3.back);
        RaycastHit hit;
        // エージェントが現目標地点に近づいてきたら、
        // 次の目標地点を選択します
        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
            GotoNextPoint();
        //アニメーションの切り替え
        _animator.SetFloat("Move", _agent.velocity.magnitude);

        if (Physics.Raycast(rayforward, out hit, 3.0f)&& hit.collider.name == "Cube11")
        {
            GotoNextPoint();
            //アニメーションの切り替え
            _animator.SetFloat("Move", _agent.velocity.magnitude);
            
        }

        if (Physics.Raycast(rayright, out hit, 3.0f) && hit.collider.name == "Cube11")
        {
                GotoNextPoint();
                //アニメーションの切り替え
                _animator.SetFloat("Move", _agent.velocity.magnitude);
        }

        if (Physics.Raycast(rayleft, out hit, 3.0f) && hit.collider.name == "Cube11")
        {
                GotoNextPoint();
                //アニメーションの切り替え
                _animator.SetFloat("Move", _agent.velocity.magnitude);
        }

        if (Physics.Raycast(rayback, out hit, 3.0f) && hit.collider.name == "Cube11")
        {
                GotoNextPoint();
                //アニメーションの切り替え
                _animator.SetFloat("Move", _agent.velocity.magnitude);
        }
    }
}