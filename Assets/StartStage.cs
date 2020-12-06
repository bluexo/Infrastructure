using Origine;
using Origine.Fsm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartStage : StageBase
{
    public override void Init(IFsm<IStageManager> owner)
    {
        base.Init(owner);
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
    }

    public override void Leave(bool isShutdown)
    {
        base.Leave(isShutdown);
    }
}
