using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Npgsql;

namespace WebApplication1.DataAccess
{
    public class FileRepository : ConnectionClass
    {
        public FileRepository() : base() { }

        public void AddFile(string filename, string fileurl, string receiver, int uid)
        {
            string sql = "INSERT INTO files (filename, fileurl, receiver, user_id) VALUES (@filename, @fileurl, @receiver, @user_id)";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@filename", filename);
            cmd.Parameters.AddWithValue("@fileurl", fileurl);
            cmd.Parameters.AddWithValue("@receiver", receiver);
            cmd.Parameters.AddWithValue("@user_id", uid);

            MyConnection.Open();
            cmd.ExecuteNonQuery();
            MyConnection.Close();
        }
    }
}