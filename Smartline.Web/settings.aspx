<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="settings.aspx.cs" Inherits="Smartline.Web.settings" %>

<%@ Register Assembly="Ext.Net" Namespace="Ext.Net" TagPrefix="ext" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link href="style/settings.css" rel="stylesheet" type="text/css" />
    <%--<script type="text/javascript" src="Scripts/settings.js"></script>--%>
</head>
<body>
    <form id="form1" runat="server">
        <ext:Window ID="Window1" runat="server" Closable="True" Resizable="false" Height="180"
            InitCenter="True" Icon="Lock" Title="Смена пароля" Draggable="False" Width="400"
            Modal="True" BodyPadding="5" Layout="FormLayout" Hidden="True">
            <Items>
                <ext:Panel ID="Panel1" runat="server" Unstyled="True" Padding="15" Layout="Anchor">
                    <Items>
                        <ext:TextField ID="txtOldSecret" runat="server" InputType="Password" FieldLabel="Старый пароль"
                            BlankText="Введите старый пароль." AllowBlank="False" AnchorHorizontal="100%"
                            LabelWidth="150">
                            <Validator Fn="check">
                            </Validator>
                        </ext:TextField>
                        <ext:TextField ID="txtPassword" runat="server" InputType="Password" FieldLabel="Новый пароль"
                            BlankText="Введите пароль." AllowBlank="False" AnchorHorizontal="100%" LabelWidth="150">
                            <Validator Fn="check">
                            </Validator>
                        </ext:TextField>
                        <ext:TextField ID="txtRepPassword" runat="server" InputType="Password" FieldLabel="Повторно новый пароль"
                            BlankText="Введите пароль." AllowBlank="False" AnchorHorizontal="100%" LabelWidth="150">
                            <Validator Fn="check">
                            </Validator>
                        </ext:TextField>
                        <ext:Label runat="server" ID="lblCheckError" Text="Проверте правильно ввода логина/пароля"
                            Hidden="True" HideMode="Offsets" StyleSpec="color:red;">
                        </ext:Label>
                    </Items>
                </ext:Panel>
            </Items>
            <Buttons>
                <ext:Button ID="btnChange" runat="server" Text="Сменить" Icon="Accept" Disabled="True">
                    <DirectEvents>
                        <Click OnEvent="btnChange_Click" Before="return validate_new_login();">
                        </Click>
                    </DirectEvents>
                </ext:Button>
            </Buttons>
        </ext:Window>
        <ext:ResourceManager ID="ResourceManager1" runat="server" Theme="Gray" DirectMethodNamespace="DR" />
        <ext:Viewport ID="Viewport1" runat="server" Layout="BorderLayout" Cls="viewport">
            <Items>
                <ext:Panel runat="server" Region="West" Unstyled="True" Padding="8">
                    <Items>
                        <ext:FieldContainer runat="server"
                            Layout="HBoxLayout"
                            AnchorHorizontal="100%"
                            FieldLabel="Максимальное количество точек трека. 0 - без ограничения"
                            LabelWidth="200">
                            <Items>
                                <ext:NumberField runat="server" ID="fldPoints" MinValue="0" MaxValue="500">
                                </ext:NumberField>
                                <ext:Button ID="Button1" runat="server" Text="Применить" Icon="Accept" MarginSpec="0 0 0 15">
                                    <Listeners>
                                        <Click Handler="savePoinsSettings(#{fldPoints}.getValue());">
                                        </Click>
                                    </Listeners>
                                </ext:Button>
                            </Items>
                        </ext:FieldContainer>
                        <%--<ext:FieldContainer runat="server"
                            Layout="HBoxLayout"
                            AnchorHorizontal="100%"
                            FieldLabel="Толщина линии трека"
                            LabelWidth="200">
                            <Items>
                                <ext:NumberField runat="server" ID="fldWeight" MinValue="1" MaxValue="3">
                                </ext:NumberField>
                                <ext:Button ID="Button2" runat="server" Text="Применить" Icon="Accept" MarginSpec="0 0 0 15">
                                    <Listeners>
                                        <Click Handler="saveWeightSettings(#{fldWeight}.getValue());">
                                        </Click>
                                    </Listeners>
                                </ext:Button>
                            </Items>
                        </ext:FieldContainer>--%>
                        <ext:FieldSet ID="FieldSet1" runat="server" Title="Пользователь">
                            <Items>
                                <ext:FieldContainer runat="server" Layout="HBoxLayout" AnchorHorizontal="100%" FieldLabel="Логин"
                                    LabelWidth="100">
                                    <Items>
                                        <ext:TextField runat="server" ID="txtUserName" ReadOnly="True">
                                        </ext:TextField>
                                        <ext:Button runat="server" Text="Сменить пароль..." Icon="Accept" MarginSpec="0 0 0 15">
                                            <Listeners>
                                                <Click Handler="App.Window1.show();">
                                                </Click>
                                            </Listeners>
                                        </ext:Button>
                                    </Items>
                                </ext:FieldContainer>
                                <ext:TextField runat="server" ID="txtName" FieldLabel="Имя (название)" Width="373">
                                </ext:TextField>
                                <ext:FieldContainer runat="server" Layout="HBoxLayout" AnchorHorizontal="100%" LabelWidth="100">
                                    <Items>
                                        <ext:Button ID="ButtonSaveUser" runat="server" Text="Сохранить" Icon="PictureSave" TextAlign="Right">
                                            <DirectEvents>
                                                <Click OnEvent="ButtonSaveClick_user"></Click>
                                            </DirectEvents>
                                        </ext:Button>
                                    </Items>
                                </ext:FieldContainer>
                            </Items>
                        </ext:FieldSet>
                        <ext:FormPanel runat="server" ID="pnlEvosIntegration" BodyPadding="10" Layout="Form" Width="400">
                            <Items>
                                <ext:Checkbox ID="chkOnIntegration" runat="server" BoxLabel="Интегация с программным комплексом Такси Навигатор компании Evos">
                                    <Listeners>
                                        <Change Handler="changeIntegarationButtonState();"></Change>
                                    </Listeners>
                                </ext:Checkbox>
                                <ext:FieldContainer ID="cntSecurity" runat="server" Layout="Anchor" Disabled="True">
                                    <Items>
                                        <ext:TextField ID="txtIntegrationLogin" runat="server" FieldLabel="Логин" AnchorHorizontal="100%"
                                            AllowBlank="False">
                                        </ext:TextField>
                                        <ext:TextField ID="txtIntegrationSecret" runat="server" InputType="Password" FieldLabel="Пароль"
                                            AnchorHorizontal="100%" MinLength="6" Note="Длина пароля должна быть не меньше 6 символов"
                                            AllowBlank="False">
                                        </ext:TextField>
                                    </Items>
                                </ext:FieldContainer>
                            </Items>
                            <Defaults>
                                <ext:Parameter Name="AllowBlank" Value="false" Mode="Raw" />
                                <ext:Parameter Name="MsgTarget" Value="side" />
                            </Defaults>
                            <Buttons>
                                <ext:Button ID="btnSaveEvosIntegration" runat="server" Icon="Accept" Text="Сохранить" FormBind="true"
                                    Disabled="True">
                                    <Listeners>
                                        <Click Fn="saveEvosIntegration"></Click>
                                    </Listeners>
                                </ext:Button>
                            </Buttons>
                            <Listeners>
                                <ValidityChange Handler=" #{btnSaveEvosIntegration}.setDisabled(!valid );" />
                            </Listeners>
                        </ext:FormPanel>
                    </Items>
                </ext:Panel>
                <ext:Panel runat="server" Title="Операторы" Region="Center" Layout="Border">
                    <Items>
                        <ext:Panel runat="server" Region="North" Split="True">
                            <Items>
                                <ext:GridPanel ID="gridPanelOperators" runat="server"
                                    InvalidateScrollerOnRefresh="False"
                                    MinHeight="150"
                                    AnchorHorizontal="100%">
                                    <TopBar>
                                        <ext:Toolbar runat="server">
                                            <Items>
                                                <ext:Button runat="server" ID="btnOpAdd" Text="Добавить оператора" Icon="Add">
                                                    <Listeners>
                                                        <Click Handler="settings.operators.addOperator();"></Click>
                                                    </Listeners>
                                                </ext:Button>
                                                <ext:Button runat="server" ID="btnOpEdit" Text="Редактировать" Icon="CogEdit" Disabled="True">
                                                    <Listeners>
                                                        <Click Handler="settings.operators.editOperator();"></Click>
                                                    </Listeners>
                                                </ext:Button>
                                                <ext:Button ID="btnOpRemove" runat="server" Text="Удалить оператора" Icon="Delete" Disabled="True">
                                                    <Listeners>
                                                        <Click Handler="settings.operators.removeOperator();"></Click>
                                                    </Listeners>
                                                </ext:Button>
                                                <ext:ToolbarFill runat="server" />
                                                <ext:Button ID="Button5" runat="server" Text="Обновить" Icon="ArrowRefresh">
                                                    <Listeners>
                                                        <Click Handler="settings.operators.loadOperators();"></Click>
                                                    </Listeners>
                                                </ext:Button>
                                                <ext:Button ID="Button4" runat="server" Text="Сохранить" Icon="PageSave">
                                                    <Listeners>
                                                        <Click Handler="#{gridPanelOperators}.store.sync();"></Click>
                                                    </Listeners>
                                                </ext:Button>
                                            </Items>
                                        </ext:Toolbar>
                                    </TopBar>
                                    <Store>
                                        <ext:Store ID="Store2" runat="server" AutoDataBind="True">
                                            <Proxy>
                                                <ext:AjaxProxy Url="operators.ashx">
                                                    <Reader>
                                                        <ext:JsonReader Root="data">
                                                        </ext:JsonReader>
                                                    </Reader>
                                                    <Writer>
                                                        <ext:JsonWriter Root="data" Encode="true" />
                                                    </Writer>
                                                </ext:AjaxProxy>
                                            </Proxy>
                                            <Model>
                                                <ext:Model ID="Model1" runat="server" IDProperty="Id">
                                                    <Fields>
                                                        <ext:ModelField Name="Id" Type="Int" />
                                                        <ext:ModelField Name="Name" Type="String" />
                                                        <ext:ModelField Name="UserName" Type="String" />
                                                        <ext:ModelField Name="NormalSecret" Type="String" />
                                                        <ext:ModelField Name="IsBlocked" Type="Boolean" />
                                                        <ext:ModelField Name="Trackers" Type="Object" />
                                                    </Fields>
                                                </ext:Model>
                                            </Model>
                                            <Sorters>
                                                <ext:DataSorter Property="Name" Direction="ASC" />
                                            </Sorters>
                                            <Listeners>
                                            </Listeners>
                                        </ext:Store>
                                    </Store>
                                    <ColumnModel ID="ColumnModel1" runat="server" ForceFit="True" AnchorHorizontal="100%">
                                        <Columns>
                                            <ext:Column ID="Column1" runat="server" Text="Логин" DataIndex="UserName"></ext:Column>
                                            <ext:Column runat="server" Text="Имя" DataIndex="Name"></ext:Column>
                                            <ext:CheckColumn ID="Column2" runat="server" Text="Заблокированный" DataIndex="IsBlocked"></ext:CheckColumn>
                                        </Columns>
                                    </ColumnModel>
                                    <SelectionModel>
                                        <ext:RowSelectionModel ID="rsmOp" runat="server" Mode="Single">
                                            <Listeners>
                                                <SelectionChange Handler="settings.operators.updateButtonStates();"></SelectionChange>
                                            </Listeners>
                                        </ext:RowSelectionModel>
                                    </SelectionModel>
                                    <Listeners>
                                        <CellDblClick Handler="settings.operators.editOperator();"></CellDblClick>
                                    </Listeners>
                                </ext:GridPanel>
                            </Items>
                        </ext:Panel>
                        <ext:Panel runat="server" Region="Center">
                            <Items>
                                <ext:Panel runat="server" Layout="TableLayout" MinHeight="250" Unstyled="True">
                                    <Items>
                                        <ext:Panel ID="Panel4" runat="server" Unstyled="True">
                                            <Items>
                                                <ext:GridPanel ID="gridOpTrackers" runat="server" Title="Трекеры которые видит оператор" Width="250" MinHeight="200" MaxHeight="200">
                                                    <Store>
                                                        <ext:Store ID="Store1" runat="server">
                                                            <Model>
                                                                <ext:Model ID="Model2" runat="server" IDProperty="Id">
                                                                    <Fields>
                                                                        <ext:ModelField Name="Id" />
                                                                        <ext:ModelField Name="Name" Type="String" />
                                                                        <ext:ModelField Name="TrackerId" Type="Int" />
                                                                    </Fields>
                                                                </ext:Model>
                                                            </Model>
                                                            <Sorters>
                                                                <ext:DataSorter Property="Name" Direction="ASC" />
                                                            </Sorters>
                                                        </ext:Store>
                                                    </Store>
                                                    <ColumnModel>
                                                        <Columns>
                                                            <ext:Column ID="Column3" runat="server" Text="Название" DataIndex="Name" Flex="1">
                                                            </ext:Column>
                                                            <ext:Column ID="Column5" runat="server" Text="ID" DataIndex="TrackerId">
                                                            </ext:Column>
                                                        </Columns>
                                                    </ColumnModel>
                                                    <SelectionModel>
                                                        <ext:RowSelectionModel runat="server" Mode="Multi">
                                                            <Listeners>
                                                                <SelectionChange Handler="settings.operators.updateTrackersButtons();"></SelectionChange>
                                                            </Listeners>
                                                        </ext:RowSelectionModel>
                                                    </SelectionModel>
                                                </ext:GridPanel>
                                            </Items>
                                        </ext:Panel>
                                        <ext:Panel ID="Panel6" runat="server" Width="40" Unstyled="True">
                                            <Items>
                                                <ext:Button ID="bntRemoveTracker" runat="server" Text=">" StyleSpec="margin-left:9px; " Disabled="True">
                                                    <Listeners>
                                                        <Click Handler="settings.operators.removeTracker();"></Click>
                                                    </Listeners>
                                                </ext:Button>
                                                <ext:Button ID="bntAddTracker" runat="server" Text="<" Disabled="True" StyleSpec="margin-left:9px;margin-top:2px;">
                                                    <Listeners>
                                                        <Click Handler="settings.operators.addTracker();"></Click>
                                                    </Listeners>
                                                </ext:Button>
                                                <ext:Button ID="bntRemoveAllTracker" runat="server" Text=">>" Disabled="True" StyleSpec="margin-left:5px;margin-top:2px;">
                                                    <Listeners>
                                                        <Click Handler="settings.operators.removeAllTrackers();"></Click>
                                                    </Listeners>
                                                </ext:Button>
                                                <ext:Button ID="bntAddAllTracker" runat="server" Text="<<" Disabled="True" StyleSpec="margin-left:5px;margin-top:2px;">
                                                    <Listeners>
                                                        <Click Handler="settings.operators.addAllTrackers();"></Click>
                                                    </Listeners>
                                                </ext:Button>
                                            </Items>
                                        </ext:Panel>
                                        <ext:Panel ID="Panel5" runat="server" Unstyled="True">
                                            <Items>
                                                <ext:GridPanel ID="gridAllTrackers" runat="server" Title="Все трекеры" Width="250" MinHeight="200" MaxHeight="200">
                                                    <Store>
                                                        <ext:Store ID="Store3" runat="server">
                                                            <Model>
                                                                <ext:Model ID="Model3" runat="server" IDProperty="Id">
                                                                    <Fields>
                                                                        <ext:ModelField Name="Id" />
                                                                        <ext:ModelField Name="Name" Type="String" />
                                                                        <ext:ModelField Name="TrackerId" Type="Int" />
                                                                    </Fields>
                                                                </ext:Model>
                                                            </Model>
                                                            <Sorters>
                                                                <ext:DataSorter Property="Name" Direction="ASC" />
                                                            </Sorters>
                                                        </ext:Store>
                                                    </Store>
                                                    <ColumnModel>
                                                        <Columns>
                                                            <ext:Column ID="Column4" runat="server" Text="Название" DataIndex="Name" Flex="1">
                                                            </ext:Column>
                                                            <ext:Column ID="Column6" runat="server" Text="ID" DataIndex="TrackerId">
                                                            </ext:Column>
                                                        </Columns>
                                                    </ColumnModel>
                                                    <SelectionModel>
                                                        <ext:RowSelectionModel ID="RowSelectionModel1" runat="server" Mode="Multi">
                                                            <Listeners>
                                                                <SelectionChange Handler="settings.operators.updateTrackersButtons();"></SelectionChange>
                                                            </Listeners>
                                                        </ext:RowSelectionModel>
                                                    </SelectionModel>
                                                </ext:GridPanel>
                                            </Items>
                                        </ext:Panel>
                                    </Items>
                                </ext:Panel>
                            </Items>
                        </ext:Panel>
                    </Items>
                </ext:Panel>
            </Items>
        </ext:Viewport>
        <ext:Window ID="wndOperator" runat="server" Closable="True" Resizable="false" Height="220"
            InitCenter="True" Icon="Lock" Title="Оператор" Draggable="True" Width="350"
            Modal="True" BodyPadding="5" Layout="FormLayout" Hidden="True">
            <Items>
                <ext:Panel ID="Panel2" runat="server" Unstyled="True" Padding="5" Layout="Anchor">
                    <Items>
                        <ext:TextField ID="txtOpUserName" runat="server" FieldLabel="Логин" Note="не меньше 4 символов"
                            BlankText="Логин" AllowBlank="False" AnchorHorizontal="100%" MinLength="4"
                            LabelWidth="100">
                            <Listeners>
                                <Change Handler="settings.operators.validateWindow();"></Change>
                            </Listeners>
                        </ext:TextField>
                        <ext:TextField ID="txtOpSecret" runat="server" FieldLabel="Пароль" Note="не меньше 6 символов" MinLength="6"
                            BlankText="Введите пароль" AllowBlank="False" AnchorHorizontal="100%" LabelWidth="100">
                            <Listeners>
                                <Change Handler="settings.operators.validateWindow();"></Change>
                            </Listeners>
                        </ext:TextField>
                        <ext:TextField ID="txtOpName" runat="server" FieldLabel="Имя"
                            AnchorHorizontal="100%" LabelWidth="100">
                            <Listeners>
                                <Change Handler="settings.operators.validateWindow();"></Change>
                            </Listeners>
                        </ext:TextField>
                        <ext:Checkbox ID="chkOpBlocked" runat="server" FieldLabel="Заблокировать"
                            AnchorHorizontal="100%" LabelWidth="100">
                            <Listeners>
                                <Change Handler="settings.operators.validateWindow();"></Change>
                            </Listeners>
                        </ext:Checkbox>
                    </Items>
                    <Buttons>
                        <ext:Button runat="server" ID="btnOpSave" Text="Применить" Icon="Accept" Disabled="True">
                            <Listeners>
                                <Click Handler="settings.operators.save();"></Click>
                            </Listeners>
                        </ext:Button>
                    </Buttons>
                </ext:Panel>
            </Items>
        </ext:Window>
    </form>
</body>
</html>
