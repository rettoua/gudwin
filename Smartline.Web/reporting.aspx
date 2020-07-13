<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="reporting.aspx.cs" Inherits="Smartline.Web.reporting" ValidateRequest="false" %>
<%@ Import Namespace="System.Web.Optimization" %>

<%@ Register Assembly="Ext.Net" Namespace="Ext.Net" TagPrefix="ext" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <%: Styles.Render("~/bundles/GlobalStyle") %>
</head>
<body>
    <form id="form1" runat="server">
        <ext:ResourceManager ID="ResourceManager1" runat="server" Theme="Gray" DirectMethodNamespace="DR" Locale="RU-ru" />
        <%: Scripts.Render("~/bundles/Global") %>
        <ext:Viewport ID="Viewport1" runat="server" Cls="viewport" Layout="BorderLayout">
            <Items>
                <ext:Panel ID="Panel2" runat="server" Title="Мастер создания отчетов" Region="West"
                    Width="275" MinWidth="225" MaxWidth="400" Split="false" Collapsible="False">
                    <Items>
                        <ext:Panel runat="server" Unstyled="True">
                            <Items>
                                <ext:Button runat="server" ID="btnCreateReport" Text="Создать новый отчет" IconUrl="Resources/start_end.png"
                                    MarginSpec="10 5 5 65" TextAlign="Center">
                                    <Listeners>
                                        <Click Handler="RM.createNewReport();"></Click>
                                    </Listeners>
                                </ext:Button>
                            </Items>
                        </ext:Panel>
                        <ext:Panel runat="server" ID="pnlCreateReport" Hidden="True" HideMode="Offsets" Padding="5"
                            Unstyled="True" Layout="Anchor" Height="350">
                            <Items>
                                <ext:ComboBox runat="server" ID="cmbReportTypes" EmptyText="Тип отчета"
                                    AllowBlank="True" ForceSelection="True" AnchorHorizontal="100%">
                                    <Items>
                                        <ext:ListItem Value="1" Text="Сводный отчет" />
                                        <ext:ListItem Value="2" Text="Детальный отчет" />
                                        <ext:ListItem Value="3" Text="Отчет по стоянкам" />
                                    </Items>
                                    <Listeners>
                                        <Change Handler="RM.validate();"></Change>
                                    </Listeners>
                                </ext:ComboBox>
                                <ext:FieldSet runat="server" Frame="True" Title="Период времени" AnchorHorizontal="100%">
                                    <Items>
                                        <ext:FieldContainer ID="filterContainer" runat="server" Layout="HBoxLayout">
                                            <Items>
                                                <ext:DateField ID="dfFrom" runat="server" Width="100" HideTrigger="True" BorderSpec="-1 0 0 0"
                                                    FieldLabel="С" LabelWidth="20" PaddingSpec="0 0 0 5" AllowBlank="False">
                                                    <Listeners>
                                                        <AfterRender Handler=" triggerRender(this);"></AfterRender>
                                                        <Change Handler="RM.fromDateChanged();"></Change>
                                                    </Listeners>
                                                </ext:DateField>
                                                <ext:TimeField HideTrigger="True" ID="tfFrom" Increment="60" runat="server" SelectedTime="0:00"
                                                    Width="50" AllowBlank="False">
                                                    <Listeners>
                                                        <AfterRender Handler=" triggerRender(this);"></AfterRender>
                                                        <Change Handler="RM.validate();"></Change>
                                                    </Listeners>
                                                </ext:TimeField>
                                            </Items>
                                        </ext:FieldContainer>
                                        <ext:FieldContainer ID="FieldContainer1" runat="server" Layout="HBoxLayout">
                                            <Items>
                                                <ext:DateField ID="dfTo" runat="server" Width="100" HideTrigger="True" FieldLabel="По"
                                                    LabelWidth="20" PaddingSpec="0 0 0 5" AllowBlank="False">
                                                    <Listeners>
                                                        <AfterRender Handler=" triggerRender(this);"></AfterRender>
                                                        <Change Handler="RM.toDateChanged();"></Change>
                                                    </Listeners>
                                                </ext:DateField>
                                                <ext:TimeField runat="server" ID="tfTo" Width="50" HideTrigger="True" Increment="60"
                                                    SelectedTime="23:59" AllowBlank="False">
                                                    <Listeners>
                                                        <AfterRender Handler=" triggerRender(this);"></AfterRender>
                                                        <Change Handler="RM.validate();"></Change>
                                                    </Listeners>
                                                </ext:TimeField>
                                            </Items>
                                        </ext:FieldContainer>
                                    </Items>
                                </ext:FieldSet>
                                <ext:GridPanel ID="gridPanelCars" runat="server" Header="False" Border="False"
                                    HideHeaders="True" Unstyled="True" ManageHeight="True" MaxHeight="200">
                                    <Store>
                                        <ext:Store runat="server">
                                            <Model>
                                                <ext:Model ID="Model1" runat="server" IDProperty="Id">
                                                    <Fields>
                                                        <ext:ModelField Name="Id" />
                                                        <ext:ModelField Name="Name" Type="String" />
                                                        <ext:ModelField Name="Selected" Type="Boolean" />
                                                    </Fields>
                                                </ext:Model>
                                            </Model>
                                            <Sorters>
                                                <ext:DataSorter Property="Name" Direction="ASC" />
                                            </Sorters>
                                        </ext:Store>
                                    </Store>
                                    <ColumnModel runat="server" ForceFit="True" Selectable="False">
                                        <Columns>
                                            <ext:Column ID="Column1" runat="server" Text="Название" DataIndex="Name" Flex="1">
                                            </ext:Column>
                                            <ext:CheckColumn ID="Column2" runat="server" Text="Описание"
                                                DataIndex="Selected" Editable="True">
                                                <Listeners>
                                                    <CheckChange Handler="RM.validate(); App.gridPanelCars.store.commitChanges();"></CheckChange>
                                                </Listeners>
                                            </ext:CheckColumn>
                                        </Columns>
                                    </ColumnModel>
                                    <View>
                                        <ext:GridView ID="GridView1" runat="server" StripeRows="False">
                                        </ext:GridView>
                                    </View>
                                    <SelectionModel>
                                        <ext:RowSelectionModel ID="RowSelectionModel1" runat="server" Mode="Multi">
                                            <Listeners>
                                            </Listeners>
                                        </ext:RowSelectionModel>
                                    </SelectionModel>
                                </ext:GridPanel>
                            </Items>
                            <Buttons>
                                <ext:Button ID="btnCancelReport" runat="server" Text="Отмена" Icon="Cancel">
                                    <Listeners>
                                        <Click Handler="RM.hideCreateReport();"></Click>
                                    </Listeners>
                                </ext:Button>
                                <ext:Button ID="btnGenerateReport" runat="server" Text="Сгенерировать" Icon="Accept" Disabled="True">
                                    <Listeners>
                                        <Click Handler="RM.addReport();"></Click>
                                    </Listeners>
                                </ext:Button>
                            </Buttons>
                        </ext:Panel>
                    </Items>
                </ext:Panel>

                <ext:TabPanel ID="TabPanelReports" runat="server" Region="Center" AutoScroll="True">
                    <TopBar>
                        <ext:Toolbar ID="ToolbarPrint" runat="server" HideMode="Offsets" Hidden="True">
                            <Items>
                                <ext:Button ID="Button1" runat="server" Text="Печать" Icon="Printer" OnClientClick="printReport();" />
                                <ext:Button runat="server" ID="btnExport" Icon="PageExcel" Text="Сохранить в  Excel">
                                    <%--<Listeners>
                                        <Click Handler="RM.export();" >                                            
                                        </Click>
                                    </Listeners>--%>
                                    <DirectEvents>
                                        <Click OnEvent="btnExportClick" IsUpload="True">
                                            <ExtraParams>
                                                <ext:Parameter Name="values" Value="RM.export()" Mode="Raw"/>
                                            </ExtraParams>
                                        </Click>
                                    </DirectEvents>
                                </ext:Button>
                            </Items>
                        </ext:Toolbar>
                    </TopBar>
                    <Items>
                        <%--<ext:DataView ID="DataView1"
                            runat="server"
                            DisableSelection="true"
                            ItemSelector="td.letter-row"
                            EmptyText="Нет данных для отображения">
                            <Store>
                                <ext:Store ID="Store1" runat="server">
                                    <Model>
                                        <ext:Model ID="Model3" runat="server" IDProperty="Title">
                                            <Fields>
                                                <ext:ModelField Name="Title" />
                                                <ext:ModelField Name="Data" IsComplex="true" />
                                                <ext:ModelField Name="Days" IsComplex="true" />
                                                <ext:ModelField Name="Summary" />
                                                <ext:ModelField Name="tracker" />                                                
                                            </Fields>
                                        </ext:Model>
                                    </Model>
                                </ext:Store>
                            </Store>
                            <Tpl runat="server" ID="SummaryReportTemplate">
                                <Html>
                                    <div id="customers-ct">
					<div class="header">
						<p>{Title}</p>                                                                        
					</div>
					<table>
						<tr>
							<th style="width:15%">Дата</th>
							<th style="width:20%">Время движения, ч</th>
							<th style="width:20%">Время стоянки, ч</th>
							<th style="width:18%">Средняя скорость, км/ч</th>
							<th style="width:18%">Макс. скорость, км/ч</th>
							<th style="width:10%">Пробег, км</th>
						</tr>
					
						<tpl for=".">
								<tr>
									<td colspan="6">
										<div class="letter-row-div"><h2 class="letter-selector" >{tracker}</h2></div>
                                        <tpl for="Days">
									        <table class="data-table">                                                
                                                <tr class="customer-record">
										            <td style="width:15%">{g}</td>
										            <td style="width:20%">&nbsp;{f}</td>
										            <td style="width:20%">&nbsp;{e}</td>
										            <td style="width:18%">&nbsp;{b}</td>
										            <td style="width:18%">&nbsp;{c}</td>
										            <td style="width:10%">&nbsp;{d}</td>
									            </tr>
                                            </table>
								        </tpl>
                                        
                                        <tpl for="Summary">
									        <table class="data-table">                                                
                                                <tr class="summary-record">
										            <td style="width:15%"></td>
										            <td style="width:20%">&nbsp;{moving}</td>
										            <td style="width:20%">&nbsp;{parking}</td>
										            <td style="width:18%">&nbsp;</td>
										            <td style="width:18%">&nbsp;</td>
										            <td style="width:10%">&nbsp;{distance}</td>
									            </tr>
                                            </table>
								        </tpl>
									</td>
								</tr>								
						</tpl>                    
					</table>
				</div>

                                </Html>
                            </Tpl>
                            <Listeners>
                                <ItemClick Fn="summaryItemClick" />
                                <Refresh Handler="this.el.select('tr.customer-record').addClsOnOver('cust-name-over');" Delay="100" />
                            </Listeners>
                        </ext:DataView>--%>

                        <%--<ext:DataView ID="DataViewDetail"
                            runat="server"
                            DisableSelection="true"
                            ItemSelector="td.letter-row"
                            EmptyText="Нет данных для отображения">
                            <Store>
                                <ext:Store ID="Store1" runat="server">
                                    <Model>
                                        <ext:Model ID="Model3" runat="server" IDProperty="Title">
                                            <Fields>
                                                <ext:ModelField Name="Title" />
                                                <ext:ModelField Name="Data" IsComplex="true" />
                                                <ext:ModelField Name="Days" IsComplex="true" />
                                                <ext:ModelField Name="tracker" />
                                                <ext:ModelField Name="g" />
                                                <ext:ModelField Name="moving" />
                                            </Fields>
                                        </ext:Model>
                                    </Model>
                                </ext:Store>
                            </Store>
                            <Tpl runat="server" ID="SummaryReportTemplate">
                                <Html>
                                    <div id="customers-ct">
					                    <div class="header">
						                    <p>Title</p>                                                                        
					                    </div>
					                        <table>
					                            <tr>
					                                <td colspan="4" style="text-align: center; padding: 5px;"><h3>Движение</h3></td>
                                                    <td colspan="4" align="center" ><h3>Стоянка</h3></td>
                                                </tr>
						                        <tr>
							                        <th style="width:8%">Начало</th>
							                        <th style="width:8%">Конец</th>
							                        <th style="width:10%">Общее время</th>
                                                    <th style="width:8%">Пробег, км</th>
							                        <th style="width:8%">Начало</th>
							                        <th style="width:8%">Конец</th>
							                        <th style="width:10%">Общее время</th>
                                                    <th style="width:40%">Адрес</th>
						                        </tr>
					
						                        <tpl for=".">
								                        <tr>
									                        <td colspan="8">
										                        <div class="letter-row-div"><h2>{tracker}</h2></div>                                        
                                                                <tpl for="Days">
                                                                    <div class="date-row" ><div class="date-row-div" onclick="detailItemClick(this)"><h3>{g}</h3></div> 
                                                                        <table class="data-table">
                                                                         <tpl for="items">									                                                             
                                                                                            <tr class="customer-record">
                                                                                                <tpl for="moving">
										                                                            <td style="width:8%">{s}</td>
										                                                            <td style="width:8%">&nbsp;{e}</td>
										                                                            <td style="width:10%">&nbsp;{t}</td>
										                                                            <td style="width:8%">&nbsp;{d}</td>
                                                                                                </tpl>
                                                                                                <tpl for="parking">
										                                                            <td style="width:8%" class="parking">&nbsp;{s}</td>
										                                                            <td style="width:8%" >&nbsp;{e}</td>
                                                                                                    <td style="width:10%">&nbsp;{t}</td>
                                                                                                    <td style="width:40%">&nbsp;{g}</td>
                                                                                                </tpl>
									                                                        </tr>          
                                                             
								                                                    </tpl>
                                                                            </table> 
                                                                    </div>                                                                                      
                                                                </tpl>
                                            
									                        </td>
								                        </tr>								
						                        </tpl>                    
					                        </table>
				                        </div>
                                </Html>
                            </Tpl>
                            <Listeners>
                                <ItemClick Fn="detailItemClick" />
                                <Refresh Handler="this.el.select('tr.customer-record').addClsOnOver('cust-name-over');" Delay="100" />
                            </Listeners>
                        </ext:DataView>--%>

                        <%--<ext:DataView ID="DataViewParking"
                            runat="server"
                            DisableSelection="true"
                            ItemSelector="div.letter-row-div"
                            EmptyText="Нет данных для отображения">
                            <Store>
                                <ext:Store ID="Store1" runat="server" >
                                    <Model>
                                        <ext:Model ID="Model3" runat="server" IDProperty="Title" >
                                            <Fields>
                                                <ext:ModelField Name="Title" />
                                                <ext:ModelField Name="Data" IsComplex="true" />
                                                <ext:ModelField Name="Days" IsComplex="true" />
                                                <ext:ModelField Name="tracker" />
                                                <ext:ModelField Name="g" />                                                
                                            </Fields>
                                        </ext:Model>
                                    </Model>
                                </ext:Store>
                            </Store>
                            <Tpl runat="server" ID="SummaryReportTemplate">
                                <Html>
                                    <div id="customers-ct">
					                    <div class="header">
						                    <p>Title</p>                                                                        
					                    </div>
					                        <table>					                            
						                        <tr>							                        
							                        <th style="width:15%">Начало</th>
							                        <th style="width:15%">Конец</th>
							                        <th style="width:15%">Общее время</th>
                                                    <th style="width:55%">Адрес</th>
						                        </tr>
					
						                        <tpl for=".">
								                        <tr>
									                        <td colspan="8">
										                        <div class="letter-row-div"><h2>{tracker}</h2></div>                                        
                                                                <tpl for="Days">
                                                                    <div class="date-row" ><div class="date-row-div" onclick="detailItemClick(this)"><h3>{g}</h3></div> 
                                                                        <table class="data-table">
                                                                         <tpl for="items">									                                                             
                                                                                            <tr class="customer-record">                                                                                                
                                                                                                <tpl for="parking">
										                                                            <td style="width:15%" class="parking">&nbsp;{s}</td>
										                                                            <td style="width:15%" >&nbsp;{e}</td>
                                                                                                    <td style="width:15%">&nbsp;{t}</td>
                                                                                                    <td style="width:55%">&nbsp;{g}</td>
                                                                                                </tpl>
									                                                        </tr>          
                                                             
								                                                    </tpl>
                                                                            </table> 
                                                                    </div>                                                                                      
                                                                </tpl>
                                            
									                        </td>
								                        </tr>								
						                        </tpl>                    
					                        </table>
				                        </div>
                                </Html>
                            </Tpl>
                            <Listeners>                                
                            </Listeners>
                        </ext:DataView>--%>
                    </Items>
                    <Listeners>
                        <TabChange Handler="App.ToolbarPrint.show();"></TabChange>
                        <TabClose Handler="App.ToolbarPrint.hide();"></TabClose>
                    </Listeners>
                </ext:TabPanel>
            </Items>
            <Listeners>
                <AfterRender Handler="intitialize();"></AfterRender>
            </Listeners>
        </ext:Viewport>
    </form>
</body>
</html>
