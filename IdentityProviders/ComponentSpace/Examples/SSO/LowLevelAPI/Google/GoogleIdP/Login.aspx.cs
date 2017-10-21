using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace GoogleIdP
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void loginButton_Click(object sender, EventArgs e)
        {
            // NB. The authenticated user's name must correspond to a user configured within Google Apps.
            if (FormsAuthentication.Authenticate(usernameTextBox.Text, passwordTextBox.Text))
            {
                FormsAuthentication.RedirectFromLoginPage(usernameTextBox.Text, false);
            }
            else
            {
                errorMessageLabel.Text = "Invalid credentials. The user name and password should be \"google\" and \"password\".";
            }
        }
    }
}
