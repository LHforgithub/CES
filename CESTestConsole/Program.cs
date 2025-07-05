// See https://aka.ms/new-console-template for more information
using CES;
using CESTestConsole;
using System.Data;
using System.Text;


var combination = new ProcessCombination();
var trigger = new TestTrigger();
var fp_1 = new TestFreeParam_1();
var fp_2 = new TestFreeParam_2();
var pp = new TestParamProcessor();
var pp2 = new TestParamProcessor();
var pp3 = new TestParamProcessor();
var ts = new TestTargetSearch();
var condition = new TestCondition_2();
var effect = new TestEffect_1();
var activity = new TestActivity();
combination.AddComponent(trigger);
combination.AddComponent(fp_1);
combination.AddComponent(fp_2);
combination.AddComponent(ts);
combination.AddComponent(condition);
combination.AddComponent(effect);
combination.AddComponent(activity);


Console.WriteLine(combination.AddReference(condition, trigger, true, false, false, 0, 0));
Console.WriteLine(combination.AddReference(condition, ts, false, false, true, 0, 1));
Console.WriteLine(combination.AddReference(effect, trigger, true, false, false, 0, 0));
Console.WriteLine(combination.AddReference(effect, fp_2, true, false, false, 0, 1));
Console.WriteLine(combination.AddReference(effect, ts, false, true, false, 0, 0));
Console.WriteLine();
var (ef, i) = combination.GetResult<SingleEffect>();
Console.WriteLine(ef);
Console.WriteLine($"Result ErrorCode : {i}");

Console.WriteLine();
trigger.OnTrigger();

var log = "";
foreach (var error in combination.ErrorComponents)
{
    log += $"[{DateTime.Now.ToLongTimeString()}] {error}\n";
}
log += LogTool.Instance.GetAllLogs();
var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
if (!File.Exists(path))
{
    File.Create(path).Close();
}

Console.WriteLine(log);
File.WriteAllText(path, log, Encoding.UTF8);