

namespace DRM.PropBag.AutoMapperSupport
{
    // This uses the default behavior as provided by the base, abstract class.
    public class MapperConfigurationProvider : MapperConfigurationProviderBase
    {
        // This is an example of how one can override the "BuildBaseConfigAction"
        // to change how a MappingConfiguration object is created.

        //    protected override IConfigurationProvider BuildBaseConfigAction(Action<IMapperConfigurationExpression> cfgAction)
        //    {
        //        // Create an instance of a class that performs two actions.
        //        //CombineActs ca = new CombineActs(UseDefaultConfiguration, cfgAction);

        //        // Create a new Action that does ours and theirs.
        //        Action<IMapperConfigurationExpression> doBoth
        //            = UseDefaultConfigurationAction.Combine<IMapperConfigurationExpression>(cfgAction);

        //        // Create the MapperConfiguation that will perform our 
        //        // initial action of using the DefaultConfiguration (i.e., it does nothing.)
        //        // and then perform the action supplied by the caller.
        //        IConfigurationProvider result = new MapperConfiguration(doBoth);
        //        return result;
        //    }
        //}

        //static class ActionExtensions
        //{
        //    public static Action<T1> Combine<T1>(this Action<T1> firstAction, Action<T1> secondAction)
        //    {
        //        return arg =>
        //        {
        //            firstAction(arg);
        //            secondAction(arg);
        //        };
        //    }
        //}


    }
}
