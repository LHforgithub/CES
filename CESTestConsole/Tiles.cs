using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CES;
namespace CESTestConsole
{
    public class TestActivity : ActivityBase
    {
        public override async Task<bool> ConditionCheck(Dictionary<ICESCondition, List<ICESParamable>> triggerConditions)
        {
            try
            {
                foreach (var item in triggerConditions)
                {
                    if (!await item.Key.Check(item.Value))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public override void Destroy()
        {

        }

        public override async Task EffectAction(Dictionary<ICESEffect, KeyValuePair<List<ICESParamable>, List<ICESTargetable>[]>> effectActionDic)
        {
            try
            {
                foreach (var item in effectActionDic)
                {
                    await item.Key.Effect(item.Value.Key, item.Value.Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public override void Init()
        {

        }
    }

    public class TestParam_1 : ICESParamable
    {
        public bool Condition { get; set; } = true;
        public int Value { get; set; } = 0;
    }
    public class TestParam_2 : ICESParamable
    {
        public bool Condition { get; set; } = false;
        public int Value { get; set; } = 1;
    }
    public class TestTarget_1 : ICESTargetable
    {
        public string Value { get; set; } = "Tag 1";
    }

    public class TestTrigger : TriggerBase
    {
        public override List<Type> ProvideParamTypes => new List<Type>() { typeof(TestParam_1) , typeof(TestParam_2) };
        public override IDescribeProcessor DescribeProcessor => throw new NotImplementedException();

        public override void OnTrigger()
        {
            Console.WriteLine("Trigger");
            ProvideParams.Add(new TestParam_1());
            ProvideParams.Add(new TestParam_2());
            Owner?.Triggered();
        }
        public override void Destroy()
        {
        }
        public override void Init()
        {
        }
        public override string ChangeDescription(string originalDesc)
        {
            return originalDesc;
        }
    }
    public class TestFreeParam_1 : FreeParamBase<TestParam_1>
    {
        public override IDescribeProcessor DescribeProcessor => throw new NotImplementedException();
        public override string ChangeDescription(string originalDesc)
        {
            return originalDesc;
        }
        public override void Destroy()
        {
        }
        public override void Init()
        {
        }
        public override TestParam_1 GetParam()
        {
            Console.WriteLine("GetParam 1");
            return new TestParam_1() { Value = 10 };
        }
    }
    public class TestFreeParam_2 : FreeParamBase<TestParam_2>
    {
        public override IDescribeProcessor DescribeProcessor => throw new NotImplementedException();
        public override string ChangeDescription(string originalDesc)
        {
            return originalDesc;
        }
        public override void Destroy()
        {
        }
        public override void Init()
        {
        }
        public override TestParam_2 GetParam()
        {
            Console.WriteLine("GetParam 2");
            return new TestParam_2();
        }
    }

    public class TestFreeParam_3 : FreeParamBase<TestTarget_1>
    {
        public override IDescribeProcessor DescribeProcessor => throw new NotImplementedException();

        public override string ChangeDescription(string originalDesc)
        {
            return originalDesc;
        }

        public override void Destroy()
        {
        }

        public override TestTarget_1 GetParam()
        {
            return new TestTarget_1();
        }

        public override void Init()
        {
        }
    }
    public class TestParamProcessor : ParamProcessorBase<TestParam_1, TestParam_1>
    {
        public override IDescribeProcessor DescribeProcessor => throw new NotImplementedException();
        public override string ChangeDescription(string originalDesc)
        {
            return originalDesc;
        }
        public override void Destroy()
        {
        }
        public override void Init()
        {

        }
        public override TestParam_1 ProcessParam(TestParam_1 param)
        {
            Console.WriteLine("ProcessParam");
            return new TestParam_1() { Condition = new TestParam_2().Condition };
        }
    }
    public class TestParamToTarget : ParamTargetsConvertorBase<TestTarget_1, TestTarget_1>
    {
        public override IDescribeProcessor DescribeProcessor => throw new NotImplementedException();

        public override string ChangeDescription(string originalDesc)
        {
            return originalDesc;
        }

        public override async Task<List<TestTarget_1>> ChangeParamToTargets(TestTarget_1 param)
        {
             return [param];
        }

        public override void Destroy()
        {
        }

        public override void Init()
        {
        }
    }

    public class TestTargetSearch : TargetSearchBase<TestTarget_1>
    {
        public override IDescribeProcessor DescribeProcessor => throw new NotImplementedException();
        public override string ChangeDescription(string originalDesc)
        {
            return originalDesc;
        }
        public override void Destroy()
        {
        }
        public override void Init()
        {
        }

        public override List<TestTarget_1> GetAllTarget()
        {
            Console.WriteLine("GetAllTarget");
            return new List<TestTarget_1>() { new TestTarget_1() };
        }
        public override async Task<List<TestTarget_1>> SelectTarget(List<TestTarget_1> filterTarget)
        {
            Console.WriteLine("SelectTarget");
            filterTarget.ForEach(x => x.Value += "1");
            return filterTarget;
        }
    }
    public class TestCondition_1 : ConditionBase
    {
        public override IDescribeProcessor DescribeProcessor => throw new NotImplementedException();
        public override List<Type> RequireParamTypes => [typeof(TestParam_1)];
        public override string ChangeDescription(string originalDesc)
        {
            return originalDesc;
        }
        public override async Task<bool> Check(List<ICESParamable> param)
        {
            Console.WriteLine("Condition");
            return (param[0] as TestParam_1).Condition;
        }
        public override void Destroy()
        {
        }
        public override void Init()
        {
        }
    }
    public class TestCondition_2 : ConditionBase
    {
        public override IDescribeProcessor DescribeProcessor => throw new NotImplementedException();
        public override List<Type> RequireParamTypes => [typeof(TestParam_1), typeof(TestTarget_1)];
        public override string ChangeDescription(string originalDesc)
        {
            return originalDesc;
        }
        public override async Task<bool> Check(List<ICESParamable> param)
        {
            Console.WriteLine("Condition");
            Console.WriteLine((param[^1] as TestTarget_1).Value);
            return (param[0] as TestParam_1).Condition;
        }
        public override void Destroy()
        {
        }
        public override void Init()
        {
        }
    }
    public class TestEffect_1 : EffectBase
    {
        public override List<Type> RequireParamTypes => [typeof(TestParam_1), typeof(TestParam_2)];
        public override List<Type> RequireTargetTypes => [typeof(TestTarget_1)];
        public override IDescribeProcessor DescribeProcessor => throw new NotImplementedException();
        public override string ChangeDescription(string originalDesc)
        {
            return originalDesc;
        }
        public override async Task<int> Effect(List<ICESParamable> param, List<ICESTargetable>[] target)
        {
            Console.WriteLine("Effect");
            foreach (var item in param)
            {
                Console.WriteLine("Effect Params");
                Console.WriteLine(item);
                if (item is TestParam_1 t1)
                {
                    Console.WriteLine($"T1 : {t1.Value}");
                }
            }
            foreach (var item in target)
            {
                Console.WriteLine("Effect Targets");
                Console.WriteLine(item.Count);
                foreach (var targetItem in item)
                {
                    Console.WriteLine($"Target: {targetItem}");
                    Console.WriteLine($"Target: {(targetItem as TestTarget_1)?.Value}");
                }
            }
            return 0;
        }
        public override void Destroy()
        {
        }
        public override void Init()
        {
        }
    }
}
