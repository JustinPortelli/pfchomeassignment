using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Npgsql;
using System.Web.Configuration;

namespace WebApplication1.DataAccess
{
    public class UsersRepository: ConnectionClass
    {
        public UsersRepository() : base()
        { }

        public void AddUser(string email, string username)
        {
            try
            {
                string sql = "INSERT into users (email, username, lastloggedin) values (@email, @username, @lastloggedin)";

                NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@lastloggedin", DateTime.Now); ;


                MyConnection.Open();
                cmd.ExecuteNonQuery();
                MyConnection.Close();
                LogsRepository logr = new LogsRepository();
                logr.WriteLogEntry("User has been added");
            }
            catch(Exception ex)
            {
                LogsRepository logr = new LogsRepository();
                logr.LogError(ex);
            }
        }

        public bool DoesEmailExist(string email)
        {
            string sql = "Select Count(*) from users where email = @email";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@email", email);

            MyConnection.Open();

            bool result = Convert.ToBoolean(cmd.ExecuteScalar());

            MyConnection.Close();

            return result;
        }

        public void UpdateLastLoggedIn(string email)
        {
            try
            {
                string sql = "update users set lastloggedin = @lastloggedin where email = @email";

                NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@lastloggedin", DateTime.Now); ;

                MyConnection.Open();
                cmd.ExecuteNonQuery();
                MyConnection.Close();
                LogsRepository logr = new LogsRepository();
                logr.WriteLogEntry("loggedin time has been updated");
            }
            catch (Exception ex)
            {
                LogsRepository logr = new LogsRepository();
                logr.LogError(ex);
            }
        }

        public int GetUserID(string email)
        {
            string connectionString = WebConfigurationManager.ConnectionStrings["postgresql"].ConnectionString;
            NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString);
            npgsqlConnection.Open();

            NpgsqlCommand npgsqlCommand = new NpgsqlCommand();
            npgsqlCommand.Connection = npgsqlConnection;
            NpgsqlTransaction mTrans;

            mTrans = npgsqlConnection.BeginTransaction();
            npgsqlCommand.Transaction = mTrans;

            int result = -1;

            try
            {
                npgsqlCommand.CommandText = "SELECT id FROM users WHERE email = @email";
                npgsqlCommand.Parameters.AddWithValue("@email", email);
                mTrans.Commit();
                NpgsqlDataReader datar = npgsqlCommand.ExecuteReader();
                while (datar.Read())
                    result = (int)datar[0];

            }
            catch (Exception ex)
            {
                LogsRepository logr = new LogsRepository();
                logr.LogError(ex);
                mTrans.Rollback();
            }
            finally
            {
                npgsqlConnection.Close();
            }
            return result;
        }
    }
}