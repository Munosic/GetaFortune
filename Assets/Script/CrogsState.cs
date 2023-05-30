/////////////////////////////////////////////////////////////////
///進化計算論「遺伝的アルゴリズムで下駄占いを学習させよう」
///1910677　山崎萌恵（リーダー）
///1910481　永井 美央花			
///1910563 福岡 美結
///
/// CrogState.cs
/////////////////////////////////////////////////////////////////

/*-----------------------------------------------------------------*/
//CrogState.cs内のプログラムはすべて福岡が担当しました。
/*-----------------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrogsState : MonoBehaviour
{
    private Vector3 pos; //下駄の座標
    private float distance; //Rayの長さ
    private Vector3 rayPos; //Rayの座標
    private Ray ray; //Ray
    private RaycastHit hit; //Rayと衝突したオブジェクト情報
    private int weather; //下駄の向き(晴れ：9、それ以外：1)
    private Transform crogs;
    private int CrogsIndex;
    private bool isGround;

    /*コンストラクタ*/
    public CrogsState(Transform crogs, int i) {
      this.crogs = crogs;
      distance = 0.5f; //Rayの長さ
      weather = 1; //天気の初期化
      //weather = 0; //天気の初期化
      CrogsIndex = i;
      pos = new Vector3(0.0f,0.0f,0.0f);
    }

    public void RayCasting(){
      /*Rayの設定*/
      rayPos = crogs.position + new Vector3(0.0f,0.0f,0.0f); //Rayの座標
      ray = new Ray(rayPos, -crogs.up); //晴れで着地判定となるRay
      isGround = Physics.Raycast(ray, out hit, distance); //晴れの向きでオブジェクトに衝突したか
      Debug.DrawRay(rayPos, (-crogs.up)* distance, Color.red); //Rayを可視化

     /*下駄が晴れで地面に衝突した時*/
     if(isGround){
       if(hit.collider.gameObject.name == "Plane"){
         pos = crogs.position; //地面に落ちた時の下駄の座標
         weather = 9;
       }
     }else{
       pos = crogs.position; //地面に落ちた時の下駄の座標
       weather = 1;//天気の初期化
     }
   }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name == "Plane")
        {
            pos = crogs.position;
            Debug.Log("collisionplane");
        }
    }

   public void IgnoreCollision()
    {
        if (isGround)
        {
            Physics.IgnoreLayerCollision(2, 9);
        }
    }

   public void InitCrogsPos(){
     pos = new Vector3(CrogsIndex*5.0f,1.2f,0.7f);
     crogs.position = pos;
     crogs.eulerAngles = new Vector3(0,0,0);
     isGround = false;
   }

   /*下駄の座標取得*/
   public Vector3 GetCrogsPos(){
     return pos;
   }

   /*下駄の向きによる天気(点数)*/
   public int weatherScore(){
     return weather;
   }
}
