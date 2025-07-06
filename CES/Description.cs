using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CES
{
    /// <summary>
    /// CES组件描述处理器接口
    /// </summary>
    public interface IDescribeProcessor
    {
        /// <summary>
        /// 获取该组件的主要描述
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public string GetMainDescription(ICESComponent component);
        /// <summary>
        /// 获取该组件的需求参数类型的描述
        /// </summary>
        /// <param name="component"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetRequiredParamDescription(ICESComponent component, int index);
        /// <summary>
        /// 获取该组件的需求目标类型的描述
        /// </summary>
        /// <param name="component"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetRequiredTargetDescription(ICESComponent component, int index);
        /// <summary>
        /// 获取该组件的提供的参数的描述
        /// </summary>
        /// <param name="component"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetProvideParamDescription(ICESComponent component, int index);
        /// <summary>
        /// 获取该组件的提供的目标的参数
        /// </summary>
        /// <param name="component"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetProvideTargetDescription(ICESComponent component, int index);
        /// <summary>
        /// 获取该组件需要特别改变的描述
        /// </summary>
        /// <param name="component"></param>
        /// <param name="originalDesc"></param>
        /// <returns></returns>
        public string ChangeDescription(ICESComponent component, string originalDesc);
    }
    /// <summary>
    /// CES SingleEffect总描述组合处理接口
    /// </summary>
    public interface IDescriptionCombiner
    {
        /// <summary>
        /// SingleEffect组合所有描述的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="singleEffect"></param>
        /// <returns></returns>
        public string CombineDescription<T>(T singleEffect) where T : SingleEffect;
    }
}
