using System;
using System.Runtime.Serialization;
using System.IO;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace TandaSpreadsheetTool
{
    class RosterBuilder
    {
        JObject rosterJson;
        JObject staff;
        Roster rosterObj;

        public RosterBuilder(JObject rooster)
        {
            this.rosterJson = rooster;
            rosterObj = JsonConvert.DeserializeObject<Roster>(rosterJson.ToString());

            for (int i = 0; i < rosterObj.schedules.Count; i++)
            {
                var currentSchedule = rosterObj.schedules[i];

            }
        }

       

    }
}
