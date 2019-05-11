using System;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems.D20
{
    public class D20System : IGameSystem, IResetAwareSystem, ITimeAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public const bool IsEditor = false;

        public D20ActionSystem Actions { get; private set; }

        public D20ObjectRegistry ObjectRegistry { get; private set; }

        public BonusSystem BonusSystem { get; }

        public ConditionRegistry Conditions { get; }

        public D20StatusSystem StatusSystem { get; }

        public D20Initiative Initiative { get; private set; }

        public RadialMenuSystem RadialMenu { get; private set; }

        [TempleDllLocation(0x1004c8a0)]
        public D20System()
        {
            Conditions = new ConditionRegistry();
            BonusSystem = new BonusSystem();
            StatusSystem = new D20StatusSystem();
            // TODO

            ObjectRegistry = new D20ObjectRegistry();
            Actions = new D20ActionSystem();
            Initiative = new D20Initiative();

            RadialMenu = new RadialMenuSystem();
        }

        [TempleDllLocation(0x1004C950)]
        public void Dispose()
        {
            ObjectRegistry?.Dispose();
            ObjectRegistry = null;

            Actions?.Dispose();
            Actions = null;

            Initiative?.Dispose();
            Initiative = null;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void AdvanceTime(TimePoint time)
        {
            // TODO
        }

        public int D20QueryPython(GameObjectBody obj, string type)
        {
            throw new NotImplementedException();
        }

        public int D20QueryPython(GameObjectBody obj, string type, object arg)
        {
            throw new NotImplementedException();
        }

        public int D20Query(GameObjectBody obj, D20DispatcherKey queryKey)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            var dispIO = DispIoD20Query.Default;
            dispatcher.Process(DispatcherType.D20Query, queryKey, dispIO);
            return dispIO.return_val;
        }

        [TempleDllLocation(0x1004e6b0)]
        public void D20SendSignal(GameObjectBody obj, D20DispatcherKey key, GameObjectBody arg)
        {
            if (obj == null)
            {
                Logger.Warn("D20SendSignal called with null handle! Key was {0}", key);
                return;
            }

            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                Logger.Info("d20SendSignal(): Object {0} lacks a Dispatcher", obj);
                return;
            }

            DispIoD20Signal dispIO = DispIoD20Signal.Default;
            dispIO.obj = arg;
            dispatcher.Process(DispatcherType.D20Signal, key, dispIO);
        }

        [TempleDllLocation(0x1004e6b0)]
        public void D20SendSignal(GameObjectBody obj, D20DispatcherKey key, int arg1 = 0, int arg2 = 0)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                Logger.Info("d20SendSignal(): Object {0} lacks a Dispatcher", obj);
                return;
            }

            var dispIO = DispIoD20Signal.Default;
            dispIO.data1 = arg1;
            dispIO.data2 = arg2;
            dispatcher.Process(DispatcherType.D20Signal, key, dispIO);
        }

        [TempleDllLocation(0x1004fee0)]
        public void RemoveDispatcher(GameObjectBody obj)
        {
            var dispatcher = obj.GetDispatcher() as Dispatcher;

            dispatcher?.ClearAll();
        }

        public bool CritterHasCondition(GameObjectBody obj, string conditionSpec, out int spellIdx)
        {
            return CritterHasCondition(obj, Conditions[conditionSpec], out spellIdx);
        }

        public bool CritterHasCondition(GameObjectBody obj, ConditionSpec conditionSpec, out int spellIdx)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                spellIdx = -1;
                return false;
            }

            var dispIO = DispIoD20Query.Default;
            dispIO.condition = conditionSpec;
            dispatcher.Process(DispatcherType.D20Query, D20DispatcherKey.QUE_Critter_Has_Condition, dispIO);

            spellIdx = (int) dispIO.data2; // TODO: This is most likely wrong. check this again.
            return dispIO.return_val != 0;
        }

        [TempleDllLocation(0x1004e620)]
        public int GetWeaponGlowType(GameObjectBody wielder, GameObjectBody item)
        {
            var dispIo = DispIoD20Query.Default;
            dispIo.obj = item;

            if (wielder != null)
            {
                var dispatcher = wielder.GetDispatcher();
                if (dispatcher == null)
                {
                    return 0;
                }

                dispatcher.Process(DispatcherType.WeaponGlowType, 0, dispIo);
                return dispIo.return_val;
            }

            DispatchForItem(item, DispatcherType.WeaponGlowType, 0, dispIo);
            return dispIo.return_val;
        }

        private void DispatchForItem(GameObjectBody item, DispatcherType dispType, D20DispatcherKey dispKey,
            object dispIo)
        {
            var condArray = item.GetInt32Array(obj_f.item_pad_wielder_condition_array);
            var argArrayCount = 0; // there's only one argument list for all attached conditions

            for (var i = 0; i < condArray.Count; i++)
            {
                var condNameHash = condArray[i];

                var condition = Conditions.GetByHash(condNameHash);
                if (condition != null)
                {
                    Span<int> condArgsIn = stackalloc int[condition.numArgs];
                    foreach (ref var arg in condArgsIn)
                    {
                        arg = item.GetInt32(obj_f.item_pad_wielder_argument_array, argArrayCount++);
                    }

                    condArgsIn[2] = -1; // ... why?
                    ItemDispatcher.DispatcherProcessorForItems(condition, condArgsIn, dispType, dispKey, dispIo);
                }
            }
        }

        [TempleDllLocation(0x10092A50)]
        public void turnBasedReset()
        {
            // TODO
        }

        public void ResetRadialMenus()
        {
            // TODO
        }

        [TempleDllLocation(0x100e5080)]
        public void SetCritterStrategy(GameObjectBody obj, string strategy)
        {
            // TODO
        }

        [TempleDllLocation(0x1004cd40)]
        public GameObjectBody D20QueryReturnObject(GameObjectBody obj, D20DispatcherKey queryKey,
            int arg1 = 0, int arg2 = 0)
        {
            Trace.Assert(queryKey == D20DispatcherKey.QUE_Critter_Is_Charmed
                         || queryKey == D20DispatcherKey.QUE_Critter_Is_Afraid
                         || queryKey == D20DispatcherKey.QUE_Critter_Is_Held);

            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                return null;
            }

            var dispIO = new DispIoD20Query();
            dispIO.return_val = 0;
            dispIO.data1 = arg1;
            dispIO.data2 = arg2;
            dispatcher.Process(DispatcherType.D20Query, queryKey, dispIO);

            return dispIO.obj;
        }

        [TempleDllLocation(0x11E61538)]
        private TimePoint _combatEndTime;

        [TempleDllLocation(0x100decb0)]
        public void EndTurnBasedCombat()
        {
            Initiative.Reset();
            _combatEndTime = GameSystems.TimeEvent.GameTime;

            if (GameUiBridge.IsTutorialActive()){
                if (GameSystems.Script.GetGlobalFlag(4)){
                    GameSystems.Script.SetGlobalFlag(4, false);
                    GameSystems.Script.SetGlobalFlag(2, true);
                    GameUiBridge.ShowTutorialTopic(18);
                }
            }
        }


    }
}