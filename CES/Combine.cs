using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CES
{
    /// <summary>
    /// 组件引用关系类，用于描述组件之间的引用（参数、目标、影响目标等），
    /// 支持合并、拆分、错误检测等操作。
    /// </summary>
    public sealed class ComponentReference
    {
        /// <summary>
        /// 源组件（被引用方）
        /// </summary>
        public ICESComponent SourceComponent { get; set; }
        /// <summary>
        /// 被引用的组件
        /// </summary>
        public ICESComponent ReferenceComponent { get; set; }
        /// <summary>
        /// 同一源组件下的所有引用
        /// </summary>
        public List<ComponentReference> SameSourceReferences { get; } = [];
        /// <summary>
        /// 是否为参数引用
        /// </summary>
        public bool IsParam { get; set; } = false;
        /// <summary>
        /// 是否为目标引用
        /// </summary>
        public bool IsTarget { get; set; } = false;
        /// <summary>
        /// 是否为影响目标引用
        /// </summary>
        public bool IsAffectTarget { get; set; } = false;
        /// <summary>
        /// 触发器参数索引
        /// </summary>
        public int TriggerParamIndex { get; set; } = 0;
        /// <summary>
        /// 多参数需求索引
        /// </summary>
        public int MultipleRequireIndex { get; set; } = 0;
        /// <summary>
        /// 错误码
        /// </summary>
        public int ErrorCode { get; set; } = 0;

        /// <summary>
        /// 拆分所有与当前引用相关的引用
        /// </summary>
        /// <param name="other">要拆分的引用，默认为自身</param>
        public void SplitAll(ComponentReference other = null)
        {
            other ??= this;
            foreach (var item in new List<ComponentReference>(SameSourceReferences))
            {
                item.SpliteOther(other);
            }
        }
        /// <summary>
        /// 拆分与指定引用的关联
        /// </summary>
        /// <param name="other">要拆分的引用</param>
        public void SpliteOther(ComponentReference other)
        {
            if (SameSourceReferences.Remove(other))
            {
                other.SpliteOther(this);
            }
        }
        /// <summary>
        /// 合并与指定引用的关联
        /// </summary>
        /// <param name="other">要合并的引用</param>
        public void MergeOther(ComponentReference other)
        {
            if (SameSourceReferences.TryAdd(other))
            {
                other.MergeOther(this);
            }
        }
        /// <summary>
        /// 判断是否为同一目标引用
        /// </summary>
        /// <param name="other">要比较的引用</param>
        /// <returns>是否为同一目标</returns>
        public bool IsSameTarget(ComponentReference other)
        {
            if (other == null)
            {
                return false;
            }
            if (SourceComponent == other.SourceComponent)
            {
                if (IsAffectTarget)
                {
                    return IsAffectTarget == other.IsAffectTarget;
                }
                if (IsParam)
                {
                    return IsParam == other.IsParam && TriggerParamIndex == other.TriggerParamIndex && MultipleRequireIndex == other.MultipleRequireIndex;
                }
                if (IsTarget)
                {
                    return IsTarget == other.IsTarget && MultipleRequireIndex == other.MultipleRequireIndex;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查引用是否合法
        /// </summary>
        /// <returns>是否非法</returns>
        public bool InvalidCheck()
        {
            if (SourceComponent == null)
            {
                return true;
            }
            if (SourceComponent is not ICESParamProcessor && SourceComponent is not ICESCondition && SourceComponent is not ICESEffect
                && SourceComponent is not ICESParamTargetConvertor)
            {
                return true;
            }
            if (IsParam == true && IsTarget == true)
            {
                return true;
            }
            if (TriggerParamIndex < 0 || MultipleRequireIndex < 0)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 是否存在错误
        /// </summary>
        /// <returns>是否有错误</returns>
        public bool IsError()
        {
            return ErrorCode != 0;
        }
        /// <summary>
        /// 释放引用，清理所有关联
        /// </summary>
        public void Dispose()
        {
            SplitAll();
            SourceComponent = null;
            ReferenceComponent = null;
            SameSourceReferences.Clear();
        }
        /// <summary>
        /// 获取所有与当前引用目标相同的引用
        /// </summary>
        /// <param name="componentReferences">可选，指定查找范围</param>
        /// <returns>所有同目标引用</returns>
        public List<ComponentReference> GetAllSameTarget(IEnumerable<ComponentReference> componentReferences = null)
        {
            var result = new List<ComponentReference>();
            componentReferences ??= SameSourceReferences;
            var item = componentReferences.FirstOrDefault();
            if (item == null)
            {
                return result;
            }
            result.AddRange(componentReferences.Where(i => i.IsSameTarget(this)));
            result.AddRange(item.GetAllSameTarget(componentReferences.Where(x => x != item)));
            return result;
        }
        /// <summary>
        /// 获取引用的字符串描述
        /// </summary>
        /// <returns>描述字符串</returns>
        public override string ToString()
        {
            return $"Source : <{SourceComponent}>,\n" +
                $"Reference : <{ReferenceComponent}>,\n" +
                $"MergeCount : <{SameSourceReferences?.Count}>,\n" +
                $"IsAsParam : <{IsParam}>,\n" +
                $"IsAsTarget: <{IsTarget}>,\n" +
                $"IsAsAffectTarget: {IsAffectTarget},\n" +
                $"ParamIndex : <{TriggerParamIndex}>,\n" +
                $"ErrorCode : <{CES.ErrorCode.Code.FirstOrDefault(x => x.Key == ErrorCode)}>";
        }
    }
    /// <summary>
    /// 错误码
    /// </summary>
    public static class ErrorCode
    {
        /// <summary>
        /// 错误码字典
        /// </summary>
        public static Dictionary<int, string> Code { get; } = new()
        {
            // ProcessCombination.AddComponent
            { -1, "ProcessCombination.AddComponent: 输入的组件无效" },
            { 2, "ProcessCombination.AddComponent: 输入的自由参数类型对应实例已经包含在处理中" },
            { 3, "ProcessCombination.AddComponent: 输入的参数处理器类型实例已经包含在处理中" },
            { 4, "ProcessCombination.AddComponent: 输入的条件类型实例已经包含在处理中" },
            { 5, "ProcessCombination.AddComponent: 输入的目标搜索器类型实例已经包含在处理中" },
            { 6, "ProcessCombination.AddComponent: 输入的效果类型实例已经包含在处理中" },
            { 7, "ProcessCombination.RemoveComponent: 输入的组件类型是触发器，但不是该实例当前触发器类型实例" },
            { 8, "ProcessCombination.RemoveComponent: 输入的组件类型是自由参数，但不是该实例当前持有的参数类型实例" },
            { 9, "ProcessCombination.RemoveComponent: 输入的组件类型是参数处理器，但不是该实例当前持有的处理器类型实例" },
            { 10, "ProcessCombination.RemoveComponent: 输入的组件类型是条件，但不是该实例当前持有的条件类型实例" },
            { 11, "ProcessCombination.RemoveComponent: 输入的组件类型是目标搜索器，但不是该实例当前持有的搜索器类型实例" },
            { 12, "ProcessCombination.RemoveComponent: 输入的组件类型是效果，但不是该实例当前持有的效果类型实例" },
            { 13, "ProcessCombination.RemoveComponent: 输入的组件类型是活动，但不是该实例当前持有的活动类型实例" },
            { 14, "ProcessCombination.AddComponent: 输入的参数转目标类型实例已经包含在处理中" },
            { 15, "ProcessCombination.AddComponent: 输入的目标转参数类型实例已经包含在处理中" },
            { 16, "ProcessCombination.RemoveComponent: 输入的组件类型是参数转目标类型，但不是该实例当前持有的参数转目标类型实例" },
            { 17, "ProcessCombination.RemoveComponent: 输入的组件类型是目标转参数类型，但不是该实例当前持有的目标转参数类型实例" },

            // ProcessCombination.AddReference
            { 5001, "ProcessCombination.AddReference: 输入的影响目标组件无效" },
            { 5002, "ProcessCombination.AddReference: 输入的引用组件无效" },
            { 5003, "ProcessCombination.AddReference: 影响目标组件未加入此实例" },
            { 5004, "ProcessCombination.AddReference: 引用资源未加入此实例" },
            { 5005, "ProcessCombination.AddReference: 输入的影响目标组件不是参数处理器、条件或效果" },
            { 5006, "ProcessCombination.AddReference: IsParam和IsTarget不能同时为true" },
            { 5007, "ProcessCombination.AddReference: 引用资源提供的是触发器，但给出的触发器参数索引越界" },
            { 5008, "ProcessCombination.AddReference: 引用资源指示为影响目标，但提供的不是触发器或目标搜索器" },
            { 5009, "ProcessCombination.AddReference: 影响目标组件存在多个参数或目标需求，引用资源需要提供参数的目标需求索引" },
            { 5010, "ProcessCombination.AddReference: 现有的引用资源中已经存在相同指向的资源" },
            { 5011, "ProcessCombination.AddReference: 无意义的引用资源"},
            { 5100, "ProcessCombination.AddReference: 输入的引用资源无效" },

            // ProcessCombination.CheckResult
            { 9001, "ProcessCombination.CheckResult: 已输入的触发器无效" },
            { 9002, "ProcessCombination.CheckResult: 已输入的活动无效" },
            { -1000, "ProcessCombination.CheckResult: 存在错误的组件" },

            // ProcessCombination.CheckParamProcessor
            { 1003, "ProcessCombination.CheckParamProcessor: 该参数处理器未定义任何引用资源" },
            { 1004, "ProcessCombination.CheckParamProcessor: 该参数处理器的引用资源引用组件为空" },
            { 1005, "ProcessCombination.CheckParamProcessor: 该参数处理器的引用资源引用组件未加入此实例" },
            { 1006, "ProcessCombination.CheckParamProcessor: 该参数处理器的引用资源不是合理的参数提供器" },
            { 1007, "ProcessCombination.CheckParamProcessor: 该参数处理器的引用资源为触发器，但提供的索引越界" },
            { 1008, "ProcessCombination.CheckParamProcessor: 引用资源提供的触发器中的参数索引对应的参数类型不匹配" },
            { 1009, "ProcessCombination.CheckParamProcessor: 引用资源提供的自由参数的类型不匹配" },
            { 1010, "ProcessCombination.CheckParamProcessor: 引用资源提供的参数处理器的类型不匹配" },
            { 1011, "ProcessCombination.CheckParamProcessor: 引用资源提供的参数处理器为需求自身" },
            { 1012, "ProcessCombination.CheckParamProcessor: 引用资源提供的参数处理器所处位置在该处理器后" },
            { 1013, "ProcessCombination.CheckParamProcessor: 引用资源中存在参数处理器列表中的循环引用" },

            // ProcessCombination.CheckCondition
            { 1103, "ProcessCombination.CheckCondition: 该条件未定义任何引用资源" },
            { 1104, "ProcessCombination.CheckCondition: 该条件的引用资源引用组件为空" },
            { 1105, "ProcessCombination.CheckCondition: 该条件的引用资源引用组件未加入此实例" },
            { 1200, "ProcessCombination.CheckCondition: 该条件的引用资源不是合法的条件的参数提供器" },
            { 1301, "ProcessCombination.CheckCondition: 引用资源提供的条件的影响触发器对象提供的参数索引越界" },
            { 1302, "ProcessCombination.CheckCondition: 引用资源提供的条件的需求参数索引越界" },
            { 1303, "ProcessCombination.CheckCondition: 引用资源提供的条件的影响触发器对象提供的参数类型与需求的参数类型不匹配" },
            { 1311, "ProcessCombination.CheckCondition: 引用资源提供的条件的需求参数索引越界" },
            { 1312, "ProcessCombination.CheckCondition: 条件的该需求参数未提供索引" },
            { 1313, "ProcessCombination.CheckCondition: 条件的该需求参数索引越界" },
            { 1323, "ProcessCombination.CheckCondition: 条件的该需求参数索引指向目标搜索器，但是该目标搜索器不是当前正在处理的实例的目标搜索器" },
            { 1324, "ProcessCombination.CheckCondition: 条件的该需求参数索引指向目标处理器，但是该索引对应的条件的需求参数类型与目标处理器的提供的目标类型不匹配" },
            { 1325, "ProcessCombination.CheckCondition: 条件的该需求参数索引指向目标处理器，但是该条件的参数需求索引中没有指向该目标处理器提供的目标类型的索引" },
            { 1331, "ProcessCombination.CheckCondition: 条件的该需求参数索引指向触发器，但是该触发器不是当前实例的触发器" },
            { 1332, "ProcessCombination.CheckCondition: 条件的该需求参数索引指向触发器，但是该索引对应的条件的需求参数类型与触发器提供的参数类型不匹配" },
            { 1333, "ProcessCombination.CheckCondition: 条件的该需求参数索引指向触发器，但是索引超出该触发器提供的参数类型的数量" },
            { 1341, "ProcessCombination.CheckCondition: 条件的该需求参数索引指向自由参数，但是该自由参数不是当前实例的自由参数" },
            { 1342, "ProcessCombination.CheckCondition: 条件的该需求参数索引指向自由参数，但是该索引对应的条件的需求参数类型与自由参数提供的参数类型不匹配" },
            { 1351, "ProcessCombination.CheckCondition: 条件的该需求参数索引指向参数处理器，但是该参数处理器不是当前实例的参数处理器" },
            { 1352, "ProcessCombination.CheckCondition: 条件的该需求参数索引指向参数处理器，但是该索引对应的条件的需求参数类型与参数处理器提供的参数类型不匹配" },
            { 1361, "ProcessCombination.CheckCondition: 条件的该需求参数索引指向目标转参数器，但是该目标转参数器不是当前实例的目标转参数器" },
            { 1362, "ProcessCombination.CheckCondition: 条件的该需求参数索引指向目标转参数器，但是该索引对应的条件的需求参数类型与目标转参数器提供的参数类型不匹配" },

            // ProcessCombination.CheckEffect
            { 2005, "ProcessCombination.CheckEffect: 该效果未定义任何引用资源" },
            { 2006, "ProcessCombination.CheckEffect: 该效果的引用资源引用组件为空" },
            { 2007, "ProcessCombination.CheckEffect: 该效果的引用资源引用组件未加入此实例" },
            { 2012, "ProcessCombination.CheckEffect: 引用资源指示为目标提供但是指向的不是合法的目标搜索器" },
            { 2021, "ProcessCombination.CheckEffect: 引用资源提供的目标搜索器不是当前实例的目标搜索器" },
            { 2022, "ProcessCombination.CheckEffect: 引用资源提供的需求目标类型索引越界" },
            { 2023, "ProcessCombination.CheckEffect: 引用资源提供的目标搜索器的目标类型与需求的目标类型不匹配" },
            { 2312, "ProcessCombination.CheckEffect: 效果的该需求参数未提供索引" },
            { 2313, "ProcessCombination.CheckEffect: 效果的该需求参数索引越界" },
            { 2331, "ProcessCombination.CheckEffect: 效果的该需求参数索引指向触发器，但是该触发器不是当前实例的触发器" },
            { 2332, "ProcessCombination.CheckEffect: 效果的该需求参数索引指向触发器，但是该索引对应的效果的需求参数类型与触发器提供的参数类型不匹配" },
            { 2333, "ProcessCombination.CheckEffect: 效果的该需求参数索引指向触发器，但是索引超出该触发器提供的参数类型的数量" },
            { 2341, "ProcessCombination.CheckEffect: 效果的该需求参数索引指向自由参数，但是该自由参数不是当前实例的自由参数" },
            { 2342, "ProcessCombination.CheckEffect: 效果的该需求参数索引指向自由参数，但是该索引对应的效果的需求参数类型与自由参数提供的参数类型不匹配" },
            { 2351, "ProcessCombination.CheckEffect: 效果的该需求参数索引指向参数处理器，但是该参数处理器不是当前实例的参数处理器" },
            { 2352, "ProcessCombination.CheckEffect: 效果的该需求参数索引指向参数处理器，但是该索引对应的效果的需求参数类型与参数处理器提供的参数类型不匹配" },
            { 2361, "ProcessCombination.CheckEffect: 效果的该需求参数索引指向目标提供器，但是该目标提供器不是当前实例的目标提供器" },
            { 2362, "ProcessCombination.CheckEffect: 效果的该需求参数索引指向目标提供器，但是该索引对应的效果的需求参数类型与目标提供器提供的参数类型不匹配" },
            { 2422, "ProcessCombination.CheckEffect: 效果的该需求目标类型未提供索引" },
            { 2423, "ProcessCombination.CheckEffect: 效果的该需求目标类型索引越界" },
            { 2441, "ProcessCombination.CheckEffect: 效果的该需求目标类型索引指向目标搜索器，但是该目标搜索器不是当前实例的目标搜索器" },
            { 2442, "ProcessCombination.CheckEffect: 效果的该需求目标类型索引指向目标搜索器，但是该索引对应的效果的需求参数类型与目标搜索器提供的目标类型不匹配" },
            { 2451, "ProcessCombination.CheckEffect: 效果的该需求目标类型索引指向参数转目标器，但是该参数转目标器不是当前实例的参数转目标器" },
            { 2452, "ProcessCombination.CheckEffect: 效果的该需求目标类型索引指向参数转目标器，但是该索引对应的效果的需求参数类型与参数转目标器提供的目标类型不匹配" },

            // ProcessCombination.CheckParamTargetsConvertor
            { 3003, "ProcessCombination.CheckParamTargetsConvertor: 该参数转目标器未定义任何引用资源" },
            { 3004, "ProcessCombination.CheckParamTargetsConvertor: 该参数转目标器的引用资源引用组件为空" },
            { 3005, "ProcessCombination.CheckParamTargetsConvertor: 该参数转目标器的引用资源引用组件未加入此实例" },
            { 3006, "ProcessCombination.CheckParamTargetsConvertor: 该参数转目标器的引用资源不是合理的参数提供器" },
            { 3007, "ProcessCombination.CheckParamTargetsConvertor: 该参数处理器的引用资源为触发器，但提供的索引越界" },
            { 3008, "ProcessCombination.CheckParamTargetsConvertor: 引用资源提供的触发器中的参数索引对应的参数类型不匹配" },
            { 3009, "ProcessCombination.CheckParamTargetsConvertor: 引用资源提供的自由参数的类型不匹配" },
            { 3010, "ProcessCombination.CheckParamTargetsConvertor: 引用资源提供的参数处理器的类型不匹配" },

            // 通用
            { 9003, "ProcessCombination: 引用资源中存在相同指向的资源" },
            { 1106, "ProcessCombination: 引用资源指示为影响目标，但提供的不是触发器或目标搜索器" },
            { 1110, "ProcessCombination: 引用资源指示为参数提供但提供的不是合法的参数提供器" },
            { 1111, "ProcessCombination: 引用资源提供的触发器参数索引越界" },
            { 1112, "ProcessCombination: 引用资源提供的需求类型索引越界" },
            { 1113, "ProcessCombination: 引用资源提供的触发器参数类型与需求的参数类型不匹配" },
            { 1121, "ProcessCombination: 引用资源提供的需求类型索引越界" },
            { 1122, "ProcessCombination: 引用资源提供的自由参数类型与需求的参数类型不匹配" },
            { 1131, "ProcessCombination: 引用资源提供的需求类型索引越界" },
            { 1132, "ProcessCombination: 引用资源提供的自由参数类型与需求的参数类型不匹配" },
            { 1141, "ProcessCombination: 引用资源提供的需求类型索引越界" },
            { 1142, "ProcessCombination: 引用资源提供的自由参数类型与需求的参数类型不匹配" },
        };
    }
    /// <summary>
    /// 组合和管理各类组件（如触发器、参数、条件、目标搜索器、效果、活动等），
    /// 并维护它们之间的引用关系。支持组件的添加、移除、索引同步、引用校验、错误检测与结果生成。
    /// </summary>
    public sealed class ProcessCombination
    {
        /// <summary>
        /// 当前组合的触发器组件
        /// </summary>
        internal ICESTrigger Trigger { get; set; }
        /// <summary>
        /// 当前组合的自由参数组件列表
        /// </summary>
        internal List<ICESFreeParam> FreeParams { get; } = [];
        /// <summary>
        /// 当前组合的参数处理器组件列表
        /// </summary>
        internal List<ICESParamProcessor> ParamProcessors { get; } = [];
        /// <summary>
        /// 当前组合的目标搜索器组件列表
        /// </summary>
        internal List<ICESTargetSearch> TargetSearches { get; } = [];
        /// <summary>
        /// 当前组合的参数转目标组件列表
        /// </summary>
        internal List<ICESParamTargetConvertor> ParamTargetConvertors { get; } = [];

        /// <summary>
        /// 当前组合的条件组件列表
        /// </summary>
        internal List<ICESCondition> Conditions { get; } = [];
        /// <summary>
        /// 当前组合的效果组件列表
        /// </summary>
        internal List<ICESEffect> Effects { get; } = [];
        /// <summary>
        /// 当前组合的活动组件
        /// </summary>
        internal ActivityBase Activity { get; set; }
        /// <summary>
        /// 当前组合的所有组件引用关系
        /// </summary>
        internal List<ComponentReference> ComponentReferences { get; } = [];
        /// <summary>
        /// 当前组合的所有错误引用组件
        /// </summary>
        public List<ComponentReference> ErrorComponents { get; } = [];
        /// <summary>
        /// 检查组件是否无效（为null或已被其他组合持有）
        /// </summary>
        /// <param name="component">待检查组件</param>
        /// <returns>是否无效</returns>
        private bool InvalidCheck(ICESComponent component)
        {
            return component == null || component.Owner != null;
        }
        /// <summary>
        /// 判断当前组合是否包含指定组件
        /// </summary>
        /// <param name="component">待判断组件</param>
        /// <returns>是否包含</returns>
        private bool Contains(ICESComponent component)
        {
            switch (component)
            {
                case null:
                    return false;
                case ICESTrigger:
                    {
                        return component == Trigger;
                    }
                case ICESFreeParam:
                    {
                        return FreeParams.Contains((ICESFreeParam)component);
                    }
                case ICESParamProcessor:
                    {
                        return ParamProcessors.Contains((ICESParamProcessor)component);
                    }
                case ICESCondition:
                    {
                        return Conditions.Contains((ICESCondition)component);
                    }
                case ICESTargetSearch:
                    {
                        return TargetSearches.Contains((ICESTargetSearch)component);
                    }
                case ICESParamTargetConvertor:
                    {
                        return ParamTargetConvertors.Contains((ICESParamTargetConvertor)component);
                    }
                case ICESEffect:
                    {
                        return Effects.Contains((ICESEffect)component);
                    }
                case ICESActivity:
                    {
                        return component == Activity;
                    }
                default:
                    return false;
            }
        }
        /// <summary>
        /// 获取最后一个参数相关组件的SelfIndex
        /// </summary>
        /// <returns>最后一个参数相关组件的SelfIndex</returns>
        private int GetLastParamSelfIndex()
        {
            return (Trigger?.ProvideParamTypes.Count ?? 0) + FreeParams.Count + ParamProcessors.Count - 1;
        }
        /// <summary>
        /// 通过索引查找对应的组件
        /// </summary>
        /// <param name="index">组件索引</param>
        /// <returns>对应的组件</returns>
        private ICESComponent SearchIndexComponent(int index)
        {
            if (index < 0)
            {
                return null;
            }
            if (index < (Trigger?.ProvideParamTypes.Count ?? 0))
            {
                return Trigger;
            }
            var list = new List<ICESComponent>();
            list.AddRange(FreeParams);
            list.AddRange(ParamProcessors);
            list.AddRange(Conditions);
            list.AddRange(TargetSearches);
            list.AddRange(ParamTargetConvertors);
            list.AddRange(Effects);
            list.Add(Activity);
            return list.FirstOrDefault(x => x.SelfIndex == index);
        }
        /// <summary>
        /// 添加组件到组合中
        /// </summary>
        /// <param name="component">要添加的组件</param>
        /// <returns>错误码，0为成功</returns>
        public int AddComponent(ICESComponent component)
        {
            if (InvalidCheck(component))
            {
                //输入的组件无效
                return -1;
            }
            switch (component)
            {
                case ICESTrigger:
                    {
                        Trigger = (ICESTrigger)component;
                        break;
                    }
                case ICESFreeParam freeParam:
                    {
                        if (!FreeParams.TryAdd(freeParam))
                        {
                            //输入的自由参数类型已经存在
                            return 2;
                        }
                        break;
                    }
                case ICESParamProcessor paramProcessor:
                    {
                        if (!ParamProcessors.TryAdd(paramProcessor))
                        {
                            //输入的参数处理器类型已经存在
                            return 3;
                        }
                        break;
                    }
                case ICESCondition condition:
                    {
                        if (!Conditions.TryAdd(condition))
                        {
                            //输入的条件类型已经存在
                            return 4;
                        }
                        break;
                    }
                case ICESTargetSearch targetSearch:
                    {
                        if (!TargetSearches.TryAdd(targetSearch))
                        {
                            //输入的目标搜索器类型已经存在
                            return 5;
                        }
                        break;
                    }
                case ICESParamTargetConvertor paramTargetConvertor:
                    {
                        if (!ParamTargetConvertors.TryAdd(paramTargetConvertor))
                        {
                            //输入的参数转目标类型已经存在
                            return 14;
                        }
                        break;
                    }
                case ICESEffect effect:
                    {
                        if (!Effects.TryAdd(effect))
                        {
                            //输入的效果类型已经存在
                            return 6;
                        }
                        break;
                    }
                case ICESActivity:
                    {
                        Activity = (ActivityBase)component;
                        break;
                    }
                default:
                    break;
            }
            return 0;
        }
        /// <summary>
        /// 从组合中移除组件
        /// </summary>
        /// <param name="component">要移除的组件</param>
        /// <returns>错误码，0为成功</returns>
        public int RemoveComponent(ICESComponent component)
        {
            if (InvalidCheck(component))
            {
                //输入的组件无效
                return -1;
            }
            switch (component)
            {
                case ICESTrigger:
                    {
                        if (component == Trigger)
                        {
                            Trigger = null;
                        }
                        else
                        {
                            //输入的组件类型是触发器，但不是该实例当前触发器
                            return 7;
                        }
                        break;
                    }
                case ICESFreeParam freeParam:
                    {
                        if (!FreeParams.Remove(freeParam))
                        {
                            //输入的组件类型是自由参数，但不是该实例当前持有的参数
                            return 8;
                        }
                        break;
                    }
                case ICESParamProcessor paramProcessor:
                    {
                        if (!ParamProcessors.Remove(paramProcessor))
                        {
                            //输入的组件类型是参数处理器，但不是该实例当前持有的处理器
                            return 9;
                        }
                        break;
                    }
                case ICESCondition condition:
                    {
                        if (!Conditions.Remove(condition))
                        {
                            //输入的组件类型是条件，但不是该实例当前持有的条件
                            return 10;
                        }
                        break;
                    }
                case ICESTargetSearch targetSearch:
                    {
                        if (!TargetSearches.Remove(targetSearch))
                        {
                            //输入的组件类型是目标搜索器，但不是该实例当前持有的搜索器
                            return 11;
                        }
                        break;
                    }
                case ICESParamTargetConvertor paramTargetConvertor:
                    {
                        if (!ParamTargetConvertors.Remove(paramTargetConvertor))
                        {
                            //输入的组件类型是参数转目标类型，但不是该实例当前持有的参数转目标类型实例
                            return 16;
                        }
                        break;
                    }
                case ICESEffect effect:
                    {
                        if (!Effects.Remove(effect))
                        {
                            //输入的组件类型是效果，但不是该实例当前持有的效果
                            return 12;
                        }
                        break;
                    }
                case ICESActivity:
                    {
                        if (component == Activity)
                        {
                            Activity = null;
                        }
                        else
                        {
                            //输入的组件类型是活动，但不是该实例当前持有的活动
                            return 13;
                        }
                        break;
                    }
                default:
                    break;
            }
            return 0;
        }
        /// <summary>
        /// 同步所有组件的SelfIndex索引
        /// </summary>
        /// <returns>错误码，0为成功</returns>
        public int SelfIndexReflect()
        {
            if (InvalidCheck(Trigger))
            {
                return 101;
            }
            if (InvalidCheck(Activity))
            {
                return 102;
            }
            var count = 0;
            Trigger.SelfIndex = 0;
            count += Trigger.ProvideParamTypes.Count;
            FreeParams.RemoveAll(x => x == null);
            for (int i = 0; i < FreeParams.Count; i++)
            {
                FreeParams[i].SelfIndex = count;
                count++;
            }
            ParamProcessors.RemoveAll(x => x == null);
            for (int i = 0; i < ParamProcessors.Count; i++)
            {
                ParamProcessors[i].SelfIndex = count;
                count++;
            }
            TargetSearches.RemoveAll(x => x == null);
            for (int i = 0; i < TargetSearches.Count; i++)
            {
                TargetSearches[i].SelfIndex = count;
                count++;
            }
            ParamTargetConvertors.RemoveAll(x => x == null);
            for (int i = 0; i < ParamTargetConvertors.Count; i++)
            {
                ParamTargetConvertors[i].SelfIndex = count;
                count++;
            }
            Conditions.RemoveAll(x => x == null);
            for (int i = 0; i < Conditions.Count; i++)
            {
                Conditions[i].SelfIndex = count;
                count++;
            }
            Effects.RemoveAll(x => x == null);
            for (int i = 0; i < Effects.Count; i++)
            {
                Effects[i].SelfIndex = count;
                count++;
            }
            Activity.SelfIndex = count;
            return 0;
        }
        /// <summary>
        /// 检查并移除无效的引用关系
        /// </summary>
        public void ReferenceReflect()
        {
            ComponentReferences.RemoveAll(x => x == null);
            var list = ComponentReferences.FindAll(x => x.InvalidCheck()
                || (x.SourceComponent != null && !Contains(x.SourceComponent)));
            foreach (var item in list)
            {
                item.SplitAll();
                item.Dispose();
            }
        }
        /// <summary>
        /// 检查引用关系是否合法
        /// </summary>
        /// <param name="sourceComponent">源组件</param>
        /// <param name="refereceComponent">被引用组件</param>
        /// <param name="IsAsParam">是否为参数引用</param>
        /// <param name="IsAsTarget">是否为目标引用</param>
        /// <param name="IsAsAffectTarget">是否为影响目标引用</param>
        /// <param name="useTriggerParamIndex">触发器参数索引</param>
        /// <param name="useMultipleRequireIndex">多参数需求索引</param>
        /// <returns>错误码，0为合法</returns>
        public int CheckReference(ICESComponent sourceComponent, ICESComponent refereceComponent, bool IsAsParam, bool IsAsTarget, bool IsAsAffectTarget, int useMultipleRequireIndex, int useTriggerParamIndex)
        {
            if (InvalidCheck(sourceComponent))
            {
                //输入的影响目标组件无效
                return 5001;
            }
            if (InvalidCheck(refereceComponent))
            {
                //输入的引用组件无效
                return 5002;
            }
            if (!Contains(sourceComponent))
            {
                //影响目标组件未加入此实例
                return 5003;
            }
            if (!Contains(refereceComponent))
            {
                //引用资源未加入此实例
                return 5004;
            }
            if (sourceComponent is not ICESParamProcessor && sourceComponent is not ICESCondition && sourceComponent is not ICESEffect
                && sourceComponent is not ICESParamTargetConvertor)
            {
                //输入的影响目标组件不是参数处理器、条件或效果
                return 5005;
            }
            if (IsAsParam == true)
            {
                if (IsAsTarget == true)
                {
                    //IsParam和IsTarget不能同时为true
                    return 5006;
                }
                if (refereceComponent is ICESTrigger tr)
                {
                    if (useTriggerParamIndex < 0 || useTriggerParamIndex > tr.ProvideParamTypes.Count)
                    {
                        //引用资源提供的是触发器，但给出的触发器参数索引越界
                        return 5007;
                    }
                }
            }
            if (IsAsAffectTarget && refereceComponent is not ICESTrigger && refereceComponent is not ICESTargetSearch)
            {
                //引用资源指示为影响目标，但提供的不是触发器或目标搜索器
                return 5008;
            }
            if ((IsAsParam || IsAsTarget) && (sourceComponent is ICESCondition || sourceComponent is ICESEffect) && useMultipleRequireIndex < 0)
            {
                //影响目标组件存在多个参数或目标需求，引用资源需要提供参数的目标需求索引
                return 5009;
            }
            if (!IsAsParam && !IsAsTarget && !IsAsAffectTarget)
            {
                //无意义的引用资源
                return 5011;
            }
            return 0;
        }
        /// <summary>
        /// 检查引用关系是否合法（重载，直接传入引用对象）
        /// </summary>
        /// <param name="reference">引用对象</param>
        /// <returns>错误码，0为合法</returns>
        public int CheckReference(ComponentReference reference)
        {
            return CheckReference(reference.SourceComponent, reference.ReferenceComponent, reference.IsParam, reference.IsTarget, reference.IsAffectTarget, reference.TriggerParamIndex, reference.MultipleRequireIndex);
        }
        /// <summary>
        /// 添加引用关系
        /// </summary>
        /// <param name="sourceComponent">源组件</param>
        /// <param name="refereceComponent">被引用组件</param>
        /// <param name="IsAsParam">是否为参数引用</param>
        /// <param name="IsAsTarget">是否为目标引用</param>
        /// <param name="IsAsAffectTarget">是否为影响目标引用</param>
        /// <param name="useTriggerParamIndex">触发器参数索引</param>
        /// <param name="useMultipleRequireIndex">多参数需求索引</param>
        /// <returns>错误码，0为成功</returns>
        public int AddReference(ICESComponent sourceComponent, ICESComponent refereceComponent, bool IsAsParam, bool IsAsTarget, bool IsAsAffectTarget, int useMultipleRequireIndex, int useTriggerParamIndex)
        {
            var result = CheckReference(sourceComponent, refereceComponent, IsAsParam, IsAsTarget, IsAsAffectTarget, useTriggerParamIndex, useMultipleRequireIndex);
            if (result != 0)
            {
                return result;
            }
            if (useTriggerParamIndex < 0)
            {
                useTriggerParamIndex = 0;
            }
            if (useMultipleRequireIndex < 0)
            {
                useMultipleRequireIndex = 0;
            }
            var reference = new ComponentReference()
            {
                SourceComponent = sourceComponent,
                ReferenceComponent = refereceComponent,
                IsParam = IsAsParam,
                IsTarget = IsAsTarget,
                IsAffectTarget = IsAsAffectTarget,
                TriggerParamIndex = useTriggerParamIndex,
                MultipleRequireIndex = useMultipleRequireIndex
            };
            var last = ComponentReferences.FirstOrDefault(x => x.SourceComponent == sourceComponent);
            if (last == null)
            {
                ComponentReferences.Add(reference);
            }
            else
            {
                foreach (var item in new List<ComponentReference>([last, .. last.SameSourceReferences]))
                {
                    if (item.IsSameTarget(reference))
                    {
                        //现有的引用资源中已经存在相同指向的资源
                        return 5010;
                    }
                }
                last.MergeOther(reference);
            }
            return 0;
        }
        /// <summary>
        /// 添加引用关系（重载，直接传入引用对象）
        /// </summary>
        /// <param name="reference">引用对象</param>
        /// <returns>错误码，0为成功</returns>
        public int AddReference(ComponentReference reference)
        {
            if (reference == null || reference.InvalidCheck() || reference.IsError())
            {
                //输入的引用资源无效
                return 5100;
            }
            var last = ComponentReferences.FirstOrDefault(x => x.SourceComponent == reference.SourceComponent);
            if (last == null)
            {
                ComponentReferences.Add(reference);
            }
            else
            {
                foreach (var item in new List<ComponentReference>([last, .. last.SameSourceReferences]))
                {
                    if (item.IsSameTarget(reference))
                    {
                        //现有的引用资源中已经存在相同指向的资源
                        return 5010;
                    }
                }
                last.MergeOther(reference);
            }
            return 0;
        }
        /// <summary>
        /// 移除引用关系
        /// </summary>
        /// <param name="sourceComponent">源组件</param>
        /// <param name="refereceComponent">被引用组件</param>
        /// <param name="IsParam">是否为参数引用</param>
        /// <param name="IsTarget">是否为目标引用</param>
        /// <param name="IsAffectTarget">是否为影响目标引用</param>
        /// <param name="MultipleRequireIndex">多参数需求索引</param>
        /// <param name="TriggerParamIndex">触发器参数索引</param>
        /// <returns>错误码，0为成功</returns>
        public int RemoveReference(ICESComponent sourceComponent, ICESComponent refereceComponent, bool IsParam, bool IsTarget, bool IsAffectTarget, int MultipleRequireIndex, int TriggerParamIndex)
        {
            var result = CheckReference(sourceComponent, refereceComponent, IsParam, IsTarget, IsAffectTarget, TriggerParamIndex, MultipleRequireIndex);
            if (result != 0)
            {
                return result;
            }
            var reference = new ComponentReference()
            {
                SourceComponent = sourceComponent,
                ReferenceComponent = refereceComponent,
                IsParam = IsParam,
                IsTarget = IsTarget,
                IsAffectTarget = IsAffectTarget,
                TriggerParamIndex = TriggerParamIndex,
                MultipleRequireIndex = MultipleRequireIndex
            };
            var list = ComponentReferences.FindAll(x => x.IsSameTarget(reference));
            foreach (var item in list)
            {
                item.Dispose();
                ComponentReferences.Remove(item);
            }
            return 0;
        }
        /// <summary>
        /// 移除引用关系（重载，直接传入引用对象）
        /// </summary>
        /// <param name="reference">引用对象</param>
        /// <returns>错误码，0为成功</returns>
        public int RemoveReference(ComponentReference reference)
        {
            if (reference == null || reference.InvalidCheck() || reference.IsError())
            {
                //输入的引用资源无效
                return 5100;
            }
            var list = ComponentReferences.FindAll(x => x.IsSameTarget(reference));
            foreach (var item in list)
            {
                item.Dispose();
                ComponentReferences.Remove(item);
            }
            return 0;
        }
        /// <summary>
        /// 检查参数处理器的引用关系是否合法，并记录所有错误引用到ErrorComponents。
        /// </summary>
        /// <param name="paramProcessor">待检查的参数处理器</param>
        private void CheckParamProcessor(ICESParamProcessor paramProcessor)
        {
            if (ComponentReferences.FirstOrDefault(x => x.SourceComponent == paramProcessor) is ComponentReference reference)
            {
                var sameTL = reference.GetAllSameTarget();
                if (sameTL.Count > 0)
                {
                    foreach (var st in sameTL)
                    {
                        //引用资源中存在相同指向的资源
                        st.ErrorCode = 9003;
                        ErrorComponents.Add(st);
                    }
                    return;
                }
                if (reference.ReferenceComponent == null)
                {
                    //该参数处理器的引用资源引用组件为空
                    reference.ErrorCode = 1004;
                    ErrorComponents.Add(reference);
                    return;
                }
                if (!Contains(reference.ReferenceComponent))
                {
                    //该参数处理器的引用资源引用组件未加入此实例
                    reference.ErrorCode = 1005;
                    ErrorComponents.Add(reference);
                    return;
                }
                switch (reference.ReferenceComponent)
                {
                    case ICESTrigger tri:
                        {
                            if (tri.ProvideParamTypes.ElementAtOrDefault(reference.TriggerParamIndex) is Type type)
                            {
                                if (paramProcessor.RequireParamType.IsInheritedBy(type))
                                {
                                    paramProcessor.RequireParamIndex = reference.TriggerParamIndex;
                                    return;
                                }
                                else
                                {
                                    //引用资源提供的触发器中的参数索引对应的参数类型不匹配
                                    reference.ErrorCode = 1008;
                                    ErrorComponents.Add(reference);
                                    return;
                                }
                            }
                            else
                            {
                                //该参数处理器的引用资源为触发器，但提供的索引越界
                                reference.ErrorCode = 1007;
                                ErrorComponents.Add(reference);
                                return;
                            }
                        }
                    case ICESFreeParam freeParam:
                        {
                            if (paramProcessor.RequireParamType.IsInheritedBy(freeParam.ProvideParamType))
                            {
                                paramProcessor.RequireParamIndex = freeParam.SelfIndex;
                                return;
                            }
                            else
                            {
                                //引用资提供的自由参数的类型不匹配
                                reference.ErrorCode = 1009;
                                ErrorComponents.Add(reference);
                                return;
                            }
                        }
                    case ICESParamProcessor pp:
                        {
                            bool LoopingReference(ICESParamProcessor pp, List<ICESParamProcessor> node)
                            {
                                var @ref = ComponentReferences.FirstOrDefault(x => x.SourceComponent == pp);
                                if (@ref == null || @ref.ReferenceComponent is not ICESParamProcessor)
                                {
                                    return false;
                                }
                                if (node.Contains(@ref.ReferenceComponent))
                                {
                                    return true;
                                }
                                node.Add(@ref.ReferenceComponent as ICESParamProcessor);
                                return LoopingReference(@ref.ReferenceComponent as ICESParamProcessor, node);
                            }
                            if (pp.SelfIndex < pp.SelfIndex)
                            {
                                if (LoopingReference(pp, []))
                                {
                                    //引用资源中存在参数处理器列表中的循环引用
                                    reference.ErrorCode = 1013;
                                    ErrorComponents.Add(reference);
                                    return;
                                }
                                if (pp.RequireParamType.IsInheritedBy(pp.ProvideParamType))
                                {
                                    paramProcessor.RequireParamIndex = pp.SelfIndex;
                                    return;
                                }
                                else
                                {
                                    //引用资源提供的参数处理器的类型不匹配
                                    reference.ErrorCode = 1010;
                                    ErrorComponents.Add(reference);
                                    return;
                                }
                            }
                            else if (pp.SelfIndex == paramProcessor.SelfIndex)
                            {
                                //引用资源提供的参数处理器为需求自身
                                reference.ErrorCode = 1011;
                                ErrorComponents.Add(reference);
                                return;
                            }
                            else
                            {
                                //引用资源提供的参数处理器所处位置在该处理器后
                                reference.ErrorCode = 1012;
                                ErrorComponents.Add(reference);
                                return;
                            }
                        }
                    default:
                        {
                            //该参数处理器的引用资源不是合理的参数提供器
                            reference.ErrorCode = 1006;
                            ErrorComponents.Add(reference);
                            return;
                        }
                }
            }
            else
            {
                //该参数处理器未定义任何引用资源
                ErrorComponents.Add(new ComponentReference() { SourceComponent = paramProcessor, ErrorCode = 1003 });
                return;
            }
        }
        
        private void CheckParamTargetsConvertor(ICESParamTargetConvertor paramTargetConvertor)
        {
            if (ComponentReferences.FirstOrDefault(x => x.SourceComponent == paramTargetConvertor) is ComponentReference reference)
            {
                var sameTL = reference.GetAllSameTarget();
                if (sameTL.Count > 0)
                {
                    foreach (var st in sameTL)
                    {
                        //引用资源中存在相同指向的资源
                        st.ErrorCode = 9003;
                        ErrorComponents.Add(st);
                    }
                    return;
                }
                if (reference.ReferenceComponent == null)
                {
                    //该参数转目标器的引用资源引用组件为空
                    reference.ErrorCode = 3004;
                    ErrorComponents.Add(reference);
                    return;
                }
                if (!Contains(reference.ReferenceComponent))
                {
                    //该参数转目标器的引用资源引用组件未加入此实例
                    reference.ErrorCode = 3005;
                    ErrorComponents.Add(reference);
                    return;
                }
                switch (reference.ReferenceComponent)
                {
                    case ICESTrigger tri:
                        {
                            if (tri.ProvideParamTypes.ElementAtOrDefault(reference.TriggerParamIndex) is Type type)
                            {
                                if (paramTargetConvertor.RequireParamType.IsInheritedBy(type))
                                {
                                    paramTargetConvertor.RequireParamIndex = reference.TriggerParamIndex;
                                    return;
                                }
                                else
                                {
                                    //引用资源提供的触发器中的参数索引对应的参数类型不匹配
                                    reference.ErrorCode = 3008;
                                    ErrorComponents.Add(reference);
                                    return;
                                }
                            }
                            else
                            {
                                //该参数转目标器的引用资源为触发器，但提供的索引越界
                                reference.ErrorCode = 3007;
                                ErrorComponents.Add(reference);
                                return;
                            }
                        }
                    case ICESFreeParam freeParam:
                        {
                            if (paramTargetConvertor.RequireParamType.IsInheritedBy(freeParam.ProvideParamType))
                            {
                                paramTargetConvertor.RequireParamIndex = freeParam.SelfIndex;
                                return;
                            }
                            else
                            {
                                //引用资源提供的参数处理器提供的类型不匹配
                                reference.ErrorCode = 3009;
                                ErrorComponents.Add(reference);
                                return;
                            }
                        }
                    case ICESParamProcessor pp:
                        {
                            if (paramTargetConvertor.RequireParamType.IsInheritedBy(pp.ProvideParamType))
                            {
                                paramTargetConvertor.RequireParamIndex = pp.SelfIndex;
                                return;
                            }
                            else
                            {
                                //引用资提供的参数处理器提供的类型不匹配
                                reference.ErrorCode = 3010;
                                ErrorComponents.Add(reference);
                                return;
                            }
                        }
                    default:
                        {
                            //该参数转目标器的引用资源不是合理的参数提供器
                            reference.ErrorCode = 3006;
                            ErrorComponents.Add(reference);
                            return;
                        }
                }
            }
            else
            {
                //该参数转目标器未定义任何引用资源
                ErrorComponents.Add(new ComponentReference() { SourceComponent = paramTargetConvertor, ErrorCode = 3003 });
                return;
            }
        }

        /// <summary>
        /// 检查条件组件的引用关系是否合法，并记录所有错误引用到ErrorComponents。
        /// </summary>
        /// <param name="condition">待检查的条件组件</param>
        private void CheckCondition(ICESCondition condition)
        {
            if (ComponentReferences.FirstOrDefault(x => x.SourceComponent == condition) is ComponentReference reference)
            {
                var refList = reference.SameSourceReferences;
                var sameTL = reference.GetAllSameTarget();
                if (sameTL.Count > 0)
                {
                    foreach (var st in sameTL)
                    {
                        //引用资源中存在相同指向的资源
                        st.ErrorCode = 9003;
                        ErrorComponents.Add(st);
                    }
                    return;
                }
                foreach (var @ref in new List<ComponentReference>([reference, .. reference.SameSourceReferences]).Distinct())
                {
                    if (@ref.ReferenceComponent == null)
                    {
                        //该条件的引用资源引用组件为空
                        @ref.ErrorCode = 1104;
                        ErrorComponents.Add(@ref);
                        continue;
                    }
                    if (!Contains(@ref.ReferenceComponent))
                    {
                        //该条件的引用资源引用组件未加入此实例
                        @ref.ErrorCode = 1105;
                        ErrorComponents.Add(@ref);
                        continue;
                    }
                    if (@ref.IsAffectTarget)
                    {
                        if (@ref.ReferenceComponent is ICESTrigger tg)
                        {
                            if (!@ref.IsParam)
                            {
                                continue;
                            }
                            var type = tg.ProvideParamTypes.ElementAtOrDefault(@ref.TriggerParamIndex);
                            var type2 = condition.RequireParamTypes.ElementAtOrDefault(@ref.MultipleRequireIndex);
                            if (type == null)
                            {
                                //引用资源提供的条件的影响触发器对象提供的参数索引越界
                                @ref.ErrorCode = 1301;
                                ErrorComponents.Add(@ref);
                                continue;
                            }
                            if (type2 == null)
                            {
                                //引用资源提供的条件的需求参数索引越界
                                @ref.ErrorCode = 1302;
                                ErrorComponents.Add(@ref);
                                continue;
                            }
                            if (!type2.IsInheritedBy(type))
                            {
                                //引用资源提供的条件的影响触发器对象提供的参数类型与需求的参数类型不匹配
                                @ref.ErrorCode = 1303;
                                ErrorComponents.Add(@ref);
                                continue;
                            }
                            condition.AffectComponentIndex = @ref.ReferenceComponent.SelfIndex;
                            condition.RequireParamIndexes.InsertOrUpdateAt(@ref.MultipleRequireIndex, @ref.TriggerParamIndex);
                            continue;
                        }
                        if (@ref.ReferenceComponent is ICESTargetSearch ts)
                        {
                            var type = condition.RequireParamTypes.ElementAtOrDefault(@ref.MultipleRequireIndex);
                            if (type == null)
                            {
                                //引用资源提供的条件的需求参数索引越界
                                @ref.ErrorCode = 1311;
                                ErrorComponents.Add(@ref);
                                continue;
                            }
                            if (!type.IsInheritedBy(ts.ProvideTargetType))
                            {
                                //引用资源提供的索引对应的条件的需求参数与引用资源提供的目标搜索器的目标类型不匹配
                                @ref.ErrorCode = 1312;
                                ErrorComponents.Add(@ref);
                                continue;
                            }
                            condition.AffectComponentIndex = @ref.ReferenceComponent.SelfIndex;
                            condition.RequireParamIndexes.InsertOrUpdateAt(@ref.MultipleRequireIndex, @ref.ReferenceComponent.SelfIndex);
                            continue;
                        }
                        //引用资源指示为影响目标，但提供的不是触发器或目标搜索器
                        @ref.ErrorCode = 1106;
                        ErrorComponents.Add(@ref);
                        continue;
                    }
                    if (@ref.IsParam)
                    {
                        switch (@ref.ReferenceComponent)
                        {
                            case ICESTrigger tri:
                                {
                                    var type = tri.ProvideParamTypes.ElementAtOrDefault(@ref.TriggerParamIndex);
                                    var type2 = condition.RequireParamTypes.ElementAtOrDefault(@ref.MultipleRequireIndex);
                                    if (type == null)
                                    {
                                        //引用资源提供的触发器参数索引越界
                                        @ref.ErrorCode = 1111;
                                        ErrorComponents.Add(@ref);
                                        continue;
                                    }
                                    if (type2 == null)
                                    {
                                        //引用资源提供的需求类型索引越界
                                        @ref.ErrorCode = 1112;
                                        ErrorComponents.Add(@ref);
                                        continue;
                                    }
                                    if (!type2.IsInheritedBy(type))
                                    {
                                        //引用资源提供的触发器参数类型与需求的参数类型不匹配
                                        @ref.ErrorCode = 1113;
                                        ErrorComponents.Add(@ref);
                                        continue;
                                    }
                                    condition.RequireParamIndexes.InsertOrUpdateAt(@ref.MultipleRequireIndex, @ref.TriggerParamIndex);
                                    continue;
                                }
                            case ICESFreeParam fp:
                                {
                                    var type = condition.RequireParamTypes.ElementAtOrDefault(@ref.MultipleRequireIndex);
                                    if (type == null)
                                    {
                                        //引用资源提供的需求类型索引越界
                                        @ref.ErrorCode = 1121;
                                        ErrorComponents.Add(@ref);
                                        continue;
                                    }
                                    if (!type.IsInheritedBy(fp.ProvideParamType))
                                    {
                                        //引用资源提供的自由参数类型与需求的参数类型不匹配
                                        @ref.ErrorCode = 1122;
                                        ErrorComponents.Add(@ref);
                                        continue;
                                    }
                                    condition.RequireParamIndexes.InsertOrUpdateAt(@ref.MultipleRequireIndex, fp.SelfIndex);
                                    continue;
                                }
                            case ICESParamProcessor pp:
                                {
                                    var type = condition.RequireParamTypes.ElementAtOrDefault(@ref.MultipleRequireIndex);
                                    if (type == null)
                                    {
                                        //引用资源提供的需求类型索引越界
                                        @ref.ErrorCode = 1131;
                                        ErrorComponents.Add(@ref);
                                        continue;
                                    }
                                    if (!type.IsInheritedBy(pp.ProvideParamType))
                                    {
                                        //引用资源提供的自由参数类型与需求的参数类型不匹配
                                        @ref.ErrorCode = 1132;
                                        ErrorComponents.Add(@ref);
                                        continue;
                                    }
                                    condition.RequireParamIndexes.InsertOrUpdateAt(@ref.MultipleRequireIndex, pp.SelfIndex);
                                    continue;
                                }
                            default:
                                {
                                    //引用资源指示为参数提供但提供的不是合法的参数提供器
                                    @ref.ErrorCode = 1110;
                                    ErrorComponents.Add(@ref);
                                    continue;
                                }
                        }
                    }
                }
                //总检查
                var affectC = SearchIndexComponent(condition.AffectComponentIndex);
                var count = GetLastParamSelfIndex() + 1;
                if (affectC == null || (affectC is not ICESTrigger && affectC is not ICESTargetSearch))
                {
                    //该条件的影响组件未提供或不是合法的影响组件
                    ErrorComponents.Add(new ComponentReference() { SourceComponent = condition, ReferenceComponent = affectC, IsAffectTarget = true, ErrorCode = 1301 });
                }
                else if (affectC is ICESTargetSearch paramCTS)
                {
                    if (!TargetSearches.Contains(paramCTS))
                    {
                        //条件的该需求参数索引指向目标搜索器，但是该目标搜索器不是当前实例的目标搜索器
                        ErrorComponents.Add(new ComponentReference()
                        { SourceComponent = condition, ReferenceComponent = paramCTS, IsParam = true, ErrorCode = 1323 });
                    }
                    if (condition.RequireParamIndexes.IndexOf(count) is int i && i != 0)
                    {
                        if (!condition.RequireParamTypes[i].IsInheritedBy(paramCTS.ProvideTargetType))
                        {
                            //条件的该需求参数索引指向目标处理器，但是该索引对应的条件的需求参数类型与目标处理器的提供的目标类型不匹配
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = condition, ReferenceComponent = paramCTS, IsParam = true, ErrorCode = 1324 });
                        }
                    }
                    else
                    {
                        //条件的该需求参数索引指向目标处理器，但是该条件的参数需求索引中没有指向该目标处理器提供的目标类型的索引
                        ErrorComponents.Add(new ComponentReference()
                        { SourceComponent = condition, ReferenceComponent = paramCTS, IsParam = true, ErrorCode = 1325 });
                    }
                }
                for (int j = 0; j < condition.RequireParamTypes.Count; j++)
                {
                    if (j >= condition.RequireParamIndexes.Count)
                    {
                        //条件的该需求参数未提供索引
                        ErrorComponents.Add(new ComponentReference() { SourceComponent = condition, IsParam = true, ErrorCode = 1312 });
                        continue;
                    }
                    var paramIndex = condition.RequireParamIndexes[j];
                    if (paramIndex == count)
                    {
                        continue;
                    }
                    var paramC = SearchIndexComponent(paramIndex);
                    if (paramC == null)
                    {
                        //条件的该需求参数索引越界
                        ErrorComponents.Add(new ComponentReference() { SourceComponent = condition, ErrorCode = 1313 });
                        continue;
                    }
                    var type = condition.RequireParamTypes[j];
                    if (paramC is ICESTrigger tg)
                    {
                        if (tg != Trigger)
                        {
                            //条件的该需求参数索引指向触发器，但是该触发器不是当前实例的触发器
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = condition, ReferenceComponent = tg, IsParam = true, TriggerParamIndex = paramIndex, MultipleRequireIndex = paramIndex, ErrorCode = 1331 });
                        }
                        if (paramIndex < tg.ProvideParamTypes.Count)
                        {
                            if (!type.IsInheritedBy(tg.ProvideParamTypes[paramIndex]))
                            {
                                //条件的该需求参数索引指向触发器，但是该索引对应的条件的需求参数类型与触发器提供的参数类型不匹配
                                ErrorComponents.Add(new ComponentReference()
                                { SourceComponent = condition, ReferenceComponent = tg, IsParam = true, TriggerParamIndex = paramIndex, MultipleRequireIndex = paramIndex, ErrorCode = 1332 });
                            }
                        }
                        else
                        {
                            //条件的该需求参数索引指向触发器，但是索引超出该触发器提供的参数类型的数量
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = condition, ReferenceComponent = tg, IsParam = true, TriggerParamIndex = paramIndex, MultipleRequireIndex = paramIndex, ErrorCode = 1333 });
                        }
                        continue;
                    }
                    if (paramC is ICESFreeParam fp)
                    {
                        if (!FreeParams.Contains(fp))
                        {
                            //条件的该需求参数索引指向自由参数，但是该自由参数不是当前实例的自由参数
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = condition, ReferenceComponent = fp, IsParam = true, ErrorCode = 1341 });
                        }
                        if (!type.IsInheritedBy(fp.ProvideParamType))
                        {
                            //条件的该需求参数索引指向自由参数，但是该索引对应的条件的需求参数类型与自由参数提供的参数类型不匹配
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = condition, ReferenceComponent = fp, IsParam = true, ErrorCode = 1342 });
                        }
                        continue;
                    }
                    if (paramC is ICESParamProcessor pp)
                    {
                        if (!ParamProcessors.Contains(pp))
                        {
                            //条件的该需求参数索引指向参数处理器，但是该参数处理器不是当前实例的参数处理器
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = condition, ReferenceComponent = pp, IsParam = true, ErrorCode = 1351 });
                        }
                        if (!type.IsInheritedBy(pp.ProvideParamType))
                        {
                            //条件的该需求参数索引指向参数处理器，但是该索引对应的条件的需求参数类型与参数处理器提供的参数类型不匹配
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = condition, ReferenceComponent = pp, IsParam = true, ErrorCode = 1352 });
                        }
                        continue;
                    }
                }
            }
            else
            {
                //该条件未定义任何引用资源
                ErrorComponents.Add(new ComponentReference() { SourceComponent = condition, ErrorCode = 1103 });
                return;
            }
        }
        /// <summary>
        /// 检查效果组件的引用关系是否合法，并记录所有错误引用到ErrorComponents。
        /// </summary>
        /// <param name="effect">待检查的效果组件</param>
        private void CheckEffect(ICESEffect effect)
        {
            if (effect == null)
            {
                return;
            }
            if (ComponentReferences.FirstOrDefault(x => x.SourceComponent == effect) is ComponentReference reference)
            {
                var refList = reference.SameSourceReferences;
                var sameTL = reference.GetAllSameTarget();
                if (sameTL.Count > 0)
                {
                    foreach (var st in sameTL)
                    {
                        //引用资源中存在相同指向的资源
                        st.ErrorCode = 9003;
                        ErrorComponents.Add(st);
                    }
                    return;
                }
                foreach (var @ref in new List<ComponentReference>([reference, .. reference.SameSourceReferences]).Distinct())
                {
                    if (@ref.ReferenceComponent == null)
                    {
                        //该效果的引用资源引用组件为空
                        @ref.ErrorCode = 2006;
                        ErrorComponents.Add(@ref);
                        continue;
                    }
                    if (!Contains(@ref.ReferenceComponent))
                    {
                        //该效果的引用资源引用组件未加入此实例
                        @ref.ErrorCode = 2007;
                        ErrorComponents.Add(@ref);
                        continue;
                    }
                    if (@ref.IsParam)
                    {
                        switch (@ref.ReferenceComponent)
                        {
                            case ICESTrigger tri:
                                {
                                    var type = tri.ProvideParamTypes.ElementAtOrDefault(@ref.TriggerParamIndex);
                                    var type2 = effect.RequireParamTypes.ElementAtOrDefault(@ref.MultipleRequireIndex);
                                    if (type == null)
                                    {
                                        //引用资源提供的触发器参数索引越界
                                        @ref.ErrorCode = 1111;
                                        ErrorComponents.Add(@ref);
                                        continue;
                                    }
                                    if (type2 == null)
                                    {
                                        //引用资源提供的需求类型索引越界
                                        @ref.ErrorCode = 1112;
                                        ErrorComponents.Add(@ref);
                                        continue;
                                    }
                                    if (!type2.IsInheritedBy(type))
                                    {
                                        //引用资源提供的触发器参数类型与需求的参数类型不匹配
                                        @ref.ErrorCode = 1113;
                                        ErrorComponents.Add(@ref);
                                        continue;
                                    }
                                    effect.RequireParamIndexes.InsertOrUpdateAt(@ref.MultipleRequireIndex, @ref.TriggerParamIndex);
                                    continue;
                                }
                            case ICESFreeParam fp:
                                {
                                    var type = effect.RequireParamTypes.ElementAtOrDefault(@ref.MultipleRequireIndex);
                                    if (type == null)
                                    {
                                        //引用资源提供的需求类型索引越界
                                        @ref.ErrorCode = 1121;
                                        ErrorComponents.Add(@ref);
                                        continue;
                                    }
                                    if (!type.IsInheritedBy(fp.ProvideParamType))
                                    {
                                        //引用资源提供的自由参数类型与需求的参数类型不匹配
                                        @ref.ErrorCode = 1122;
                                        ErrorComponents.Add(@ref);
                                        continue;
                                    }
                                    effect.RequireParamIndexes.InsertOrUpdateAt(@ref.MultipleRequireIndex, fp.SelfIndex);
                                    continue;
                                }
                            case ICESParamProcessor pp:
                                {
                                    var type = effect.RequireParamTypes.ElementAtOrDefault(@ref.MultipleRequireIndex);
                                    if (type == null)
                                    {
                                        //引用资源提供的需求类型索引越界
                                        @ref.ErrorCode = 1131;
                                        ErrorComponents.Add(@ref);
                                        continue;
                                    }
                                    if (!type.IsInheritedBy(pp.ProvideParamType))
                                    {
                                        //引用资源提供的自由参数类型与需求的参数类型不匹配
                                        @ref.ErrorCode = 1132;
                                        ErrorComponents.Add(@ref);
                                        continue;
                                    }
                                    effect.RequireParamIndexes.InsertOrUpdateAt(@ref.MultipleRequireIndex, pp.SelfIndex);
                                    continue;
                                }
                            default:
                                {
                                    //引用资源指示为参数提供但提供的不是合法的参数提供器
                                    @ref.ErrorCode = 1110;
                                    ErrorComponents.Add(@ref);
                                    continue;
                                }
                        }
                    }
                    if (@ref.IsTarget)
                    {
                        if (@ref.ReferenceComponent is ICESTargetSearch targetSearch)
                        {
                            var type = effect.RequireTargetTypes.ElementAtOrDefault(@ref.MultipleRequireIndex);
                            if (type == null)
                            {
                                //引用资源提供的需求目标类型索引越界
                                @ref.ErrorCode = 2022;
                                ErrorComponents.Add(@ref);
                                continue;
                            }
                            if (!type.IsInheritedBy(targetSearch.ProvideTargetType))
                            {
                                //引用资源提供的目标搜索器的目标类型与需求的目标类型不匹配
                                @ref.ErrorCode = 2023;
                                ErrorComponents.Add(@ref);
                                continue;
                            }
                            effect.RequireTargetIndexes.InsertOrUpdateAt(@ref.MultipleRequireIndex, targetSearch.SelfIndex);
                            continue;
                        }
                        else if (@ref.ReferenceComponent is ICESParamTargetConvertor paramTargetConvertor)
                        {
                            var type = effect.RequireTargetTypes.ElementAtOrDefault(@ref.MultipleRequireIndex);
                            if (type == null)
                            {
                                //引用资源提供的需求目标类型索引越界
                                @ref.ErrorCode = 2022;
                                ErrorComponents.Add(@ref);
                                continue;
                            }
                            if (!type.IsInheritedBy(paramTargetConvertor.ProvideTargetType))
                            {
                                //引用资源提供的目标搜索器的目标类型与需求的目标类型不匹配
                                @ref.ErrorCode = 2023;
                                ErrorComponents.Add(@ref);
                                continue;
                            }
                            effect.RequireTargetIndexes.InsertOrUpdateAt(@ref.MultipleRequireIndex, paramTargetConvertor.SelfIndex);
                            continue;
                        }
                        else
                        {
                            //引用资源指示为目标提供但是指向的不是合法的目标搜索器
                            @ref.ErrorCode = 2012;
                            ErrorComponents.Add(@ref);
                            continue;
                        }

                    }
                }
                //总检查
                for (int j = 0; j < effect.RequireParamTypes.Count; j++)
                {
                    if (j >= effect.RequireParamIndexes.Count)
                    {
                        //效果的该需求参数未提供索引
                        ErrorComponents.Add(new ComponentReference() { SourceComponent = effect, IsParam = true, ErrorCode = 2312 });
                        continue;
                    }
                    var paramIndex = effect.RequireParamIndexes[j];
                    var paramC = SearchIndexComponent(paramIndex);
                    if (paramC == null)
                    {
                        //效果的该需求参数索引越界
                        ErrorComponents.Add(new ComponentReference() { SourceComponent = effect, ErrorCode = 2313 });
                        continue;
                    }
                    var type = effect.RequireParamTypes[j];
                    if (paramC is ICESTrigger tg)
                    {
                        if (tg != Trigger)
                        {
                            //效果的该需求参数索引指向触发器，但是该触发器不是当前实例的触发器
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = effect, ReferenceComponent = tg, IsParam = true, TriggerParamIndex = paramIndex, MultipleRequireIndex = paramIndex, ErrorCode = 2331 });
                        }
                        if (paramIndex < tg.ProvideParamTypes.Count)
                        {
                            if (!type.IsInheritedBy(tg.ProvideParamTypes[paramIndex]))
                            {
                                //效果的该需求参数索引指向触发器，但是该索引对应的效果的需求参数类型与触发器提供的参数类型不匹配
                                ErrorComponents.Add(new ComponentReference()
                                { SourceComponent = effect, ReferenceComponent = tg, IsParam = true, TriggerParamIndex = paramIndex, MultipleRequireIndex = paramIndex, ErrorCode = 2332 });
                            }
                        }
                        else
                        {
                            //效果的该需求参数索引指向触发器，但是索引超出该触发器提供的参数类型的数量
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = effect, ReferenceComponent = tg, IsParam = true, TriggerParamIndex = paramIndex, MultipleRequireIndex = paramIndex, ErrorCode = 2333 });
                        }
                        continue;
                    }
                    if (paramC is ICESFreeParam fp)
                    {
                        if (!FreeParams.Contains(fp))
                        {
                            //效果的该需求参数索引指向自由参数，但是该自由参数不是当前实例的自由参数
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = effect, ReferenceComponent = fp, IsParam = true, ErrorCode = 2341 });
                        }
                        if (!type.IsInheritedBy(fp.ProvideParamType))
                        {
                            //效果的该需求参数索引指向自由参数，但是该索引对应的效果的需求参数类型与自由参数提供的参数类型不匹配
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = effect, ReferenceComponent = fp, IsParam = true, ErrorCode = 2342 });
                        }
                        continue;
                    }
                    if (paramC is ICESParamProcessor pp)
                    {
                        if (!ParamProcessors.Contains(pp))
                        {
                            //效果的该需求参数索引指向参数处理器，但是该参数处理器不是当前实例的参数处理器
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = effect, ReferenceComponent = pp, IsParam = true, ErrorCode = 2351 });
                        }
                        if (!type.IsInheritedBy(pp.ProvideParamType))
                        {
                            //效果的该需求参数索引指向参数处理器，但是该索引对应的效果的需求参数类型与参数处理器提供的参数类型不匹配
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = effect, ReferenceComponent = pp, IsParam = true, ErrorCode = 2352 });
                        }
                        continue;
                    }
                }
                for (int j = 0; j < effect.RequireTargetTypes.Count; j++)
                {
                    if (j >= effect.RequireTargetIndexes.Count)
                    {
                        //效果的该需求目标类型未提供索引
                        ErrorComponents.Add(new ComponentReference() { SourceComponent = effect, IsTarget = true, ErrorCode = 2422 });
                        continue;
                    }
                    var targetIndex = effect.RequireTargetIndexes[j];
                    var targetC = SearchIndexComponent(targetIndex);
                    var type = effect.RequireTargetTypes[j];
                    if (targetC is ICESTargetSearch ts)
                    {
                        if (!TargetSearches.Contains(ts))
                        {
                            //效果的该需求目标类型索引指向目标搜索器，但是该目标搜索器不是当前实例的目标搜索器
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = effect, ReferenceComponent = ts, IsTarget = true, ErrorCode = 2441 });
                        }
                        if (!type.IsInheritedBy(ts.ProvideTargetType))
                        {
                            //效果的该需求目标类型索引指向目标搜索器，但是该索引对应的效果的需求参数类型与目标搜索器提供的目标类型不匹配
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = effect, ReferenceComponent = ts, IsTarget = true, ErrorCode = 2442 });
                        }
                        continue;
                    }
                    else if (targetC is ICESParamTargetConvertor paramTargetConvertor)
                    {
                        if (!ParamTargetConvertors.Contains(paramTargetConvertor))
                        {
                            //效果的该需求目标类型索引指向参数转目标器，但是该参数转目标器不是当前实例的参数转目标器
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = effect, ReferenceComponent = paramTargetConvertor, IsTarget = true, ErrorCode = 2451 });
                        }
                        if (!type.IsInheritedBy(paramTargetConvertor.ProvideTargetType))
                        {
                            //效果的该需求目标类型索引指向参数转目标器，但是该索引对应的效果的需求参数类型与参数转目标器提供的目标类型不匹配
                            ErrorComponents.Add(new ComponentReference()
                            { SourceComponent = effect, ReferenceComponent = paramTargetConvertor, IsTarget = true, ErrorCode = 2452 });
                        }
                        continue;
                    }
                    //效果的该需求目标类型索引越界
                    ErrorComponents.Add(new ComponentReference() { SourceComponent = effect, ErrorCode = 2423 });
                    continue;
                }
            }
            else
            {
                //该效果未定义任何引用资源
                ErrorComponents.Add(new ComponentReference() { SourceComponent = effect, ErrorCode = 2005 });
                return;
            }
        }
        /// <summary>
        /// 检查当前组合的所有组件引用关系是否合法，返回错误码。
        /// </summary>
        /// <returns>0为无错误，负数或正数为错误码</returns>
        public int CheckResult()
        {
            if (InvalidCheck(Trigger))
            {
                //已输入的触发器无效
                return 9001;
            }
            if (InvalidCheck(Activity))
            {
                //已输入的活动无效
                return 9002;
            }
            SelfIndexReflect();
            ReferenceReflect();
            ErrorComponents.Clear();
            for (int i = 0; i < ParamProcessors.Count; i++)
            {
                CheckParamProcessor(ParamProcessors[i]);
            }
            for (int i = 0; i < ParamTargetConvertors.Count; i++)
            {
                CheckParamTargetsConvertor(ParamTargetConvertors[i]);
            }
            for (int i = 0; i < Conditions.Count; i++)
            {
                CheckCondition(Conditions[i]);
            }
            for (int i = 0; i < Effects.Count; i++)
            {
                CheckEffect(Effects[i]);
            }
            if (ErrorComponents.Count != 0)
            {
                //存在错误的组件
                return -1000;
            }
            return 0;
        }
        /// <summary>
        /// 获取组合结果对象及校验码，若无错误则返回完整的SingleEffect对象。
        /// </summary>
        /// <returns>元组：(结果对象, 校验码)</returns>
        public (SingleEffect, int) GetResult()
        {
            var result = new SingleEffect();
            var i = CheckResult();
            if (i == 0)
            {
                FreeParams.Sort();
                ParamProcessors.Sort();
                Conditions.Sort();
                TargetSearches.Sort();
                Effects.Sort();
                result.Trigger = Trigger;
                Trigger.Owner = result;
                result.FreeParams.AddRange([.. FreeParams]);
                FreeParams.ForEach(x => x.Owner = result);
                result.ParamProcessors.AddRange([.. ParamProcessors]);
                ParamProcessors.ForEach(x => x.Owner = result);
                result.Conditions.AddRange([.. Conditions]);
                Conditions.ForEach(x => x.Owner = result);
                result.TargetSearches.AddRange([.. TargetSearches]);
                TargetSearches.ForEach(x => x.Owner = result);
                result.ParamTargetConvertors.AddRange([.. ParamTargetConvertors]);
                ParamTargetConvertors.ForEach(x => x.Owner = result);
                result.Effects.AddRange([.. Effects]);
                Effects.ForEach(x => x.Owner = result);
                result.Activity = Activity;
                Activity.Owner = result;
            }
            return (result, i);
        }
    }
}