using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CES
{
    /// <summary>
    /// 对单个效果的序列化改写方法
    /// </summary>
    public static class CESSingleEffectConvertor
    {
        /// <summary>
        /// 序列化单个效果
        /// </summary>
        /// <param name="singleEffect"></param>
        /// <returns></returns>
        public static CESSerializableSingleEffect Serialize(SingleEffect singleEffect)
        {
            var result = new CESSerializableSingleEffect
            {
                ID = singleEffect.ID,
                Trigger = SerializeComponent(singleEffect.Trigger),
                FreeParams = [.. singleEffect.FreeParams.Select(SerializeComponent)],
                ParamProcessors = [.. singleEffect.ParamProcessors.Select(SerializeComponent)],
                TargetSearches = [.. singleEffect.TargetSearches.Select(SerializeComponent)],
                ParamTargetConvertors = [.. singleEffect.ParamTargetConvertors.Select(SerializeComponent)],
                Conditions = [.. singleEffect.Conditions.Select(SerializeComponent)],
                Effects = [.. singleEffect.Effects.Select(SerializeComponent)],
                Activity = SerializeComponent(singleEffect.Activity),
                DesciptionCombineFunction = singleEffect.DesciptionCombineFunction?.GetType().FullName ?? ""
            };
            return result;
        }
        /// <summary>
        /// 反序列化单个效果
        /// </summary>
        /// <param name="serializeableSingleEffect"></param>
        /// <param name="assemblies">用于反序列化时获取类型的额外的程序集</param>
        /// <returns></returns>
        public static SingleEffect Deserialize(CESSerializableSingleEffect serializeableSingleEffect, Assembly[] assemblies = null)
        {
            var result = new SingleEffect();
            result.ID = serializeableSingleEffect.ID;
            result.Trigger = DeserializeComponent<ICESTrigger>(serializeableSingleEffect.Trigger, assemblies);
            result.Trigger.Owner = result;
            result.FreeParams.AddRange([.. serializeableSingleEffect.FreeParams.Select((x) => DeserializeComponent<ICESFreeParam>(x, assemblies))]);
            result.FreeParams.ForEach(x => x.Owner = result);
            result.ParamProcessors.AddRange([.. serializeableSingleEffect.ParamProcessors.Select((x) => DeserializeComponent<ICESParamProcessor>(x, assemblies))]);
            result.ParamProcessors.ForEach(x => x.Owner = result);
            result.TargetSearches.AddRange([.. serializeableSingleEffect.TargetSearches.Select((x) => DeserializeComponent<ICESTargetSearch>(x, assemblies))]);
            result.TargetSearches.ForEach(x => x.Owner = result);
            result.ParamTargetConvertors.AddRange([.. serializeableSingleEffect.ParamTargetConvertors.Select((x) => DeserializeComponent<ICESParamTargetConvertor>(x, assemblies))]);
            result.ParamTargetConvertors.ForEach(x => x.Owner = result);
            result.Conditions.AddRange([.. serializeableSingleEffect.Conditions.Select((x) => DeserializeComponent<ICESCondition>(x, assemblies))]);
            result.Conditions.ForEach(x => x.Owner = result);
            result.Effects.AddRange([.. serializeableSingleEffect.Effects.Select((x) => DeserializeComponent<ICESEffect>(x, assemblies) )]);
            result.Effects.ForEach(x => x.Owner = result);
            result.Activity = DeserializeComponent<ICESActivity>(serializeableSingleEffect.Activity, assemblies);
            result.Activity.Owner = result;
            if (GetTypeWithAssemblies(serializeableSingleEffect.DesciptionCombineFunction, assemblies) is Type type)
            {
                result.DesciptionCombineFunction = (IDescriptionCombiner)Activator.CreateInstance(type);
            }
            return result;
        }
        /// <summary>
        /// 序列化单个组件
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public static SerializableComponent SerializeComponent(ICESComponent component)
        {
            return component switch
            {
                ICESTrigger trigger => SerializeTrigger(trigger),
                ICESFreeParam freeParam => SerializeFreeParam(freeParam),
                ICESParamProcessor processor => SerializeParamProcessor(processor),
                ICESTargetSearch targetSearch => SerializeTargetSearch(targetSearch),
                ICESParamTargetConvertor paramTargetConvertor => SerializeParamToTarget(paramTargetConvertor),
                ICESCondition condition => SerializeCondition(condition),
                ICESEffect effect => SerializeEffect(effect),
                ICESActivity activity => SerializeActivity(activity),
                _ => null
            };
        }
        /// <summary>
        /// 反序列化单个组件
        /// </summary>
        /// <param name="serializableComponent"></param>
        /// <param name="assemblies">用于反序列化时获取类型的额外的程序集</param>
        /// <returns></returns>
        public static T DeserializeComponent<T>(SerializableComponent serializableComponent, Assembly[] assemblies = null) where T : ICESComponent
        {
            return 0 switch
            {
                _ when serializableComponent.ComponentType == GetNameByType<ICESTrigger>() => (T)DeserializeTrigger(serializableComponent, assemblies),
                _ when serializableComponent.ComponentType == GetNameByType<ICESFreeParam>() => (T)DeserializeFreeParam(serializableComponent, assemblies),
                _ when serializableComponent.ComponentType == GetNameByType<ICESParamProcessor>() => (T)DeserializeParamProcessor(serializableComponent, assemblies),
                _ when serializableComponent.ComponentType == GetNameByType<ICESTargetSearch>() => (T)DeserializeTargetSearch(serializableComponent, assemblies),
                _ when serializableComponent.ComponentType == GetNameByType<ICESParamTargetConvertor>() => (T)DeserializeParamToTarget(serializableComponent, assemblies),
                _ when serializableComponent.ComponentType == GetNameByType<ICESCondition>() => (T)DeserializeCondition(serializableComponent, assemblies),
                _ when serializableComponent.ComponentType == GetNameByType<ICESEffect>() => (T)DeserializeEffect(serializableComponent, assemblies),
                _ when serializableComponent.ComponentType == GetNameByType<ICESActivity>() => (T)DeserializeActivity(serializableComponent, assemblies),
                _ => default,
            };
        }
        private static SerializableComponent SerializeTrigger(ICESTrigger trigger)
        {
            if (trigger == null)
                return null;
            var result = new SerializableComponent
            {
                ComponentType = GetTypeName(trigger),
                InstanceType = trigger.GetType().FullName,
                SelfIndex = trigger.SelfIndex,
                UsingNumbers = [.. trigger.UsingNumber],
            };
            return result;
        }
        /// <summary>
        /// 反序列化触发器
        /// </summary>
        /// <param name="serializableComponent"></param>
        /// <returns></returns>
        private static ICESTrigger DeserializeTrigger(SerializableComponent serializableComponent, Assembly[] assemblies = null)
        {
            if (serializableComponent == null || serializableComponent.ComponentType != GetNameByType<ICESTrigger>())
                return null;
            var type = GetTypeWithAssemblies(serializableComponent.InstanceType, assemblies);
            if (type == null || Activator.CreateInstance(type) is not ICESTrigger result)
                return null;
            result.SelfIndex = serializableComponent.SelfIndex;
            result.UsingNumber.Clear();
            result.UsingNumber.AddRange(serializableComponent.UsingNumbers ?? []);
            return result;
        }

        /// <summary>
        /// 序列化自由参数
        /// </summary>
        /// <param name="freeParam"></param>
        /// <returns></returns>
        private static SerializableComponent SerializeFreeParam(ICESFreeParam freeParam)
        {
            if (freeParam == null)
                return null;
            var result = new SerializableComponent
            {
                ComponentType = GetTypeName(freeParam),
                InstanceType = freeParam.GetType().FullName,
                SelfIndex = freeParam.SelfIndex,
                UsingNumbers = [.. freeParam.UsingNumber],
            };
            return result;
        }
        /// <summary>
        /// 反序列化自由参数
        /// </summary>
        /// <param name="serializableComponent"></param>
        /// <returns></returns>
        private static ICESFreeParam DeserializeFreeParam(SerializableComponent serializableComponent, Assembly[] assemblies = null)
        {
            if (serializableComponent == null || serializableComponent.ComponentType != GetNameByType<ICESFreeParam>())
                return null;
            var type = GetTypeWithAssemblies(serializableComponent.InstanceType, assemblies);
            if (type == null || Activator.CreateInstance(type) is not ICESFreeParam result)
                return null;
            result.SelfIndex = serializableComponent.SelfIndex;
            result.UsingNumber.Clear();
            result.UsingNumber.AddRange(serializableComponent.UsingNumbers ?? []);
            return result;
        }
        /// <summary>
        /// 序列化参数处理器
        /// </summary>
        /// <param name="paramProcessor"></param>
        /// <returns></returns>
        private static SerializableComponent SerializeParamProcessor(ICESParamProcessor paramProcessor)
        {
            if (paramProcessor == null)
                return null;
            var result = new SerializableComponent
            {
                ComponentType = GetTypeName(paramProcessor),
                InstanceType = paramProcessor.GetType().FullName,
                SelfIndex = paramProcessor.SelfIndex,
                RequireParamsIndexes = [paramProcessor.RequireParamIndex],
                UsingNumbers = [.. paramProcessor.UsingNumber],
            };
            return result;
        }
        /// <summary>
        /// 反序列化参数处理器
        /// </summary>
        /// <param name="serializableComponent"></param>
        /// <returns></returns>
        private static ICESParamProcessor DeserializeParamProcessor(SerializableComponent serializableComponent, Assembly[] assemblies = null)
        {
            if (serializableComponent == null || serializableComponent.ComponentType != GetNameByType<ICESParamProcessor>())
                return null;
            var type = GetTypeWithAssemblies(serializableComponent.InstanceType, assemblies);
            if (type == null || Activator.CreateInstance(type) is not ICESParamProcessor result)
                return null;
            result.SelfIndex = serializableComponent.SelfIndex;
            result.RequireParamIndex = serializableComponent.RequireParamsIndexes?.ElementAtOrDefault(0) ?? 0;
            result.UsingNumber.Clear();
            result.UsingNumber.AddRange(serializableComponent.UsingNumbers ?? []);
            return result;
        }
        /// <summary>
        /// 序列化目标获取器
        /// </summary>
        /// <param name="targetSearch"></param>
        /// <returns></returns>
        private static SerializableComponent SerializeTargetSearch(ICESTargetSearch targetSearch)
        {
            if (targetSearch == null)
                return null;
            var result = new SerializableComponent
            {
                ComponentType = GetTypeName(targetSearch),
                InstanceType = targetSearch.GetType().FullName,
                SelfIndex = targetSearch.SelfIndex,
                UsingNumbers = [.. targetSearch.UsingNumber],
            };
            return result;
        }
        /// <summary>
        /// 反序列化目标获取器
        /// </summary>
        /// <param name="serializableComponent"></param>
        /// <returns></returns>
        private static ICESTargetSearch DeserializeTargetSearch(SerializableComponent serializableComponent, Assembly[] assemblies = null)
        {
            if (serializableComponent == null || serializableComponent.ComponentType != GetNameByType<ICESTargetSearch>())
                return null; 
            var type = GetTypeWithAssemblies(serializableComponent.InstanceType, assemblies);
            if (type == null || Activator.CreateInstance(type) is not ICESTargetSearch result)
                return null;
            result.SelfIndex = serializableComponent.SelfIndex;
            result.UsingNumber.Clear();
            result.UsingNumber.AddRange(serializableComponent.UsingNumbers ?? []);
            return result;
        }
        /// <summary>
        /// 序列化参数转目标器
        /// </summary>
        /// <param name="paramTargetConvertor"></param>
        /// <returns></returns>
        private static SerializableComponent SerializeParamToTarget(ICESParamTargetConvertor paramTargetConvertor)
        {
            if (paramTargetConvertor == null)
                return null;
            var result = new SerializableComponent
            {
                ComponentType = GetTypeName(paramTargetConvertor),
                InstanceType = paramTargetConvertor.GetType().FullName,
                SelfIndex = paramTargetConvertor.SelfIndex,
                RequireParamsIndexes = [paramTargetConvertor.RequireParamIndex],
                UsingNumbers = [.. paramTargetConvertor.UsingNumber]
            };
            return result;
        }
        /// <summary>
        /// 反序列化参数转目标器
        /// </summary>
        /// <param name="serializableComponent"></param>
        /// <returns></returns>
        private static ICESParamTargetConvertor DeserializeParamToTarget(SerializableComponent serializableComponent, Assembly[] assemblies = null)
        {
            if (serializableComponent == null || serializableComponent.ComponentType != GetNameByType<ICESParamTargetConvertor>())
                return null;
            var type = GetTypeWithAssemblies(serializableComponent.InstanceType, assemblies);
            if (type == null || Activator.CreateInstance(type) is not ICESParamTargetConvertor result)
                return null;
            result.SelfIndex = serializableComponent.SelfIndex;
            result.RequireParamIndex = serializableComponent.RequireParamsIndexes?.ElementAtOrDefault(0) ?? 0;
            result.UsingNumber.Clear();
            result.UsingNumber.AddRange(serializableComponent.UsingNumbers ?? []);
            return result;
        }
        /// <summary>
        /// 序列化条件
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        private static SerializableComponent SerializeCondition(ICESCondition condition)
        {
            if (condition == null)
                return null;
            var result = new SerializableComponent
            {
                ComponentType = GetTypeName(condition),
                InstanceType = condition.GetType().FullName,
                SelfIndex = condition.SelfIndex,
                RequireParamsIndexes = [.. condition.RequireParamIndexes],
                UsingNumbers = [.. condition.UsingNumber]
            };
            return result;
        }
        /// <summary>
        /// 反序列化条件
        /// </summary>
        /// <param name="serializableComponent"></param>
        /// <returns></returns>
        private static ICESCondition DeserializeCondition(SerializableComponent serializableComponent, Assembly[] assemblies = null)
        {
            if (serializableComponent == null || serializableComponent.ComponentType != GetNameByType<ICESCondition>())
                return null;
            var type = GetTypeWithAssemblies(serializableComponent.InstanceType, assemblies);
            if (type == null || Activator.CreateInstance(type) is not ICESCondition result)
                return null;
            result.SelfIndex = serializableComponent.SelfIndex;
            result.RequireParamIndexes.Clear();
            result.RequireParamIndexes.AddRange(serializableComponent.RequireParamsIndexes ?? []);
            result.UsingNumber.Clear();
            result.UsingNumber.AddRange(serializableComponent.UsingNumbers ?? []);
            return result;
        }
        /// <summary>
        /// 序列化效果
        /// </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        private static SerializableComponent SerializeEffect(ICESEffect effect)
        {
            if (effect == null)
                return null;
            var result = new SerializableComponent
            {
                ComponentType = GetTypeName(effect),
                InstanceType = effect.GetType().FullName,
                SelfIndex = effect.SelfIndex,
                RequireParamsIndexes = [.. effect.RequireParamIndexes],
                RequireTargetsIndexes = [.. effect.RequireTargetIndexes],
                UsingNumbers = [.. effect.UsingNumber]
            };
            return result;
        }
        /// <summary>
        /// 反序列化效果
        /// </summary>
        /// <param name="serializableComponent"></param>
        /// <returns></returns>
        private static ICESEffect DeserializeEffect(SerializableComponent serializableComponent, Assembly[] assemblies = null)
        {
            if (serializableComponent == null || serializableComponent.ComponentType != GetNameByType<ICESEffect>())
                return null;
            var type = GetTypeWithAssemblies(serializableComponent.InstanceType, assemblies);
            if (type == null || Activator.CreateInstance(type) is not ICESEffect result)
                return null;
            result.SelfIndex = serializableComponent.SelfIndex;
            result.RequireParamIndexes.Clear();
            result.RequireParamIndexes.AddRange(serializableComponent.RequireParamsIndexes ?? []);
            result.RequireTargetIndexes.Clear();
            result.RequireTargetIndexes.AddRange(serializableComponent.RequireTargetsIndexes ?? []);
            result.UsingNumber.Clear();
            result.UsingNumber.AddRange(serializableComponent.UsingNumbers ?? []);
            return result;
        }
        /// <summary>
        /// 序列化执行器
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private static SerializableComponent SerializeActivity(ICESActivity activity)
        {
            if (activity == null)
                return null;
            var result = new SerializableComponent
            {
                ComponentType = GetTypeName(activity),
                InstanceType = activity.GetType().FullName,
                SelfIndex = activity.SelfIndex,
                UsingNumbers = [.. activity.UsingNumber],
            };
            return result;
        }
        /// <summary>
        /// 反序列化执行器
        /// </summary>
        /// <param name="serializableComponent"></param>
        /// <returns></returns>
        private static ICESActivity DeserializeActivity(SerializableComponent serializableComponent, Assembly[] assemblies = null)
        {
            if (serializableComponent == null || serializableComponent.ComponentType != GetNameByType<ICESActivity>())
                return null;
            var type = GetTypeWithAssemblies(serializableComponent.InstanceType, assemblies);
            if (type == null || Activator.CreateInstance(type) is not ICESActivity result)
                return null;
            result.SelfIndex = serializableComponent.SelfIndex;
            result.UsingNumber.Clear();
            result.UsingNumber.AddRange(serializableComponent.UsingNumbers ?? []);
            return result;
        }

        private static Type GetTypeWithAssemblies(string typeName, Assembly[] assemblies)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return default;
            }
            var type = Type.GetType(typeName);
            if (type == null && assemblies != null)
            {
                foreach (var assembly in assemblies)
                {
                    if (assembly.GetType(typeName) is Type otherType)
                    {
                        type = otherType;
                        break;
                    }
                }
            }
            return type;
        }
        private static string GetTypeName(ICESComponent component)
        {
            return component switch
            {
                ICESTrigger => "Trigger",
                ICESFreeParam => "FreeParam",
                ICESParamProcessor => "ParamProcessor",
                ICESTargetSearch => "TargetSearch",
                ICESParamTargetConvertor => "ParamTargetConvertor",
                ICESCondition => "Condition",
                ICESEffect => "Effect",
                ICESActivity => "Activity",
                _ => "null"
            };
        }
        private static string GetNameByType<T>() where T : ICESComponent
        {
            if (typeof(T) == typeof(ICESTrigger))
            {
                return "Trigger";
            }
            else if (typeof(T) == typeof(ICESFreeParam))
            {
                return "FreeParam";
            }
            else if (typeof(T) == typeof(ICESParamProcessor))
            {
                return "ParamProcessor";
            }
            else if (typeof(T) == typeof(ICESTargetSearch))
            {
                return "TargetSearch";
            }
            else if (typeof(T) == typeof(ICESParamTargetConvertor))
            {
                return "ParamTargetConvertor";
            }
            else if (typeof(T) == typeof(ICESCondition))
            {
                return "Condition";
            }
            else if (typeof(T) == typeof(ICESEffect))
            {
                return "Effect";
            }
            else if (typeof(T) == typeof(ICESActivity))
            {
                return "Activity";
            }
            else
            {
                return "null";
            }
        }
    }
    /// <summary>
    /// 组件的可序列化信息
    /// </summary>
    [Serializable]
    public class SerializableComponent
    {
        /// <summary>
        /// 组件类型名称
        /// </summary>
        public string ComponentType { get; set; }
        /// <summary>
        /// 实例的具体类型
        /// </summary>
        public string InstanceType { get; set; }
        /// <summary>
        /// 组件的自索引
        /// </summary>
        public int SelfIndex { get; set; }
        /// <summary>
        /// 组件的使用数字列表
        /// </summary>
        public List<float> UsingNumbers { get; set; } = [];
        /// <summary>
        /// 组件的需求参数的引用组件索引
        /// </summary>
        public List<int> RequireParamsIndexes { get; set; } = [];
        /// <summary>
        /// 组件的需求目标的引用组件索引
        /// </summary>
        public List<int> RequireTargetsIndexes { get; set; } = [];
    }
    /// <summary>
    /// 可序列化的单个效果
    /// </summary>
    [Serializable]
    public class CESSerializableSingleEffect
    {
        /// <summary>
        /// 效果ID。
        /// </summary>
        public virtual int ID { get; set; } = 0;
        /// <summary>
        /// 效果的触发器组件。
        /// </summary>
        public SerializableComponent Trigger { get; set; }
        /// <summary>
        /// 自由参数列表。
        /// </summary>
        public List<SerializableComponent> FreeParams { get; set; } = [];
        /// <summary>
        /// 参数处理器列表。
        /// </summary>
        public List<SerializableComponent> ParamProcessors { get; set; } = [];
        /// <summary>
        /// 目标搜索组件列表。
        /// </summary>
        public List<SerializableComponent> TargetSearches { get; set; } = [];
        /// <summary>
        /// 参数转目标组件列表
        /// </summary>
        public List<SerializableComponent> ParamTargetConvertors { get; set; } = [];
        /// <summary>
        /// 条件组件列表。
        /// </summary>
        public List<SerializableComponent> Conditions { get; set; } = [];
        /// <summary>
        /// 子效果列表。
        /// </summary>
        public List<SerializableComponent> Effects { get; set; } = [];
        /// <summary>
        /// 活动组件。
        /// </summary>
        public SerializableComponent Activity { get; set; }
        /// <summary>
        /// 描述合成方法类型。
        /// </summary>
        public virtual string DesciptionCombineFunction { get; set; }
    }
}
