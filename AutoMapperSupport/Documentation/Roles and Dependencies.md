# Markdown file

I. AutoMapperProvider implements ICachePropBagMappers and provides support for creating MapperRequests, i.e., instances of IPropBagMapperKey<TSource,TDestination>

II. AutoMapperProvider is given object that implement the following interfaces upon construction:
1. 	IMapTypeDefinitionProvider 
2. 	ICachePropBagMappers
3. 	IPropModelProvider (Optional, if not provided, all requests must supply a PropModel.

III. Each request to register a MapperRequest takes the following arguments:
0. The Type Paramerters: TSource and TDestination.
1. PropModel propModel (or a ResourceKey that the PropModelProvider will use to return a PropModel.
2. Type typeToWrap
3. string configPackageName
4. IHaveAMapperConfigurationStep configStarterForThisRequest = null
5. IPropFactory propFactory = null







