<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="accounting.aspx.cs" Inherits="Smartline.Web.manage.accounting" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <ext:ResourceManager ID="ResourceManager1" runat="server" Theme="Gray" DirectMethodNamespace="DR" Locale="RU-ru" />
        <ext:Viewport ID="Viewport1" runat="server" Cls="viewport" Layout="BorderLayout">
            <Items>
                <ext:Panel runat="server" ID="pnlAccounting" Title="Настройки">
                    <Items>
                        <ext:Panel runat="server" Padding="15" Unstyled="True">
                            <Items>
                                <ext:NumberField runat="server" ID="fldMonthWriteOff"
                                    LabelWidth="200"
                                    FieldLabel="Сумма списания за месяц"
                                    EmptyText="0"
                                    MinValue="0">
                                </ext:NumberField>
                                <ext:NumberField runat="server" ID="fldFinancialLock"
                                    LabelWidth="200"
                                    EmptyText="0"
                                    FieldLabel="Граничная сумма для финансовой блокировки"
                                    MaxValue="-1">
                                </ext:NumberField>
                            </Items>
                            <Buttons>
                                <ext:Button runat="server"
                                    Text="Сохранить"
                                    Icon="Accept">
                                    <DirectEvents>
                                        <Click OnEvent="SaveSettingsClick"></Click>
                                    </DirectEvents>
                                </ext:Button>
                            </Buttons>
                        </ext:Panel>
                    </Items>
                </ext:Panel>
            </Items>
        </ext:Viewport>
    </form>
</body>
</html>
