using UnityEngine;

/// <summary>
/// ゲームマネージャー
/// </summary>
public class GameManager : MonoBehaviour
{
    //キューブ用変数
    [SerializeField]
    GameObject[] cubeObjects = new GameObject[10];

    //キューブスクリプト用の変数
    Cube[] cubeScripts = new Cube[10];

    //ゲームステータス用変数
    int gameStatus = 0;

    //時間管理用変数
    float myTime = 0.0f;

    /// <summary>
    /// Start
    /// </summary>
    void Start()
    {
        //キューブスクリプトを取得
        for (var i = 0; i< cubeObjects.Length; i++)
        {
            cubeScripts[i] = cubeObjects[i].GetComponent<Cube>();
        }

    }

    //初期設定用メソッド
    void Init()
    {
        //キューブを配置
        for(int i = 0; i < cubeObjects.Length; i++)
        {
            cubeScripts[i].doSetPos(i);
            //キューブのイージング設定
            cubeScripts[i].doSetEasing();
        }
        //ステータス更新
        gameStatus++;
    }

    //イージング用メソッド
    void Easing()
    {
        for (int i = 0; i < 10; i++)
        {
            cubeScripts[i].doEasing(i);
        }
        if(Wait(2.0f) == true)
        {
            //ステータスを更新
            gameStatus++;
        }
    }

    //インゲーム用メソッド
    void InGame()
    {
        //スペースキーの入力確認
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < cubeScripts.Length; i++)
            {
                cubeScripts[i].doSetAnim();
            }
        }
        //キューブのアニメーション
        for(int i = 0; i < cubeObjects.Length; i++)
        {
            cubeScripts[i].doAnim(i);
        }
    }

    //アウトゲーム用メソッド
    void OutGame()
    {
        //ステータス処理
        switch (gameStatus)
        {
            case 0:
                Init();//初期設定
                break;
            case 1:
                Easing();//イージング
                break;
            case 2:
                InGame();//インゲーム
                break;
            default:
                break;

        }
    }

    //時間待機用メソッド
    bool Wait(float inTime)
    {
        myTime += Time.deltaTime;
        if(myTime > inTime)
        {
            myTime = 0.0f;
            return true;
        }
        return false;
    }


    /// <summary>
    /// Update
    /// </summary>
    void Update()
    {
        //アウトゲーム
        OutGame();
    }
}
