using DataAccess;
using MessierModel;

namespace MessierApi
{
    public class Query
    {
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<MessierTarget> GetTargets([Service] MessierContext ctx) => 
            ctx.Targets;
    }
}
