/////////////////////////////////////////////////////////////////
///進化計算論「遺伝的アルゴリズムで下駄占いを学習させよう」
///1910677　山崎萌恵（リーダー）
///1910481　永井 美央花			
///1910563 福岡 美結
///
/// GAProgram.cs
/////////////////////////////////////////////////////////////////

/*-----------------------------------------------------------------*/
//GAProgram.cs内のプログラムはすべて山崎が担当しました。
/*-----------------------------------------------------------------*/
//【実行時のお願い】
//Application.persistentDataPathで表示されるフォルダにinput.txtという初期データファイルを用意するか
//Start関数のInitialize1をInitialize2に変更して実行してください
/*-----------------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public partial class GAProgram : MonoBehaviour
{
    /*-----------GAに関するパラメータの設定------------*/
    int MAX_GEN = 300;     /*最大世代交替*/
    const int POP_SIZE = 200;/*集団のサイズ*/
    const int PARAMETER = 4;/*パラメータの個数*/
    const int NUM_MOVE = 5; /*動きの数*/
    static int LEN_CHROM = NUM_MOVE * PARAMETER;/*遺伝子の長さ*/
    double GEN_GAP = 0.2;   /*世代交代の割合*/
    double P_MUTAION = 0.1; /*突然変異の確率*/

    /*-----------必要となる定数------------*/
    int RANDOM_MAX = 32767; //ランダム用
    const int BEFORE = 0;   //データ表示用
    const int AFTER = 1;    //データ表示用
    //float genDuration = 8.0f; //制限時間(遷移時間一定の時)
    float genDuration;          //制限時間(遷移時間可変の時)
    protected float genDurationLeft; //残り時間（秒）
    float[,,] chrom = new float[POP_SIZE, NUM_MOVE, PARAMETER];//現在の世代
    float[,,] new_chrom = new float[POP_SIZE, NUM_MOVE, PARAMETER];//次の世代
    float[] fitness = new float[POP_SIZE];  //適応度
    int[] isSunny = new int[POP_SIZE];      //晴れかどうか
    float[] crog_Pos = new float[POP_SIZE]; //距離
    float max;  float min;//最大値/最小値探索用
    float sumfitness;     //適応度の合計値
    int n_min;  int n_max;//最大値/最小値探索用

    /*-----------ファイル出力関係------------*/
    protected int gen = 1;  //現在の世代
    string path;            //交叉や突然変異も含めた実行結果の出力
    string path_result;     //世代ごとの配列を出力
    string path_ave;        //適応度の平均を出力
    string path_weather;    //晴れ（雨、曇り）の確率を出力
    string path_crogPos;    //下駄の距離を出力
    string path_max;        //適応度の最大値を出力
    public string path_InitData = "/input.txt";//読み込み時のファイル
    const int print_gen = 10;//何世代ごとに結果の配列をファイル出力するかどうか
    const int repeat = 3;    //同じ動きを繰り返す回数

    /*ファイル関係の初期化*/
    void InitFiles() {
        //フォルダの生成
        Debug.Log("実行結果はここにあります\n" + Application.persistentDataPath);
        Directory.CreateDirectory(Application.persistentDataPath + "/GAResults");
        //ファイルの書き込み
        path = Application.persistentDataPath + "/test.txt";
        File.WriteAllText(path, "");            //test.txtを白紙に戻す
        path_ave = Application.persistentDataPath + "/AverageData.csv";
        File.WriteAllText(path_ave, "0,0\n");   //AverageData.csv"を白紙に戻す
        path_weather = Application.persistentDataPath + "/WeatherData.csv";
        File.WriteAllText(path_weather, "0,0\n");//WeatherData.csvを白紙に戻す
        path_crogPos = Application.persistentDataPath + "/CrogPosData.csv";
        File.WriteAllText(path_crogPos, "0,0\n");//CrogPosData.csvを白紙に戻す
        path_max = Application.persistentDataPath + "/MaxData.csv";
        File.WriteAllText(path_max, "0,0\n");//MaxData.csvを白紙に戻す
        path_result = Application.persistentDataPath + "/GAResults/gen0.txt";
        File.WriteAllText(path_result, "");      //test+gen+.txtを白紙に戻す
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("GAProgram Start!");
        Debug.Log(MAX_GEN+"世代実行します");

        /*---------ファイル関連の初期化----------*/
        InitFiles();

        /*---------初期データの設定----------*/
        Initialize1();

        genDuration = maxCulcDuration()+4.0f; //制限時間（遷移時間の合計＋下駄が落ちるまでの時間）
        genDurationLeft = genDuration; //残り時間


        /*---------アニメーション関連----------*/
        for (int i = 0; i < POP_SIZE; i++)
        {
            curFrame[i] = 1000; //初期化(遷移時間より大きい値に設定)
            curMove[i] = -1; //動きの番号の初期化
        }

        /*----------下駄の状態--------------*/
        InitCrogs();
    }

    /*疑似乱数*/
    static ulong next = 1;

    uint Rand() {
        next = next * 1103515245 + 12345;
        return (uint)(next/65536)%32768;
    }

    /*初期データ設定（ファイルから読み込み）*/
    void Initialize1()
    {
        //ファイルの読み込み
        string path2 = Application.persistentDataPath + path_InitData;
        string data = File.ReadAllText(path2);
        string[] s1 = data.Split(':');
        int i, j, k;

        for (i = 0; i < POP_SIZE; i++)
        {
            for (j = 0; j < NUM_MOVE; j++)
            {
                string[] s2 = s1[i * NUM_MOVE + j].Split(',');
                for (k = 0; k < PARAMETER; k++)
                {
                    //数値を読み込み
                    chrom[i, j, k] = float.Parse(s2[k]);
                    //gen0.txtに書き込み
                    File.AppendAllText(path_result, chrom[i, j, k] + "");
                    if (k != PARAMETER - 1) { File.AppendAllText(path_result, ","); }
                }
                //gen0.txtに書き込み
                if (i != POP_SIZE - 1 || j != NUM_MOVE - 1) { File.AppendAllText(path_result, ":"); }
            }
            fitness[i] = 1;//fitnessの初期化
        }
    }

    /*初期データ設定（ランダム生成）*/
    void Initialize2()
    {
        int i, j, k;

        for (i = 0; i < POP_SIZE; i++){
            for (j = 0; j < NUM_MOVE; j++){
                for (k = 0; k < PARAMETER; k++){
                    //Range[k,0]は最小値、Range[k,0]は最大値
                    chrom[i, j, k] = Random.Range(Range[k, 0], Range[k, 1]);
                    //gen0.txtに書き込み
                    File.AppendAllText(path_result, chrom[i, j, k] + "");
                    if (k != PARAMETER - 1) { File.AppendAllText(path_result, ","); }
                }
                if (i != POP_SIZE - 1 || j != NUM_MOVE - 1) { File.AppendAllText(path_result, ":"); }
            }
            fitness[i] = 1;//fitnessの初期化
        }
    }

    /*データ表示関数*/
    void PrintEachChromFitness(int i) {
        int j, k;
        File.AppendAllText(path, "["+i+"]");
        for (j = 0; j < NUM_MOVE; j++)
        {
            for (k = 0; k < PARAMETER; k++){
                File.AppendAllText(path, "," + chrom[i, j, k]);
            }
            File.AppendAllText(path, "\n   ");
        }
        File.AppendAllText(path, ":"+fitness[i]+"\n");
    }

    void PrintEachChromFitnessChild(int i)
    {
        int j, k;
        File.AppendAllText(path, "[" + i + "]");
        for (j = 0; j < NUM_MOVE; j++)
        {
            for (k = 0; k < PARAMETER; k++)
            {
                File.AppendAllText(path, "," + new_chrom[i, j, k]);
            }
            File.AppendAllText(path, "\n   ");
        }
        File.AppendAllText(path, ":" + fitness[i] + "\n");
    }

    void PrintChromFitness() {
        int i;
        for (i = 0; i < POP_SIZE; i++) { PrintEachChromFitness(i); }
    }

    void PrintStatistics(int gen) {
        File.AppendAllText(path, "[gen=" + gen + "] max=" + max + " min=" + min + " sumfitness=" + sumfitness + " ave=" + (double)sumfitness / (double)POP_SIZE+"\n");
        File.AppendAllText(path_ave, gen + " ," + (double)sumfitness / (double)(POP_SIZE) + "\n");
        int countWeather = 0;//晴れを出した下駄の合計値
        float SumCrogPos = 0;//下駄の距離の合計値
        for (int i = 0; i < POP_SIZE; i++) { countWeather+=isSunny[i]; SumCrogPos += crog_Pos[i]; }
        //ファイルに晴れの正答率、距離の平均、適応度の最大値を出力
        File.AppendAllText(path_weather, gen + " ," + (double)countWeather / (double)(POP_SIZE*repeat) + "\n");
        File.AppendAllText(path_crogPos, gen + " ," + SumCrogPos/(float)POP_SIZE + "\n");
        File.AppendAllText(path_max, gen + " ," + fitness.Max() + "\n");
        
        //print_gen世代ごとに結果を保存
        if (gen % print_gen == 0){
            int i, j, k;
            //ファイルのパスを指定して書き込み
            path_result = Application.persistentDataPath + "/GAResults/gen" + gen + ".txt";
            File.WriteAllText(path_result, "");//test+gen+.txtを白紙に戻す
            for (i = 0; i < POP_SIZE; i++)
            {
                for (j = 0; j < NUM_MOVE; j++)
                {
                    for (k = 0; k < PARAMETER; k++)
                    {
                        File.AppendAllText(path_result, chrom[i, j, k] + "");
                        if (k != PARAMETER - 1) { File.AppendAllText(path_result, ","); }
                    }
                    File.AppendAllText(path_result, ":");
                }
            }
        }
    }

    /*データ表示関数（その2）*/
    void PrintCrossover(int flag, int parent1, int parent2, int child1, int child2, int n_cross)
    {
        switch (flag)
        {
            case BEFORE:
                File.AppendAllText(path, "parent1 |"); PrintEachChromFitness(parent1);
                File.AppendAllText(path, "parent2 |"); PrintEachChromFitness(parent2);
                File.AppendAllText(path, "delete1 |"); PrintEachChromFitness(child1);
                File.AppendAllText(path, "delete2 |"); PrintEachChromFitness(child2);
                File.AppendAllText(path, "n_cross = " + n_cross + "\n");
                break;
            case AFTER:
                File.AppendAllText(path, "child1 |"); PrintEachChromFitnessChild(child1);
                File.AppendAllText(path, "child2 |"); PrintEachChromFitnessChild(child2);
                File.AppendAllText(path, "----------------------\n");
                break;
        }
    }

    /*交叉*/
    /*一点交叉*/
    unsafe void Crossover(int parent1, int parent2, int child1, int child2)
    {
        int n_cross;
        int i, j, k;

        /*交叉位置*/
        n_cross = (int)(Rand() % (NUM_MOVE - 1) + 1);
        k = 0;
        /*交叉*/
        PrintCrossover(BEFORE, parent1, parent2, child1, child2, n_cross);
        for (j = 0; j < n_cross; j++)
        {
            for (k = 0; k < PARAMETER; k++)
            {
                new_chrom[child1, j, k] = chrom[parent1, j, k];
                new_chrom[child2, j, k] = chrom[parent2, j, k];
            }
        }
        for (j = n_cross; j < NUM_MOVE; j++)
        {
            for (k = 0; k < PARAMETER; k++)
            {
                new_chrom[child1, j, k] = chrom[parent2, j, k];
                new_chrom[child2, j, k] = chrom[parent1, j, k];
            }
        }
        PrintCrossover(AFTER, parent1, parent2, child1, child2, n_cross);
    }

    /*一様交叉（動き）*/
    unsafe void UniformCrossover2(int parent1, int parent2, int child1, int child2)
    {
        int i, j, k;
        int[] mask = new int[NUM_MOVE];//動きの数だけマスクを生成

        /*マスクの作成*/
        for (i = 0; i < NUM_MOVE; i++)
        {
            mask[i] = (int)(Rand() % 2);
        }
        /*交叉*/
        for (j = 0; j < NUM_MOVE; j++)
        {
            if (mask[j] == 0)
            {
                for (k = 0; k < PARAMETER; k++)
                {
                    new_chrom[child1, j, k] = chrom[parent1, j, k];
                    new_chrom[child2, j, k] = chrom[parent2, j, k];
                }
            }
            else
            {
                for (k = 0; k < PARAMETER; k++)
                {
                    new_chrom[child1, j, k] = chrom[parent2, j, k];
                    new_chrom[child2, j, k] = chrom[parent1, j, k];
                }
            }
        }
    }

    /*一様交叉（すべてのパラメータ）*/
    unsafe void UniformCrossoverAll(int parent1, int parent2, int child1, int child2)
    {
        int i, j, k;
        int[] mask = new int[LEN_CHROM];//（動き×パラメータ）の数だけマスクを生成
        
        /*マスクの作成*/
        for (i = 0; i < LEN_CHROM; i++)
        {
            mask[i] = (int)(Rand() % 2);
        }
        /*交叉*/
        for (j = 0; j < LEN_CHROM; j++)
        {
            if (mask[j] == 0)
            {
                new_chrom[child1, (int)j / PARAMETER, (int)j % PARAMETER] = chrom[parent1, (int)j / PARAMETER, (int)j % PARAMETER];
                new_chrom[child2, (int)j / PARAMETER, (int)j % PARAMETER] = chrom[parent2, (int)j / PARAMETER, (int)j % PARAMETER];
            }
            else
            {
                new_chrom[child1, (int)j / PARAMETER, (int)j % PARAMETER] = chrom[parent2, (int)j / PARAMETER, (int)j % PARAMETER];
                new_chrom[child2, (int)j / PARAMETER, (int)j % PARAMETER] = chrom[parent1, (int)j / PARAMETER, (int)j % PARAMETER];
            }

        }
    }

    /*一様交叉（つま先と時間だけ交叉、つま先と膝と時間を交叉、すべて交叉の3パターン）*/
    unsafe void UniformCrossoverAllPattern(int parent1, int parent2, int child1, int child2)
    {
        int i, j, k;
        int[] mask = new int[NUM_MOVE];//動きの数だけマスクを生成

        /*マスクの作成*/
        for (i = 0; i < NUM_MOVE; i++)
        {
            mask[i] = (int)(Rand() % 4);//0～3でランダム
        }

        /*交叉*/
        for (j = 0; j < NUM_MOVE; j++)
        {
            if (mask[j] == 0)//交叉させない
            {
                for (k = 0; k < PARAMETER; k++)
                {
                    new_chrom[child1, j, k] = chrom[parent1, j, k];
                    new_chrom[child2, j, k] = chrom[parent2, j, k];
                }
            }
            else if (mask[j] == 1)//つま先と時間のみ交叉
            {
                new_chrom[child1, j, 0] = chrom[parent1, j, 0];
                new_chrom[child2, j, 0] = chrom[parent2, j, 0];

                new_chrom[child1, j, 1] = chrom[parent1, j, 1];
                new_chrom[child2, j, 1] = chrom[parent2, j, 1];

                new_chrom[child1, j, 2] = chrom[parent2, j, 2];
                new_chrom[child2, j, 2] = chrom[parent1, j, 2];

                new_chrom[child1, j, 3] = chrom[parent2, j, 3];
                new_chrom[child2, j, 3] = chrom[parent1, j, 3];
            }
            else if (mask[j] == 2)//つま先と膝と時間のみ交叉
            {
                new_chrom[child1, j, 0] = chrom[parent1, j, 0];
                new_chrom[child2, j, 0] = chrom[parent2, j, 0];

                new_chrom[child1, j, 1] = chrom[parent2, j, 1];
                new_chrom[child2, j, 1] = chrom[parent1, j, 1];

                new_chrom[child1, j, 2] = chrom[parent2, j, 2];
                new_chrom[child2, j, 2] = chrom[parent1, j, 2];

                new_chrom[child1, j, 3] = chrom[parent2, j, 3];
                new_chrom[child2, j, 3] = chrom[parent1, j, 3];
            }
            else if (mask[j] == 3)//すべて交叉
            {
                for (k = 0; k < PARAMETER; k++)
                {
                    new_chrom[child1, j, k] = chrom[parent2, j, k];
                    new_chrom[child2, j, k] = chrom[parent1, j, k];
                }
            }
        }
    }

    int counter = 0;//同じ動きが何回繰り返されたかを数える
    void FixedUpdate()
    {
        /*制限時間を減らす*/
        genDurationLeft -= Time.deltaTime;

        /*下駄のRayCast*/
        for (int i = 0; i < POP_SIZE; i++)
        {
            crogsState[i].RayCasting();
        }

        /*アニメーション*/
        AllrotateObjects();

        /*制限時間を過ぎたらGAスタート*/
        if (genDurationLeft < 0) {
            if (gen <= MAX_GEN)
            {
                if(counter == 0) { ScoreArray1(); }//一番はじめは初期化用のスコア計算を行う
                else {
                    ScoreArray(); //スコアをfitnessに足し合わせる
                }
                /*足と下駄を初期位置に戻す*/
                for (int i = 0; i < POP_SIZE; i++)
                {
                    InitRotateRB(LegParent.transform.GetChild(i + 1));
                    crogsState[i].InitCrogsPos();
                    initState[i] = true;
                    curFrame[i] = -200;
                    curMove[i] = -1;
                }
                /*repeat回アニメーションが繰り返されたら*/
                if (counter == repeat-1)
                {
                    counter = -1;
                    printScore();
                    Debug.Log(gen + "世代：開始");
                    Generation(gen);//GAを行う
                    Debug.Log(gen + "世代：終了");
                    gen++;
                }
                genDuration = maxCulcDuration() + 4.0f;//制限時間（遷移時間の合計＋下駄が落ちるまでの時間）
                genDurationLeft = genDuration;
                counter++;

            }
            else {//最大世代数まで到達したら止める
                Debug.Log("end!");
                UnityEditor.EditorApplication.isPlaying = false;
            }
        }
    }
}
