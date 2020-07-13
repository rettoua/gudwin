<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="cars.aspx.cs" Inherits="Smartline.Web.cars" %>

<%@ Register Assembly="Ext.Net" Namespace="Ext.Net" TagPrefix="ext" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <ext:ResourceManager ID="ResourceManager1" runat="server" Theme="Gray" Locale="RU-ru" DirectMethodNamespace="DR" />
        <ext:Viewport runat="server" Layout="BorderLayout">
            <Items>
                <ext:Panel runat="server" Region="Center" Title="Автомобили" AutoScroll="True">
                    <Items>
                        <ext:GridPanel ID="gridPanelCars" runat="server"
                            MultiSelect="True" AutoScroll="True"
                            ManageHeight="True" Layout="Anchor" AnchorVertical="100%">
                            <TopBar>
                                <ext:Toolbar ID="toolbarTabsWrapper" runat="server">
                                    <Items>
                                        <ext:Button runat="server" Text="Добавить" Icon="Add" Disabled="True" Hidden="True">
                                            <Listeners>
                                                <Click Handler="App.gridPanelCars.store.add({trackerid :0 });">
                                                </Click>
                                            </Listeners>
                                        </ext:Button>
                                        <ext:ToolbarSeparator Hidden="True" />
                                        <ext:Button ID="btnDelete" runat="server" Text="Удалить" Icon="Delete"
                                            Disabled="True" Hidden="False">
                                            <Listeners>
                                                <Click Handler="Ext.Msg.confirm('Удаление трекера', 'Подтвердите удаление трекера ', function(btn, text){
                                                                    if (btn == 'yes'){
                                                                    App.gridPanelCars.deleteSelected();
                                                                    }
                                                                }); ">
                                                </Click>
                                            </Listeners>
                                        </ext:Button>
                                        <ext:ToolbarFill runat="server" />
                                        <ext:Button ID="btnRefresh" runat="server" Text="Обновить" Icon="Reload">
                                            <Listeners>
                                                <Click Handler="App.gridPanelCars.store.reload();">
                                                </Click>
                                            </Listeners>
                                        </ext:Button>
                                        <ext:ToolbarSeparator />
                                        <ext:Button ID="btnSaveTrackers" runat="server" Text="Сохранить"
                                            Icon="PictureSave">
                                            <Listeners>
                                                <Click Handler="#{gridPanelCars}.store.sync(); ">
                                                </Click>
                                            </Listeners>
                                        </ext:Button>
                                    </Items>
                                </ext:Toolbar>
                            </TopBar>
                            <Store>
                                <ext:Store runat="server" ID="storeCar" AutoDataBind="True">
                                    <Proxy>
                                        <ext:AjaxProxy Url="cars_source.ashx">
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
                                        <ext:Model ID="Model1" runat="server" IDProperty="id">
                                            <Fields>
                                                <ext:ModelField Name="id" />
                                                <ext:ModelField Name="name" Type="String" />
                                                <ext:ModelField Name="v_name" Type="String" />
                                                <ext:ModelField Name="description" />
                                                <ext:ModelField Name="trackerid" Type="Int" />
                                                <ext:ModelField Name="v_trackerid" Type="Int" />
                                                <ext:ModelField Name="hevos" Type="Boolean" />
                                                <ext:ModelField Name="r1" IsComplex="True" />
                                                <ext:ModelField Name="r2" IsComplex="True" />
                                                <ext:ModelField Name="s1" IsComplex="True" />
                                                <ext:ModelField Name="s2" IsComplex="True" />
                                                <ext:ModelField Name="r" IsComplex="True" />
                                                <ext:ModelField Name="color" Type="String" />
                                                <ext:ModelField Name="consumption" Type="Float" />
                                            </Fields>
                                        </ext:Model>
                                    </Model>
                                </ext:Store>
                            </Store>
                            <ColumnModel runat="server">
                                <Columns>
                                    <ext:Column ID="Column4" runat="server" Text="id" DataIndex="id" Visible="False" />
                                    <ext:Column ID="Column7" runat="server" Text="Виртуальный трекер" Width="140">
                                        <Renderer Fn="renderVirtualTracker"></Renderer>
                                        <Commands>
                                            <ext:ImageCommand Icon="CogEdit" CommandName="edit" />
                                        </Commands>
                                        <Listeners>
                                            <Command Fn="editVirtualTracker" />
                                        </Listeners>
                                    </ext:Column>
                                    <ext:Column ID="Column1" runat="server" Text="Название" DataIndex="name" Width="140">
                                        <Editor>
                                            <ext:TextField ID="TextField1" runat="server" AllowBlank="False">
                                            </ext:TextField>
                                        </Editor>
                                    </ext:Column>
                                    <ext:Column ID="Column2" runat="server" Text="Описание" DataIndex="description" Width="200">
                                        <Editor>
                                            <ext:TextField runat="server">
                                            </ext:TextField>
                                        </Editor>
                                    </ext:Column>
                                    <ext:Column runat="server"
                                        DataIndex="consumption"
                                        Text="Расход л/100км" >
                                        <Editor>
                                            <ext:NumberField runat="server" MinValue="0" MaxValue="100"></ext:NumberField>
                                        </Editor>
                                    </ext:Column>
                                    <ext:Column ID="Column3" runat="server" Text="ID трекера" DataIndex="trackerid" Width="70">
                                    </ext:Column>
                                    <ext:CheckColumn ID="ColumnEvos" runat="server" Text="Скрыть в Evos" DataIndex="hevos" Width="85"
                                        ToolTip="Не отображать машину в системе Такси Навигатор компании Evos" Editable="True">
                                        <Editor>
                                            <ext:Checkbox runat="server"></ext:Checkbox>
                                        </Editor>
                                    </ext:CheckColumn>
                                    <ext:Column ID="Column5" runat="server" DataIndex="color" Text="Цвет трека">
                                        <Renderer Fn="colorRenderer" />
                                        <Editor>
                                            <ext:DropDownField ID="DropDownField1" runat="server" Editable="false" MatchFieldWidth="false">
                                                <Component>
                                                    <ext:Panel ID="Panel1" runat="server">
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
                                            </ext:DropDownField>
                                        </Editor>
                                    </ext:Column>
                                    <ext:Column ID="column6" runat="server" Text="Реле" Width="100">
                                        <Renderer Fn="columnRenderR"></Renderer>
                                    </ext:Column>
                                    <ext:Column ID="columnR1" runat="server" Text="Вход 1" Width="100">
                                        <Renderer Fn="columnRenderR1"></Renderer>
                                    </ext:Column>
                                    <ext:Column ID="columnR2" runat="server" Text="Вход 2" Width="100">
                                        <Renderer Fn="columnRenderR2"></Renderer>
                                    </ext:Column>
                                    <ext:Column ID="columnS1" runat="server" Text="Датчик 1" Width="100">
                                        <Renderer Fn="columnRenderS1"></Renderer>
                                    </ext:Column>
                                    <ext:Column ID="columnS2" runat="server" Text="Датчик 2" Width="100">
                                        <Renderer Fn="columnRenderS2"></Renderer>
                                    </ext:Column>
                                </Columns>
                            </ColumnModel>
                            <View>
                                <ext:GridView runat="server" AutoScroll="True" LoadingText="Загрузка данных..." LoadMask="True">
                                </ext:GridView>
                            </View>
                            <Plugins>
                                <ext:RowEditing runat="server"
                                    ClicksToMoveEditor="1"
                                    AutoCancel="false"
                                    CancelBtnText="Отмена"
                                    SaveBtnText="Сохранить"
                                    ErrorsText="Ошибка данных"
                                    DirtyText="Сохраните или отмените изменения в текущей записи прежде чем редактировать новую">
                                    <Listeners>
                                        <Edit Handler="#{gridPanelCars}.store.sync(); "></Edit>
                                    </Listeners>
                                </ext:RowEditing>
                            </Plugins>
                            <SelectionModel>
                                <ext:RowSelectionModel runat="server" Mode="Multi">
                                    <Listeners>
                                        <Select Handler="App.btnDelete.setDisabled(false); App.pnlDetails.setDisabled(false); selectTracker();">
                                        </Select>
                                        <Deselect Handler="App.btnDelete.setDisabled(true); App.pnlDetails.setDisabled(true);">
                                        </Deselect>
                                    </Listeners>
                                </ext:RowSelectionModel>
                            </SelectionModel>
                        </ext:GridPanel>
                    </Items>
                </ext:Panel>
                <ext:Panel ID="pnlDetails" runat="server" Title="Реле/датчики" Region="East" Collapsible="False" Split="true"
                    MinWidth="225" Width="425" MaxWidth="600" BodyPadding="10" Layout="Anchor" Disabled="True" AutoScroll="True">
                    <TopBar>
                        <ext:Toolbar runat="server">
                            <Items>
                                <ext:Button runat="server" ID="btnSave" Icon="Accept" Text="Применить изменения">
                                    <Listeners>
                                        <Click Handler="saveSensors();"></Click>
                                    </Listeners>
                                </ext:Button>
                            </Items>
                        </ext:Toolbar>
                    </TopBar>
                    <Items>
                        <ext:FieldSet ID="FieldSet4" runat="server" Title="Реле" Layout="Anchor">
                            <Items>
                                <ext:Checkbox ID="chkOnRelay" runat="server" BoxLabel="Выключен" StyleSpec="color:red;">
                                    <Listeners>
                                        <Change Handler="relayStateChanged('Relay',this.getValue());"></Change>
                                    </Listeners>
                                </ext:Checkbox>
                                <ext:FieldContainer ID="cntRelay" runat="server" Layout="Anchor" Disabled="True">
                                    <Items>
                                        <ext:TextField ID="txtNameRelay" runat="server" FieldLabel="Название" AnchorHorizontal="100%"></ext:TextField>
                                    </Items>
                                </ext:FieldContainer>
                            </Items>
                        </ext:FieldSet>
                        <ext:FieldSet runat="server" Title="Вход 1" Layout="Anchor">
                            <Items>
                                <ext:Checkbox ID="chkOnRelay1" runat="server" BoxLabel="Выключен" StyleSpec="color:red;">
                                    <Listeners>
                                        <Change Handler="relayStateChanged('Relay1',this.getValue());"></Change>
                                    </Listeners>
                                </ext:Checkbox>
                                <ext:FieldContainer ID="cntRelay1" runat="server" Layout="Anchor" Disabled="True">
                                    <Items>
                                        <ext:TextField ID="txtNameRelay1" runat="server" FieldLabel="Название" AnchorHorizontal="100%"></ext:TextField>
                                    </Items>
                                </ext:FieldContainer>
                            </Items>
                        </ext:FieldSet>
                        <ext:FieldSet ID="FieldSet1" runat="server" Title="Вход 2" Layout="Anchor">
                            <Items>
                                <ext:Checkbox ID="chkOnRelay2" runat="server" BoxLabel="Выключен" StyleSpec="color:red;">
                                    <Listeners>
                                        <Change Handler="relayStateChanged('Relay2',this.getValue());"></Change>
                                    </Listeners>
                                </ext:Checkbox>
                                <ext:FieldContainer ID="cntRelay2" runat="server" Layout="Anchor" Disabled="True">
                                    <Items>
                                        <ext:TextField ID="txtNameRelay2" runat="server" FieldLabel="Название" AnchorHorizontal="100%"></ext:TextField>
                                    </Items>
                                </ext:FieldContainer>
                            </Items>
                        </ext:FieldSet>
                        <ext:FieldSet ID="FieldSet2" runat="server" Title="Датчик 1" Layout="Anchor">
                            <Items>
                                <ext:Checkbox ID="chkOnSensor1" runat="server" BoxLabel="Выключен" StyleSpec="color:red;">
                                    <Listeners>
                                        <Change Handler="relayStateChanged('Sensor1',this.getValue());"></Change>
                                    </Listeners>
                                </ext:Checkbox>
                                <ext:FieldContainer ID="cntSensor1" runat="server" Layout="Anchor" Disabled="True">
                                    <Items>
                                        <ext:TextField ID="txtNameSensor1" runat="server" FieldLabel="Название" AnchorHorizontal="100%"></ext:TextField>
                                    </Items>
                                </ext:FieldContainer>
                            </Items>
                        </ext:FieldSet>
                        <ext:FieldSet ID="FieldSet3" runat="server" Title="Датчик 2" Layout="Anchor">
                            <Items>
                                <ext:Checkbox ID="chkOnSensor2" runat="server" BoxLabel="Выключен" StyleSpec="color:red;">
                                    <Listeners>
                                        <Change Handler="relayStateChanged('Sensor2',this.getValue());"></Change>
                                    </Listeners>
                                </ext:Checkbox>
                                <ext:FieldContainer ID="cntSensor2" runat="server" Layout="Anchor" Disabled="True">
                                    <Items>
                                        <ext:TextField ID="txtNameSensor2" runat="server" FieldLabel="Название" AnchorHorizontal="100%"></ext:TextField>
                                    </Items>
                                </ext:FieldContainer>
                            </Items>
                        </ext:FieldSet>
                    </Items>
                </ext:Panel>
                <ext:Window ID="windowVirtualTracker" runat="server" Title="Редактирование виртуального трекера" Width="300"
                    Constrain="true" Hidden="True" Resizable="False" CloseAction="Hide" Icon="CogEdit">
                    <Items>
                        <ext:Panel runat="server" Unstyled="True" Padding="5">
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
                                        <Click Handler="clearVirtual();"></Click>
                                    </Listeners>
                                </ext:Button>
                                <ext:Button ID="btnSaveVirtual" runat="server" Icon="Accept" Text="Применить" Disabled="True">
                                    <Listeners>
                                        <Click Handler="saveVirtual();"></Click>
                                    </Listeners>
                                </ext:Button>
                            </Buttons>
                        </ext:Panel>
                    </Items>
                </ext:Window>
            </Items>
        </ext:Viewport>
    </form>
</body>
</html>
