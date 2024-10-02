﻿using MF.CostumeFramework.Reloaded.Hooks;
using MF.Toolkit.Interfaces.Library;
using MF.Toolkit.Interfaces.Messages;

namespace MF.CostumeFramework.Reloaded.Costumes;

internal class CostumeService
{
    private readonly AssetHooks assetHooks;
    private readonly CostumeTblHooks costumeTblHooks;

    public CostumeService(IMetaphorLibrary mf, IMessage msg, CostumeRegistry registry)
    {
        this.assetHooks = new AssetHooks(mf, registry);
        this.costumeTblHooks = new CostumeTblHooks(msg, registry);
    }
}