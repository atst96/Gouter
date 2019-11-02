using System;
using System.Collections.Generic;
using System.Text;

namespace Gouter
{
    internal interface ISubscribable<T> where T : ISubscribableObject
    {
        void Subscribe(T observer);
        void Describe(T observer);
    }
}
