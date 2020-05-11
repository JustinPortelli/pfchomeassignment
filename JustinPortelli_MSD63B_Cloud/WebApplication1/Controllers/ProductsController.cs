using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.DataAccess;
using WebApplication1.Models;
using Google.Cloud.Storage.V1;
using Google.Apis.Storage.v1.Data;

namespace WebApplication1.Controllers
{
    public class ProductsController : Controller
    {
        [HttpGet]
        public ActionResult Create()
        { return View(); }

        [HttpPost]
        public ActionResult Create(Product p, HttpPostedFileBase file)
        {
            //upload image related to product on the bucket
            try
            {
                if(file!= null)
                { 
                #region Uploading file on Cloud Storage
                var storage = StorageClient.Create();
                string link = "";

                    using (var f = file.InputStream)
                    {
                        var filename = Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);
                        var storageObject = storage.UploadObject("pfct001", filename, null, f);
                        
                        link = "https://storage.cloud.google.com/pfct001/"+file.FileName;

                        if (null == storageObject.Acl)
                        {
                            storageObject.Acl = new List<ObjectAccessControl>();
                        }


                        storageObject.Acl.Add(new ObjectAccessControl()
                        {
                            Bucket = "pfct001",
                            Entity = $"user-" + "portellijustin198@gmail.com", //whereas ryanattard@gmail.com has to be replaced by a gmail email address who you want to have access granted
                            Role = "OWNER", //READER
                        });

                        storageObject.Acl.Add(new ObjectAccessControl()
                        {
                            Bucket = "pfct001",
                            Entity = $"user-" + "portellijustin198@gmail.com", //whereas ryanattard@gmail.com has to be replaced by a gmail email address who you want to have access granted
                            Role = "READER", //READER
                        });

                        var updatedObject = storage.UpdateObject(storageObject, new UpdateObjectOptions()
                        {
                            // Avoid race conditions.
                            IfMetagenerationMatch = storageObject.Metageneration,
                        });
                    }
                    //store details in a relational db including the filename/link
                    #endregion


                    
                }
                    #region Storing details of product in db [INCOMPLETE]
                    p.OwnerFk = "portellijustin198@gmail.com"; //User.Identity.Name
                    ProductsRepository pr = new ProductsRepository();
                   
                //pr.AddProduct(p);
                    #endregion

                    #region Updating Cache with latest list of Products from db

                    //enable: after you switch on db
                    CacheRepository cr = new CacheRepository();
                    //cr.UpdateCache(pr.GetProducts());

                    #endregion
                    PubSubRepository psr = new PubSubRepository();
                    //psr.AddToEmailQueue(p); //adding it to queue to be sent as an email later on.
                ViewBag.Message = "Product created successfully";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Product failed to be created; " + ex.Message;
            }

            return View();
        }


        // GET: Products
        public ActionResult Index()
        {

            #region get products of db - removed....instead insert next region
            ProductsRepository pr = new ProductsRepository();
            //var products = pr.GetProducts(); //gets products from db
            #endregion

            #region instead of getting products from DB, to make your application faster , you load them from the cache
            CacheRepository cr = new CacheRepository();
            var products = cr.GetProductsFromCache();
            #endregion

            return View("Index",products);
        }

        public ActionResult Delete(int id)
        {
            try
            {
                
                ProductsRepository pr = new ProductsRepository();
                pr.DeleteProduct(id);
            }
            catch (Exception ex)
            {
                new LogsRepository().LogError(ex);
            }   
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeleteAll(int[] ids)
        {
            //1. Requirement when opening a transaction: Connection has to be opened

            ProductsRepository pr = new ProductsRepository();
            pr.MyConnection.Open();

            pr.MyTransaction = pr.MyConnection.BeginTransaction(); //from this point onwards all code executed against the db will remain pending

            try
            {
                foreach (int id in ids)
                {
                    pr.DeleteProduct(id);
                }

                pr.MyTransaction.Commit(); //Commit: you are confirming the changes in the db
            }
            catch (Exception ex)
            {
                //Log the exception on the cloud
                pr.MyTransaction.Rollback(); //Rollback: it will reverse all the changes done within the try-clause in the db
            }

            pr.MyConnection.Close();

            return RedirectToAction("Index");
        }
    }
}