﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using SRP_DAL;
using SRPApp.Classes;
using STG.SRP.ControlRooms;
using STG.SRP.Core.Utilities;
using STG.SRP.Utilities;


namespace STG.SRP.ControlRoom.Modules.Tenant
{

    public partial class TenantGroupList : BaseControlRoomPage
	{
        	private String _mStrSortExp;
	        private SortDirection _mSortDirection = SortDirection.Ascending;

        protected void Page_Load(object sender, EventArgs e)
        {

            MasterPage.IsSecure = true;
            MasterPage.PageTitle = "Organization User Group List";

            ControlRoomAccessPermission.CheckControlRoomAccessPermission(1000); // User Security;

            if (!IsPostBack)
            {
                List<RibbonPanel> moduleRibbonPanels = StandardModuleRibbons.MasterTenantRibbon();
                foreach (var moduleRibbonPanel in moduleRibbonPanels)
                {
                    MasterPage.PageRibbon.Add(moduleRibbonPanel);
                }
                MasterPage.PageRibbon.DataBind();
            }

            _mStrSortExp = String.Empty;
            if (!IsPostBack)
            {
                _mStrSortExp = String.Empty;
                
            }
            else
            {
                if (null != ViewState["_SortExp_"])
                {
                    _mStrSortExp = ViewState["_SortExp_"] as String;
                }

                if (null != ViewState["_Direction_"])
                {
                    _mSortDirection = (SortDirection)ViewState["_Direction_"];
                }
            }

            if (!IsPostBack)
            {
                if (Session["ATENID"] != null)
                {
                    TenID.SelectedValue = Session["ATENID"].ToString();
                    LoadData(Session["ATENID"].ToString());
                }
            }

        }

        protected void GvSorting(object sender, GridViewSortEventArgs e)
        {
            if (String.Empty != _mStrSortExp)
            {
                if (String.Compare(e.SortExpression, _mStrSortExp, true) == 0)
                {
                    _mSortDirection =
                        (_mSortDirection == SortDirection.Ascending) ? SortDirection.Descending : SortDirection.Ascending;
                }
            }
            ViewState["_Direction_"] = _mSortDirection;
            ViewState["_SortExp_"] = _mStrSortExp = e.SortExpression;
            LoadData();
        }

        protected void GvRowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                if (String.Empty != _mStrSortExp)
                {
                    GlobalUtilities.AddSortImage(e.Row, (GridView)sender, _mStrSortExp, _mSortDirection);
                }
            }
        }

        protected void GvSelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void LoadData(string TenIDOverride = "0")
        {
            var ds = new DataSet();
            var dt = new DataTable();
            
            var aTenID = (TenIDOverride == "0" ? TenID.SelectedValue : TenIDOverride);
            Session["ATENID"] = aTenID;
            if (aTenID != "0")
            {
                
                dt = SRPGroup.FetchAllAsDataTable(int.Parse(aTenID));
            } 
            
            
            ds.Tables.Add(dt);
            if (_mStrSortExp != "")
            {
                dt.DefaultView.Sort = _mStrSortExp + (_mSortDirection == SortDirection.Descending ? " DESC" : "");
                var ds2 = new DataSet();
                ds2.Tables.Add(dt.DefaultView.ToTable());
                ds = ds2;
            }
            gv.DataSource = ds;
            gv.DataBind();
        }

        protected void GvRowCommand(object sender, GridViewCommandEventArgs e)
        {
            string editpage = "~/ControlRoom/Modules/Tenant/TenantGroupsAddEdit.aspx";
            if (e.CommandName.ToLower() == "addrecord")
            {
                Session["GID"] = ""; 
                Response.Redirect(editpage);
            }
            if (e.CommandName.ToLower() == "editrecord")
            {
                int key = Convert.ToInt32(e.CommandArgument);
                Session["GID"] = key; Response.Redirect(editpage);
            }
            if (e.CommandName.ToLower() == "deleterecord")
            {
                int key = Convert.ToInt32(e.CommandArgument);
                try
                {

                    var obj = new SRPGroup(key);
                    if (obj.IsValid(BusinessRulesValidationMode.DELETE))
                    {
                        SRPGroup.Delete(key);

                        LoadData();
                        var masterPage = (IControlRoomMaster)Master;
                        if (masterPage != null) masterPage.PageMessage = SRPResources.DeleteOK;
                    }
                    else
                    {
                        var masterPage = (IControlRoomMaster)Master;
                        string message = String.Format(SRPResources.ApplicationError1, "<ul>");
                        foreach (BusinessRulesValidationMessage m in obj.ErrorCodes)
                        {
                            message = string.Format(String.Format("{0}<li>{{0}}</li>", message), m.ErrorMessage);
                        }
                        message = string.Format("{0}</ul>", message);
                        if (masterPage != null) masterPage.PageError = message;
                    }


                    
                }
                catch (Exception ex)
                {
                    var masterPage = (IControlRoomMaster)Master;
                    if (masterPage != null)
                        masterPage.PageError = String.Format(SRPResources.ApplicationError1, ex.Message);
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}

