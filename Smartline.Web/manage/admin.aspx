<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="admin.aspx.cs" Inherits="Smartline.Web.admin" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%@ Register Assembly="Ext.Net" Namespace="Ext.Net" TagPrefix="ext" %>

<%@ Register TagPrefix="use" Namespace="Smartline.Web.manage" Assembly="Smartline.Web" %>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <style>
        .list-item
        {
            font: normal 11px tahoma, arial, helvetica, sans-serif;
            padding: 3px 10px 3px 10px;
            border: 1px solid #fff;
            border-bottom: 1px solid #eeeeee;
            white-space: normal;
            color: #555;
        }

            .list-item h3
            {
                display: block;
                font: inherit;
                font-weight: bold;
                margin: 0px;
                color: #222;
            }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <ext:ResourceManager ID="ResourceManager1" runat="server" Theme="Gray" Locale="RU-ru" DirectMethodNamespace="DR" />
            <ext:Viewport runat="server" ID="viewport" Layout="BorderLayout">

                <Items>
                    <ext:Panel ID="PanelTrackers" runat="server" Region="North" IDMode="Static"
                        Title="Трекеры" Collapsible="True" Split="True" Layout="Fit" Height="300" AutoScroll="True" >
                    </ext:Panel>
                    <ext:Panel ID="PanelAdmin" runat="server" Region="Center" Layout="Fit" IDMode="Static">
                    </ext:Panel>
                </Items>
            </ext:Viewport>
        </div>
    </form>
</body>
</html>
