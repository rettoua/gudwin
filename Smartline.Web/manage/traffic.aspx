<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="traffic.aspx.cs" Inherits="Smartline.Web.manage.traffic" EnableViewState="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style>
        .x-grid3-hd-inner
        {
            white-space: normal;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <ext:ResourceManager ID="ResourceManager1" runat="server" Theme="Gray" Locale="RU-ru" DirectMethodNamespace="DR" />
        <ext:Viewport runat="server" ID="viewport" Layout="BorderLayout">
            <Items>
                <ext:Panel ID="Panel1" runat="server" Region="West" Split="True" MinWidth="300" Width="300" Layout="Fit">
                    <Items>
                        <ext:GridPanel runat="server"
                            ID="gridAllTrackers"
                            AutoScroll="True"
                            Title="Все трекеры">
                            <TopBar>
                                <ext:Toolbar ID="Toolbar2" runat="server">
                                    <Items>
                                        <ext:Button runat="server" ID="btnAddToWatch" Text="Добавить к просмотру" Icon="ChartCurveAdd" Disabled="False">
                                            <Listeners>
                                                <Click Handler="TRAFFIC.addToWatch();"></Click>
                                            </Listeners>
                                        </ext:Button>
                                        <ext:Button runat="server" ID="btnFilterByStatus" Text="Только онлайн" Icon="StatusOnline" EnableToggle="true">
                                            <Listeners>
                                                <Click Handler="TRAFFIC.applyFilterByStatus();"></Click>
                                            </Listeners>
                                        </ext:Button>
                                    </Items>
                                </ext:Toolbar>
                            </TopBar>
                            <Store>
                                <ext:Store runat="server" ID="store1">
                                    <Model>
                                        <ext:Model ID="Model1" runat="server" IDProperty="Id">
                                            <Fields>
                                                <ext:ModelField Name="Id"></ext:ModelField>
                                                <ext:ModelField Name="Name"></ext:ModelField>
                                                <ext:ModelField Name="Description"></ext:ModelField>
                                                <ext:ModelField Name="TrackerId"></ext:ModelField>
                                                <ext:ModelField Name="IsOnline"></ext:ModelField>
                                                <ext:ModelField Name="User"></ext:ModelField>
                                            </Fields>
                                        </ext:Model>
                                    </Model>
                                </ext:Store>
                            </Store>
                            <ColumnModel>
                                <Columns>
                                    <ext:RowNumbererColumn ID="RowNumbererColumn1" runat="server" />
                                    <ext:Column ID="Column9" runat="server" Text="Пользователь" DataIndex="User">                                        
                                    </ext:Column>
                                    <ext:Column ID="Column5" runat="server" Text="Название" DataIndex="Name">
                                        <Renderer Fn="TRAFFIC.onlineTrackerRenderer"></Renderer>
                                    </ext:Column>
                                    <ext:Column ID="Column6" runat="server" Text="Описание" DataIndex="Description">
                                        <Renderer Fn="TRAFFIC.onlineTrackerRenderer"></Renderer>
                                    </ext:Column>
                                    <ext:Column ID="CheckColumn1" runat="server" Text="Трекер ID" DataIndex="TrackerId">
                                        <Renderer Fn="TRAFFIC.onlineTrackerRenderer"></Renderer>
                                    </ext:Column>
                                </Columns>
                            </ColumnModel>
                            <SelectionModel>
                                <ext:RowSelectionModel ID="RowSelectionModel2" runat="server" Mode="Multi">
                                    <Listeners>
                                        <%--<SelectionChange Handler="App.btnAddToWatch.setDisabled(!this.hasSelection());"></SelectionChange>--%>
                                    </Listeners>
                                </ext:RowSelectionModel>
                            </SelectionModel>
                        </ext:GridPanel>
                    </Items>
                </ext:Panel>
                <ext:Panel ID="Panel2" runat="server" Region="Center" Split="True" MinWidth="100" Width="100" Layout="Fit">
                    <Items>
                        <ext:GridPanel runat="server"
                            ID="gridSelectedTrackers"
                            AutoScroll="True"
                            Title="Трекеры для просмотра статистики по трафику">
                            <TopBar>
                                <ext:Toolbar ID="Toolbar1" runat="server">
                                    <Items>
                                        <ext:Button ID="btnRemoveFromWatch" runat="server" Text="Убрать из просмотра" Icon="ChartCurveDelete" Disabled="False">
                                            <Listeners>
                                                <Click Handler="TRAFFIC.removeFromWatch();"></Click>
                                            </Listeners>
                                        </ext:Button>
                                        <ext:ToolbarSeparator ID="ToolbarSeparator1" runat="server" />
                                        <ext:DateField ID="fldFromDate" runat="server" FieldLabel="От" LabelWidth="25" LabelAlign="Right" Format="d.MM.Y" Width="125">
                                            <Listeners>
                                                <Change Handler="TRAFFIC.fromDateChanged();"></Change>
                                            </Listeners>
                                        </ext:DateField>
                                        <ext:DateField ID="fldToDate" runat="server" FieldLabel="До" LabelWidth="25"
                                            LabelAlign="Right" Format="d.MM.Y" Width="125">
                                            <Listeners>
                                                <Change Handler="TRAFFIC.toDateChanged();"></Change>
                                            </Listeners>
                                        </ext:DateField>
                                        <ext:Button ID="btnCalcTraffic" runat="server" Text="Подсчитать" Icon="Accept">
                                            <Listeners>
                                                <Click Handler="TRAFFIC.calc();"></Click>
                                            </Listeners>
                                            <%--<DirectEvents>
                                                <Click OnEvent="ReconfigureTrafficGrid" ></Click>
                                            </DirectEvents>--%>
                                        </ext:Button>
                                    </Items>
                                </ext:Toolbar>
                            </TopBar>
                            <Store>
                                <ext:Store runat="server" ID="store2">
                                    <Model>
                                        <ext:Model ID="Model2" runat="server" IDProperty="Id">
                                            <Fields>
                                                <ext:ModelField Name="Id"></ext:ModelField>
                                                <ext:ModelField Name="Name"></ext:ModelField>
                                                <ext:ModelField Name="Description"></ext:ModelField>
                                                <ext:ModelField Name="TrackerId"></ext:ModelField>
                                                <ext:ModelField Name="IsOnline"></ext:ModelField>
                                                <ext:ModelField Name="User"></ext:ModelField>
                                                <ext:ModelField Name="In" Type="Int"></ext:ModelField>
                                                <ext:ModelField Name="Out" Type="Int"></ext:ModelField>
                                                <ext:ModelField Name="Packages" Type="Object"></ext:ModelField>
                                            </Fields>
                                        </ext:Model>
                                    </Model>
                                </ext:Store>
                            </Store>
                            <ColumnModel>
                                <Columns>
                                    <ext:RowNumbererColumn runat="server" />
                                    <ext:Column ID="Column1" runat="server" Text="Название" DataIndex="Name" Locked="False">
                                        <Renderer Fn="TRAFFIC.onlineTrackerRenderer"></Renderer>
                                    </ext:Column>
                                    <ext:Column ID="Column3" runat="server" Text="Трекер ID" DataIndex="TrackerId" Locked="False">
                                        <Renderer Fn="TRAFFIC.onlineTrackerRenderer"></Renderer>
                                    </ext:Column>
                                    <ext:Column ID="Column2" runat="server" Text="Всего" Locked="False">
                                        <Columns>
                                            <ext:Column ID="Column4" runat="server" Text="Входящий" DataIndex="In" Width="65" Sortable="True">
                                                <Renderer Fn="TRAFFIC.bytesRenderer"></Renderer>
                                            </ext:Column>
                                            <ext:Column ID="Column7" runat="server" Text="Исходящий" DataIndex="Out" Width="70" Sortable="True">
                                                <Renderer Fn="TRAFFIC.bytesRenderer"></Renderer>
                                            </ext:Column>
                                            <ext:Column ID="Column8" runat="server" Text="Пакетов" DataIndex="Packages" Width="65">
                                                <Renderer Fn="TRAFFIC.packagesRenderer"></Renderer>
                                            </ext:Column>
                                        </Columns>
                                    </ext:Column>
                                </Columns>
                            </ColumnModel>
                            <SelectionModel>
                                <ext:RowSelectionModel ID="RowSelectionModel1" runat="server" Mode="Multi">
                                    <Listeners>
                                        <%--<SelectionChange Handler="App.btnRemoveFromWatch.setDisabled(!this.hasSelection());"></SelectionChange>--%>
                                    </Listeners>
                                </ext:RowSelectionModel>
                            </SelectionModel>
                        </ext:GridPanel>
                    </Items>
                </ext:Panel>
            </Items>
        </ext:Viewport>
    </form>
</body>
</html>
