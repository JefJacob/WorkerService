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
            CompetitionEntity comp = new CompetitionEntity();
            comp = CompetitionRepo.GetCompetition(fileName);

            logger.Info("Started Processing Result Details/Standard");
            while (reader.Read())
            {
                try
                {

                    //Console.WriteLine(reader["Full_Eventname"].ToString() + reader["Rnd_ltr"].ToString() + reader["First_name"].ToString() + reader["Last_name"].ToString() + reader["Team_Abbr"].ToString() + reader["Reg_no"].ToString() + reader["Res_markDisplay"].ToString() + reader["Res_wind"].ToString() + reader["Res_place"].ToString());
                    String division = reader["Div_name"].ToString();
                    string eventGender = reader["Event_gender"].ToString();
                    string eventName = "";
                    DateTime Birth_date;
                    if (reader["Birth_date"].ToString() == "")
                        Birth_date = DateTime.Parse("1900-01-01 00:00:00");
                    else
                        Birth_date = DateTime.Parse(reader["Birth_date"].ToString());

                    if (division.Contains("AMB"))
                        division = division.Replace("AMB", "Ambulatory");
                    if (division.Contains("WC"))
                        division = division.Replace("WC", "Wheelchair");

                    string[] div_categories = { "U6", "U8", "U10", "U12", "U14", "U16", "U18" };
                    if (div_categories.Contains(division))
                        if (eventGender.Equals("M"))
                            eventGender = "Boys";
                        else if (eventGender.Equals("F"))
                            eventGender = "Girls";
                        else
                            eventGender = "Mixed";
                    else
                        if (eventGender.Equals("M"))
                        eventGender = "Men";
                    else if (eventGender.Equals("F"))
                        eventGender = "Women";
                    else
                        eventGender = "Mixed";

                    //for masters 
                    if (division.Equals("Masters"))
                    {
                        DateTime sdate = comp.StartDate;
                        int Years = new DateTime(sdate.Subtract(Birth_date).Ticks).Year - 1;
                        if (Years < 30)
                            division = "29 & Under Masters";
                        else if (Years < 35)
                            division = "30 Masters";
                        else if (Years < 40)
                            division = "35 Masters";
                        else if (Years < 45)
                            division = "40 Masters";
                        else if (Years < 50)
                            division = "45 Masters";
                        else if (Years < 55)
                            division = "50 Masters";
                        else if (Years < 60)
                            division = "55 Masters";
                        else if (Years < 65)
                            division = "60 Masters";
                        else if (Years < 70)
                            division = "65 Masters";
                        else if (Years < 75)
                            division = "70 Masters";
                        else if (Years < 80)
                            division = "75 Masters";
                        else
                            division = "80 Masters";
                    }
                    if (reader["Event_name"].ToString().Contains("athlon"))//combined events
                    {
                        if (!reader["Event_dist"].ToString().Equals("0"))
                            if (!reader["Event_note"].ToString().Equals(""))
                                eventName = reader["Event_dist"].ToString() + " Meters " + reader["MultiSubEvent_name"].ToString() + " " + reader["Event_name"].ToString() + " " + reader["Event_note"].ToString();
                            else
                                eventName = reader["Event_dist"].ToString() + " Meters " + " " + reader["MultiSubEvent_name"].ToString() + " " + reader["Event_name"].ToString();
                        else
                           if (!reader["Event_note"].ToString().Equals(""))
                            eventName = reader["MultiSubEvent_name"].ToString() + " " + reader["Event_name"].ToString() + " " + reader["Event_note"].ToString();
                        else
                            eventName = reader["MultiSubEvent_name"].ToString() + " " + reader["Event_name"].ToString();
                    }
                    else
                    {
                        if (!reader["Event_dist"].ToString().Equals("0"))
                            if (!reader["Event_note"].ToString().Equals(""))
                                eventName = reader["Event_dist"].ToString() + " Meters " + reader["Event_name"].ToString() + " " + reader["Event_note"].ToString();
                            else
                                eventName = reader["Event_dist"].ToString() + " Meters " + reader["Event_name"].ToString();
                        else
                            if (!reader["Event_note"].ToString().Equals(""))
                            eventName = reader["Event_name"].ToString() + " " + reader["Event_note"].ToString();
                        else
                            eventName = reader["Event_name"].ToString();
                    }
                    eventName = eventName.Trim();
                    String Rnd_ltr = reader["Rnd_ltr"].ToString();
                    String First_name = reader["First_name"].ToString();
                    String Last_name = reader["Last_name"].ToString();
                    String Team_Abbr = reader["Team_Abbr"].ToString();
                    String Reg_no = reader["Reg_no"].ToString();
                    String Ath_Sex = reader["Ath_Sex"].ToString();
                    String Res_markDisplay = reader["Res_markDisplay"].ToString();
                    String Res_wind = reader["Res_wind"].ToString();
                    String Res_place = reader["Res_place"].ToString();

                    ResultEntity result = new ResultEntity();
                    int eventId = AthleteEventRepo.GetAthleteEventId(eventGender + " " + eventName + " " + division, Rnd_ltr);
                    if (eventId == 0)
                    {

                        AthleteEventEntity athleteEvent = new AthleteEventEntity();
                        athleteEvent.EventGender = eventGender;
                        athleteEvent.EventRound = Rnd_ltr;
                        athleteEvent.EventDivision = division;
                        athleteEvent.EventName = eventName;
                        AthleteEventRepo.AddAthleteEvent(athleteEvent);
                        eventId = AthleteEventRepo.GetAthleteEventId(eventGender + " " + eventName + " " + division, Rnd_ltr);
                    }
                    int athleteid = 0;
                    athleteid = AthleteRepo.GetAthleteIdByACNum(Reg_no);
                    if (athleteid == 0)
                    {
                        athleteid = AthleteRepo.GetAthleteIdByName(First_name, Last_name, Birth_date);
                        if (athleteid == 0)
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
                            if (String.IsNullOrWhiteSpace(Reg_no))
                                result.AthleteId = AthleteRepo.GetAthleteIdByName(First_name, Last_name, Birth_date);
                            else
                                result.AthleteId = AthleteRepo.GetAthleteIdByACNum(Reg_no);
                        }
                        else
                        {
                            result.AthleteId = athleteid;
                        }
                    }
                    else
                    {
                        result.AthleteId = athleteid;
                    }

                    result.CompId = GetCompId(fileName);
                    result.EventId = eventId;
                    result.Mark = Res_markDisplay;
                    if (reader["Event_name"].ToString().Contains("athlon"))
                        result.Position = Convert.ToInt32(reader["Event_score"].ToString());
                    else
                        result.Position = Convert.ToInt32(Res_place);
                    result.Wind = Res_wind;

                    if (ResultRepo.CheckDuplicate(result) == 0)
                        ResultRepo.AddResult(result);
                    else
                        logger.Error("Duplicate Result: " + fileName.Replace(".mdb", "") + "->" + eventGender + " " + eventName + " " + division + " " + Rnd_ltr + "->" + First_name + " " + Last_name + " DOB: " + Birth_date.ToString("dd-MM-yyyy"));
                }
                catch (Exception e)
                {
                    logger.Error("Exception : " + e.Message);
                }
                finally { }
            }
            logger.Info("Completed Processing Result Details/Standard");
        }

    }
}
