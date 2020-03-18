using AutoMapper;

namespace NextPipe.Utilities.Resources
{
    public static class GenericMapper<TSource, TDestination>
    {          
        public static TDestination Map(TSource source)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDestination>();
            });

            IMapper mapper = config.CreateMapper();

            return mapper.Map<TSource, TDestination>(source);         
        } 

        public static TDestination Map(TSource source, TDestination destination)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDestination>();
            });

            IMapper mapper = config.CreateMapper();

            mapper.Map<TSource, TDestination>(source, destination);
            
            return destination;
        }
        
        public static TDestination Map(MapperConfiguration customMap, TSource source)
        {
            var config = customMap;

            IMapper mapper = config.CreateMapper();

            return mapper.Map<TSource, TDestination>(source);
        }
    }
}