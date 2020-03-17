using System;
using System.Collections.Generic;
using System.Text;
using NLog;
using System.Data.Odbc;
using FileProcessor.Repository;
using FileProcessor.Entities;
using System.Linq;

namespace FileProcessor.Logic
{
    public class DataTransfer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public static int GetCompId(string fileName)
        {

            return CompetitionRepo.GetCompetitionId(fileName);
        }
        public static void ProcessClubData(OdbcDataReader reader)
        {
            logger.Info("Started Processing Club Details");
            List<ClubEntity> sList = new List<ClubEntity>();
            while (reader.Read())
            {
                sList.Add(
                    new ClubEntity()
                    {
                        ClubCode = reader["Team_abbr"].ToString(),
                        ClubName = reader["Team_name"].ToString()
                    });
            }
            List<ClubEntity> dList = new List<ClubEntity>();
            dList = ClubRepo.GetClubs();
            List<ClubEntity> newClub = sList.Except(dList).ToList();
            if (newClub.Count != 0)
            {
                foreach (ClubEntity x in newClub)
                {
                    ClubRepo.AddClub(x);
                }
            }


            logger.Info("Processing Club Details Completed");
        }

        public static void ProcessResultsData(OdbcDataReader reader, string fileName)
        {
            logger.Info("Started Processing Result Details/Standard");
            while (reader.Read())
            {
                try
                {

                    //Console.WriteLine(reader["Full_Eventname"].ToString() + reader["Rnd_ltr"].ToString() + reader["First_name"].ToString() + reader["Last_name"].ToString() + reader["Team_Abbr"].ToString() + reader["Reg_no"].ToString() + reader["Res_markDisplay"].ToString() + reader["Res_wind"].ToString() + reader["Res_place"].ToString());
                    String Full_Eventname = reader["Full_Eventname"].ToString();
                    if (Full_Eventname.Contains("AMB"))
                        Full_Eventname = Full_Eventname.Replace("AMB", "Ambulatory");
                    if (Full_Eventname.Contains("WC"))
                        Full_Eventname = Full_Eventname.Replace("WC", "Wheelchair");

                    String Rnd_ltr = reader["Rnd_ltr"].ToString();
                    String First_name = reader["First_name"].ToString();
                    String Last_name = reader["Last_name"].ToString();
                    String Team_Abbr = reader["Team_Abbr"].ToString();
                    String Reg_no = reader["Reg_no"].ToString();
                    DateTime Birth_date;
                    if (reader["Birth_date"].ToString() == "")
                        Birth_date = DateTime.Parse("1900-01-01 00:00:00");
                    else
                        Birth_date = DateTime.Parse(reader["Birth_date"].ToString());
                    String Ath_Sex = reader["Ath_Sex"].ToString();
                    String Res_markDisplay = reader["Res_markDisplay"].ToString();
                    String Res_wind = reader["Res_wind"].ToString();
                    String Res_place = reader["Res_place"].ToString();

                    ResultEntity result = new ResultEntity();

                    if (AthleteEventRepo.GetAthleteEventId(Full_Eventname, Rnd_ltr) == 0)
                    {
                        string[] words = Full_Eventname.Split(' ');
                        List<string> wList = new List<string>(words);
                        AthleteEventEntity athleteEvent = new AthleteEventEntity();
                        athleteEvent.EventGender = words[0];
                        athleteEvent.EventRound = Rnd_ltr;
                        athleteEvent.EventDivision = words[words.Length - 1];
                        //athleteEvent.Name = Full_Eventname.Replace(words[0], "").Replace(words[words.Length-1], "").Trim();
                        wList.RemoveAt(0);
                        wList.RemoveAt(wList.Count - 1);
                        athleteEvent.EventName = string.Join(" ", wList);
                        AthleteEventRepo.AddAthleteEvent(athleteEvent);
                    }

                    if (AthleteRepo.GetAthleteIdByACNum(Reg_no) == 0)
                    {
                        if (AthleteRepo.GetAthleteIdByName(First_name, Last_name, Birth_date) == 0)
                        {
                            AthleteEntity athlete = new AthleteEntity();
                            athlete.ACNum = Reg_no;
                            athlete.ClubAffiliationSince = DateTime.Now;
                            athlete.ClubCode = Team_Abbr;
                            athlete.DOB = Birth_date;
                            athlete.FirstName = First_name;
                            athlete.LastName = Last_name;
                            athlete.AthleteGender = Ath_Sex;
                            athlete.Address = "";
                            athlete.City = "";
                            athlete.AthleteEmail = "";
                            athlete.Phone = "";
                            athlete.HeadShot = "";
                            athlete.AthleteSpecialNoteId = 0;
                            AthleteRepo.AddAthlete(athlete);
                            result.AthleteId = AthleteRepo.GetAthleteIdByName(First_name, Last_name, Birth_date);
                        }
                        else
                        {
                            result.AthleteId = AthleteRepo.GetAthleteIdByName(First_name, Last_name, Birth_date);
                        }
                    }
                    else
                    {
                        result.AthleteId = AthleteRepo.GetAthleteIdByACNum(Reg_no);
                    }

                    result.CompId = GetCompId(fileName);
                    result.EventId = AthleteEventRepo.GetAthleteEventId(Full_Eventname, Rnd_ltr);
                    result.Mark = Res_markDisplay;
                    result.Position = Convert.ToInt32(Res_place);
                    result.Wind = Res_wind;

                    if (ResultRepo.CheckDuplicate(result) == 0)
                        ResultRepo.AddResult(result);
                    else
                        logger.Error("Duplicate Result: " + fileName.Replace(".mdb", "") + "->" + Full_Eventname + " " + Rnd_ltr + "->" + First_name + " " + Last_name + " DOB: " + Birth_date.ToString("dd-MM-yyyy"));
                }
                catch (Exception e)
                {
                    logger.Error("Exception : " + e.Message);
                }
                //finally { reader.Close(); }
            }
            logger.Info("Completed Processing Result Details/Standard");
        }

        public static void ProcessResultsDataRelay(OdbcDataReader reader, string fileName)
        {
            logger.Info("Started Processing Result Details/Relay");
            while (reader.Read())
            {
                try
                {

                    //Console.WriteLine(reader["Full_Eventname"].ToString() + reader["Rnd_ltr"].ToString() + reader["First_name"].ToString() + reader["Last_name"].ToString() + reader["Team_Abbr"].ToString() + reader["Reg_no"].ToString() + reader["Res_markDisplay"].ToString() + reader["Res_wind"].ToString() + reader["Res_place"].ToString());
                    String Full_Eventname = reader["Full_Eventname"].ToString();
                    if (Full_Eventname.Contains("AMB"))
                        Full_Eventname = Full_Eventname.Replace("AMB", "Ambulatory");
                    if (Full_Eventname.Contains("WC"))
                        Full_Eventname = Full_Eventname.Replace("WC", "Wheelchair");
                    String Relay_ltr = reader["Relay_ltr"].ToString();
                    String First_name = reader["First_name"].ToString();
                    String Last_name = reader["Last_name"].ToString();
                    String Team_Abbr = reader["Team_Abbr"].ToString();
                    String Reg_no = reader["Reg_no"].ToString();
                    DateTime Birth_date;
                    if (reader["Birth_date"].ToString() == "")
                        Birth_date = DateTime.Parse("1900-01-01 00:00:00");
                    else
                        Birth_date = DateTime.Parse(reader["Birth_date"].ToString());
                    String Ath_Sex = reader["Ath_Sex"].ToString();
                    String Res_markDisplay = reader["Res_markDisplay"].ToString();
                    String Res_wind = reader["Res_wind"].ToString();
                    String Res_place = reader["Res_place"].ToString();

                    ResultEntity result = new ResultEntity();

                    if (AthleteEventRepo.GetAthleteEventId(Full_Eventname, Relay_ltr) == 0)
                    {
                        string[] words = Full_Eventname.Split(' ');
                        List<string> wList = new List<string>(words);
                        AthleteEventEntity athleteEvent = new AthleteEventEntity();
                        athleteEvent.EventGender = words[0];
                        athleteEvent.EventRound = Relay_ltr;

                        athleteEvent.EventDivision = words[words.Length - 1];
                        wList.RemoveAt(0);
                        wList.RemoveAt(wList.Count - 1);
                        athleteEvent.EventName = string.Join(" ", wList);
                        //athleteEvent.Name = Full_Eventname.Replace(words[0], "").Replace(words[words.Length - 1], "").Trim();
                        AthleteEventRepo.AddAthleteEvent(athleteEvent);

                    }

                    if (AthleteRepo.GetAthleteIdByACNum(Reg_no) == 0)
                    {
                        if (AthleteRepo.GetAthleteIdByName(First_name, Last_name, Birth_date) == 0)
                        {
                            AthleteEntity athlete = new AthleteEntity();
                            athlete.ACNum = Reg_no;
                            athlete.ClubAffiliationSince = DateTime.Now;
                            athlete.ClubCode = Team_Abbr;
                            athlete.DOB = Birth_date;
                            athlete.FirstName = First_name;
                            athlete.LastName = Last_name;
                            athlete.AthleteGender = Ath_Sex;
                            athlete.Address = "";
                            athlete.City = "";
                            athlete.AthleteEmail = "";
                            athlete.Phone = "";
                            athlete.HeadShot = "";
                            athlete.AthleteSpecialNoteId = 0;
                            AthleteRepo.AddAthlete(athlete);
                            result.AthleteId = AthleteRepo.GetAthleteIdByName(First_name, Last_name, Birth_date);
                        }
                        else
                        {
                            result.AthleteId = AthleteRepo.GetAthleteIdByName(First_name, Last_name, Birth_date);
                        }
                    }
                    else
                    {
                        result.AthleteId = AthleteRepo.GetAthleteIdByACNum(Reg_no);
                    }

                    result.CompId = GetCompId(fileName);
                    result.EventId = AthleteEventRepo.GetAthleteEventId(Full_Eventname, Relay_ltr);
                    result.Mark = Res_markDisplay;
                    result.Position = Convert.ToInt32(Res_place);
                    result.Wind = Res_wind;
                    if (ResultRepo.CheckDuplicate(result) == 0)
                        ResultRepo.AddResult(result);
                    else
                        logger.Error("Duplicate Result: " + fileName.Replace(".mdb", "") + "->" + Full_Eventname + " " + Relay_ltr + "->" + First_name + " " + Last_name + " DOB: " + Birth_date.ToString("dd-MM-yyyy"));
                }
                catch (Exception e)
                {
                    logger.Error("Exception : " + e.Message);
                }
                finally { }
            }
            logger.Info("Completed Processing Result Details/Relay");
        }


        public static void ProcessResultsDataCombined(OdbcDataReader reader, string fileName)
        {
            logger.Info("Started Processing Result Details/Combined");
            while (reader.Read())
            {
                try
                {
                    //Splitting and processing AThlete Events
                    String Full_Eventname = reader["Full_Eventname"].ToString();
                    if (Full_Eventname.Contains("AMB"))
                        Full_Eventname = Full_Eventname.Replace("AMB", "Ambulatory");
                    if (Full_Eventname.Contains("WC"))
                        Full_Eventname = Full_Eventname.Replace("WC", "Wheelchair");

                    string gender = "";
                    string division = "";
                    string eventName = "";
                    if (Full_Eventname.Contains("29 & Under"))
                        Full_Eventname = Full_Eventname.Replace("29 & Under", "29");
                    string[] words = Full_Eventname.Split(' ');

                    //M29 & Under Indoor Pentathlon Masters
                    //Men Long Jump Indoor Pentathlon 50 Masters
                    //COmbined master events
                    if (words[0].StartsWith("M"))
                    {
                        gender = "Men";
                        division = words[0].Replace("M", "");
                    }
                    else if (words[0].StartsWith("W"))
                    {
                        gender = "Women";
                        division = words[0].Replace("W", "");
                    }
                    else
                    {
                        gender = "Mixed";
                        division = words[0].Replace("X", "");
                    }
                    if (reader["Div_name"].ToString().Equals("Masters"))
                    {
                        if (Full_Eventname.Contains("29"))
                            division = "29 & Under " + reader["Div_name"].ToString();
                        else
                            division = division + " " + reader["Div_name"].ToString();
                    }
                    else
                        division = division + " " + reader["Div_name"].ToString();
                    List<string> wList = new List<string>(words);
                    wList.RemoveAt(0);//removes  
                    wList.RemoveAt(wList.Count - 1);

                    if (!reader["Event_dist"].ToString().Equals("0"))
                        if (!reader["Res_note"].ToString().Equals(""))
                            eventName = reader["Event_dist"].ToString() + " Meters " + reader["MultiSubEvent_name"].ToString() + " " + reader["Res_note"].ToString() + " " + string.Join(" ", wList);
                        else
                            eventName = reader["Event_dist"].ToString() + " Meters " + reader["MultiSubEvent_name"].ToString() + " " + string.Join(" ", wList);
                    else
                        if (!reader["Res_note"].ToString().Equals(""))
                        eventName = reader["MultiSubEvent_name"].ToString() + " " + reader["Res_note"].ToString() + " " + string.Join(" ", wList);
                    else
                        eventName = reader["MultiSubEvent_name"].ToString() + " " + string.Join(" ", wList);

                    String Rnd_ltr = reader["Rnd_ltr"].ToString();
                    String First_name = reader["First_name"].ToString();
                    String Last_name = reader["Last_name"].ToString();
                    String Team_Abbr = reader["Team_Abbr"].ToString();
                    String Reg_no = reader["Reg_no"].ToString();
                    DateTime Birth_date;
                    if (reader["Birth_date"].ToString() == "")
                        Birth_date = DateTime.Parse("1900-01-01 00:00:00");
                    else
                        Birth_date = DateTime.Parse(reader["Birth_date"].ToString());
                    String Ath_Sex = reader["Ath_Sex"].ToString();
                    String Res_markDisplay = reader["Res_markDisplay"].ToString();
                    String Res_wind = reader["Res_wind"].ToString();
                    String Event_score = reader["Event_score"].ToString();


                    ResultEntity result = new ResultEntity();

                    if (AthleteEventRepo.GetAthleteEventId(gender + " " + eventName + " " + division, Rnd_ltr) == 0)
                    {

                        AthleteEventEntity athleteEvent = new AthleteEventEntity();
                        athleteEvent.EventGender = gender;
                        athleteEvent.EventRound = Rnd_ltr;
                        athleteEvent.EventDivision = division;
                        athleteEvent.EventName = eventName;

                        AthleteEventRepo.AddAthleteEvent(athleteEvent);

                    }

                    if (AthleteRepo.GetAthleteIdByACNum(Reg_no) == 0)
                    {
                        if (AthleteRepo.GetAthleteIdByName(First_name, Last_name, Birth_date) == 0)
                        {
                            AthleteEntity athlete = new AthleteEntity();
                            athlete.ACNum = Reg_no;
                            athlete.ClubAffiliationSince = DateTime.Now;
                            athlete.ClubCode = Team_Abbr;
                            athlete.DOB = Birth_date;
                            athlete.FirstName = First_name;
                            athlete.LastName = Last_name;
                            athlete.AthleteGender = Ath_Sex;
                            athlete.Address = "";
                            athlete.City = "";
                            athlete.AthleteEmail = "";
                            athlete.Phone = "";
                            athlete.HeadShot = "";
                            athlete.AthleteSpecialNoteId = 0;
                            AthleteRepo.AddAthlete(athlete);
                            result.AthleteId = AthleteRepo.GetAthleteIdByName(First_name, Last_name, Birth_date);
                        }
                        else
                        {
                            result.AthleteId = AthleteRepo.GetAthleteIdByName(First_name, Last_name, Birth_date);
                        }
                    }
                    else
                    {
                        result.AthleteId = AthleteRepo.GetAthleteIdByACNum(Reg_no);
                    }

                    result.CompId = GetCompId(fileName);
                    result.EventId = AthleteEventRepo.GetAthleteEventId(gender + " " + eventName + " " + division, Rnd_ltr);
                    result.Mark = Res_markDisplay;
                    result.Position = Convert.ToInt32(Event_score);
                    result.Wind = Res_wind;

                    if (ResultRepo.CheckDuplicate(result) == 0)
                        ResultRepo.AddResult(result);
                    else
                        logger.Error("Duplicate Result: " + fileName.Replace(".mdb", "") + "->" + Full_Eventname + " " + Rnd_ltr + "->" + First_name + " " + Last_name + " DOB: " + Birth_date.ToString("dd-MM-yyyy"));
                }
                catch (Exception e)
                {

                    logger.Error("Exception : " + e.Message);
                }
                finally { }
            }
            logger.Info("Completed Processing Result Details/Combined");
        }

        public static void ProcessResultsDataMasters(OdbcDataReader reader, string fileName)
        {
            logger.Info("Started Processing Result Details/Masters");
            while (reader.Read())
            {
                try
                {

                    String Full_Eventname = reader["Full_Eventname"].ToString();
                    if (Full_Eventname.Contains("AMB"))
                        Full_Eventname = Full_Eventname.Replace("AMB", "Ambulatory");
                    if (Full_Eventname.Contains("WC"))
                        Full_Eventname = Full_Eventname.Replace("WC", "Wheelchair");
                    String Rnd_ltr = reader["Rnd_ltr"].ToString();
                    String First_name = reader["First_name"].ToString();
                    String Last_name = reader["Last_name"].ToString();
                    String Team_Abbr = reader["Team_Abbr"].ToString();
                    String Reg_no = reader["Reg_no"].ToString();
                    DateTime Birth_date;
                    if (reader["Birth_date"].ToString() == "")
                        Birth_date = DateTime.Parse("1900-01-01 00:00:00");
                    else
                        Birth_date = DateTime.Parse(reader["Birth_date"].ToString());
                    String Ath_Sex = reader["Ath_Sex"].ToString();
                    String Res_markDisplay = reader["Res_markDisplay"].ToString();
                    String Res_wind = reader["Res_wind"].ToString();
                    String Res_place = reader["Res_place"].ToString();
                    Full_Eventname = "Mixed " + reader["Full_Eventname"].ToString();

                    ResultEntity result = new ResultEntity();



                    Full_Eventname = Full_Eventname.Replace("X", "");
                    string[] words = Full_Eventname.Split(' ');
                    List<string> wList = new List<string>(words);
                    string Gender = words[0];
                    string EventRound = Rnd_ltr;
                    string Division = "";
                    wList.RemoveAt(0);
                    wList.RemoveAt(wList.Count - 1);
                    string Name = string.Join(" ", wList);

                    if (Full_Eventname.Contains("29 & Under"))
                    {
                        Division = "29 & Under " + words[words.Length - 1];
                        Name = Name.Replace("29 & Under ", "").Trim();
                    }
                    else
                    {
                        Division = words[1] + " " + words[words.Length - 1];
                        wList.RemoveAt(0);
                        Name = string.Join(" ", wList);
                        Name = Name.Trim();
                    }
                    if (AthleteEventRepo.GetAthleteEventId(Gender + " " + Name + " " + Division, Rnd_ltr) == 0)
                    {

                        AthleteEventEntity athleteEvent = new AthleteEventEntity();
                        athleteEvent.EventGender = Gender;
                        athleteEvent.EventRound = Rnd_ltr;
                        athleteEvent.EventDivision = Division;
                        athleteEvent.EventName = Name;

                        AthleteEventRepo.AddAthleteEvent(athleteEvent);
                    }

                    if (AthleteRepo.GetAthleteIdByACNum(Reg_no) == 0)
                    {
                        if (AthleteRepo.GetAthleteIdByName(First_name, Last_name, Birth_date) == 0)
                        {
                            AthleteEntity athlete = new AthleteEntity();
                            athlete.ACNum = Reg_no;
                            athlete.ClubAffiliationSince = DateTime.Now;
                            athlete.ClubCode = Team_Abbr;
                            athlete.DOB = Birth_date;
                            athlete.FirstName = First_name;
                            athlete.LastName = Last_name;
                            athlete.AthleteGender = Ath_Sex;
                            athlete.Address = "";
                            athlete.City = "";
                            athlete.AthleteEmail = "";
                            athlete.Phone = "";
                            athlete.HeadShot = "";
                            athlete.AthleteSpecialNoteId = 0;
                            AthleteRepo.AddAthlete(athlete);
                            result.AthleteId = AthleteRepo.GetAthleteIdByName(First_name, Last_name, Birth_date);
                        }
                        else
                        {
                            result.AthleteId = AthleteRepo.GetAthleteIdByName(First_name, Last_name, Birth_date);
                        }
                    }
                    else
                    {
                        result.AthleteId = AthleteRepo.GetAthleteIdByACNum(Reg_no);
                    }

                    result.CompId = GetCompId(fileName);
                    result.EventId = AthleteEventRepo.GetAthleteEventId(Gender + " " + Name + " " + Division, Rnd_ltr);
                    result.Mark = Res_markDisplay;
                    result.Position = Convert.ToInt32(Res_place);
                    result.Wind = Res_wind;

                    if (ResultRepo.CheckDuplicate(result) == 0)
                        ResultRepo.AddResult(result);
                    else
                        logger.Error("Duplicate Result: " + fileName.Replace(".mdb", "") + "->" + Gender + " " + Name + " " + Division + " " + Rnd_ltr + "->" + First_name + " " + Last_name + " DOB: " + Birth_date.ToString("dd-MM-yyyy"));
                }
                catch (Exception e)
                {
                    logger.Error("Exception : " + e.Message);
                }
                finally { }
            }
            logger.Info("Completed Processing Result Details/Masters");
        }

    }
}
