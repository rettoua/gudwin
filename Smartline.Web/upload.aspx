<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="upload.aspx.cs" Inherits="Smartline.Web.upload" %>

<%@ Register Assembly="Ext.Net" Namespace="Ext.Net" TagPrefix="ext" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Загрузка файла</title>
</head>
<body>
    <form id="form1" runat="server">
        <ext:ResourceManager ID="ResourceManager1" runat="server" />
        <ext:Panel runat="server" Width="350" Title="Загрузка файла" Icon="DiskUpload" Padding="10">
            <Items>
                <ext:Panel runat="server" Unstyled="True" Padding="10" Layout="Anchor" ButtonAlign="Center">
                    <Items>
                        <ext:FileUploadField ID="FileUploadField1" runat="server" FieldLabel="Файл"
                            AnchorHorizontal="100%" LabelWidth="70" ButtonText="Выбрать..." >
                        </ext:FileUploadField>
                        <ext:TextField runat="server" ID="txtFileName" FieldLabel="Имя файла" LabelWidth="70" AnchorHorizontal="100%"></ext:TextField>
                    </Items>
                    <Buttons>
                        <ext:Button runat="server" Text="Загрузить" Icon="Accept">
                            <DirectEvents><Click OnEvent="UploadClick"></Click></DirectEvents>
                        </ext:Button>
                    </Buttons>
                </ext:Panel>

            </Items>
        </ext:Panel>
    </form>
</body>
</html>
