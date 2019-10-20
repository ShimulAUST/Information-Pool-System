using InfopoolFinal.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace InfopoolFinal.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        //data source=server name; catalog=database name
        //
        //Shimul
        //string connectString = @"Data Source=DESKTOP-CBKATEE\SHIMULSQL; Initial Catalog = ISDFINAL; Integrated Security=True";
        //ASHNA
        string connectString = @"Data Source=DESKTOP-DOUTL4V; Initial Catalog = ISDFINAL; Integrated Security=True";
        //MAYEESHA
        //string connectString = @"Data Source=DESKTOP-L60BB46; Initial Catalog = ISDFINAL; Integrated Security=True";

        //HOME PAGE OF SITE AND FOR NEWS
        public ActionResult Index()
        {
            var news = new DataTable();

            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from news order by  PostTime DESC";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(news);
            }
            return View(news);
        }


        //HOMEPAGE FOR BLOGS
        public ActionResult AllBlogs()
        {
            var blg = new DataTable();

            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from blogs where approvalstatus=1 order by  PostTime DESC";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(blg);
            }
            return View(blg);
        }

        //POST METHOD FOR SEARCH
        [HttpPost]
        public ActionResult Search(string search)
        {
            string se = search;
            UserInfoModel u = new UserInfoModel();
            if (TempData["currentSession"] != null)
                u = TempData["currentSession"] as UserInfoModel;
            TempData.Keep();

            var lk = new DataTable();
            //SEARCHING AMONG TAGS(EXACT MATCH)
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "select * from blogs where ApprovalStatus=1 and blogid=(select BlogId from BLOGTAGS where Tag1='" + se + "' or Tag2='" + se + "' or Tag3='" + se + "' or Tag4='" + se + "')" + ";";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(lk);
            }
            //SEARCHING AMONG TYPES OF BLOGS(EXACT MATCH)
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "select * from blogs where ApprovalStatus=1 and TypeOfBlog='" + se + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(lk);
            }
            //SEARCHING AMONG TYPES OF BLOGS(NONEXACT MATCH)
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "select * from blogs where ApprovalStatus=1 and blogid=(select BlogId from BLOGTAGS where Tag1 like '%" + se + "%' or Tag2 like '%" + se + "%' or Tag3 like '%" + se + "%' or Tag4 like '%" + se + "%')" + ";";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(lk);
            }
            //SEARCHING AMONG TITLES(NONEXACT MATCH)
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "select * from blogs where ApprovalStatus=1 and BlogTitle like '%" + se + "%';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(lk);
            }
            ViewBag.searchTerm = se;



            return View(lk);
        }
        //ABOUT PAGE FOR SITE
        public ActionResult About()
        {

            return View();
        }

        //GET METHOD FOR REGISTERING
        [HttpGet]
        public ActionResult Register()
        {

            return View(new Models.RegisterViewModel());
        }

        //POST METHOD FOR REGISTERING
        [HttpPost]
        public ActionResult Register(Models.RegisterViewModel newuser)
        {
            if(newuser.Password.Equals(newuser.ConfirmPassword))
            {
                //INSERTING NEW USER
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    string query = "INSERT INTO UserDetails(UserName,Email,Pass) VALUES (@name,@email,@password)";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@name", newuser.Name);
                    sqlCommand.Parameters.AddWithValue("@email", newuser.Email);
                    sqlCommand.Parameters.AddWithValue("@password", newuser.Password);
                    string conpass = newuser.ConfirmPassword;
                    sqlCommand.ExecuteNonQuery();

                }

                //OBTAINING USER ID FOR INSERTING FIRST LOGINDETAIL-SESSION START
                var user = new DataTable();
                using (var sqlCon = new SqlConnection(connectString))
                {
                    sqlCon.Open();
                    var query = "Select * from UserDetails where Email='" + newuser.Email + "';";
                    var sqlDa = new SqlDataAdapter(query, sqlCon);
                    sqlDa.Fill(user);
                }
                int id = (int)user.Rows[0][0];

                //STARTING FIRST SESSION
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    string query = "INSERT INTO LoginDetails(UserId) VALUES (@id)";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@id", id);

                    sqlCommand.ExecuteNonQuery();

                }

                //ENDING FIRST SESSION
                DateTime localDate = DateTime.Now;
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();

                    string query1 = "Update LoginDetails Set sessionend=@end where loginID=(select max(loginid) from logindetails where userid=@id);";
                    SqlCommand sqlCommand1 = new SqlCommand(query1, sqlConnection);
                    sqlCommand1.Parameters.AddWithValue("@end", localDate);
                    sqlCommand1.Parameters.AddWithValue("@id", id);
                    sqlCommand1.ExecuteNonQuery();

                }
                //REDIRECTING TO LOGIN PAGE FOR FIRST LOGIN
                TempData["msg"] = "<script>alert('You have successfully registered. Please login for using your account.')</script>";
                return RedirectToAction("Login", "Home");
            }
            else
            {
                //PASSWORDS DIDNT MATCH
                //TempData["msg"] = "<script>alert('Your passwords didn't match. Try again.')</script>";
                TempData["msg"] = "<script>alert('Your passwords did not match. Please try again.')</script>";

                return RedirectToAction("Index", "Home");
            }


        }

        //PROCEDURE FOR LOGOUT
        public ActionResult Logout()
        {
            UserInfoModel u = new UserInfoModel();
            if (TempData["currentSession"] != null)
                u = TempData["currentSession"] as UserInfoModel;

            string email = u.Email;

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

            //ENDING CURRENT SESSION
            DateTime localDate = DateTime.Now;
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();

                string query1 = "Update LoginDetails Set sessionend=@end where loginID=(select max(loginid) from logindetails where userid=@id);";
                SqlCommand sqlCommand1 = new SqlCommand(query1, sqlConnection);
                sqlCommand1.Parameters.AddWithValue("@end", localDate);
                sqlCommand1.Parameters.AddWithValue("@id", id);
                sqlCommand1.ExecuteNonQuery();

            }

            //ENDING CURRENT SESSION IN TEMPDATA
            TempData["currentSession"] = null;

            //REDIRECTING TO HOME PAGE
            TempData["msg"] = "<script>alert('You have logged out of you account')</script>";
            return RedirectToAction("Index", "Home");
        }

        //GET METHOD FOR LOGIN PAGE
        [HttpGet]
        public ActionResult Login()
        {
            return View(new Models.LoginViewModel());
        }

        //POST METHOD FOR LOGGING IN
        [HttpPost]
        public ActionResult Login(Models.LoginViewModel user)
        {
            var us = new DataTable();

            //OBTAINING USER ID
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where Email='" + user.Email + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(us);
            }
            if(us.Rows.Count!=0)
            {
                //matching passwords
                if (us.Rows[0][3].ToString() == user.Password)
                {

                    int id = (int)us.Rows[0][0];

                    
                    //ENDING PREVIOUS SESSION,IF NULL
                    var prevSession = new DataTable();
                    using (var sqlCon = new SqlConnection(connectString))
                    {
                        sqlCon.Open();
                        var query = "Select * from logindetails where UserId=" + id + " and loginid=(select max(loginid) from logindetails where userid=" + id + ") and sessionend is null;";
                        var sqlDa = new SqlDataAdapter(query, sqlCon);
                        sqlDa.Fill(prevSession);
                    }

                    if (prevSession.Rows.Count != 0)
                    {
                        int logid = (int)prevSession.Rows[0][0];
                        DateTime localDate = DateTime.Now;
                        using (SqlConnection sqlConnection = new SqlConnection(connectString))
                        {
                            sqlConnection.Open();

                            string query1 = "Update LoginDetails Set sessionend=@end where loginID=" + logid + ";";
                            SqlCommand sqlCommand1 = new SqlCommand(query1, sqlConnection);
                            sqlCommand1.Parameters.AddWithValue("@end", localDate);

                            sqlCommand1.ExecuteNonQuery();

                        }
                    }


                    //STARTING NEW SESSION
                    using (SqlConnection sqlConnection = new SqlConnection(connectString))
                    {
                        sqlConnection.Open();
                        string query = "INSERT INTO LoginDetails(UserId) VALUES (@id)";
                        SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                        sqlCommand.Parameters.AddWithValue("@id", id);
                        sqlCommand.ExecuteNonQuery();

                    }

                    //SAVING SESSION VARIABLE
                    UserInfoModel currentUser = new UserInfoModel();
                    currentUser.Name = (us.Rows[0][1]).ToString();
                    currentUser.Email = (us.Rows[0][2]).ToString();
                    TempData["currentSession"] = currentUser;
                    System.Web.HttpContext.Current.Session["sessionString"] = currentUser;


                    //REDIRECTING ACCORDING TO USER STATUS
                    byte bloggerStat = (byte)us.Rows[0][4];

                    //NORMAL USER
                    if (bloggerStat == 0)
                    {
                        TempData["msg"] = "<script>alert('You have successfully logged in!')</script>";
                        return RedirectToAction("Index", "User", new { email = user.Email });

                    }
                    //PERSON APPLIED TO BE BLOGGER
                    else if (bloggerStat == 1)
                    {
                        TempData["msg"] = "<script>alert('You have successfully logged in!')</script>";
                        return RedirectToAction("Index1", "User", new { email = user.Email });

                    }
                    //BLOGGER
                    else
                    {
                        TempData["msg"] = "<script>alert('You have successfully logged in!')</script>";
                        return RedirectToAction("Index", "Blogger", new { email = user.Email });

                    }


                }

                //UNSUCCESSFUL LOGIN ATTEMPT
                else
                {
                    TempData["msg"] = "<script>alert('Your login attempt was unsuccessful. Please try again.')</script>";
                    return RedirectToAction("Login", "Home");
                }
            }
            else
            {
                TempData["msg"] = "<script>alert('No account with this email address exits in our system. Try registering first.')</script>";
                return RedirectToAction("Register", "Home");
            }
            

        }

        //GET METHOD FOR SENDING COMPLAINT
        [HttpGet]
        public ActionResult Contact()
        {
            ContactViewModel contactViewModel = new ContactViewModel();
            return View(contactViewModel);
        }

        //POST METHOD FOR SENDING COMPLAINT
        [HttpPost]
        public ActionResult Contact(Models.ContactViewModel cont)
        {
            //SUBMITTING COMPLAINT
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();
                string query = "INSERT INTO COMPLAINT(COMPLAINTSUBJECT,COMPLAINTDETAILS,USEREMAIL) VALUES " +
                    "(@title,@body,@email)";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@title", cont.Title);
                sqlCommand.Parameters.AddWithValue("@body", cont.Complaint);
                sqlCommand.Parameters.AddWithValue("@email", cont.Email);

                sqlCommand.ExecuteNonQuery();

            }
            //REDIRECTING TO HOME PAGE
            TempData["msg"] = "<script>alert('You have successfully sent us a complaint. We will look into it as soon as possible.')</script>";
            return RedirectToAction("Index", "Home");




        }

        //SHOWING UPCOMING EVENTS
        public ActionResult Events()
        {
            var events = new DataTable();
            DateTime today = DateTime.Today;
            //OBTAINING EVENTS
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from events where eventdate>='" + today + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(events);
            }
            return View(events);

        }

        //SHOWING CLASSIFIEDS
        public ActionResult Classifieds()
        {
            //OBTAINING ADS
            var ads = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from ads where AdsApprovalStatus=2;";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(ads);
            }
            return View(ads);
        }
    }
}