﻿using System;
using System.Web.UI.WebControls;
using SRPApp.Classes;
using STG.SRP.ControlRooms;
using STG.SRP.Core.Utilities;
using STG.SRP.DAL;


namespace STG.SRP.ControlRoom.Modules.Setup
{
    public partial class MGMixAndMatchItemsList : BaseControlRoomPage
    {
        private String _mStrSortExp;
        private SortDirection _mSortDirection = SortDirection.Ascending;


        protected void Page_Load(object sender, EventArgs e)
        {
            MasterPage.RequiredPermission = 4300;
            MasterPage.IsSecure = true;
            MasterPage.PageTitle = string.Format("{0}", "Mix-And-Match Items");

            _mStrSortExp = String.Empty;
            
            if (!IsPostBack)
            {
                SetPageRibbon(StandardModuleRibbons.SetupRibbon());
                if (Session["MGID"] != null)
                {
                    lblMGID.Text = Session["MGID"].ToString();

                    var o = Minigame.FetchObject(int.Parse(lblMGID.Text));
                    AdminName.Text = o.AdminName;

                    var o2 = MGMixAndMatch.FetchObjectByParent(int.Parse(lblMGID.Text));
                    lblMMID.Text = o2.MMID.ToString();

                }
                else
                {
                    Response.Redirect("MiniGameList.aspx");
                }

            
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

        protected void LoadData()
        {
            odsData.Select();
            gv.DataBind();
        }


        protected void GvRowCommand(object sender, GridViewCommandEventArgs e)
        {
            string editpage = "~/ControlRoom/Modules/Setup/MGMixAndMatchItemsAddEdit.aspx";
            if (e.CommandName.ToLower() == "addrecord")
            {
                Response.Redirect(String.Format("{0}?MGID={1}&MMID={2}", editpage, lblMGID.Text, lblMMID.Text));
                //Response.Redirect(editpage);
            }
            if (e.CommandName.ToLower() == "back")
            {
                Response.Redirect("~/ControlRoom/Modules/Setup/MGMixAndMatchAddEdit.aspx"); 
                //Response.Redirect(String.Format("~/ControlRoom/Modules/Setup/MGMixAndMatchAddEdit.aspx?PK={0}", lblMGID.Text));
            }
            if (e.CommandName.ToLower() == "editrecord")
            {
                int key = Convert.ToInt32(e.CommandArgument);
                Response.Redirect(String.Format("{0}?PK={1}", editpage, key));
            }
            if (e.CommandName.ToLower() == "deleterecord")
            {
                var key = Convert.ToInt32(e.CommandArgument);
                try
                {
                    var obj = new MGMixAndMatchItems();
                    if (obj.IsValid(BusinessRulesValidationMode.DELETE))
                    {
                        MGMixAndMatchItems.FetchObject(key).Delete();

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
    }
}

