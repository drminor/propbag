using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.ViewModelTools
{
    public interface IViewModelFactory<L2T, L2TRaw>
    {
        object GetNewViewModel(string fullClassName);
        object GetNewViewModel(string fullClassName, IPropFactory pfOverride);
        object GetNewViewModel(string fullClassName, IPropFactory pfOverride, string fcnOverride);

        object GetNewViewModel(IPropModel<L2TRaw> propModel, IPropFactory pfOverride, string fcnOverride);

        bool HasAutoMapperServices { get; }

        ICachePropModels<L2TRaw> PropModelCache { get; }
        IPropStoreAccessServiceCreator<L2T, L2TRaw> PropStoreAccessServiceCreator { get; }
        IViewModelActivator<L2T, L2TRaw> ViewModelActivator { get; }
        IProvideAutoMappers AutoMapperService { get; }
        ICreateWrapperTypes WrapperTypeCreator { get; }
    }
}