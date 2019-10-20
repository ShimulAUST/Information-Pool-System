using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using InfopoolFinal.Models;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Web.UI;

namespace InfopoolFinal.Controllers
{
    public class AdminController : Controller
    {


        //Shimul
        //string connectString = @"Data Source=DESKTOP-CBKATEE\SHIMULSQL; Initial Catalog = ISDFINAL; Integrated Security=True";
        //ASHNA
        string connectString = @"Data Source=DESKTOP-DOUTL4V; Initial Catalog = ISDFINAL; Integrated Security=True";
        //MAYEESHA
        //string connectString = @"Data Source=DESKTOP-L60BB46; Initial Catalog = ISDFINAL; Integrated Security=True";
        // GET: Admin
        public ActionResult Index(string adminname)
        {
            //get admin home page
            ViewBag.adminname = adminname;    
            return View();
        }

        //Add links of news method
        [HttpGet]
        public ActionResult AddLink(string adminName)
        {
            AddLinkViewModel addLinkViewModel = new AddLinkViewModel();
            addLinkViewModel.AdminNameFromView = adminName;
            return View(addLinkViewModel);
        }
        [HttpPost]
        public ActionResult AddLink(Models.AddLinkViewModel addLink)
        {
            ViewData["sessionString"] = System.Web.HttpContext.Current.Session["sessionString"] as String;
            var adminDataTable = new DataTable();
            string adminName = addLink.AdminNameFromView;

            string btype = "none";
            switch (addLink.newsType)
            {
                case 0: btype = "Breaking"; break;
                case 1: btype = "World"; break;
                case 2: btype = "Technology"; break;
                case 3: btype = "Fashion"; break;
                case 4: btype = "Business"; break;
                case 5: btype = "StockMarket"; break;
                case 6: btype = "Sports"; break;
            }
            string stype = "none";
            switch (addLink.newsSource)
            {
                case 0: stype = "BBC"; break;
                case 1: stype = "CNN"; break;
                case 2: stype = "GOOGLE NEWS"; break;
                case 3: stype = "FOX NEWS"; break;
                case 4: stype = "PROTHOM ALO"; break;
                case 5: stype = "ITTEFAQ"; break;
                case 6: stype = "BANGLADESH PRATIDIN"; break;
                case 7: stype = "JUGANTAR"; break;
                case 8: stype = "KALER KANTHA"; break;
                case 9: stype = "NEW AGE"; break;
                case 10: stype = "DAILY STAR"; break;
                case 11: stype = "DAILY SUN"; break;
            }
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();
                var query = "Select * from ADMINS where AdminUserName='" + ViewData["sessionString"] + "';";
                var sqlDa = new SqlDataAdapter(query, sqlConnection);
                sqlDa.Fill(adminDataTable);
            }
            int id = 0;
            try
            {
                id = (int)adminDataTable.Rows[0][0];
            }
            catch (Exception e)
            {
                TempData["msg"] = "<script>alert('You have to be logged in')</script>";
                return RedirectToAction("Index", "Home");
            }
            if (id > 0)
            {
                
                    using (SqlConnection sqlConnection = new SqlConnection(connectString))
                    {
                        sqlConnection.Open();
                        string query = "INSERT INTO News(Headline,NewsLink,SourceName,NewsType,AdminId) VALUES " +
                            "(@Head,@Link,@Source,@Type,@adminId)";
                        SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                        sqlCommand.Parameters.AddWithValue("@Head", addLink.newsHeadline);
                        sqlCommand.Parameters.AddWithValue("@Link", addLink.newsLink);
                        sqlCommand.Parameters.AddWithValue("@Source", stype);
                        sqlCommand.Parameters.AddWithValue("@Type", btype);
                        sqlCommand.Parameters.AddWithValue("@adminId", id);
                        
                        sqlCommand.ExecuteNonQuery();
                        
                    }
                
            }
            TempData["msg"] = "<script>alert('You have successfully added a news link')</script>";
            string url1 = string.Format("/Admin/Index?adminName={0}", ViewData["sessionString"]);
            return Redirect(url1);
            
        }

        //Add events method

        [HttpGet]
        public ActionResult AddEvent(string adminName)
        {
            AddEventViewModel addEventViewModel = new AddEventViewModel();
            addEventViewModel.AdminNameFromView = adminName;
            return View(addEventViewModel);
        }
        [HttpPost]
        public ActionResult AddEvent(AddEventViewModel addevent)
        {
            ViewData["sessionString"] = System.Web.HttpContext.Current.Session["sessionString"] as String;
            var adminDataTable = new DataTable();
            string adminName = addevent.AdminNameFromView;
            string fileName = "";
            string filePath = "";
            var file = addevent.file[0];

            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();
                var query = "Select * from ADMINS where AdminUserName='" + ViewData["sessionString"] + "';";
                var sqlDa = new SqlDataAdapter(query, sqlConnection);
                sqlDa.Fill(adminDataTable);
            }
            int id = 0;
            try
            {
                id = (int)adminDataTable.Rows[0][0];
            }
            catch (Exception e)
            {
                TempData["msg"] = "<script>alert('You have to be logged in')</script>";
                return RedirectToAction("Index", "Home");
            }
            try
            {
                if (id > 0)
                {
                    if (file.ContentLength > 0)
                    {
                        fileName = Path.GetFileName(file.FileName);
                        filePath = Path.Combine("H:/3-2/Software Development V/Project/final project/InfopoolFinal/images/Events", fileName);
                        string toSave = "H:/3-2/Software Development V/Project/final project/InfopoolFinal/images/Events/" + fileName;
                        file.SaveAs(filePath);
                        using (SqlConnection sqlConnection = new SqlConnection(connectString))
                        {
                            sqlConnection.Open();
                            string query = "INSERT INTO Events(EventTitle,EventDetail,EventTime,EventDate,Venue,EventCoverPicture,AdminId) VALUES " +
                                "(@Title,@Detail,@Time,@Date,@Venue,@EventCoverPicture,@adminId)";
                            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                            sqlCommand.Parameters.AddWithValue("@Title", addevent.eventTitle);
                            sqlCommand.Parameters.AddWithValue("@Detail", addevent.eventDetail);
                            sqlCommand.Parameters.AddWithValue("@Time", addevent.eventTime);
                            sqlCommand.Parameters.AddWithValue("@Date", addevent.eventDate);
                            sqlCommand.Parameters.AddWithValue("@Venue", addevent.eventVenue);
                            sqlCommand.Parameters.AddWithValue("@EventCoverPicture", toSave);
                            sqlCommand.Parameters.AddWithValue("@adminId", id);
                            sqlCommand.ExecuteNonQuery();
                        }
                        TempData["msg"] = "<script>alert('You have successfully added an event')</script>";
                        string url1 = string.Format("/Admin/Index?adminName={0}", ViewData["sessionString"]);
                        return Redirect(url1);
                    }
                    else
                    {
                        TempData["msg"] = "<script>alert('We were unable to add the event')</script>";
                        string url1 = string.Format("/Admin/Index?adminName={0}", ViewData["sessionString"]);
                        return Redirect(url1);
                    }
                }
                else
                {
                    TempData["msg"] = "<script>alert('You have to be logged in')</script>";
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                TempData["msg"] = "<script>alert('You have to be logged in')</script>";
                return RedirectToAction("Index", "Home");
            }
        }

        //for see complaints
        public ActionResult SeeComplaints(string adminName)
        {

            ViewData["sessionString"] = System.Web.HttpContext.Current.Session["sessionString"] as String;
            if (ViewData["sessionString"] != null)
            {
                var complaints = new DataTable();
                using (var sqlCon = new SqlConnection(connectString))
                {
                    sqlCon.Open();
                    var query = "Select * from Complaint Where ComplaintStatus='" + 0 + "';";
                    var sqlDa = new SqlDataAdapter(query, sqlCon);
                    sqlDa.Fill(complaints);
                }
                return View(complaints);
            }
            else
            {
                TempData["msg"] = "<script>alert('You have to be logged in')</script>";
                return RedirectToAction("Index", "Home");
            }

        }
        
        //for sending email for seeComplaints Resolve button
        public ActionResult SendEmail(int id,string adminName)
        {
            ViewData["sessionString"] = System.Web.HttpContext.Current.Session["sessionString"] as String;
            var userDataTable = new DataTable();
            var adminDataTable = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();
                var query = "Select * from ADMINS where AdminUserName='" + adminName + "';";
                var sqlDa = new SqlDataAdapter(query, sqlConnection);
                sqlDa.Fill(adminDataTable);

            }
            try
            {
                int adminid = (int)adminDataTable.Rows[0][0];
            }
            catch (Exception e)
            {
                TempData["msg"] = "<script>alert('You have to be logged in')</script>";
                return RedirectToAction("Index", "Home");
            
            }
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();
                var query = "Select * from COMPLAINT where ComplaintId ='" + id + "';";
                var sqlDa = new SqlDataAdapter(query, sqlConnection);
                sqlDa.Fill(userDataTable);

            }
            string userEmail = (string)userDataTable.Rows[0][4];
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();

                string query = "UPDATE Complaint SET ComplaintStatus ='" + 1 + "'WHERE ComplaintId=@ComplaintId;";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@ComplaintId", id);
                sqlCommand.ExecuteNonQuery();

            }


            var fromAddress = new MailAddress("infopool2019@gmail.com");
            var fromPassword = "S@123456789";
            var toAddress = new MailAddress(userEmail);

            string subject = "No Reply";
            
            
            string body = "We are trying to solve your problem as soon as possible. Please Stay with us. ";

            string Signature = "From" + "\n" +
                "Information Pool Team";

            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body+"\n"+Signature
        })

                smtp.Send(message);
            TempData["msg"] = "<script>alert('The user was notified via mail')</script>";
            string url1 = string.Format("/Admin/SeeComplaints?adminName={0}", ViewData["sessionString"]);
            return Redirect(url1);

        }

        //Ignore method for see complaint ignore button
        public ActionResult IgnoreEmail(int id)
        { 
            ViewData["sessionString"] = System.Web.HttpContext.Current.Session["sessionString"] as String;
            var userDataTable = new DataTable();
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();
                var query = "Select * from COMPLAINT where ComplaintId ='" + id + "';";
                var sqlDa = new SqlDataAdapter(query, sqlConnection);
                sqlDa.Fill(userDataTable);
            }
            string userEmail = (string)userDataTable.Rows[0][4];
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();
                string query = "UPDATE Complaint SET ComplaintStatus ='" + 2 + "'WHERE ComplaintId=@ComplaintId;";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@ComplaintId", id);
                sqlCommand.ExecuteNonQuery();
            }


            var fromAddress = new MailAddress("infopool2019@gmail.com");
            var fromPassword = "S@123456789";
            var toAddress = new MailAddress(userEmail);

            string subject = "No Reply";


            string body = "We have looked into your problem and resolved it. ";

            string Signature = "From" + "\n" +
                "Information Pool Team";

            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body + "\n" + Signature
            })

                smtp.Send(message);
            TempData["msg"] = "<script>alert('The complaint was ignored and the user was notified via mail')</script>";

            
            return RedirectToAction("SeeComplaints");
        }
        //see pending blog method
        public ActionResult SeePendingBlogs(string adminName)
        {
            var pendingBlogs = new DataTable();

            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from blogs where ApprovalStatus='" + 0 + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(pendingBlogs);
            }
            return View(pendingBlogs);


        }

        //update pending blog approval button methon
        public ActionResult UpdatePendingBlogApproval(int id)
        {

            var adminDataTable = new DataTable();

            ViewData["sessionString"] = System.Web.HttpContext.Current.Session["sessionString"] as String;
            if (ViewData["sessionString"] != null)
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    var query = "Select * from ADMINS where AdminUserName='" + ViewData["sessionString"] + "';";
                    var sqlDa = new SqlDataAdapter(query, sqlConnection);
                    sqlDa.Fill(adminDataTable);

                }
                int adminid = (int)adminDataTable.Rows[0][0];

                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();

                    string query = "UPDATE BLOGS SET ApprovalStatus ='" + 1 + "',AdminId='" + adminid + "'WHERE BlogId=@BlogId;";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@BlogId", id);


                    sqlCommand.ExecuteNonQuery();

                }

                var userDataTable = new DataTable();
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    var query = "Select * from blogs where BlogId="+id;
                    var sqlDa = new SqlDataAdapter(query, sqlConnection);
                    sqlDa.Fill(userDataTable);
                }
                int userid =(int) userDataTable.Rows[0][6];
                var userDataTable2 = new DataTable();
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    var query = "Select * from userdetails where userId=" + userid;
                    var sqlDa = new SqlDataAdapter(query, sqlConnection);
                    sqlDa.Fill(userDataTable2);
                }
                string userEmail = userDataTable2.Rows[0][2].ToString();
                var fromAddress = new MailAddress("infopool2019@gmail.com");
                var fromPassword = "S@123456789";
                var toAddress = new MailAddress(userEmail);

                string subject = "No Reply";


                string body = "We have approved your blog. Thanks for staying with us. ";

                string Signature = "From" + "\n" +
                    "Information Pool Team";

                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body + "\n" + Signature
                })

                    smtp.Send(message);
                TempData["msg"] = "<script>alert('The Blog was approved')</script>";
                string url1 = string.Format("/Admin/SeePendingBlogs?adminName={0}", ViewData["sessionString"]);
                return Redirect(url1);
            }
            else
            {
                TempData["msg"] = "<script>alert('You have to be logged in')</script>";
                return RedirectToAction("Index", "Home");
            }
        }

        //Remove Link Button Method
        public ActionResult RemoveLink(string adminName)
        {
            ViewData["sessionString"] = System.Web.HttpContext.Current.Session["sessionString"] as String;
            if (ViewData["sessionString"] != null)
            {
                var Link = new DataTable();
                using (var sqlCon = new SqlConnection(connectString))
                {
                    sqlCon.Open();
                    var query = "Select * from news";
                    var sqlDa = new SqlDataAdapter(query, sqlCon);
                    sqlDa.Fill(Link);
                }
                return View(Link);
            }
            else
            {
                TempData["msg"] = "<script>alert('You have to be logged in')</script>";
                return RedirectToAction("Index", "Home");
            }
        }

        //Remove Button Action in RemoveLink Table
        public ActionResult Delete(int id)
        {
            ViewData["sessionString"] = System.Web.HttpContext.Current.Session["sessionString"] as String;
            if (ViewData["sessionString"] != null)
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    string query = "DELETE FROM NEWS WHERE NewsId = @NewsId";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@NewsId", id);
                    sqlCommand.ExecuteNonQuery();
                }
                TempData["msg"] = "<script>alert('The link was removed')</script>";
                string url1 = string.Format("/Admin/RemoveLink?adminName={0}", ViewData["sessionString"]);
                return Redirect(url1);
            }
            else
            {
                TempData["msg"] = "<script>alert('You have to be logged in')</script>";
                return RedirectToAction("Index", "Home");
            }
        }

        //Remove Event Button Method
        public ActionResult RemoveEvent(string adminName)
        {

            ViewData["sessionString"] = System.Web.HttpContext.Current.Session["sessionString"] as String;

            var events = new DataTable();

            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from events";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(events);
            }
            return View(events);
        }

        //Remove button in Remove Event Table
        public ActionResult DeleteEvent(int id)
        {
            ViewData["sessionString"] = System.Web.HttpContext.Current.Session["sessionString"] as String;

            if (ViewData["sessionString"] != null)
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    string query = "DELETE FROM events WHERE EventId = @EventId";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@EventId", id);
                    sqlCommand.ExecuteNonQuery();

                }
                TempData["msg"] = "<script>alert('The event was removed')</script>";
                string url1 = string.Format("/Admin/RemoveEvent?adminName={0}", ViewData["sessionString"]);
                return Redirect(url1);
            }
            else
            {
                TempData["msg"] = "<script>alert('You have to be logged in')</script>";
                return RedirectToAction("Index", "Home");
            }
        }
        
        //See all blogs button method
        public ActionResult SeeAllBlogs(string adminName)
        {
            var ads = new DataTable();

            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from blogs where ApprovalStatus='" + 1 + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(ads);
            }
            return View(ads);
        }

        //Dis approved Blog
        public ActionResult DisApprovalBlogs(int id)
        {
            ViewData["sessionString"] = System.Web.HttpContext.Current.Session["sessionString"] as String;
            if (ViewData["sessionString"] != null)
            {

                var adminDataTable = new DataTable();


                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    var query = "Select * from ADMINS where AdminUserName='" + ViewData["sessionString"] + "';";
                    var sqlDa = new SqlDataAdapter(query, sqlConnection);
                    sqlDa.Fill(adminDataTable);

                }
                int adminid = (int)adminDataTable.Rows[0][0];
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();

                    string query = "UPDATE BLOGS SET ApprovalStatus ='" + 0 + "',AdminId='" + adminid + "'WHERE BlogId=@BlogId;";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@BlogId", id);


                    sqlCommand.ExecuteNonQuery();

                }
                TempData["msg"] = "<script>alert('The blog was disapproved')</script>";
                string url1 = string.Format("/Admin/SeeAllBlogs?adminName={0}", ViewData["sessionString"]);
                return Redirect(url1);

            }
            else
            {
                TempData["msg"] = "<script>alert('You have to be logged in')</script>";
                return RedirectToAction("Index", "Home");
            }

        }

        //delete blog button in see all blogs

        public ActionResult DeleteBlog(int id)
        {
            ViewData["sessionString"] = System.Web.HttpContext.Current.Session["sessionString"] as String;

            if (ViewData["sessionString"] != null)
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();

                    string query = "DELETE FROM Like_comment WHERE BlogId = @BlogId";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@BlogId", id);
                    sqlCommand.ExecuteNonQuery();

                }
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();

                    string query = "DELETE FROM Blogtags WHERE BlogId = @BlogId";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@BlogId", id);
                    sqlCommand.ExecuteNonQuery();

                }
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    string query = "DELETE FROM Blogs WHERE BlogId = @BlogId";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@BlogId", id);
                    sqlCommand.ExecuteNonQuery();

                }
                TempData["msg"] = "<script>alert('The blog was deleted')</script>";
                string url1 = string.Format("/Admin/SeeAllBlogs?adminName={0}", ViewData["sessionString"]);
                return Redirect(url1);
            }
            else
            {
                TempData["msg"] = "<script>alert('You have to be logged in')</script>";
                return RedirectToAction("Index", "Home");
            }
        }


        //UpdatePendingAdsApproval
        public ActionResult UpdatePendingAdsApproval(int id)
        {
            ViewData["sessionString"] = System.Web.HttpContext.Current.Session["sessionString"] as String;
            if (ViewData["sessionString"] != null)
            {

                var adminDataTable = new DataTable();
                var userDataTable = new DataTable();
                var userTable = new DataTable();
          


                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    var query = "Select * from ADMINS where AdminUserName='" + ViewData["sessionString"] + "';";
                    var sqlDa = new SqlDataAdapter(query, sqlConnection);
                    sqlDa.Fill(adminDataTable);

                }
                int adminid = (int)adminDataTable.Rows[0][0];
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    var query = "Select * from Ads where AdsId ='" + id + "';";
                    var sqlDa = new SqlDataAdapter(query, sqlConnection);
                    sqlDa.Fill(userDataTable);

                }
                int userid = (int)userDataTable.Rows[0][7];
               

                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    var query = "Select * from UserDetails where UserId ='" + userid + "';";
                    var sqlDa = new SqlDataAdapter(query, sqlConnection);
                    sqlDa.Fill(userTable);

                }
                string email = userTable.Rows[0][2].ToString();

                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();

                    string query = "UPDATE Ads SET AdsApprovalStatus ='" + 2 + "',AdminId='" + adminid + "'WHERE AdsId=@AdsId;";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@AdsId", id);


                    sqlCommand.ExecuteNonQuery();
                }
                

            
               
                var fromAddress = new MailAddress("infopool2019@gmail.com");
                var fromPassword = "S@123456789";
                var toAddress = new MailAddress(email);

                string subject = "No Reply";
                string body = "We have approved your Ad. It will be displayed on our site for 48 hours";
                string Signature = "From" +"\n"+
                    "Information Pool Team";
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body ="\n"+ body + "\n\n" + Signature
                })

                    smtp.Send(message);

                TempData["msg"] = "<script>alert('The ad was approved and email notification was sent')</script>";
                string url1 = string.Format("/Admin/SeePendingAds?adminName={0}", ViewData["sessionString"]);
                return Redirect(url1);
                
            }
            else
            {
                TempData["msg"] = "<script>alert('You have to be logged in')</script>";
                return RedirectToAction("Index","Home");
            }

        }

        public ActionResult SeeReports(string adminName)
        {

            return View();
        }
       
        
        public ActionResult SeePayments()
        {
            var ads = new DataTable();

            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from Ads where AdsApprovalStatus='" + 2 + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(ads);
            }
            return View(ads);
        }
        public ActionResult SeePendingAds(string adminName)
        {

            var ads = new DataTable();

            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from ads where AdsApprovalStatus='" + 0 + "' and AdsTransactionId IS NOT NULL;";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(ads);
            }
            return View(ads);
        }
        public ActionResult Logout()
        {
            ViewData["sessionString"] = null;
            TempData["msg"] = "<script>alert('You have logged out')</script>";
            return RedirectToAction("Index", "Home");
        }

        public ActionResult SeePendingBlogger()
        {
            var pendingBlogger = new DataTable();

            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where BloggerOrNot='" + 1 + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(pendingBlogger);
            }
            return View(pendingBlogger);
           
        }

        //update blogger

        public ActionResult UpdatePendinBloggerApproval(int id)
        {
            ViewData["sessionString"] = System.Web.HttpContext.Current.Session["sessionString"] as String;
            if (ViewData["sessionString"] != null)
            {

                var adminDataTable = new DataTable();
                var userDataTable = new DataTable();


                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    var query = "Select * from ADMINS where AdminUserName='" + ViewData["sessionString"] + "';";
                    var sqlDa = new SqlDataAdapter(query, sqlConnection);
                    sqlDa.Fill(adminDataTable);

                }
                int adminid = (int)adminDataTable.Rows[0][0];

                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    var query = "Select * from UserDetails where UserId='" +id+ "';";
                    var sqlDa = new SqlDataAdapter(query, sqlConnection);
                    sqlDa.Fill(userDataTable);

                }
                string email = userDataTable.Rows[0][2].ToString();

                /**/

                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();

                    string query = "UPDATE UserDetails SET BloggerOrNot ='" + 2 + "'WHERE UserId=@UserId;";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@UserId", id);


                    sqlCommand.ExecuteNonQuery();

                }
                var fromAddress = new MailAddress("infopool2019@gmail.com");
                var fromPassword = "S@123456789";
                var toAddress = new MailAddress(email);

                string subject = "No Reply";
                string body = "You are now a blogger. You can now post different kind of blogs in our website. Thank you for staying with us. ";
                string Signature = "From" +"\n\n"+
                    "Information Pool Team";
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body + "\n" + Signature
                })

                    smtp.Send(message);


                TempData["msg"] = "<script>alert('The blogger was approved and notified via email')</script>";
                string url1 = string.Format("/Admin/SeePendingBlogger?adminName={0}", ViewData["sessionString"]);
                return Redirect(url1);

            }
            else
            {
                TempData["msg"] = "<script>alert('You have to be logged in')</script>";
                return RedirectToAction("Index", "Home");
            }

        }




        //Admin Login Method
        [HttpGet]
        public ActionResult AdminLogin()
        {

            return View(new Models.AdminViewModel());
        }

        [HttpPost]
        public ActionResult AdminLogin(Models.AdminViewModel admin1)
        {


            var news1 = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from Admins where AdminUserName='" + admin1.adminUserName + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(news1);

            }

            //matching passwords
            try
            {
                if (news1.Rows[0][2].ToString() == admin1.adminPass)
                {
                    System.Web.HttpContext.Current.Session["sessionString"] = admin1.adminUserName;
                    TempData["msg"] = "<script>alert('You have successfully logged in')</script>";
                    string url = string.Format("/Admin/Index?adminName={0}", admin1.adminUserName);
                    return Redirect(url);
                }
                else {
                    TempData["msg"] = "<script>alert('Login Failure. Please try again')</script>";
                    return RedirectToAction("Index", "Home");
                } 
            }
            catch(Exception e)
            {
                TempData["msg"] = "<script>alert('Login Failure. Please try again')</script>";
                return RedirectToAction("Index", "Home");
            }
        }
    }




}