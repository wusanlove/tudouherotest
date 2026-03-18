using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Lesson2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        #region 知识点一 代码中的文件操作是做什么
        //在电脑上我们可以在操作系统中创建删除修改文件
        //可以增删查改各种各样的文件类型

        //代码中的文件操作就是通过代码来做这些事情
        #endregion

        #region 知识点二 文件相关操作公共类
        //C#提供了一个名为File（文件）的公共类 
        //让我们可以快捷的通过代码操作文件相关
        //类名：File
        //命名空间： System.IO
        #endregion

        #region 知识点三 文件操作File类的常用内容
        //1.判断文件是否存在
        if(File.Exists(Application.dataPath + "/UnityTeach.tang"))
        {
            print("文件存在");
        }
        else
        {
            print("文件不存在");
        }

        //2.创建文件
        //FileStream fs = File.Create(Application.dataPath + "/UnityTeach.tang");

        //3.写入文件
        //将指定字节数组 写入到指定路径的文件中
        byte[] bytes = BitConverter.GetBytes(999);
        File.WriteAllBytes(Application.dataPath + "/UnityTeach.tang", bytes);
        //将指定的string数组内容 一行行写入到指定路径中
        string[] strs = new string[] { "123", "唐老狮", "123123kdjfsalk", "123123123125243"};
        File.WriteAllLines(Application.dataPath + "/UnityTeach2.tang", strs);
        //将指定字符串写入指定路径
        File.WriteAllText(Application.dataPath + "/UnityTeach3.tang", "唐老狮哈\n哈哈哈哈123123131231241234123");

        //4.读取文件
        //读取字节数据
        bytes = File.ReadAllBytes(Application.dataPath + "/UnityTeach.tang");
        print(BitConverter.ToInt32(bytes, 0));

        //读取所有行信息
        strs = File.ReadAllLines(Application.dataPath + "/UnityTeach2.tang");
        for (int i = 0; i < strs.Length; i++)
        {
            print(strs[i]);
        }

        //读取所有文本信息
        print(File.ReadAllText(Application.dataPath + "/UnityTeach3.tang"));

        //5.删除文件
        //注意 如果删除打开着的文件 会报错
        File.Delete(Application.dataPath + "/UnityTeach.tang");

        //6.复制文件
        //参数一：现有文件 需要是流关闭状态
        //参数二：目标文件
        File.Copy(Application.dataPath + "/UnityTeach2.tang", Application.dataPath + "/唐老狮.tanglaoshi", true);

        //7.文件替换
        //参数一：用来替换的路径
        //参数二：被替换的路径
        //参数三：备份路径
        File.Replace(Application.dataPath + "/UnityTeach3.tang", Application.dataPath + "/唐老狮.tanglaoshi", Application.dataPath + "/唐老狮备份.tanglaoshi");

        //8.以流的形式 打开文件并写入或读取
        //参数一：路径
        //参数二：打开模式
        //参数三：访问模式
        //FileStream fs = File.Open(Application.dataPath + "/UnityTeach2.tang", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        
        #endregion

        #region 总结
        //File类提供了各种方法帮助我们进行文件的基础操作，需要记住这些关键API

        //一般情况下想要整体读写内容 可以使用File提供的Write和Read相关功能
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
