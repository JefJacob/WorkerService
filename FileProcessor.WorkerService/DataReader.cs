using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.IO;
using System.Text;
using FileProcessor.Logic;
using NLog;

namespace FileProcessor.WorkerService
{
    public class DataReader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void ProcessFile(string folderPath)
        {
            logger.Info("Application Started | ProcessFile");

            //var file = Directory.GetFiles(@folderPath, "*.mdb").FirstOrDefault();
            string file = folderPath;
            if (File.Exists(file))
            {

                OdbcConnectionStringBuilder builder =
            new OdbcConnectionStringBuilder();
                builder.Driver = "Microsoft Access Driver (*.mdb, *.accdb)";
                builder.Add("DBQ", file);

                logger.Info(builder.ConnectionString);
                string fileName = Path.GetFileName(file);


                try
                {

                    if (DataTransfer.GetCompId(fileName) != 0)
                    {

                        ReadData(builder.ConnectionString, fileName);
                    }
                    else
                    {
                        logger.Error("Competition does not exist. Please create new Competition");
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Exception : " + ex.Message);
                }
            }
        }
        
        public static void ReadData(string connectionString, string fileName)
        {
            logger.Info("Application has started Reading Access DB");
            //Club data 
            string queryStringClub = "SELECT * FROM Team";

            using (var connection = new OdbcConnection(connectionString))
            {
                var command = new OdbcCommand(queryStringClub, connection);
                connection.Open();
                var reader = command.ExecuteReader();
                DataTransfer.ProcessClubData(reader);
                reader.Close();
            }

            //Standard events and masters
            string queryStringResults = "select Event_gender,Event_dist,Event_name,Event_note,Div_name,Full_Eventname,Rnd_ltr,Results.First_name,Results.Last_name,Results.Team_Abbr, "
            + " Results.Reg_no,Athlete.Birth_date,Athlete.Ath_Sex, Res_markDisplay,Res_wind,Res_place "
            + " from( Results inner join Athlete on Athlete.Ath_no = Results.Ath_no) "
            + " inner join Divisions on Divisions.Div_no = Results.Div_no "
            + " where Event_name not like '%athlon%' ";

            using (OdbcConnection connection = new OdbcConnection(connectionString))
            {
                OdbcCommand command = new OdbcCommand(queryStringResults, connection);
                connection.Open();
                OdbcDataReader reader = command.ExecuteReader();
                DataTransfer.ProcessResultsData(reader, fileName,"Standard and Masters Event");
                reader.Close();
            }

            //Relay events and sprint Medley
            string queryStringRelayResults = "select Event_gender,Event_dist,Event_name,Event_note,Div_name,Full_Eventname,Rnd_Ltr,  Athlete.First_name,Athlete.Last_name,  Results.Team_Abbr,Res_markDisplay,Res_wind,Res_place  ,Athlete.Birth_date, "
            + " Athlete.Reg_no,Athlete.Ath_sex "
            + " from(Results inner join Athlete ON Athlete.Ath_no = Results.RelayLeg1_Ath_no OR Athlete.Ath_no = Results.RelayLeg2_Ath_no OR Athlete.Ath_no = Results.RelayLeg3_Ath_no OR Athlete.Ath_no = Results.RelayLeg4_Ath_no) "
            + " inner join Divisions on Divisions.Div_no = Results.Div_no "
            + " where Full_Eventname like '%relay%'  OR Full_Eventname like '%Sprint Medley%'";

            using (OdbcConnection connection = new OdbcConnection(connectionString))
            {
                OdbcCommand command = new OdbcCommand(queryStringRelayResults, connection);
                connection.Open();
                OdbcDataReader reader = command.ExecuteReader();
                DataTransfer.ProcessResultsData(reader, fileName, "Relay events and sprint Medley");
                reader.Close();
            }

            //combined events
            string queryStringCombinedResults
                = "  select Results.Event_gender,Results.Event_dist,Results.Event_name,Results.MultiSubEvent_name,Results.Event_note,Div_name, "
                + " Results.Rnd_ltr,Results.First_name,Results.Last_name,Results.Team_Abbr, "
                + " Athlete.Reg_no, Athlete.Birth_date,Athlete.Ath_Sex, Res_markDisplay,Res_wind,Res_place,Res_note, Event_score "
                + " from(Results inner join Athlete on Athlete.Ath_no = Results.Ath_no) "
                + " inner join Divisions on Divisions.Div_no = Results.Div_no "
                + " where Event_name like '%athlon%' ";
            using (OdbcConnection connection = new OdbcConnection(connectionString))
            {
                OdbcCommand command = new OdbcCommand(queryStringCombinedResults, connection);
                connection.Open();
                OdbcDataReader reader = command.ExecuteReader();
                DataTransfer.ProcessResultsData(reader, fileName,"Combined Events");
                reader.Close();
            }

            

        }
    }
}
