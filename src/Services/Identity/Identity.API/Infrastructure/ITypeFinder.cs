using System;
using System.Collections.Generic;

namespace LibraryBuddy.Services.Identity.API.Infrastructure
{
    public interface ITypeFinder
    {
        IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true);
        IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true);
    }
}