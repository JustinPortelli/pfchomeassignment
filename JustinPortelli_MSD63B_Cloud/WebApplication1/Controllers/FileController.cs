using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.DataAccess;
using WebApplication1.Models;
using Google.Cloud.Storage.V1;
using Google.Apis.Storage.v1.Data;
using Npgsql;
using System.Web.Configuration;

namespace WebApplication1.Controllers
{
    public class FileController : Controller
    {
        // GET: File
        public ActionResult Index()
        {
            CacheRepository cr = new CacheRepository();
            UsersRepository ur = new UsersRepository();
            FileRepository fr = new FileRepository();
            var result = new List<Models.FileUpload>();
            result = cr.GetFilesFromCache(ur.GetUserID(User.Identity.Name));
            return View(result);
        }
        [HttpGet]
        public ActionResult FileUpload()
        { return View(); }

        public List<FileUpload> GetFiles(int user_id)
        {
            string connectionString = WebConfigurationManager.ConnectionStrings["postgresql"].ConnectionString;
            NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString);

            string sql = "SELECT * FROM files WHERE user_id = @user_id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, npgsqlConnection);
            cmd.Parameters.AddWithValue("@user_id", user_id);

            npgsqlConnection.Open();
            List<FileUpload> results = new List<FileUpload>();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    FileUpload fu = new FileUpload();
                    fu.id = reader.GetInt32(0);
                    fu.fileName = reader.GetString(1);
                    fu.fileUrl = reader.GetString(2);
                    fu.receiverEmail = reader.GetString(3);
                    results.Add(fu);
                }
                npgsqlConnection.Close();
                return results;
            }
        }


        [HttpPost]
        public ActionResult FileUpload(FileUpload sf, HttpPostedFileBase file)
        {
            try
            {
                if (file != null)
                {
                    var storage = StorageClient.Create();
                    string link = "";
                    using (var f = file.InputStream)
                    {
                        var filename = Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);
                        var storageObject = storage.UploadObject("justinportellipfc", filename, null, f);

                        link = "https://storage.cloud.google.com/justinportellipfc/" + filename;


                        if (null == storageObject.Acl)
                        {
                            storageObject.Acl = new List<ObjectAccessControl>();
                        }


                        storageObject.Acl.Add(new ObjectAccessControl()
                        {
                            Bucket = "justinportellipfc",
                            Entity = $"user-" + User.Identity.Name, //whereas ryanattard@gmail.com has to be replaced by a gmail email address who you want to have access granted
                            Role = "OWNER", //READER
                        });

                        storageObject.Acl.Add(new ObjectAccessControl()
                        {
                            Bucket = "justinportellinpfc",
                            Entity = $"user-" + sf.receiverEmail, //whereas ryanattard@gmail.com has to be replaced by a gmail email address who you want to have access granted
                            Role = "READER", //READER
                        });

                        var updatedObject = storage.UpdateObject(storageObject, new UpdateObjectOptions()
                        {
                            // Avoid race conditions.
                            IfMetagenerationMatch = storageObject.Metageneration,
                        });
                        FileRepository fr = new FileRepository();
                        UsersRepository ur = new UsersRepository();
                        CacheRepository cr = new CacheRepository();
                        fr.AddFile(filename.ToString(), link, sf.receiverEmail, ur.GetUserID(User.Identity.Name));
                        cr.UpdateCache(GetFiles(ur.GetUserID(User.Identity.Name)), ur.GetUserID(User.Identity.Name));

                        PubSubRepository psr = new PubSubRepository();
                        psr.AddToEmailQueue(sf);
                    }


                    ViewBag.Message = "File uploaded";
                    LogsRepository logr = new LogsRepository();
                    logr.WriteLogEntry("File was Uploaded");
                }

            }
            catch (Exception ex)
            {
                LogsRepository logr = new LogsRepository();
                logr.LogError(ex);
                ViewBag.Message = "File was not uploaded";
            }
            return View();
        }

        public ActionResult ListFiles()
        {
            CacheRepository cr = new CacheRepository();
            UsersRepository ur = new UsersRepository();
            var result = new List<Models.FileUpload>();
            result = cr.GetFilesFromCache(ur.GetUserID(User.Identity.Name));
            return View(result);
        }
    }
}