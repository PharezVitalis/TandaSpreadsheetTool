using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace TandaSpreadsheetTool
{
    /// <summary>
    /// Class to handle network requests
    /// </summary>
    /// <remarks>1:only 1 should ever exists (so could be static class), 2:Could automate requests so that there is only 1 request method </remarks>
    class Networker
    {
        /// <summary>
        /// the message from the last exception that occured
        /// </summary>
        string mostRecentError = "";

        protected string userNameKey = "iv7jlwbcx#hcq&*";

        /// <summary>
        /// the username
        /// </summary>
        string username = "";
        
        /// <summary>
        /// Acess token
        /// </summary>
        JObject token;

        /// <summary>
        /// Array of Teams from Tanda as JObjects
        /// </summary>
        JArray teams;

        /// <summary>
        /// Array of stamm members from Tanda as JObjects
        /// </summary>
        JArray staff;

        /// <summary>
        /// A Notifiable form
        /// </summary>
        INotifiable form;

       
        /// <summary>
        /// Sends network requests
        /// </summary>
        HttpClient client;

        /// <summary>
        /// Request response
        /// </summary>
        HttpResponseMessage httpresponse;

        /// <summary>
        /// Creates a new instance of the Networker Class
        /// </summary>
        /// <param name="form">Notifiable form</param>
        public Networker(INotifiable form)
        {
           ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
           

            httpresponse = new HttpResponseMessage();
            httpresponse.EnsureSuccessStatusCode();

            this.form = form;

            client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 30);
            client.BaseAddress = new Uri("https://my.tanda.co/api/");
        }

        #region FileIO
        /// <summary>
        /// Loads the token 
        /// </summary>
        /// <param name="password">Password to decrypt the token</param>
        /// <returns>True if the token was sucessfully loaded and decrypted</returns>
        public bool LoadToken(string password)
        {
            form.NewNotifier();

            if (File.Exists(RosterManager.Path + "data.wpn") & password != "")
            {
                if (password != "")
                {
                    form.UpdateProgress("Loading login details");
                    try
                    {


                        using (FileStream file = new FileStream(RosterManager.Path + "data.wpn", FileMode.Open))
                        {
                            byte[] data = new byte[file.Length];

                            file.Read(data, 0, (int)file.Length);

                            var str = Encoding.UTF8.GetString(data);
                          
                            str = Decrypt(str, password);
                            password = "";

                            if (str == "Failed")
                            {
                                form.UpdateProgress("Failed to login, access denied");
                                form.RemoveNotifier();
                                return false;
                            }

                            token = JObject.Parse(str);
                        }

                       
                    }
                    catch (Exception e)
                    {
                        form.UpdateProgress("Failed to get token");
                        mostRecentError = e.Message;
                        form.RemoveNotifier();
                        return false;
                    }
                }


            }
            else
            {
                form.UpdateProgress("No data found");
                form.RemoveNotifier();
                return false;
            }
            form.UpdateProgress("Data loaded sucessfully");
            form.RemoveNotifier();

            return true;
        }

        /// <summary>
        /// Removes all Networker files
        /// </summary>
        public void ClearFileData()
        {
            form.NewNotifier();
            form.UpdateProgress("Clearing user file Data");

            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + "data.wpn"))
            {
                try
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "data.wpn");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    mostRecentError = ex.Message;
                }
            }

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "ud.wpn"))
            {
                try
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "ud.wpn");
                }
                catch (Exception ex)
                {
                    mostRecentError = ex.Message;
                }
                
            }

            form.UpdateProgress("Data cleared");
            form.RemoveNotifier();

            token = null;
            username = "";
        }

        /// <summary>
        /// Saves the username
        /// </summary>
        /// <param name="username">username value</param>
        public void SaveUserName(string username)
        {            
           byte[] storedBytes=Encoding.UTF8.GetBytes(Encrypt(username, userNameKey));

            try
            {
                File.WriteAllBytes(RosterManager.Path + "ud.wpn", storedBytes);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Failed to Save User Data");
                mostRecentError = ex.Message;
            }
        }

        /// <summary>
        /// Loads the username
        /// </summary>
        /// <returns>The loaded username if sucessful, otherwise an empty string </returns>
        public string LoadUsername()
        {
            if (!File.Exists(RosterManager.Path  + "ud.wpn"))
            {
                return "";
            }

            byte[] fileData =  new byte[1];
            string username ="";

            try
            {
                fileData = File.ReadAllBytes(RosterManager.Path + "ud.wpn");
                username = Decrypt(Encoding.UTF8.GetString(fileData), userNameKey);
                
            }
            
            catch(Exception e)
            {
                mostRecentError = e.Message;
            }

             

            if (!(username == "Failed" || username == ""))
            {
                this.username = username;
            }
           

            return username;
        }

        /// <summary>
        /// Encrypts and Saves the token
        /// </summary>
        /// <param name="token">the token to be saved</param>
        /// <param name="password">the encryption string</param>
        private void SaveToken(JObject token, string password)
        {
            string cypher = Encrypt(token.ToString(), password);

            byte[] outBytes = Encoding.UTF8.GetBytes(cypher);

          

            try
            {
                File.WriteAllBytes(RosterManager.Path+ "data.wpn", outBytes);
            }
            catch(Exception e)
            {
                mostRecentError = e.Message;

                Console.WriteLine("Failed to save network information " + e.Message);
            }
        }
#endregion

        #region Encryption

        /// <summary>
        /// Encrypts the data string using the key
        /// </summary>
        /// <param name="data">data to be encrypted</param>
        /// <param name="key">key value used to encrypt string</param>
        /// <returns>The encrypted string</returns>
        public  string Encrypt(string data, string key)
        {
            var DES = new TripleDESCryptoServiceProvider();

            DES.Mode = CipherMode.ECB;
            
            DES.Key = GetKey(key);

            DES.Padding = PaddingMode.PKCS7;
            var DESEncrypt = DES.CreateEncryptor();
            var buffer = Encoding.UTF8.GetBytes(data);

            Console.WriteLine("Data input length: " + data.Length);

            try
            {
                string cypherText = Convert.ToBase64String(DESEncrypt.TransformFinalBlock(buffer, 0, buffer.Length));

                DES.Clear();
                DESEncrypt.Dispose();
                Array.Clear(buffer,0,buffer.Length);

                return cypherText;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                mostRecentError = ex.Message;
              
                return "Failed";
            }
            
        }

        /// <summary>
        /// Modifes the key to suit encryption
        /// </summary>
        /// <param name="password">password value</param>
        /// <returns>modified bytes generated from the password</returns>
        private  byte[] GetKey(string password)
        {
            string key = null;
            if (Encoding.UTF8.GetByteCount(password) < 24)
            {
                key = password.PadRight(24, ' ');
            }
            else
            {
                key = password.Substring(0, 24);
            }

            return Encoding.UTF8.GetBytes(key);
        }

        /// <summary>
        /// Decrypts data using key
        /// </summary>
        /// <param name="data">Data to be decrypted</param>
        /// <param name="key">key to decrypt data with</param>
        /// <returns>Decrypted value</returns>
        public  string Decrypt(string data, string key)
        {
            var DES = new TripleDESCryptoServiceProvider();
            DES.Mode = CipherMode.ECB;
            DES.Key = GetKey(key);

            

            DES.Padding = PaddingMode.PKCS7;
            var DESDecrypt = DES.CreateDecryptor();

            var buffer = Convert.FromBase64String(data.Replace(" ", "+"));

            
            try
            {
                string message = Encoding.UTF8.GetString(DESDecrypt.TransformFinalBlock(buffer, 0, buffer.Length));

                DES.Clear();
                DESDecrypt.Dispose();
                Array.Clear(buffer, 0, buffer.Length); 

                return message;
            }
            catch (Exception ex)
            {
                mostRecentError = ex.Message;
                Console.WriteLine(ex.Message);
                return "Failed";
            }
           
        }

#endregion


        #region NetworkRequests
        /// <summary>
        /// Get's the token from file then decrypts it
        /// </summary>
        /// <param name="username">token username</param>
        /// <param name="password">password that encrypted the token</param>
        /// <returns>True if sucessfully loaded and decrypted</returns>
        public async Task<bool> GetToken(string username, string password)
        {           

            if (token != null )
            {
                return false;
            }
            form.NewNotifier();
            form.UpdateProgress("Getting Token");


            client.DefaultRequestHeaders.Clear();



            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            var formContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("username",username),
                new KeyValuePair<string, string>("password",password),
                new KeyValuePair<string, string>("scope","roster user department"),
                new KeyValuePair<string,string>("grant_type","password")
            });







            try
            {
                Console.WriteLine(client.BaseAddress + "oauth/token/");
                httpresponse = await client.PostAsync("oauth/token/", formContent);
                var tokenStr = await httpresponse.Content.ReadAsStringAsync();



                token = JObject.Parse(tokenStr);

                this.username = username;

                SaveUserName(username);
                SaveToken(token, password);


            }
            catch (Exception ex)
            {
                form.RemoveNotifier();
                
                return false;
            }


            form.RemoveNotifier();
            return true;

        }

        /// <summary>
        /// Get's the roster value containing the given date 
        /// </summary>
        /// <remarks>Data is more consistent with the API if the day is always Monday</remarks>
        /// <param name="containingDate">Date which roster will contain</param>
        /// <returns>The JObject roster recieved from the Tanda API</returns>
        public async Task<JObject> GetRooster(DateTime containingDate)
        {
            

           
            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.Add("Authorization", "bearer " + token.GetValue("access_token"));
            var payload = "";
            JObject roster = null;
            try
            {
                httpresponse = await client.GetAsync("v2/rosters/on/" + containingDate.ToString("yyyy-MM-dd"));
                
                 payload = await httpresponse.Content.ReadAsStringAsync();

                 roster = JObject.Parse(payload);
               if (!httpresponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("Failed to get roster from : " + containingDate.ToShortDateString());
                }
            }
            catch (Exception ex)
            {
               
                mostRecentError = ex.Message;
               
            }

          

            return roster;
        }

        /// <summary>
        /// Gets all teams associated with the current account
        /// </summary>
        /// <returns>A JSON Array containing the teams</returns>
        public async Task<JArray> GetDepartments()
        {
           
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", "bearer " + token.GetValue("access_token"));
            try
            {
                httpresponse = await client.GetAsync("v2/departments");
                var payload = await httpresponse.Content.ReadAsStringAsync();
            
                teams = JArray.Parse(payload);
            }
            catch (Exception ex)
            {
              
               
                mostRecentError = ex.Message;
                
            }

            
            

            return teams;
        }
    
        /// <summary>
        /// Gets all the staff associated with the current account
        /// </summary>
        /// <returns></returns>
        public async Task<JArray> GetStaff()
        {
           
            client.DefaultRequestHeaders.Clear();


            client.DefaultRequestHeaders.Add("Authorization", "bearer " + token.GetValue("access_token"));


            try
            {
                
                    httpresponse = await client.GetAsync("v2/users");
                    var payload = await httpresponse.Content.ReadAsStringAsync();
                    staff = JArray.Parse(payload);

            }
            catch (Exception ex)
            {
            
                mostRecentError = ex.Message;
               
            }

            
           

            return staff;
        }
        #endregion


#region Acessors



        /// <summary>
        /// Returns the last recieved Teams JSON Array
        /// </summary>
        public JArray Teams
        {
            get
            {
                return teams;
            }
        }

        /// <summary>
        /// Returns the last recieved Staff JSON Array
        /// </summary>
        public JArray Staff
        {
            get
            {
                return staff;
            }
        }

        /// <summary>
        /// The last exception thrown by the networker
        /// </summary>
        public string LastNetErrMsg
        {
            get
            {
                return mostRecentError;
            }
        }

        /// <summary>
        /// Whether the token is not null
        /// </summary>
        public bool Authenticated
        {
            get
            {
                return token != null;
            }
        }

        /// <summary>
        /// The last user to log in
        /// </summary>
        public string LastUser
        {
            get
            {
                return username;
            }
        }

        /// <summary>
        /// Whether data files to do with networking exists
        /// </summary>
        bool IsCleanLogIn
        {
            get
            {
                return !File.Exists(AppDomain.CurrentDomain.BaseDirectory + "data.wpn");
            }
        }
        #endregion

        ~Networker()
        {
            client.CancelPendingRequests();
            client.Dispose();
            httpresponse.Dispose();
            
        }

        
        
    }
}
