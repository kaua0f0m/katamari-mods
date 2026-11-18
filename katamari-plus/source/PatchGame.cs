using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

class PatchGame
{
    static void Main(string[] args)
    {
        string folder = Path.GetFullPath(args[0]);
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.Combine(folder, "Managed"));
        resolver.AddSearchDirectory(folder);
        var original = AssemblyDefinition.ReadAssembly(Path.Combine(folder, "Managed/Assembly-CSharp.dll"), new ReaderParameters { AssemblyResolver = resolver });
        var helper = AssemblyDefinition.ReadAssembly(Path.Combine(folder, "KatamariQoL.dll"), new ReaderParameters { AssemblyResolver = resolver });
        ModuleDefinition module = original.MainModule;
        TypeDefinition qos = helper.MainModule.Types.Single(t => t.Name == "KatamariQoL");
        Func<string, MethodReference> call = name => module.ImportReference(qos.Methods.Single(m => m.Name == name));
        Func<string, string, MethodDefinition> method = (type, name) => module.Types.Single(t => t.Name == type).Methods.Single(m => m.Name == name);
        Action<MethodDefinition, string> postfix = (m, name) => {
            var il = m.Body.GetILProcessor();
            foreach (Instruction ret in m.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                // Keep the original instruction as the branch target. Some native paths
                // jump directly to this return and must also run the postfix.
                ret.OpCode = OpCodes.Ldarg_0;
                ret.Operand = null;
                var invoke = il.Create(OpCodes.Call, call(name));
                il.InsertAfter(ret, invoke);
                il.InsertAfter(invoke, il.Create(OpCodes.Ret));
            }
        };
        MethodDefinition awake = method("GlobalManager", "Awake");
        foreach (var ret in awake.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            awake.Body.GetILProcessor().InsertBefore(ret, Instruction.Create(OpCodes.Call, call("Install")));

        var titleType = module.Types.Single(t => t.Name == "Title2Manager");
        var oldStart = method("Title2Manager", "Start");
        oldStart.Name = "QOL_OriginalStart";
        var start = new MethodDefinition("Start", oldStart.Attributes, oldStart.ReturnType);
        titleType.Methods.Add(start);
        var ti = start.Body.GetILProcessor();
        ti.Emit(OpCodes.Ldarg_0);
        ti.Emit(OpCodes.Ldarg_0);
        ti.Emit(OpCodes.Call, oldStart);
        ti.Emit(OpCodes.Call, call("TitleStart"));
        ti.Emit(OpCodes.Ret);

        var movie = method("UIMoviePlayer", "PlayMove");
        var mi = movie.Body.GetILProcessor();
        var first = movie.Body.Instructions.First();
        mi.InsertBefore(first, mi.Create(OpCodes.Ldarg_0));
        mi.InsertBefore(first, mi.Create(OpCodes.Ldarg_1));
        mi.InsertBefore(first, mi.Create(OpCodes.Call, call("SkipMovie")));
        mi.InsertBefore(first, mi.Create(OpCodes.Brfalse, first));
        mi.InsertBefore(first, mi.Create(OpCodes.Ret));

        int introHooks = 0;
        foreach (var nested in module.Types.Single(t => t.Name == "Title3Manager").NestedTypes.Where(t => t.Name.Contains("<Start>")))
        foreach (var m in nested.Methods.Where(m => m.HasBody))
        foreach (var i in m.Body.Instructions.ToArray())
        {
            var target = i.Operand as MethodReference;
            if (target == null || target.Name != "IsSelectDown") continue;
            var il = m.Body.GetILProcessor();
            var extra = il.Create(OpCodes.Call, call("FastIntro"));
            il.InsertAfter(i, extra);
            il.InsertAfter(extra, il.Create(OpCodes.Or));
            introHooks++;
        }
        if (introHooks != 2) throw new Exception("Unexpected intro iterator: " + introHooks);

        var timer = method("GameManager", "sSubRemainTime");
        timer.Body = new MethodBody(timer);
        timer.Body.GetILProcessor().Emit(OpCodes.Ldarg_0);
        timer.Body.GetILProcessor().Emit(OpCodes.Call, call("TickTimer"));
        timer.Body.GetILProcessor().Emit(OpCodes.Ret);

        var pause = method("PauseMenu", "Update");
        var pi = pause.Body.GetILProcessor();
        var pfirst = pause.Body.Instructions.First();
        pi.InsertBefore(pfirst, pi.Create(OpCodes.Ldarg_0));
        pi.InsertBefore(pfirst, pi.Create(OpCodes.Call, call("BeforePause")));
        pi.InsertBefore(pfirst, pi.Create(OpCodes.Brtrue, pfirst));
        pi.InsertBefore(pfirst, pi.Create(OpCodes.Ret));
        postfix(pause, "AfterPause");

        postfix(method("QualityManager", "Start"), "QualityStart");
        var qualityUpdate = method("QualityManager", "Update");
        var qu = qualityUpdate.Body.GetILProcessor();
        var quFirst = qualityUpdate.Body.Instructions.First();
        qu.InsertBefore(quFirst, qu.Create(OpCodes.Ldarg_0));
        qu.InsertBefore(quFirst, qu.Create(OpCodes.Call, call("QualityUpdate")));
        qu.InsertBefore(quFirst, qu.Create(OpCodes.Brtrue, quFirst));
        qu.InsertBefore(quFirst, qu.Create(OpCodes.Ret));
        var qualityClick = method("QualityManager", "ButtonClick");
        var qc = qualityClick.Body.GetILProcessor();
        var qcFirst = qualityClick.Body.Instructions.First();
        qc.InsertBefore(qcFirst, qc.Create(OpCodes.Ldarg_0));
        qc.InsertBefore(qcFirst, qc.Create(OpCodes.Ldarg_2));
        qc.InsertBefore(qcFirst, qc.Create(OpCodes.Call, call("QualityClick")));
        qc.InsertBefore(qcFirst, qc.Create(OpCodes.Brtrue, qcFirst));
        qc.InsertBefore(qcFirst, qc.Create(OpCodes.Ret));

        postfix(method("KatamariPauseController", "Start"), "OptionsStart");
        var options = method("KatamariPauseController", "Update");
        var oi = options.Body.GetILProcessor();
        var of = options.Body.Instructions.First();
        oi.InsertBefore(of,oi.Create(OpCodes.Ldarg_0));
        oi.InsertBefore(of,oi.Create(OpCodes.Call,call("OptionsUpdate")));
        oi.InsertBefore(of,oi.Create(OpCodes.Brtrue,of));
        oi.InsertBefore(of,oi.Create(OpCodes.Ret));
        postfix(options,"OptionsAfter");

        var stick = method("Player", "DoStickSimulation2");
        int rightHooks=0,leftHooks=0;
        foreach(var i in stick.Body.Instructions.ToArray())
        {
            var target=i.Operand as MethodReference;
            if(target==null || target.Name!="GetAxis")continue;
            bool right=i.Previous.OpCode==OpCodes.Ldc_I4_1;
            bool left=i.Previous.OpCode==OpCodes.Ldc_I4_0;
            if(!right&&!left)continue;
            var il=stick.Body.GetILProcessor();
            var player=il.Create(OpCodes.Ldarg_0);
            il.InsertAfter(i,player);il.InsertAfter(player,il.Create(OpCodes.Call,call(right?"RemapSimpleRight":"RemapSimpleLeft")));
            if(right)rightHooks++;else leftHooks++;
        }
        if(rightHooks<1||leftHooks<1)throw new Exception("Right-stick input hook missing");

        var music = module.Types.Single(t => t.Name == "SoundController").Methods.Single(m => m.Name == "Play"
            && m.Parameters.Count == 8 && m.Parameters[0].ParameterType.Name == "eChannel" && m.Parameters[1].ParameterType.Name == "eClipGroup");
        var bi = music.Body.GetILProcessor();
        var bf = music.Body.Instructions.First();
        bi.InsertBefore(bf, bi.Create(OpCodes.Ldarg_1));
        bi.InsertBefore(bf, bi.Create(OpCodes.Ldarg_2));
        bi.InsertBefore(bf, bi.Create(OpCodes.Ldarg_3));
        bi.InsertBefore(bf, bi.Create(OpCodes.Ldarg, music.Parameters[3]));
        bi.InsertBefore(bf, bi.Create(OpCodes.Call, call("MusicIndex")));
        bi.InsertBefore(bf, bi.Create(OpCodes.Starg, music.Parameters[2]));

        string output = Path.Combine(folder, "patched/Assembly-CSharp.dll");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        original.Write(output);
        Console.WriteLine("Patched native hooks; intro skip checks=" + introHooks + "; output=" + output);
        using (var verify = AssemblyDefinition.ReadAssembly(output))
            Console.WriteLine("Reparsed " + verify.MainModule.Types.Count + " types; references=" + verify.MainModule.AssemblyReferences.Count);
    }
}
