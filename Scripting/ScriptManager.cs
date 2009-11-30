using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System.IO;
using System.CodeDom.Compiler;
using System.Reflection;

namespace SharpOT.Scripting
{
    public class ScriptManager
    {
        private static CSharpCodeProvider cSharpCodeProvider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
        private static VBCodeProvider vBCodeProvider = new VBCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
        private static List<IScript> loadedScripts = new List<IScript>();

        private static StringBuilder errorLog;

        public static string LoadAllScripts(Game game)
        {
            // load scripts from the current assembly
            LoadScript(game, System.Reflection.Assembly.GetExecutingAssembly().Location);

            errorLog = new StringBuilder();
            foreach (string directory in SharpOT.Properties.Settings.Default.ScriptsDirectory.Split(';'))
            {
                if (!Directory.Exists(directory)) continue;
                foreach (string path in Directory.GetFiles(directory))
                {
                    // TODO: concatenate files, then load
                    if (!File.Exists(path)) continue;
                    LoadScript(game, path);
                }
            }
            return errorLog.ToString();
        }

        public static void ReloadAllScripts(Game game)
        {
            UnloadAllScripts();
            LoadAllScripts(game);
        }

        public static void UnloadAllScripts()
        {
            foreach (IScript script in loadedScripts)
            {
                script.Stop();
            }
            loadedScripts.Clear();
        }

        public static void LoadScript(Game game, string path)
        {
            Assembly assembly = null;
            switch (Path.GetExtension(path))
            {
                case ".exe":
                case ".dll":
                    assembly = LoadAssembly(path);
                    break;
                case ".cs":
                    assembly = CompileScript(path, cSharpCodeProvider);
                    break;
                case ".vb":
                    assembly = CompileScript(path, vBCodeProvider);
                    break;
            }

            if (assembly != null)
            {
                foreach (IScript script in FindInterfaces<IScript>(assembly))
                {
                    loadedScripts.Add(script);
                    script.Start(game);
                }
                foreach (ICommand cmd in FindInterfaces<ICommand>(assembly))
                {
                    Commands.RegisterCommand(cmd);
                }
                foreach (IActionItem actionItem in FindInterfaces<IActionItem>(assembly))
                {
                    ActionItems.RegisterAction(actionItem);
                }
            }
        }

        public static Assembly CompileScript(string path, CodeDomProvider provider)
        {
            CompilerParameters compilerParameters = new CompilerParameters();
            compilerParameters.GenerateExecutable = false;
            compilerParameters.GenerateInMemory = true;
            compilerParameters.IncludeDebugInformation = false;
            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Core.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Data.Linq.dll");
            compilerParameters.ReferencedAssemblies.Add(System.Reflection.Assembly.GetExecutingAssembly().Location);
            CompilerResults results = provider.CompileAssemblyFromFile(compilerParameters, path);
            if (!results.Errors.HasErrors)
            {
                return results.CompiledAssembly;
            }
            else
            {
                foreach (CompilerError error in results.Errors)
                    errorLog.AppendLine(error.ToString());
            }
            return null;
        }

        public static IEnumerable<IType> FindInterfaces<IType>(Assembly assembly)
        {   
            foreach (Type t in assembly.GetTypes())
            {
                if (t.GetInterface(typeof(IType).Name, true) != null && !t.IsInterface && !t.IsAbstract)
                {
                    yield return (IType)assembly.CreateInstance(t.FullName);
                }
            }
        }

        public static Assembly LoadAssembly(string path)
        {
            return Assembly.LoadFile(path);
        }
    }
}
