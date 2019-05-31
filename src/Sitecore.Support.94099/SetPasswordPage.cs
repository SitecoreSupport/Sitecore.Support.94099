using Sitecore.Controls;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Resources;
using Sitecore.Security.Accounts;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using System;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace Sitecore.Support.Shell.Applications.Security.SetPassword
{
    /// =================================================================================
    ///  This is original version of SetPasswordPage class before patched.                                 
    ///  Application error when generate a password for a locked user account.         
    ///  A fix is needed in Generate_Click() method to check if the user is locked out.
    /// =================================================================================


    /// <summary>Represents a GridDesignerPage.</summary>
    public class SetPasswordPage : DialogPage
    {
        /// <summary></summary>
        protected TextBox OldPassword;
        /// <summary></summary>
        protected TextBox NewPassword;
        /// <summary></summary>
        protected TextBox ConfirmPassword;
        /// <summary></summary>
        protected System.Web.UI.WebControls.Label UserName;
        /// <summary></summary>
        protected System.Web.UI.WebControls.Label DomainName;
        /// <summary></summary>
        protected Edit RandomPassword;
        /// <summary></summary>
        protected ThemedImage Portrait;

        /// <summary>
        /// Gets or sets a value indicating whether this instance has generated password.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has generated password; otherwise, <c>false</c>.
        /// </value>
        public bool HasGeneratedPassword
        {
            get
            {
                object obj = this.ViewState[nameof(HasGeneratedPassword)];
                if (obj == null)
                    return false;
                return (bool)obj;
            }
            set
            {
                this.ViewState[nameof(HasGeneratedPassword)] = (object)value;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"></see> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull((object)e, nameof(e));
            base.OnLoad(e);
            User user = User.FromName(WebUtil.GetSafeQueryString("us"), true);
            Assert.IsNotNull((object)user, typeof(User));
            string str = ImageBuilder.ResizeImageSrc(user.Profile.Portrait, 48, 48).Trim();
            Assert.IsNotNull((object)str, "portrait");
            if (!string.IsNullOrEmpty(str))
                this.Portrait.Src = str;
            this.UserName.Text = user.GetLocalName();
            this.DomainName.Text = user.GetDomainName();
            this.RandomPassword.Value = Translate.TextByLanguage("No password has been generated yet.", Language.Current);
        }

        /// <summary>Handles a click on the OK button.</summary>
        /// <remarks>When the user clicks OK, the dialog is closed by calling
        /// the <see cref="M:Sitecore.Web.UI.Sheer.ClientResponse.CloseWindow">CloseWindow</see> method.</remarks>
        protected override void OK_Click()
        {
            string text1 = this.OldPassword.Text;
            string text2 = this.NewPassword.Text;
            string text3 = this.ConfirmPassword.Text;
            if (!string.IsNullOrEmpty(text1) || !string.IsNullOrEmpty(text2) || (!string.IsNullOrEmpty(text3) || !this.HasGeneratedPassword))
            {
                if (string.IsNullOrEmpty(text1))
                {
                    SheerResponse.Alert(Translate.Text("You must supply the old password.\n\nIf you do not know the old password, click Generate to generate a new password."));
                    return;
                }
                if (string.IsNullOrEmpty(text2))
                {
                    SheerResponse.Alert(Translate.Text("You must supply a new password."));
                    return;
                }
                if (text2 != text3)
                {
                    SheerResponse.Alert(Translate.Text("The passwords do not match."));
                    return;
                }
                MembershipUser user = SetPasswordPage.GetUser();
                Assert.IsNotNull((object)user, typeof(User));
                bool flag = false;
                try
                {
                    flag = user.ChangePassword(text1, text2);
                }
                catch (ArgumentException ex)
                {
                }
                if (!flag)
                {
                    SheerResponse.Alert(Translate.Text("Failed to set the password.\n\nPossible reasons are:\n\n1) The old password is incorrect.\n2) The new password does not meet the security requirements.\n\nTo learn about the password security requirements, please consult your administrator."));
                    return;
                }
                SheerResponse.Alert(Translate.Text("The password has been changed."));
            }
            base.OK_Click();
        }

        /// <summary>Handles the Generate_ click event.</summary>
        protected void Generate_Click()
        {
            ClientPipelineArgs currentArgs = ContinuationManager.Current.CurrentArgs as ClientPipelineArgs;
            Assert.IsNotNull((object)currentArgs, typeof(ClientPipelineArgs));
            MembershipUser user = SetPasswordPage.GetUser();
            Assert.IsNotNull((object)user, typeof(User));
            if (currentArgs.IsPostBack)
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
