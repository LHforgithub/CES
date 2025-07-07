using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
namespace CES
{
    public interface ICESComponent : IComparable<ICESComponent>
    {
        public SingleEffect Owner { get; set; }
        public List<float> UsingNumber { get; }
        public bool IsUsingNumber { get; }
        public int UsingNumberCount { get; }
        public int SelfIndex { get; set; }
        public IDescribeProcessor DescribeProcessor { get; }
        public string ChangeDescription(string originalDesc);
        public void Init();
        public void Destroy();
    }
    public interface ICESParamComponent : ICESComponent
    {
    }
    public interface ICESTargetComponent : ICESComponent
    {
    }
    public interface ICESTrigger : ICESParamComponent
    {
        public List<Type> ProvideParamTypes { get; }
        public List<ICESParamable> ProvideParams { get; }
        public void OnTrigger();
    }
    public interface ICESFreeParam : ICESParamComponent
    {
        public Type ProvideParamType { get; }
        public ICESParamable TryGetParam();
    }
    public interface ICESParamProcessor : ICESParamComponent
    {
        public Type RequireParamType { get; }
        public int RequireParamIndex { get; set; }
        public Type ProvideParamType { get; }
        public ICESParamable Process(ICESParamable param);
    }
    public interface ICESCondition : ICESComponent
    {
        public List<Type> RequireParamTypes { get; }
        public List<int> RequireParamIndexes { get; }
        public int AffectComponentIndex { get; set; }
        public Task<bool> Check(List<ICESParamable> param);
    }
    public interface ICESTargetSearch : ICESTargetComponent
    {
        public Type ProvideTargetType { get; }
        public List<ICESTargetable> GetAll();
        public Task<List<ICESTargetable>> Search(List<ICESTargetable> filterTarget);
    }
    public interface ICESParamTargetConvertor : ICESTargetComponent
    {
        public Type RequireParamType { get; }
        public int RequireParamIndex { get; set; }
        public Type ProvideTargetType { get; }
        public Task<List<ICESTargetable>> ParamToTargets(ICESParamable param);
    }
    public interface ICESActivity : ICESComponent
    {
        public Task Action();
    }
    public interface ICESEffect : ICESComponent
    {
        public List<Type> RequireParamTypes { get; }
        public List<int> RequireParamIndexes { get; }
        public List<Type> RequireTargetTypes { get; }
        public List<int> RequireTargetIndexes { get; }
        public Task<int> Effect(List<ICESParamable> @params, List<ICESTargetable>[] targets);
    }
    public interface ICESParamable
    {

    }
    public interface ICESTargetable : ICESParamable
    {

    }
}
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
