using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

namespace HttpHandler_Proxy {
  public class TwitterProxy : IHttpHandler {

    public void ProcessRequest(HttpContext context) {
      string screenName = context.Request.QueryString["screenName"];

      if (string.IsNullOrWhiteSpace(screenName))
        screenName = "Encosia";

      string cachedResponse = context.Cache.Get("twitter-status-" + screenName) as string;

      if (!string.IsNullOrWhiteSpace(cachedResponse)) {
        context.Response.ContentType = "application/json";
        context.Response.Write(cachedResponse);

        return;
      }

      WebClient twitter = new WebClient();

      string requestUrl = "http://api.twitter.com/1/statuses/user_timeline.json?id=" + screenName;

      string response = twitter.DownloadString(requestUrl);

      // This monstrosity essentially just caches the result for twitter-status-screenName,
      //  with a maximum lifetime of 5 minutes from the time that we first cached it.
      context.Cache.Add("twitter-status-" + screenName, response,
        null, DateTime.Now.AddMinutes(5), System.Web.Caching.Cache.NoSlidingExpiration,
        System.Web.Caching.CacheItemPriority.Normal, null);

      context.Response.ContentType = twitter.ResponseHeaders["content-type"];
      context.Response.Write(response);
    }

    public bool IsReusable {
      get {
        return true;
      }
    }
  }
}