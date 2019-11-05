using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.Script
{
    public class ScriptSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public delegate void InitiateDialog(GameObjectBody obj1, GameObjectBody obj2, int scriptNumber,
            int unk1, int argFromEvent);

        public delegate void ShowMessage(GameObjectBody speaker, GameObjectBody speakingTo, string text, int speechId);

        private const bool IsEditor = false;

        [TempleDllLocation(0x103073B8)]
        private int[] _globalVars = new int[2000];

        [TempleDllLocation(0x103073A8)]
        private uint[] _globalFlags = new uint[100];

        [TempleDllLocation(0x103073A0)]
        private int _currentStoryState = 0;

        [TempleDllLocation(0x103073AC)]
        private InitiateDialog _scriptDialogInitiate = null;

        [TempleDllLocation(0x103073BC)]
        private ShowMessage _scriptShowMessage = null;

        [TempleDllLocation(0x102AC388)]
        private Dictionary<int, string> _storyStateText;

        private readonly ScriptAssembly _scriptAssembly;

        public SpellScriptSystem Spells { get; }

        public ActionScriptSystem Actions { get; }

        [TempleDllLocation(0x10006580)]
        public ScriptSystem()
        {
            // TODO: init python from here
            _scriptAssembly = new ScriptAssembly(Globals.Config.ScriptAssemblyName);

            Spells = new SpellScriptSystem(_scriptAssembly);
            Actions = new ActionScriptSystem();
        }

        [TempleDllLocation(0x10007b60)]
        public void Dispose()
        {
            // TODO: shutdown python here
        }

        [TempleDllLocation(0x10006630)]
        public void LoadModule()
        {
            _storyStateText = Tig.FS.ReadMesFile("mes/storystate.mes");
        }

        [TempleDllLocation(0x10006650)]
        public void UnloadModule()
        {
            _storyStateText.Clear();
        }

        [TempleDllLocation(0x10007ae0)]
        public void Reset()
        {
            Stub.TODO("Clear dialoger picker args"); // TODO Clear dialog picker args PyGame_Exit

            _currentStoryState = 0;
            Array.Fill(_globalVars, 0);
            Array.Fill(_globalFlags, 0u);
        }

        [TempleDllLocation(0x100066e0)]
        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10006670)]
        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        public bool TryGetDialogScript(int scriptId, out IDialogScript dialogScript)
        {
            return _scriptAssembly.TryCreateDialogScript(scriptId, out dialogScript);
        }

        [TempleDllLocation(0x1000bb60)]
        public bool Invoke(ref ObjScriptInvocation invocation)
        {
            if (IsEditor)
            {
                return true;
            }

            var attachee = invocation.attachee;
            if (attachee == null)
            {
                throw new NullReferenceException("Cannot run a script without an attachee");
            }

            var script = attachee.GetScript(obj_f.scripts_idx, (int) invocation.eventId);

            if (script.scriptId == 0)
            {
                return true; // No script attached
            }

            if (!_scriptAssembly.TryCreateObjectScript(script.scriptId, out var scriptObj))
            {
                Logger.Error("Object {0} has broken script {1} attached.", attachee, script.scriptId);
                return true;
            }

            return scriptObj.Invoke(ref invocation);
        }

        [TempleDllLocation(0x10025d60)]
        public int ExecuteObjectScript(GameObjectBody triggerer, GameObjectBody attachee, int spellId,
            ObjScriptEvent evt)
        {
            var invocation = new ObjScriptInvocation();
            invocation.eventId = evt;
            invocation.triggerer = triggerer;
            invocation.attachee = attachee;
            if (spellId != 0)
            {
                invocation.spell = GameSystems.Spell.GetActiveSpell(spellId);
            }

            return Invoke(ref invocation) ? 1 : 0;
        }

        [TempleDllLocation(0x10025d60)]
        public int ExecuteObjectScript(GameObjectBody triggerer, GameObjectBody attachee, GameObjectBody objectArg,
            ObjScriptEvent evt, int unk2)
        {
            var invocation = new ObjScriptInvocation();
            invocation.eventId = evt;
            invocation.triggerer = triggerer;
            invocation.attachee = attachee;
            return Invoke(ref invocation) ? 1 : 0;
        }

        public int ExecuteObjectScript(GameObjectBody triggerer, GameObjectBody attachee, ObjScriptEvent evt)
        {
            var invocation = new ObjScriptInvocation();
            invocation.eventId = evt;
            invocation.triggerer = triggerer;
            invocation.attachee = attachee;
            return Invoke(ref invocation) ? 1 : 0;
        }

        public bool GetLegacyHeader(ref ObjectScript script)
        {
            var path = GameSystems.ScriptName.GetScriptPath(script.scriptId);
            if (path != null)
            {
                using var reader = Tig.FS.OpenBinaryReader(path);
                script.unk1 = reader.ReadInt32();
                script.counters = reader.ReadUInt32();
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x10006a20)]
        [TempleDllLocation(0x10006a30)]
        public int StoryState
        {
            get => _currentStoryState;
            set
            {
                if (value > _currentStoryState)
                {
                    _currentStoryState = value;
                }
            }
        }

        [TempleDllLocation(0x10006790)]
        public bool GetGlobalFlag(int index) => ((_globalFlags[index / 32] >> index % 32) & 1) != 0;

        [TempleDllLocation(0x100067c0)]
        public void SetGlobalFlag(int index, bool enable)
        {
            var value = enable ? 1 : 0;

            _globalFlags[index / 32] = (uint) ((value << index % 32) | _globalFlags[index / 32] & ~(1 << index % 32));
        }

        [TempleDllLocation(0x10006760)]
        public int GetGlobalVar(int index) => _globalVars[index];

        [TempleDllLocation(0x10006770)]
        public void SetGlobalVar(int index, int value) => _globalVars[index] = value;

        [TempleDllLocation(0x10BCA76C)]
        private GameObjectBody _animationScriptContext;

        [TempleDllLocation(0x100aeda0)]
        public void SetAnimObject(GameObjectBody obj)
        {
            // Sets the Python global for which obj was just animated
            _animationScriptContext = obj;
        }

        /// <summary>
        /// Is used to judge whether someone is threatening an attack of opportunity during movement.
        /// </summary>
        public bool ShouldIgnoreTargetDuringCombat(GameObjectBody obj, GameObjectBody target)
        {
            Stub.TODO();
            return false;
        }

        /// <summary>
        /// Executes custom Python script logic.
        /// </summary>
        public T ExecuteScript<T>(string module, string function, params object[] args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes custom Python script logic.
        /// </summary>
        public void ExecuteScript(string module, string function, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}