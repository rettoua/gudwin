<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default_new.aspx.cs" Inherits="Smartline.Web._default" %>

<%@ Register Assembly="Ext.Net" Namespace="Ext.Net" TagPrefix="ext" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script>
        var tile = function () {
            Ext.net.Desktop.desktop.tileWindows();
        };

        var cascade = function () {
            Ext.net.Desktop.desktop.cascadeWindows();
        };

        var initSlidePanel = function () {
            this.setHeight(Ext.net.Desktop.desktop.body.getHeight());

            if (!this.windowListen) {
                this.windowListen = true;

                this.el.alignTo(Ext.net.Desktop.desktop.body, 'tl-tr', [0, 0]);
                Ext.EventManager.onWindowResize(initSlidePanel, this);
            }
        };
    </script>
    <style>
        .ux-desktop-shortcut .widen
        {
            margin-left: -10px;
        }
    </style>
</head>
<body>
    <ext:ResourceManager ID="ResourceManager1" runat="server" Locale="RU-ru">
        <Listeners>
            <WindowResize Handler="Ext.net.Bus.publish('App.Desktop.ready');" Buffer="500" />
        </Listeners>
    </ext:ResourceManager>

    <ext:Desktop ID="Desktop1" runat="server">

        <Modules>
            <ext:DesktopModule ModuleID="notepad">
                <Shortcut Name="Notepad" IconCls="x-notepad-shortcut" SortIndex="2" />
                <Launcher Text="Notepad" Icon="ApplicationForm" />
                <Window>
                    <ext:Window ID="Window2" runat="server"
                        Title="Notepad"
                        Width="600"
                        Height="400"
                        Icon="ApplicationForm"
                        AnimCollapse="false"
                        Border="false"
                        HideMode="Offsets"
                        Layout="FitLayout"
                        CloseAction="Destroy">
                        <Items>
                            <ext:HtmlEditor ID="HtmlEditor1" runat="server" Text="Some <b>rich</b> <font color='red'>text</font> goes <u>here</u><br>Give it a try!">
                            </ext:HtmlEditor>
                        </Items>
                    </ext:Window>
                </Window>
            </ext:DesktopModule>

            <ext:DesktopModule ModuleID="map">
                <Shortcut Name="Карта" SortIndex="1">
                </Shortcut>
                <Window>
                    <ext:Window ID="Window1" runat="server"
                        Title="Мониторинг"
                        ExpandOnShow="True"
                        Width="600"
                        Height="400"
                        Icon="ApplicationForm"
                        AnimCollapse="false"
                        Border="false"
                        Maximized="True"
                        HideMode="Offsets"
                        Layout="FitLayout"
                        CloseAction="Destroy">
                        <Loader Url="map.aspx" Mode="Frame">
                            <LoadMask ShowMask="True"></LoadMask>
                        </Loader>
                    </ext:Window>
                </Window>
            </ext:DesktopModule>
            <ext:DesktopModule ModuleID="settings">
                <Window>
                    <ext:Window ID="wndSettings" runat="server"
                        Title="Настройки"
                        Width="600"
                        Height="400"                        
                        Icon="ApplicationForm"
                        AnimCollapse="false"
                        Border="false"
                        HideMode="Offsets"
                        Layout="FitLayout"
                        CloseAction="Hide">
                        <Loader Url="settings.aspx" Mode="Frame">
                            <LoadMask ShowMask="True"></LoadMask>
                        </Loader>
                    </ext:Window>
                </Window>
            </ext:DesktopModule>

            <ext:DesktopModule ModuleID="add-module">
                <Shortcut Name="Render dynamic module" Handler="function() {#{DirectMethods}.AddNewModule();}" X="200" Y="100" TextCls="x-long-label">
                </Shortcut>
            </ext:DesktopModule>

            <ext:DesktopModule ModuleID="add1-module">
                <Shortcut Name="Render another module" Handler="function() {#{DirectMethods}.AddAnotherModule();}" X="200" Y="300" TextCls="x-long-label">
                </Shortcut>
            </ext:DesktopModule>


        </Modules>

        <DesktopConfig Wallpaper="resources/wallpaper_blue.jpg" ShortcutDragSelector="true">
            <ShortcutDefaults IconCls="x-default-shortcut" />
            <ContextMenu>
                <ext:Menu ID="Menu1" runat="server">
                    <Items>
                        <ext:MenuItem ID="MenuItem1" runat="server" Text="Change Settings" />
                        <ext:MenuSeparator ID="MenuSeparator1" runat="server" />
                        <ext:MenuItem ID="MenuItem2" runat="server" Text="Tile" Handler="tile" Icon="ApplicationTileVertical" />
                        <ext:MenuItem ID="MenuItem3" runat="server" Text="Cascade" Handler="cascade" Icon="ApplicationCascade" />
                    </Items>
                </ext:Menu>
            </ContextMenu>

            <Content>
                <ext:Image ID="Image1" runat="server" ImageUrl="resources/docked_title_blue.png"
                    StyleSpec="position:absolute;top: 1px;left: 50%;width: 150px; height: 40px; margin-left: -75px;" />

                <ext:Toolbar ID="Toolbar1" runat="server" Width="320" Floating="true" ClassicButtonStyle="true" Flat="true" Border="false" Shadow="false">
                    <Defaults>
                        <ext:Parameter Name="IconAlign" Value="top" />
                        <ext:Parameter Name="Width" Value="60" />
                        <%--<ext:Parameter Name="Icon" Value="resources/cmd.png" />--%>
                        <ext:Parameter Name="Scale" Value="large" />
                        <ext:Parameter Name="Handler" Value="function(){Ext.Msg.alert('Launch', this.text);}" Mode="Raw" />
                    </Defaults>
                    <Items>
                        <ext:Button ID="Button1" runat="server" Text="Почта" />
                        <ext:Button ID="Button2" runat="server" Text="Excel" />
                        <ext:Button ID="Button3" runat="server" Text="Notepad" />
                        <ext:Button ID="Button4" runat="server" Text="Paint" />
                        <ext:Button ID="Button5" runat="server" Text="Explorer" />
                    </Items>
                    <MessageBusListeners>
                        <ext:MessageBusListener Name="App.Desktop.ready" Handler="this.el.anchorTo(Ext.net.Desktop.desktop.body, 'c-b', [0, -50]);" />
                    </MessageBusListeners>
                </ext:Toolbar>
            </Content>
        </DesktopConfig>

        <StartMenu Title="Имя пользователя" Icon="Application" Height="300">
            <ToolConfig>
                <ext:Toolbar ID="Toolbar2" runat="server" Width="100">
                    <Items>
                        <ext:Button ID="Button6" runat="server" Text="Настройки" Icon="Cog">
                            <Listeners>
                                <Click Handler="App.wndSettings.show();"></Click>
                            </Listeners>
                        </ext:Button>
                        <ext:Button ID="Button7" runat="server" Text="Выход" Icon="Key">
                            <DirectEvents>
                                <Click OnEvent="ClickSignOut">
                                    <EventMask ShowMask="true" Msg="Выход..." />
                                </Click>
                            </DirectEvents>
                        </ext:Button>



                    </Items>
                </ext:Toolbar>
            </ToolConfig>
        </StartMenu>

        <TaskBar TrayWidth="100" StartBtnText="Старт">
            <QuickStart>
                <ext:Toolbar ID="Toolbar3" runat="server">
                    <Items>
                        <ext:Button ID="Button8" runat="server" Handler="tile" Icon="ApplicationTileVertical">
                            <QTipCfg Text="Tile windows" />
                        </ext:Button>

                        <ext:Button ID="Button9" runat="server" Handler="cascade" Icon="ApplicationCascade">
                            <QTipCfg Text="Cascade windows" />
                        </ext:Button>
                    </Items>
                </ext:Toolbar>
            </QuickStart>

            <Tray>
                <ext:Toolbar ID="Toolbar4" runat="server">
                    <Items>
                        <ext:Button ID="LangButton" runat="server" Text="RU" MenuArrow="False" Cls="x-bold-text" MenuAlign="br-tr">
                            <Menu>
                                <ext:Menu ID="Menu2" runat="server">
                                    <Items>
                                        <ext:CheckMenuItem ID="CheckMenuItem1" runat="server" Group="lang" Text="Русский" Checked="true" CheckHandler="function(item, checked) {checked && #{LangButton}.setText('RU');}" />
                                        <ext:CheckMenuItem ID="CheckMenuItem2" runat="server" Group="lang" Text="Ураїнська" CheckHandler="function(item, checked) {checked && #{LangButton}.setText('UA');}" />
                                        <ext:MenuSeparator ID="MenuSeparator2" runat="server" />
                                        <ext:MenuItem ID="MenuItem4" runat="server" Text="Show the Language Bar" />
                                    </Items>
                                </ext:Menu>
                            </Menu>
                        </ext:Button>
                        <ext:ToolbarFill ID="ToolbarFill1" runat="server" />
                    </Items>
                </ext:Toolbar>
            </Tray>
        </TaskBar>
        <Listeners>
            <Ready BroadcastOnBus="App.Desktop.ready" />
        </Listeners>


    </ext:Desktop>
</body>
</html>
