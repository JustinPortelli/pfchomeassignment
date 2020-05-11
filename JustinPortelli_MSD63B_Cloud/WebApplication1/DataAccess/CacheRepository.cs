using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Web;
using StackExchange.Redis;
using WebApplication1.Models;

namespace WebApplication1.DataAccess
{
    public class CacheRepository
    {

        private IDatabase db;
        public CacheRepository()
        {
            // var connection = ConnectionMultiplexer.Connect("localhost"); //localhost if cache server is installed locally after downloaded from https://github.com/rgl/redis/downloads 
            // connection to your REDISLABS.com db as in the next line NOTE: DO NOT USE MY CONNECTION BECAUSE I HAVE A LIMIT OF 30MB...CREATE YOUR OWN ACCOUNT
            var connection = ConnectionMultiplexer.Connect("redis-14570.c80.us-east-1-2.ec2.cloud.redislabs.com:14570,password=AmVNTcBBHuq6kUJOe2RNB09ChMUMD8ft"); //<< connection here should be to your redis database from REDISLABS.COM
            db = connection.GetDatabase();
        }

        /// <summary>
        /// store a list of products in cache
        /// </summary>
        /// <param name="Files"></param>
        public void UpdateCache(List<FileUpload> file, int uid)
        {
            try
            {
                if (db.KeyExists("file" + uid))
                    db.KeyDelete("file" + uid);

                string jsonString;
                jsonString = JsonSerializer.Serialize(file);

                db.StringSet("file" + uid, jsonString);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error while attempting to update cache: " + ex);
            }
        }

        public List<FileUpload> GetFilesFromCache(int uid)
        {
            if (db.KeyExists("file" + uid) == true)
            {
                var files = JsonSerializer.Deserialize<List<FileUpload>>(db.StringGet("file" + uid));
                return files;
            }
            else
            {
                return new List<FileUpload>();
            }
        }

        public void DeleteCache()
        {
            try
            {
                var connection = ConnectionMultiplexer.Connect("redis-14570.c80.us-east-1-2.ec2.cloud.redislabs.com:14570,password=AmVNTcBBHuq6kUJOe2RNB09ChMUMD8ft");
                var server = connection.GetServer("redis-14570.c80.us-east-1-2.ec2.cloud.redislabs.com:14570");
                server.FlushDatabase();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error when attempting to delete cache: " + ex);
            }

        }

        /// <summary>
        /// Gets a list of products from cache
        /// </summary>
        /// <returns></returns>
        public List<Product> GetProductsFromCache()
        {
            if (db.KeyExists("products") == true)
            {
                var products = JsonSerializer.Deserialize<List<Product>>(db.StringGet("products"));
                return products;
            }
            else
            {
                return new List<Product>();
            }
        }
    }
}