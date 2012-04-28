using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.GACManagedAccess;
using System.Runtime.InteropServices;
using System.Reflection;

namespace CoreLib
{
    public delegate bool EnumCallback(string param);
    public delegate bool CheckCallback(int i, bool isBroken, string error);

    public class Core
    {
        static String GetFullName(IAssemblyName fusionAsmName)
        {
            StringBuilder sDisplayName = new StringBuilder(1024);
            int iLen = 1024;

            int hr = fusionAsmName.GetDisplayName(sDisplayName, ref iLen, (int)AssemblyNameDisplayFlags.ALL);
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            return sDisplayName.ToString();
        }

        static public void EnumAllAssemblies(EnumCallback callback)
        {
            if (callback == null)
            {
                return;
            }

            IAssemblyEnum asEnum = null;
            Utils.CreateAssemblyEnum(out asEnum, IntPtr.Zero, null, AssemblyCacheFlags.GAC, IntPtr.Zero);
            if (asEnum != null)
            {
                while (true)
                {
                    IAssemblyName asName = null;
                    asEnum.GetNextAssembly(IntPtr.Zero, out asName, 0);
                    if (asName == null)
                    {
                        break;
                    }
                    string fullName = GetFullName(asName);
                    Marshal.ReleaseComObject(asName);

                    if (!callback(fullName))
                    {
                        break;
                    }
                }
                Marshal.ReleaseComObject(asEnum);
            }
        }

        static public void CheckAssemblies(List<string> assemblyList,  CheckCallback callback)
        {
            if (assemblyList == null || assemblyList.Count < 1)
            {
                return;
            }

            if (callback == null)
            {
                return;
            }

            for (int i = 0; i < assemblyList.Count; ++i)
            {
                string error = null;
                bool isBroken = !CheckAssembly(assemblyList[i], out error);
                if (!callback(i, isBroken, error))
                {
                    break;
                }
            }
        }

        static public bool CheckAssembly(string assemblyNameStr, out string error)
        {
            error = null;
            Assembly assembly = Assembly.ReflectionOnlyLoad(assemblyNameStr);
            // 尝试加载其引用的其他程序集，如果发现有任何一个引用的程序集无法加载，则判定当前程序集失效
            AssemblyName[] refAssemblyNames = assembly.GetReferencedAssemblies();
            for (int i = 0; i < refAssemblyNames.Length; ++i)
            {
                try
                {
                    Assembly refAssembly = Assembly.ReflectionOnlyLoad(refAssemblyNames[i].FullName);
                }
                catch (System.Exception ex)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
