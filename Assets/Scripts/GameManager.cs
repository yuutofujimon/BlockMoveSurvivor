using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// 指定位置に爆弾があるかどうかを記録
    /// </summary>
    private HashSet<Vector2Int> _bombGrid = new HashSet<Vector2Int>();

    /// <summary>
    /// 指定位置に爆弾があるかどうか
    /// </summary>
    public bool IsBombAtPosition(Vector2Int position)
    {
        return _bombGrid.Contains(position);
    }

    /// <summary>
    /// 爆弾が置かれた位置を記録
    /// </summary>
    /// <param name="position"> 爆弾が置かれた位置 </param>
    public void AddBombAtPosition(Vector2Int position)
    {
        _bombGrid.Add(position);
    }

    /// <summary>
    /// 指定位置の爆弾情報を削除
    /// </summary>
    /// <param name="position"> 爆弾が置かれていた位置 </param>
    public void RemoveBombAtPosition(Vector2Int position)
    {
        _bombGrid.Remove(position);
    }
}
