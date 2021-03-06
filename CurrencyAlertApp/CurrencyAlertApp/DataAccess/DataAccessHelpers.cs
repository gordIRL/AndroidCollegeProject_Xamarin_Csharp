﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
// to pass a XDocument add reference:    System.Xml.Linq.
using Android.Util;
using SQLite;
using System.Globalization;


namespace CurrencyAlertApp.DataAccess
{
    public class DataAccessHelpers
    {
        // General Declarations:
        // Create a single CultureInfo object (once so it can be reused) for correct Parsing of strings to DateTime object
        static readonly CultureInfo cultureInfo = new CultureInfo("en-US");

        // location of database
        static readonly string DBLocation = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "CurrencyAlertApp.db3");

        // To use test data from xml file in asset - set 'testMode = true'
        // Please manually select 'Update XML (Market Data)' Menu Option when app is running
        static readonly bool testMode = false;
        
        //(no more call methods needed)
        public static XDocument XmlTestDataFile { get; set; }
        // 
        public static double TimeToGoOffBeforeMarketAnnouncement { get; set; }

        // NewsObject Declarations:
        // list to store newsObjects retrieved from database
        static List<NewsObject> newsObjectsList = new List<NewsObject>();
        
        // UserAlert Declarations:
        // list to store userAlert retrieved from database
        static List<UserAlert> userAlertList = new List<UserAlert>();        

        // Dummy method to confirm unit tests are wired up correctly
        public static int MulitplyNumbers(int num1, int num2)
        {
            return num1 * num2;
        }
        

        
        // UserAlert Methods
        

        // create empty table - for program load
        public static void CreateEmptyUserAlertTable()
        {
            using (SQLiteConnection conn = new SQLiteConnection(DBLocation))
            {
                if (conn.Table<UserAlert>() == null)
                {                    
                    Log.Debug("DEBUG", "Created USER ALERT table - null instance detected");
                }
                else
                {
                    Log.Debug("DEBUG", "USER ALERT table already created");
                }
                conn.CreateTable<UserAlert>();
            }
        }



        // method to convert newsObject to userAlert object
        public static UserAlert ConvertNewsObjectToUserAlert(NewsObject newsObject)
        {
            UserAlert userAlert = new UserAlert
            {
                // don't assign ID - SQLite will do this automatically when object is inserted into DB
                Title = newsObject.Title,
                CountryChar = newsObject.CountryChar,
                MarketImpact = newsObject.MarketImpact,
                DateAndTime = newsObject.DateAndTime,
                DateInTicks = newsObject.DateInTicks,

                IsPersonalAlert = false,  // because this is converting a market event
                DescriptionOfPersonalEvent = string.Empty   
            };
            return userAlert;
        }

       


        /* 
         * method to add a single UserAlert to UserAlert to  
         * database - passed from Main Activity
         * -- don't use this to repopulate adapter from database (ticks)
        */
       
        public static int AddNewUserAlertToDatabase(UserAlert userAlert)
        {
            int userAlertID = 0;

            using (SQLiteConnection conn = new SQLiteConnection(DBLocation))
            {
                try
                {
                    // insert new UserAlert into database                    

                    // this won't insert the 'ignored' C# DateTime into the database (only TICKS stored)
                    conn.Insert(userAlert);  
                    userAlertID = userAlert.UserAlertID;

                    // display output for testing
                    Log.Debug("DEBUG", "\n\nINSERTED Single User Alert - ID:  " + userAlert.UserAlertID.ToString());
                    Log.Debug("DEBUG", "\n\nINSERTED Single User Alert - ToString:\n" + userAlert.ToString());
                    Log.Debug("DEBUG", "FINISHED\n\n\n");
                }
                catch
                {
                    userAlertID = 0;
                } 
                Log.Debug("DEBUG", "FINISHED\n\n\n");
            } 

            // IMPORTANT - set object properties to null - to clear existing data out
            //           - otherwise you get duplicate entries in the UserAlerts database
            UserAlertsActivity.SelectedNewsObject_PassedFrom_MainActivity = null;
            UserAlertsActivity.SelectedUserAlert_PassedFrom_PersonalAlertsActivity = null;

            return userAlertID;
        }




        public static List<UserAlert> GetAllUserAlertDataFromDatabase()
        {
            // method to populate the userAlertList & return it
            List<UserAlert> tempUserAlertsList = new List<UserAlert>();
            userAlertList.Clear();

            using (SQLiteConnection conn = new SQLiteConnection(DBLocation))
            {
                //needed to avoid a null reference crash if table doesn't exist
                // but it won't overwrite an existing table without 'conn.DropTable' 
                conn.CreateTable<UserAlert>();

                // retrieve all data from database & store in list
                var retrievedDataList = conn.Table<UserAlert>();

                // store each item in list into a returnable list
                foreach (var item in retrievedDataList)
                {
                    // convert DateInTicks to DateTimeObject 
                    item.DateAndTime = new DateTime(item.DateInTicks);

                    //userAlertList.Add(item);
                    tempUserAlertsList.Add(item);
                }
            }
            
            // sort list by long DateInTicks
            var sortedUserAlertList = from myvar in tempUserAlertsList
                                       orderby myvar.DateInTicks
                                      select myvar;
            
            // convert 'var' List into 'real' List
            //List<NewsObject> finalNewsObjectList = new List<NewsObject>();
            foreach (var item in sortedUserAlertList)
            {
                userAlertList.Add(item);
            }
            return userAlertList;
        }





        public static int DeleteSelectedUserAlert(int userAlertID)
        {
            int rowCount = 0;

            using (SQLiteConnection conn = new SQLiteConnection(DBLocation))
            {
                rowCount = conn.Delete<UserAlert>(userAlertID);
            }
            return rowCount;
        }


        // method to populate Table<UserAlert> with dummy data   
        // use to add data passed from Main Activity
        // don't use to repopulate adapter from database (ticks issue)
        public static void PopulateUserAlertTableWithDummyData()
        {
            List<UserAlert> userAlertsList = new List<UserAlert>();
            userAlertsList = DataAccessHelpers.DummyDataForUserAlert();

            using (SQLiteConnection conn = new SQLiteConnection(DBLocation))
            {
                conn.DropTable<UserAlert>();
                conn.CreateTable<UserAlert>();

                foreach (var tempUserAlert in userAlertsList)
                {
                    // insert current UserAlert into database
                    // this won't insert the 'ignored' C# DateTime into the database (only TICKS stored)
                    conn.Insert(tempUserAlert);  

                    Log.Debug("DEBUG", "INSERTED:\n" + tempUserAlert.ToString());
                }

                // list of items currently in the database 
                var retrievedDataList = conn.Table<UserAlert>();

                // set breakpoint here when required
                Log.Debug("DEBUG", "FINISHED\n\n\n");

                // Display what's currently in the table
                foreach (var item in retrievedDataList)
                {
                    Log.Debug("DEBUG", item.ToString());
                }
            }
        }


        public static List<UserAlert> DummyDataForUserAlert()
        {
            UserAlert userAlert1 = new UserAlert()
            {
                UserAlertID = 1,
                IsPersonalAlert = false,
                DateAndTime = new DateTime(2018, 1, 15, 10, 15, 0),
                DateInTicks = 636516081000000000,
                CountryChar = "USD",
                MarketImpact = "High",
                Title = "Non Farm Payroll"
            };
            UserAlert userAlert2 = new UserAlert()
            {
                UserAlertID = 2,
                IsPersonalAlert = false,
                DateAndTime = new DateTime(2018, 2, 20, 13, 20, 0),
                DateInTicks = 636547296000000000,
                CountryChar = "GBP",
                MarketImpact = "Low",
                Title = "Meeting 1"
            };
            UserAlert userAlert3 = new UserAlert()
            {
                UserAlertID = 3,
                IsPersonalAlert = false,
                DateAndTime = new DateTime(2018, 3, 3, 14, 30, 0),
                DateInTicks = 636556842000000000,
                CountryChar = "EUR",
                MarketImpact = "Medium",
                Title = "Interest Rate"
            };
            UserAlert userAlert4 = new UserAlert()
            {
                UserAlertID = 4,
                IsPersonalAlert = false,
                DateAndTime = new DateTime(2018, 8, 6, 11, 00, 0),
                DateInTicks = 636691500000000000,
                CountryChar = "AUD",
                MarketImpact = "Low",
                Title = "Car Sales monthly"
            };
            // Create UserAlert List
            List<UserAlert> userAlertsList = new List<UserAlert>
            {
                // Add dummy data to User Alert List
                userAlert1,
                userAlert2,
                userAlert3,
                userAlert4
            };

            // Return List
            return userAlertsList;
        }// end DummyDataForUserAlert



        //-------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Everything below is the original SetUp Data before work started on UserAlert Section  !!!!
        /// </summary>
        /// 

        // create empty table - for program load
        public static void CreateEmptyTable()
        {
            using (SQLiteConnection conn = new SQLiteConnection(DBLocation))
            {
                if (conn.Table<NewsObject>() == null)
                {
                    Log.Debug("DEBUG", "Created new table - null instance detected");
                }
                conn.CreateTable<NewsObject>();
            }
        }


        public static void CreateTableForURLDownload()
        {
            using (SQLiteConnection conn = new SQLiteConnection(DBLocation))
            {
                conn.DropTable<URLObject>();
                conn.CreateTable<URLObject>();
                URLObject urlForexFactoryXmlDownload = new URLObject { URLAddress = "https://cdn-nfs.forexfactory.net/ff_calendar_thisweek.xml" };
                conn.Insert(urlForexFactoryXmlDownload);
            }
        }

        public static string GetURLForXMLDownloadFromDatabase()
        {
            using (SQLiteConnection conn = new SQLiteConnection(DBLocation))
            {
                // 1st Database item is at '1' - ie not zero-based like arrays !! 
                string url = conn.Get<URLObject>(1).URLAddress;                 
                Log.Debug("DEBUG", "URL in DB: " + url);
                return url;
            }
        }

        public static DateTime Convert_Strings_DateAndTime_To_SingleDateTimeObject(string dateString, string timeString, CultureInfo cultureInfo)
        {
            // Returns a DateTime object from combining a date(string) and a time(string)
            // used to convert the xml format date (string) and time (string) into a C# DateTime object

            string dateAndTimeString = dateString +" " +  timeString;

            // original version - working - but would create a new CultureInfo object on each instance
            //DateTime dateAndTimeObject = DateTime.Parse(dateAndTimeString, new CultureInfo("en-US"));

            // final version - working - uses cultureInfo instantiated at class level (line 28/29 above)
            DateTime dateAndTimeObject = DateTime.Parse(dateAndTimeString, cultureInfo);

            return dateAndTimeObject;
        }     


        public static bool DownloadNewXMLAndStoreInDatabase()
        {
            // download external XML file hosted on public website 
            bool dataUpdateSuccessful = false;

            try
            {
                // testMode bool os now declared at top of this class (approx. line 36 )        

                if (testMode == false)
                {
                    // convert XML and store in database
                    XDocument xmlFile = XDocument.Load(GetURLForXMLDownloadFromDatabase());
                    Log.Debug("DEBUG", "XML data downloaded - SUCCESS");

                    ConvertXmlAndStoreInDatabase(xmlFile);
                    Log.Debug("DEBUG", "XML data stored in database - SUCCESS");
                    dataUpdateSuccessful = true;
                }
                if (testMode == true)
                {
                    // use static PROPERTY - set in MainActivity - to get sample data (from xml file in assets folder)
                    ConvertXmlAndStoreInDatabase(XmlTestDataFile);
                    Log.Debug("DEBUG", "!!!!! XML  TEST-Data     stored in database - SUCCESS");
                    dataUpdateSuccessful = true;
                }
            }
            catch
            {
                dataUpdateSuccessful = false;
                Log.Debug("DEBUG", "FAIL - XML data not downloaded");
            }
            return dataUpdateSuccessful;
        }

        public static void DropNewsObjectTable()
        {
            newsObjectsList.Clear();

            using (SQLiteConnection conn = new SQLiteConnection(DBLocation))
            {
                conn.DropTable<NewsObject>();
                conn.CreateTable<NewsObject>();
            }
        }


        public static void ConvertXmlAndStoreInDatabase(XDocument xmlFile)
        {
            // converts downloaded xml data & add to database
            newsObjectsList.Clear();

            using (SQLiteConnection conn = new SQLiteConnection(DBLocation))
            {
                conn.DropTable<NewsObject>();
                conn.CreateTable<NewsObject>();

                foreach (var item in xmlFile.Descendants("event"))
                {
                    // get date & time values from XML (string)
                    string dateOnly = item.Element("date").Value.TrimEnd();
                    string timeOnly = item.Element("time").Value.TrimEnd();

                    // convert date & time strings to DateTime object
                    DateTime tempDateTime = Convert_Strings_DateAndTime_To_SingleDateTimeObject(dateOnly, timeOnly, cultureInfo);

                    // adjust time for GMT in xml file & add 1 hour if it is currently daylight saving
                    bool isDaylightSavingsTime = tempDateTime.IsDaylightSavingTime();
                    //Log.Debug("DEBUG", "\n\n\n It is currently daylightsavings time: " 
                    //    + isDaylightSavingsTime.ToString()  +    "\n\n\n");

                    if (isDaylightSavingsTime == true)
                    {   
                        // add 1 hour to every datetime to bring GMT time to correct time during daylight savings time
                        tempDateTime = tempDateTime.AddHours(1);

                        // NOTE:  tempDateTime.AddHours(1);  (doesn't work because)
                        // This method does not change the value of this DateTime. 
                        // Instead, it returns a new DateTime whose value is the result of this operation. (MSDN)
                    }

                    // set alarm to go off a set amount before news alart time
                    tempDateTime = tempDateTime.AddMinutes(TimeToGoOffBeforeMarketAnnouncement);
                    
                    // convert DateTime object to a long of ticks
                    long dateTimeInTicks = tempDateTime.Ticks;

                    // create a newsObject for every 'event' in xml file
                    NewsObject newsObject = new NewsObject
                    {
                        Title = item.Element("title").Value.TrimEnd(),
                        // .Value - removes surrounding tags
                        CountryChar = item.Element("country").Value.TrimEnd(),
                        MarketImpact = item.Element("impact").Value.TrimEnd(),
                        DateInTicks = dateTimeInTicks
                    };
                    // insert newsObject into database
                    conn.Insert(newsObject);
                };
            }
        }


        public static List<NewsObject> GetAllNewsObjectDataFromDatabase()
        {
            // this populates the newsObjectList &&&& returns it
            newsObjectsList.Clear();

            using (SQLiteConnection conn = new SQLiteConnection(DBLocation))
            {
                // retrieve all data from database & store in list
                var retrievedDataList = conn.Table<NewsObject>();

                // store each item in list into a returnable list
                foreach (var item in retrievedDataList)
                {
                    // convert DateInTicks to DateTimeObject 
                    item.DateAndTime = new DateTime(item.DateInTicks);

                    newsObjectsList.Add(item);
                }
            }         
            return newsObjectsList;
        }



        public static List<NewsObject> LINQ_SortAllByUserSelection(List<string> marketImpact_selectedList, List<string> currencies_selectedList)
        {
            List<NewsObject> tempNewsObjectsList = new List<NewsObject>();
            
            // call to database & populate newsObjectList with the result 
            newsObjectsList.Clear();

            // returns List<NewsObject>  &&&& populates newsObjectList declared above (YES)
            GetAllNewsObjectDataFromDatabase();              

            // loop through MarketImpact List (ie. act on each - all HIGH, all Medium, all Low events)
            foreach (var marketImpactSelectedItem in marketImpact_selectedList)
            {
                foreach (var currencySelectedItem in currencies_selectedList)
                {
                    // use LINQ to get all the selected currencies within the current MarketImpact
                    var tempLinqQueryList = from myVar in newsObjectsList
                                            where myVar.MarketImpact == marketImpactSelectedItem.ToString() &&
                                            myVar.CountryChar == currencySelectedItem.ToString()
                                            select myVar;

                    // add each newsobject from linq query to object list
                    foreach (var linqResultItem in tempLinqQueryList)
                    {
                        tempNewsObjectsList.Add(linqResultItem);
                    }
                }
            } 

            // sort list by long DateInTicks
            var sortedNewsObjectList = from myvar in tempNewsObjectsList
                                       orderby  myvar.DateInTicks
                                       select myvar;

            // convert 'var' List into 'real' List
            List<NewsObject> finalNewsObjectList = new List<NewsObject>();
            foreach (var item in sortedNewsObjectList)
            {
                finalNewsObjectList.Add(item);
            }
            return finalNewsObjectList;
        }


        //// old - not in use anymore - for demonstration purposes
        ////public static List<NewsObject> TestXMLDataFromAssetsFile(XDocument xmlTestFile)
        ////{
        ////    List<NewsObject> listToReturn = new List<NewsObject>();
        ////    // next component won't run unless:  
        ////    // reference is added enable passing of an XDocument:  System.Xml.Linq  
        ////    //
        ////    // remember to declare Xdocument in Main Activity:
        ////    //         XDocument xmlTestFile = XDocument.Load(Assets.Open("ff_calendar_thisweek.xml"));


        ////    //// 'Root.Elements' version vs 'Descendants' version  (both versions work)             
        ////    //foreach (var item in xmlTestFile.Root.Elements())
        ////    //{    OR.................  (using  descendents version below)


        ////    // 'Descendants' version (both versions work) - data retrieved indivudually from xml file            
        ////    foreach (var item in xmlTestFile.Descendants("event"))
        ////    // 'event' is the surrounding xml <tag>
        ////    {
        ////        // get date & time values from XML (string)
        ////        string dateOnly = item.Element("date").Value.TrimEnd();
        ////        string timeOnly = item.Element("time").Value.TrimEnd();

        ////        // convert date & time strings to DateTime object
        ////        DateTime tempDateTime = Convert_Strings_DateAndTime_To_SingleDateTimeObject(dateOnly, timeOnly, cultureInfo);

        ////        // convert DateTime object to a Long of ticks
        ////        long dateTimeInTicks = tempDateTime.Ticks;

        ////        NewsObject tempNewsObject = new NewsObject
        ////        {
        ////            // assign xml values to newsObject - uses xml <tag> names from xml file
        ////            Title = item.Element("title").Value,
        ////            CountryChar = item.Element("country").Value,
        ////            MarketImpact = item.Element("impact").Value,  // .Value - removes surrounding tags - giving only the value 
        ////            DateAndTime = tempDateTime,
        ////            DateInTicks = dateTimeInTicks  // ticks aren't really needed here as these objects won't be stored in the database
        ////        };

        ////        // add the tempNewsObject to list to return
        ////        listToReturn.Add(tempNewsObject);
        ////    }
        ////    return listToReturn;
        ////}




        //// old - not in use anymore - for demonstration purposes
        ////public static List<NewsObject> TestLINQQueryUsingXML(XDocument xmlTestFile)
        ////{
        ////    // LINQ queries (using xml file in Assets)           
        ////    List<NewsObject> linqQueryResultsList = new List<NewsObject>();

        ////    // sample selection using LINQ - GBP & USD currencies with 'High' impact status
        ////    var highestImpact = from myVar in xmlTestFile.Descendants("event")
        ////                        where myVar.Element("impact").Value == "High" &&
        ////                        (myVar.Element("country").Value == "GBP" || myVar.Element("country").Value == "USD")
        ////                        select myVar;

        ////    foreach (var item in highestImpact)
        ////    {
        ////        // get date & time values from XML (string)
        ////        string dateOnly = item.Element("date").Value.TrimEnd();
        ////        string timeOnly = item.Element("time").Value.TrimEnd();

        ////        // convert date & time strings to DateTime object
        ////        DateTime tempDateTime = Convert_Strings_DateAndTime_To_SingleDateTimeObject(dateOnly, timeOnly, cultureInfo);

        ////        // convert DateTime object to a Long of ticks
        ////        long dateTimeInTicks = tempDateTime.Ticks;

        ////        NewsObject tempNewsObject = new NewsObject
        ////        {
        ////            // assign xml values to newsObject - uses xml <tag> names from xml file
        ////            Title = item.Element("title").Value,
        ////            CountryChar = item.Element("country").Value,
        ////            MarketImpact = item.Element("impact").Value,  // .Value - removes surrounding tags - giving only the value
        ////            DateAndTime = tempDateTime,
        ////            DateInTicks = dateTimeInTicks  // ticks aren't really needed here as these objects won't be stored in the database
        ////        };

        ////        // add the tempNewsObject to list to return
        ////        linqQueryResultsList.Add(tempNewsObject);
        ////    }
        ////    return linqQueryResultsList;
        ////}
        ///


    }
}
   