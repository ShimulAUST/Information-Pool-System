using InfopoolFinal.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace InfopoolFinal.Controllers
{
    public class UserController : Controller
    {

        //Shimul
        //string connectString = @"Data Source=DESKTOP-CBKATEE\SHIMULSQL; Initial Catalog = ISDFINAL; Integrated Security=True";
        //ASHNA
        string connectString = @"Data Source=DESKTOP-DOUTL4V; Initial Catalog = ISDFINAL; Integrated Security=True";
        //MAYEESHA
        //string connectString = @"Data Source=DESKTOP-L60BB46; Initial Catalog = ISDFINAL; Integrated Security=True";

        //HOME PAGE OF NORMAL USER
        public ActionResult Index(string email)
        {
            ViewBag.email = email;
            TempData.Keep();
            return View();

        }
        //HOME PAGE OF USER WHO APPLIED TO BE BLOGGER
        public ActionResult Index1(string email)
        {
            ViewBag.email = email;
            TempData.Keep();
            return View();

        }

        //PROFILE PAGE OF USER
        public ActionResult Profile(string email, UserInfoModel u)
        {
            ViewBag.email = email;
            TempData["currentSession"] = u;
            TempData.Keep();

            //OBTAINING USER DETAILS
            var prof = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where Email='" + email + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(prof);
            }
            return View(prof);


        }

        //PROCEDURE FOR PICKING HOME PAGE OF DIFFERENT USERS: USED FOR PROFILE BUTTON IN NAVBAR 
        public ActionResult PickProfile()
        {
            UserInfoModel u = new UserInfoModel();
            if (TempData["currentSession"] != null)
                u = TempData["currentSession"] as UserInfoModel;
            string email = u.Email;

            //OBTAINING USER'S STATUS
            var user = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where Email='" + email + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(user);
            }
            byte bloggerStat = (byte)user.Rows[0][4];

            //FOR NORMAL USER
            if (bloggerStat == 0)
            {
                return RedirectToAction("Index", "User", new { email = email });

            }
            //USER APPLIED TO BE BLOGGER
            else if (bloggerStat == 1)
            {
                return RedirectToAction("Index1", "User", new { email = email });

            }
            //BLOGGER
            else
            {
                return RedirectToAction("Index", "Blogger", new { email = email });

            }

        }


        //GET METHOD FOR CHANGING PASSWORD
        [HttpGet]
        public ActionResult ChangePasword(string email, UserInfoModel u)
        {
            TempData["currentSession"] = u;
            ChangePasswordModel chViewModel = new ChangePasswordModel();
            chViewModel.Email = email;
            return View(chViewModel);

        }

        //POST METHOD FOR CHANGING PASSWORD
        [HttpPost]
        public ActionResult ChangePasword(Models.ChangePasswordModel ch)
        {
            string email = ch.Email;

            //OBTAINING USER ID AND DETAILS
            var user = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where Email='" + email + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(user);
            }
            int id = (int)user.Rows[0][0];
            string old = user.Rows[0][3].ToString();

            //MATCHING OLD PASSWORD WITH INPUT
            if (ch.OldPassword.Equals(old))
            {
                //UPDATING PASSWORD IF PASSWORDS MATCH
                if (ch.NewPassword.Equals(ch.ConfirmNewPass))
                {
                    using (SqlConnection sqlConnection = new SqlConnection(connectString))
                    {
                        sqlConnection.Open();
                        string query = "Update Userdetails set pass=@newpass where userId=" + id;

                        SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                        sqlCommand.Parameters.AddWithValue("@newpass", ch.NewPassword);

                        sqlCommand.ExecuteNonQuery();

                    }
                }
                else
                {
                    //new pass and confirm pass doesnt match
                    TempData["msg"] = "<script>alert('Your new password did not match the confirmation password')</script>";
                    ChangePasswordModel chViewModel1 = new ChangePasswordModel();
                    chViewModel1.Email = email;
                    return View(chViewModel1);
                }


            }
            else
            {
                //old password doesnt match
                TempData["msg"] = "<script>alert('Your Old Password is incorrect. Please try again.')</script>";
                ChangePasswordModel chViewModel1 = new ChangePasswordModel();
                chViewModel1.Email = email;
                return View(chViewModel1);
            }
            byte bloggerStat = (byte)user.Rows[0][4];
            TempData["msg"] = "<script>alert('You have successfully changed your password')</script>";
            //NORMAL USER REDIRECT TO HOMEPAGE
            if (bloggerStat == 0)
            {
                return RedirectToAction("Index", "User", new { email = email });

            }
            //USER-WHO-APPLIED-TO-BE-BLOGGER REDIRECT TO HOMEPAGE
            else if (bloggerStat == 1)
            {
                return RedirectToAction("Index1", "User", new { email = email });

            }
            //BLOGGER REDIRECT TO HOMEPAGE
            else
            {
                return RedirectToAction("Index", "Blogger", new { email = email });

            }


        }

        //GET METHOD FOR PAYING FOR ADS
        [HttpGet]
        public ActionResult Payment(int uid, int adId, string email, UserInfoModel u)
        {
            TempData["currentSession"] = u;
            PayViewModel payViewModel = new PayViewModel();
            payViewModel.AdId = adId;
            payViewModel.Userid = uid;
            payViewModel.EmailFromView = email;
            //RETRIEVING TOTAL PAYABLE AMOUNT
            var ad = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select AdsTotalAmount from ads where adsid=" + adId + ";";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(ad);
            }
            decimal amnt = (decimal)ad.Rows[0][0];
            payViewModel.Amount = Decimal.ToDouble(amnt);
            return View(payViewModel);
        }


        //POST METHOD FOR PAYING FOR ADS
        [HttpPost]
        public ActionResult Payment(Models.PayViewModel pay)
        {
            string email = pay.EmailFromView;

            //OBTAINING USER DETAILS
            var user = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where Email='" + email + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(user);
            }

            //UPDATING AD TRANSACTION ID
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();
                string query = "Update ads set AdsTransactionId=@trxid,AdsPaymentStatus=1 where adsid=" + pay.AdId;
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@trxid", pay.Trxid);


                sqlCommand.ExecuteNonQuery();

            }
            TempData["msg"] = "<script>alert('You have successfully paid for an ad')</script>";
            //REDIRECTING ACCORDING TO BLOGGER STATUS
            byte bloggerStat = (byte)user.Rows[0][4];
            //NORMAL USER
            if (bloggerStat == 0)
            {
                return RedirectToAction("Index", "User", new { email = email });

            }
            //USER WHO APPLIED TO BE BLOGGER
            else if (bloggerStat == 1)
            {
                return RedirectToAction("Index1", "User", new { email = email });

            }
            //BLOGGER
            else
            {
                return RedirectToAction("Index", "Blogger", new { email = email });

            }

        }

        //GET METHOD FOR POSTING AD
        [HttpGet]
        public ActionResult PostAd(string email, UserInfoModel u)
        {
            TempData["currentSession"] = u;
            AdViewModel adViewModel = new AdViewModel();
            adViewModel.EmailFromView = email;
            return View(adViewModel);
        }


        //POST METHOD FOR POSTING AD
        [HttpPost]
        public ActionResult PostAd(Models.AdViewModel ad)
        {
            string email = ad.EmailFromView;

            //OBTAINING USER ID
            var user = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where Email='" + email + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(user);
            }
            int id = (int)user.Rows[0][0];
            string body = ad.Body;
            int charCnt = body.Length;
            double paym = 0;
            //CALCULATING PAYABLE AMOUNT
            if (charCnt < 100)
            {
                paym = 100;
            }
            else
            {
                paym = 100 + ((charCnt - 100) * 0.50);
            }

            //POSTING AD
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();
                string query = "INSERT INTO ADS(AdsTitle,AdsBody,AdsPaymentStatus,UserId,AdsTotalAmount,Contact) VALUES " +
                    "(@title,@body,@pay,@id,@amnt,@cont)";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@title", ad.Title);
                sqlCommand.Parameters.AddWithValue("@body", ad.Body);
                sqlCommand.Parameters.AddWithValue("@pay", 0);
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.Parameters.AddWithValue("@amnt", paym);
                sqlCommand.Parameters.AddWithValue("@cont", ad.Contact);

                sqlCommand.ExecuteNonQuery();

            }
            TempData["msg"] = "<script>alert('You have successfully submitted an ad for our approval')</script>";
            //REDIRECTING ACCORDING TO BLOGGER STATUS
            byte bloggerStat = (byte)user.Rows[0][4];
            if (bloggerStat == 0)
            {
                return RedirectToAction("Index", "User", new { email = email });

            }
            else if (bloggerStat == 1)
            {
                return RedirectToAction("Index1", "User", new { email = email });

            }
            else
            {
                return RedirectToAction("Index", "Blogger", new { email = email });

            }

        }

        //SHOWING PENDING ADS(UNAPPROVED) OF A USER
        public ActionResult PendingAd(string email, UserInfoModel u)
        {
            TempData["currentSession"] = u;
            ViewBag.email = email;

            //OBTAINING USER ID
            var user = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where Email='" + email + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(user);
            }
            int id = (int)user.Rows[0][0];

            //OBTAINING ADS
            var ads = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from ads where userid=" + id + " and AdsApprovalStatus=0;";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(ads);
            }
            return View(ads);
        }

        //SHOWING PREVIOUS ADS OF A USER
        public ActionResult PreviousAds(string email, UserInfoModel u)
        {
            TempData["currentSession"] = u;
            ViewBag.email = email;

            //OBTAINING USER ID
            var user = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where Email='" + email + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(user);
            }
            int id = (int)user.Rows[0][0];

            //OBTAINING ADS
            var ads = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from ads where userid=" + id;
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(ads);
            }
            return View(ads);
        }

        //PROCEDURE FOR SHOWING LOGIN DETAILS OF USER
        public ActionResult LoginDetails(string email, UserInfoModel u)
        {
            TempData["currentSession"] = u;
            ViewBag.email = email;

            //OBTAINING USER ID
            var user = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where Email='" + email + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(user);
            }
            int id = (int)user.Rows[0][0];

            //OBTAINING LOGIN LOG
            var log = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from logindetails where userid=" + id;
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(log);
            }
            return View(log);
        }

        //PROCEDURE FOR SHOWING PAYMENT DETAILS OF USER
        public ActionResult PaymentDetails(string email, UserInfoModel u)
        {
            TempData["currentSession"] = u;
            TempData.Keep();
            ViewBag.email = email;

            //OBTAINING USER DETAILS
            var user = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where Email='" + email + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(user);
            }
            int id = (int)user.Rows[0][0];

            //OBTAINING AD DETAILS
            var ads = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from ads where userid=" + id;
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(ads);
            }
            return View(ads);
        }

        //PROCEDURE FOR BLOGGER APPLICATION
        public ActionResult BloggerApply(string email, UserInfoModel u)
        {
            TempData["currentSession"] = u;
            ViewBag.email = email;

            //OBTAINING USER ID
            var user = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where Email='" + email + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(user);
            }
            int id = (int)user.Rows[0][0];


            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();

                //INSERTING BLOGGER APPLICATION FOR USER
                string query = "INSERT INTO BloggerApplications(UserId) VALUES " +
                    "(@id)";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.ExecuteNonQuery();

                //UPDATING BLOGGERSTATUS
                string query1 = "Update UserDetails Set BloggerOrNot=1 where userID=" + id + ";";
                SqlCommand sqlCommand1 = new SqlCommand(query1, sqlConnection);
                sqlCommand1.Parameters.AddWithValue("@id", id);
                sqlCommand1.ExecuteNonQuery();

            }
            TempData["currentSession"] = u;

            DateTime localDate = DateTime.Now;
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();

                //UPDATING LOGOUT DETAILS
                string query1 = "Update LoginDetails Set sessionend=@end where loginID=(select max(loginid) from logindetails where userid=@id);";
                SqlCommand sqlCommand1 = new SqlCommand(query1, sqlConnection);
                sqlCommand1.Parameters.AddWithValue("@end", localDate);
                sqlCommand1.Parameters.AddWithValue("@id", id);
                sqlCommand1.ExecuteNonQuery();

            }
            //REDIRECTING TO LOGIN PAGE
            TempData["msg"] = "<script>alert('You have successfully applied to be a blogger. Please login again.')</script>";
            return RedirectToAction("Login", "Home", new { email = email });

        }
    }
}