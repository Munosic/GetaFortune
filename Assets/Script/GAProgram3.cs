/////////////////////////////////////////////////////////////////
///進化計算論「遺伝的アルゴリズムで下駄占いを学習させよう」
///1910677　山崎萌恵（リーダー）
///1910481　永井 美央花			
///1910563 福岡 美結
///
/// GAProgram3.cs
/////////////////////////////////////////////////////////////////

/*-----------------------------------------------------------------*/
//GAProgram3.cs内のプログラムはすべて福岡が担当しました。
/*-----------------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public partial class GAProgram
{

    /*下駄の生成*/
    public Transform CrogsParent;
    private Transform[] crogs = new Transform[POP_SIZE];
    private CrogsState[] crogsState = new CrogsState[POP_SIZE];
    private void InitCrogs(){
      for(int i = 0; i < POP_SIZE; i++){
        crogs[i] = CrogsParent.transform.GetChild(i+1);
        crogsState[i] = new CrogsState(crogs[i],i);
      }
    }

    /*下駄の状態スコアをfitnessに入れる*/
    private void ScoreArray(){
      float[] distScore = new float[POP_SIZE];
      for(int i = 0; i < POP_SIZE; i++){
            distScore[i] = crogsState[i].GetCrogsPos().z;
            //fitness[i] = crogsState[i].weatherScore();//晴れのみ
            crog_Pos[i] += distScore[i];
           if (distScore[i] < 7.0f) { distScore[i] = 0f; }
            distScore[i] = 10 * (1 - Mathf.Exp(-distScore[i] / 10));
            fitness[i] += crogsState[i].weatherScore()* distScore[i]; //投げた距離も考慮
            if(crogsState[i].weatherScore() == 9) { isSunny[i]++; }
            //fitness[i] += distScore[i] * 100; //投げた距離のみ
        }
    }

    /*下駄の状態スコアをfitnessに入れる（最初に呼ばれる）*/
    private void ScoreArray1()
    {
        float[] distScore = new float[POP_SIZE];
        for (int i = 0; i < POP_SIZE; i++)
        {
            distScore[i] = crogsState[i].GetCrogsPos().z;
            //fitness[i] = crogsState[i].weatherScore();//晴れのみ
            crog_Pos[i] = distScore[i];
            if (distScore[i] < 7.0f) { distScore[i] = 0f; }
            distScore[i] = 10 * (1 - Mathf.Exp(-distScore[i] / 10));
            fitness[i] = crogsState[i].weatherScore() * distScore[i]; //投げた距離も考慮
            if (crogsState[i].weatherScore() == 9) { isSunny[i]=1; }
            else { isSunny[i] = 0; }
            //fitness[i] = distScore[i] * 100; //投げた距離のみ
        }
    }

    /*スコアを表示*/
    private void printScore(){
      string fitnessPath = Application.persistentDataPath + "/fitness.txt";
      File.WriteAllText(fitnessPath, "");//fitness.txtを白紙に戻す
      for(int i = 0; i < POP_SIZE; i++){
        File.AppendAllText(fitnessPath, i+":"+fitness[i]+"\n");
      }
    }

    /*最小値/最大値/合計値を求める*/
    void Statistics()
    {
        int i;
        max = 0;
        min = POP_SIZE;
        sumfitness = 0;

        for (i = 0; i < POP_SIZE; i++){
            if (fitness[i] > max) { max = fitness[i]; n_max = i; }
            if (fitness[i] < min)
            {
                min = fitness[i]; n_min = i;
            }
            sumfitness += fitness[i];
        }
    }

    // 選択
    /*ルーレット選択*/
    int Select()
    {
        int i;
        float sum;
        double rand;

        sum = 0;
        rand = (double)Rand()/((double)(RANDOM_MAX+1));

        for (i = 0; i < POP_SIZE; i++) {
            sum+=fitness[i];
            if ((double)sum / (double)sumfitness > rand) {
                    break;
            }
        }
        return i;
    }

    /*トーナメント選択*/
    int tournamentSelect()
    {
        float gen_max = 0;
        int r;
        int maxIn = 0;
        float tournamentSize = 3;
        for(int i = 0; i < tournamentSize; i++)
        {
            r = (int)(Random.Range(0, POP_SIZE));
            if(fitness[r] > gen_max)
            {
                 gen_max = fitness[r];
                 maxIn = r;
            }
        }
        return maxIn;
    }

    /*ランキング選択*/
    int rankingSelect() {
        float[] rank = new float[POP_SIZE];//{1位の確率,2位の確率,...}
        float rank_max = 0;
        int selected = 0;//何位のやつが選ばれたか
        for (int i = 0; i < POP_SIZE; i++)
        {
            rank[i] = Mathf.Pow((float)(POP_SIZE-i),4.0f);
            rank_max += rank[i];//ランキング確率の合計値を求めてます
        }

        /*ランキングをもとにルーレットで選択するプログラム→得られたやつがselected（第2位が選ばれましたとか）*/
        double r = Random.Range(0, 1);
        float sum = 0;

        for (selected = 0; selected < POP_SIZE; selected++)
        {
            sum += rank[selected];
            if ((double)sum / (double)rank_max > r)
            {
                break;
            }
        }

        return eliteIndex[selected];//エリート順に並べたときのchromのインデックス
    }

    /*エリート選択*/
    int eliteSelect() {
        int MAX_ELITE_NUM = 10;
        return eliteIndex[Random.Range(0,MAX_ELITE_NUM)];
    }
}
