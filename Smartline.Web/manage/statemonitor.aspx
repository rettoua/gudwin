<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="statemonitor.aspx.cs" Inherits="Smartline.Web.manage.statemonitor" %>

<%@ Register Assembly="Ext.Net" Namespace="Ext.Net" TagPrefix="ext" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <script language="javascript">
        function columnRender(record) {
            switch (record) {
                case 'nc':
                    return "Инициализация нового подлючения";
                case 'tc':
                    return "Трекер подключен";
                case 'td':
                    return "Трекер отключился";
                case 'it':
                    return "Неизвестный трекер";
            }
            return record;
        }
    </script>
    <style>
        .editable-over
        {
            background-color: #73e6ff;
            cursor: pointer;
        }

        .filter-icon
        {
            background-image: url('../Resources/dot.png');
            background-position: center;
            background-repeat: no-repeat;
            width: 26px;
            height: 26px;
        }

        span.event-inactive
        {
            color: gray;
        }

        span.event-date
        {
            font-weight: bold;
            color: rgb(0, 149, 255);
        }

        span.online-counter
        {
            color: rgb(0, 149, 255);
            font-size: 11px;
            font-weight: bold;
            font-family: tahoma,arial,verdana,sans-serif;
            line-height: 15px;
            text-transform: none;
        }

        span.online-counter-value
        {
            color: #333;
            font-size: 11px;
            font-weight: bold;
            font-family: tahoma,arial,verdana,sans-serif;
            line-height: 15px;
            text-transform: none;
        }
    </style>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <ext:ResourceManager ID="ResourceManager1" runat="server" Theme="Gray" Locale="RU-ru" DirectMethodNamespace="DR" />
            <ext:Viewport runat="server" ID="viewport" Layout="BorderLayout" AutoScroll="True">
                <Items>
                    <ext:Panel runat="server" Region="Center">
                        <Items>
                            <ext:GridPanel ID="gridMonitoringEvents" runat="server"
                                Title="Состояния подключения трекеров" Split="True" Layout="Anchor" AutoScroll="True" InvalidateScrollerOnRefresh="False" SelectionMemory="True">
                                <Store>
                                    <ext:Store ID="Store1" runat="server">
                                        <Model>
                                            <ext:Model ID="Model1" runat="server">
                                                <Fields>
                                                    <ext:ModelField Name="d" Type="Date"></ext:ModelField>
                                                    <ext:ModelField Name="typeid"></ext:ModelField>
                                                    <ext:ModelField Name="t"></ext:ModelField>
                                                    <ext:ModelField Name="i"></ext:ModelField>
                                                    <ext:ModelField Name="u"></ext:ModelField>
                                                    <ext:ModelField Name="r"></ext:ModelField>
                                                </Fields>
                                            </ext:Model>
                                        </Model>
                                        <Sorters>
                                            <ext:DataSorter Property="d" Direction="DESC" />
                                        </Sorters>
                                    </ext:Store>
                                </Store>
                                <ColumnModel>
                                    <Columns>
                                        <ext:DateColumn ID="DateColumn1" runat="server" Text="Время" DataIndex="d" Format="d-m-Y H:i:s"
                                            Width="150">
                                        </ext:DateColumn>
                                        <ext:Column ID="Column2" runat="server" Text="Событие" DataIndex="typeid" Width="195">
                                            <Renderer Fn="columnRender"></Renderer>
                                        </ext:Column>
                                        <ext:Column ID="Column1" runat="server" Text="Идентификатор трекера" DataIndex="t" Width="150">
                                        </ext:Column>
                                        <ext:Column ID="Column3" runat="server" Text="IP" DataIndex="i" Width="150">
                                        </ext:Column>
                                        <ext:Column ID="Column4" runat="server" Text="Пользователь" DataIndex="u">
                                        </ext:Column>
                                        <ext:Column ID="Column5" runat="server" Text="Причина" DataIndex="r">
                                        </ext:Column>
                                    </Columns>
                                </ColumnModel>
                                <SelectionModel>
                                    <ext:RowSelectionModel ID="RowSelectionModel1" runat="server" Mode="Single">
                                    </ext:RowSelectionModel>
                                </SelectionModel>
                                <View>
                                    <ext:GridView ID="GridView1" runat="server"></ext:GridView>
                                </View>
                            </ext:GridPanel>
                        </Items>
                    </ext:Panel>
                    <ext:Panel runat="server" Region="West" Title="Онлайн статистика" Width="450" Split="True">
                        <Items>
                            <ext:FieldContainer ID="FieldContainer1" runat="server" Padding="5" Layout="Form">
                                <Items>
                                    <ext:Label runat="server" ID="txtTotalTrackers"></ext:Label>
                                    <ext:Label runat="server" ID="txtPackages"></ext:Label>
                                </Items>
                            </ext:FieldContainer>
                            <ext:Chart
                                ID="OnlineActivityChart"
                                runat="server"
                                StyleSpec="background:#fff;"
                                Animate="False"
                                Width="350"
                                Height="350"
                                MaxWidth="350"
                                MinWidth="350"
                                MinHeight="350"
                                MaxHeight="350"
                                Resizable="False">
                                <Store>
                                    <ext:Store ID="OnlineActivityStore" runat="server">
                                        <Model>
                                            <ext:Model ID="Model2" runat="server">
                                                <Fields>
                                                    <ext:ModelField Name="date" Type="Date" />
                                                    <ext:ModelField Name="packages" Type="Int" />
                                                </Fields>
                                            </ext:Model>
                                        </Model>
                                    </ext:Store>
                                </Store>
                                <Axes>
                                    <ext:NumericAxis
                                        Fields="packages"
                                        Title="Кол-во принятых пакетов"
                                        Minimum="0"
                                        Maximum="10">
                                        <GridConfig>
                                            <%--<Odd Opacity="1" Fill="#dedede" Stroke="#ddd" StrokeWidth="0.5" />--%>
                                        </GridConfig>
                                    </ext:NumericAxis>

                                    <ext:TimeAxis
                                        Position="Bottom"
                                        Fields="date"
                                        Title="Время"
                                        DateFormat="hh:mm:ss"
                                        FromDate="<%# new DateTime(2011, 1, 1) %>"
                                        ToDate="<%# new DateTime(2011, 1, 7) %>"
                                        AutoDataBind="true" />
                                </Axes>
                                <Series>
                                    <ext:LineSeries Axes="Left,Bottom" XField="date" YField="packages" ShowMarkers="False">
                                        <Label Display="None" Field="packages" TextAnchor="middle" />
                                        <%--<MarkerConfig Size="5" Radius="5" />--%>
                                    </ext:LineSeries>
                                </Series>
                            </ext:Chart>
                        </Items>
                    </ext:Panel>
                </Items>
            </ext:Viewport>
        </div>
        <ext:TaskManager ID="TaskManagerOnlineUpdater" runat="server">
            <Tasks>
                <ext:Task TaskID="DataTask" AutoRun="true" Interval="2000">
                    <Listeners>
                        <Update Handler="MON.updateOnlineActivity();"></Update>
                    </Listeners>
                </ext:Task>
            </Tasks>
        </ext:TaskManager>
    </form>
</body>
</html>
