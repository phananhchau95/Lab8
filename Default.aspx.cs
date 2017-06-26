/*--------------------------------------------------------------------------*
 *  Copyright (C) 2007 Extentrix Systems
 *
 *  File:      Default.aspx.cs
 *
 *  Contents:  It is an example on how to consume Extentrix Web Services 2.0 – Application Edition
 *--------------------------------------------------------------------------*/

using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using ExtentrixWS;
using System.Net;
using System.Windows.Forms;
using System.IO;

public partial class _Default : System.Web.UI.Page 
{
    //define a web service proxy object.
    private ExtentrixWS.ExtentrixWebServicesForCPS proxy;
    
    //define a Citrix Presentation Server Credentials object.
    private Credentials credentials;

    protected void Page_Load(object sender, EventArgs e)
    {
        //When using the HttpWebRequest to POST form data using HTTP 1.1,
        //it ALWAYS adds the following HTTP header "Expect: 100-Continue". 
        //Fixing the problem set it to false.
        ServicePointManager.Expect100Continue = false;

        //intialaize objects
         proxy = new ExtentrixWebServicesForCPS();
         credentials = new Credentials();

        //set credentials
        //these values are according to Citrix testdrive presentation server
        //that Extentrix published a web service for it for deloveper to use it
        //as a test web service.

        credentials.Password = "demo";
        credentials.UserName = "citrixdesktop";
        credentials.Domain = "testdrive";
        
        //because it is a sample and we will use no encryption methode
        //so the password will send as a clear text.
        credentials.PasswordEncryptionMethod = 0;
        
        //set the domain type to windows domain
        credentials.DomainType = 0;


        // 1) Get all the published applications list by calling GetApplicationsByCredentialsEx web service.
        // 2) create an ImageButton for each application
        // 3) Create Image for the application 
        // 4) Add it to the AppList panel.
        // 5) Set the event handler for each ImageButton, so when clicking it the associated application will run

        //calling the web service
        ApplicationItemEx[] items = proxy.GetApplicationsByCredentialsEx(credentials, Request.UserHostName, Request.UserHostAddress, new string[] { "icon","icon-info" }, new string[] { "all" }, new string[] { "all" });

        //loop for each published application
        for (int i = 0; i < items.Length; i++) {
            //create the ImageButton
            System.Web.UI.WebControls.ImageButton app = new System.Web.UI.WebControls.ImageButton();
            
            //set the Image URL to the created image
            app.ImageUrl = createIcon(items[i].InternalName,items[i].Icon);

            //set the ToolTip to the name of the published application
            app.ToolTip = items[i].InternalName;

            //add the ImageButton to the AppList panel
            AppList.Controls.Add(app);

            //set the event handler for the ImageButton.
            app.Click += new System.Web.UI.ImageClickEventHandler(this.OnApplicationClicked);
            
        }
        

    }

    private void OnApplicationClicked (object sender, System.EventArgs e)
    {
        // Get the event source object.
        System.Web.UI.WebControls.ImageButton app = (System.Web.UI.WebControls.ImageButton)sender;

        ServicePointManager.Expect100Continue = false;

        //Get the ICA file content by calling LaunchApplication web service.
        string ica = proxy.LaunchApplication(app.ToolTip, credentials, Request.UserHostName, Request.UserHostAddress);

        //Set the response content type to "application/x-ica" to run the ICA file.
        Response.ContentType = "application/x-ica";

        //Run the application by writing the ICA file content to the response.
        Response.BinaryWrite(Response.ContentEncoding.GetBytes(ica));
        Response.End();
    }



    public static String createIcon(String iconName, String iconData)
    {
        
        //Save the published application icon to file.
        // 1)Get the encoded bytes from the icon data string
        // 2) write the bytes to a file stream.

        try
        {
            //Get the encoded byte array
            byte[] bimg = System.Convert.FromBase64String(iconData);

            //change the path according to your system.
            string path = "c:/Extentrix/" + iconName + ".gif";


            //write the icon bytes to a file.
            FileStream fStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            BinaryWriter stream = new BinaryWriter(fStream);
            stream.Write(bimg);
            stream.Close();
            fStream.Close();

            return path;

        }
        catch (Exception exc)
        {

        }
        return "";
    }

}
