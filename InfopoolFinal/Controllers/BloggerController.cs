using InfopoolFinal.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;

namespace InfopoolFinal.Controllers
{
    public class BloggerController : Controller
    {

        //Shimul
        //string connectString = @"Data Source=DESKTOP-CBKATEE\SHIMULSQL; Initial Catalog = ISDFINAL; Integrated Security=True";
        //ASHNA
        string connectString = @"Data Source=DESKTOP-DOUTL4V; Initial Catalog = ISDFINAL; Integrated Security=True";
        //MAYEESHA
        //string connectString = @"Data Source=DESKTOP-L60BB46; Initial Catalog = ISDFINAL; Integrated Security=True";


        //GOES TO BLOGGER'S HOME PAGE
        public ActionResult Index(string email, UserInfoModel u)
        {
            ViewBag.email = email;
            TempData["currentSession"] = u;
            return View();
        }


        //VIEWS CLICKED BLOG FROM SIDENAV OR NAVBAR
        public ActionResult BlogView(int blogid)
        {
            ViewBag.ShowLikeBtn = false;
            UserInfoModel u = new UserInfoModel();
            if (TempData["currentSession"] != null)
            {
                //OBTAINING USER ID
                var blog = new DataTable();
                u = TempData["currentSession"] as UserInfoModel;
                TempData.Keep();
                string email = u.Email;
                using (var sqlCon = new SqlConnection(connectString))
                {
                    sqlCon.Open();
                    var query = "Select * from userdetails where email='" + email + "';";
                    var sqlDa = new SqlDataAdapter(query, sqlCon);
                    sqlDa.Fill(blog);
                }
                int id = (int)blog.Rows[0][0];

                //CHECKING WHETHER USER LIKED IT PREVIOUSLY
                var blog2 = new DataTable();
                using (var sqlCon = new SqlConnection(connectString))
                {
                    sqlCon.Open();
                    var query = "Select * from like_comment where blogid=" + blogid + " and userid=" + id;
                    var sqlDa = new SqlDataAdapter(query, sqlCon);
                    sqlDa.Fill(blog2);
                }
                int cnt = blog2.Rows.Count;
                ViewBag.ShowLikeBtn = false;
                ViewBag.ShowCommentBox = false;
                //SHOWS LIKE BUTTON AND COMMENT TEXTBOX IF USER DIDNT LIKE IT/COMMENT PREVIOUSLY
                if (cnt == 0)
                {
                    ViewBag.ShowLikeBtn = true;
                    ViewBag.ShowCommentBox = true;
                }
                else
                {
                    byte whetherLiked = (byte)blog2.Rows[0][2];
                    string whetherCommented = blog2.Rows[0][3].ToString();
                    //DISABLE LIKING IF PREVIOUSLY LIKED, COMMENT ALLOWED
                    if (cnt == 1 && whetherLiked == 1 && whetherCommented.Equals(""))
                    {
                        ViewBag.ShowLikeBtn = false;
                        ViewBag.ShowCommentBox = true;
                    }
                    //DISABLE COMMENTING, LIKING ALLOWED
                    else if (cnt == 1 && !whetherCommented.Equals("") && whetherLiked == 0)
                    {
                        ViewBag.ShowLikeBtn = true;
                        ViewBag.ShowCommentBox = false;
                    }
                    //DISABLE BOTH
                    else
                    {
                        ViewBag.ShowLikeBtn = false;
                        ViewBag.ShowCommentBox = false;
                    }
                }

            }
            //OBTAINING BLOG FOR VIEWING
            var blog1 = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from blogs where blogid=" + blogid;
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(blog1);
            }
            //OBTAINING COMMENTS
            var comm = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from like_comment where blogid=" + blogid + " and comment is not null";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(comm);
            }
            List<string> allcomments = new List<string>();
            for (int i = 0; i < comm.Rows.Count; i++)
            {
                allcomments.Add(comm.Rows[i][3].ToString());
            }
            ViewBag.comments = allcomments;
            return View(blog1);
        }


        //PROCEDURE FOR LIKING A POST(ONLY FOR LOGGED IN USER)
        public ActionResult LikePost(int blogid)
        {
            UserInfoModel u = new UserInfoModel();
            if (TempData["currentSession"] != null)
                u = TempData["currentSession"] as UserInfoModel;
            TempData.Keep();
            //OBTAINING USER ID FOR INSERTING LIKE
            string email = u.Email;
            var us = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where Email='" + email + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(us);
            }
            int id = (int)us.Rows[0][0];

            //SEEING IF ANY ROW ALREADY EXISTS
            var lk = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from Like_comment where userid=" + id + " and blogid=" + blogid + ";";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(lk);
            }
            int numRow = lk.Rows.Count;

            //IF NO PREVIOUS ROWS
            if (numRow == 0)
            {
                //INSERTING NEW LIKE
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    string query = "INSERT INTO Like_Comment(UserId,BlogId,Likes) VALUES " +
                        "(@id,@bid,@like)";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Parameters.AddWithValue("@bid", blogid);
                    sqlCommand.Parameters.AddWithValue("@like", 1);

                    sqlCommand.ExecuteNonQuery();
                }
                //UPDATING LIKECOUNT
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    string query1 = "UPDATE BLOGS SET LIKECOUNT=(SELECT COUNT(USERID) FROM LIKE_COMMENT WHERE BLOGID=" + blogid + ") WHERE BLOGID=" + blogid + ";";
                    SqlCommand sqlCommand1 = new SqlCommand(query1, sqlConnection);
                    sqlCommand1.ExecuteNonQuery();

                }

            }
            else
            {
                byte whetherLiked = (byte)lk.Rows[0][2];
                //IF PREVIOUS ROW EXISTS AND NO LIKE
                if (numRow == 1 && whetherLiked == 0)
                {
                    using (SqlConnection sqlConnection = new SqlConnection(connectString))
                    {
                        sqlConnection.Open();
                        string query = "Update Like_comment set likes=1 where userid=@id and blogid=@bid";
                        SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                        sqlCommand.Parameters.AddWithValue("@id", id);
                        sqlCommand.Parameters.AddWithValue("@bid", blogid);

                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }


            return RedirectToAction("BlogView", "Blogger", new { blogid = blogid, u = u });
        }


        //PROCEDURE FOR COMMENTING ON POSTS
        [HttpPost]
        public ActionResult CommentPost(string comment, int blogid)
        {
            string com = comment;
            int blgid = blogid;
            UserInfoModel u = new UserInfoModel();
            if (TempData["currentSession"] != null)
                u = TempData["currentSession"] as UserInfoModel;
            TempData.Keep();
            //OBTAINING USER ID FOR INSERTING COMMENT
            string email = u.Email;
            var us = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where Email='" + email + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(us);
            }
            int id = (int)us.Rows[0][0];

            //SEEING IF ANY ROW ALREADY EXISTS
            var lk = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from Like_comment where userid=" + id + " and blogid=" + blgid + ";";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(lk);
            }
            int numRow = lk.Rows.Count;

            //IF NO PREVIOUS ROWS EXIST
            if (numRow == 0)
            {
                //INSERTING NEW COMMENT
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    string query = "INSERT INTO Like_Comment(UserId,BlogId,Comment) VALUES " +
                        "(@id,@bid,@comment)";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Parameters.AddWithValue("@bid", blgid);
                    sqlCommand.Parameters.AddWithValue("@comment", com);

                    sqlCommand.ExecuteNonQuery();
                }
            }
            else
            {
                string comnt = lk.Rows[0][3].ToString();
                //IF PREVIOUS ROW EXISTS AND ONLY HAS LIKE
                if (numRow == 1 && comnt.Equals(""))
                {
                    using (SqlConnection sqlConnection = new SqlConnection(connectString))
                    {
                        sqlConnection.Open();
                        string query = "Update Like_comment set comment=@comment where userid=@id and blogid=@bid";
                        SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                        sqlCommand.Parameters.AddWithValue("@id", id);
                        sqlCommand.Parameters.AddWithValue("@bid", blgid);
                        sqlCommand.Parameters.AddWithValue("@comment", com);

                        sqlCommand.ExecuteNonQuery();
                    }
                }
                //IF ALREADY HAS COMMENT
                else
                {
                    TempData["msg"] = "<script>alert('You have already posted a comment')</script>";
                }
            }



            return RedirectToAction("BlogView", "Blogger", new { blogid = blogid, u = u });
        }


        //PROCEDURE FOR SEEING PENDING(UNAPPROVED) BLOGS OF A BLOGGER
        public ActionResult SeePendingBlogs(string email, UserInfoModel u)
        {
            ViewBag.email = email;
            TempData["currentSession"] = u;
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

            //OBTAINING UNAPPROVED BLOGS OF PARTICULAR USER
            var blgs = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from blogs where bloggerid=" + id + " and ApprovalStatus=0;";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(blgs);
            }
            return View(blgs);
        }


        //PROCEDURE FOR SEEING PREVIOUS(APPROVED) BLOGS OF A PARTICULAR BLOGGER
        public ActionResult SeePreviousBlogs(string email, UserInfoModel u)
        {
            ViewBag.email = email;
            TempData["currentSession"] = u;
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

            //OBTAINING BLOGS
            var blgs = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from blogs where bloggerid=" + id;
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(blgs);
            }
            return View(blgs);
        }


        //GET METHOD FOR POSTING NEW BLOG
        [HttpGet]
        public ActionResult PostNewBlog(string email, UserInfoModel u)
        {
            TempData["currentSession"] = u;
            BlogViewModel blogViewModel = new BlogViewModel();
            blogViewModel.EmailFromView = email;
            return View(blogViewModel);
        }

        //POST METHOD FOR POSTING NEW BLOG
        [HttpPost]
        public ActionResult PostNewBlog(Models.BlogViewModel blog)
        {
            string email = blog.EmailFromView;
            string fileName = "";
            string filePath = "";
            var file = blog.file[0];

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

            //FOR BLOGTYPE DROPDOWN MENU
            string btype = "none";
            switch (blog.BlogType)
            {
                case 0: btype = "Breaking"; break;
                case 1: btype = "World"; break;
                case 2: btype = "Technology"; break;
                case 3: btype = "Fashion"; break;
                case 4: btype = "Business"; break;
                case 5: btype = "StockMarket"; break;
                case 6: btype = "Sports"; break;
            }

            //FOR SAVING BLOG COVER PICTURE
            if (file.ContentLength > 0)
            {
                fileName = Path.GetFileName(file.FileName);
                filePath = Path.Combine(Server.MapPath("~/images/Blogs"), fileName);
                string toSave = "~/images/Blogs/" + fileName;
                file.SaveAs(filePath);

                //INSERTING NEW BLOG
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    string query = "INSERT INTO BLOGS(BlogTitle,BlogBody,TypeOfBlog,BlogCoverImage,BloggerId) VALUES " +
                        "(@title,@body,@type,@image,@id)";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@title", blog.BlogTitle);
                    sqlCommand.Parameters.AddWithValue("@body", blog.BlogBody);
                    sqlCommand.Parameters.AddWithValue("@type", btype);
                    sqlCommand.Parameters.AddWithValue("@image", toSave);
                    sqlCommand.Parameters.AddWithValue("@id", id);

                    sqlCommand.ExecuteNonQuery();

                }

                //OBTAINING BLOG ID OF NEW BLOG FOR INSERTING BLOGTAGS
                var blg = new DataTable();
                using (var sqlCon = new SqlConnection(connectString))
                {
                    sqlCon.Open();
                    var query = "Select blogid from Blogs where blogid=(select max(blogid) from Blogs);";
                    var sqlDa = new SqlDataAdapter(query, sqlCon);
                    sqlDa.Fill(blg);
                }
                int bid = (int)blg.Rows[0][0];

                //INSERTING BLOGTAGS
                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    string query = "INSERT INTO BLOGTAGS(BlogId,Tag1,Tag2,Tag3,Tag4) VALUES " +
                        "(@bid,@tag1,@tag2,@tag3,@tag4)";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@bid", bid);

                    //TAG1 IS MANDATORY, TAGS 2,3,4 ARE OPTIONAL
                    if (blog.Tag1 != null)
                    {
                        sqlCommand.Parameters.AddWithValue("@tag1", blog.Tag1);
                    }

                    if (blog.Tag2 != null)
                    {
                        sqlCommand.Parameters.AddWithValue("@tag2", blog.Tag2);
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@tag2", "");
                    }
                    if (blog.Tag3 != null)
                    {
                        sqlCommand.Parameters.AddWithValue("@tag3", blog.Tag3);
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@tag3", "");
                    }
                    if (blog.Tag4 != null)
                    {
                        sqlCommand.Parameters.AddWithValue("@tag4", blog.Tag4);
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@tag4", "");
                    }

                    sqlCommand.ExecuteNonQuery();
                }

            }

            //REDIRECTING TO BLOGGER'S HOMEPAGE
            TempData["msg"] = "<script>alert('You have successfully posted a blog for our approval.')</script>";
            return RedirectToAction("Index", "Blogger", new { email = email });



        }

        //PROCEDURE FOR DELETING A BLOG
        public ActionResult DeleteBlog(int blogid, int bloggerid)
        {
            UserInfoModel u = new UserInfoModel();
            if (TempData["currentSession"] != null)
                u = TempData["currentSession"] as UserInfoModel;
            TempData.Keep();
            //DELETING LIKE_COMMENT FIRST AS THEY HAVE DEPENDENCY ON BLOG TABLE
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();

                string query = "DELETE FROM Like_comment WHERE BlogId = @BlogId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@BlogId", blogid);
                sqlCommand.ExecuteNonQuery();

            }

            //DELETING BLOGTAGS FIRST AS THEY HAVE DEPENDENCY ON BLOG TABLE
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();

                string query = "DELETE FROM Blogtags WHERE BlogId = @BlogId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@BlogId", blogid);
                sqlCommand.ExecuteNonQuery();

            }
            //DELETING THE BLOG ITSELF
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();

                string query = "DELETE FROM Blogs WHERE BlogId = @BlogId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@BlogId", blogid);
                sqlCommand.ExecuteNonQuery();

            }

            //OBTAINING USER ID
            var user = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where userid='" + bloggerid + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(user);
            }
            string email = user.Rows[0][2].ToString();

            //REDIRECTING TO PREVIOUS BLOGS PAGE
            TempData["msg"] = "<script>alert('Your blog was deleted.')</script>";
            return RedirectToAction("Index", "Blogger", new { email = email });


        }


        //GET METHOD FOR EDITING BLOG
        [HttpGet]
        public ActionResult EditBlog(int blogid, int bloggerid)
        {
            BlogViewModel blogViewModel = new BlogViewModel();
            blogViewModel.BlogId = blogid;
            blogViewModel.BloggerId = bloggerid;

            //OBTAINING PREVIOUS DATA OF BLOG
            var blog = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from Blogs where blogid='" + blogid + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(blog);
            }
            blogViewModel.BlogTitle = blog.Rows[0][1].ToString();
            blogViewModel.BlogBody = blog.Rows[0][2].ToString();

            //OBTAINING USER DETAILS FOR REINSERTING BLOG
            var blog1 = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where userid='" + bloggerid + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(blog1);
            }
            blogViewModel.EmailFromView = blog1.Rows[0][2].ToString();
            return View(blogViewModel);
        }


        //POST METHOD FOR EDITING BLOG
        [HttpPost]
        public ActionResult EditBlog(Models.BlogViewModel blog)
        {
            string email = blog.EmailFromView;
            //OBTAINING USER ID FOR REINSERTING BLOG
            var user = new DataTable();
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from UserDetails where Email='" + email + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(user);
            }
            int id = (int)user.Rows[0][0];

            //FOR BLOGTYPE DROPDOWN MENU
            string btype = "none";
            switch (blog.BlogType)
            {
                case 0: btype = "Breaking"; break;
                case 1: btype = "World"; break;
                case 2: btype = "Technology"; break;
                case 3: btype = "Fashion"; break;
                case 4: btype = "Business"; break;
                case 5: btype = "StockMarket"; break;
                case 6: btype = "Sports"; break;
            }

            //UPDATING ROW IN BLOG TABLE AND SETTING APPROVAL STATUS=UNAPPROVED AGAIN
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();
                string query = "Update Blogs set blogtitle=@title, blogbody=@body, TypeOfBlog=@type, approvalstatus=0, bloggerid=@id where blogid=@blogid;";

                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@title", blog.BlogTitle);
                sqlCommand.Parameters.AddWithValue("@body", blog.BlogBody);
                sqlCommand.Parameters.AddWithValue("@type", btype);
                sqlCommand.Parameters.AddWithValue("@blogid", blog.BlogId);
                sqlCommand.Parameters.AddWithValue("@id", id);

                sqlCommand.ExecuteNonQuery();


            }
            //REDIRECTING TO BLOGGER HOME PAGE
            TempData["msg"] = "<script>alert('Your blog has been resubmitted for approval')</script>";
            return RedirectToAction("Index", "Blogger", new { email = email });



        }
    }
}