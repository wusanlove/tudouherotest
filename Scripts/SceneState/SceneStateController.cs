namespace SceneState
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine.SceneManagement;
    using UnityEngine;

    public class SceneStateController
    {
    
        //总结下状态模式的应用思路：  接口/抽象类（ISceneState）  具体状态类（MainMenuState、BattleState）  状态控制器（SceneStateController）  场景切换（SceneManager.LoadSceneAsync）
        //场景切换完成回调（AsyncOperation.isDone）  场景切换过程中状态更新（StateUpdate）
        //状态模式的核心就是状态控制器，状态控制器负责管理所有的状态类，并且在每一帧调用当前状态的StateUpdate方法，在切换状态的时候调用当前状态的StateEnd方法和新状态的StateStart方法
    
    
        private ISceneState mState;
        private AsyncOperation mAO;
        private bool mIsRunStart = false;  //是否已经调用了StateStart方法

        public void SetState(ISceneState state,bool isLoadScene=true)
        {
            if (mState != null)
            {
                mState.StateEnd();//让上一个场景状态做一下清理工作
            }
            mState = state;
            if (isLoadScene)//如果需要加载场景，就先加载场景，等场景加载完成后再调用StateStart方法，如果不需要加载场景，就直接调用StateStart方法
            {
                mAO = SceneManager.LoadSceneAsync(mState.SceneName);
                mIsRunStart = false;
            } else
            {
                mState.StateStart();
                mIsRunStart = true;
            }
        
        }

        public void StateUpdate()
        {
            if (mAO != null && mAO.isDone == false) return;

            if (mIsRunStart==false&& mAO != null && mAO.isDone == true)
            {
                mState.StateStart();
                mIsRunStart = true;
            }

            if (mState != null)
            {
                mState.StateUpdate();
            }
        }
    }

}