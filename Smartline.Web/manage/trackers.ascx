<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="trackers.ascx.cs" Inherits="Smartline.Web.manage.trackers" %>
<%@ Register Assembly="Ext.Net" Namespace="Ext.Net" TagPrefix="ext" %>

<ext:GridPanel runat="server" ID="gridTrackers">
    <TopBar>
        <ext:Toolbar runat="server">
            <Items>
                <ext:Button ID="btnAddTracker" runat="server" Text="Добавить" Icon="Add">
                    <Listeners>
                        <Click Handler="App.windowTrack.show();"></Click>
                    </Listeners>
                </ext:Button>
                <ext:Button ID="btnDeleteTracker" runat="server" Text="Удалить"
                    Icon="Decline" Disabled="True">
                    <Listeners>
                        <Click Handler="deleteTrackerInfoConfirmation(App.gridTrackers);"></Click>
                    </Listeners>
                </ext:Button>
            </Items>
        </ext:Toolbar>
    </TopBar>
    <Store>
        <ext:Store runat="server"
            ID="storeTrackers"
            PageSize="25"
            RemoteSort="False">
            <Model>
                <ext:Model ID="Model1" runat="server" IDProperty="Id">
                    <Fields>
                        <ext:ModelField Name="Id" Type="Int"></ext:ModelField>
                        <ext:ModelField Name="TrackerId" Type="Int"></ext:ModelField>
                        <ext:ModelField Name="AddTime" Type="Date"></ext:ModelField>
                        <ext:ModelField Name="AddedBy" Type="String"></ext:ModelField>
                        <ext:ModelField Name="IP" Type="String"></ext:ModelField>
                        <ext:ModelField Name="User" Type="String"></ext:ModelField>
                        <ext:ModelField Name="Owner" Type="String"></ext:ModelField>
                        <ext:ModelField Name="ApplyTime" Type="Date"></ext:ModelField>
                        <ext:ModelField Name="OldTracker" Type="Boolean"></ext:ModelField>
                    </Fields>
                </ext:Model>
            </Model>
        </ext:Store>
    </Store>
    <ColumnModel>
        <Columns>
            <ext:Column runat="server" Text="Идентификатор в системе" DataIndex="Id"></ext:Column>
            <ext:Column runat="server" Text="Идентификатор" DataIndex="TrackerId"></ext:Column>
            <ext:DateColumn runat="server" Text="Когда добавили" DataIndex="AddTime" Format="d-m-Y H:i:s"></ext:DateColumn>
            <ext:Column runat="server" Text="Кем добавлен" DataIndex="AddedBy"></ext:Column>
            <ext:Column runat="server" Text="Владелец" DataIndex="Owner"></ext:Column>
            <ext:Column runat="server" Text="С какого IP" DataIndex="IP"></ext:Column>
            <ext:Column runat="server" Text="Используется пользователем" DataIndex="User" Width="170"></ext:Column>
            <ext:DateColumn runat="server" Text="Добавили пользователю" DataIndex="ApplyTime" Format="d-m-Y H:i:s"></ext:DateColumn>
            <ext:CheckColumn runat="server" Text="Старый трекер(без дистанции)" DataIndex="OldTracker"></ext:CheckColumn>
        </Columns>
    </ColumnModel>
    <Plugins>
        <ext:FilterHeader runat="server">
        </ext:FilterHeader>
    </Plugins>
    <SelectionModel>
        <ext:RowSelectionModel Mode="Single">
            <Listeners>
                <SelectionChange Handler="App.btnDeleteTracker.setDisabled(!this.hasSelection());"></SelectionChange>
            </Listeners>
        </ext:RowSelectionModel>
    </SelectionModel>
    <BottomBar>
        <ext:PagingToolbar runat="server" HideRefresh="True">
        </ext:PagingToolbar>
    </BottomBar>
</ext:GridPanel>
<ext:Window ID="windowTrack" runat="server" Title="Добавление нового трекера" Width="400"
    Hidden="True" Resizable="False" CloseAction="Hide" Modal="True">
    <Items>
        <ext:Panel runat="server" Padding="5" Unstyled="True" Layout="Form" ColumnWidth=".5">
            <Items>
                <ext:FieldContainer runat="server" Layout="HBoxLayout">
                    <Items>
                        <ext:NumberField runat="server" ID="txtTrackerId" DecimalPrecision="0" HideTrigger="True"
                            FieldLabel="Трекер ID" Width="280" PaddingSpec="0 10 0 0">
                            <Listeners>
                                <Change Handler="validateTrackerInfo(); "></Change>
                            </Listeners>
                        </ext:NumberField>
                    </Items>
                </ext:FieldContainer>
                <ext:Panel runat="server" ID="pnlCheckTrackerResult" Unstyled="True" Hidden="True" HideMode="Offsets" PaddingSpec="0 0 0 105">
                    <Content>
                        Трекер с ID 
                    </Content>
                </ext:Panel>
                <ext:NumberField runat="server" ID="txtTrackerCount" DecimalPrecision="0" HideTrigger="True" MinValue="1"
                    FieldLabel="Кол-во трекеров" Width="280" PaddingSpec="0 10 0 0" Text="1">
                </ext:NumberField>
                <ext:ComboBox runat="server" ID="cmbUser"
                    FieldLabel="Назначен"
                    Editable="false"
                    DisplayField="UserName"
                    ValueField="UserName"
                    TypeAhead="true"
                    QueryMode="Local"
                    ForceSelection="true"
                    TriggerAction="All"
                    EmptyText="Выберите администратора..."
                    SelectOnFocus="true">
                    <Store>
                        <ext:Store ID="StoreAdminUsers" runat="server">
                            <Model>
                                <ext:Model ID="Model2" runat="server">
                                    <Fields>
                                        <ext:ModelField Name="UserName" />
                                        <ext:ModelField Name="Name" />
                                    </Fields>
                                </ext:Model>
                            </Model>
                        </ext:Store>
                    </Store>
                    <ListConfig>
                        <ItemTpl ID="ItemTpl1" runat="server">
                            <Html>
                                <div class="list-item">
							        <h3>{UserName}</h3>
							        {Name}
						        </div>
                            </Html>
                        </ItemTpl>
                    </ListConfig>
                    <Listeners>
                        <Change Handler="validateTrackerInfo();"></Change>
                    </Listeners>
                </ext:ComboBox>
                <ext:Checkbox runat="server" ID="chkOldTracker" FieldLabel="Старый трекер"></ext:Checkbox>
            </Items>
            <Buttons>
                <ext:Button runat="server" ID="btnValidate" Text="Сохранить" Icon="Accept" Disabled="True">
                    <DirectEvents>
                        <Click OnEvent="btnValidate_click">
                            <EventMask ShowMask="True" Msg="Валидация терекеров..."></EventMask>
                        </Click>
                    </DirectEvents>

                </ext:Button>
                <ext:Button runat="server" ID="btnSaveNewTrackerInfo" Text="Сохранить invisible" Icon="Accept" Disabled="True" Hidden="True" HideMode="Offsets">
                    <DirectEvents>
                        <Click OnEvent="btnSaveNewTrackerInfo_click">
                            <EventMask ShowMask="True" Msg="Сохранение терекеров..."></EventMask>
                        </Click>
                    </DirectEvents>
                </ext:Button>
            </Buttons>
        </ext:Panel>
    </Items>
    <Listeners>
        <Hide Handler="clearTrackeInfoData();"></Hide>
    </Listeners>
</ext:Window>
