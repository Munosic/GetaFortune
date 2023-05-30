/////////////////////////////////////////////////////////////////
///進化計算論「遺伝的アルゴリズムで下駄占いを学習させよう」
///1910677　山崎萌恵（リーダー）
///1910481　永井 美央花			
///1910563 福岡 美結
///
/// GAAnimation.cs
/////////////////////////////////////////////////////////////////

/*-----------------------------------------------------------------*/
//GAAnimation.cs内のプログラムはすべて福岡が担当しました。
/*-----------------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using DG.Tweening;

public partial class GAProgram : MonoBehaviour
{
    public Transform LegParent;
    private int[] curFrame = new int[POP_SIZE]; //現在のフレーム数
    private int[] curMove = new int[POP_SIZE]; //現在の動きの番号
    private float[] transition = new float[POP_SIZE]; //遷移時間
    private bool[] initState = new bool[POP_SIZE];
    private bool[] isMove = new bool[POP_SIZE];
    private float[] SumTransition = new float[POP_SIZE];

    /*回転の速度（外部から調整可能）*/
    public float u_speedX = -1.5f;
    public float k_speedX = -0.2f;
    public float f_speedX = -0.2f;
    float[,] speed = new float[POP_SIZE, PARAMETER - 1];

    /*現在の回転角度*/
    float [,] rotate = new float[POP_SIZE, PARAMETER - 1];

    static int[,] Range = new int[PARAMETER,2]{
      { -100, 20 }, //uplegRangeX
      { 0, 140 }, //kneeRangeX
      { 0, 45 }, //footRangeX
      { 30, 100 }, //遷移時間
    };

    /*1セットの遷移の速度(angulerVelocity)*/
    private float TransitionSpeed_anguler(float preRotate, float curRotate, float transition){
      float sp = ((curRotate-preRotate)*Mathf.Deg2Rad)/transition; //1秒で増える角度(angulerVelocity)
      return sp;
    }

    /*1セットの遷移の速度*/
    private float TransitionSpeed(float preRotate, float curRotate, float transition){
      float sp = (curRotate-preRotate)/transition; //1フレームで増える角度
      return sp;
    }

    /*遷移時間の最大値を求める*/
    private float maxCulcDuration()
    {
        float maxDuration = 0;
        float[] duration = new float[POP_SIZE];
        for (int i = 0; i < POP_SIZE; i++)
        {
            for (int j = 0; j < NUM_MOVE; j++)
            {
                duration[i] += chrom[i, j, PARAMETER - 1];
                if (duration[i] > maxDuration)
                {
                    maxDuration = duration[i];
                }
            }
        }
        return (maxDuration + 500) / 200;//下駄が落ちるまでの時間も考慮に入れた
    }

    /*足全体の回転アニメーション(Rigidbody.MoveRotation)*/
    void rotateObjectsRB2(Transform uplegRB, int i){
      Transform upleg = uplegRB.transform.GetChild(0);
      Transform knee = upleg.transform.GetChild(2);
      Transform foot = knee.transform.GetChild(2);
      float preRotate = 0; //一つ前の動きの回転値
      float curRotate; //現在の動きの回転値
      if(curFrame[i] <= 0)
        {
          isMove[i] = false;
            if (curFrame[i] == 0)
            {
                curMove[i] = -1;
                initState[i] = false;
                isMove[i] = true;
                curFrame[i] = 1000;
            }
        }

      if(curFrame[i] > transition[i]){ //遷移時間を超えたら(各動きにつき一回通る)
        curMove[i] += 1; //次の動きの数にする。
        isMove[i] = true;

        if(curMove[i] == 0){ //最初の動きの時
          preRotate = 0; //初期状態をpreRotateとする
        }else if(curMove[i] >= NUM_MOVE){
          isMove[i] = false;
        }
        if(isMove[i]){
          transition[i] = chrom[i,curMove[i],PARAMETER-1]; //遷移時間を現在の動きの遷移時間に設定
          curFrame[i] = 1; //フレーム数を1に戻す
          /*各回転パラメータに対して、遷移速度を計算*/
          for(int j = 0; j < PARAMETER - 1; j++){
            if(curMove[i] > 0){preRotate = chrom[i,curMove[i]-1,j];} //一つ前の動きの回転値
            curRotate = chrom[i,curMove[i],j]; //現在の動きの回転値
            speed[i,j] = TransitionSpeed(preRotate,curRotate,transition[i]);
          }
        }
      }

      if(isMove[i]){
        /*各パラメータに対して、回転値を計算*/
        for(int j = 0; j < PARAMETER - 1; j++){
          rotate[i,j] += speed[i,j];
        }

        /*各関節の回転値を書き換え*/
        upleg.gameObject.GetComponent<Rigidbody>().MoveRotation(Quaternion.AngleAxis(rotate[i,0], new Vector3(1.0f,0.0f,0.0f)));
        knee.gameObject.GetComponent<Rigidbody>().MoveRotation(Quaternion.AngleAxis(rotate[i,1]+rotate[i,0], new Vector3(1.0f,0.0f,0.0f)));
        foot.gameObject.GetComponent<Rigidbody>().MoveRotation(Quaternion.AngleAxis(rotate[i,2]+rotate[i,0]+rotate[i,1], new Vector3(1.0f,0.0f,0.0f)));
      }
      curFrame[i] += 1; //現在のフレーム数を1増やす
    }

    /*rigidbodyでの初期化*/
    void InitRotateRB(Transform uplegRB){
      Transform upleg = uplegRB.transform.GetChild(0);
      Transform knee = upleg.transform.GetChild(2);
      Transform foot = knee.transform.GetChild(2);

      /*回転値を初期化*/
      upleg.eulerAngles = new Vector3(0,0,0);
      knee.eulerAngles = new Vector3(0,0,0);
      foot.eulerAngles = new Vector3(0,0,0);

      /*速度の初期化*/
      for(int i = 0; i < POP_SIZE; i++){
        for(int j = 0; j < PARAMETER-1; j++){
          rotate[i,j] = 0;
          speed[i,j] = 0;
        }
      }
    }

    /*全個体のアニメーション*/
    void AllrotateObjects(){
      for(int i = 1; i <= POP_SIZE; i++){
        rotateObjectsRB2(LegParent.transform.GetChild(i), i-1);
      }
    }
}
