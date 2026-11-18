using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
class VerifyGame
{
    static void Main(string[] args)
    {
        var original=AssemblyDefinition.ReadAssembly(args[0]);
        var patched=AssemblyDefinition.ReadAssembly(args[1]);
        var helper=AssemblyDefinition.ReadAssembly(args[2]);
        int methods=0,branches=0;
        foreach(var type in patched.MainModule.GetTypes())foreach(var method in type.Methods)
        {
            if(!method.HasBody)continue;methods++;
            foreach(var i in method.Body.Instructions)
                if(i.OpCode.OperandType==OperandType.ShortInlineBrTarget || i.OpCode.OperandType==OperandType.InlineBrTarget)
                {if(i.Operand==null)throw new Exception("Broken branch: "+method.FullName);branches++;}
        }
        Func<AssemblyDefinition,MethodDefinition> camera=a=>a.MainModule.Types.Single(t=>t.Name=="SimulationNativeMethods").Methods.Single(m=>m.Name=="GetCameraTransform");
        Func<MethodDefinition,string> il=m=>string.Join("\n",m.Body.Instructions.Select(i=>i.OpCode.Name+" "+i.Operand).ToArray());
        if(il(camera(original))!=il(camera(patched)))throw new Exception("Camera implementation changed");
        var calls=helper.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody)
            .SelectMany(m=>m.Body.Instructions).Where(i=>i.Operand is MethodReference).Select(i=>((MethodReference)i.Operand).Name).ToArray();
        foreach(string forbidden in new[]{"OrbitCamera","UltraStick","StartWheelSpin","TriggerDash","CancelSpinDash"})
            if(calls.Contains(forbidden))throw new Exception("Abandoned control path retained: "+forbidden);
        Console.WriteLine("Verified "+methods+" method bodies and "+branches+" branch targets.");
        Console.WriteLine("Original camera IL unchanged. Empty native dash exports unused.");
        Console.WriteLine("Helper runtime="+helper.MainModule.RuntimeVersion+"; mscorlib="+helper.MainModule.AssemblyReferences.Single(r=>r.Name=="mscorlib").Version);
    }
}
