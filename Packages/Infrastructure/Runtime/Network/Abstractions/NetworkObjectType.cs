using System.Collections;
using System.Collections.Generic;

namespace Love.Network
{
    /// <summary>
    /// 网络对象类型
    /// </summary>
    public enum NetworkObjectType
    {
        Player = 10,  //玩家控制
        AI = 5,       //AI 控制
        Passive = 2,  //被动交互
    }

}