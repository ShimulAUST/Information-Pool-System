using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace InfopoolFinal.Controllers
{
    public class SidenavController : Controller
    {
        //data source=server name; catalog=database name

        //Shimul
        //string connectString = @"Data Source=DESKTOP-CBKATEE\SHIMULSQL; Initial Catalog = ISDFINAL; Integrated Security=True";
        //ASHNA
        string connectString = @"Data Source=DESKTOP-DOUTL4V; Initial Catalog = ISDFINAL; Integrated Security=True";
        //MAYEESHA
        //string connectString = @"Data Source=DESKTOP-L60BB46; Initial Catalog = ISDFINAL; Integrated Security=True";
        // GET: Sidenav

        //FOR SHOWING NEWS FROM SIDENAV BAR
        public ActionResult News(string type)
        {
            var news = new DataTable();
            //OBTAINING NEWS BY TYPE
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from news where newstype='" + type + "';";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(news);
            }
            return View(news);

        }

        //FOR SHOWING BLOGS FROM SIDENAV BAR
        public ActionResult Blogs(string type)
        {
            var blg = new DataTable();
            //OBTAINING BLOGS BY TYPE
            using (var sqlCon = new SqlConnection(connectString))
            {
                sqlCon.Open();
                var query = "Select * from blogs where TypeOfBlog='" + type + "' and ApprovalStatus=1;";
                var sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(blg);
            }
            return View(blg);

        }

    }
}