using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

namespace HttpHandler_Proxy {
  public class TwitterProxy : IHttpHandler {

    public void ProcessRequest(HttpContext context) {
      // This will be the case whether there's a cache hit or not.
      context.Response.ContentType = "application/json";

      string id = context.Request.QueryString["id"];

      if (string.IsNullOrWhiteSpace(id))
        id = "Encosia";

      // Check to see if the twitter status is already cached,
      //   then retrieve and return the cached value if so.
      if (context.Cache["tweets-" + id] != null) {
        string cachedTweets = context.Cache["tweets-" + id].ToString();

        context.Response.Write(cachedTweets);

        // We're done here.
        return;
      }

      WebClient twitter = new WebClient();

      string requestUrl = "http://api.twitter.com/1/statuses/user_timeline.json?id=" + id;

      string tweets = twitter.DownloadString(requestUrl);

      // This monstrosity essentially just caches the WebClient result
      //  with a maximum lifetime of 5 minutes from now.
      // If you don't care about the expiration, this can be a simple
      //  context.Cache["tweets"] = tweets; instead.
      context.Cache.Add("tweets-" + id, tweets,
        null, DateTime.Now.AddMinutes(5),
        System.Web.Caching.Cache.NoSlidingExpiration,
        System.Web.Caching.CacheItemPriority.Normal, 
        null);

      context.Response.Write(tweets);
    }

    public bool IsReusable {
      get {
        return true;
      }
    }
  }
}