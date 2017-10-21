using System;
using System.Web.Security;

namespace ExampleIdentityProvider
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void loginButton_Click(object sender, EventArgs e)
        {
            // For simplicity, this example uses forms authentication with credentials stored in web.config.
            // Your application can use any authentication method you choose (eg Active Directory, custom database etc).
            // There are no restrictions on the method of authentication.
            if (FormsAuthentication.Authenticate(userNameTextBox.Text, passwordTextBox.Text))
            {
                FormsAuthentication.RedirectFromLoginPage(userNameTextBox.Text, false);
            }
            else
            {
                errorMessageLabel.Text = "Invalid credentials. The user name and password should be \"idp-user\" and \"password\".";
            }
        }
    }
}
