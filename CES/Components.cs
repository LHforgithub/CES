using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CES
{
    /// <summary>
    /// 触发器基类，实现了ICESTrigger和ICESTargetable接口
    /// </summary>
    public abstract class TriggerBase : ICESTrigger, ICESTargetable
    {
        /// <summary>
        /// 所属的SingleEffect对象
        /// </summary>
        public SingleEffect Owner { get; set; }
        /// <summary>
        /// 使用的数值参数列表
        /// </summary>
        public virtual List<float> UsingNumber => [];
        /// <summary>
        /// 是否使用数值参数
        /// </summary>
        public virtual bool IsUsingNumber { get; }
        /// <summary>
        /// 使用的数值参数数量
        /// </summary>
        public virtual int UsingNumberCount { get; }
        /// <summary>
        /// 组件自身索引
        /// </summary>
        public int SelfIndex { get; set; } = 0;
        /// <summary>
        /// 提供的参数类型列表
        /// </summary>
        public virtual List<Type> ProvideParamTypes => [];
        /// <summary>
        /// 提供的参数实例列表
        /// </summary>
        public List<ICESParamable> ProvideParams { get; } = [];
        /// <summary>
        /// 描述处理器
        /// </summary>
        public abstract IDescribeProcessor DescribeProcessor { get; }
        /// <summary>
        /// 修改描述文本
        /// </summary>
        /// <param name="originalDesc"></param>
        /// <returns></returns>
        public abstract string ChangeDescription(string originalDesc);
        /// <summary>
        /// 触发时调用
        /// </summary>
        public abstract void OnTrigger();
        /// <summary>
        /// 初始化，持有者效果初始化时自动调用
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// 销毁，持有者效果销毁时自动调用
        /// </summary>
        public abstract void Destroy();
        /// <summary>
        /// 用于排序
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ICESComponent other)
        {
            return SelfIndex.CompareTo(other?.SelfIndex ?? 0);
        }   
    }
    /// <summary>
    /// 自由参数基类，泛型T为参数类型，实现ICESFreeParam接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FreeParamBase<T> : ICESFreeParam where T : ICESParamable
    {
        /// <summary>
        /// 所属的SingleEffect对象
        /// </summary>
        public SingleEffect Owner { get; set; }
        /// <summary>
        /// 使用的数值参数列表
        /// </summary>
        public virtual List<float> UsingNumber => [];
        /// <summary>
        /// 是否使用数值参数
        /// </summary>
        public virtual bool IsUsingNumber { get; }
        /// <summary>
        /// 使用的数值参数数量
        /// </summary>
        public virtual int UsingNumberCount { get; }
        /// <summary>
        /// 组件自身索引
        /// </summary>
        public int SelfIndex { get; set; } = 0;
        /// <summary>
        /// 提供的参数类型
        /// </summary>
        public Type ProvideParamType => typeof(T);
        /// <summary>
        /// 描述处理器
        /// </summary>
        public abstract IDescribeProcessor DescribeProcessor { get; }
        /// <summary>
        /// 修改描述文本
        /// </summary>
        /// <param name="originalDesc"></param>
        /// <returns></returns>
        public abstract string ChangeDescription(string originalDesc);
        /// <summary>
        /// 获取参数实例，自动调用
        /// </summary>
        /// <returns>从已知信息中获取的参数实例</returns>
        public ICESParamable TryGetParam() => GetParam();
        /// <summary>
        /// 获取参数实例
        /// </summary>
        /// <returns>从已知信息中获取的参数实例</returns>
        public abstract T GetParam();
        /// <summary>
        /// 初始化，持有者效果初始化时自动调用
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// 销毁，持有者效果销毁时自动调用
        /// </summary>
        public abstract void Destroy();
        /// <summary>
        /// 自动比较顺序
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ICESComponent other)
        {
            return SelfIndex.CompareTo(other?.SelfIndex ?? 0);
        }
    }
    /// <summary>
    /// 参数处理器基类，T为输入参数类型，U为输出参数类型，实现ICESParamProcessor接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public abstract class ParamProcessorBase<T, U> : ICESParamProcessor where T : ICESParamable where U : ICESParamable
    {
        /// <summary>
        /// 所属的SingleEffect对象
        /// </summary>
        public SingleEffect Owner { get; set; }
        /// <summary>
        /// 使用的数值参数列表
        /// </summary>
        public virtual List<float> UsingNumber => [];
        /// <summary>
        /// 是否使用数值参数
        /// </summary>
        public virtual bool IsUsingNumber { get; }
        /// <summary>
        /// 使用的数值参数数量
        /// </summary>
        public virtual int UsingNumberCount { get; }
        /// <summary>
        /// 组件自身索引
        /// </summary>
        public int SelfIndex { get; set; } = 0;
        /// <summary>
        /// 需要的参数类型
        /// </summary>
        public virtual Type RequireParamType => typeof(T);
        /// <summary>
        /// 需要的参数索引
        /// </summary>
        public int RequireParamIndex { get; set; }
        /// <summary>
        /// 提供的参数类型
        /// </summary>
        public virtual Type ProvideParamType => typeof(U);
        /// <summary>
        /// 描述处理器
        /// </summary>
        public abstract IDescribeProcessor DescribeProcessor { get; }
        /// <summary>
        /// 修改描述文本
        /// </summary>
        /// <param name="originalDesc"></param>
        /// <returns></returns>
        public abstract string ChangeDescription(string originalDesc);
        /// <summary>
        /// 处理参数，自动调用
        /// </summary>
        /// <param name="param">要处理的参数</param>
        /// <returns>处理后的参数</returns>
        public ICESParamable Process(ICESParamable param) => ProcessParam((T)param);
        /// <summary>
        /// 处理参数，将输入参数返回为新的输出参数
        /// </summary>
        /// <param name="param">要处理的参数</param>
        /// <returns>处理后的参数</returns>
        public abstract U ProcessParam(T param);
        /// <summary>
        /// 初始化，持有者效果初始化时自动调用
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// 销毁，持有者效果销毁时自动调用
        /// </summary>
        public abstract void Destroy();
        /// <summary>
        /// 自动比较顺序
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ICESComponent other)
        {
            return SelfIndex.CompareTo(other?.SelfIndex ?? 0);
        }
    }
    /// <summary>
    /// 条件基类，实现ICESCondition和ICESTargetable接口
    /// </summary>
    public abstract class ConditionBase : ICESCondition, ICESTargetable
    {
        /// <summary>
        /// 所属的SingleEffect对象
        /// </summary>
        public SingleEffect Owner { get; set; }
        /// <summary>
        /// 使用的数值参数列表
        /// </summary>
        public virtual List<float> UsingNumber => [];
        /// <summary>
        /// 是否使用数值参数
        /// </summary>
        public virtual bool IsUsingNumber { get; }
        /// <summary>
        /// 使用的数值参数数量
        /// </summary>
        public virtual int UsingNumberCount { get; }
        /// <summary>
        /// 组件自身索引
        /// </summary>
        public int SelfIndex { get; set; } = 0;
        /// <summary>
        /// 影响的组件索引
        /// </summary>
        public int AffectComponentIndex { get; set; }
        /// <summary>
        /// 需要的参数类型列表
        /// </summary>
        public virtual List<Type> RequireParamTypes { get; } = [];
        /// <summary>
        /// 需要的参数索引列表
        /// </summary>
        public List<int> RequireParamIndexes { get; } = [];
        /// <summary>
        /// 描述处理器
        /// </summary>
        public abstract IDescribeProcessor DescribeProcessor { get; }
        /// <summary>
        /// 修改描述文本
        /// </summary>
        /// <param name="originalDesc"></param>
        /// <returns></returns>
        public abstract string ChangeDescription(string originalDesc);
        /// <summary>
        /// 检查条件是否成立
        /// </summary>
        /// <param name="param">检查条件所需参数列表</param>
        /// <returns><see langword="true"/>如果满足条件</returns>
        public abstract Task<bool> Check(List<ICESParamable> param);
        /// <summary>
        /// 初始化，持有者效果初始化时自动调用
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// 销毁，持有者效果销毁时自动调用
        /// </summary>
        public abstract void Destroy();
        /// <summary>
        /// 自动比较顺序
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ICESComponent other)
        {
            return SelfIndex.CompareTo(other?.SelfIndex ?? 0);
        }
    }
    /// <summary>
    /// 目标搜索基类，T为目标类型，实现ICESTargetSearch和ICESTargetable接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TargetSearchBase<T> : ICESTargetSearch, ICESTargetable where T : ICESTargetable
    {
        /// <summary>
        /// 所属的SingleEffect对象
        /// </summary>
        public SingleEffect Owner { get; set; }
        /// <summary>
        /// 使用的数值参数列表
        /// </summary>
        public virtual List<float> UsingNumber => [];
        /// <summary>
        /// 是否使用数值参数
        /// </summary>
        public virtual bool IsUsingNumber { get; }
        /// <summary>
        /// 使用的数值参数数量
        /// </summary>
        public virtual int UsingNumberCount { get; }
        /// <summary>
        /// 组件自身索引
        /// </summary>
        public int SelfIndex { get; set; } = 0;
        /// <summary>
        /// 提供的目标类型
        /// </summary>
        public virtual Type ProvideTargetType => typeof(T);
        /// <summary>
        /// 描述处理器
        /// </summary>
        public abstract IDescribeProcessor DescribeProcessor { get; }
        /// <summary>
        /// 修改描述文本
        /// </summary>
        /// <param name="originalDesc"></param>
        /// <returns></returns>
        public abstract string ChangeDescription(string originalDesc);
        /// <summary>
        /// 获取所有目标，自动调用
        /// </summary>
        /// <returns></returns>
        public List<ICESTargetable> GetAll() => [.. GetAllTarget().OfType<ICESTargetable>()];
        /// <summary>
        /// 搜索目标，自动调用
        /// </summary>
        /// <param name="filterTarget"></param>
        /// <returns></returns>
        public async Task<List<ICESTargetable>> Search(List<ICESTargetable> filterTarget) => [.. (await SelectTarget([.. filterTarget.OfType<T>()])).OfType<ICESTargetable>()];
    
        /// <summary>
        /// 获取所有目标（泛型）
        /// </summary>
        /// <returns></returns>
        public abstract List<T> GetAllTarget();
        /// <summary>
        /// 选择目标
        /// </summary>
        /// <param name="filterTarget"></param>
        /// <returns></returns>
        public abstract Task<List<T>> SelectTarget(List<T> filterTarget);
        /// <summary>
        /// 初始化，持有者效果初始化时自动调用
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// 销毁，持有者效果销毁时自动调用
        /// </summary>
        public abstract void Destroy();
        /// <summary>
        /// 自动比较顺序
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ICESComponent other)
        {
            return SelfIndex.CompareTo(other?.SelfIndex ?? 0);
        }
    }
    /// <summary>
    /// 活动基类，实现ICESActivity接口
    /// </summary>
    public abstract class ActivityBase : ICESActivity
    {
        /// <summary>
        /// 所属的SingleEffect对象
        /// </summary>
        public SingleEffect Owner { get; set; }
        /// <summary>
        /// 使用的数值参数列表
        /// </summary>
        public virtual List<float> UsingNumber => [];
        /// <summary>
        /// 是否使用数值参数
        /// </summary>
        public virtual bool IsUsingNumber { get; }
        /// <summary>
        /// 使用的数值参数数量
        /// </summary>
        public virtual int UsingNumberCount { get; }
        /// <summary>
        /// 组件自身索引
        /// </summary>
        public int SelfIndex { get; set; } = 0;
        /// <summary>
        /// 描述处理器（活动默认无）
        /// </summary>
        public IDescribeProcessor DescribeProcessor => null;
        /// <summary>
        /// 修改描述文本
        /// </summary>
        /// <param name="originalDesc"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 活动执行主流程
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 按所需方式检查条件
        /// </summary>
        /// <param name="triggerConditions">所有要检查的条件</param>
        /// <returns><see langword="true"/>如果满足条件</returns>
        public abstract Task<bool> ConditionCheck(Dictionary<ICESCondition, List<ICESParamable>> triggerConditions);
        /// <summary>
        /// 执行效果
        /// </summary>
        /// <param name="effectActionDic">所有可执行的效果</param>
        /// <returns></returns>
        public abstract Task EffectAction(Dictionary<ICESEffect, KeyValuePair<List<ICESParamable>, List<ICESTargetable>[]>> effectActionDic);
        /// <summary>
        /// 初始化，持有者效果初始化时自动调用
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// 销毁，持有者效果销毁时自动调用
        /// </summary>
        public abstract void Destroy();
        /// <summary>
        /// 自动比较顺序
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ICESComponent other)
        {
            return SelfIndex.CompareTo(other?.SelfIndex ?? 0);
        }
    }
    /// <summary>
    /// 效果基类，实现ICESEffect和ICESTargetable接口
    /// </summary>
    public abstract class EffectBase : ICESEffect, ICESTargetable
    {
        /// <summary>
        /// 所属的SingleEffect对象
        /// </summary>
        public SingleEffect Owner { get; set; }
        /// <summary>
        /// 使用的数值参数列表
        /// </summary>
        public virtual List<float> UsingNumber => [];
        /// <summary>
        /// 是否使用数值参数
        /// </summary>
        public virtual bool IsUsingNumber { get; }
        /// <summary>
        /// 使用的数值参数数量
        /// </summary>
        public virtual int UsingNumberCount { get; }
        /// <summary>
        /// 组件自身索引
        /// </summary>
        public int SelfIndex { get; set; } = 0;
        /// <summary>
        /// 需要的参数类型列表
        /// </summary>
        public virtual List<Type> RequireParamTypes => [];
        /// <summary>
        /// 需要的参数在持有者中的索引索引列表
        /// </summary>
        public List<int> RequireParamIndexes { get; } = [];
        /// <summary>
        /// 需要的目标类型列表
        /// </summary>
        public virtual List<Type> RequireTargetTypes => [];
        /// <summary>
        /// 需要的目标索引列表
        /// </summary>
        public List<int> RequireTargetIndexes { get; } = [];
        /// <summary>
        /// 描述处理器
        /// </summary>
        public abstract IDescribeProcessor DescribeProcessor { get; }
        /// <summary>
        /// 修改描述文本
        /// </summary>
        /// <param name="originalDesc"></param>
        /// <returns></returns>
        public abstract string ChangeDescription(string originalDesc);
        /// <summary>
        /// 效果执行方法
        /// </summary>
        /// <param name="params">按<see cref="RequireParamTypes"/>中的顺序填入的参数实例</param>
        /// <param name="targets">按<see cref="RequireTargetTypes"/>中的顺序填入的对象（列表）实例</param>
        /// <returns></returns>
        public abstract Task<int> Effect(List<ICESParamable> @params, List<ICESTargetable>[] targets);
        /// <summary>
        /// 初始化，持有者效果初始化时自动调用
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// 销毁，持有者效果销毁时自动调用
        /// </summary>
        public abstract void Destroy();
        /// <summary>
        /// 自动比较顺序
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ICESComponent other)
        {
            return SelfIndex.CompareTo(other?.SelfIndex ?? 0);
        }
    }
}