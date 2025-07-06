using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CES
{
    // 触发器基类，实现了ICESTrigger和ICESTargetable接口
    public abstract class TriggerBase : ICESTrigger, ICESTargetable
    {
        // 所属的SingleEffect对象
        public SingleEffect Owner { get; set; }
        // 使用的数值参数列表
        public virtual List<float> UsingNumber => [];
        // 是否使用数值参数
        public virtual bool IsUsingNumber { get; }
        // 使用的数值参数数量
        public virtual int UsingNumberCount { get; }
        // 组件自身索引
        public int SelfIndex { get; set; } = 0;
        // 提供的参数类型列表
        public virtual List<Type> ProvideParamTypes => [];
        // 提供的参数实例列表
        public List<ICESParamable> ProvideParams { get; } = [];
        // 描述处理器
        public abstract IDescribeProcessor DescribeProcessor { get; }
        // 修改描述文本
        public abstract string ChangeDescription(string originalDesc);
        // 触发时调用
        public abstract void OnTrigger();
        // 初始化
        public abstract void Init();
        // 销毁
        public abstract void Destroy();
        // 用于排序
        public int CompareTo(ICESComponent other)
        {
            return SelfIndex.CompareTo(other?.SelfIndex ?? 0);
        }   
    }
    // 自由参数基类，泛型T为参数类型，实现ICESFreeParam接口
    public abstract class FreeParamBase<T> : ICESFreeParam where T : ICESParamable
    {
        public SingleEffect Owner { get; set; }
        public virtual List<float> UsingNumber => [];
        public virtual bool IsUsingNumber { get; }
        public virtual int UsingNumberCount { get; }
        public int SelfIndex { get; set; } = 0;
        // 提供的参数类型
        public Type ProvideParamType => typeof(T);
        public abstract IDescribeProcessor DescribeProcessor { get; }
        public abstract string ChangeDescription(string originalDesc);
        // 获取参数实例
        public ICESParamable TryGetParam() => GetParam();
        public abstract T GetParam();
        public abstract void Init();
        public abstract void Destroy();
        public int CompareTo(ICESComponent other)
        {
            return SelfIndex.CompareTo(other?.SelfIndex ?? 0);
        }
    }
    // 参数处理器基类，T为输入参数类型，U为输出参数类型，实现ICESParamProcessor接口
    public abstract class ParamProcessorBase<T, U> : ICESParamProcessor where T : ICESParamable where U : ICESParamable
    {
        public SingleEffect Owner { get; set; }
        public virtual List<float> UsingNumber => [];
        public virtual bool IsUsingNumber { get; }
        public virtual int UsingNumberCount { get; }
        public int SelfIndex { get; set; } = 0;
        // 需要的参数类型
        public virtual Type RequireParamType => typeof(T);
        // 需要的参数索引
        public int RequireParamIndex { get; set; }
        // 提供的参数类型
        public virtual Type ProvideParamType => typeof(U);
        public abstract IDescribeProcessor DescribeProcessor { get; }
        public abstract string ChangeDescription(string originalDesc);
        // 处理参数
        public ICESParamable Process(ICESParamable param) => ProcessParam((T)param);
        public abstract U ProcessParam(T param);
        public abstract void Init();
        public abstract void Destroy();
        public int CompareTo(ICESComponent other)
        {
            return SelfIndex.CompareTo(other?.SelfIndex ?? 0);
        }
    }
    // 条件基类，实现ICESCondition和ICESTargetable接口
    public abstract class ConditionBase : ICESCondition, ICESTargetable
    {
        public SingleEffect Owner { get; set; }
        public virtual List<float> UsingNumber => [];
        public virtual bool IsUsingNumber { get; }
        public virtual int UsingNumberCount { get; }
        public int SelfIndex { get; set; } = 0;
        // 影响的组件索引
        public int AffectComponentIndex { get; set; }
        // 需要的参数类型列表
        public virtual List<Type> RequireParamTypes { get; } = [];
        // 需要的参数索引列表
        public List<int> RequireParamIndexes { get; } = [];
        public abstract IDescribeProcessor DescribeProcessor { get; }
        public abstract string ChangeDescription(string originalDesc);
        // 检查条件是否成立
        public abstract Task<bool> Check(List<ICESParamable> param);
        public abstract void Init();
        public abstract void Destroy();
        public int CompareTo(ICESComponent other)
        {
            return SelfIndex.CompareTo(other?.SelfIndex ?? 0);
        }
    }
    // 目标搜索基类，T为目标类型，实现ICESTargetSearch和ICESTargetable接口
    public abstract class TargetSearchBase<T> : ICESTargetSearch, ICESTargetable where T : ICESTargetable
    {
        public SingleEffect Owner { get; set; }
        public virtual List<float> UsingNumber => [];
        public virtual bool IsUsingNumber { get; }
        public virtual int UsingNumberCount { get; }
        public int SelfIndex { get; set; } = 0;
        // 提供的目标类型
        public virtual Type ProvideTargetType => typeof(T);
        public abstract IDescribeProcessor DescribeProcessor { get; }
        public abstract string ChangeDescription(string originalDesc);
        // 获取所有目标
        public List<ICESTargetable> GetAll() => [.. GetAllTarget().OfType<ICESTargetable>()];
        // 搜索目标
        public async Task<List<ICESTargetable>> Search(List<ICESTargetable> filterTarget) => [.. (await SelectTarget([.. filterTarget.OfType<T>()])).OfType<ICESTargetable>()];
    
        // 获取所有目标（泛型）
        public abstract List<T> GetAllTarget();
        // 选择目标
        public abstract Task<List<T>> SelectTarget(List<T> filterTarget);
        public abstract void Init();
        public abstract void Destroy();
        public int CompareTo(ICESComponent other)
        {
            return SelfIndex.CompareTo(other?.SelfIndex ?? 0);
        }
    }
    // 活动基类，实现ICESActivity接口
    public abstract class ActivityBase : ICESActivity
    {
        public SingleEffect Owner { get; set; }
        public virtual List<float> UsingNumber => [];
        public virtual bool IsUsingNumber { get; }
        public virtual int UsingNumberCount { get; }
        public int SelfIndex { get; set; } = 0;
        // 描述处理器（活动默认无）
        public IDescribeProcessor DescribeProcessor => null;
        // 修改描述文本
        public virtual string ChangeDescription(string originalDesc)
        {
            return "";
        }

        // 查找指定影响索引的条件及其参数
        private Dictionary<ICESCondition, List<ICESParamable>> ConditionParamsSearch(int affectIndex, List<ICESParamable> plusParams = null)
        {
            var result = new Dictionary<ICESCondition, List<ICESParamable>>();
            plusParams ??= [];
            List<ICESCondition> conditions = [.. Owner.Conditions];
            for (int i = 0; i < conditions.Count; i++)
            {
                var condition = conditions[i];
                if (condition == null || condition.AffectComponentIndex != affectIndex)
                {
                    continue;
                }
                var paramList = new List<ICESParamable>();
                for (int j = 0; j < condition.RequireParamTypes.Count; j++)
                {
                    var index = condition.RequireParamIndexes[j];
                    var comp = Owner.SearchParamComponentByIndex(index);
                    if (comp == null)
                    {
                        LogTool.Instance.Log($"Invalid Activity. Condition's parameters error. Param index: {j}, Require Index: {index}");
                        continue;
                    }
                    paramList.Add(Owner.GetFinalParam(comp));
                }
                paramList = [.. paramList.Where(x => x != null)];
                if (paramList.Count + plusParams.Count != condition.RequireParamTypes.Count)
                {
                    LogTool.Instance.Log($"Invalid Activity. Condition's parameters count error. Need param num: {condition.RequireParamTypes.Count}, recent count: {paramList.Count + plusParams.Count}.");
                    continue;
                }
                result.Add(condition, [.. paramList, .. plusParams]);
            }
            return result;
        }
        // 检查目标条件并获取目标
        private async Task<List<ICESTargetable>> TargetConditionCheckGet(ICESTargetSearch searchFunc)
        {
            var allTargets = searchFunc.GetAll();
            var result = new List<ICESTargetable>();
            foreach (var target in allTargets)
            {
                if (await ConditionCheck(ConditionParamsSearch(searchFunc.SelfIndex, [target])))
                {
                    result.Add(target);
                }
            }
            return await searchFunc.Search(result);
        }
        // 活动执行主流程
        public async Task Action()
        {
            if (Owner == null || Owner.Activity != this)
            {
                LogTool.Instance.Log($"Invalid Activity. Owner info error.");
                return;
            }
            if (await ConditionCheck(ConditionParamsSearch(Owner.Trigger.SelfIndex)))
            {
                var dic = new Dictionary<ICESEffect, KeyValuePair<List<ICESParamable>, List<ICESTargetable>[]>>();
                for (int i = 0; i < Owner.Effects.Count; i++)
                {
                    if (i >= Owner.Effects.Count)
                    {
                        break;
                    }
                    var effect = Owner.Effects[i];
                    if (effect != null)
                    {
                        var effectParams = new List<ICESParamable>();
                        for (int j = 0; j < effect.RequireParamTypes.Count; j++)
                        {
                            if (j >= effect.RequireParamIndexes.Count)
                            {
                                break;
                            }
                            var index = effect.RequireParamIndexes[j];
                            var param = Owner.SearchParamComponentByIndex(index);
                            if (param == null)
                            {
                                break;
                            }
                            effectParams.InsertOrUpdateAt(j, Owner.GetFinalParam(param, index));
                        }
                        if (effectParams.Count != effect.RequireParamTypes.Count)
                        {
                            LogTool.Instance.Log($"Invalid Activity. Effect's parameters count error. Need param num: {effect.RequireParamTypes.Count}, recent count: {effectParams.Count}.");
                            continue;
                        }
                        var effectTargets = new List<ICESTargetable>[effect.RequireTargetTypes.Count];
                        for (int j = 0; j < effect.RequireTargetTypes.Count; j++)
                        {
                            if (j >= effect.RequireTargetIndexes.Count)
                            {
                                break;
                            }
                            var index = effect.RequireTargetIndexes[j];
                            var target = Owner.SearchParamComponentByIndex(index);
                            if (target is ICESTargetSearch tar)
                            {
                                effectTargets[j] = await TargetConditionCheckGet(tar);
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (effectTargets.Count() != effect.RequireTargetTypes.Count)
                        {
                            LogTool.Instance.Log($"Invalid Activity. Effect's parameters count error. Need param num: {effect.RequireTargetTypes.Count}, recent count: {effectTargets.Count()}.");
                            continue;
                        }
                        dic.Add(effect, new KeyValuePair<List<ICESParamable>, List<ICESTargetable>[]>(effectParams, effectTargets));
                    }
                }
                await EffectAction(dic);
            }
        }
        // 检查条件
        public abstract Task<bool> ConditionCheck(Dictionary<ICESCondition, List<ICESParamable>> triggerConditions);
        // 执行效果
        public abstract Task EffectAction(Dictionary<ICESEffect, KeyValuePair<List<ICESParamable>, List<ICESTargetable>[]>> effectActionDic);
        public abstract void Init();
        public abstract void Destroy();
        public int CompareTo(ICESComponent other)
        {
            return SelfIndex.CompareTo(other?.SelfIndex ?? 0);
        }
    }
    // 效果基类，实现ICESEffect和ICESTargetable接口
    public abstract class EffectBase : ICESEffect, ICESTargetable
    {
        public SingleEffect Owner { get; set; }
        public virtual List<float> UsingNumber => [];
        public virtual bool IsUsingNumber { get; }
        public virtual int UsingNumberCount { get; }
        public int SelfIndex { get; set; } = 0;
        public abstract IDescribeProcessor DescribeProcessor { get; }
        // 需要的参数类型列表
        public virtual List<Type> RequireParamTypes => [];
        // 需要的参数索引列表
        public List<int> RequireParamIndexes { get; } = [];
        // 需要的目标类型列表
        public virtual List<Type> RequireTargetTypes => [];
        // 需要的目标索引列表
        public List<int> RequireTargetIndexes { get; } = [];
        public abstract string ChangeDescription(string originalDesc);
        // 效果执行方法
        public abstract Task<int> Effect(List<ICESParamable> @params, List<ICESTargetable>[] targets);
        public abstract void Init();
        public abstract void Destroy();
        public int CompareTo(ICESComponent other)
        {
            return SelfIndex.CompareTo(other?.SelfIndex ?? 0);
        }
    }
}