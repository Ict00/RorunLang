using System.Text.Json;
using RorunLang;
using RorunLang.Execution;
using RorunLang.Parse;
Operators.Init();

try
{
    ExecutorHelper.Import(args[0]);
}
catch
{
    try
    {
        ExecutorHelper.Import("main.kii");
    }
    catch
    {
        ExecutorHelper.Build("block main {println \"File not found\";}");
    }
}
Console.Clear();
ExecutorHelper.Run();