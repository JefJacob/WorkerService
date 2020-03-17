using System;
using System.Collections.Generic;
using System.Text;
using FileProcessor.Entities;
using NLog;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;

namespace FileProcessor.Repository
{
    public class ClubRepo
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public static int AddClub(ClubEntity club)
        {
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["AODB"].ConnectionString);
            string insertStatement =
                "INSERT into Clubs " +
                "(ClubCode,ClubName) " +
                "VALUES (@ClubCode,@ClubName)";
            SqlCommand insertCommand =
                new SqlCommand(insertStatement, connection);

            insertCommand.Parameters.AddWithValue(
                "@ClubCode", club.ClubCode);
            insertCommand.Parameters.AddWithValue(
                "@ClubName", club.ClubName);



            try
            {
                connection.Open();
                int value = insertCommand.ExecuteNonQuery();
                return value;

            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("Violation of PRIMARY KEY constraint"))
                    logger.Error("Duplicate:" + club.ClubCode);
                return 0;
            }
            finally
            {
                connection.Close();
            }
        }

        public static ClubEntity GetClub(String ClubCode)
        {
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["AODB"].ConnectionString);
            string selectStatement
                = "SELECT * "

                + "FROM Clubs "
                + "WHERE ClubCode = @Code";
            SqlCommand selectCommand =
                new SqlCommand(selectStatement, connection);
            selectCommand.Parameters.AddWithValue(
                "@Code", ClubCode);
            try
            {
                connection.Open();
                SqlDataReader proReader =
                    selectCommand.ExecuteReader(
                        System.Data.CommandBehavior.SingleRow);
                if (proReader.Read())
                {
                    ClubEntity club = new ClubEntity();
                    club.ClubCode = proReader["ClubCode"].ToString();
                    club.ClubName = proReader["ClubName"].ToString();

                    return club;
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public static List<ClubEntity> GetClubs()
        {
            List<ClubEntity> clubEntities = new List<ClubEntity>();
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["AODB"].ConnectionString);
            string selectStatement
                = "SELECT * "

                + "FROM Clubs ";

            SqlCommand selectCommand =
                new SqlCommand(selectStatement, connection);

            try
            {
                connection.Open();
                SqlDataReader proReader =
                    selectCommand.ExecuteReader();
                if (proReader.HasRows)
                {
                    DataTable dt = new DataTable();
                    dt.Load(proReader);

                    clubEntities = (from x in dt.AsEnumerable()
                                    select new ClubEntity()
                                    {
                                        ClubCode = x["ClubCode"].ToString(),
                                        ClubName = x["ClubName"].ToString()
                                    }).ToList();

                }
                return clubEntities;

            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
