using System;
using System.Runtime.Serialization;
using System.IO;

using Newtonsoft.Json.Linq;

namespace TandaSpreadsheetTool
{
    class RoosterBuilder
    {
        JObject rooster;
        JObject staff;

        public RoosterBuilder(JObject rooster)
        {
            this.rooster = rooster;

            CleanRooster();
        }

        public RoosterBuilder(string path)
        {

            try
            {
                rooster = JObject.Parse(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                
            }

        }

        void CleanRooster()
        {
          
            
        }

        void Deserialise()
        {
         
        }

    }
}
