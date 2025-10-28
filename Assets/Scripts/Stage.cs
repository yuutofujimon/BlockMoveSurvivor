using UnityEngine;

/// <summary>
/// ステージ
/// </summary>
public class Stage : MonoBehaviour
{
    private GameObject[,] gameObjectArray_ = new GameObject[7, 13];

    public GameObject TilePrefab; // マス用のプレハブをInspectorで指定

    /// <summary>
    /// Start
    /// </summary>
    void Start()
    {
        int height = gameObjectArray_.GetLength(0);
        int width = gameObjectArray_.GetLength(1);

        //ステージの中心が原点に来るようにオフセットを計算
        float offsetX = -(width * 2.4f) / 2.0f + 2.4f / 2.0f;
        float offsetZ = -(height * 2.4f) / 2.0f + 2.4f / 2.0f;
    }

    /// <summary>
    /// CSVからマップを生成
    /// </summary>
    void GenerateMapFromCSV(string fileName)
    {
        // ResourcesフォルダからCSVを読み込む
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);
        if (csvFile == null)
        {
            Debug.LogError($"CSVファイル {fileName} が見つかりません！");
            return;
        }

        // CSVを行ごとに分割
        string[] lines = csvFile.text.Split('\n');

        for (var y = 0; y < lines.Length; y++)
        {
            // 行をカンマで分割して値を取得
            string[] values = lines[y].Trim().Split(',');

            for (var x = 0; x < values.Length; x++)
            {
                // 値が "1" の場合のみマスを生成
                if (values[x] == "1")
                {
                    // マス（タイル）を生成
                    Vector3 position = new Vector3(x, 0, -y); // 例: X-Z平面に配置
                    Instantiate(TilePrefab, position, Quaternion.identity);
                }
            }
        }
    }
}
