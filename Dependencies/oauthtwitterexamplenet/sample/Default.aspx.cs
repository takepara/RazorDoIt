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

#region License
/*
 Copyright (c) 2010 Whitley Media

 Permission is hereby granted, free of charge, to any person
 obtaining a copy of this software and associated documentation
 files (the "Software"), to deal in the Software without
 restriction, including without limitation the rights to use,
 copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the
 Software is furnished to do so, subject to the following
 conditions:

 The above copyright notice and this permission notice shall be
 included in all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace oAuthExample
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string url = "";
            string xml = "";
            oAuthTwitter oAuth = new oAuthTwitter();

            if (Request["oauth_token"] == null)
            {
                //Redirect the user to Twitter for authorization.
                //Using oauth_callback for local testing.
                oAuth.CallBackUrl = "http://localhost";
                Response.Redirect(oAuth.AuthorizationLinkGet());
            }
            else
            {
                //Get the access token and secret.
                oAuth.AccessTokenGet(Request["oauth_token"], Request["oauth_verifier"]);
                if (oAuth.TokenSecret.Length > 0)
                {
                    //We now have the credentials, so make a call to the Twitter API.
                    url = "http://twitter.com/account/verify_credentials.xml";
                    xml = oAuth.oAuthWebRequest(oAuthTwitter.Method.GET, url, String.Empty);
                    apiResponse.InnerHtml = Server.HtmlEncode(xml);

                    //POST Test
                    //url = "http://twitter.com/statuses/update.xml";
                    //xml = oAuth.oAuthWebRequest(oAuthTwitter.Method.POST, url, "status=" + oAuth.UrlEncode("Hello @swhitley - Testing the .NET oAuth API"));
                    //apiResponse.InnerHtml = Server.HtmlEncode(xml);
                }
            }
        }
    }
}
