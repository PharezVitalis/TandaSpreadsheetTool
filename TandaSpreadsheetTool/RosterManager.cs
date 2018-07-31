using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace TandaSpreadsheetTool
{
    /// <summary>
    /// Manages and Generates roster objects
    /// </summary>
    class RosterManager
    {
        /// <summary>
        /// Networker instance
        /// </summary>
        Networker networker;


        /// <summary>
        /// the max number of connection retries
        /// </summary>
        int maxTries;

        /// <summary>
        /// The delay between each connection attempt
        /// </summary>
        int retryDelay;


        /// <summary>
        /// A List of all the rosters that have been created
        /// </summary>
        List<FormattedRoster> builtRosters;

      
        List<User> staffObjs;


        List<Team> teamObjs;

        //flags
        bool hasStaff = false;
        bool hasTeams = false;

        /// <summary>
        /// The main form's Notification Interface
        /// </summary>
        INotifiable form;

        /// <summary>
        /// The last time the staff list has been updated
        /// </summary>
        DateTime staffLstLastUpdated;

        /// <summary >
        /// The path where the roster data files are stored
        /// </summary>
        public static string Path
        {
            get
            {
                return Environment.CurrentDirectory + "\\" + "Data" + "\\";
            }
        }

        /// <summary>
        /// Creates a new instance of the RosterManager class
        /// </summary>
        /// <param name="networker">A Networking object</param>
        /// <param name="form">A notifiable form</param>
        /// <param name="maxTries">Maximum connection attempts</param>
        /// <param name="retryDelay">The delay between connection attempts</param>
        public RosterManager(Networker networker, INotifiable form, int maxTries = 5, int retryDelay = 500)
        {
            this.maxTries = maxTries;

            this.networker = networker;

            this.retryDelay = retryDelay;
            this.form = form;

            staffObjs = new List<User>();

            teamObjs = new List<Team>();

            builtRosters = new List<FormattedRoster>();



        }

        /// <summary>
        /// The team list
        /// </summary>
        public Team[] Teams
        {
            get
            {
                return teamObjs.ToArray();
            }
        }

        /// <summary>
        /// The last time the staff was updated
        /// </summary>
        public string LastStaffUpdate
        {
            get
            {
                if (File.Exists(Path + "staff.json"))
                {
                    return File.GetLastWriteTime(Path + "staff.json").ToLongDateString();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get's the staff JSON data and converts it to objects
        /// </summary>
        /// <param name="forceUpdate">If true, The local copy of the staff will be overridden</param>
        /// <returns>True if sucuessful</returns>
        public async Task<bool> GetStaff(bool forceUpdate = false)
        {
            var staffJArr = new JArray();
            var readFromFile = false;

            form.EnableNotifiers();

            if (!forceUpdate & File.Exists(Path + "staff.json"))
            {
                form.UpdateProgress("Reading Staff Data file");

                var fileData = File.ReadAllText(Path + "staff.json");
                try
                {
                    staffJArr = JArray.Parse(fileData);
                    staffLstLastUpdated = File.GetLastAccessTime(Path + "staff.json");
                    readFromFile = true;
                }
                catch (Exception e)
                {
                    form.UpdateProgress(" to Get Staff Data: " +e.Message);
                    
                    if (! (e.InnerException is IOException))
                    {
                        form.UpdateProgress("Removing corrupted file");
                        File.Delete(Path + "staff.json");

                    }
                    else
                    {
                        form.UpdateProgress("Staff File data is inacessible");
                    }

                    form.UpdateProgress("Getting Staff Data From Network");
                    staffJArr = await networker.GetStaff();
                }



            }
            else
            {
                 form.UpdateProgress("Getting Staff Data From Network");
                staffJArr = await networker.GetStaff();
            }
            form.UpdateProgress("Building staff data");
            try
            {
                for (int i = 0; i < staffJArr.Count; i++)
                {
                    var staff = JsonConvert.DeserializeObject<User>(staffJArr[i].ToString());

                    if (!staffObjs.Contains(staff))
                    {
                        staffObjs.Add(staff);
                    }
                }
            }

            catch (Exception e)
            {
                form.UpdateProgress("Failed: Failed to get staff from Tanda API: "+e.Message);
                
                form.RaiseMessage("Error Building Roster", "Failed to Build Roster, the file from Tanda is Invalid. ");
                form.DisableNotifiers();
                hasStaff = false;
                return false;
            }

            if (!readFromFile)
            {
                form.UpdateProgress("Saving staff data");
                string[] removedFields = { "date_of_birth", "passcode" , "user_levels", "preferred_hours", "active", "email",
                    "photo", "phone","normalised_phone","time_zone", "created_at", "employment_start_date", "employee_id",
                "department_ids","award_template_id", "award_tag_ids", "report_department_id",  "managed_department_ids",
               "utc_offset",  "part_time_fixed_hours","qualifications","updated_at" };

                staffLstLastUpdated = DateTime.Now;

                for (int i = 0; i < staffJArr.Count; i++)
                {
                    var currentToken = staffJArr[i];
                    RemoveFields(currentToken, removedFields);

                }
                try
                {
                    File.WriteAllText(Path + "staff.json", staffJArr.ToString());
                }
                catch(Exception e)
                {
                    form.UpdateProgress("Failed to save file: " + e.Message);
                }
                
            }

            form.UpdateProgress("Finished Getting Staff");
            form.DisableNotifiers();

            hasStaff = true;
            return true;

        }

        /// <summary>
        /// Formatted DateTime string for when the last staff update was
        /// </summary>
        /// <remarks>This was intended to be used to automatically update the staff list if it is old</remarks>
        public string LastUpdated
        {
            get
            {
                return staffLstLastUpdated.ToString("dd/MM/yy hh:mm");
            }
        }

        /// <summary>
        /// Strips sensitive data from the staff JSON files, so they are safe to save
        /// </summary>
        /// <param name="token">The JToken to have data removed</param>
        /// <param name="fields">The fields to be removed</param>
        private void RemoveFields(JToken token, string[] fields)
        {
            JContainer container = token as JContainer;
            if (container == null) return;

            List<JToken> removeList = new List<JToken>();
            foreach (JToken el in container.Children())
            {
                JProperty p = el as JProperty;
                if (p != null && fields.Contains(p.Name))
                {
                    removeList.Add(el);
                }
                RemoveFields(el, fields);
            }

            foreach (JToken el in removeList)
            {
                el.Remove();
            }
        }

        /// <summary>
        /// Gets the team JSON data and converts it to objects
        /// </summary>
        /// <param name="forceUpdate">If true, The local copy of the staff will be overridden</param>
        /// <returns>True if sucuessful</returns>
        public async Task<bool> GetTeams(bool forceUpdate = false)
        {
            var teamsArr = new JArray();
            bool readFromFile = false;

            form.EnableNotifiers();
           

            if (!forceUpdate & File.Exists(Path + "teams.json"))
            {
                form.UpdateProgress("Getting Team Data from File");

                var fileData = File.ReadAllText(Path + "teams.json");
                try
                {
                    teamsArr = JArray.Parse(fileData);
                    readFromFile = true;
                }
                catch (Exception e)
                {
                    form.UpdateProgress("Failed to get team data from file " +e.Message);

                 
                   
                    form.UpdateProgress("Getting team data from network");
                    teamsArr = await networker.GetStaff();
                }



            }
            else
            {
                form.UpdateProgress("Getting team data from network");
                teamsArr = await networker.GetDepartments();
            }


            form.UpdateProgress("Building team data");

            try
            {
                for (int i = 0; i < teamsArr.Count; i++)
                {
                    var team = JsonConvert.DeserializeObject<Team>(teamsArr[i].ToString());

                    if (!teamObjs.Contains(team))
                    {
                        teamObjs.Add(team);
                    }
                }
            }
            catch(Exception e)
            {
                form.UpdateProgress("Failed, data formats changed or incorrect: "+e.Message);
                form.DisableNotifiers();
                hasTeams = false;
                return false;
            }

            if (!readFromFile)
            {
                form.UpdateProgress("Writing team data to file");
                try
                {
                    File.WriteAllText(Path + "teams.json", teamsArr.ToString());
                }
                catch (Exception e)
                {
                    form.UpdateProgress("Failed to save file: " + e.Message);
                    
                }
                
            }
            form.UpdateProgress("Finished formatting team data");
            form.DisableNotifiers();
            hasTeams = true;
            return true;
        }

        /// <summary>
        /// Sets a date to a given time
        /// </summary>
        /// <param name="day">The date to be set</param>
        /// <param name="hours">24 hour based time</param>
        /// <param name="minutes">minutes passed the hour</param>
        /// <returns>The adjusted DateTime object</returns>
        public static DateTime SetToTime(DateTime day, int hours, int minutes)
        {


            day = day - day.TimeOfDay;
            day = day.AddHours(hours);
            day = day.AddMinutes(minutes);
            return day;

        }

        /// <summary>
        /// Loads in local roster files
        /// </summary>
        /// <param name="getStaffFileData">Whether staff and team data should be read from file as well</param>
        public void LoadRosters( bool getStaffFileData = true)
        {
            var rosterPath = Path + "Rosters";

            form.EnableNotifiers();
            form.UpdateProgress("Getting roster data files.");

            var fileNames = Directory.GetFiles(rosterPath);

            if (getStaffFileData)
            {
                // safe not to await as file read times should be quick - it won't get from network
                if (File.Exists(Path + "staff.json"))
                {
                    GetStaff();
                }
                if (File.Exists(Path + "teams.json"))
                {
                    GetTeams();
                }
            }


            for (int i = 0; i < fileNames.Length; i++)
            {
                var file = fileNames[i];

                if (file.Contains("Roster") & file.Contains(".bin"))
                {
                    try
                    {
                       
                        using (var stream = File.Open(file, FileMode.Open))
                        {
                            var bf = new BinaryFormatter();

                            var item = (FormattedRoster)bf.Deserialize(stream);
                            builtRosters.Add(item);
                        }
                       
                    }
                    catch
                    {
                        form.UpdateProgress("Failed to Read file: " + file);

                        continue;
                    }

                }


            }
            form.DisableNotifiers();
        }

        /// <summary>
        /// Builds roster objects (all 1 week long) within the given time frame
        /// </summary>
        /// <param name="dateFrom">The date from which the roster starts</param>
        /// <param name="dateTo">The date where the roster stops</param>
        /// <param name="save">Saves data to a hardcoded path (within exe files) if true </param>
        /// <returns>The formatted roster</returns>
        public async Task<FormattedRoster> BuildRoster(DateTime dateFrom, DateTime dateTo, bool save = true)
        {
            var currentTries = 0;
            form.EnableNotifiers();
            form.UpdateProgress("Getting roster data.");

            dateFrom = SetToTime(dateFrom, 0, 0);
            dateTo = SetToTime(dateTo, 23, 59);

           
            
        

            var startDate = dateFrom;
            startDate = SetToTime(startDate, 0, 00);

            var endDate = dateTo;
            endDate = SetToTime(endDate, 23, 59);

            while (endDate.DayOfWeek!= DayOfWeek.Monday)
            {
                endDate = endDate.AddDays(1);
            }

            while(startDate.DayOfWeek != DayOfWeek.Monday)
            {
                startDate = startDate.AddDays(-1);
            }

            int weeks = (int)(endDate - startDate).TotalDays / 7;

            var rosters =  new JObject[weeks];



            for (int i = 0; i < weeks; i++)
            {

                rosters[i] = await networker.GetRooster(startDate);

                startDate = startDate.AddDays(7);

            }
            if (dateFrom.DayOfWeek != DayOfWeek.Monday)
            {
                rosters[rosters.Length - 1] = await networker.GetRooster(dateTo);
            }

            if (!hasTeams)
            {
                currentTries = 0;
               
                while (true)
                {
                    form.UpdateProgress("Getting team data: Attempt " + (currentTries+1) + " of " + maxTries);
                    await GetTeams();

                    currentTries++;
                    if (!hasTeams)
                    {
                        if (currentTries < maxTries)
                        {
                            System.Threading.Thread.Sleep(retryDelay);
                        }
                        else
                        {
                            form.UpdateProgress("Failed to get team data");
                            form.RaiseMessage("Failed to get teams", "Failed to get team data", MessageBoxIcon.Error);
                            form.DisableNotifiers();
                            return null;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                form.UpdateProgress("Got team data");
            }

           

            if (!hasStaff)
            {
                currentTries = 0;

               
                while (true)
                {
                    form.UpdateProgress("Getting staff data: Attempt " + (currentTries + 1) + " of " + maxTries);
                    await GetStaff();
                    currentTries++;
                    if (!hasStaff)
                    {
                        if (currentTries < maxTries)
                        {
                            System.Threading.Thread.Sleep(retryDelay);
                        }
                        else
                        {
                            form.UpdateProgress("Failed to get staff data");
                            form.RaiseMessage("Failed to get staff", "Failed to get staff data", MessageBoxIcon.Warning);
                            form.DisableNotifiers();
                            return null;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                form.UpdateProgress("Got Staff data");
            }


            form.UpdateProgress("Building roster");



            var rosterObjs = new List<Roster>();

            try
            {

                var modifyStart = false;

                for (int i = 0; i < rosters.Length; i++)
                {
                   var currentRoster  = JsonConvert.DeserializeObject<Roster>(rosters[i].ToString());

                    if (currentRoster.schedules.Count<1)
                    {
                        if (i == 0)
                        {
                            modifyStart = true;
                        }
                        continue;
                    }

                    rosterObjs.Add(currentRoster);
                }

                if (modifyStart)
                {
                    dateFrom = rosterObjs[0].start;
                    SetToTime(dateFrom, 0, 0);
                }

                if (rosterObjs.Count < 1)
                {
                    form.UpdateProgress("No rosters found");
                    form.RaiseMessage("No Schedules Available", "No schedules were found within the specified date range", MessageBoxIcon.Exclamation);
                    form.DisableNotifiers();
                    return null;
                }
             
            }

            catch (Exception e)
            {
                form.UpdateProgress("Failed: failed to format roster data: " + e.Message);
                form.RaiseMessage("Failed to format Roster", "Failed to format roster data: " + e.Message, MessageBoxIcon.Warning);
                form.DisableNotifiers();
                return null;
            }
            var outRoster = CreateRoster(rosterObjs.ToArray(), ref dateFrom,ref dateTo, rosterObjs.Count != rosters.Length);

            outRoster.finish = dateTo;
            outRoster.start = dateFrom;


            if (save)
            {
                form.UpdateProgress("Saving to file");

                var bf = new BinaryFormatter();

                var ms = new MemoryStream();
                bf.Serialize(ms, outRoster);

                var bytes = ms.ToArray();
                var path = GenRosterPath(outRoster);
                try
                {
                    File.WriteAllBytes(path, bytes);
                }
                catch (Exception e)
                {
                    form.UpdateProgress("Failed to save file: " + e.Message);
                }
                
                ms.Dispose();
            }
           
            builtRosters.Add(outRoster);
            form.UpdateProgress("Finished Building Roster");
            form.DisableNotifiers();
            return outRoster;
        }

        /// <summary>
        /// Autogenerates a full path to save a roster
        /// </summary>
        /// <param name="roster">The roster to be saved</param>
        /// <returns>The full roster path</returns>
        public static string GenRosterPath(FormattedRoster roster)
        {
            return Path + "Rosters\\" + "Roster " + roster.start.ToString("dd-MM-yyyy") + " to " + roster.finish.ToString("dd-MM-yyyy") + ".bin";
        }

        /// <summary>
        /// Removes a roster and deletes it's file
        /// </summary>
        /// <param name="index">the index to be removed</param>
        public void Remove(int index)
        {
            if(index>-1 && index < builtRosters.Count)
            {
                var path = GenRosterPath(builtRosters[index]);

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                builtRosters.RemoveAt(index);
            }
        }

        /// <summary>
        /// converts a DD/MM/YY or .../YYYY date format to a DateTime
        /// </summary>
        /// <param name="date">string date value</param>
        /// <returns>DateTime object of given date</returns>
        public static DateTime BuildDate(string date)
        {
            int days = 0;
            int months = 0;
            int years = 0;

            try
            {
                days = Convert.ToInt32(date.Substring(0, 2));
                months = Convert.ToInt32(date.Substring(3, 2));
                if (date.Length > 8)
                {
                    years = Convert.ToInt32(date.Substring(6, 4));
                }
                else
                {
                    years = 2000 + (Convert.ToInt32(date.Substring(6, 2)));
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                return new DateTime(1970, 1, 1);

            }

            return new DateTime(years, months, days);
        }

        /// <summary>
        /// Converts a unix time value to a DateTime
        /// </summary>
        /// <param name="unixValue">Value to be converted to date</param>
        /// <returns>DateTime object of given date</returns>
        public static DateTime UnixToDate(int unixValue)
        {
            var date = new DateTime(1970, 1, 1);
            date = date.AddSeconds(unixValue);
            date = date.ToLocalTime();



            return date;
        }

        /// <summary>
        /// Converts a unix time value to a DateTime
        /// </summary>
        /// <param name="unixValue">Value to be converted to date</param>
        /// <returns>DateTime object of given date</returns>
        public static DateTime UnixToDate(long unixValue)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0);
            date = date.AddSeconds(unixValue);
            date = date.ToLocalTime();


            return date;
        }

        /// <summary>
        /// formats date to YYYY/MM/DD, as required by the Tanda API
        /// </summary>
        /// <param name="date">date to be converted</param>
        /// <returns>Converted date value in YYYY/MM/DD format</returns>
        /// <remarks>Compatible only with DD/MM/YYYY format</remarks>
        public static string FormatDate(string date)
        {
            return date.Substring(6, 4) + "/" + date.Substring(3, 2) + "/" + date.Substring(0, 2);
        }
        /// <summary>
        /// Returns how frequently an connection attempt will be made (in ms)
        /// </summary>
        public int RetryDelay
        {
            get
            {
                return retryDelay;
            }
            set
            {
                if (value < 1500 && value > 0)
                {
                    retryDelay = value;
                }
                else if (value < 0)
                {
                    retryDelay = 0;
                }
                else
                {
                    retryDelay = 1500;
                }
            }
        }

        /// <summary>
        /// Helper function to create a single formatted roster from an array of unformatted roster Objects
        /// </summary>
        /// <param name="rosters">Rosters to be formatted</param>
        /// <param name="dateFrom">The date from which a roster will be added</param>
        /// <param name="dateTo">The latest date from which a roster will be added</param>
        /// <returns>A formatted roster </returns>
        private FormattedRoster CreateRoster(Roster[] rosters, ref DateTime dateFrom, ref DateTime dateTo, bool modifyDate = false)
        {

            var roster = new FormattedRoster();
            if (modifyDate)
            {
                dateTo = new DateTime(1970, 01, 01);
                form.UpdateProgress("Some schedule values were empty");
            }
            

            for (int i = 0; i < rosters.Length; i++)
            {
              
                var currentRoster = rosters[i];

                var dayCount = currentRoster.schedules.Count;
                for (int j = 0; j < dayCount; j++)
                {
                    var currentDay = currentRoster.schedules[j];
                    var scheduleCount = currentDay.schedules.Count;                  

                    for (int k = 0; k < scheduleCount; k++)
                    {
                        var currentSchedule = currentDay.schedules[k];

                        var staffIdObj = currentSchedule.user_id;

                        var staffId = 0;

                        if (staffIdObj == null)
                        {
                            continue;
                        }
                        else
                        {
                            staffId = Convert.ToInt32(staffIdObj);
                        }

                        var staffMember = roster.staff.Find(x => x.id == staffId);

                        if (staffMember == null)
                        {
                            staffMember = new FormattedStaff();
                            staffMember.id = staffId;
                            staffMember.name = staffObjs.Find(x => x.id == staffId).name;
                            if (staffMember.name == null)
                            {
                                continue;
                            }
                            roster.staff.Add(staffMember);
                        }


                        var formattedSch = new FormattedSchedule();

                        var start = currentSchedule.start;

                        if (start != null)
                        {
                            formattedSch.startDate = UnixToDate(Convert.ToInt32(start));
                            if (!modifyDate&formattedSch.startDate.Date> dateTo.Date)
                            {
                                break;
                            }
                            else if (formattedSch.startDate.Date < dateFrom.Date)
                            {
                                continue;
                            }
                            else if (modifyDate && formattedSch.startDate > dateTo)
                            {
                                dateTo = formattedSch.startDate;
                            }
                        }
                        else
                        {
                            
                            continue;
                        }
                        formattedSch.startTime = formattedSch.startDate.ToString("HH:mm");



                        var teamId = currentSchedule.department_id;

                        var team = (teamId != null) ? teamObjs.Find(x => x.id == Convert.ToInt32(teamId)) : null;

                        if (currentSchedule.finish != null)
                        {
                            formattedSch.endTime = UnixToDate(Convert.ToInt32(currentSchedule.finish)).ToString("HH:mm");
                        }

                        if (team != null)
                        {
                            formattedSch.team = team.name;
                           
                            formattedSch.teamColour = team.colour;
                            formattedSch.teamNameShort = team.export_name;
                        }

                        staffMember.schedules.Add(formattedSch);





                    }// end schedule for


                }//end day for




            }// end rosters for

            if (modifyDate)
            {
                form.RaiseMessage("Empty Schedules", "Some Schedule values were empty, the date has been recalculated");
            }

            return roster;
        }

        /// <summary>
        /// Gets all the rosters held within the current instance
        /// </summary>
        /// <returns>An array of all formatted rosters</returns>
        public FormattedRoster[] GetAllRosters()
        {

            return builtRosters.ToArray();

        }

    }
}
