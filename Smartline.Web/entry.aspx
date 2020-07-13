<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="entry.aspx.cs" Inherits="Smartline.Web.entry" %>

<%@ Register Assembly="Ext.Net" Namespace="Ext.Net" TagPrefix="ext" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>GUDWIN система GPS мониторинга</title>
    <script src="Scripts/entry.js" type="text/javascript"></script>
    <link href="style/login.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="Form1" runat="server">
        <asp:ScriptManager runat="server" ID="ScriptManager1" />
        <ext:ResourceManager runat="server" Theme="Gray" DirectMethodNamespace="G.Ajax" />
        <div class="div-logo-name">
            GUDWIN система GPS мониторинга
        </div>
        <ext:Window ID="Window1" runat="server" Closable="false" Resizable="false" Height="170"
            InitCenter="True" Icon="Lock" Title="Авторизация" Draggable="false" Width="350"
            Modal="false" BodyPadding="5" Layout="FormLayout">
            <Items>
                <ext:Panel runat="server" Unstyled="True" Padding="18" Layout="Anchor">
                    <Items>
                        <ext:TextField ID="txtUsername" runat="server" FieldLabel="Логин" AllowBlank="false"
                            BlankText="Введите логин." Text="Demo" LabelWidth="60" AnchorHorizontal="100%">
                            <Validator Fn="check_log_button">
                            </Validator>
                        </ext:TextField>
                        <ext:TextField ID="txtPassword" runat="server" InputType="Password" FieldLabel="Пароль"
                            AllowBlank="false" BlankText="Введите пароль." Text="Demo" LabelWidth="60" AnchorHorizontal="100%">
                            <Validator Fn="check_log_button">
                            </Validator>
                            <KeyMap>
                                <Binding>
                                    <ext:KeyBinding Handler="App.btnLogin.fireEvent('click');">
                                        <Keys>
                                            <ext:Key Code="ENTER" />
                                        </Keys>
                                    </ext:KeyBinding>
                                </Binding>
                            </KeyMap>
                        </ext:TextField>
                        <ext:Label runat="server" ID="lblCheckError" Text="Проверте правильно ввода логина/пароля"
                            Hidden="True" HideMode="Offsets" StyleSpec="color:red;">
                        </ext:Label>
                    </Items>
                </ext:Panel>
            </Items>
            <Buttons>
                <ext:Button ID="btnLogin" runat="server" Text="Войти" Icon="Accept">
                    <DirectEvents>
                        <Click OnEvent="btnLogin_Click">
                            <EventMask Msg="Авторизация...">
                            </EventMask>
                        </Click>
                    </DirectEvents>
                </ext:Button>
            </Buttons>
        </ext:Window>
        <div class="div-copyright">
            <br />
            Copyright © 2013. Smartline. <a class="link" href="http://www.smartline.com.ua">www.smartline.com.ua</a>
        </div>
    </form>
</body>
</html>
