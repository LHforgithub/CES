using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CES
{
    /// <summary>
    /// 表示单个效果的核心类，包含触发器、参数、条件、目标搜索、活动与子效果等组件。
    /// </summary>
    public class SingleEffect
    {
        /// <summary>
        /// 效果ID。
        /// </summary>
        public virtual int ID { get; set; } = 0;

        /// <summary>
        /// 效果的触发器组件。
        /// </summary>
        public ICESTrigger Trigger { get; set; }

        /// <summary>
        /// 自由参数列表。
        /// </summary>
        public List<ICESFreeParam> FreeParams { get; } = [];

        /// <summary>
        /// 参数处理器列表。
        /// </summary>
        public List<ICESParamProcessor> ParamProcessors { get; } = [];

        /// <summary>
        /// 目标搜索组件列表。
        /// </summary>
        public List<ICESTargetSearch> TargetSearches { get; } = [];
        /// <summary>
        /// 参数转目标组件列表
        /// </summary>
        public List<ICESParamTargetConvertor> ParamTargetConvertors { get; } = [];

        /// <summary>
        /// 条件组件列表。
        /// </summary>
        public List<ICESCondition> Conditions { get; } = [];

        /// <summary>
        /// 子效果列表。
        /// </summary>
        public List<ICESEffect> Effects { get; } = [];

        /// <summary>
        /// 活动组件。
        /// </summary>
        public ICESActivity Activity { get; set; }

        /// <summary>
        /// 所有组件的集合，便于统一遍历和查找。
        /// </summary>
        public List<ICESComponent> AllComponents => [Trigger, .. FreeParams, .. ParamProcessors, .. TargetSearches, .. ParamTargetConvertors, .. Conditions, .. Effects, Activity];

        /// <summary>
        /// 效果的拥有者。
        /// </summary>
        public virtual ICESAbilityable Owner { get; set; }

        /// <summary>
        /// 描述合成方法类型。
        /// </summary>
        public virtual IDescriptionCombiner DesciptionCombineFunction { get; set; } = null;

        /// <summary>
        /// 通过索引查找参数组件。
        /// </summary>
        /// <param name="index">参数索引</param>
        /// <returns>对应的参数组件</returns>
        public ICESComponent SearchParamComponentByIndex(int index)
        {
            if (index > -1 && index < Trigger.ProvideParamTypes.Count)
            {
                return Trigger;
            }
            return AllComponents.FirstOrDefault(x => x.SelfIndex == index);
        }

        /// <summary>
        /// 获取最终参数值。
        /// </summary>
        /// <param name="component">参数组件</param>
        /// <param name="paramIndex">参数索引</param>
        /// <returns>参数值</returns>
        public ICESParamable GetFinalParam(ICESComponent component, int paramIndex = 0)
        {
            switch (component)
            {
                case ICESTrigger trigger:
                    return trigger.ProvideParams.ElementAtOrDefault(paramIndex);
                case ICESFreeParam freeParam:
                    return freeParam.TryGetParam();
                case ICESParamProcessor paramProcessor:
                    var param = AllComponents.FirstOrDefault(x => x.SelfIndex == paramProcessor.RequireParamIndex);
                    return paramProcessor.Process(GetFinalParam(param, paramProcessor.RequireParamIndex));
                default:
                    return default;
            }
        }

        /// <summary>
        /// 触发效果，执行活动。
        /// </summary>
        public virtual async Task Triggered()
        {
            if (Activity != null)
            {
                await Activity.Action();
            }
        }

        /// <summary>
        /// 初始化所有组件。
        /// </summary>
        public virtual void Init()
        {
            Trigger?.Init();
            foreach (var freeParam in new List<ICESFreeParam>(FreeParams))
            {
                freeParam?.Init();
            }
            foreach (var paramProcessor in new List<ICESParamProcessor>(ParamProcessors))
            {
                paramProcessor?.Init();
            }
            foreach (var condition in new List<ICESCondition>(Conditions))
            {
                condition?.Init();
            }
            foreach (var targetSearch in new List<ICESTargetSearch>(TargetSearches))
            {
                targetSearch?.Init();
            }
            foreach (var paramToTarget in new List<ICESParamTargetConvertor>(ParamTargetConvertors))
            {
                paramToTarget?.Init();
            }
            Activity?.Init();
            foreach (var effect in new List<ICESEffect>(Effects))
            {
                effect?.Init();
            }
        }

        /// <summary>
        /// 销毁所有组件并清理引用。
        /// </summary>
        public virtual void Destroy()
        {
            Trigger?.Destroy();
            foreach (var freeParam in new List<ICESFreeParam>(FreeParams))
            {
                freeParam?.Destroy();
            }
            foreach (var paramProcessor in new List<ICESParamProcessor>(ParamProcessors))
            {
                paramProcessor?.Destroy();
            }
            foreach (var condition in new List<ICESCondition>(Conditions))
            {
                condition?.Destroy();
            }
            foreach (var targetSearch in new List<ICESTargetSearch>(TargetSearches))
            {
                targetSearch?.Destroy();
            }
            foreach (var paramToTarget in new List<ICESParamTargetConvertor>(ParamTargetConvertors))
            {
                paramToTarget?.Destroy();
            }
            Activity?.Destroy();
            foreach (var effect in new List<ICESEffect>(Effects))
            {
                effect?.Destroy();
            }
            Trigger = null;
            FreeParams.Clear();
            ParamProcessors.Clear();
            Conditions.Clear();
            TargetSearches.Clear();
            ParamTargetConvertors.Clear();
            Activity = null;
            Effects.Clear();
        }

        /// <summary>
        /// 返回效果的详细字符串信息。
        /// </summary>
        /// <returns>字符串信息</returns>
        public override string ToString()
        {
            return $"ID : <{ID}>\n" +
                $"Trigger : <{Trigger}>\n" +
                $"FreeParamsCount : <{FreeParams.Count}>\n" +
                $"ParamProcessorsCount : <{ParamProcessors.Count}>\n" +
                $"TargetSearchesCount : <{TargetSearches.Count}>\n" +
                $"ParamTargetConvertorsCount : {ParamTargetConvertors.Count}\n" +
                $"ConditionsCount : <{Conditions.Count}>\n" +
                $"EffectsCount : <{Effects.Count}>\n" +
                $"Activity : <{Activity}>";
        }
    }
    /// <summary>
    /// 可持有单个效果的类
    /// </summary>
    public interface ICESAbilityable
    {
        /// <summary>
        /// 持有的所有效果
        /// </summary>
        public List<SingleEffect> HandledEffects { get; }

    }
}
