<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="Smartline.Web.tracker" %>

<%@ Import Namespace="System.Web.Optimization" %>

<%@ Register Assembly="Ext.Net" Namespace="Ext.Net" TagPrefix="ext" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script type="text/javascript">
    //hide
    var HideLogoLoad = function () {
        var div = document.getElementById('mask');
        div.style.display = 'none';
    };
    var toggle = function (toolbar) {
        Ext.select('.tab-switch').each(function (t) {
            Ext.getCmp(t.dom.id).hide();
        });
    };
</script>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>GUDWIN система GPS мониторинга</title>    
    <%: Styles.Render("~/bundles/GlobalStyle") %>
</head>
<body>

    <div id="mask" style="position: absolute; background-color: #32333D; text-align: center; z-index: 1000; left: 0; top: 0; height: 100%; width: 100%; opacity: 0.7;">
        <table id="logotable" style="height: 100%; width: 100%; border: 0px; padding: 0px; margin: 0px;">
            <tr>
                <td id="loadtd" style="color: white; font-size: 13px; font-family: tahoma;">
                    <img style="vertical-align: middle;" src="Resources/extanim32.gif" alt="logo" />
                    Загрузка модулей...
                </td>
            </tr>
        </table>
    </div>

    <div id="logodiv" class="logo">
        GUDWIN система GPS мониторинга
    </div>
    <form id="Form1" runat="server">
        <ext:ResourceManager ID="ResourceManager1" runat="server" Theme="Gray" DirectMethodNamespace="DR" Locale="RU-ru" />
        <%: Scripts.Render("~/bundles/Global") %>
        <ext:Viewport ID="Viewport1" runat="server" Cls="viewport" Layout="BorderLayout">
            <Items>
                <ext:Panel ID="Panel4" runat="server" Height="105" Width="350" Layout="card" ActiveIndex="0"
                    DefaultBorder="false" Border="false" Region="Center">
                    <TopBar>
                        <ext:Toolbar ID="toolbarTabsWrapper" runat="server" Layout="Container" Flat="true">
                            <Items>
                                <ext:Toolbar ID="toolbarTabs" runat="server" Flat="true" EnableOverflow="false">
                                    <Items>
                                        <ext:TabStrip ID="tabs" runat="server" StyleSpec="padding-bottom: 2px; margin-left: 275px;"
                                            EnableTabScroll="true">
                                            <Items>
                                                <ext:Tab TabID="tabMap" Text="Карта" Icon="WorldLink" ActionItemID="pnlMap" />
                                                <ext:Tab TabID="tabCars" Text="Трекеры" Icon="Controller" ActionItemID="pnlCars" Hidden="True" />
                                                <ext:Tab TabID="tabSettings" Text="Настройки" Icon="FolderExplore" ActionItemID="pnlSettings" />
                                                <ext:Tab TabID="tabReport" runat="server" Text="Отчеты" Icon="Report" ActionItemID="pnlReporting" />
                                                <ext:Tab TabID="tabAdmin" runat="server" Hidden="True" Text="Администрирование" Icon="CogAdd" ActionItemID="pnlAdmin" />
                                                <ext:Tab TabID="tabStateMonitor" runat="server" Hidden="True" Text="Монитор событий" Icon="LightningGo" ActionItemID="pnlStateMonitor" />
                                                <ext:Tab TabID="tabTraffic" runat="server" Hidden="True" Text="Трафик" Icon="DriveNetwork" ActionItemID="pnlTraffic" />
                                                <ext:Tab TabID="tabAccounting" runat="server" Hidden="True" Text="Управление счетами" Icon="CalculatorEdit" ActionItemID="pnlAccounting" />
                                            </Items>
                                        </ext:TabStrip>
                                        <ext:ToolbarFill ID="ctl45" />
                                        <ext:Label ID="Label1" runat="server" />
                                        <ext:ToolbarSpacer ID="ctl48" />
                                        <ext:Label ID="authUser" runat="server" Cls="auth-user" />
                                        <ext:ToolbarSpacer ID="ToolbarSpacer1" />
                                        <ext:LinkButton ID="btnAccounting" runat="server" Text="Личный кабинет" Cls="auth-user-signout" Visible="True">
                                            <Listeners>
                                                <Click Handler="App.windowPersonal.show();"></Click>
                                            </Listeners>
                                        </ext:LinkButton>
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
                        <ext:Panel ID="pnlMap" runat="server" Header="false" MaskOnDisable="False">
                            <Loader Url="map.aspx" Mode="Frame" runat="server">
                            </Loader>
                        </ext:Panel>
                        <ext:Panel ID="pnlCars" runat="server" Header="false" MaskOnDisable="False" IDMode="Static">
                            <Loader runat="server" Url="cars.aspx" Mode="Frame">
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
                            <Loader Url="manage/admin.aspx" Mode="Frame" runat="server">
                            </Loader>
                        </ext:Panel>
                        <ext:Panel ID="pnlStateMonitor" runat="server" Header="false" IDMode="Static">
                            <Loader ID="Loader1" Url="manage/statemonitor.aspx" Mode="Frame" runat="server">
                            </Loader>
                        </ext:Panel>
                        <ext:Panel ID="pnlTraffic" runat="server" Header="false" IDMode="Static">
                            <Loader ID="Loader2" Url="manage/traffic.aspx" Mode="Frame" runat="server">
                            </Loader>
                        </ext:Panel>
                        <ext:Panel ID="pnlAccounting" runat="server" Header="false" IDMode="Static">
                            <Loader ID="Loader3" Url="manage/accounting.aspx" Mode="Frame" runat="server">
                            </Loader>
                        </ext:Panel>
                    </Items>
                </ext:Panel>
            </Items>
        </ext:Viewport>
        <ext:TaskManager ID="TaskManagerOnlineUpdater" runat="server">
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

        <ext:Window ID="windowPersonal" runat="server" Title="Личный кабинет" Width="585" Height="400"
            Hidden="True" Resizable="True" CloseAction="Hide" Layout="Fit">
            <Items>
                <ext:TabPanel ID="tabPanelPersonal" runat="server" Border="false" Layout="Anchor" TabMenuHidden="True" HideMode="Visibility">
                    <Items>
                        <%-- <ext:Panel runat="server" Title="Обзор">
                            <Items>
                                <ext:Panel runat="server" Padding="5" Layout="VBoxLayout" AnchorHorizontal="100%" Unstyled="True">
                                    <Items>
                                        <ext:FieldSet ID="fldSetAccount" runat="server" Border="false">
                                            <Items>
                                                <ext:Label ID="lblAmount" runat="server"></ext:Label>
                                                <ext:LinkButton ID="lnkAmount" runat="server" Text="Пополить" PaddingSpec="0 0 0 50">
                                                    <Listeners>
                                                        <Click Handler="App.wndRecharge.show();"></Click>
                                                    </Listeners>
                                                </ext:LinkButton>
                                            </Items>
                                        </ext:FieldSet>
                                    </Items>
                                </ext:Panel>
                            </Items>
                        </ext:Panel>
                        <ext:Panel runat="server" Title="Расходы" Layout="Fit">
                            <Items>
                                <ext:GridPanel runat="server" ID="gridWriteOffTransaction" TitleCollapse="True">
                                    <TopBar>
                                        <ext:Toolbar runat="server" ID="toolbar1">
                                            <Items>
                                                <ext:DateField ID="fldWriteOffFrom" runat="server" FieldLabel="От" LabelWidth="20" Width="120">
                                                    <Listeners>
                                                        <Change Handler="Core.PaymentHistory.updateWriteOffLoadButton();"></Change>
                                                    </Listeners>
                                                </ext:DateField>
                                                <ext:DateField ID="fldWriteOffTo" runat="server" FieldLabel="До" LabelWidth="20" Width="120">
                                                    <Listeners>
                                                        <Change Handler="Core.PaymentHistory.updateWriteOffLoadButton();"></Change>
                                                    </Listeners>
                                                </ext:DateField>
                                                <ext:Button ID="bntWriteOff" runat="server" Icon="Accept" Disabled="True">
                                                    <Listeners>
                                                        <Click Handler="Core.PaymentHistory.loadWriteOffs(App.fldWriteOffFrom.getValue(), App.fldWriteOffTo.getValue()); "></Click>
                                                    </Listeners>
                                                </ext:Button>
                                            </Items>
                                        </ext:Toolbar>
                                    </TopBar>
                                    <Store>
                                        <ext:Store ID="Store14444" runat="server" GroupField="trackerName">
                                            <Model>
                                                <ext:Model ID="Model14444" runat="server" IDProperty="time">
                                                    <Fields>
                                                        <ext:ModelField Name="time" Type="Date" />
                                                        <ext:ModelField Name="trackerUid" Type="Int" />
                                                        <ext:ModelField Name="trackerName" Type="String" />
                                                        <ext:ModelField Name="amount" Type="Float" />
                                                        <ext:ModelField Name="amountBefore" Type="Float" />
                                                        <ext:ModelField Name="amountAfter" Type="Float" />
                                                    </Fields>
                                                </ext:Model>
                                            </Model>
                                            <Sorters>
                                                <ext:DataSorter Property="time" Direction="ASC" />
                                            </Sorters>
                                        </ext:Store>
                                    </Store>
                                    <Features>
                                        <ext:Grouping
                                            ID="Grouping1"
                                            runat="server"
                                            HideGroupedHeader="True"
                                            StartCollapsed="False"
                                            EnableGroupingMenu="false"
                                            GroupHeaderTplString='{name}' />

                                    </Features>
                                    <ColumnModel ID="ColumnModel135" runat="server" ForceFit="True" AnchorHorizontal="100%">
                                        <Columns>
                                            <ext:SummaryColumn runat="server" ID="columnDateWriteOff" Text="Время списания" DataIndex="time" Format="d-m-Y g:i" Width="130">
                                                <Renderer Format="Date" FormatArgs="'d-m-Y g:i'"></Renderer>
                                            </ext:SummaryColumn>
                                            <ext:Column runat="server" ID="columnTrackerNameWriteOff" Text="Трекер" DataIndex="trackerName" Width="130"  />
                                            <ext:SummaryColumn runat="server" ID="columnAmountWriteOff" Text="Сумма" DataIndex="amount" Format="0.00" Width="45"
                                                SummaryType="Sum">
                                                <Renderer Handler="return value +' грн';" />
                                            </ext:SummaryColumn>
                                            <ext:NumberColumn runat="server" Text="Сумма до списания" DataIndex="amountBefore" Width="125" Groupable="False">
                                                <Renderer Handler="return value +' грн';" />
                                            </ext:NumberColumn>
                                            <ext:NumberColumn runat="server" Text="Сумма после списания" DataIndex="amountAfter" Width="130" Groupable="False">
                                                <Renderer Handler="return value +' грн';" />
                                            </ext:NumberColumn>
                                        </Columns>
                                    </ColumnModel>
                                    <DockedItems>
                                        <ext:FieldContainer ID="FieldContainerWriteOffs" runat="server" Layout="HBoxLayout" Dock="Bottom" StyleSpec="margin-top:2px;">
                                            <Defaults>
                                                <ext:Parameter Name="height" Value="22" />
                                            </Defaults>
                                            <Items>
                                                <ext:DisplayField runat="server" ID="some1" Name="time" Cls="total-field" />
                                                <ext:DisplayField runat="server" ID="some2" Name="trackerName" Cls="total-field" />
                                                <ext:DisplayField runat="server" ID="some3" Name="amount" Cls="total-field" />
                                                <ext:DisplayField runat="server" Name="amountBefore" Cls="total-field" />
                                                <ext:DisplayField runat="server" Name="amountAfter" Cls="total-field" />
                                            </Items>
                                        </ext:FieldContainer>
                                    </DockedItems>
                                    <Listeners>
                                        <ColumnResize Handler="Core.PaymentHistory.updateWriteOffsTotal();" />
                                        <ColumnMove Handler="Core.PaymentHistory.updateWriteOffsTotal();" />
                                        <ColumnHide Handler="Core.PaymentHistory.updateWriteOffsTotal();" />
                                    </Listeners>
                                </ext:GridPanel>
                            </Items>
                        </ext:Panel>
                        <ext:Panel runat="server" Title="Платежи" Layout="Fit">
                            <Items>
                                <ext:GridPanel runat="server" ID="gridPaymentTransactions" TitleCollapse="True">
                                    <TopBar>
                                        <ext:Toolbar runat="server" ID="toolbarPaymentTransaction">
                                            <Items>
                                                <ext:DateField ID="fldHistoryFrom" runat="server" FieldLabel="От" LabelWidth="20" Width="120">
                                                    <Listeners>
                                                        <Change Handler="Core.PaymentHistory.updatePaymentsLoadButton();"></Change>
                                                    </Listeners>
                                                </ext:DateField>
                                                <ext:DateField ID="fldHistoryTo" runat="server" FieldLabel="До" LabelWidth="20" Width="120">
                                                    <Listeners>
                                                        <Change Handler="Core.PaymentHistory.updatePaymentsLoadButton();"></Change>
                                                    </Listeners>
                                                </ext:DateField>
                                                <ext:Button ID="btnShowPayments" runat="server" Icon="Accept" Disabled="True">
                                                    <Listeners>
                                                        <Click Handler="Core.PaymentHistory.loadPayments(App.fldHistoryFrom.getValue(), App.fldHistoryTo.getValue()); "></Click>
                                                    </Listeners>
                                                </ext:Button>
                                            </Items>
                                        </ext:Toolbar>
                                    </TopBar>
                                    <Store>
                                        <ext:Store ID="StorePaymentTransactions" runat="server">
                                            <Model>
                                                <ext:Model ID="ModelPaymentTransactions" runat="server" IDProperty="orderId">
                                                    <Fields>
                                                        <ext:ModelField Name="orderId" />
                                                        <ext:ModelField Name="paymentTime" Type="Date" />
                                                        <ext:ModelField Name="amount" Type="Float" />
                                                        <ext:ModelField Name="acc_amount_before" Type="Float" />
                                                        <ext:ModelField Name="acc_amount_after" Type="Float" />
                                                    </Fields>
                                                </ext:Model>
                                            </Model>
                                            <Sorters>
                                                <ext:DataSorter Property="paymentTime" Direction="ASC" />
                                            </Sorters>
                                        </ext:Store>
                                    </Store>
                                    <ColumnModel ID="ColumnModelPaymentTransactions" runat="server" ForceFit="True" AnchorHorizontal="100%">
                                        <Columns>
                                            <ext:DateColumn runat="server" ID="columnPaymentDate" Text="Время пополнения" DataIndex="paymentTime" Format="d-m-Y g:i" Width="130" />
                                            <ext:NumberColumn runat="server" Text="Сумма" DataIndex="amount" Format="0.00">
                                                <Renderer Handler="return value +' грн';" />
                                            </ext:NumberColumn>
                                            <ext:NumberColumn runat="server" Text="Сумма до пополнения" DataIndex="acc_amount_before" Width="125">
                                                <Renderer Handler="return value +' грн';" />
                                            </ext:NumberColumn>
                                            <ext:NumberColumn runat="server" Text="Сумма после пополнения" DataIndex="acc_amount_after" Width="130">
                                                <Renderer Handler="return value +' грн';" />
                                            </ext:NumberColumn>
                                        </Columns>
                                    </ColumnModel>
                                    <DockedItems>
                                        <ext:FieldContainer ID="FieldContainerPayments" runat="server" Layout="HBoxLayout" Dock="Bottom" StyleSpec="margin-top:2px;">
                                            <Defaults>
                                                <ext:Parameter Name="height" Value="22" />
                                            </Defaults>
                                            <Items>
                                                <ext:DisplayField runat="server" ID="DisplayField1" Name="paymentTime" Cls="total-field" />
                                                <ext:DisplayField runat="server" ID="DisplayField2" Name="amount" Cls="total-field" />
                                                <ext:DisplayField runat="server" ID="DisplayField3" Name="acc_amount_before" Cls="total-field" />
                                                <ext:DisplayField runat="server" Name="acc_amount_after" Cls="total-field" />
                                            </Items>
                                        </ext:FieldContainer>
                                    </DockedItems>
                                    <Listeners>
                                        <ColumnResize Handler="Core.PaymentHistory.updatePaymentsTotal();" />
                                        <ColumnMove Handler="Core.PaymentHistory.updatePaymentsTotal();" />
                                        <ColumnHide Handler="Core.PaymentHistory.updatePaymentsTotal();" />
                                    </Listeners>
                                </ext:GridPanel>
                            </Items>                            
                        </ext:Panel>--%>
                    </Items>
                </ext:TabPanel>
            </Items>
            <Listeners>
            </Listeners>
        </ext:Window>
        <ext:XTemplate ID="XTemplatePayments" runat="server">
            <Html>

                <table class="payment-history-header">
                    <tr>
                        <th id="th_payment_time">Время пополнения</th>
                        <th id="th_amount">Сумма</th>
                        <th id="th_acc_amount_after">На счету</th>
                        <th id="th_acc_amount_before">На счету до пополнения</th>
                    </tr>
                </table>
                <div>
                    <table class="payment-history">
                <tpl for="items">                    
                    <tr>
                        <td id="payment_time">
                            {paymentTime}
                        </td>
					<td id="amount">
						{amount} грн						
					</td>
                     <td id="acc_amount_after">
						{acc_amount_after} 					грн
					</td>
                         <td id="acc_amount_before">
						{acc_amount_before} 					грн
					</td>
                    </tr>                                      
				</tpl>                     
                    <tr id="total">
                        <td id="total_text">
                            Всего:
                        </td>
					<td id="total_amount">
								{total}	грн	
					</td>
                     <td >						
					</td>
                    <td >						
					</td>
                    </tr>                         
                    </table>              
                    </div>
            </Html>
        </ext:XTemplate>
        <ext:Window ID="wndRecharge" runat="server" Title="Пополнение счета" Width="300" Height="200"
            Hidden="True" Resizable="False" CloseAction="Hide" Modal="True" ButtonAlign="Center">
            <Items>
                <ext:NumberField ID="fldAmount" runat="server" FieldLabel="Сумма пополнения" MinValue="5" LabelAlign="Top"
                    PaddingSpec="35 0 0 75" Text="5">
                </ext:NumberField>
                <ext:Button ID="Button1" runat="server" Text="Пополнить" Icon="Accept" MarginSpec="5 0 0 110">
                    <Listeners>
                        <Click Handler=" Core.PaymentManager.MakePayment();"></Click>
                    </Listeners>
                </ext:Button>
            </Items>

        </ext:Window>
    </form>    
</body>
</html>
