/////////////////////////////////////////////////////////////////
///進化計算論「遺伝的アルゴリズムで下駄占いを学習させよう」
///1910677　山崎萌恵（リーダー）
///1910481　永井 美央花			
///1910563 福岡 美結
///
/// GAProgram2.cs
/////////////////////////////////////////////////////////////////

/*-----------------------------------------------------------------*/
//GAProgram2.cs内のプログラムはすべて永井が担当しました。
/*-----------------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public partial class GAProgram
{

    /*データ表示関数（その3）*/
    void PrintMutation(int flag, int child, int n_mutateM, int n_mutateP)
    {
        switch (flag)
        {
            case BEFORE:
                File.AppendAllText(path, "child(OLD)|");PrintEachChromFitness(child);
                File.AppendAllText(path, "MOVE=" + n_mutateM + "PARAMETER=" + n_mutateP + "\n");
                break;
            case AFTER:
                File.AppendAllText(path, "child(NEW)|"); PrintEachChromFitness(child);
                File.AppendAllText(path, "----------------------\n");
                break;
        }
    }

    // 世代交代（担当：永井）
    /*1世代の処理*/
    unsafe void Generation(int gen)
    {
        int parent1, parent2;
        int child1, child2;
        int n_gen;

        /*集団の表示*/
        Statistics();           // sumfitness, max, min, n_minの更新
        PrintStatistics(gen);   // txtファイルに世代情報を出力

        // new_chromにchromをfitnessが高い順に入れ替えてコピー
        SortAndCopy();

        /*世代交代*/
        child1 = POP_SIZE - 1; child2 = POP_SIZE - 2; // 集団の添え字の末尾二つから始める
        n_gen = (int)((double)POP_SIZE * GEN_GAP / 2.0); // n_gen: 世代交代を行う回数
        for (int i=0; i<n_gen; i++)
        {
            // 選択
            parent1 = parent2 = Select();
            //parent1 = parent2 = tournamentSelect();
            //parent1 = parent2 = rankingSelect();
            //parent1 = parent2 = eliteSelect();
            while (parent1 == parent2) parent2 = Select(); // 単為生殖しないよう選択しなおす
            //while (parent1 == parent2) parent2 = tournamentSelect(); // 単為生殖しないよう選択しなおす
            //while (parent1 == parent2) parent2 = rankingSelect(); // 単為生殖しないよう選択しなおす
            //while (parent1 == parent2) parent2 = eliteSelect(); // 単為生殖しないよう選択しなおす

            // 交差
            //Crossover(parent1, parent2, child1, child2); // 一点交差
            //UniformCrossoverAll(parent1, parent2, child1, child2); // 一様交差
            //UniformCrossover2(parent1, parent2, child1, child2); // 一様交差（動き）
            UniformCrossoverAllPattern(parent1, parent2, child1, child2); //部位ごとの一様交叉
            
            
            // 突然変異
            //Mutation(child1);
            //Mutation(child2);
            // 突然変異（動きの前半だけ）
            MutationFirst(child1, 0.5f, 3.0f);
            MutationFirst(child2, 0.5f, 3.0f);


            child1 -= 2; child2 -= 2; // 添え字移動
        }

        // chromを上書き
        for (int i = 0; i < POP_SIZE; i++)
        {
            for (int j = 0; j < NUM_MOVE; j++)
            {
                for (int k = 0; k < PARAMETER; k++)
                {
                    chrom[i, j, k] = new_chrom[i, j, k];
                }
            }
        }
    }

    // 突然変異（担当：永井）
    void Mutation(int child)
    {
        int n_mutateM, n_mutateP;
        double rand;

        // 一定確率で突然変異が起こる
        rand = (double)Rand() / ((double)(RANDOM_MAX + 1));
        if (rand < P_MUTAION) {
            // 突然変異が起きた個体は、1～3箇所をランダムに変更する
            for (int i = 0; i < (int)(Rand() % 3) + 1; i++) {
                /*突然変異位置*/
                n_mutateM = (int)(Rand() % NUM_MOVE);  // どのタイミングか
                n_mutateP = (int)(Rand() % PARAMETER); // どの部位か

                /*突然変異*/
                PrintMutation(BEFORE, child, n_mutateM, n_mutateP);
                new_chrom[child, n_mutateM, n_mutateP] = Random.Range(Range[n_mutateP, 0], Range[n_mutateP, 1]);
                PrintMutation(AFTER, child, n_mutateM, n_mutateP);
            }
        }
    }

    // 最初の方の動きを確率n倍で突然変異させる
    void MutationFirst(int child, float per_until, float times)
    {
        int n_mutateM, n_mutateP;
        double rand;
        int first = (int)Mathf.Round(NUM_MOVE * per_until); // "最初の方"が要素数で言うといくつか

        // 一定確率で突然変異が起こる
        rand = (double)Rand() / ((double)(RANDOM_MAX + 1));
        if (rand < P_MUTAION)
        {
            // 突然変異が起きた個体は、1～3箇所をランダムに変更する
            for (int i = 0; i < (int)(Rand() % 3) + 1; i++)
            {
                /*突然変異位置*/
                // どのタイミングの動きか
                rand = (double)Rand() / ((double)(RANDOM_MAX + 1));
                if(rand < (double)(times / (1+times)))
                {
                    // 前半部分の突然変異
                    n_mutateM = (int)(Rand() % first);
                }
                else
                {
                    // 後半部分の突然変異
                    n_mutateM = (int)((Rand() % (NUM_MOVE-first)) + first);
                }

                n_mutateP = (int)(Rand() % PARAMETER); // どの部位か

                /*突然変異*/
                PrintMutation(BEFORE, child, n_mutateM, n_mutateP);
                new_chrom[child, n_mutateM, n_mutateP] = Random.Range(Range[n_mutateP, 0], Range[n_mutateP, 1]);
                PrintMutation(AFTER, child, n_mutateM, n_mutateP);
            }
        }
    }

    /*chromをfitness降順に並べ替えたものをnew_chromに代入するプログラム（担当：永井）*/
    /*chromは書き換えない*/

    int[] eliteIndex = new int[POP_SIZE];//chromをエリート順に並べたときのindexを保存しておくための配列（by 山崎）
    /*選択ソートの改造*/
    void SortAndCopy()
    {
        bool[] is_copied = new bool[POP_SIZE]; // コピー済みか否かを管理する
        int n_max;
        float fit_max;
        for (int i = 0; i < POP_SIZE; i++)
        {
            // 未コピーかつfitnessが最大になるchromの添え字を探索
            n_max = 0; // fitnessが最大値の時の添え字
            fit_max = float.NegativeInfinity; // 未コピー中のfitnessの最大値（初期値-∞）

            for (int t=0; t<POP_SIZE; t++)
            {
                if(fit_max < fitness[t] && !is_copied[t])
                {
                    fit_max = fitness[t];
                    n_max = t;
                }
            }
            // new_chromに代入
            for (int j = 0; j < NUM_MOVE; j++)
            {
                for (int k = 0; k < PARAMETER; k++)
                {
                    new_chrom[i, j, k] = chrom[n_max, j, k];
                }
            }
            eliteIndex[i] = n_max;//元の配列のインデックスを保存（by 山崎）
            // 最大値を持つchromをコピー済みにして、次回の探索から省く
            is_copied[n_max] = true;
        }
    }

}
