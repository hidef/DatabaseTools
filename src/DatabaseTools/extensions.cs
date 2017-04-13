using System.Collections.Generic;
using System.Linq;

namespace DatabaseTools
{
    public static class extensions
    {
        public static bool EqualTo<T>(this IList<T> self, IList<T> other) where T : class
        {
            if ( self == other ) return true;
            if ( self == null ) return false;
            if ( other == null ) return false;
            
            if ( self.Count() != other.Count() ) return false;

            int count = self.Count();
            for ( int i = 0; i < count; i++ )
            {
                if ( self[i] != other[i] ) return false;
            }

            return true;
        }
    }
}