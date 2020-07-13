<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="map.aspx.cs" Inherits="Smartline.Web.map" %>
<%@ Import Namespace="System.Web.Optimization" %>

<%@ Register Assembly="Ext.Net" Namespace="Ext.Net" TagPrefix="ext" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" href="carimagecss.ashx" type="text/css" />
    <script src="http://maps.googleapis.com/maps/api/js?sensor=false&language=ru" type="text/javascript"></script>    
    <%--<script src="http://api-maps.yandex.ru/2.0/?load=package.standard&mode=debug&lang=ru-RU" type="text/javascript"></script>--%>
    
    <style>
        .status {
            color: #555;
        }

        .x-progress.left-align .x-progress-text {
            text-align: left;
        }

        .x-progress.custom {
            height: 19px;
            border: 1px solid #686868;
            padding: 0 2px;
        }

        .ext-strict .x-progress.custom {
            height: 17px;
        }

        .custom .x-progress-bar {
            height: 17px;
            border: none;
            background: transparent url(custom-bar.gif) repeat-x !important;
            border-top: 1px solid #BEBEBE;
        }

        .ext-strict .custom .x-progress-bar {
            height: 15px;
        }

        .icon-recordgrey {
            background-image: url(Resources/sensor_grey.png);
        }

        .icon-pan {
            background-image: url(Resources/pan.png);
        }

        .icon-road {
            background-image: url(Resources/road.png);
        }

        .leaflet-container .leaflet-control-zoom {
            /*margin-left: 13px;*/
            margin-top: 12px;
            margin-left: 347px !important;
        }

        .sensor-label {
            color: grey;
            font-style: italic;
            font-size: 10px;
            font-family: cursive;
        }

        .lines:hover {
            cursor: pointer;
            stroke: #0099FF;
            stroke-width: 5;
        }

        .leaflet-container {
            cursor: auto !important;
        }

        div.leaflet-popup-content td.speed {
            font-weight: bold;
            font-family: sans-serif;
            color: grey;
            font-size: 10px;
            text-align: center;
        }

            div.leaflet-popup-content td.speed span {
                font-size: 14px;
                font-weight: bold;
                -ms-text-shadow: -1px -1px 0 #a0a0a0, 1px -1px 0 #a0a0a0, -1px 1px 0 #a0a0a0, 1px 1px 0 #a0a0a0;
                text-shadow: -1px -1px 0 #a0a0a0, 1px -1px 0 #a0a0a0, -1px 1px 0 #a0a0a0, 1px 1px 0 #a0a0a0;
            }

        div.leaflet-popup-content td.time {
            font-weight: bold;
            font-family: sans-serif;
            color: grey;
            font-size: 11px;
            text-align: center;
        }
    </style>
    <%: Styles.Render("~/bundles/GlobalStyle") %>
</head>
<body>
    <form id="form1" runat="server">
        <ext:ResourceManager ID="ResourceManager1" runat="server" Theme="Gray" DirectMethodNamespace="DR" Locale="RU-ru" />
        <%: Scripts.Render("~/bundles/Global") %>
        <ext:Viewport ID="Viewport1" runat="server" Layout="BorderLayout">
            <Items>
                <ext:Panel ID="TabPanel1" runat="server" Region="Center" Header="False" AnchorHorizontal="100%">
                </ext:Panel>

                <ext:Panel ID="pnlSensorHistory"
                    runat="server"
                    Title="История по датчикам"
                    Region="South"
                    HideMode="Offsets"
                    Hidden="True"
                    Closable="True"
                    CloseAction="Hide"
                    Layout="HBoxLayout"
                    Height="200">
                    <Items>
                        <ext:FieldSet runat="server"
                            Title="Фильтр">
                            <Items>
                                <ext:FieldContainer runat="server" Layout="HBoxLayout">
                                    <Items>
                                        <ext:ComboBox ID="cmbCarsSensorHistory" runat="server" FieldLabel="Авто"
                                            DisplayField="Name" ValueField="Id" Editable="True" AllowBlank="False"
                                            TypeAhead="true"
                                            QueryMode="Local"
                                            LabelWidth="35"
                                            ForceSelection="true">
                                            <Listeners>
                                                <BeforeSelect></BeforeSelect>
                                                <Change Handler="TM.trackChanged();"></Change>
                                                <BeforeQuery Handler="var q = queryEvent.query;
                                      queryEvent.query = new RegExp(q);
                                      queryEvent.query.length = q.length;" />
                                            </Listeners>
                                        </ext:ComboBox>
                                    </Items>
                                </ext:FieldContainer>
                                <ext:RadioGroup runat="server" ColumnsNumber="2" PaddingSpec="0 0 0 40">
                                    <Items>
                                        <ext:Radio runat="server" BoxLabel="Датчик 1" Checked="True" />
                                        <ext:Radio runat="server" BoxLabel="Датчик 2" />
                                    </Items>
                                </ext:RadioGroup>
                                <ext:FieldContainer runat="server" Layout="HBoxLayout">
                                    <Items>
                                        <ext:DateField ID="fldSensorHistoryDateFrom" runat="server" HideTrigger="True" BorderSpec="0 0 0 0"
                                            FieldLabel="C" AllowBlank="False" LabelWidth="35" Width="138">
                                            <Listeners>
                                                <Change Handler="$App.RepositoryManager.View.FromDateChanged();"></Change>
                                                <AfterRender Handler="triggerRender(this);"></AfterRender>
                                            </Listeners>
                                        </ext:DateField>
                                        <ext:TimeField ID="fldSensorHistoryTimeFrom" HideTrigger="True" Increment="60" runat="server" SelectedTime="0:00"
                                            Width="50" AllowBlank="False">
                                            <Listeners>
                                                <AfterRender Handler="triggerRender(this);"></AfterRender>
                                            </Listeners>
                                        </ext:TimeField>
                                    </Items>
                                </ext:FieldContainer>
                                <ext:FieldContainer runat="server" Layout="HBoxLayout">
                                    <Items>
                                        <ext:DateField ID="fldSensorHistoryDateTo" runat="server" HideTrigger="True" FieldLabel="По"
                                            LabelWidth="35" Width="138" AllowBlank="False">
                                            <Listeners>
                                                <AfterRender Handler="triggerRender(this);"></AfterRender>
                                                <Change Handler="$App.RepositoryManager.View.ToDateChanged();"></Change>
                                            </Listeners>
                                        </ext:DateField>
                                        <ext:TimeField ID="fldSensorHistoryTimeTo" runat="server" Width="50" HideTrigger="True" Increment="60"
                                            SelectedTime="23:59" AllowBlank="False">
                                            <Listeners>
                                                <AfterRender Handler="triggerRender(this);"></AfterRender>
                                            </Listeners>
                                        </ext:TimeField>
                                    </Items>
                                </ext:FieldContainer>
                                <ext:Button runat="server" Text="ОК" MarginSpec="0 0 0 5" Icon="Accept">
                                    <Listeners>
                                        <Click Handler="TM.load();">
                                        </Click>
                                    </Listeners>
                                </ext:Button>
                                <ext:Button runat="server" MarginSpec="0 0 0 5" Icon="Decline">
                                    <Listeners>
                                        <Click Handler="TM.load();">
                                        </Click>
                                    </Listeners>
                                </ext:Button>
                            </Items>
                        </ext:FieldSet>
                    </Items>
                </ext:Panel>
            </Items>
        </ext:Viewport>
        <ext:Window ID="pnlCars" runat="server" Title="Авто"
            X="0"
            Y="0"
            Width="350"
            Height="150"
            Collapsible="true" Layout="Anchor"
            Closable="False">
            <TopBar>
                <ext:Toolbar ID="Toolbar1" runat="server">
                    <Items>
                        <ext:Button runat="server" ID="chkShowTrack" ToolTip="Рисовать трек"
                            IconCls="icon-road" EnableToggle="true">
                            <Listeners>
                                <Click Handler="$App.SettingsManager.ChangeTrackingVisibility(this);"></Click>
                            </Listeners>
                        </ext:Button>
                        <ext:Button runat="server" IconCls="icon-pan" ToolTip="Центрировать карту по активным машинам"
                            ID="btnPan"
                            Visible="True"
                            EnableToggle="True">
                            <Listeners>
                                <Click Handler="$App.SettingsManager.ChangePan(this);"></Click>
                            </Listeners>
                        </ext:Button>
                        <ext:ToolbarFill ID="ToolbarFill1" runat="server" />
                        <ext:Button runat="server" Icon="ForwardGreen" ToolTip="Следить за всеми активными машинами"
                            ID="Button2"
                            Visible="True">
                            <Listeners>
                                <Click Handler="$App.RepositoryManager.StartAllTracking();"></Click>
                            </Listeners>
                        </ext:Button>
                        <ext:Button runat="server" Icon="StopGreen" ToolTip="Выключить слежение за всеми активными машинами"
                            ID="Button4"
                            Visible="True">
                            <Listeners>
                                <Click Handler="$App.RepositoryManager.StopAllTracking();"></Click>
                            </Listeners>
                        </ext:Button>
                        <ext:CycleButton ID="btnFilterCarByState" runat="server" PrependText="Авто: " ShowText="true">
                            <Menu>
                                <ext:Menu ID="Menu1" runat="server">
                                    <Items>
                                        <ext:CheckMenuItem ID="cmiAll" runat="server" Text="Все" Icon="Car" Checked="True" />
                                        <ext:MenuSeparator ID="MenuSeparator1" runat="server" />
                                        <ext:CheckMenuItem ID="cmiActive" runat="server" Text="Активные" Icon="CarAdd" />
                                        <ext:CheckMenuItem ID="cmiRun" runat="server" Text="Едут" Icon="CarStart" />
                                        <ext:CheckMenuItem ID="cmiStop" runat="server" Text="Стоят" Icon="CarStop" />
                                        <ext:MenuSeparator ID="MenuSeparator2" runat="server" />
                                        <ext:CheckMenuItem ID="cmiInactive" runat="server" Text="Не активные" Icon="CarDelete" />
                                    </Items>
                                </ext:Menu>
                            </Menu>
                            <Listeners>
                                <Change Handler="$App.RepositoryManager.ApplyCarsFilter();"></Change>
                            </Listeners>
                        </ext:CycleButton>
                        <ext:Button runat="server" Icon="Magnifier" ToolTip="Отображать/скрыть фильтр"
                            EnableToggle="true" ID="Button3">
                            <Listeners>
                                <Click Handler="App.pnlFilter.setVisible(this.pressed===true);"></Click>
                            </Listeners>
                        </ext:Button>
                    </Items>
                </ext:Toolbar>
            </TopBar>
            <Items>
                <ext:Panel ID="pnlFilter" runat="server" Layout="Anchor" Unstyled="True" Height="20" Hidden="True">
                    <Items>
                        <ext:TextField ID="txtFilterValue" runat="server" AnchorHorizontal="100%" EmptyText="Введите текст для фильтрации"
                            BorderSpec="0 0 0 0">
                            <Listeners>
                                <Change Handler="$App.RepositoryManager.ApplyCarsFilter();"></Change>
                            </Listeners>
                        </ext:TextField>
                    </Items>
                </ext:Panel>
                <ext:GridPanel ID="gridPanelCars" runat="server" HideHeaders="True"
                    InvalidateScrollerOnRefresh="False"
                    AnchorHorizontal="100%"
                    AnchorVertical="100%">
                    <Store>
                        <ext:Store ID="Store2" runat="server">
                            <Model>
                                <ext:Model ID="Model1" runat="server" IDProperty="Id">
                                    <Fields>
                                        <ext:ModelField Name="Id" />
                                        <ext:ModelField Name="Name" Type="String" />
                                        <ext:ModelField Name="Speed" Type="Float" />
                                        <ext:ModelField Name="LastSendTime" Type="Date" />
                                        <ext:ModelField Name="EndSendTime" Type="Date" />
                                        <ext:ModelField Name="IsTracked" />
                                        <ext:ModelField Name="Latitude" />
                                        <ext:ModelField Name="Longitude" />
                                        <ext:ModelField Name="Init" Type="Boolean" />
                                        <ext:ModelField Name="Relay" Type="Object" />
                                        <ext:ModelField Name="Relay1" Type="Object" />
                                        <ext:ModelField Name="Relay2" Type="Object" />
                                        <ext:ModelField Name="Sensor1" Type="Object" />
                                        <ext:ModelField Name="Sensor2" Type="Object" />
                                        <ext:ModelField Name="TrackerId" Type="Int" />
                                        <ext:ModelField Name="Color" Type="String" />
                                        <ext:ModelField Name="Battery" Type="Int" />
                                        <ext:ModelField Name="s" Type="Object" />
                                        <ext:ModelField Name="Image" Type="Object" />
                                    </Fields>
                                </ext:Model>
                            </Model>
                            <Sorters>
                                <ext:DataSorter Property="Name" Direction="ASC" />
                            </Sorters>
                            <Listeners>
                                <Load Handler="init_repo_manager(App.gridPanelCars);">
                                </Load>
                            </Listeners>
                        </ext:Store>
                    </Store>
                    <ColumnModel ID="ColumnModel1" runat="server" ForceFit="True" Selectable="False" AnchorHorizontal="100%">
                        <Columns>
                            <ext:ImageCommandColumn ID="Column4" runat="server" Width="25" Align="Right">
                                <Commands>
                                    <ext:ImageCommand Icon="CogEdit" Style="margin-top: -5px;" CommandName="settings">
                                        <ToolTip Text="Настройки трекера" />
                                    </ext:ImageCommand>
                                </Commands>
                                <Listeners>
                                    <Command Handler="if($App){$App.SettingsManager.CarSettings.show(record);}"></Command>
                                </Listeners>
                            </ext:ImageCommandColumn>
                            <ext:ImageCommandColumn ID="ImageCommandColumn1" runat="server" Width="30" Align="Right">
                                <Commands>
                                    <ext:ImageCommand CommandName="Battery" Icon="CarStart" HideMode="Visibility">
                                        <ToolTip Text="Питание от батареи" />
                                    </ext:ImageCommand>
                                </Commands>
                                <PrepareCommand Handler="if($App){ $App.RepositoryManager.View.RenderToolbar.apply($App.RepositoryManager.View,arguments);}">
                                </PrepareCommand>
                            </ext:ImageCommandColumn>
                            <ext:Column ID="Column1" runat="server" Text="Название" DataIndex="Name" Flex="1">
                                <Renderer Handler="if($App){return $App.RepositoryManager.View.RenderName.apply($App.RepositoryManager.View,arguments);}">
                                </Renderer>
                            </ext:Column>
                            <ext:Column ID="Column2" runat="server" Text="Описание" DataIndex="Speed" Width="140">
                                <Renderer Handler="if($App){return $App.RepositoryManager.View.RenderSpeed.apply($App.RepositoryManager.View,arguments);}">
                                </Renderer>
                            </ext:Column>
                            <ext:ImageCommandColumn ID="CommandColumn1" runat="server" Width="20" Align="Right">
                                <Commands>
                                    <ext:ImageCommand Icon="CarStart" CommandName="CarStart">
                                        <ToolTip Text="Начать трекинг" />
                                    </ext:ImageCommand>
                                    <ext:ImageCommand Icon="CarStop" CommandName="CarStop">
                                        <ToolTip Text="Остановить трекинг" />
                                    </ext:ImageCommand>
                                </Commands>
                                <Listeners>
                                    <Command Handler="if($App){ $App.RepositoryManager.View.ExecuteCommand.apply($App.RepositoryManager.View,arguments);}" />
                                </Listeners>
                                <PrepareCommand Handler="if($App){ $App.RepositoryManager.View.RenderToolbar.apply($App.RepositoryManager.View,arguments);}">
                                </PrepareCommand>
                            </ext:ImageCommandColumn>
                        </Columns>
                    </ColumnModel>
                    <View>
                        <ext:GridView ID="GridView1" runat="server" MarkDirty="False" PreserveScrollOnRefresh="True">
                            <GetRowClass Handler="if($App){return $App.RepositoryManager.View.VisualRowClass.apply($App.RepositoryManager.View,arguments);}" />
                        </ext:GridView>
                    </View>
                    <SelectionModel>
                        <ext:RowSelectionModel ID="RowSelectionModel1" runat="server" Mode="Single">
                            <Listeners>
                            </Listeners>
                        </ext:RowSelectionModel>
                    </SelectionModel>
                    <Listeners>
                        <ItemClick Handler="if($App){ $App.RepositoryManager.Centralize.apply($App.RepositoryManager,arguments);}"></ItemClick>
                        <ItemDblClick Handler="if($App){$App.SettingsManager.CarSettings.show(record);}"></ItemDblClick>
                    </Listeners>
                </ext:GridPanel>
            </Items>
        </ext:Window>
        <ext:Window ID="windowTrack" runat="server" Title="История движения" Width="585"
            Constrain="true" Hidden="True" Resizable="False" CloseAction="Hide" >
            <Items>
                <ext:Panel ID="PanelTracking" runat="server" Border="false" Padding="5" StyleSpec="background-color:white;">
                    <Items>
                        <ext:FieldContainer ID="filterContainer" runat="server" Layout="HBoxLayout" Width="575">
                            <Items>
                                <ext:ComboBox ID="carsForTracking" runat="server" FieldLabel="Авто" LabelWidth="35"
                                    DisplayField="Name" ValueField="Id" Editable="True" AllowBlank="False"
                                    TypeAhead="true"
                                    QueryMode="Local"
                                    ForceSelection="true">
                                    <Listeners>
                                        <Change Handler="TM.trackChanged();"></Change>
                                        <BeforeQuery Handler="var q = queryEvent.query;
                                      queryEvent.query = new RegExp(q);
                                      queryEvent.query.length = q.length;" />
                                    </Listeners>
                                </ext:ComboBox>
                                <ext:DateField ID="dfFrom" runat="server" Width="100" HideTrigger="True" BorderSpec="0 0 0 0"
                                    FieldLabel="C" LabelWidth="20" PaddingSpec="0 0 0 5" AllowBlank="False">
                                    <Listeners>
                                        <Change Handler="$App.RepositoryManager.View.FromDateChanged();"></Change>
                                        <AfterRender Handler="triggerRender(this);"></AfterRender>
                                    </Listeners>
                                </ext:DateField>
                                <ext:TimeField HideTrigger="True" ID="tfFrom" Increment="60" runat="server" SelectedTime="0:00"
                                    Width="50" AllowBlank="False">
                                    <Listeners>
                                        <AfterRender Handler="triggerRender(this);"></AfterRender>
                                    </Listeners>
                                </ext:TimeField>
                                <ext:DateField ID="dfTo" runat="server" Width="100" HideTrigger="True" FieldLabel="По"
                                    LabelWidth="20" PaddingSpec="0 0 0 5" AllowBlank="False">
                                    <Listeners>
                                        <AfterRender Handler="triggerRender(this);"></AfterRender>
                                        <Change Handler="$App.RepositoryManager.View.ToDateChanged();"></Change>
                                    </Listeners>
                                </ext:DateField>
                                <ext:TimeField runat="server" ID="tfTo" Width="50" HideTrigger="True" Increment="60"
                                    SelectedTime="23:59" AllowBlank="False">
                                    <Listeners>
                                        <AfterRender Handler="triggerRender(this);"></AfterRender>
                                    </Listeners>
                                </ext:TimeField>
                                <ext:Button runat="server" Text="ОК" MarginSpec="0 0 0 5">
                                    <Listeners>
                                        <Click Handler="TM.load();">
                                        </Click>
                                    </Listeners>
                                </ext:Button>
                                <ext:Button ID="Button1" runat="server" Icon="Decline" MarginSpec="0 0 0 5">
                                    <Listeners>
                                        <Click Handler="TM.clear();">
                                        </Click>
                                    </Listeners>
                                </ext:Button>
                            </Items>
                        </ext:FieldContainer>
                        <ext:Panel ID="pnlTrackSummary" runat="server" Unstyled="True" AutoDoLayout="True" Height="20">
                        </ext:Panel>
                        <ext:Panel ID="pnlTrackTemplate" runat="server" Unstyled="True" AutoScroll="True" AutoDoLayout="True" Height="220">
                            <Listeners>
                            </Listeners>
                        </ext:Panel>
                        <ext:Panel runat="server" ID="pnlTrackSettings" Unstyled="True" PaddingSpec="5 0 0 0 ">
                            <Items>
                                <ext:FieldContainer ID="FieldContainer1" runat="server" Layout="HBoxLayout" Width="500">
                                    <Items>
                                        <ext:Button ID="btnTrackerSetting" runat="server" IconCls="tracker-setting-icon"
                                            Pressed="True" Height="32" Text="Маркеры" Width="95" EnableToggle="True">
                                            <Listeners>
                                                <Click Handler="TM.toggleParking();"></Click>
                                            </Listeners>
                                        </ext:Button>
                                        <ext:Button ID="btnShowPoints" runat="server" IconCls="tracker-point-icon inactive"
                                            Pressed="False" Height="32" StyleSpec="margin-left:7px;" Text="Точки">
                                            <Listeners>
                                                <Click Handler="this.toggle(); TM.togglePoints(); "></Click>
                                            </Listeners>
                                        </ext:Button>
                                        <ext:SplitButton ID="btnShowSpeed" runat="server" IconCls="tracker-speed-icon inactive"
                                            Pressed="False" Height="32" StyleSpec="margin-left:7px;" Text="Скорость" EnableToggle="True">
                                            <Menu>
                                                <ext:Menu runat="server">
                                                    <Items>
                                                        <ext:MenuItem runat="server" Text="Редактировать скоростной режим">
                                                            <Listeners>
                                                                <Click Handler="if($App){$App.SettingsManager.EditSpeedLiminits();}"></Click>
                                                            </Listeners>
                                                        </ext:MenuItem>
                                                    </Items>
                                                </ext:Menu>
                                            </Menu>
                                            <Listeners>
                                                <Click Handler="TM.toggleSpeed(); "></Click>
                                            </Listeners>
                                        </ext:SplitButton>
                                        <ext:Slider
                                            ID="sliderParkingLength"
                                            runat="server"
                                            Single="true"
                                            Width="110"
                                            MinValue="1"
                                            Number="5"
                                            PaddingSpec="0 0 0 10"
                                            MaxValue="30"
                                            Shadow="True"
                                            Note="остановка: 5 мин."
                                            NoteAlign="Down">
                                            <Listeners>
                                                <Change Handler="if(TM){ TM.changeParkingLength(item, newValue);}"></Change>
                                                <ChangeComplete Handler="if(TM){ TM.changeCompleteParkingLength(item, newValue);}" />
                                            </Listeners>
                                            <Plugins>
                                                <ext:SliderTip ID="SliderTip1" runat="server">
                                                    <GetText Fn="function (slider) { return Ext.String.format('<b>{0} мин.</b>', slider.value); }" />
                                                </ext:SliderTip>
                                            </Plugins>
                                        </ext:Slider>
                                        <ext:ToolbarFill runat="server" />
                                    </Items>
                                </ext:FieldContainer>
                            </Items>
                        </ext:Panel>
                    </Items>
                </ext:Panel>
            </Items>
            <Listeners>
            </Listeners>
        </ext:Window>
        <ext:XTemplate ID="XTemplateHistory" runat="server">
            <Html>
                <tpl for=".">
                    <div class="track" onmouseover="TM.trackHover({id});" onmouseout="TM.trackLeave({id});" onclick="TM.trackClick({id});">
					    <p>
						   <span class="track-time">[{movingStartReadable}]</span> Расстояние: <b>{distanceReadable} км.</b>&nbsp;&nbsp; Средняя скорость: <b>{avgSpeed} км/ч</b> &nbsp;&nbsp;Макс. скорость: <b>{maxSpeed} км/ч</b>	
					    </p>
                        <p id="parking_data{id}" class="track">
                           <span class="track-time">[{startReadable}]</span> Остановка: {time} {geo}
                        </p>
					</div>                           
				</tpl>
            </Html>
        </ext:XTemplate>
        <ext:XTemplate ID="XTemplateSummary" runat="server" Height="40">
            <Html>
                <div class="track-summary"><b>ВСЕГО:</b> Пробег: <b>{distanceReadable} км.</b>&nbsp;&nbsp; Средняя скр.: <b>{avgSpeed} км/ч</b> &nbsp;&nbsp;Макс. скр: <b>{maxSpeed} км/ч</b> &nbsp;&nbsp;Простой: <b>{time}</b></div>
            </Html>
        </ext:XTemplate>

        <ext:Window ID="wndEditSpeedLimits" runat="server" Title="Скоростной режим" Width="300" Height="170" BodyStyle="background:white;"
            Constrain="true" Hidden="True" Resizable="False" CloseAction="Hide" Icon="Color" Layout="Fit" Modal="True">
            <Items>
                <ext:Panel ID="Panel8" runat="server" Border="false" Padding="10">
                    <Items>
                        <ext:FieldContainer ID="FieldContainer3" runat="server" Layout="HBoxLayout" Width="500">
                            <Items>
                                <ext:NumberField runat="server" ID="fldSpeedLimitsL1" FieldLabel="До" Width="120" LabelWidth="20" MinValue="1">
                                    <Listeners>
                                        <Change Handler="if($App){ $App.SettingsManager.SpeedLimit1Changed();}"></Change>
                                    </Listeners>
                                </ext:NumberField>
                                <ext:Label runat="server" Text="км/ч" PaddingSpec="0 0 0 10"></ext:Label>
                                <ext:DropDownField ID="fldSpeedLimitL1c" runat="server" Width="90" PaddingSpec="0 0 0 10"
                                    Editable="false" MatchFieldWidth="false" FieldStyle="background-image:none;" AnchorHorizontal="100%">
                                    <Component>
                                        <ext:Panel ID="Panel9" runat="server">
                                            <Items>
                                                <ext:ColorPicker ID="ColorPicker2" runat="server">
                                                    <Listeners>
                                                        <Select Handler="item.ownerCt.dropDownField.setValue('#' + color);" />
                                                        <BeforeRender Handler="this.colors[0] = '34AADA';"></BeforeRender>
                                                    </Listeners>
                                                </ext:ColorPicker>
                                            </Items>
                                        </ext:Panel>
                                    </Component>
                                    <Listeners>
                                        <Change Handler="this.inputEl.dom.style.color = this.inputEl.dom.style.backgroundColor = newValue;"></Change>
                                    </Listeners>
                                </ext:DropDownField>
                            </Items>
                        </ext:FieldContainer>
                        <ext:FieldContainer ID="FieldContainer4" runat="server" Layout="HBoxLayout" Width="500">
                            <Items>
                                <ext:NumberField runat="server" ID="fldSpeedLimitsL2" FieldLabel="До" Width="120" LabelWidth="20" MinValue="2">
                                    <Listeners>
                                        <Change Handler="if($App){ $App.SettingsManager.SpeedLimit2Changed();}"></Change>
                                    </Listeners>
                                </ext:NumberField>
                                <ext:Label ID="Label5" runat="server" Text="км/ч" PaddingSpec="0 0 0 10"></ext:Label>
                                <ext:DropDownField ID="fldSpeedLimitL2c" runat="server" Width="90" PaddingSpec="0 0 0 10"
                                    Editable="false" MatchFieldWidth="false" FieldStyle="background-image:none;" AnchorHorizontal="100%">
                                    <Component>
                                        <ext:Panel ID="Panel10" runat="server">
                                            <Items>
                                                <ext:ColorPicker ID="ColorPicker3" runat="server">
                                                    <Listeners>
                                                        <Select Handler="item.ownerCt.dropDownField.setValue('#' + color);" />
                                                        <BeforeRender Handler="this.colors[0] = '34AADA';"></BeforeRender>
                                                    </Listeners>
                                                </ext:ColorPicker>
                                            </Items>
                                        </ext:Panel>
                                    </Component>
                                    <Listeners>
                                        <Change Handler="this.inputEl.dom.style.color = this.inputEl.dom.style.backgroundColor = newValue;"></Change>
                                    </Listeners>
                                </ext:DropDownField>
                            </Items>
                        </ext:FieldContainer>
                        <ext:FieldContainer ID="FieldContainer5" runat="server" Layout="HBoxLayout" Width="500">
                            <Items>
                                <ext:NumberField runat="server" ID="fldSpeedLimitsL3" FieldLabel="До" Width="120" LabelWidth="20" MinValue="3">
                                    <Listeners>
                                        <Change Handler="if($App){ $App.SettingsManager.SpeedLimit3Changed();}"></Change>
                                    </Listeners>
                                </ext:NumberField>
                                <ext:Label ID="Label6" runat="server" Text="км/ч" PaddingSpec="0 0 0 10"></ext:Label>
                                <ext:DropDownField ID="fldSpeedLimitL3c" runat="server" Width="90" PaddingSpec="0 0 0 10"
                                    Editable="false" MatchFieldWidth="false" FieldStyle="background-image:none;" AnchorHorizontal="100%">
                                    <Component>
                                        <ext:Panel ID="Panel11" runat="server">
                                            <Items>
                                                <ext:ColorPicker ID="ColorPicker4" runat="server">
                                                    <Listeners>
                                                        <Select Handler="item.ownerCt.dropDownField.setValue('#' + color);" />
                                                        <BeforeRender Handler="this.colors[0] = '34AADA';"></BeforeRender>
                                                    </Listeners>
                                                </ext:ColorPicker>
                                            </Items>
                                        </ext:Panel>
                                    </Component>
                                    <Listeners>
                                        <Change Handler="this.inputEl.dom.style.color = this.inputEl.dom.style.backgroundColor = newValue;"></Change>
                                    </Listeners>
                                </ext:DropDownField>
                            </Items>
                        </ext:FieldContainer>
                    </Items>
                    <Buttons>
                        <ext:Button runat="server" Text="Отмена" Icon="Cancel">
                            <Listeners>
                                <Click Handler="App.wndEditSpeedLimits.hide();"></Click>
                            </Listeners>
                        </ext:Button>
                        <ext:Button runat="server" ID="btnSaveSpeedLimit" Text="Применить" Icon="Accept">
                            <Listeners>
                                <Click Handler="if($App){ $App.SettingsManager.SaveSpeedLimites();}"></Click>
                            </Listeners>
                        </ext:Button>
                    </Buttons>
                </ext:Panel>
            </Items>
        </ext:Window>

        <ext:Window ID="wndRelay" runat="server" Title="Управление датчиками" Width="500" Height="250" BodyStyle="background:white;"
            Constrain="true" Hidden="True" Resizable="False" CloseAction="Hide" Icon="Controller" Layout="Fit"
            Modal="True">
            <Items>
                <ext:Panel ID="Panel1" runat="server" Border="false" Padding="5">
                    <Items>
                        <ext:Panel runat="server" StyleSpec="font-family: tahoma,arial,verdana,sans-serif;font-size:11px;padding-bottom:10px;" Border="False">
                            <Items>
                                <ext:Panel runat="server" Border="False">
                                    <Content>
                                        В списке отображаются подключеные трекеры с активными датчиками. "Включить" датчики можно во вкладке <b>Трекеры</b>
                                    </Content>
                                </ext:Panel>
                            </Items>
                        </ext:Panel>
                        <ext:GridPanel ID="gridPanelTrackerRelay" runat="server" Height="100" Border="False">
                            <Store>
                                <ext:Store ID="Store1" runat="server">
                                    <Model>
                                        <ext:Model ID="Model2" runat="server" IDProperty="Id">
                                            <Fields>
                                                <ext:ModelField Name="Id" />
                                                <ext:ModelField Name="Name" Type="String" />
                                                <ext:ModelField Name="Speed" Type="Float" />
                                                <ext:ModelField Name="LastSendTime" Type="Date" />
                                                <ext:ModelField Name="EndSendTime" Type="Date" />
                                                <ext:ModelField Name="IsTracked" />
                                                <ext:ModelField Name="Latitude" />
                                                <ext:ModelField Name="Longitude" />
                                                <ext:ModelField Name="Init" Type="Boolean" />
                                                <ext:ModelField Name="Relay" Type="Object" />
                                                <ext:ModelField Name="Relay1" Type="Object" />
                                                <ext:ModelField Name="Relay2" Type="Object" />
                                                <ext:ModelField Name="Sensor1" Type="Object" />
                                                <ext:ModelField Name="Sensor2" Type="Object" />
                                                <ext:ModelField Name="TrackerId" Type="Int" />
                                                <ext:ModelField Name="Color" Type="String" />
                                            </Fields>
                                        </ext:Model>
                                    </Model>
                                    <Sorters>
                                        <ext:DataSorter Property="Name" Direction="ASC" />
                                    </Sorters>
                                </ext:Store>
                            </Store>
                            <ColumnModel ID="ColumnModel2" runat="server" ForceFit="True">
                                <Columns>
                                    <ext:Column ID="Column3" runat="server" Text="Название" DataIndex="Name" Flex="1">
                                    </ext:Column>
                                    <ext:Column ID="ColumnRelay" runat="server" DataIndex="Relay" Text="Релле" RightCommandAlign="False">
                                        <Renderer Handler=" if(Relay){return Relay.relayRenderer.apply(Relay,arguments);}" />
                                        <Commands>
                                            <ext:ImageCommand CommandName="Chart" Icon="Decline" Text="Вкл">
                                            </ext:ImageCommand>
                                        </Commands>
                                        <Listeners>
                                            <Command Handler="if(Relay){return Relay.executeRelay.apply(Relay,arguments);}"></Command>
                                        </Listeners>
                                        <PrepareCommand Handler="if(RPI){return Relay.relayCommandPrepare.apply(Relay,arguments);}"></PrepareCommand>
                                    </ext:Column>
                                    <ext:Column ID="Column5" runat="server" DataIndex="Relay1" Text="Выход 1" RightCommandAlign="False">
                                        <Renderer Handler=" if(Relay){return Relay.relayRenderer.apply(Relay,arguments);}" />
                                        <Commands>
                                            <ext:ImageCommand CommandName="Chart" Icon="Accept" Text="Вкл">
                                            </ext:ImageCommand>
                                        </Commands>
                                        <Listeners>
                                            <Command Handler="if(Relay){return Relay.executeRelay.apply(Relay,arguments);}"></Command>
                                        </Listeners>
                                        <PrepareCommand Handler="if(RPI){return Relay.relayCommandPrepare.apply(Relay,arguments);}"></PrepareCommand>
                                    </ext:Column>
                                    <ext:Column ID="Column6" runat="server" DataIndex="Relay2" Text="Выход 2" RightCommandAlign="False">
                                        <Renderer Handler=" if(Relay){return Relay.relayRenderer.apply(Relay,arguments);}" />
                                        <Commands>
                                            <ext:ImageCommand CommandName="Chart" Icon="Accept" Text="Вкл">
                                            </ext:ImageCommand>
                                        </Commands>
                                        <Listeners>
                                            <Command Handler="if(Relay){return Relay.executeRelay.apply(Relay,arguments);}"></Command>
                                        </Listeners>
                                        <PrepareCommand Handler="if(RPI){return Relay.relayCommandPrepare.apply(Relay,arguments);}"></PrepareCommand>
                                    </ext:Column>
                                    <ext:Column ID="Column7" runat="server" DataIndex="Sensor1" Width="40" Text="Вход 1">
                                        <Renderer Handler="if(RPI){return Relay.sensorRenderer.apply(Relay,arguments);}" />
                                    </ext:Column>
                                    <ext:Column ID="Column8" runat="server" DataIndex="Sensor2" Width="40" Text="Тревожная кнопка">
                                        <Renderer Handler="if(RPI){return Relay.sensorRenderer.apply(Relay,arguments);}" />
                                    </ext:Column>
                                </Columns>
                            </ColumnModel>
                            <View>
                                <ext:GridView ID="GridView2" runat="server" MarkDirty="False">
                                </ext:GridView>
                            </View>
                            <Plugins>
                                <ext:CellEditing ID="CellEditing2" runat="server">
                                </ext:CellEditing>
                            </Plugins>
                            <SelectionModel>
                                <ext:RowSelectionModel ID="RowSelectionModel2" runat="server" Mode="Single">
                                </ext:RowSelectionModel>
                            </SelectionModel>
                        </ext:GridPanel>
                        <ext:Panel ID="pnlNoActiveTrackers" runat="server" StyleSpec="font-family: tahoma,arial,verdana,sans-serif;font-size:11px;color:#333;padding-bottom:10px;" Border="False">
                            <Items>
                                <ext:Panel runat="server" Border="False">
                                    <Content>
                                        <div style="font-family: tahoma,arial,verdana,sans-serif; font-size: 14px; color: #0048ea; text-align: center;">
                                            <img alt="" style="margin-top: 3px; padding-top: 3px;" src="Resources/Attention2.png" />
                                            Нет активных трекеров
                                        </div>
                                    </Content>
                                </ext:Panel>
                            </Items>
                        </ext:Panel>
                    </Items>
                </ext:Panel>
            </Items>
        </ext:Window>
        <ext:TaskManager ID="TaskManagerOnlineUpdater" runat="server">
            <Tasks>
                <ext:Task TaskID="taskUpdateRelay" AutoRun="True" Interval="2000">
                    <Listeners>
                        <Update Handler="Relay.loadData();"></Update>
                    </Listeners>
                </ext:Task>
            </Tasks>
        </ext:TaskManager>
        <ext:TaskManager ID="TaskManager1" runat="server">
            <Tasks>
                <ext:Task TaskID="taskUpdateRelay" AutoRun="True" Interval="900000">
                    <Listeners>
                        <Update Handler="
                            $.ajax({
                                    url: 'SessionRefresh.ashx',
                                    contentType: 'application/json; charset=utf-8'                                    
                                });                            
                            ">
                        </Update>
                    </Listeners>
                </ext:Task>
            </Tasks>
        </ext:TaskManager>
        <ext:TaskManager ID="TaskManager2" runat="server">
            <Tasks>
                <ext:Task TaskID="taskUpdateRelay1" AutoRun="True" Interval="60000">
                    <Listeners>
                        <Update Handler="App.gridPanelCars.getView().refresh();"></Update>
                    </Listeners>
                </ext:Task>
            </Tasks>
        </ext:TaskManager>
        <ext:Window ID="wndCarSettings" runat="server" Title="Карточка трекера"
            X="385"
            Y="5"
            Width="385"
            Height="450"
            Hidden="True"
            Collapsible="False"
            Layout="Fit"
            Closable="True"
            CloseAction="Hide">
            <Items>
                <ext:Panel ID="pnlCarSettings"
                    runat="server"
                    Border="false"
                    Padding="5"
                    Layout="Anchor"
                    ButtonAlign="Right"
                    StyleSpec="background-color:white;">
                    <Items>
                        <ext:Label runat="server" ID="lblTrackerId" Cls="car-settings.font" Html="<b>Идентификатор трекера:    </b>"></ext:Label>
                        <ext:TextField runat="server" ID="txtName" FieldLabel="Название" AnchorHorizontal="100%" PaddingSpec="10 0 0 0" />
                        <ext:TextField runat="server" ID="txtDescription" FieldLabel="Описание" AnchorHorizontal="100%" />
                        <ext:FieldContainer ID="FieldContainer2" runat="server" AnchorHorizontal="100%" Layout="HBoxLayout">
                            <Items>
                                <ext:Label runat="server" Text="Одометр:" Width="100"></ext:Label>
                                <ext:Label
                                    ID="lblOdometer"
                                    runat="server"
                                    Text="Загрузка данных..."
                                    StyleSpec="cursor: pointer;"
                                    Icon="ApplicationEdit">
                                    <Editor>
                                        <ext:Editor ID="Editor1" runat="server" Shadow="false" Alignment="tl-tl?">
                                            <Field>
                                                <ext:NumberField ID="fldOdometer"
                                                    runat="server"
                                                    Cls="x-form-field-editor"
                                                    Width="100" />
                                            </Field>
                                            <Listeners>
                                                <BeforeStartEdit Handler="if($App){ $App.SettingsManager.CarSettings.startEdit.apply($App.SettingsManager.CarSettings, arguments);}"></BeforeStartEdit>
                                                <Complete Handler="if($App){ $App.SettingsManager.CarSettings.cancelEdit(item, value, startValue);}"></Complete>
                                            </Listeners>
                                        </ext:Editor>
                                    </Editor>
                                    <Listeners>
                                    </Listeners>
                                </ext:Label>
                            </Items>
                        </ext:FieldContainer>
                        <ext:FieldContainer runat="server" AnchorHorizontal="100%"
                            Layout="HBoxLayout">
                            <Items>
                                <ext:NumberField runat="server" ID="fldConsumption" FieldLabel="Расход л/100км" MinValue="0" Width="150">
                                </ext:NumberField>
                                <ext:Checkbox runat="server" ID="chkHideInEvos" FieldLabel="Скрыть в Evos" PaddingSpec="0 0 0 15"></ext:Checkbox>
                            </Items>
                        </ext:FieldContainer>
                        <ext:DropDownField ID="fldColor" runat="server"
                            Editable="false" MatchFieldWidth="false" FieldLabel="Цвет трека" FieldStyle="background-image:none;" AnchorHorizontal="100%">
                            <Component>
                                <ext:Panel ID="Panel3" runat="server">
                                    <Items>
                                        <ext:ColorPicker ID="ColorPicker1" runat="server">
                                            <Listeners>
                                                <Select Handler="item.ownerCt.dropDownField.setValue('#' + color);" />
                                                <BeforeRender Handler="this.colors[0] = '34AADA';"></BeforeRender>
                                            </Listeners>
                                        </ext:ColorPicker>
                                    </Items>
                                </ext:Panel>
                            </Component>
                            <Listeners>
                                <Change Handler="this.inputEl.dom.style.color = this.inputEl.dom.style.backgroundColor = newValue;"></Change>
                            </Listeners>
                        </ext:DropDownField>
                        <ext:ComboBox
                            ID="CarImagesCombobox"
                            runat="server"
                            Width="250"
                            Editable="false"
                            DisplayField="title"
                            ValueField="name"
                            QueryMode="Local"
                            ForceSelection="true"
                            TriggerAction="All"
                            AnchorHorizontal="100%"
                            FieldLabel="Иконка">
                            <Store>
                                <ext:Store ID="CarImagesStore" runat="server">
                                    <Model>
                                        <ext:Model runat="server" IDProperty="name">
                                            <Fields>
                                                <ext:ModelField Name="name" Type="String" />
                                                <ext:ModelField Name="title" Type="String" />
                                                <ext:ModelField Name="isrecalc" Type="Boolean" />
                                            </Fields>
                                        </ext:Model>
                                    </Model>
                                </ext:Store>
                            </Store>
                            <ListConfig>
                                <ItemTpl runat="server">
                                    <Html>
                                        <div class="list-item">
                                            <img src="Resources/car_{name}.png" />							    							    
                                            <img src="Resources/car_{name}_stop.png" />							    							    
                                            <img src="Resources/car_{name}_sos.png" />							    							    
                                            <img src="Resources/car_{name}_sos_lite.png" />							    							    
						                </div>
                                    </Html>
                                </ItemTpl>
                            </ListConfig>
                            <Listeners>
                                <Change Handler="if(this.valueModels.length>0){this.setIconCls('car-icon-'+ this.valueModels[0].get('name'));}" />
                            </Listeners>
                        </ext:ComboBox>
                        <ext:Button runat="server" ID="btnVirtualTracker" Flat="True" Icon="ApplicationEdit">
                            <Listeners>
                                <Click Handler="$App.SettingsManager.CarSettings.editVirtualTracker();"></Click>
                            </Listeners>
                        </ext:Button>
                        <ext:Panel runat="server" Title="Датчики" PaddingSpec="10 0 0 0 " Height="160">
                            <Items>
                                <ext:Checkbox ID="chkSensor2" runat="server"
                                    FieldLabel="Использовать датчик <a href='#' onmouseenter='$App.SettingsManager.CarSettings.mouseEnterSensor2();' onmouseleave='$App.SettingsManager.CarSettings.mouseLeaveSensor2();'  class='alarm_sensor'>Вход 2</a> как <b>тревожную кнопку</b>"
                                    LabelWidth="320"
                                    LabelAlign="Right">
                                    <Listeners>
                                        <Change Handler="$App.SettingsManager.CarSettings.changeSensorAlarmCheckbox();"></Change>
                                    </Listeners>
                                </ext:Checkbox>
                                <ext:Panel runat="server" ID="pnlSensors" Layout="HBoxLayout" Padding="5" Unstyled="True" Height="100">
                                    <Items>
                                        <ext:Panel runat="server" Width="70" Height="90" PaddingSpec="0 5 0 5" Unstyled="True">
                                            <Items>
                                                <ext:TextField runat="server" ID="txtRelay" Width="60"></ext:TextField>
                                                <ext:Button ID="btnRelay" runat="server" Width="60" Height="40" Text="ВЫКЛ"
                                                    StyleSpec="background-color: greenyellow;background-image:none;font-weight: bold;">
                                                    <Listeners>
                                                        <Click Handler="if($App){ $App.SettingsManager.CarSettings.relayClick(this,'');}"></Click>
                                                    </Listeners>
                                                </ext:Button>
                                                <ext:Label runat="server" Text="Реле" StyleSpec="color:grey;text-align:center;font-style: italic;"></ext:Label>
                                            </Items>
                                        </ext:Panel>
                                        <ext:Panel ID="Panel4" runat="server" Width="70" Height="90" Unstyled="True" PaddingSpec="0 5 0 5">
                                            <Items>
                                                <ext:TextField ID="txtRelay1" runat="server" Width="60"></ext:TextField>
                                                <ext:Button ID="btnRelay1" runat="server" Width="60" Height="40" Text="ВКЛ"
                                                    StyleSpec="background-color: red;background-image:none;">
                                                    <Listeners>
                                                        <Click Handler="$App.SettingsManager.CarSettings.relayClick(this,1);"></Click>
                                                    </Listeners>
                                                </ext:Button>
                                                <ext:Label ID="Label1" runat="server" Text="Выход 1" StyleSpec="color:grey;text-align:center;font-style: italic;"></ext:Label>
                                            </Items>
                                        </ext:Panel>
                                        <ext:Panel ID="Panel5" runat="server" Width="70" Height="90" Unstyled="True" PaddingSpec="0 5 0 5">
                                            <Items>
                                                <ext:TextField ID="txtRelay2" runat="server" Width="60"></ext:TextField>
                                                <ext:Button ID="btnRelay2" runat="server" Width="60" Height="40" Text="ВКЛ"
                                                    StyleSpec="background-color: red;background-image:none;">
                                                    <Listeners>
                                                        <Click Handler="if($App){ $App.SettingsManager.CarSettings.relayClick(this,2);}"></Click>
                                                    </Listeners>
                                                </ext:Button>
                                                <ext:Label ID="Label2" runat="server" Text="Выход 2" StyleSpec="color:grey;text-align:center;font-style: italic;"></ext:Label>
                                            </Items>
                                        </ext:Panel>
                                        <ext:Panel ID="Panel6" runat="server" Width="70" Height="90" Unstyled="True" PaddingSpec="0 5 0 5">
                                            <Items>
                                                <ext:TextField ID="txtSensor1" runat="server" Width="60"></ext:TextField>
                                                <ext:Button ID="btnSensor1" runat="server" Width="60" Height="40"
                                                    StyleSpec="background-color: greenyellow;background-image:none;">
                                                </ext:Button>
                                                <ext:Label ID="Label3" runat="server" Text="Вход 1" StyleSpec="color:grey;text-align:center;font-style: italic;"></ext:Label>
                                            </Items>
                                        </ext:Panel>
                                        <ext:Panel ID="PanelSensor2" runat="server" Width="70" Height="90" Unstyled="True" PaddingSpec="0 5 0 5">
                                            <Items>
                                                <ext:TextField ID="txtSensor2" runat="server" Width="60"></ext:TextField>
                                                <ext:Button ID="btnSensor2" runat="server" Width="60" Height="40"
                                                    StyleSpec="background-color: red;background-image:none;">
                                                </ext:Button>
                                                <ext:Label ID="lblSensor2" runat="server" Text="Вход 2"
                                                    StyleSpec="color:grey;text-align:center;font-style: italic;"
                                                    Hidden="True" HideMode="Offsets">
                                                </ext:Label>
                                                <ext:Button ID="btnSensor2Off" runat="server" Text="Отбой" Width="60" PaddingSpec="3 0 0 0">
                                                    <Listeners>
                                                        <Click Handler="if($App){ $App.SettingsManager.CarSettings.sensorAlarmClick();}"></Click>
                                                    </Listeners>
                                                </ext:Button>
                                            </Items>
                                        </ext:Panel>
                                    </Items>
                                </ext:Panel>
                            </Items>
                        </ext:Panel>
                    </Items>
                    <Buttons>
                        <ext:Button runat="server" ID="btnSave" Text="Сохранить" Icon="Accept">
                            <Listeners>
                                <Click Handler="if($App){ $App.SettingsManager.CarSettings.save();}"></Click>
                            </Listeners>
                        </ext:Button>
                    </Buttons>
                </ext:Panel>
            </Items>
            <Listeners>
                <Hide Handler="$App.SettingsManager.CarSettings.hide();"></Hide>
            </Listeners>
        </ext:Window>
        <ext:Window ID="windowVirtualTracker" runat="server"
            Title="Редактирование виртуального трекера"
            Width="300"
            Constrain="true"
            Hidden="True"
            Resizable="False"
            CloseAction="Hide"
            Modal="True"
            Icon="CogEdit">
            <Items>
                <ext:Panel ID="Panel2" runat="server" Unstyled="True" Padding="5">
                    <Items>
                        <ext:TextField runat="server" ID="txtVirtualName" FieldLabel="Название">
                            <Listeners>
                                <Change Handler="App.btnSaveVirtual.setDisabled(!newValue);"></Change>
                            </Listeners>
                        </ext:TextField>
                        <ext:Label runat="server" ID="lblVirtualId" Text="павыпа" />
                    </Items>
                    <Buttons>
                        <ext:Button ID="btnDeleteVirtual" runat="server" Icon="Delete" Text="Очистить">
                            <Listeners>
                                <Click Handler="$App.SettingsManager.CarSettings.clearVirtual();"></Click>
                            </Listeners>
                        </ext:Button>
                        <ext:Button ID="btnSaveVirtual" runat="server" Icon="Accept" Text="Применить" Disabled="True">
                            <Listeners>
                                <Click Handler="$App.SettingsManager.CarSettings.saveVirtual();"></Click>
                            </Listeners>
                        </ext:Button>
                    </Buttons>
                </ext:Panel>
            </Items>
            <Listeners>
                <Hide Handler="if($App){ $App.SettingsManager.CarSettings.applyVirtualData();}"></Hide>
            </Listeners>
        </ext:Window>
    </form>
</body>
</html>
