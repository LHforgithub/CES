// See https://aka.ms/new-console-template for more information
using CES;
using CESTestConsole;
using System.Data;
using System.Text;
using Newtonsoft.Json;

var combination = new ProcessCombination();
var trigger = new TestTrigger();
var fp_1 = new TestFreeParam_1();
var fp_2 = new TestFreeParam_2();
var fp_3 = new TestFreeParam_3();
var pp = new TestParamProcessor();
var pp2 = new TestParamProcessor();
var pp3 = new TestParamProcessor();
var ts = new TestTargetSearch();
var p2t = new TestParamToTarget();
var condition = new TestCondition_1();
var effect = new TestEffect_1();
var activity = new TestActivity();

Console.WriteLine(combination.AddComponent(trigger));
Console.WriteLine(combination.AddComponent(fp_1));
Console.WriteLine(combination.AddComponent(fp_2));
Console.WriteLine(combination.AddComponent(fp_3));
Console.WriteLine(combination.AddComponent(ts));
Console.WriteLine(combination.AddComponent(p2t));
Console.WriteLine(combination.AddComponent(condition));
Console.WriteLine(combination.AddComponent(effect));
Console.WriteLine(combination.AddComponent(activity));


Console.WriteLine(combination.AddReference(condition, trigger, false, false, true, 0, 0));
Console.WriteLine(combination.AddReference(condition, trigger, true, false, false, 0, 0));
//Console.WriteLine(combination.AddReference(condition, ts, true, false, true, 1, 0));
Console.WriteLine(combination.AddReference(effect, trigger, true, false, false, 0, 0));
Console.WriteLine(combination.AddReference(effect, fp_2, true, false, false, 1, 0));
//Console.WriteLine(combination.AddReference(effect, ts, false, true, false, 0, 0));
Console.WriteLine(combination.AddReference(p2t, fp_3, true, false, false, 0, 0));
Console.WriteLine(combination.AddReference(effect, p2t, false, true, false, 0, 0));

Console.WriteLine();
var (ef, i) = combination.GetResult();
Console.WriteLine(ef);
Console.WriteLine($"Result ErrorCode : {i}");

Console.WriteLine();
trigger.OnTrigger();



var savepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "savepath.json");
if (!File.Exists(savepath))
{
    File.Create(savepath).Close();
}
var save = JsonConvert.SerializeObject(CESSingleEffectConvertor.Serialize(ef), Formatting.Indented);
File.WriteAllText(savepath, save, Encoding.UTF8);

var saveObj = JsonConvert.DeserializeObject<CESSerializableSingleEffect>(File.ReadAllText(savepath));
var newEf = CESSingleEffectConvertor.Deserialize(saveObj, [trigger.GetType().Assembly]);
Console.WriteLine("New Save Trigger:");
if (newEf != null)
{
    newEf.Trigger.OnTrigger();
}



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

