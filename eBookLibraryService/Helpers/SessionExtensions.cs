using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace eBookLibraryService.Helpers
{
    public static class SessionHelper
    {
        // Get an object from the session
        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        }

        // Set an object to the session
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
    }
}
