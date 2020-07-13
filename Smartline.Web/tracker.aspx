<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="tracker.aspx.cs" Inherits="Smartline.Web.tracker"  %>

<%@ Register Assembly="Ext.Net" Namespace="Ext.Net" TagPrefix="ext" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script type="text/javascript">
    var toggle = function (toolbar) {
        Ext.select('.tab-switch').each(function (t) {
            Ext.getCmp(t.dom.id).hide();
        });
    };
</script>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>GUDWIN система GPS мониторинга</title>
    <link href="style/tracker.css" rel="stylesheet" type="text/css" />
</head>
<body>     
    <div class="logo">
        GUDWIN система GPS мониторинга
    </div>
    <form id="Form1" runat="server">
    <ext:ResourceManager ID="ResourceManager1" runat="server" Theme="Gray" />
    <ext:Viewport ID="Viewport1" runat="server" Cls="viewport" Layout="BorderLayout">
        <Items>
            <ext:Panel ID="Panel4" runat="server" Height="105" Width="350" Layout="card" ActiveIndex="0"
                DefaultBorder="false"  Border="false" Region="Center">
                <TopBar>
                    <ext:Toolbar ID="toolbarTabsWrapper" runat="server" Layout="Container" Flat="true">
                        <Items>
                            <ext:Toolbar ID="toolbarTabs" runat="server" Flat="true" EnableOverflow="false">
                                <Items>
                                    <ext:TabStrip ID="tabs" runat="server" StyleSpec="padding-bottom: 2px; margin-left: 275px;"
                                        EnableTabScroll="true">
                                        <Items>
                                            <ext:Tab Text="Карта" Icon="WorldLink" ActionItemID="pnlMap" />
                                            <ext:Tab Text="Автомобили" Icon="Car" ActionItemID="pnlCars" />
                                            <ext:Tab Text="Настройки" Icon="FolderExplore" ActionItemID="pnlSettings" />
                                            <ext:Tab Text="Отчеты" Icon="Report" ActionItemID="pnlReporting" />
                                            <ext:Tab Text="Администрирование" Icon="CogAdd" ActionItemID="pnlAdmin" />
                                        </Items>
                                    </ext:TabStrip>
                                    <ext:ToolbarFill ID="ctl45" />
                                    <ext:Label ID="Label1" runat="server" />
                                    <ext:ToolbarSpacer ID="ctl48" />
                                    <ext:Label ID="authUser" runat="server" Cls="auth-user" />
                                    <ext:ToolbarSpacer Width="5" ID="ctl51" />
                                    <ext:LinkButton ID="Label2" runat="server" Text="Выход" Cls="auth-user-signout">
                                        <DirectEvents>
                                            <Click OnEvent="ClickSignOut">
                                            </Click>
                                        </DirectEvents>
                                    </ext:LinkButton>
                                </Items>
                            </ext:Toolbar>
                        </Items>
                    </ext:Toolbar>
                </TopBar>
                <Items>
                    <ext:Panel ID="pnlMap" runat="server" Header="false" MaskOnDisable="False"  >
                        <Loader Url="map.aspx" Mode="Frame" runat="server" >
                        </Loader>
                    </ext:Panel>
                    <ext:Panel ID="pnlCars" runat="server" Header="false" MaskOnDisable="False" IDMode="Static">
                        <Loader runat="server" Url="cars.aspx" Mode="Frame" >                            
                        </Loader>
                    </ext:Panel>
                    <ext:Panel ID="pnlSettings" runat="server" Header="false" IDMode="Static" AutoScroll="true">
                        <Loader Url="settings.aspx" Mode="Frame" runat="server">
                        </Loader>
                    </ext:Panel>
                    <ext:Panel ID="pnlReporting" runat="server" Header="false" IDMode="Static">
                        <Loader Url="reporting.aspx" Mode="Frame" runat="server">
                        </Loader>
                    </ext:Panel>
                    <ext:Panel ID="pnlAdmin" runat="server" Header="false" IDMode="Static">
                        <Loader Url="admin.aspx" Mode="Frame" runat="server">
                        </Loader>
                    </ext:Panel>
                </Items>
            </ext:Panel>
        </Items>
        <Listeners>
        </Listeners>
    </ext:Viewport>
    </form>
</body>
</html>
