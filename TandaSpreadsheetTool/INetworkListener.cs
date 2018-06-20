using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TandaSpreadsheetTool
{
    interface INetworkListener
    {
     bool Connected
        {
            set;
        }
        
        NetworkStatus NetStatus
        {
            set;
        }

    }

    enum NetworkStatus
    {
        BUSY,IDLE,ERROR
    }
}
