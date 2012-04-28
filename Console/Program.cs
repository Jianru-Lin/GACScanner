using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib;

namespace MyConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Write("正在枚举 GAC 程序集...");

                List<string> assemblyList = new List<string>();

                Core.EnumAllAssemblies((assemblyNameStr) =>
                {
                    assemblyList.Add(assemblyNameStr);
                    return true;
                });

                Console.WriteLine("完成");

                Console.WriteLine("正在分析依赖关系，找出无效的程序集...");

                int count = 0;

                Core.CheckAssemblies(assemblyList, (i, isBroken, error) =>
                {
                    if (isBroken)
                    {
                        count++;
                        Console.WriteLine(assemblyList[i]);
                    }
                    return true;
                });

                Console.WriteLine("分析完毕，{0}个程序集中有{1}个损坏", assemblyList.Count, count);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.ReadKey();
            }
        }
    }
}
