﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.ApplicationBlocks.Data;
using SRPApp.Classes;
using STG.SRP.Core.Utilities;
using STG.SRP.Utilities;

namespace STG.SRP.ControlRoom
{
    public partial class DBCreate : BaseControlRoomPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    var p = DAL.Programs.GetAll();
                    Response.Redirect("~/ControlRoom/");
                    /*
                     * MasterPage.RequiredPermission = 3000;
                    MasterPage.IsSecure = true;

                    FailureText.Visible = true;
                    FailureText.Text =
                        "WARNING - IT APPEARS THAT THE APPLIACTION HAS ALREADY BEEN INSTALLED. CONTINUING WILL DELETE THE CURRENT INSTALL AND ALL ITS DATA AND REINSTALL.  ALL CURRENT DATA WILL BE LOST.";
                     */
                }
                catch
                {
                    // got an error, so there is no database ... continue with initialize ...
                }
                
            }

        }

        protected void InstallBtn_Click(object sender, EventArgs e)
        {
            ////////////////////////////////////////
            ////////////////////////////////////////
            ////////////////////////////////////////
            var InstallFile = "~/ControlRoom/Modules/Install/InstallScript.config";
            ////////////////////////////////////////
            ////////////////////////////////////////
            ////////////////////////////////////////
            

            if (!this.IsValid)
            {
                return;
            }
            var conn = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}",
                                        DBServer.Text, DBName.Text, UserName.Text, Password.Text);
            var rcon = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}",
                                        DBServer.Text, DBName.Text, RunUser.Text, RuntimePassword.Text);
            var mailHost = Mailserver.Text;


            try
            {
                SqlHelper.ExecuteNonQuery(conn, CommandType.Text, "select 1 as abc");
                SqlHelper.ExecuteNonQuery(rcon, CommandType.Text, "select 1 as abc");
            }
            catch (Exception ex)
            {
                errorLabel.Text = FailureText.Text = string.Format("ERROR:{0}", ex.Message);
                return;
            }

            ////////////////////////////////////////
            ////////////////////////////////////////
            ////////////////////////////////////////
            
            var error = "";
            var sr = new StreamReader(Server.MapPath(InstallFile));
            var sb = new StringBuilder();

            using (SqlConnection connection = new SqlConnection(conn))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction = connection.BeginTransaction("InstallTransaction");
                command.Connection = connection;
                command.Transaction = transaction;
               
                while (!sr.EndOfStream)
                {
                    sb = new StringBuilder();
                    while (!sr.EndOfStream)
                    {
                        var s = sr.ReadLine();
                        if (s != null && (s.ToUpper().Trim().Equals("GO") || s.ToUpper().Trim().StartsWith("GO ") || s.ToUpper().Trim().StartsWith("GO--")))
                        {
                            break;
                        }
                        sb.AppendLine(s);
                    }
                    try
                    {
                        command.CommandText = sb.ToString();
                        command.ExecuteNonQuery();
                        //SqlHelper.ExecuteNonQuery(connection, CommandType.Text, sb.ToString());
                    }
                    catch (Exception ex)
                    {
                        error = string.Format("{0}ERROR:{1}<br>DATA:{2}<br>SQL:<br>{3}<hr>", (error.Length == 0 ? "" : error),
                                              ex.Message, ex.Data, sb);
                    }

                }
                sr.Close();
                if (error.Length == 0)
                {
                    try
                    {
                        transaction.Commit();                        
                    }
                    catch (Exception ex)
                    {
                        error = string.Format("{0}ERROR:{1}<br>DATA:{2}<br>SQL:<br>{3}<hr>", (error.Length == 0 ? "" : error),
                                              ex.Message, ex.Data, sb);
                    }
                }
                if (error.Length != 0)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex)
                    {
                        error = string.Format("{0}ERROR:{1}<br>DATA:{2}<br>SQL:<br>{3}<hr>", (error.Length == 0 ? "" : error),
                                              ex.Message, ex.Data, sb);
                    }
                }

            }


            if (error.Length == 0)
            {
                var config = System.IO.File.ReadAllText(Server.MapPath("~/web.config"));

                config =
                    config.Replace(
                        "connectionString=\"Data Source=(local);Initial Catalog=SRP;User ID=SRP;Password=SRP\"",
                        "connectionString=\"" + rcon + "\"");
                config =
                    config.Replace(
                        "<network host=\"relayServerHostname\" port=\"25\" userName=\"username\" password=\"password\" />",
                        string.Format("<network host=\"{0}\" port=\"25\"/>", mailHost));

                //Modify the web.config
                System.IO.File.WriteAllText(Server.MapPath("~/web.config"), config);
            }

            if (error.Length == 0)
            {
                // Delete the Install File
                //System.IO.File.Delete(Server.MapPath(InstallFile));
                Response.Redirect("~/Default.aspx");
            }
            else
            {
                FailureText.Text = "There have been errors, see details below.";
                errorLabel.Text = error;
            }
        }

        

    }
}