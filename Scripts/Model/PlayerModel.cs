using Unity.VisualScripting;

public class PlayerModel:BaseMgr<PlayerModel>
{
    public int hp;
    public int attack;
    public int defense;
    public int speed;
    public int harvest = 0; //收获
    public int slot = 6; //操作
    public float shopDiscount = 1; //道具价格 百分比
    public float expMuti = 1; //经验倍率 百分比
    public float pickRange = 1; //拾取范围 百分比
    public float critical_strikes_probability = 0; //附加暴击率 百分比
    void start()
    {
        Init();
    }

    private void Init()
    {
        // 这里可以从配置文件或者其他数据源加载玩家的初始属性
        
    }
}