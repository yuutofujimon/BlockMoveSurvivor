using UnityEngine;

/// <summary>
/// 壁をつかむ判定
/// </summary>
public class CatchWall : MonoBehaviour
{
    /// <summary>
    /// 壁をつかむことができるかどうか
    /// </summary>
    public bool AbleCatchWall { get; private set; }

    /// <summary>
    /// プレイヤーからつかんだ壁の方向
    /// </summary>
    public Vector3 WallDirection { get; private set; }

    /// <summary>
    /// 接触判定のある壊せる壁
    /// </summary>
    private AbleBreakWall _ableBreakWall;

    /// <summary>
    /// プレイヤーのつかむ判定が接触したとき
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("AbleBreakWall"))
        {
            AbleCatchWall = true;
            _ableBreakWall = other.GetComponent<AbleBreakWall>();

            // プレイヤーから当たった壁の向きを保存
            WallDirection = other.transform.position - transform.position;
            // Y軸方向の成分を除去
            WallDirection = new Vector3(WallDirection.x, 0, WallDirection.z);
            // 正規化
            WallDirection.Normalize();
        }
    }

    /// <summary>
    /// プレイヤーのつかむ判定が離れたとき
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("AbleBreakWall"))
        {
            AbleCatchWall = false;
            //_ableBreakWall = null;
        }
    }

    public void OnPushPullAction(Vector3 dir)
    {
        _ableBreakWall.OnPushPullAction(dir);
    }
}
