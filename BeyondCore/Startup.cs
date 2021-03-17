using System;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;

namespace BeyondCore
{
    public class Startup : AsyncResource
    {
        public override void OnStart() {
            var core = new Core();
            Alt.Export("AddMoneyToBank", new Action<IPlayer, int>(Core.Db.AddMoneyToBank));
            Alt.Export("AddMoneyToHand", new Action<IPlayer, int>(Core.Db.AddMoneyToHand));
            Alt.Export("AddBlackMoney", new Action<IPlayer, int>(Core.Db.AddBlackMoney));
            Alt.Export("GetMoneyOnBank", new Func<IPlayer, int>(Core.Db.GetMoneyOnBank));
            Alt.Export("GetMoneyOnHand", new Func<IPlayer, int>(Core.Db.GetMoneyOnHand));
            Alt.Export("GetBlackMoney", new Func<IPlayer, int>(Core.Db.GetBlackMoney));
            Alt.Export("RemoveMoneyFromBank", new Action<IPlayer, int>(Core.Db.RemoveMoneyFromBank));
            Alt.Export("RemoveMoneyFromHand", new Action<IPlayer, int>(Core.Db.RemoveMoneyFromHand));
            Alt.Export("RemoveBlackMoney", new Action<IPlayer, int>(Core.Db.RemoveBlackMoney));
            Alt.Export("GetJobIdOfPlayer", new Func<IPlayer, int>(Core.Db.GetJobIdOfPlayer));
            Alt.Export("GetJobSalary", new Func<int, int>(Core.Db.GetJobSalary));
            Alt.Export("GetJobName", new Func<int, string>(Core.Db.GetJobName));
            Alt.Log("BeyondCore enabled.");
        }

        public override void OnStop() {
            Alt.Log("BeyondCore disabled.");
        }
    }
}