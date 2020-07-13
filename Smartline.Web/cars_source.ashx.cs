using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using Ext.Net;
using Smartline.Mapping;
using Smartline.Server.Runtime.TrackerEngine;

namespace Smartline.Web {
    /// <summary>
    /// Get trackers of user and save changes like insert/update/delete
    /// </summary>
    public class cars_source : IHttpHandler, IReadOnlySessionState {

        public void ProcessRequest(HttpContext context) {
            string action = context.Request.QueryString["action"];
            var user = (User)context.Session["user"];
            if (action == "read" && user != null && user.Trackers != null) {
                user = CouchbaseManager.GetUser(user.Id + "");
                if (user.Trackers != null) {
                    string trackers = JSON.Serialize(user.Trackers);
                    context.Response.ContentType = "text/json";
                    context.Response.Write(trackers);
                }
            } else {
                var trackersToResponse = new List<Tracker>();
                var dataHandler = new StoreDataHandler(context);
                List<Tracker> trackers = dataHandler.ObjectData<Tracker>();
                switch (action) {
                    case "update": {
                            foreach (Tracker tracker in trackers) {
                                var trackInUser = user.Trackers.FirstOrDefault(o => o.Id == tracker.Id);
                                if (trackInUser != null) {
                                    bool sensorsChanged = trackInUser.SensorsChanged(tracker);
                                    if (tracker.V_TrackerId != 0 && trackInUser.V_TrackerId == 0) {
                                        tracker.V_TrackerId = (int)Increments.GenerateTrackerId();
                                    }
                                    trackInUser.Update(tracker);
                                    trackersToResponse.Add(trackInUser);
                                    if (sensorsChanged) {
                                        //TrackerUpdateSaver.Instance.Add(tracker.TrackerId);
                                    }
                                }
                            }
                            user.Update();
                            context.Session["user"] = user;
                        }
                        break;
                    //case "create": {
                    //        int nextId = 0;
                    //        foreach (Tracker tracker in trackers) {
                    //            nextId = Tracker.NextId(nextId);
                    //            tracker.Id = nextId;
                    //            user.Trackers.Add(tracker);
                    //            trackersToResponse.Add(tracker);
                    //            nextId++;
                    //        }
                    //        user.Update();
                    //    }
                    //    break;
                    case "destroy": {
                            var track = trackers.Select(o => o.Id);
                            user.Trackers.RemoveAll(o => track.Contains(o.Id));
                            user.Update();
                        }
                        break;
                }
                var response = new StoreResponseData {
                    Data = JSON.Serialize(trackersToResponse),
                    Total = trackersToResponse.Count
                };
                response.Return();
            }
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}