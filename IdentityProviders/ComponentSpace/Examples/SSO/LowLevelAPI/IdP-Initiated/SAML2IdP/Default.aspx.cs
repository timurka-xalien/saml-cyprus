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

namespace SAML2IdP
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Construct a URL to the local SSO service and specifying the target URL of the SP.
            spHyperLink.NavigateUrl = string.Format("~/SAML/SSOService.aspx?target={0}", HttpUtility.UrlEncode(Configuration.SPTargetURL));
        }
    }
}
