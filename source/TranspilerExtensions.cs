using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace jshepler.ngu.mods
{
    public static class TranspilerExtensions
    {
        public static readonly MethodInfo Application_persistentDataPath =
            AccessTools.PropertyGetter(typeof(UnityEngine.Application), "persistentDataPath");

        public static void LogTranspilerStart(string className, string methodName)
        {
            Plugin.LogInfo($"[{className}] Transpiler started: {methodName}");
        }

        public static void LogTranspilerEnd(string className, string methodName, int instructionCount)
        {
            Plugin.LogInfo($"[{className}] Transpiler completed: {methodName}, {instructionCount} instructions");
        }

        public static void LogAllStrings(IEnumerable<CodeInstruction> instructions, string className, string keyword = null)
        {
            var list = instructions.ToList();
            var found = new List<string>();
            
            foreach (var instr in list)
            {
                if (instr.opcode == System.Reflection.Emit.OpCodes.Ldstr && instr.operand is string str)
                {
                    if (keyword == null || str.Contains(keyword))
                    {
                        found.Add(str);
                    }
                }
            }
            
            if (found.Count > 0)
            {
                Plugin.LogInfo($"[{className}] 找到 {found.Count} 个字符串 {(keyword != null ? $"包含 '{keyword}'" : "")}:");
                foreach (var s in found.Take(20))
                {
                    Plugin.LogInfo($"  - \"{s}\"");
                }
                if (found.Count > 20)
                    Plugin.LogInfo($"  ... 还有 {found.Count - 20} 个");
            }
            else
            {
                Plugin.LogWarning($"[{className}] 未找到包含 '{keyword}' 的字符串");
            }
        }

        public static void LogMatchFound(string className, string pattern, int index)
        {
            Plugin.LogInfo($"[{className}] Match found at [{index}]: {Truncate(pattern, 50)}");
        }

        public static void LogMatchFailed(string className, string pattern)
        {
            Plugin.LogWarning($"[{className}] Match failed: {Truncate(pattern, 50)}");
        }

        public static void LogError(string className, string message, Exception ex = null)
        {
            if (ex != null)
                Plugin.LogError($"[{className}] {message}\n{ex}");
            else
                Plugin.LogError($"[{className}] {message}");
        }

        private static string Truncate(string s, int maxLen)
        {
            if (string.IsNullOrEmpty(s)) return "(null)";
            return s.Length <= maxLen ? s : s.Substring(0, maxLen) + "...";
        }

        public static IEnumerable<CodeInstruction> SafeTranspile(
            string className,
            string methodName,
            IEnumerable<CodeInstruction> instructions,
            Action<CodeMatcher> patchAction,
            bool logOnFail = true)
        {
            try
            {
                LogTranspilerStart(className, methodName);
                
                var cm = new CodeMatcher(instructions);
                patchAction(cm);
                
                var result = cm.InstructionEnumeration().ToList();
                LogTranspilerEnd(className, methodName, result.Count);
                return result;
            }
            catch (Exception ex)
            {
                LogError(className, $"Transpiler failed: {methodName}", ex);
                return instructions; // 返回原始指令而不是崩溃
            }
        }

        public static bool SafeMatchAndPatch(
            CodeMatcher cm,
            string className,
            System.Reflection.Emit.OpCode opcode,
            object operand,
            Action<CodeMatcher> patchAction,
            string description = null)
        {
            try
            {
                cm.MatchForward(false, new CodeMatch(opcode, operand));
                if (cm.IsValid)
                {
                    patchAction(cm);
                    return true;
                }
                else
                {
                    var desc = description ?? $"opcode={opcode}, operand={operand}";
                    Plugin.LogWarning($"[{className}] 补丁已跳过：未找到 '{Truncate(desc, 60)}'");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError(className, $"MatchAndPatch failed: {opcode}", ex);
                return false;
            }
        }
    }

    public static class CecilExtensions
    {
        public static bool IsMatchingString(this Instruction instruction, string expected)
        {
            return instruction.OpCode == OpCodes.Ldstr && 
                   instruction.Operand is string str && 
                   str == expected;
        }

        public static string GetStringOperand(this Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Ldstr && instruction.Operand is string str)
                return str;
            return null;
        }
    }
}
