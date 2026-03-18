using System.Collections.Generic;

public class LevelData
{
    public int id; //关卡id
    public int waveTimer; //当前关卡的时间 
    public List<WaveData> enemys; //生成敌人信息
}

public class WaveData
{
    public string enemyName; //敌人名称
    public int timeAxis; //时间轴
    public int count; //生成数量
    public int elite; //是否为精英 1为精英
}