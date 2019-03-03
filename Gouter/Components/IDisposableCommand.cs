using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gouter
{
    public interface IDisposableCommand : IDisposable, ICommand
    {
    }
}
