/////////////////////////////////////////////////////////////////
///進化計算論「遺伝的アルゴリズムで下駄占いを学習させよう」
///1910677　山崎萌恵（リーダー）
///1910481　永井 美央花			
///1910563 福岡 美結
///
/// Arrange.cs
/////////////////////////////////////////////////////////////////

/*-----------------------------------------------------------------*/
//Arrange.cs内のプログラムはすべて福岡が担当しました。
/*-----------------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrange : MonoBehaviour
{
    // Start is called before the first frame update
    // オブジェクトを生成する元となるPrefabへの参照を保持します。
    public GameObject sourceObj;

    // 生成したオブジェクトの親オブジェクトへの参照を保持します。
    public Transform parentTran;

    static int POP_SIZE = 200;/*集団のサイズ*/
    void Start()
    {
      Debug.Log("Arrage Start!");

        CreateObjects();
        sourceObj.SetActive(false);
    }

    void CreateObjects(){
      float xOffset = 5.0f;
      float xPos;
      float yPos = 5.5f;
      float zPos = 0.0f;
      GameObject[] obj = new GameObject[POP_SIZE];
      for (int i = 0; i < POP_SIZE; i++)
       {
         // ゲームオブジェクトを生成します。

         obj[i] = Instantiate(sourceObj, Vector3.zero, Quaternion.identity);

         // ゲームオブジェクトの親オブジェクトを設定します。
         obj[i].transform.SetParent(parentTran);

         // ゲームオブジェクトの位置を設定します。
         xPos = xOffset * i;
         if(sourceObj.name=="upleg_sa_R"){
           yPos = 5.5f;
           zPos = 0.0f;
           obj[i].transform.GetChild(2).transform.GetChild(2).transform.GetChild(1).gameObject.layer = 9;
         }else if(sourceObj.name=="Crogs"){
           yPos = 1.2f;
           zPos = 0.7f;
           obj[i].layer = 2;
         }
         obj[i].transform.localPosition = new Vector3(xPos, yPos, zPos);

       }
    }
}
