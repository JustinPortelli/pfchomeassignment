using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.DataAccess
{
    public class ProductsRepository: ConnectionClass
    {
        public ProductsRepository() : base() { }






        public List<Product> GetProducts()
        {
            string sql = "Select Id, Name, Price, Ownerfk from products";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            

            MyConnection.Open();
            List<Product> results = new List<Product>();

            using (var reader = cmd.ExecuteReader())
            {
                while(reader.Read())
                {
                    Product p = new Product();
                    p.Id = reader.GetInt32(0);
                    p.Name = reader.GetString(1);
                    p.Price = reader.GetDouble(2);
                    p.OwnerFk = reader.GetString(3);
                    results.Add(p);
                }
            }

            MyConnection.Close();

            return results;
        }



        public List<Product> GetProducts(string email)
        {
            string sql = "Select Id, Name, Price, Ownerfk from products where ownerfk=@email";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@email", email);

            MyConnection.Open();
            List<Product> results = new List<Product>();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Product p = new Product();
                    p.Id = reader.GetInt32(0);
                    p.Name = reader.GetString(1);
                    p.Price = reader.GetDouble(2);
                    p.OwnerFk = reader.GetString(3);
                    results.Add(p);
                }
            }

            MyConnection.Close();

            return results;
        }



        public void DeleteProduct(int id)
        {
            string sql = "Delete from products where Id = @id";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@id", id);

            bool connectionOpenedInThisMethod = false;

            if (MyConnection.State == System.Data.ConnectionState.Closed)
            {
                MyConnection.Open();
                connectionOpenedInThisMethod = true;
            }

            if(MyTransaction != null)
            {
                cmd.Transaction = MyTransaction; //to participate in the opened trasaction (somewhere else), assign the Transaction property to the opened transaction
            }
            cmd.ExecuteNonQuery();

            if (connectionOpenedInThisMethod == true)
            {
                MyConnection.Close();
            }
        }






    }
}