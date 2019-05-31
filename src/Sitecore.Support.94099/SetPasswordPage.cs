using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Security.Accounts;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using System;
using System.Web.Security;

namespace Sitecore.Support.Shell.Applications.Security.SetPassword
{
    /// =============================================================================================
    /// This patched fix generate password for locked out user will throw an exception.
    /// Check the user is locked out before generate random password.
    /// Alert user with text : The user is locked out. Please unlock the user before change password.
    /// ====================================================== =======================================

    /// <summary>Represents a GridDesignerPage.</summary>
    public class SetPasswordPage : Sitecore.Shell.Applications.Security.SetPassword.SetPasswordPage
    {
        /// <summary>Handles the Generate_ click event.</summary>
        protected new void Generate_Click()
        {
            ClientPipelineArgs currentArgs = ContinuationManager.Current.CurrentArgs as ClientPipelineArgs;
            Assert.IsNotNull((object)currentArgs, typeof(ClientPipelineArgs));
            MembershipUser user = SetPasswordPage.GetUser();
            Assert.IsNotNull((object)user, typeof(User));
            if (user.IsLockedOut)
                SheerResponse.Alert(Translate.Text("The user is locked out. Please unlock the user before change password."));
            else if (currentArgs.IsPostBack)
            {
                if (!(currentArgs.Result == "yes"))
                    return;
                string str;
                try
                {
                    str = user.ResetPassword();
                }
                catch (NotSupportedException ex)
                {
                    SheerResponse.Alert(ex.Message);
                    return;
                }
                SheerResponse.SetStyle("RandomPassword", "color", "Black");
                SheerResponse.Eval("document.getElementById('RandomPassword').disabled = false;");
                SheerResponse.SetAttribute("RandomPassword", "readonly", "readonly");
                this.RandomPassword.Value = str;
                this.HasGeneratedPassword = true;
                SheerResponse.Alert(Translate.Text("The password has been changed."));
            }
            else
            {
                SheerResponse.Confirm(Translate.Text("Are you sure you want to reset the password?"));
                currentArgs.WaitForPostBack();
            }
        }

        /// <summary>Gets the user.</summary>
        /// <returns>The user.</returns>
        private static MembershipUser GetUser()
        {
            return Membership.GetUser(WebUtil.GetQueryString("us"));
        }
    }
}