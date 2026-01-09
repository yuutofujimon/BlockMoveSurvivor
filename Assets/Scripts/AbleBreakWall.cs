using UnityEngine;

public class AbleBreakWall : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Vector3 _moveVelocity;

    /// <summary>
    /// Awake
    /// </summary>
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    /// <summary>
    /// Update
    /// </summary>
    private void Update()
    {
        // TODO: 壁が動いている際、次のグリッドに移動で来たかを判定する
    }

    void FixedUpdate()
    {
        if (_moveVelocity != Vector3.zero)
        {
            _rigidbody.MovePosition(_rigidbody.position + _moveVelocity * Time.fixedDeltaTime);
        }
    }

    public void OnPushPullAction(Vector3 dir)
    {
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        _moveVelocity = dir * 3f;
    }

    public void OnReleasedAction()
    {
        _moveVelocity = Vector3.zero;
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }
}