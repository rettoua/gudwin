<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="Smartline.Web.entry" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>GUDWIN система GPS мониторинга</title>    
    <link href="style/login.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="Form1" runat="server">
        <asp:ScriptManager runat="server" ID="ScriptManager1" />
        <ext:ResourceManager runat="server" Theme="Gray" DirectMethodNamespace="G.Ajax" Locale="RU-ru" />
        <ext:Viewport ID="Viewport1" runat="server" Cls="viewport" StyleSpec="background-color:#FFF5F5F5;">
            <Items>
                <ext:Panel runat="server" Unstyled="True" ID="pnlDockedTitle" Cls="login-title" >
                   <Content>GUDWIN система GPS мониторинга</Content>                                 
                </ext:Panel>
                <ext:Panel ID="WindowLogin" runat="server" DefaultButton="btnLogin" MarginSpec="200 0 0 0"
                    Icon="Lock" Title="Авторизация" Width="350" BodyStyle="background:white;">
                    <Items>
                        <ext:Panel runat="server" Unstyled="True" PaddingSpec="18 18 18 18" Layout="Anchor">
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
                                </ext:TextField>
                                <ext:Checkbox runat="server" ID="chkSaveMe" FieldLabel="Запомнить меня"></ext:Checkbox>
                                <ext:Label runat="server" ID="lblCheckError" Text="Проверьте правильность ввода логина и пароля"
                                    Hidden="True" HideMode="Display" StyleSpec="color:red;" Height="18">
                                </ext:Label>
                                <ext:Button ID="btnLogin" runat="server" Text="Войти" Icon="Accept" MarginSpec="0 0 0 230"
                                    Width="80">
                                    <DirectEvents>
                                        <Click OnEvent="btnLogin_Click">
                                        </Click>
                                    </DirectEvents>
                                    <Listeners>
                                        <Click Handler="App.WindowLogin.el.mask('Авторизация...');"></Click>
                                    </Listeners>
                                </ext:Button>
                            </Items>
                        </ext:Panel>

                    </Items>

                </ext:Panel>
            </Items>
            <LayoutConfig>
                <ext:VBoxLayoutConfig Align="Center" />
            </LayoutConfig>
        </ext:Viewport>
        <div class="div-copyright">
            <br />
            Copyright © 2016. Smartline. <a class="link" href="http://www.smartline.com.ua">www.smartline.com.ua</a>
        </div>
    </form>
</body>
</html>
