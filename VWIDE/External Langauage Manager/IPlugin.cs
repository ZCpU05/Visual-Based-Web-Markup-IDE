using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace External_Langauage_Manager
{
    public interface IPlugin
    {
        void OnStartup();
        string extension { get; }
        Task<string> execute(string code);
    }
}
