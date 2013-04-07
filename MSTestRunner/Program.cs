using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// linuxにCopyして実行してもOK！
namespace MSTestRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            int allTestCount = 0;
            int successTestCount = 0;
            int failedTestCount = 0;

            // 自分のパスを取得
            Assembly myAssembly = Assembly.GetEntryAssembly();
            string path = System.IO.Path.GetDirectoryName(myAssembly.Location);

            foreach (var file in Directory.GetFiles(path))
            {
                if (!file.EndsWith(".dll")) continue;

                string dllname = System.IO.Path.GetFileNameWithoutExtension(file);
                Assembly asm = null;
                try
                {
                    asm = Assembly.Load(dllname);
                }
                catch
                {
                    continue;
                }

                if (asm == null) continue;


                foreach (var type in asm.GetTypes())
                {
                    var attrClass = type.GetCustomAttributes(inherit: false);
                    bool isTestClass = attrClass.Select(x => x is TestClassAttribute).Contains(true);

                    // TestClassで無ければスルー
                    if (!isTestClass) continue;

                    // ClassInitializeアトリビュートがついたメソッドを実行

                    // TestInitializeアトリビュートがついたメソッドを実行

                    // TestClassのインスタンスを生成
                    object testClass = Activator.CreateInstance(type);

                    // すべてのPublicメソッド
                    foreach (var method in type.GetMethods())
                    {
                        var attrMethod = method.GetCustomAttributes(inherit: true);
                        bool isTestMethod = attrMethod.Select(x => x is TestMethodAttribute).Contains(true);

                        if (!isTestMethod) continue;

                        try
                        {
                            allTestCount++;
                            method.Invoke(testClass, null);
                        }
                        catch (Exception e)
                        {
                            failedTestCount++;
                            string message = e.InnerException.Message;


                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[NG] {0}.{1}, {2}", type.Name, method.Name, message);
                            Console.ResetColor();
                            continue;
                        }

                        successTestCount++;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("[OK] {0}.{1}", type.Name, method.Name);
                        Console.ResetColor();
                        // TestCleanup
                    }


                    // ClassCleanup
                }
            }

            Console.WriteLine("失敗 = {0}, 成功 = {1}, ケース数 = {2}", failedTestCount, successTestCount, allTestCount);
            Console.WriteLine("終了するためには何かキーを押してください...");
            Console.ReadKey();
        }
    }
}
