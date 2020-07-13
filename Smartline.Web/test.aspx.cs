using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Ext.Net;


namespace COG.Environmental
{
    public partial class test : System.Web.UI.Page
    {

        //protected void Page_Load(object sender, EventArgs e)
        //{
        //    var es = new Elem("Sys.Entity", "DF7C4808-637E-4469-8FB4-9F9401730676");
        //    var entityBuilder = AutoGen.Get(es);
        //    entityBuilder.Save();
        //    //return;
        //    //var sampler = new Elem("Sys.Sampler", "9bb3e664-9a25-4770-b73c-9fa200d1e7ab");
        //    //var cb = ControlBuilder.Get(sampler);
        //    //cb.UpdateContent();
        //    //cb.Save();
        //    //return;

        //    //var elem = new Elem("Sys.Entity" + "", "DF7C4808-637E-4469-8FB4-9F9401730676");
        //    //var field = new Elem("Sys.Field", "74d8e7d7-0026-4849-9676-9f94017ab8c6");
        //    ////field.SetValue("FieldKey", "TestWell");
        //    //var eb = AutoGen.Get(elem);
        //    //eb.UpdateContent();
        //    //eb.RemoveField("74d8e7d7-0026-4849-9676-9f94017ab8c6");
        //    //eb.RemoveFieldObsolete("d1afa16a-fa82-42de-a30e-9f94017ab8e3");
        //    //eb.RenameField(field);
        //    //eb.Save();

        //}

    }
}