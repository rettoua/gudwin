<%@ Page Language="C#" %>

<%@ Import Namespace="System.Runtime.InteropServices" %>

<%@ Register Assembly="Ext.Net" Namespace="Ext.Net" TagPrefix="ext" %>
<%@ Import Namespace="Smartline.Web.Log" %>

<script runat="server">

    private void download(object sender, DirectEventArgs e) {

        var trackerId = Convert.ToInt32(fldTrackerId.Value);
        var date = (DateTime)fldDate.Value;
        if (trackerId == 0 || date == DateTime.MinValue) {
            X.Msg.Alert("Валидация", "Данные введены некоректно").Show();
            return;
        }
        var logParser = new FileLogParser(trackerId, date);
        string result = logParser.Parse();
        if (result == "") {
            X.Msg.Alert("Файл не найден", "Логи файл за выбранную дату отсутствует").Show();
            return;
        }
        Response.Clear();
        Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}.txt", trackerId));
        Response.ContentType = "text/plain";
        Response.Write(result);
        Response.End();
    }

</script>

<!DOCTYPE html>
<html>

<script type="text/javascript">
    var load = function () {
        if (!App.fldTrackerId.getValue() || !App.fldDate.getValue()) {
            Ext.Msg.alert('Валидация', 'Данные введены неправильно. Введите коректные данные!');
            return;
        }
        showMask();

    }

    var showMask = function () {
        App.Window1.mask('Загрузка...');
    }

    var hideMask = function () {
        App.Window1.unmask();
    }
</script>
<form id="form1" runat="server">
    <ext:ResourceManager ID="ResourceManager1" runat="server">
    </ext:ResourceManager>
    <ext:Window
        ID="Window1"
        runat="server"
        Title="Загрузка лога"
        Icon="Application"
        Height="185"
        Width="350"
        BodyStyle="background-color: #fff;"
        BodyPadding="5"
        ButtonAlign="Center"
        Closable="False"
        Modal="true">
        <Items>
            <ext:NumberField runat="server" ID="fldTrackerId" MinValue="1" FieldLabel="ID трекера" PaddingSpec="25 0 0 25"></ext:NumberField>
            <ext:DateField runat="server" ID="fldDate" FieldLabel="День" PaddingSpec="0 0 0 25"></ext:DateField>
        </Items>
        <Buttons>
            <ext:Button runat="server" Icon="Accept" Text="Выбрать данные с лога">
                <DirectEvents>
                    <Click
                        AutoDataBind="true"
                        Buffer="300"
                        IsUpload="true"
                        Method="GET"
                        OnEvent="download">
                    </Click>
                </DirectEvents>
            </ext:Button>
        </Buttons>
    </ext:Window>
</form>
</html>
