﻿using MF.CostumeFramework.Reloaded.Common;
using MF.CostumeFramework.Reloaded.Configuration;
using MF.CostumeFramework.Reloaded.Hooks;
using MF.Toolkit.Interfaces.Inventory;
using MF.Toolkit.Interfaces.Library;
using MF.Toolkit.Interfaces.Messages;

namespace MF.CostumeFramework.Reloaded.Costumes;

internal class CostumeService : IUseConfig
{
    private readonly AssetHooks assetHooks;
    private readonly CostumeTblHooks costumeTblHooks;

    public CostumeService(IMetaphorLibrary mf, IMessage msg, IInventory inv, CostumeRegistry registry)
    {
        this.assetHooks = new AssetHooks(mf, registry);
        this.costumeTblHooks = new CostumeTblHooks(msg, inv, registry);
    }

    public void UpdateConfig(Config config)
    {
        assetHooks.UpdateConfig(config);
    }
}
