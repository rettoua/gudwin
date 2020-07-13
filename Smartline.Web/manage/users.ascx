<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="users.ascx.cs" Inherits="Smartline.Web.manage.users" %>
<%@ Register Assembly="Ext.Net" Namespace="Ext.Net" TagPrefix="ext" %>
<%--<script type="text/javascript" src="/Scripts/admin.js"></script>--%>
<%--<script type="text/javascript" src="/Scripts/HeaderFilter.js"></script>--%>
<ext:Panel runat="server" Layout="BorderLayout">
    <Items>
        <ext:Panel runat="server" Region="Center" Title="Пользователи" ID="pnlUsers" Layout="BorderLayout">
            <Items>
                <ext:GridPanel runat="server" ID="gridUsers" MinHeight="150" Region="Center">
                    <TopBar>
                        <ext:Toolbar ID="Toolbar1" runat="server">
                            <Items>
                                <ext:Button ID="btnAddUser" runat="server" Text="Новый пользователь" Icon="Add">
                                    <Listeners>
                                        <Click Handler="addUser();"></Click>
                                    </Listeners>
                                </ext:Button>
                                <ext:Button ID="btnDeleteUser" runat="server" Text="Удалить" Icon="Decline" Disabled="True">
                                    <DirectEvents>
                                        <Click OnEvent="btnDeleteUser_click">
                                            <ExtraParams>
                                                <ext:Parameter Name="id" Value="App.gridUsers.selModel.getSelection()[0].get('Id')" Mode="Raw" />
                                            </ExtraParams>
                                        </Click>
                                    </DirectEvents>
                                </ext:Button>
                            </Items>
                        </ext:Toolbar>
                    </TopBar>
                    <Store>
                        <ext:Store runat="server" ID="storeUsers">
                            <Model>
                                <ext:Model runat="server" IDProperty="UserName">
                                    <Fields>
                                        <ext:ModelField Name="Id" Type="Int"></ext:ModelField>
                                        <ext:ModelField Name="UserName" Type="String"></ext:ModelField>
                                        <ext:ModelField Name="Name" Type="String"></ext:ModelField>
                                        <ext:ModelField Name="IsBlocked" Type="Boolean"></ext:ModelField>
                                        <ext:ModelField Name="Reason" Type="String"></ext:ModelField>
                                        <ext:ModelField Name="IsAdmin" Type="Boolean"></ext:ModelField>
                                        <ext:ModelField Name="trackers" IsComplex="True" Type="Object"></ext:ModelField>
                                    </Fields>
                                </ext:Model>
                            </Model>
                        </ext:Store>
                    </Store>
                    <ColumnModel>
                        <Columns>
                            <ext:Column ID="Column1" runat="server" Text="Логин" DataIndex="UserName"></ext:Column>
                            <ext:Column ID="Column2" runat="server" Text="Имя пользователя" DataIndex="Name"></ext:Column>
                            <ext:CheckColumn ID="ColumnIsAdministrator" runat="server" Text="Администратор" DataIndex="IsAdmin"></ext:CheckColumn>
                            <ext:CheckColumn ID="Column3" runat="server" Text="Заблокирован" DataIndex="IsBlocked"></ext:CheckColumn>
                            <ext:Column ID="Column4" runat="server" Text="Причина блокировки" DataIndex="Reason"></ext:Column>
                        </Columns>
                    </ColumnModel>
                    <Plugins>
                        <ext:FilterHeader runat="server">
                        </ext:FilterHeader>
                    </Plugins>
                    <SelectionModel>
                        <ext:RowSelectionModel ID="RowSelectionModel1" runat="server" Mode="Single">
                            <Listeners>
                                <SelectionChange Handler="setUser(selected); loadTrackerByUser(App.gridUsers, App.gridUserTrackers); App.btnDeleteUser.setDisabled(!this.hasSelection()); "></SelectionChange>
                            </Listeners>
                        </ext:RowSelectionModel>
                    </SelectionModel>
                </ext:GridPanel>
                <ext:Panel runat="server" ID="pnlEditUserCommon" Region="South" Title="Редактирование пользователя" Width="400">
                    <Items>
                        <ext:Panel ID="pnlEditUser" runat="server" Unstyled="True" Padding="10" Layout="Anchor" Disabled="True">
                            <Items>
                                <ext:TextField ID="txtLogin" runat="server" FieldLabel="Логин" AnchorHorizontal="100%"
                                    PaddingSpec="0 10 0 0" Width="282">
                                    <Listeners>
                                        <Change Handler="userValidation();"></Change>
                                    </Listeners>
                                </ext:TextField>
                                <ext:Panel runat="server" ID="pnlCheckLoginResult" Unstyled="True" Hidden="True" HideMode="Offsets" PaddingSpec="0 0 0 105">
                                    <Content>
                                        Трекер с ID 
                                    </Content>
                                </ext:Panel>
                                <ext:TextField ID="txtSecret" runat="server" FieldLabel="Пароль" InputType="Password"
                                    AnchorHorizontal="100%" MinLength="6" Note="минимум 6 символов">
                                    <Listeners>
                                        <Change Handler="userValidation();"></Change>
                                    </Listeners>
                                </ext:TextField>
                                <ext:TextField ID="txtName" runat="server" FieldLabel="Имя" AnchorHorizontal="100%">
                                    <Listeners>
                                        <Change Handler="userValidation();"></Change>
                                    </Listeners>
                                </ext:TextField>
                                <ext:Checkbox ID="txtIsAdmin" runat="server" FieldLabel="Администратор" AnchorHorizontal="100%"
                                    HideMode="Offsets" ReadOnly="True">
                                </ext:Checkbox>
                                <ext:Hidden ID="hdnId" runat="server">
                                </ext:Hidden>
                                <ext:Checkbox ID="txtIsBlocked" runat="server" FieldLabel="Заблокирован" AnchorHorizontal="100%">
                                    <Listeners>
                                        <Change Handler="App.txtReason.setVisible(item.getValue() == true);"></Change>
                                    </Listeners>
                                </ext:Checkbox>
                                <ext:TextArea ID="txtReason" runat="server" FieldLabel="Причина" AnchorHorizontal="100%"
                                    Hidden="True" HideMode="Offsets">
                                </ext:TextArea>
                            </Items>
                            <Buttons>
                                <ext:Button ID="Button1" runat="server" Text="Отмена" Icon="Decline">
                                    <Listeners>
                                        <Click Handler="onUserDetailClear();"></Click>
                                    </Listeners>
                                </ext:Button>
                                <ext:Button runat="server" ID="btnSaveUser" Text="Сохранить" Icon="Accept" Disabled="False">
                                    <DirectEvents>
                                        <Click OnEvent="btnSaveClick">
                                            <ExtraParams>
                                                <ext:Parameter Encode="True" Name="user" Value="compileUser()" Mode="Raw" />
                                                <ext:Parameter Encode="True" Name="new" Value="isNewUser()" Mode="Raw" />
                                            </ExtraParams>
                                        </Click>
                                    </DirectEvents>
                                </ext:Button>
                            </Buttons>
                        </ext:Panel>
                    </Items>
                </ext:Panel>
            </Items>
        </ext:Panel>
        <ext:Panel runat="server" ID="Panel2" Region="East" MinWidth="400" AutoScroll="True">
            <Items>
                <ext:GridPanel runat="server" ID="gridUserTrackers" Title="Трекеры пользователя" Width="400" AutoScroll="True">
                    <TopBar>
                        <ext:Toolbar ID="gridUserTrackersToolbar" runat="server" Disabled="True">
                            <Items>
                                <ext:Button ID="Button3" runat="server" Text="Добавить трекер" Icon="Add">
                                    <Listeners>
                                        <Click Handler="App.windowTracker.show();"></Click>
                                    </Listeners>
                                </ext:Button>
                                <ext:Button runat="server" ID="btnResetTracker" Text="Переназначить другому пользователю" Icon="UserAdd" Disabled="True">
                                    <Listeners>
                                        <Click Handler="showResetTracker(#{gridUserTrackers}, #{gridUsers}, #{wndResetTracker}, #{cmbAvailableUser});"></Click>
                                    </Listeners>
                                </ext:Button>
                            </Items>
                        </ext:Toolbar>
                    </TopBar>
                    <Store>
                        <ext:Store runat="server" ID="store1">
                            <Model>
                                <ext:Model runat="server" IDProperty="id">
                                    <Fields>
                                        <ext:ModelField Name="id"></ext:ModelField>
                                        <ext:ModelField Name="name"></ext:ModelField>
                                        <ext:ModelField Name="description"></ext:ModelField>
                                        <ext:ModelField Name="trackerid"></ext:ModelField>
                                        <ext:ModelField Name="oldtracker"></ext:ModelField>
                                    </Fields>
                                </ext:Model>
                            </Model>
                        </ext:Store>
                    </Store>
                    <ColumnModel>
                        <Columns>
                            <ext:Column ID="Column5" runat="server" Text="Название" DataIndex="name"></ext:Column>
                            <ext:Column ID="Column6" runat="server" Text="Описание" DataIndex="description"></ext:Column>
                            <ext:Column ID="CheckColumn1" runat="server" Text="Трекер ID" DataIndex="trackerid"></ext:Column>
                            <ext:CheckColumn ID="Column7" runat="server" Text="Старый трекер" DataIndex="oldtracker"></ext:CheckColumn>
                        </Columns>
                    </ColumnModel>
                    <SelectionModel>
                        <ext:RowSelectionModel ID="RowSelectionModel2" runat="server" Mode="Multi">
                            <Listeners>
                                <SelectionChange Handler="#{btnResetTracker}.setDisabled(!#{gridUserTrackers}.selModel.hasSelection());"></SelectionChange>
                            </Listeners>
                        </ext:RowSelectionModel>
                    </SelectionModel>
                </ext:GridPanel>
            </Items>
        </ext:Panel>
    </Items>
</ext:Panel>
<ext:Window ID="windowTracker" runat="server" Title="Добавление нового трекера" Width="400"
    Hidden="True" Resizable="False" CloseAction="Hide" Modal="True">
    <Items>
        <ext:Panel ID="Panel1" runat="server" Padding="5" Unstyled="True" Layout="Form" ColumnWidth=".5">
            <Items>
                <ext:TextField runat="server" ID="txtUserName" ReadOnly="True"
                    FieldLabel="Пользователь" FieldStyle="color:grey;">
                </ext:TextField>
                <%--<ext:Hidden runat="server" ID="hdn"></ext:Hidden>--%>
                <ext:ComboBox runat="server" ID="cmbInfoTrackers"
                    FieldLabel="Трекер"
                    Editable="True"
                    DisplayField="TrackerId"
                    ValueField="Id"
                    TypeAhead="true"
                    MultiSelect="True"
                    QueryMode="Local"
                    ForceSelection="true"
                    TriggerAction="All"
                    EmptyText="Выберите трекер..."
                    SelectOnFocus="true">
                    <Store>
                        <ext:Store ID="StoreTrackers" runat="server">
                            <Model>
                                <ext:Model ID="Model9" runat="server">
                                    <Fields>
                                        <ext:ModelField Name="Id" />
                                        <ext:ModelField Name="TrackerId" />
                                    </Fields>
                                </ext:Model>
                            </Model>
                        </ext:Store>
                    </Store>
                    <Listeners>
                        <Change Handler="validateNewTracker();"></Change>
                    </Listeners>
                </ext:ComboBox>
            </Items>
            <Buttons>
                <ext:Button runat="server" ID="btnAddTrackerToUser" Text="Добавить" Icon="Accept" Disabled="True">
                    <DirectEvents>
                        <Click OnEvent="btnAddTrackerToUser_click">
                            <EventMask ShowMask="True" Msg="Добавление терекеров"></EventMask>
                        </Click>
                    </DirectEvents>
                </ext:Button>
            </Buttons>
        </ext:Panel>
    </Items>
    <Listeners>
        <Hide Handler="clearNewTrackerData();"></Hide>
    </Listeners>
</ext:Window>

<ext:Window runat="server" ID="wndResetTracker" Title="Добавить трекер(-ы) другому пользователю" Width="400"
    Hidden="True" Resizable="False" CloseAction="Hide" Modal="True">
    <Items>
        <ext:Panel ID="Panel3" runat="server" Padding="5" Unstyled="True" Layout="Form" ColumnWidth="1">
            <Items>
                <ext:ComboBox runat="server" ID="cmbAvailableUser"
                    FieldLabel="Новый пользователь"
                    LabelWidth="150"
                    Editable="True"
                    DisplayField="username"
                    ValueField="id"
                    TypeAhead="true"
                    QueryMode="Local"
                    ForceSelection="true"
                    TriggerAction="All"
                    EmptyText="Выберите нового пользователя"                    
                    SelectOnFocus="true">
                    <Store>
                        <ext:Store ID="storeAvailableUser" runat="server" AutoLoad="False">
                            <Model>
                                <ext:Model ID="modelAvailableUser" runat="server">
                                    <Fields>
                                        <ext:ModelField Name="username" />
                                        <ext:ModelField Name="Name" />
                                        <ext:ModelField Name="id" />
                                    </Fields>
                                </ext:Model>
                            </Model>
                            <Sorters>
                                <ext:DataSorter Property="username" Direction="ASC" />
                            </Sorters>
                        </ext:Store>
                    </Store>
                    <ListConfig>
                        <ItemTpl ID="ItemTpl12" runat="server">
                            <Html>
                                <div class="list-item">
							        <h3>{username}</h3>
							        {Name}
						        </div>
                            </Html>
                        </ItemTpl>
                    </ListConfig>
                    <Listeners>
                        <Change Handler="#{btnApplyChangeTrackerUser}.setDisabled(!this.getValue());"></Change>
                        <BeforeQuery Handler="var q = queryEvent.query;
                                      queryEvent.query = new RegExp(q, 'i');
                                      queryEvent.query.length = q.length;" />
                    </Listeners>
                </ext:ComboBox>
            </Items>
            <Buttons>
                <ext:Button runat="server" ID="btnApplyChangeTrackerUser" Text="Сохранить" Icon="Accept" Disabled="True">
                    <Listeners>
                        <Click Handler="resetTrackers(#{gridUserTrackers}, #{gridUsers}, #{wndResetTracker}, #{cmbAvailableUser});"></Click>
                    </Listeners>
                </ext:Button>
            </Buttons>
        </ext:Panel>
    </Items>
</ext:Window>
